using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.ReportModels
{
    public class SubjectResultModel
    {
        public Dictionary<int,string> Courseses { get; set; }
    }

    public class CourseDetailModelRp
    {
        public string CourseName { get; set; }    
        public string CourseCode { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string SubjectDetailName { get; set; }
        public string SubjectCode { get; set; }
        public double SubjectDetailDuration { get; set; }
        public string RoomName { get; set; }
        public string TypeLearning { get; set; }
        public string[] header { get; set; }
        public string Instructors { get; set; }
        public string Hannah { get; set; }
        public string Mentor { get; set; }
        public bool? SubjectActive { get; set; }

        public IEnumerable<TraineeRp> TraineeRps { get; set; }

        public class TraineeRp
        {
            public string FullName { get; set; }
            public string StaffId { get; set; }
            public string DepartmentCode { get; set; }
            public string Point { get; set; }
            public double? First_Check_Score { get; set; }
            public double? Re_Check_Score { get; set; }
            public string First_Check_Result { get; set; }
            public string Re_Check_Result { get; set; }
            public string Grace { get; set; }
            public string ReMark { get; set; }
            public object FirstCheck { get; set; }

            public object Recheck { get; set; }
            public bool? bit_Average_Calculate { get; set; }

        }

    }
}
