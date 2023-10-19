using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APICourseMentorHannah
    {
        public int? CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int DateFrom { get; set; }
        public int DateTo { get; set; }

        public string TimeFrom { get; set; }

        public string TimeTo { get; set; }
        public int TimeBlock { get; set; }

        public int Time { get; set; }
        public IEnumerable<SubjectInstructor> SubjectInstructors { get; set; }
        public IEnumerable<Mentee> Mentees { get; set; }

        public class SubjectInstructor
        {
            public string Username { get; set; }
            public string FullName { get; set; }
            public int Type { get; set; }
        }

        public class Mentee
        {
            public string Username { get; set; }
            public string FullName { get; set; }
        }
    }
}
