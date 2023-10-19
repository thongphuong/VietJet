using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace TMS.Core.ViewModels.Courses
{
   public class SetCertificateModel
    {
        public int? Id { get; set; }
        public Dictionary<int,string> Programs { get; set; } 
    }

    public class LoadImg
    {
        public string Content { get; set; }
    }
    public class GroupCertificateModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public Dictionary<int, string> Certificates { get; set; }
        public Dictionary<int,string> Subjects { get; set; }

        public int? CertificateId { get; set; }
        public int? SubjectId { get; set; }
        public int?[] ArrSubjectId { get; set; }

        public int?[] IdSubjects { get; set; }
    }

    public class GroupCertificateForCourse
    {
        public int? CertificateGroupId { get; set; }
        public int? CertificateForAllGroupId { get; set; }
        public string ATOGroup { get; set; }
    }
}
