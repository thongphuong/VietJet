using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIChangePasswordFromLMS
    {
        public string TraineeCode { get; set; }

        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
