using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.ReportModels
{
   public class AttendanceSheetModel
    {
        public Dictionary<int, string> Courses { get; set; }
    }

    public class AttendanceSheetModelRp
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string SubjectDetailName { get; set; }
        public float SubjectDetailDuration { get; set; }
        public string Venue { get; set; }
          public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public IEnumerable<TraineeModelRp> Trainees { get; set; }
        public string[] header { get; set; }
        public class TraineeModelRp
        {
            public string FullName { get; set; }
            public string Eid { get; set; }
            public string DepartmentCode { get; set; }

        }
    }
   
}
