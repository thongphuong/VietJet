namespace TMS.Core.ViewModels.Jobtitles
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class JobtitleModifyViewModel
    {
        public int? Id { get; set; }

        [Display(Name = @"Active")]
        [Required]
        public int IsActive { get; set; }

        [Display(Name = @"Assigned Subject")]
        //[Required]
        public int[] AssignedSubjects { get; set; }

        //public string Code { get; set; }
        [Display(Name = @"lblJobLevel", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceName = "VALIDATION_JOBTITLE_HEADER", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public int? JobHeaderId { get; set; }

        [Display(Name = @"lblPosition", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceName = "VALIDATION_JOBTITLE_POSITION", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public int? JobPositionId { get; set; }

        [Display(Name = @"lblJobTitleName", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceName = "VALIDATION_JOBTITLE_NAME", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Name { get; set; }

        public string Description { get; set; }
        public bool check_hidden_level_position { get; set; }

        [Display(Name = @"Available Subject")]
        public Dictionary<int,string> Subjects { get; set; }
        public Dictionary<int, string> YesNoDictionary { get; set; }
    }
}
