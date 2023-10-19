using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.RecCompany
{
    public class RecCompanyModels
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceName = "VALIDATION_CODE", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Code { get; set; }
        [Required(ErrorMessageResourceName = "VALIDATION_NAME", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "VALIDATION_EMAIL_REQUIED", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
       
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
