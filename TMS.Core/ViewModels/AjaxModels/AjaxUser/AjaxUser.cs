using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.AjaxModels
{
    public class AjaxUser
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string LastOnline { get; set; }
        public string Status { get; set; }
        public string Option { get; set; }
        public string UserState { get; set; }
    }
}
