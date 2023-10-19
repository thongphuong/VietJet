using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel
{
    public class UserChangePassword
    {
       
        [Required]
        [DisplayName(@"Old Password")]
        public string OldPassword { get; set; }
        [Required]
        [RegularExpression(".{6,}", ErrorMessage = "6 characters minimum")]
        [DisplayName(@"New Password")]
        public string NewPassword { get; set; }
        [Required]
        [DisplayName(@"Confirm Password")]
        [RegularExpression(".{6,}", ErrorMessage = "6 characters minimum")]
        [Compare("NewPassword", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }
    }
}
