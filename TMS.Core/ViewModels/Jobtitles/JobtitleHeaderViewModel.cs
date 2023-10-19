using System.ComponentModel.DataAnnotations;
namespace TMS.Core.ViewModels.Jobtitles
{
    using System.Collections.Generic;
    using TMS.Core.ViewModels.Common;

    public class JobtitleHeaderViewModel
    {
        public int? Id { get; set; }
        [Display(Name = @"lblJobLevel", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? JobLevel{ get; set; }
        [Display(Name = @"lblName", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_NAME")]
        public string Name { get; set; }
        [Display(Name = @"lblDescription", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Description { get; set; }

        public IEnumerable<CustomDataListModel> JobLvlDictionary { get; set; } 
    }
}
