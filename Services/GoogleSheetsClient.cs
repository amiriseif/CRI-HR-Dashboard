using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

public class GoogleSheetsClient
{
    private readonly IConfiguration _config;

    public GoogleSheetsClient(IConfiguration config)
    {
        _config = config;
    }

    private SheetsService CreateService()
    {
        var jsonPath = _config["GoogleSheets:ServiceAccountJsonPath"];
        var json = System.IO.File.ReadAllText(jsonPath);

        var credential = GoogleCredential
            .FromJson(json)
            .CreateScoped(SheetsService.ScopeConstants.SpreadsheetsReadonly);

        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "HRDashboard"
        });
    }

    public async Task<IList<IList<object>>> ReadRangeAsync(string spreadsheetId, string range)
    {
        var sheets = CreateService();
        var request = sheets.Spreadsheets.Values.Get(spreadsheetId, range);
        ValueRange response = await request.ExecuteAsync();
        return response.Values ?? new List<IList<object>>();
    }
}
