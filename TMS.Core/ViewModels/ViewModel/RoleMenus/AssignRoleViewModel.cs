using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel.RoleMenus
{
    public class AssignRoleViewModel
    {
        public string Name { get; set; }
        public bool IsMenu { get; set; }

        public bool AllRole { get; set; }
        public bool RoleView { get; set; }
        public bool RoleModify{ get; set; }
        public bool RoleDelete { get; set; }
        public bool RoleReport { get; set; }
    }
}
