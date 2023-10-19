using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TMS.Core.ViewModels.UserModels
{
    using System.ComponentModel.DataAnnotations;

    public class UserProfile
    {
        public int? Id { get; set; }
        
        [Display(Name = @"lblUserName", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                  ErrorMessageResourceName = "VALIDATION_USERNAME_REQUIED")]
        [RegularExpression(".{6,}", ErrorMessageResourceName = "VALIDATION_USER_USERNAME_EXPRESSION", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string UserName { get; set; }

        [Display(Name = "lblFullName", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //          ErrorMessageResourceName = "VALIDATION_FULLNAME_REQUIED")]
        public string FullName { get; set; }
        [Display(Name = "SYS_FIRSTNAME", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "SYS_LASTNAME", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required]
        public string LastName { get; set; }
        [Display(Name = "USER_ADDRESS", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Address { get; set; }
        [Display(Name = "USER_PHONENO", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Numbers { get; set; }
        [Display(Name = @"lblEmail", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                  ErrorMessageResourceName = "VALIDATION_EMAIL_REQUIED")]
        public string Email { get; set; }
        [RegularExpression(".{6,}", ErrorMessageResourceName = "VALIDATION_USER_USERNAME_EXPRESSION", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Password { get; set; }
        public string PasswordSentEmail { get; set; }
        [Display(Name = "USER_COMFRIMPASS", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Compare("Password", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_CONFIRMPASSWORD_EXPRESSION")]
        public string PasswordConfirm { get; set; }
        [Display(Name = "USER_ROLE", ResourceType = typeof(App_GlobalResources.Resource))]
        public IEnumerable<int> Role { get; set; }
        public int? Department { get; set; }
        [Display(Name = @"Instructor")]
        public int? InstructorId { get; set; }


        [Display(Name = "USER_GROUPUSER", ResourceType = typeof(App_GlobalResources.Resource))]
        public IEnumerable<int> GroupUser { get; set; }

        public int[] Permissions { get; set; }

        public Dictionary<int, string> GroupUsers { get; set; }
        public Dictionary<int, string> Departments { get; set; }
        public Dictionary<int, string> Instructors { get; set; }
        public Dictionary<int, string> Roles { get; set; }
        public bool Checksitepermissiondata { get; set; }
        public string nameImage { get; set; }
        public string ImgAvatar { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }

        public string Namesite { get; set; }
        public bool CheckGroupUser { get; set; }
    }
}
