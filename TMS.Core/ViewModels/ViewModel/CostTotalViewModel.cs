using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel
{
    public class CostTotalViewModel
    {
        public int? course_detail_ID { get; set; }
        public string CourseName { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string SubjectName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Total { get; set; }
        public bool? Commitment { get; set; }
        public string Grade { get; set; }
        public string ReTrainingCost { get; set; }
        public DateTime? Non_Working_Day { get; set; }
        public bool? Non_Working { get; set; }
        public string Compensation_Cost { get; set; }
    }
}
