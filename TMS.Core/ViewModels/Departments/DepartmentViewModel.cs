using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Departments
{
    public class DepartmentViewModel
    {
        public bool IsMaster { get; set; }
        public IEnumerable<DepartmentModel> DepartmentViewModels { get; set; }
        public Dictionary<int, string> DepartList { get; set; }
        public Notification Notification { get; set; }
        public Dictionary<int, string> EmployeeList { get; set; }
        public List<int?> ListSelect { get; set; }
    }
    public class DepartmentModel
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string Ancestor { get; set; }
        public string Code { get; set; }

    }

    public class UserPermissionViewModel : DepartmentViewModel
    {
        public ICollection<int?> GroupUserPermissions { get; set; }
        public ICollection<int> PermissionIds { get; set; }
        public int? UserId { get; set; }
    }

    public class GroupUserPermissionViewModel : DepartmentViewModel
    {
        public ICollection<int> GroupUserPermissionIds { get; set; }
        public ICollection<int> PermissionIds { get; set; }
        public int? GroupUserId { get; set; }
        public int? UserId { get; set; }
    }
    
}
