using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Company
{
    public class CompanyModels
    {
        public int? Id { get; set; }
        [Required (ErrorMessageResourceName = "VALIDATION_CODE", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Code { get; set; }
        [Required(ErrorMessageResourceName = "VALIDATION_NAME", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
