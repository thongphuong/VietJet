using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
   public class APIAssignLMS
    {
        public int CourseDetailId { get; set; }
        public string CourseCode { get; set; }
        public string TraineeCode { get; set; }

        //CourseDetail
        public string SubjectCode { get; set; }
        
    }
}
