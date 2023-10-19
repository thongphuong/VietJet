using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Core.ViewModels.ViewModel
{
    public class Course_Validation
    {
        public int? type { get; set; }
        public int? partner { get; set; }
        [Required]
        public string coursecode { get; set; }
        [Required]
        public string coursename { get; set; }
       // [Required]
        public DateTime? dtm_startdate { get; set; }
       // [Required]
        public DateTime? dtm_enddate { get; set; }
        public string venue { get; set; }
        [Required]
        public int? typecourse { get; set; }
        public int? notrainee { get; set; }
        public int? survey { get; set; }
        public string note { get; set; }
        public int? groupsubject { get; set; }
    }
}