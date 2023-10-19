using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using DAL.Entities;

namespace TMS.Core.ViewModels.Subjects
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class SubjectModifyViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int? ParentId { get; set; }
        [StringLength(8,MinimumLength = 3)]
        [Required]
        public string Code { get; set; }

        public IEnumerable<SubjectDetailModifyModel> SubjectDetailModel { get; set; }

        public IEnumerable<SubjectInstructor> SubjectInstructor { get; set; }
        public Dictionary<int,string> Subjects { get; set; }
    }
    public class SubjectInstructor
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = @"Instructor")]
        public int InstructorId { get; set; }
        [Required]
        [Display(Name = @"Subject Id")]
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public DateTime? CreateDate { get; set; }
        public double? Allowance { get; set; }
    }
    public class SubjectDetailModifyModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_NAME")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_CODE")]
        public string Code { get; set; }
            
        public int[] InstructorAbility { get; set; }
        //[Display(Name = @"lblTeachers", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Teacher{ get; set; }
        public object instructor { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_SUBJECT_RECURENT")]
        public int Recurrent { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_SUBJECT_DURATION")]
        public double Duration { get; set; }
        [DisplayName(@"Pass Score")]
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //       ErrorMessageResourceName = "VALIDATION_SUBJECT_PASSSCORE")]
        public double? PassScore { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_SUBJECT_AVERAGE_CALCULTE")]
        public int IsAverageCaculate { get; set; }
        public IEnumerable<SubjectScoreViewModel> SubjectScoreModels { get; set; }
        public Dictionary<int, string> AverageStatus { get; set; }
        public Dictionary<int, string> Instructors { get; set; }
        public Dictionary<int, string> GroupCourses { get; set; }
        public int?[] ListGroupCourses { get; set; }
        //[Display(Name = @"lblTeachers", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceName = "VALIDATION_INSTRUCTOR", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public IEnumerable<SubjectDetailInfoViewModel> ListInstructors { get; set; }
        public SubjectDetail subject { get; set; }

        public bool IsUsed { get; set; }
        public string Departments { get; set; }
        public List<int?> DepartmentIds { get; set; } 
        public SelectList JobTitles { get; set; }
        public IEnumerable<Subject_Score> SubjectScores { get; set; }
        public int? levelsubject { get; set; }
        public List<Subject_Types> ListSubjectType { get; set; }
        public string HtmlSubjectType { get; set; }
        public int  CourseTypeID { get; set; }
        public string HtmlGroupSubject { get; set; }
        public List<SubjectDetailChildInfoViewModel> ListSubjectChild { get; set; }
        public List<Department> ListRelevalDeparment { get; set; }
        public List<int?> SubjectTrainingCenter { get; set; }
        public List<SubjectDetail> SubjectDetails { get; set; }
        public List<int> SubjectIdList { get; set; }
        public bool? IsEdit { get; set; }
    }
    public class SubjectDetailInfoViewModel
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = @"Instructor")]
        public int InstructorId { get; set; }
        [Required]
        [Display(Name = @"Subject Id")]
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public DateTime? CreateDate { get; set; }
        public decimal? Allowance { get; set; }
        public string InstructorEID { get; set; }
    }
    public class ProfileSubjectModel
    {
        public bool? bit_Active { get; set; }
        public string SubjectCode { get; set; }
        public DateTime? dtm_from { get; set; }
        public string dtm_from_to { get; set; }
        public string subjectName { get; set; }
        public object point { get; set; }
        public string remark { get; set; }
        public string grade { get; set; }
        public string TypeLearning { get; set; }
        public string recurrent { get; set; }
        public DateTime? ex_Date { get; set; }
        public Course_Detail courseDetails { get; set; }
        public int? memberId { get; set; }
        public string Status { get; set; }
        public string certificate { get; set; }
        public string codecertificate { get; set; }
        public int? checkstatus { get; set; }
        public string Path { get; set; }
        public object firstCheck { get; set; }        public object reCheck { get; set; }
    }
    public class Subject_Types
    {
        public int? ID { get; set; }
        public string Name { get; set; }
    }
    public class SubjectDetailChildInfoViewModel
    {
        public string CoursetypeName { get; set; }
        public int? CoursetypeID { get; set; }
        public int Recurrent { get; set; }
        public double Duration { get; set; }
        public string GroupSubjectName { get; set; }
        public string CertificateName { get; set; }
        public string CertificateCode { get; set; }
    }
}
