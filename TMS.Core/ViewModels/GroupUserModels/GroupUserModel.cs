using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TMS.Core.App_GlobalResources;

namespace TMS.Core.ViewModels.GroupUserModels
{

    public class GroupUserModel
    {
        public int? Id { get; set; }

        [Display(Name = "GROUPUSER_NAME", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "VALIDATION_GROUPUSER_NAME", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Name { get; set; }

        [Display(Name = "GROUPUSER_TITLE", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "VALIDATION_GROUPUSER_TITLE", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Title { get; set; }

        public string CheckSitePermissionData { get; set; } = "PERMISSION_DATA";
    }
}
