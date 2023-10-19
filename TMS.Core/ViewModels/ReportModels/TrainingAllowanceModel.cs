using System;
using System.Collections.Generic;
using DAL.Entities;

namespace TMS.Core.ViewModels.ReportModels
{
    public class TrainingAllowanceModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public IEnumerable<Department> Department { get; set; }

        public class TrainingAllowanceModelRp
        {
            public string CourseName { get; set; }
            public string CourseCode { get; set; }
            public string SubjectDetailName { get; set; }
            public float SubjectDetailDuration { get; set; }
            public string Venue { get; set; }
            public string TimeFrom { get; set; }
            public string TimeTo { get; set; }
            public IEnumerable<TraineeModelRp> Trainees { get; set; }

            public class TraineeModelRp
            {
                public string FullName { get; set; }
                public string Eid { get; set; }
                public string DepartmentCode { get; set; }

            }
        }

    }
}
