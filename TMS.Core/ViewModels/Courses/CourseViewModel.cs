using System;
using System.Collections.Generic;

namespace TMS.Core.ViewModels.Courses
{
    using System.ComponentModel.DataAnnotations;

    public class CourseViewModel
    {
        //Checksitepermissiondata
      
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_COURSE_CODE")]
        [Display(Name = @"lblCourseCode", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Code { get; set; }

        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //          ErrorMessageResourceName = "VALIDATION_COURSE_NAME")]
        [Display(Name = @"lblCourseName", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                  ErrorMessageResourceName = "VALIDATION_COURSE_DEPARTMENT")]
        [Display(Name = @"lblDepartment", ResourceType = typeof(App_GlobalResources.Resource))]
        public int[] DepartmentIds { get; set; }
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //       ErrorMessageResourceName = "VALIDATION_COURSE_GROUPCOURSE")]
        [Display(Name = @"lblGroupCourse", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? GroupSubjectId { get; set; }

        [Display(Name = @"lblCourseType", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? CourseType { get; set; }

        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //         ErrorMessageResourceName = "VALIDATION_COURSE_FROMDATE")]
        //[Display(Name = @"lblFrom", ResourceType = typeof(App_GlobalResources.Resource))]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? BeginDate { get; set; }

       
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        [Display(Name = @"lblCustomer", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Customer { get; set; }

        [Display(Name = @"lblVenue", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Venue { get; set; }

        [Display(Name = @"lblCompany", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? PartnerId { get; set; }

        [Display(Name = @"lblMaxTranineeMembers", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? MaxTranineeMembers { get; set; }

        [Display(Name = @"lblMinTranineeMembers", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? MinTranineeMembers { get; set; }

        [Display(Name = @"lblSurvey", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Survey { get; set; }

        [Display(Name = @"lblNote", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Note { get; set; }
        public string process { get; set; }

        public bool InProcess { get; set; }
        public IEnumerable<CourseDetailModel> CourseDetailModels { get; set; }
        //public Dictionary<int, string> DictionaryDepartments { get; set; }
        public string Departments { get; set; }
        public List<int?> DepartmentId { get; set; }
        public int? IsPublic { get; set; }
        public int? IsBindSubject { get; set; }
        public IEnumerable<int?> BindToSubject { get; set; }
        //[Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? ProgramCost { get; set; }
        [Range( 1, 100,ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
            ErrorMessageResourceName = "VALIDATION_COURSE_MAXGRADE")]
        public int? MaxGrade { get; set; }
        public CourseDetailModel CourseDetailitems { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_COURSE_FROMDATE")]
        [Display(Name = @"lblFrom", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_startdate { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                  ErrorMessageResourceName = "VALIDATION_COURSE_TODATE")]
        [Display(Name = @"lblTo", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_enddate { get; set;  }
     
    }

    public class CourseDetailModel
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = @"Subject")]
        public int SubjectId { get; set; }
        public int Registable { get; set; }
        public int LearningType { get; set; }

        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //           ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_FROMDATE")]
        //[Display(Name = @"lblFromDate", ResourceType = typeof(App_GlobalResources.Resource))]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateFrom { get; set; }

       
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateTo { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                   ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_FROMTIME")]
        [Display(Name = @"lblFromTime", ResourceType = typeof(App_GlobalResources.Resource))]

        public string TimeFrom { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                    ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_TOTIME")]
        [Display(Name = @"lblToTime", ResourceType = typeof(App_GlobalResources.Resource))]

        public string TimeTo { get; set; }
        public int? Room { get; set; }

        public int? Commitment { get; set; }
        public int? MarkType { get; set; }
        public double? CommitmentExpiredate { get; set; }
        public int? Attempts { get; set; }
        public int? Grademethod { get; set; }
        public int? Mentor { get; set; }
        public int? Hannah { get; set; }
        public decimal? Allowance { get; set; }

        [Display(Name = @"VALIDATION_COURSE_INSTRUCTOR" , ResourceType = typeof(App_GlobalResources.Messege))]
        public IEnumerable<SubjectInstructor> SubjectInstructors { get; set; }
        
        public Dictionary<int,string> ListInstructorBySubject { get; set; }
        public Dictionary<int, string> ListInstructorByMentorHannah { get; set; }
        public IEnumerable<TeachingAssistant> TeachingAssistants { get; set; }

        public IEnumerable<Blended> Blended { get; set; } 
        public IEnumerable<Ingredients> CourseIngredient { get; set; } 

       
        //[Display(Name = @"lblRegistryDate", ResourceType = typeof(App_GlobalResources.Resource))]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //public DateTime? RegistryDate { get; set; }

   
        //[Display(Name = @"lblExpiryDate", ResourceType = typeof(App_GlobalResources.Resource))]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //public DateTime? ExpiryDate { get; set; }
        public int? Time { get; set; }
        public int? TimeBlock { get; set; }
        public string str_remark { get; set; }
        public decimal? MonitorAllowance { get; set; }
        public decimal? ExaminerAllowance { get; set; }
        public double? MonitorDuration { get; set; }
        public double? ExaminerDuration { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectType { get; set; }
        public int? IdGlobal { get; set; }
        public int? RoomIdGlobal { get; set; }
        public string Remark { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                   ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_FROMDATE")]
        [Display(Name = @"lblFromDate", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_time_from { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                  ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_TODATE")]
        [Display(Name = @"lblToDate", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_time_to { get; set; }
    }

    public class Blended
    {
        public int? Id { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? RoomId { get; set; }
        public string Room { get; set; }
       
        public string LearningTypeName { get; set; }
        public int? ExaminerId { get; set; }
        public string Examiner { get; set; }
        public int? MarkTypecRo { get; set; }
        public double? BlendedDuration { get; set; }
        public decimal? BlendedAllowance { get; set; }
        public string dtm_DateFrom_blend { get; set; }
        public string dtm_DateTo_blend { get; set; }
    }
    public class Ingredients
    {
        public int? Id { get; set; }
        public int? Percent { get; set; }
        public int? IngredientId { get; set; }
        public int IsOnline { get; set; }
    }
    public class SubjectInstructor
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                   ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_INSTRUCTOR")]
        [Display(Name = @"lblInstructor", ResourceType = typeof(App_GlobalResources.Resource))]
        public int InstructorId { get; set; }
        [Required]
        [Display(Name = @"Subject Id")]
        public int SubjectDetailId { get; set; }
        public string Name { get; set; }
        public double? Duration { get; set; }
        public decimal? InstructorAllowance { get; set; }
    }

    public class TeachingAssistant
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                   ErrorMessageResourceName = "VALIDATION_COURSEDETAIL_INSTRUCTOR")]
        [Display(Name = @"lblInstructor", ResourceType = typeof(App_GlobalResources.Resource))]
        public int InstructorId { get; set; }
        [Required]
        [Display(Name = @"Subject Id")]
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public double? Duration { get; set; }
    }
}
