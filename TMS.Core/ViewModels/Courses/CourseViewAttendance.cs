using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class CourseViewAttendance
    {
        public Dictionary<int,string> Courseses { get; set; } 
        public int CourseId { get; set; }
        public int CourseDetailtId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string Instructor { get; set; }  
        public string Mentor { get; set; }

        public string Duration { get; set; }
        public string Date { get; set; }
        public string Room { get; set; }
        public int TypeLearning { get; set; }
    }
}
