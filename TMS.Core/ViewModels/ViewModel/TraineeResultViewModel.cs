using System;
namespace TMS.Core.ViewModels.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using DAL.Entities;
    using System.Web;

    public class TraineeResultViewModel
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
    public class Trainee_Validation
    {

        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_USER_EID")]
        [Display(Name = @"EID", ResourceType = typeof(App_GlobalResources.Messege))]
        public string eid { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_USER_FULLNAME")]
        [Display(Name = @"TRAINEE_FULLNAME", ResourceType = typeof(App_GlobalResources.Resource))]
        public string FullName { get; set; }
        //[Required(ErrorMessage = "nhap fullname")]
        //[Required]
        //[DisplayName(@"First Name")]
        public string FirstName { get; set; }
        //[Required]
        //[DisplayName(@"Last Name")]
        public string LastName { get; set; }
        //[Required(ErrorMessage = "nhap passport")]

        [Display(Name = @"TRAINEE_PASSPORT", ResourceType = typeof(App_GlobalResources.Resource))]
        public string passport { get; set; }

        [Display(Name = @"TRAINEE_ID", ResourceType = typeof(App_GlobalResources.Resource))]
        public string str_id { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = @"lblDateOfBirth", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? dtm_Birthdate { get; set; }

       
        [Display(Name = @"TRAINEE_GENDER", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? gender { get; set; }

        [Display(Name = @"TRAINEE_PLACEOFBIRTH", ResourceType = typeof(App_GlobalResources.Resource))]
        public string str_Place_Of_Birth { get; set; }
        public int Role { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_MAIL")]
        [Display(Name = @"TRAINEE_EMAIL", ResourceType = typeof(App_GlobalResources.Resource))]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_EMAIL_EXPRESSION")]
        public string mail { get; set; }
        public string PathEducation { get; set; }

        [Display(Name = @"TRAINEE_NATION", ResourceType = typeof(App_GlobalResources.Resource))]
        public string nation { get; set; }
        //[StringLength(11, MinimumLength = 7, ErrorMessage = "Phone is not valid")]
        //[Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        //[Required]
        [Display(Name = @"TRAINEE_PHONE", ResourceType = typeof(App_GlobalResources.Resource))]
        public string phone { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = @"TRAINEE_JOINDATE", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? dtm_Join_Date { get; set; }

        //[Required(ErrorMessage = "Please enter type.")]
        [Display(Name = @"TRAINEE_TYPE", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_TYPE")]
        public int? type { get; set; }

       
        [Display(Name = @"lblJobTitle_Occupation", ResourceType = typeof(App_GlobalResources.Resource))]
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_JOBTITLE")]
        public int? Job_Title_id { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
           ErrorMessageResourceName = "VALIDATION_USER_DEPARTMENT")]
        [Display(Name = @"TRAINEE_DEPARTMENT", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Department_Id { get; set; }

        [Display(Name = @"TRAINEE_COMPANY", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Company_Id { get; set; }
        public string nameImage { get; set; }
        public string ImgAvatar { get; set; }
        [Display(Name = @"lblNonWorkingDay", ResourceType = typeof(App_GlobalResources.Resource))]
        public DateTime? Resignation_Date { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public ICollection<TraineeEducation> Educations { get; set; }
        public Dictionary<int, string> JobtitleDictionary { get; set; }

        public string password { get; set; }
    }

    public class TraineeEducation
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
}