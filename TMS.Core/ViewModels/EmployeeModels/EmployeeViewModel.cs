using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.EmployeeModels
{
    public class EmployeeViewModel
    {
        public Trainee Employees { get; set; }
        public IEnumerable<Trainee_Contract> Training_Contracts { get; set; }
        public IEnumerable<SubjectDetail> Training_Competency { get; set; }
        public IEnumerable<Trainee_Record> Training_Education { get; set; }
        public IEnumerable<Course> Training_Course { get; set; }
    }
}
