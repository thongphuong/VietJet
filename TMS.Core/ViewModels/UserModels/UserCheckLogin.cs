using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.UserModels
{
    public class UserCheckLogin
    {
        public string Username { get; set; }
        public string Ip { get; set; }
        public int Attempts { get; set; }

        public int Time { get; set; }
    }
}
