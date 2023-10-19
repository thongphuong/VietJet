using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Contracts
{
    public class ContractModels
    {
        public int? Id { get; set; }
        [Required]
        public int? ContractorID { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_CONTRACTCODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_CONTRACTNO")]
        public string ContractNO { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_CONTRACT_SIGNDATE")]
        public DateTime? SignDate { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_CONTRACT_EXPRIEDATE")]
        public DateTime? Expiredate { get; set; }
        public string Note { get; set; }
        public Decimal? Price { get; set; }
        public int? Currency { get; set; }
        public int? StatusID { get; set; }
        public int? TypeID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public Dictionary<int, string> StatusList { get; set; }
        public Dictionary<int, string> TypeList { get; set; }
        public Dictionary<int, string> ContractorList { get; set; }
        public Dictionary<int, string> Curencylist { get; set; }
        public SelectList Contractor { get; set; }
        public SelectList Status { get; set; }
        public SelectList Type { get; set; }
    }
}
