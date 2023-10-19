using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Cost
{
    public class CostResultModel
    {
        public Dictionary<int, string> Subjects_BondAgreement { get; set; }
        public Dictionary<int, string> Course_BondAgreement { get; set; }
    }
    public class CostReTrainingModel
    {
        public Dictionary<int, string> Subject_Retraining { get; set; }
        public Dictionary<int, string> Course_Retraining { get; set; }
    }
    public class CostModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_CODE")]
        [Display(Name = @"Code")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_NAME")]
        [Display(Name = @"Name")]
        public string Name { get; set; }
        public  string Description { get; set; }
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //     ErrorMessageResourceName = "VALIDATION_GROUPCODE")]
        public int? GroupCostId { get; set; }
        public Dictionary<int,string> GroupCosts { get; set; } 

    }
}
