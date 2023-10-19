using System;
using System.Collections.Generic;

namespace TMS.Core.ViewModels.Courses
{
    using DAL.Entities;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Utils;

    public class APICourseViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Dictionary<string, string> DepartmentIds { get; set; }
        public int? GroupSubjectId { get; set; }
        public int? CourseType { get; set; }
        [Required]
        public int BeginDate { get; set; }
        [Required]
        public int EndDate { get; set; }
        public bool? Customer { get; set; }
        public string Venue { get; set; }
        public int? PartnerId { get; set; }
        [Display(Name = @"MAX.NBR.TRAINEE")]
        public int? MaxTranineeMembers { get; set; }
        public bool? Survey { get; set; }
        public string Note { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public IEnumerable<CourseDetailModel> Subject { get; set; }
        public class CourseDetailModel
        {
            public int? Id { get; set; }
            [Required]
            public int SubjectId { get; set; }
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public bool? Registable { get; set; }
            public int? LearningType { get; set; }
            [Required]
            public int? DateFrom { get; set; }
            [Required]
            public int? DateTo { get; set; }
            [Required]
            public string TimeFrom { get; set; }
            [Required]
            public string TimeTo { get; set; }
            public int? Room { get; set; }
            public int? AttemptsAllowed { get; set; }
            public bool? IsDeleted { get; set; }
            public bool? IsActive { get; set; }
            public string Duration { get; set; }
            public IEnumerable<SubjectInstructor> SubjectInstructors { get; set; }
            public Dictionary<string, string> ListInstructorBySubject { get; set; }
            public Dictionary<string, string> SubjectScore { get; set; }
            public CourseDetailModel(Course_Detail coursedetail,string KeyLang)
            {
                this.Id = coursedetail.Id;
                this.SubjectId = (int)coursedetail.SubjectDetailId;
                this.SubjectCode = coursedetail.SubjectDetail.Code;
                this.SubjectName = coursedetail.SubjectDetail.Name;
                this.Registable = coursedetail.bit_Regisable;
                this.LearningType = coursedetail.type_leaning;
                this.Duration = coursedetail.Duration?.ToString();
                this.DateFrom = (coursedetail.dtm_time_from != null ? (int)DateUtil.ConvertToUnixTime((DateTime)coursedetail.dtm_time_from) : 0);
                this.DateTo = (coursedetail.dtm_time_to != null ? (int)DateUtil.ConvertToUnixTime((DateTime)coursedetail.dtm_time_to) : 0);
                this.TimeFrom = coursedetail.time_from;
                this.TimeTo = coursedetail.time_to;
                this.Room = coursedetail.RoomId;
                this.AttemptsAllowed = coursedetail.AttemptsAllowed;

                this.IsDeleted = coursedetail.IsDeleted;
                this.IsActive = coursedetail.IsActive;

                this.SubjectInstructors = coursedetail.Course_Detail_Instructor.Select(a => new SubjectInstructor(a));
                //this.ListInstructorBySubject = coursedetail.Course_Detail_Instructor.ToDictionary(b => b.Trainee.str_Staff_Id, b => b.Trainee.LastName + " " + b.Trainee.FirstName);
                this.ListInstructorBySubject = coursedetail.Course_Detail_Instructor.ToDictionary(b => b.Trainee.str_Staff_Id, b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName, KeyLang));
                this.SubjectScore = coursedetail.SubjectDetail.Subject_Score.OrderByDescending(c => c.point_from).ToDictionary(b => b.grade, b => b.point_from.ToString());
            }
            private string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
            {
                string fullName;
                switch (culture)
                {
                    case "vi":
                        fullName = firstName + " " + lastName;
                        break;
                    default:
                        fullName = lastName + " " + firstName;
                        break;
                }
                return fullName;
            }
        }
       
        public class SubjectInstructor
        {
            public int? Id { get; set; }
            [Required]
            public int InstructorId { get; set; }
            public string InstructorCode { get; set; }
            [Required]
            public int SubjectId { get; set; }
            public decimal? Allowance { get; set; }
            public double? Duration { get; set; }
            public SubjectInstructor(Course_Detail_Instructor course_detail_instructor)
            {
                this.Id = course_detail_instructor.Id;
                this.InstructorId = (int)course_detail_instructor.Instructor_Id;
                this.InstructorCode = course_detail_instructor.Trainee.str_Staff_Id;
                this.SubjectId = course_detail_instructor.Course_Detail.SubjectDetail.Id;
                this.Duration = course_detail_instructor.Duration;
                this.Allowance = course_detail_instructor.Allowance;
            }
        }
        public APICourseViewModel(Course course,string KeyLang)
        {
            this.Id = course.Id;
            this.Code = course.Code;
            this.Name = course.Name;
            this.DepartmentIds = course.Course_TrainingCenter.ToDictionary(c => c.Department.Code, c => c.Department.Name);
            this.GroupSubjectId = course.GroupSubjectId;
            this.CourseType = course.CourseTypeId;
            this.BeginDate = (course.StartDate != null ? (int)DateUtil.ConvertToUnixTime((DateTime)course.StartDate) : 0);
            this.EndDate = (course.EndDate != null ? (int)DateUtil.ConvertToUnixTime((DateTime)course.EndDate) : 0);
            this.Customer = course.CustomerType;
            this.Venue = course.Venue;
            this.PartnerId = course.CompanyId;
            this.MaxTranineeMembers = course.NumberOfTrainee;
            this.Survey = course.Survey;
            this.Note = course.Note;
            this.IsDeleted = course.IsDeleted;
            this.IsActive = course.IsActive;
            this.Subject = course.Course_Detail.Select(a => new CourseDetailModel(a, KeyLang));

        }
    }
}
