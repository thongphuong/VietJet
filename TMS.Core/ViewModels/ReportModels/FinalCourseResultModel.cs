using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ReportModels
{
   public class FinalCourseResultModel
    {
        public Dictionary<int,string> Courses { get; set; }
        public Dictionary<int,string> Certificates { get; set; } 
        public Dictionary<int,string> Departments { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public Dictionary<int, string> GroupCertificates { get; set; }
        public Dictionary<int, string> SubjectList { get; set; }
    }

    public class FinalCourseResultModeltModelRp
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Venue { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string[] header { get; set; }
        public IEnumerable<TraineeModelRp> Trainees { get; set; }
        public class TraineeModelRp
        {
            public string FullName { get; set; }
            public string Eid { get; set; }
            public string DepartmentCode { get; set; }
            public string Point { get; set; }
            public string Grace { get; set; }
            public string ReMark { get; set; }  
            public int? Id { get; set; }  
        }
    }

}
