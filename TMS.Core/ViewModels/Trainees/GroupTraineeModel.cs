using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Trainees
{
   public class GroupTraineeModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_GROUPTRAINEE_CODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_GROUPTRAINEE_NAME")]
        public string Name { get; set; }
       public string Description { get; set; }
       public bool IsActived { get; set; }
        public IEnumerable<int?> TraineeIds { get; set; }
        public Dictionary<int, string> GroupTrainees { get; set; }
    }
}
