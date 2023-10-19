using System;

namespace TMS.Core.ViewModels.ViewModel
{
    public class TrainingActivityReportViewModel
    {
        public int Course_Detail_Id { get; set; }
        public string CourseCode { get; set; }
        public string Company { get; set; }
        public string CourseName { get; set; }
        public string SubjectName { get; set; }
        public string TypeOfTraining { get; set; }
        public string Dept { get; set; }
        public string Method { get; set; }
        public string Customer { get; set; }
        public double? Duration { get; set; }
        public string Venue { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Participants { get; set; }
        public int Distinction { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int? TrainingId { get; set; }
        public bool? bit_Active { get; set; }

    }
}