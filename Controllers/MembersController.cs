using HRDashboard.Data;
using HRDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class MembersController : Controller
{
    private readonly GoogleSheetsClient _sheets;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public MembersController(GoogleSheetsClient sheets, IConfiguration config, ApplicationDbContext db)
    {
        _sheets = sheets;
        _config = config;
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var members = await _db.SheetMembers!.OrderBy(s => s.ExternalId).ToListAsync();

        var vm = new HomeSheetViewModel
        {
            Title = "Members",
        };

        if (members.Count > 0)
        {
            foreach (var m in members)
            {
                vm.Rows.Add(new List<object>
                {
                    m.ExternalId,
                    m.Email ?? string.Empty,
                    m.Name ?? string.Empty,
                    m.PhoneNumber ?? string.Empty,
                    m.Adresse ?? string.Empty,
                    m.TypeMembership ?? string.Empty,
                    m.MembershipStatus ?? string.Empty,
                    m.Department ?? string.Empty,
                    m.CotisationPaymentStatus ?? string.Empty,
                });
            }

            return View(vm);
        }

        var spreadsheetId = _config["GoogleSheets:SpreadsheetId"];
        var range = _config["GoogleSheets:Range"];
        if (!string.IsNullOrWhiteSpace(spreadsheetId) && !string.IsNullOrWhiteSpace(range))
        {
            var rows = await _sheets.ReadRangeAsync(spreadsheetId, range);
            vm.Rows = rows;
        }

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> QueryAgent([FromBody] QueryRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserInput))
            return BadRequest(new { error = "Missing user_input" });

        // Call the n8n webhook server-side to avoid exposing the URL in the browser
        var webhook = _config["External:QueryGenerationWebhook"]; // prefer configuration
        if (string.IsNullOrWhiteSpace(webhook))
        {
            // fallback to hard-coded URL if config not present
            webhook = "https://seifeddineamiri.app.n8n.cloud/webhook/querrygeneration";
        }

        string agentResponseText;
        using (var http = new HttpClient())
        {
            var payload = JsonSerializer.Serialize(new { user_input = request.UserInput });
            var resp = await http.PostAsync(webhook, new StringContent(payload, Encoding.UTF8, "application/json"));
            agentResponseText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new { error = "Webhook error", detail = agentResponseText });
            }
        }

        // Try to extract filters from the agent response. We avoid executing arbitrary code.
        var query = _db.SheetMembers!.AsQueryable();
        bool applied = false;

        var text = agentResponseText;
        // If agent returned JSON containing a `linqQuery` property, extract it.
        try
        {
            using var doc = JsonDocument.Parse(agentResponseText);
            if (TryFindProperty(doc.RootElement, "linqQuery", out var linqEl) && linqEl.ValueKind == JsonValueKind.String)
            {
                text = linqEl.GetString() ?? text;
            }
        }
        catch
        {
            // ignore parse errors and continue with raw text
        }

        // If the agent returned a direct LINQ expression like
        // dbContext.Members.Where(m => m.CotisationPaymentStatus == "Invalid").Select(...)
        // try to parse the Where clause safely and apply the filter.
        var directLinqRegex = new Regex("Where\\s*\\(\\s*m\\s*=>\\s*m\\.(\\w+)\\s*==\\s*\\\"([^\\\"]+)\\\"\\s*\\)", RegexOptions.IgnoreCase);
        var directMatch = directLinqRegex.Match(text);
        if (directMatch.Success)
        {
            var field = directMatch.Groups[1].Value;
            var val = directMatch.Groups[2].Value;
            // Only apply if the field is known
            var known = new[] { "CotisationPaymentStatus", "MembershipStatus", "Department", "TypeMembership", "Email", "Name" };
            if (known.Contains(field))
            {
                query = ApplyEqualsFilter(query, field, val);
                applied = true;
            }
        }
        if (string.IsNullOrWhiteSpace(text))
            return Ok(new { members = new List<SheetMember>(), raw = agentResponseText });

        // Common intent: unpaid / not paid
        if (Regex.IsMatch(text, "didn'?t pay|not paid|did not pay|haven't paid|not yet paid", RegexOptions.IgnoreCase))
        {
            query = query.Where(s => s.CotisationPaymentStatus == null || !EF.Functions.Like(s.CotisationPaymentStatus, "%Paid%"));
            applied = true;
        }

        // Known fields to look for
        var fields = new[] { "CotisationPaymentStatus", "MembershipStatus", "Department", "TypeMembership", "Email", "Name" };
        foreach (var field in fields)
        {
            // pattern: Field == "Value" or Field: Value or Field contains "Value"
            var eqRegex = new Regex($"{Regex.Escape(field)}\\s*(==|=|:)\\s*[\'\"]([^\'\"]+)[\'\"]", RegexOptions.IgnoreCase);
            var containsRegex = new Regex($"{Regex.Escape(field)}\\s*contains\\s*[\'\"]([^\'\"]+)[\'\"]", RegexOptions.IgnoreCase);

            var m = eqRegex.Match(text);
            if (m.Success)
            {
                var op = m.Groups[1].Value.Trim();
                var val = m.Groups[2].Value.Trim();
                if (string.Equals(op, "!="))
                {
                    query = ApplyNotEqualsFilter(query, field, val);
                }
                else
                {
                    query = ApplyEqualsFilter(query, field, val);
                }
                applied = true;
                continue;
            }

            var mc = containsRegex.Match(text);
            if (mc.Success)
            {
                var val = mc.Groups[1].Value.Trim();
                query = ApplyContainsFilter(query, field, val);
                applied = true;
            }
        }

        // Execute the query
        var results = await query.Take(1000).ToListAsync();

        return Ok(new { members = results, raw = agentResponseText, appliedFilters = applied });
    }

    // Recursively search JSON for a property name and return its element.
    private static bool TryFindProperty(JsonElement el, string name, out JsonElement found)
    {
        found = default;
        if (el.ValueKind == JsonValueKind.Object)
        {
            if (el.TryGetProperty(name, out var v))
            {
                found = v;
                return true;
            }
            foreach (var prop in el.EnumerateObject())
            {
                if (TryFindProperty(prop.Value, name, out found))
                    return true;
            }
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
            {
                if (TryFindProperty(item, name, out found))
                    return true;
            }
        }
        return false;
    }

    // Helpers to build simple filters
    private IQueryable<SheetMember> ApplyEqualsFilter(IQueryable<SheetMember> q, string field, string value)
    {
        value = value.Trim();
        switch (field)
        {
            case "CotisationPaymentStatus": return q.Where(s => s.CotisationPaymentStatus == value);
            case "MembershipStatus": return q.Where(s => s.MembershipStatus == value);
            case "Department": return q.Where(s => s.Department == value);
            case "TypeMembership": return q.Where(s => s.TypeMembership == value);
            case "Email": return q.Where(s => s.Email == value);
            case "Name": return q.Where(s => s.Name == value);
            default: return q;
        }
    }

    private IQueryable<SheetMember> ApplyNotEqualsFilter(IQueryable<SheetMember> q, string field, string value)
    {
        switch (field)
        {
            case "CotisationPaymentStatus": return q.Where(s => s.CotisationPaymentStatus != value);
            case "MembershipStatus": return q.Where(s => s.MembershipStatus != value);
            case "Department": return q.Where(s => s.Department != value);
            case "TypeMembership": return q.Where(s => s.TypeMembership != value);
            case "Email": return q.Where(s => s.Email != value);
            case "Name": return q.Where(s => s.Name != value);
            default: return q;
        }
    }

    private IQueryable<SheetMember> ApplyContainsFilter(IQueryable<SheetMember> q, string field, string value)
    {
        switch (field)
        {
            case "CotisationPaymentStatus": return q.Where(s => s.CotisationPaymentStatus != null && s.CotisationPaymentStatus.Contains(value));
            case "MembershipStatus": return q.Where(s => s.MembershipStatus != null && s.MembershipStatus.Contains(value));
            case "Department": return q.Where(s => s.Department != null && s.Department.Contains(value));
            case "TypeMembership": return q.Where(s => s.TypeMembership != null && s.TypeMembership.Contains(value));
            case "Email": return q.Where(s => s.Email != null && s.Email.Contains(value));
            case "Name": return q.Where(s => s.Name != null && s.Name.Contains(value));
            default: return q;
        }
    }

    public class QueryRequest { public string? UserInput { get; set; } }
}
