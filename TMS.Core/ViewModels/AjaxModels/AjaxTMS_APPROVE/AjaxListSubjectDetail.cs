using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.AjaxModels.AjaxTMS_APPROVE
{
    public class AjaxListSubjectDetail
    {
        public string Id { get; set; }
        public string Course { get; set; }
        public string FromTo { get; set; }
        public string RequestBy { get; set; }
        public string Remark { get; set; }
        public string Action { get; set; }       
        public string firstName { get; set; }       
        public string lastName { get; set; }       
    }
}
