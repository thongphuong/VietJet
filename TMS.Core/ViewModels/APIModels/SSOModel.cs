using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class SSOModel
    {
        public string token { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string eIdentifier { get; set; } // username
        public string action { get; set; } // username
    }
}
