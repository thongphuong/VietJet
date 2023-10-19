
using DAL.Entities;

namespace TMS.Core.ViewModels.ViewModel.RoleMenus
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class MenuViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        [Required]
        public string MenuTitle { get; set; }
        public string Icon { get; set; }
        [Required]
        public int? MenuIndex { get; set; }
        public int? ParentId { get; set; }

        public int? UserId { get; set; }
        public bool? IsMenu { get; set; }
        public int? Function { get; set; }
        public GroupFunction Functionlist { get; set; }

        public  string Checkbox { get; set; }

        public Dictionary<int, string> Menu { get; set; }
        public List<int> CurrentFunctions { get; set; }
        public Dictionary<int, string> ListFunctions { get; set; }
        public  string Ancestor { get; set; }
    }
}
