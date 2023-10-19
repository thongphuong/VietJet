using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.AjaxModels.AjaxCourse
{
   public class AjaxSubDetailTrainee
    {
        public int? Traineeid { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string DeptCode { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Grade { get; set; }
        public string Remark { get; set; }
        public string Point { get; set; }
        public object FirstResultCertificate { get; set; }
        public object ReResultCertificate { get; set; }

    }
}
