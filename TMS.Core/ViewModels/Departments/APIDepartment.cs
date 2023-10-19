using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.Services.TraineeHis;

namespace TMS.Core.ViewModels.Departments
{
    public class APIDepartment
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string ParentCode { get; set; }
        public string Ancestor { get; set; }

        public APIDepartment(Department department)
        {
            this.Code = department.Code;
            this.Name = department.Name;
            this.IsDeleted = (bool)department.IsDeleted;
            this.IsActive = (bool)department.IsActive;
            this.ParentCode = department.Code;
           this.Ancestor = department.Ancestor;

        }
    }
   
}
