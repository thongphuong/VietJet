using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIUserMentorHannah
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public int? Gender { get; set; }
        public string Email { get; set; }
        public int? Role { get; set; }
        public string Avatar { get; set; }
        public IEnumerable<Target> Targets { get; set; }
        public IEnumerable<CourseOfMentee> CourseOfMentees { get; set; }
        public IEnumerable<CourseOfMentor> CourseOfMentors { get; set; }
        public IEnumerable<CourseOfHannah> CourseOfHannahs { get; set; }
        public class Target
        {
            public int? Type { get; set; }
        }
        public class CourseOfMentee
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
        }
        public class CourseOfMentor
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
        }
        public class CourseOfHannah
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
        }
    }
}
