using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel.RoleMenus
{
    public class FunctionViewModel
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public int Type { get; set; }
        public int Method { get; set; }
    }
}
