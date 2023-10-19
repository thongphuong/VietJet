namespace TMS.Core.ViewModels.Jobtitles
{
    public class JobtitleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? ParentId { get; set; }
    }
}
