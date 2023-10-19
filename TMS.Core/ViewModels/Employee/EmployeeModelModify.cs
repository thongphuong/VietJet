using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TMS.Core.ViewModels.ViewModel;

namespace TMS.Core.ViewModels.Employee
{
   public class EmployeeModelModify
    {
        public bool CheckHannahMentor { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = @"EMPLOYEE_JOINED_PARTY_DATE", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? JoinedPartyDate { get; set; }
        public int Control { get; set; }
        public int? Id { get; set; }
        
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_USER_FULLNAME")]
        [Display(Name = @"TRAINEE_FULLNAME", ResourceType = typeof(App_GlobalResources.Resource))]
        public string FullName { get; set; }

        public int Role { get; set; }
        public bool? IsExaminer { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                 ErrorMessageResourceName = "VALIDATION_USER_EID")]
        [Display(Name = @"EIDGV", ResourceType = typeof(App_GlobalResources.Messege))]
        public string Eid { get; set; }

        [Display(Name = @"TRAINEE_ID", ResourceType = typeof(App_GlobalResources.Resource))]
        public string PersonalId { get; set; }


        [Display(Name = @"TRAINEE_PASSPORT", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Passport { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //[Display(Name = @"TRAINEE_BIRTHDAY", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? Birthdate { get; set; }

        [Display(Name = @"TRAINEE_GENDER", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Gender { get; set; }

        [Display(Name = @"TRAINEE_PLACEOFBIRTH", ResourceType = typeof(App_GlobalResources.Resource))]
        public string PlaceOfBirth { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_MAIL")]
        [Display(Name = @"TRAINEE_EMAIL", ResourceType = typeof(App_GlobalResources.Resource))]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_EMAIL_EXPRESSION")]

        public string Email { get; set; }

        [Display(Name = @"TRAINEE_NATION", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Nation { get; set; }
        [Display(Name = @"TRAINEE_PHONE", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Phone { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //[Display(Name = @"TRAINEE_JOINDATE", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? JoinedDate { get; set; }

        [Display(Name = @"TRAINEE_TYPE", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_TYPE")]
        public int? EmployeeType { get; set; }

        [Display(Name = @"lblJobTitle_Occupation", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //      ErrorMessageResourceName = "VALIDATION_JOBTITLE")]
        public int? JobTitleId { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
           ErrorMessageResourceName = "VALIDATION_USER_DEPARTMENT")]
        [Display(Name = @"TRAINEE_DEPARTMENT", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? DepartmentId { get; set; }

        [Display(Name = @"TRAINEE_COMPANY", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? CompanyId { get; set; }

        public HttpPostedFileBase ImgFile { get; set; }
        public string NameImage { get; set; } // use to bind data
        public string ImgAvatar { get; set; } // use to update data

        public string PathEducation { get; set; }

        [Display(Name = @"USER_PASSWORD", ResourceType = typeof(App_GlobalResources.Resource))]
        public string password { get; set; }
        
        [Display(Name = "USER_COMFRIMPASS", ResourceType = typeof(App_GlobalResources.Resource))]
        //[System.ComponentModel.DataAnnotations.Compare("password", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_CONFIRMPASSWORD_EXPRESSION")]
        public string PasswordConfirm { get; set; }

        [Display(Name = @"Allowance")]
        public string TrainingAllowance { get; set; }

       
       
        [Display(Name = @"lblSubject", ResourceType = typeof(App_GlobalResources.Resource))]
        public Dictionary<int, string> Subjects { get; set; }

        public Dictionary<int, string> InstructorTypes { get; set; }
        public IEnumerable<int?> InstructorType { get; set; }
        public IEnumerable<EmployeeEducation> Educations { get; set; }
        public IEnumerable<EmployeeContract> Contracts { get; set; }
        public IEnumerable<EmployeeSubject> InstructorSubjects { get; set; }
        //public IEnumerable<InstructorSubject> ListAllowance { get; set; }
        public IEnumerable<int> Abilities { get; set; }
        [Display(Name = @"lblResignationDate", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? ResignationDate { get; set; }

        public SelectList Nations { get; set; }
        public SelectList PlacesOfBirths { get; set; }
        public  SelectList CourseTypes { get; set; }
        public SelectList Company { get; set; }
        public string Departments { get; set; }
        public SelectList Jobtitles { get; set; }
        public SelectList Genders { get; set; }
        public List<int?> RelevantDepartmentId { get; set; }
        public string RelevantDepartmentList { get; set; }
        [Display(Name = @"TRAINEE_BIRTHDAY", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_Birthdate { get; set; }
        [Display(Name = @"TRAINEE_JOINDATE", ResourceType = typeof(App_GlobalResources.Resource))]
        public string dtm_Join_Date { get; set; }
        public class EmployeeEducation
        {
            public int? Id { get; set; }

            //chi su dung ngoai view
            public int? IsDeleted { get; set; }

            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
            public DateTime? TimeFrom { get; set; }
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
            public DateTime? TimeTo { get; set; }

            public string CourseName { get; set; }
            public string Organization { get; set; }
            public string Note { get; set; }
            public ICollection<FileUpload> FileUploads { get; set; }
            public HttpPostedFileBase[] FileImage { get; set; }
            public string[] ListNameImage { get; set; }
            public string dtm_time_to { get; set; }
            public string dtm_time_from { get; set; }
            public class FileUpload
            {
                public int Id { get; set; }
                public string ModelNameImg { get; set; }

                public int[] ListId { get; set; }
                public int IsDeleted { get; set; }
                public int[] ListIsDeleted { get; set; }
                public string[] ListNameImage { get; set; }
                public HttpPostedFileBase[] FIleImage { get; set; }
            }
        }

        public class EmployeeSubject
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

        public class EmployeeContract
        {
            public int Id { get; set; }
            [Display(Name = @"Contract No")]

            public string ContractNo { get; set; }
            public string Description { get; set; }
            [Display(Name = @"Expired Date")]
            public DateTime? ExpireDate { get; set; }
        }
    }
}
