using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Employee
{
   public class EmployeeModelIndex
    {
        public int Control { get; set; }
        public string Departments { get; set; }
        public SelectList Genders { get; set; }
        public SelectList JobTitles { get; set; }
        public SelectList Status { get; set; }
        public SelectList Type { get; set; }
        public SelectList Mentor { get; set; }
        public bool SearchMentor { get; set; }
    }
}
