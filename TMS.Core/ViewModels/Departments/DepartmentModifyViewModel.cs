using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Departments
{
    using System.ComponentModel.DataAnnotations;

    public class DepartmentModifyViewModel : DepartmentViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public int CreateBy { get; set; }
        public Dictionary<int,string> Departments { get; set; }
        public string Notify { get; set; }
        public string CurrentUserId { get; set; }
        public Dictionary<int, string> DictionaryInstructor { get; set; }
        public int? HeadName { get; set; }
        public bool? is_training { get; set; }


    }
   
}
