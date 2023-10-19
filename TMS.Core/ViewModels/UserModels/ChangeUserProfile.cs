using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TMS.Core.ViewModels.UserModels
{
    public class ChangeUserProfile
    {
        public int? Id { get; set; }
        [Display(Name = @"lblUserName", ResourceType = typeof(App_GlobalResources.Resource))]
        public string UserName { get; set; }
        [Required]
        [Display(Name = @"lblFullName" ,ResourceType = typeof(App_GlobalResources.Resource))]
        public string FullName { get; set;}
        //[Required]
        //[DisplayName(@"Fisrt Name")]
        //public string FirstName { get; set; }
        //[Required]
        //[DisplayName(@"Last Name")]
        //public string LastName { get; set; }
        [Display(Name = @"USER_ADDRESS", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Address { get; set; }
        [Display(Name = @"lblPhone", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Numbers { get; set; }
        [Required]
        [Display(Name = @"USER_EMAIL", ResourceType = typeof(App_GlobalResources.Resource))]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessageResourceName = @"VALIDATION_MAIL", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public  string Email { get; set; }
        [Display(Name = @"lblRole", ResourceType  = typeof(App_GlobalResources.Resource))]
        public string Role { get; set; }
        public int RoleId { get; set; }
        [Display(Name = @"lblInstructor", ResourceType = typeof(App_GlobalResources.Resource))]
        public  string Instructor { get; set; }
        [Display(Name = @"lblGroupUser", ResourceType = typeof(App_GlobalResources.Resource))]
        public string GroupUser { get; set; }
        public IEnumerable<int> GroupUsers { get; set; }
        public int? Department { get; set; }
        public Dictionary<int, string> Departments { get; set; }

        public string nameImage { get; set; }
        public string ImgAvatar { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public bool Checksitepermissiondata { get; set; }

        public string Namesite { get; set; }
    }
}
