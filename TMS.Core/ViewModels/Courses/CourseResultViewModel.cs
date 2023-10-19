using DAL.Entities;
using System.Collections.Generic;
namespace TMS.Core.ViewModels.Courses
{
    public class CourseResultViewModel
    {
        public Dictionary<int,string> Courses { get; set; }
        public Dictionary<int,string> CourseDetailsByCourse { get; set; }
        public int CourseId { get; set; }
        public int CourseDetailId { get; set; }
        public int SubjectDetailId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string RoomName { get;  set; }
        public string Duration { get; set; }
        public string DateFromTo { get; set; }
        public string TypeLearning { get; set; }
        public string Instructors { get; set; }
        public string Hannah { get; set; }
        public string Mentor { get; set; }

        public bool IsApproved { get; set; }

        public bool ProcessStep { get; set; }
        public bool IsCalculate { get; set; }
        public Dictionary<int,string> Ingredients { get; set; }
        public int  TypeLearningId { get; set; }

        public bool ProcessStepRequirement { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<Course_Detail_Course_Ingredients> Ingredientses { get; set; }
        public string MaxGrade { get; set; }
        public string DurationInstructor { get; set; }
        public int? MarkType { get; set; }
        public double? PassScore { get; set; }
        public bool checkSuccessCron { get; set; }
        public class Member 
        {
            public int? Id { get; set; }
            public bool? Type { get; set; }
            public int? TraineeId { get; set; }
            public int? Course_Result_Id { get; set; }
            public string Code {get; set; }
            public string Name {get; set; }
            public string DepartmentCode { get; set; }
            public string DepartmentName {get; set; }
            public string LearningTime {get; set; }
            public string Result {get; set; }
            public string Remark {get; set; }
            public string CheckFail {get; set; }
            public string CheckBox { get; set; }

            public IEnumerable<ResultIngredient> ResultIngredientses { get; set; }
            public string Score { get; set; }
            public string Score_Re { get; set; }
            public string Result_Re { get; set; }
            public string Score_temp { get; set; }
            public string Result_temp { get; set; }
            public string Score_Re_temp { get; set; }
            public string Result_Re_temp { get; set; }
            public string RealReResult { get; set; }

            public class ResultIngredient 
            {

                public string IngredientCode { get; set; }
                public int CourseDetailIngredient_Id {get; set; }
                public int CourseResult_Id {get; set; }
                public string Score {get; set; }
            }
        }
    }
}
