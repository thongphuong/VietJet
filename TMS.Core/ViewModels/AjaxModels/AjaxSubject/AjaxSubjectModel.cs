using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.AjaxModels.AjaxSubject
{
   public class AjaxSubjectModel
    {
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }

        public double Duration { get; set; }
        public double Recurrent { get; set; }
        public string IsCaculate { get; set; }
        public string PassScore { get; set; }

        public string GroupCourse { get; set; }
        public string Status { get; set; }
        public string Option { get; set; }
    }
}
