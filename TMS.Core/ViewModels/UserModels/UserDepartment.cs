using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.UserModels
{
   public class UserDepartment
    {
        public int id { get;  set; }
        public IEnumerable<int?> DepartmentId { get; set; }
    }
}
