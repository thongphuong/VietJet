using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.Core.ViewModels.ViewModel
{
    public class TrainingPayment
    {
        public int? Id { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int? InstructorId { get; set; }
        public string Departments { get; set; }
        public int[] CourseDetailIds { get; set; }
        public Dictionary<int,string> RequestOrApproval { get; set; }
        public List<int?> DepartmentIds { get; set; }
        public int? Type { get; set; }

    }

    public class NewFromToPayment
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get;set; }
    }
}
