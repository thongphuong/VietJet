using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
   public class AjaxCourseResultFinalModel
    {
        public string TraineeCode { get; set; }

        public string FullName { get; set; }
        public string DepartmentName { get; set; }

        public string DateFromTo { get; set; }

        public string Point { get; set; }
        public string Grade { get; set; }

        // get value in hidden
       public int SubjectDetailId { get; set; }
        public int CourseDetailId { get; set; }
       public int TraineeId { get; set; }
        public int Id { get; set; }
        public bool Type { get; set; }
        public string FirstResult { get; set; }
        public string ReResult { get; set; }
        public string codecertificate { get; set; }
        public string checkcodecertificate { get; set; }
        public string CertificateID { get; set; }
        public string Remark { get; set; }
        public int? checkstatus { get; set; }
        public object FirstResultCertificate { get; set; }
        public object ReResultCertificate { get; set; }
        public string Path { get; set; }
        public string CourseCode { get; set; }
        public string createDate { get; set; }
        public string SubjectCode { get; set; }
    }
}
