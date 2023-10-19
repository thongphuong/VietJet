namespace TMS.Core.ViewModels.ViewModel
{
    using System.ComponentModel.DataAnnotations;

    public class JobTitleImportViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = @"Department Code")]
        public string DepartmentCode { get; set; }
        [Display(Name = @"Subject Codes")]
        public string[] SubjectCodes { get; set; }
    }
}