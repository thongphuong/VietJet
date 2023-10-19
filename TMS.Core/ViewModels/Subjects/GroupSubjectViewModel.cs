using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Subjects
{
    using System.ComponentModel.DataAnnotations;

    public class GroupSubjectViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_MCOURSE_NAME")]
        //[Display(Name = @"MCOURSE_NAME", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_MCOURSE_CODE")]
        [Display(Name = @"lblCode", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Code { get; set; }

       
        [Display(Name = @"lblSubjectType", ResourceType = typeof(App_GlobalResources.Resource))]
        public string SubjectType { get; set; } // áp dụng lựa chọn này cho tất cả subject trong khóa học (course_detail)

        [Display(Name = @"MCOURSE_DESCRIPTION", ResourceType = typeof(App_GlobalResources.Resource))]
        public string Description { get; set; }
        public int IsActive { get; set; }

        [Display(Name = @"lblAssignedSubject", ResourceType = typeof(App_GlobalResources.Resource))]
        //[Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
        //        ErrorMessageResourceName = "MCOURSE_ASSIGNEDSUBJECT")]
        public IEnumerable<int> AssignedSubjects { get; set; }
        public Dictionary<int,string> AvailableSubjects { get; set; }
        public Dictionary<int,string> SubjectTypes { get; set; }
        public string CertificateName { get; set; }
        public string CertificateCode { get; set; }
        public string HtmlSubjectType { get; set; }
        public List<Subject_Types> ListSubjectType { get; set; }
    }
}
