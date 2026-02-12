namespace HRDashboard.Models
{
    public class HomeSheetViewModel
    {
        public string? Title { get; set; }
        public IList<IList<object>> Rows { get; set; } = new List<IList<object>>();
    }
}
