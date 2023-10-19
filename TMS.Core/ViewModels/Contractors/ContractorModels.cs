using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TMS.Core.ViewModels.Contractors
{
    public class ContractorModels
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_MCOURSE_CODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_SORT_NAME_REQUIED")]
        public string SortName { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string SerialNumberTax { get; set; }
    }
}
