using HRDashboard.Data;
using HRDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace HRDashboard.Controllers
{
    [Authorize]
    public class WorkshopsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public WorkshopsController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // Extract subject/body from webhook response content.
        private static (string? subject, string? body) ExtractSubjectBodyFromResponse(string? respContent)
        {
            if (string.IsNullOrWhiteSpace(respContent)) return (null, null);

            // Attempt to parse JSON and search for useful fields
            try
            {
                using var doc = JsonDocument.Parse(respContent);
                string? subject = null;
                string? body = null;

                void SearchElement(JsonElement el)
                {
                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in el.EnumerateObject())
                        {
                            var name = prop.Name?.Trim() ?? string.Empty;
                            if (string.Equals(name, "object", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(name, "subject", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(name, "mailsubject", StringComparison.OrdinalIgnoreCase))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(subject))
                                    subject = prop.Value.GetString();
                            }

                            if (string.Equals(name, "body", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(name, "mailbody", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(name, "content", StringComparison.OrdinalIgnoreCase))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(body))
                                    body = prop.Value.GetString();
                            }

                            // if property is string, it may contain both object: ... body: ...
                            if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                var s = prop.Value.GetString() ?? string.Empty;
                                if ((subject == null || body == null) && (s.IndexOf("object:", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("body:", StringComparison.OrdinalIgnoreCase) >= 0))
                                {
                                    var (sub2, body2) = ParseSubjectBodyFromInline(s);
                                    if (!string.IsNullOrWhiteSpace(sub2) && string.IsNullOrWhiteSpace(subject)) subject = sub2;
                                    if (!string.IsNullOrWhiteSpace(body2) && string.IsNullOrWhiteSpace(body)) body = body2;
                                }
                            }

                            // Recurse
                            if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                SearchElement(prop.Value);
                            }
                        }
                    }
                    else if (el.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in el.EnumerateArray()) SearchElement(item);
                    }
                }

                SearchElement(doc.RootElement);

                // If not found, check for common single-string fields
                if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
                {
                    // look for properties named linqQuery/linqQ
                    void FindByName(JsonElement el)
                    {
                        if (el.ValueKind != JsonValueKind.Object) return;
                        foreach (var prop in el.EnumerateObject())
                        {
                            var name = prop.Name?.Trim() ?? string.Empty;
                            if ((string.Equals(name, "linqQuery", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "linqQ", StringComparison.OrdinalIgnoreCase)) && prop.Value.ValueKind == JsonValueKind.String)
                            {
                                var s = prop.Value.GetString() ?? string.Empty;
                                var (sub2, body2) = ParseSubjectBodyFromInline(s);
                                if (!string.IsNullOrWhiteSpace(sub2) && string.IsNullOrWhiteSpace(subject)) subject = sub2;
                                if (!string.IsNullOrWhiteSpace(body2) && string.IsNullOrWhiteSpace(body)) body = body2;
                            }
                            if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array) FindByName(prop.Value);
                        }
                    }

                    FindByName(doc.RootElement);
                }

                return (subject, body);
            }
            catch
            {
                // Not JSON or parse failed — try to parse inline from raw string
                return ParseSubjectBodyFromInline(respContent);
            }
        }

        private static (string? subject, string? body) ParseSubjectBodyFromInline(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return (null, null);

            var subject = (string?)null;
            var body = (string?)null;

            var idxObj = s.IndexOf("object:", StringComparison.OrdinalIgnoreCase);
            var idxBody = s.IndexOf("body:", StringComparison.OrdinalIgnoreCase);

            if (idxObj >= 0 && idxBody > idxObj)
            {
                var start = idxObj + "object:".Length;
                subject = s.Substring(start, idxBody - start).Trim().Trim(new char[] { '\n', '\r', ' ', '\t', ',', ':' });
                body = s.Substring(idxBody + "body:".Length).Trim();
                return (subject, body);
            }

            // fallback: if contains linqQuery: extract that block
            var idxLinq = s.IndexOf("linqQuery", StringComparison.OrdinalIgnoreCase);
            if (idxLinq >= 0)
            {
                var candidate = s.Substring(idxLinq);
                // try to find 'body:' inside candidate
                var bIdx = candidate.IndexOf("body:", StringComparison.OrdinalIgnoreCase);
                if (bIdx >= 0)
                {
                    // subject might appear before body within candidate
                    var objIdx = candidate.IndexOf("object:", StringComparison.OrdinalIgnoreCase);
                    if (objIdx >= 0 && objIdx < bIdx)
                    {
                        var subjStart = objIdx + "object:".Length;
                        subject = candidate.Substring(subjStart, bIdx - subjStart).Trim().Trim(new char[] { '\n', '\r', ' ', '\t', ',', ':' });
                    }
                    body = candidate.Substring(bIdx + "body:".Length).Trim();
                    return (subject, body);
                }
            }

            // If none of the patterns matched, return whole text as body
            return (null, s.Trim());
        }

        [HttpGet]
        public IActionResult Index()
        {
            var list = _db.Workshops?.OrderByDescending(w => w.ScheduledAt).ToList() ?? new List<Workshop>();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workshop model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.Workshops!.Add(model);
            await _db.SaveChangesAsync();

            // prepare payload for mail generation webhook
            var webhook = _config["External:MailGenerationWebhook"] ?? "https://seifeddineamiri.app.n8n.cloud/webhook/mail generation";
            var bodyText = $"Workshop: {model.Subject}\nTrainer: {model.Trainer}\nDate: {model.ScheduledAt:yyyy-MM-dd HH:mm}\nType: {model.Type}\nDetails: {model.LocationOrLink}";

            string? respContent = null;
            try
            {
                using var http = new System.Net.Http.HttpClient();
                var payload = JsonSerializer.Serialize(new { user_input = bodyText });
                var resp = await http.PostAsync(webhook, new StringContent(payload, Encoding.UTF8, "application/json"));
                respContent = await resp.Content.ReadAsStringAsync();

                // try to extract subject/body from JSON or nested/string responses
                try
                {
                    var (subject, body) = ExtractSubjectBodyFromResponse(respContent);
                    model.MailSubject = subject;
                    model.MailBody = body ?? respContent;
                }
                catch
                {
                    model.MailBody = respContent;
                }
            }
            catch
            {
                // network failure — leave MailBody null
            }

            // update workshop with generated mail info
            try
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // ignore persistence errors for mail fields
            }

            TempData["Success"] = "Workshop scheduled and mail generation requested.";
            return RedirectToAction("Index");
        }
    }
}
