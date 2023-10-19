using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Nation
{
    public class NationModels
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_CODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_NAME")]
        public string Name { get; set; }
        public string CreatedUser { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDay { get; set; }
        public bool isActive { get; set; }
    }
}
