namespace TMS.Core.ViewModels.Courses
{
    using System.Collections.Generic;

    public class CourseModifyModel : CourseViewModel
    {
        public Dictionary<int, string> DictionaryGroupSubjects { get; set; }
        //public Dictionary<int, string> DictionaryDepartments { get; set; }
        public Dictionary<int, string> DictionaryCompanies { get; set; }
        public Dictionary<int, string> DictionaryCourseAreas { get; set; }
        public Dictionary<int, string> DictionaryCourseTypes { get; set; }
        public Dictionary<int, string> DictionarySurvey { get; set; }
        public Dictionary<int, string> DictionaryLearningTypes { get; set; }
        public Dictionary<int, string> DictionaryRooms { get; set; }
        public Dictionary<int, string> DictionaryMentor { get; set; }
        public Dictionary<int, string> DictionaryHannah { get; set; }
        public Dictionary<int, string> DictionaryAttemptsAllowed { get; set; }
        public Dictionary<int, string> DictionaryGradingMethod { get; set; }
        public IEnumerable<SubjectDetailInfoViewModel> ListSubjects { get; set; }
        public int SubmitType { get; set; }
        public bool IsDraft { get; set; }
        public bool ProcessStep { get; set; }
        public bool ProcessStepRequirement { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFinalApproved { get; set; }
        public bool HiddenButton { get; set; }
        public int CourseId { get; set; }
        public Dictionary<int, string> Courseses { get; set; }

        public bool CheckCommitment { get; set; }
        public bool CheckCostPerPerson { get; set; }
        public bool CheckExportFinal { get; set; }


        public Dictionary<int, string> DictionaryLearningTypeBlended { get; set; }
        public Dictionary<int, string> DictionaryIngredients { get; set; }
        public string HtmlRoom { get; set; }
        public string strCourseDetailRoom { get; set; }
        public bool IsEditsubject { get; set; }
    }

    public class SubjectDetailInfoViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Duration { get; set; }
        public int? int_Course_Type { get; set; }
        public bool Average_Calculator { get; set; }
        public string string_Course_Type { get; set; }
        public bool IsActive { get; set; }
        public int? Recurrent { get; set; }
        public double? PassScore { get; set; }
    }
    public class CourseRoomViewModel
    {
        public int CourseId { get; set; }
        public string ListDetailRoom { get; set; }
        public int? CourseDetailId { get; set; }
        public string Remark { get; set; }
        public int? RoomIDGlobal { get; set; }
        public int? IdGlobal { get; set; }
    }
    public class ListDetailRoomViewModel
    {
        public string DateTime { get; set; }
        public int? RoomID { get; set; }
        public string RoomOther { get; set; }
    }
}
