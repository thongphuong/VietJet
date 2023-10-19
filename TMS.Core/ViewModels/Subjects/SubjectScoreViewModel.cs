using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Subjects
{
    using System.ComponentModel.DataAnnotations;

    public class SubjectScoreViewModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_SUBJECT_GRADE")]
        public string Grade { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_SUBJECT_POINT")]
        [Range(0,100)]
        public double? Point { get; set; }
    }
}
