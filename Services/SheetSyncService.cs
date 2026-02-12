using HRDashboard.Data;
using HRDashboard.Models;
using Microsoft.EntityFrameworkCore;

public class SheetSyncService
{
    private readonly ApplicationDbContext _db;
    private readonly GoogleSheetsClient _sheets;
    private readonly IConfiguration _config;
    private readonly ILogger<SheetSyncService> _logger;

    public SheetSyncService(ApplicationDbContext db, GoogleSheetsClient sheets, IConfiguration config, ILogger<SheetSyncService> logger)
    {
        _db = db;
        _sheets = sheets;
        _config = config;
        _logger = logger;
    }

    public async Task SyncAsync()
    {
        try
        {
            var spreadsheetId = _config["GoogleSheets:SpreadsheetId"];
            var range = _config["GoogleSheets:Range"] ?? "Sheet1!A:Z";
            if (string.IsNullOrWhiteSpace(spreadsheetId))
            {
                _logger.LogInformation("GoogleSheets:SpreadsheetId not configured; skipping sheet sync.");
                return;
            }

            var values = await _sheets.ReadRangeAsync(spreadsheetId, range);
            if (values == null || values.Count <= 1)
            {
                _logger.LogInformation("No rows found in configured spreadsheet range.");
                return;
            }

            // First row = headers. Iterate remaining rows.
            var rows = values.Skip(1);
            var added = 0;
            var updated = 0;

            foreach (var row in rows)
            {
                // Columns expected (by position):
                // 0: id, 1: email, 2: name, 3: phone number, 4: adresse, 5: type membership,
                // 6: Membership status, 7: department, 8: cotisation payment status

                var externalId = row.ElementAtOrDefault(0)?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(externalId))
                    continue;

                var email = row.ElementAtOrDefault(1)?.ToString();
                var name = row.ElementAtOrDefault(2)?.ToString();
                var phone = row.ElementAtOrDefault(3)?.ToString();
                var adresse = row.ElementAtOrDefault(4)?.ToString();
                var typeMem = row.ElementAtOrDefault(5)?.ToString();
                var memStatus = row.ElementAtOrDefault(6)?.ToString();
                var dept = row.ElementAtOrDefault(7)?.ToString();
                var cot = row.ElementAtOrDefault(8)?.ToString();

                var existing = await _db.SheetMembers!.FirstOrDefaultAsync(s => s.ExternalId == externalId);
                if (existing == null)
                {
                    var model = new SheetMember
                    {
                        ExternalId = externalId,
                        Email = email,
                        Name = name,
                        PhoneNumber = phone,
                        Adresse = adresse,
                        TypeMembership = typeMem,
                        MembershipStatus = memStatus,
                        Department = dept,
                        CotisationPaymentStatus = cot,
                        LastSyncedAt = DateTime.UtcNow
                    };
                    _db.SheetMembers!.Add(model);
                    added++;
                }
                else
                {
                    var changed = false;
                    if (existing.Email != email) { existing.Email = email; changed = true; }
                    if (existing.Name != name) { existing.Name = name; changed = true; }
                    if (existing.PhoneNumber != phone) { existing.PhoneNumber = phone; changed = true; }
                    if (existing.Adresse != adresse) { existing.Adresse = adresse; changed = true; }
                    if (existing.TypeMembership != typeMem) { existing.TypeMembership = typeMem; changed = true; }
                    if (existing.MembershipStatus != memStatus) { existing.MembershipStatus = memStatus; changed = true; }
                    if (existing.Department != dept) { existing.Department = dept; changed = true; }
                    if (existing.CotisationPaymentStatus != cot) { existing.CotisationPaymentStatus = cot; changed = true; }
                    if (changed)
                    {
                        existing.LastSyncedAt = DateTime.UtcNow;
                        _db.SheetMembers!.Update(existing);
                        updated++;
                    }
                }
            }

            if (added > 0 || updated > 0)
            {
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Sheet sync completed. Added: {Added}, Updated: {Updated}", added, updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while syncing Google Sheet to database");
        }
    }
}
