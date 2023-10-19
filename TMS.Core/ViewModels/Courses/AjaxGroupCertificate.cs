using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class AjaxGroupCertificate
    {
        public string Eid { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string GroupSubject { get; set; }
        public string Code { get; set; }
        public string Option { get; set; }
    }

    public class GetSubject
    {
        public string Content { get; set; }
        public bool IsCompleted { get; set; }
    }
}
