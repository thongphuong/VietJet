using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DAL.Entities;
using Microsoft.SqlServer.Server;

namespace TMS.Core.ViewModels.ReportModels
{
    public class TrainingActivityModel
    {
        public Dictionary<int, string> Courses { get; set; }
        public Dictionary<int, string> Rooms { get; set; }
        public string Departments { get; set; }
        public SelectList Status { get; set; }

    }

    public class TrainingModelRp
    {
        public IEnumerable<CourseRp> CourseRps { get; set; }

    }
    public class CourseRp
    {
        public string CourseCode { get; set; }
        public string Company { get; set; }
        public IEnumerable<Course_Detail> Subjects { get; set; } 
        public string CourseName { get; set; }
        public string GroupCourse { get; set; }
        public string SubjectName { get; set; }
        public string TypeOfTraining { get; set; }
        public string Customer { get; set; }
        public string Venue { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int Participants { get; set; }
        public int Distinction { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Certificate { get; set; }

    }

    public class TrainingSubjectRp
    {
            
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public string Method { get; set; }
            public float Duration { get; set; }
            public int Participants { get; set; }
            public int Distinction { get; set; }
            public int Pass { get; set; }
            public bool? bit_Active { get; set; }


    }
}
