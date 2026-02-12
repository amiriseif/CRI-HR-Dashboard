namespace HRDashboard.Models
{
    public class MembersViewModel
    {
        public string? Title { get; set; }
        public IList<IList<object>> Rows { get; set; } = new List<IList<object>>();
    }
}
