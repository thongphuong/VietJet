using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Contact
{
  public  class ContactDetails
    {
        public int? Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public bool isDeleted { get; set; }
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //     ErrorMessageResourceName = "VALIDATION_MAIL")]
        //[Display(Name = @"TRAINEE_EMAIL", ResourceType = typeof(App_GlobalResources.Resource))]
        //[RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessageResourceType = typeof(App_GlobalResources.Messege), ErrorMessageResourceName = "VALIDATION_USER_EMAIL_EXPRESSION")]
        //public string FEmail { get; set; }
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //        ErrorMessageResourceName = "VALIDATION_CONTENT_MAIL")]
        //public string FSubject { get; set; }
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //        ErrorMessageResourceName = "VALIDATION_CONTENT_MAIL")]
        public string FSubject { get; set; }
        public string FContent { get; set; }
        public ReplyContact MyReply { get; set; }

      public class ReplyContact
      {
          public int Id { get; set; }
          public string ReplyEmail { get; set; }
          public string ReplyContent { get; set; }
          public string ReplySubject { get; set; }
      }

    }
}
