using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.UserModels
{
    public class UserADModel
    {
        public int UserBranchId { get; set; }
        public int UserSupervisorId { get; set; }
        public string UserSupervisorName { get; set; }
        public string UserADType { get; set; }
    }
}
