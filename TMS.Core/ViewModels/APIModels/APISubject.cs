using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APISubject
    {
        public int CourseId { get; set; }
        public int CourseDetailId { get; set; }
        public bool IsAverageCalculate { get; set; }
        public string CourseCode { get; set; }
        public string GroupCourseCode { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public bool Registable { get; set; }
        public int LearningType { get; set; }
        public int DateFrom { get; set; }
        public int DateTo { get; set; }

        public string TimeFrom { get; set; }

        public string TimeTo { get; set; }

        public string RoomCode { get; set; }
        public string RoomName { get; set; }

        public int AttemptsAllowed { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }
        public double Duration { get; set; }
        public decimal? Allowance { get; set; }
        public int IsPublic { get; set; }
        public int RegistryDate { get; set; }
        public int ExpiryDate { get; set; }
        public decimal Cost { get; set; }
        public int Maxgrade { get; set; }
        public IEnumerable<SubjectInstructor> SubjectInstructors { get; set; }

        public IEnumerable<SubjectScore> SubjectScores { get; set; }
        public IEnumerable<APIDepartment> Departments { get; set; }
        public IEnumerable<APIBlended> Period { get; set; }

        public IEnumerable<APIIngredient> Ingredient { get; set; }
        public class APIIngredient
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public int Weigth { get; set; }
           
            public APIIngredient(Course_Detail_Course_Ingredients a)
            {
                Name = a.Course_Ingredients_Learning?.Name;
                Code = a.Course_Ingredients_Learning?.Code;
                Weigth = a.Percent ?? 0;
            }
        }
        public class APIBlended
        {
            public int Id { get; set; }
            public int DateFrom { get; set; }
            public int DateTo { get; set; }
            public string Type { get; set; }
            public string Room { get; set; }

            //huybt bổ sung
            public int ExaminerId { get; set; }
            public int mark_type_cro { get; set; }

            public APIBlended(Course_Blended_Learning a)
            {
                this.Id = a.Id;
                this.DateFrom = a.DateFrom.HasValue ?(int)DateUtil.ConvertToUnixTime(a.DateFrom.Value) : 0;
                this.DateTo = a.DateTo.HasValue ? (int)DateUtil.ConvertToUnixTime(a.DateTo.Value.AddHours(-23).AddMinutes(-59).AddSeconds(-59)) : 0;
                this.Type = a.LearningType;
                this.Room = a.Room?.str_Name ?? "" ;

                //huybt bổ sung
                this.ExaminerId = a.ExaminerId.HasValue ? a.ExaminerId.Value : 0;
                this.mark_type_cro = a.mark_type_cro.HasValue ? a.mark_type_cro.Value : 0;
            }
        }
        public class APIDepartment
        {
            public string DepartmentCode { get; set; }
            public string DepartmentName { get; set; }
            public string Description { get; set; }

            public APIDepartment(Department department)
            {
                this.DepartmentCode = department.Code ?? "";
                this.DepartmentName = department.Name ?? "";
                this.Description = department.Description ?? "";

            }
        }
        public class SubjectInstructor
        {
            public string InstructorCode { get; set; }
            public string SubjectCode { get; set; }
            public decimal? Allowance { get; set; }
            public double? Duration { get; set; }
            public string FullName { get; set; }
            public int Type { get; set; }

            public SubjectInstructor(Course_Detail_Instructor courseDetailInstructor, string KeyLang)
            {
                this.InstructorCode = courseDetailInstructor.Trainee.str_Staff_Id;
                //this.FullName = courseDetailInstructor.Trainee.FirstName.Trim() + " " + courseDetailInstructor.Trainee.LastName.Trim();
                this.FullName = ReturnDisplayLanguage(courseDetailInstructor.Trainee.FirstName,
                    courseDetailInstructor.Trainee.LastName, KeyLang);
                this.SubjectCode = courseDetailInstructor.Course_Detail.SubjectDetail.Code;
                this.Allowance = courseDetailInstructor.Allowance;
                this.Duration = courseDetailInstructor.Duration;
                this.Type = courseDetailInstructor.Type ?? (int)UtilConstants.TypeInstructor.Instructor;
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



        public class SubjectScore
        {
            public string Grade { get; set; }
            public double Point { get; set; }

            public SubjectScore(Subject_Score subjectScore)
            {
                this.Grade = subjectScore.grade;
                this.Point = subjectScore.point_from ?? 0;
            }
        }

        public APISubject(Course_Detail courseDetail, string Keylang)
        {
            this.CourseId = courseDetail.Course?.Id ?? 0;
            this.CourseDetailId = courseDetail.Id;
            this.IsAverageCalculate = (bool)courseDetail.SubjectDetail.IsAverageCalculate;
            this.CourseCode = courseDetail.Course?.Code ?? "";
            this.GroupCourseCode =
                courseDetail.SubjectDetail?.CAT_GROUPSUBJECT_ITEM?.FirstOrDefault()?.CAT_GROUPSUBJECT?.Code ?? "";
            this.SubjectCode = courseDetail.SubjectDetail?.Code ?? "";
            this.SubjectName = courseDetail.SubjectDetail?.Name ?? "";
            this.Registable = courseDetail.bit_Regisable ?? false;
            this.LearningType = courseDetail.type_leaning ?? 0; //UtilConstants.LearningTypes.Online == 0 , UtilConstants.LearningTypes.Offline == 1
            this.Duration = courseDetail.Duration ?? 0;
            this.DateFrom = (int)DateUtil.ConvertToUnixTime((DateTime)courseDetail.dtm_time_from);
            this.DateTo = (int)DateUtil.ConvertToUnixTime(courseDetail.dtm_time_to.Value.AddHours(23).AddMinutes(59));
            this.TimeFrom = courseDetail.time_from ?? "";
            this.TimeTo = courseDetail.time_to ?? "";
            this.RoomCode = courseDetail.Room?.str_code ?? "";
            this.RoomName = courseDetail.Room?.str_Name ?? "";
            this.AttemptsAllowed = courseDetail.AttemptsAllowed ?? 0;
            this.Duration = courseDetail.Duration ?? 0;
            this.Allowance = courseDetail.Allowance ?? 0;

            this.IsDeleted = (bool)courseDetail.IsDeleted;
            this.IsActive = (bool)courseDetail.IsActive;
            this.IsPublic = courseDetail.Course?.IsPublic == true  ? 1 : 0;
            this.RegistryDate = courseDetail.RegistryDate != null ? (int)DateUtil.ConvertToUnixTime(courseDetail.RegistryDate.Value) : 0;
            this.ExpiryDate = courseDetail.ExpiryDate != null ? (int)DateUtil.ConvertToUnixTime(courseDetail.ExpiryDate.Value) : 0;
            this.Cost = courseDetail.Course?.TrainingProgam_Cost?.LastOrDefault()?.Cost ?? 0; ;
            this.Maxgrade = courseDetail.Course?.MaxGrade ?? 100;
            this.SubjectInstructors = courseDetail.Course_Detail_Instructor.Where(a=>a.Course_Detail_Id == courseDetail.Id).Select(a => new SubjectInstructor(a, Keylang));
            this.SubjectScores = courseDetail.SubjectDetail.Subject_Score.OrderByDescending(a => a.point_from).Select(a => new SubjectScore(a));
            this.Departments = courseDetail.Course.Course_TrainingCenter.Select(a => new APIDepartment(a.Department));
            this.Period = courseDetail.Course_Blended_Learning.Where(a => a.IsDeleted == false && a.IsActive == true).OrderBy(b => b.DateFrom).Select(a => new APIBlended(a));
            this.Ingredient = courseDetail.Course_Detail_Course_Ingredients?.Where(a => a.IsActive == true && a.IsDeleted == false).Select(b => new APIIngredient(b));
        }


    }
}
