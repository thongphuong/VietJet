using System;
using System.ComponentModel;

namespace TMS.Core.ViewModels.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public class InstructorResultViewModel
    {
        public int Trainee_Id { get; set; }
        public int Course_Result_Id { get; set; }
        public int Subject_Id { get; set; }
        public string First_Check_Result { get; set; }
        public string Re_Check_Result { get; set; }
        public string Remark { get; set; }
        public string ResultType { get; set; }
        public DateTime? Learning_From { get; set; }
        public DateTime? Learning_To { get; set; }
        public Boolean Editable { get; set; }
    }
    public class InstructorValidation
    {

        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_USER_EID")]
        [Display(Name = @"EIDGV", ResourceType = typeof(App_GlobalResources.Messege))]
        public string Eid { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_USER_FULLNAME")]
        [Display(Name = @"TRAINEE_FULLNAME", ResourceType = typeof(App_GlobalResources.Resource))]
        public string FullName { get; set; }
       // [Display(Name = @"First Name")]
       // [Required]
        public string FirstName { get; set; }
       // [Display(Name = @"Last Name")]
       // [Required]
        public string LastName{ get; set; }
        [Display(Name = @"TRAINEE_PASSPORT", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Passport { get; set; }
        [Display(Name = @"TRAINEE_ID", ResourceType = typeof(App_GlobalResources.Resource))]
        public string PersonalId { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = @"TRAINEE_BIRTHDAY", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? Birthdate { get; set; }
        
        [Display(Name = @"TRAINEE_GENDER", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Gender { get; set; }

        [Display(Name = @"TRAINEE_PLACEOFBIRTH", ResourceType = typeof(App_GlobalResources.Resource))]
        public string PlaceOfBirth { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_MAIL")]
        [Display(Name = @"TRAINEE_EMAIL", ResourceType = typeof(App_GlobalResources.Resource))]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_EMAIL_EXPRESSION")]
      
        public string Mail { get; set; }
        public string PathEducation { get; set; }

        [Display(Name = @"TRAINEE_NATION", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Nation { get; set; }
        [Display(Name = @"TRAINEE_PHONE", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Phone { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = @"TRAINEE_JOINDATE", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? JoinedDate { get; set; }

        [Display(Name = @"TRAINEE_TYPE", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_TYPE")]      
        public int? EmployeeType { get; set; }

        [Display(Name = @"lblJobTitle_Occupation", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_JOBTITLE")]
        public int? JobTitleId { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
           ErrorMessageResourceName = "VALIDATION_USER_DEPARTMENT")]
        [Display(Name = @"TRAINEE_DEPARTMENT", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Department_Id { get; set; }

        [Display(Name = @"TRAINEE_COMPANY", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? CompanyId { get; set; }

        public HttpPostedFileBase ImgFile { get; set; }
        public string NameImage { get; set; } // use to bind data
        public string ImgAvatar { get; set; } // use to update data

        [Display(Name = @"Allowance")]
        public string TrainingAllowance { get; set; }
        public Dictionary<int,string> Departments { get; set; }
        public Dictionary<int,string> Jobtitles { get; set; }
        [Display(Name = @"lblSubject", ResourceType = typeof(App_GlobalResources.Resource))]
        public Dictionary<int,string> Subjects { get; set; }
        public IEnumerable<TraineeEducation> Educations { get; set; }
        public IEnumerable<InstructorContract> Contracts { get; set; }
        public IEnumerable<InstructorSubject> InstructorSubject { get; set; }
        public IEnumerable<InstructorSubject> ListAllowance { get; set; }
        public IEnumerable<int> Abilities { get; set; }
    }

    public class InstructorContract
    {
        public int Id{ get; set; }
        [Display(Name = @"Contract No")]
        
        public string ContractNo { get; set; }
        public string Description { get; set; }
        [Display(Name = @"Expired Date")]
        public DateTime? ExpireDate { get; set; }
    }
    public class InstructorSubject
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = @"Instructor")]
        public int InstructorId { get; set; }
        [Required]
        [Display(Name = @"Subject Id")]
        public int SubjectId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime? CreateDate { get; set; }
        public decimal? Allowance { get; set; }
    }
}