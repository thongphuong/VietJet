using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel.RoleMenus
{
    public class GroupFunctionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DefaultUrl { get; set; }
        public bool IsActive { get; set; }
        public List<int> CurrentFunctions { get; set; }
        public Dictionary<int, string> ListFunctions { get; set; }

        public int FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string FunctionUrlName { get; set; }
        public int FunctionActionType { get; set; }

        public int? ShowOrder { get; set; }
        public string Icon { get; set; }
        public int? ParentId { get; set; }
        public Dictionary<int, string> Menu { get; set; }
    }
}
