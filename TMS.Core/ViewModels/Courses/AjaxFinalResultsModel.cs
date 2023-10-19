using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
   public  class AjaxFinalResultsModel
    {
        public int Id { get; set; }
        public string TraineeCode { get; set; }
        public int? TraineeId { get; set; }
        public  string FullName { get; set; }
        public string JobTitleName { get; set; }
        public string DepartmentName { get; set; }
        public string Point { get; set; }
        public string Grade { get; set; }
        public string Action { get; set; }
        public string remark { get; set; }
    }
}
