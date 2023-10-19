using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
   public class TraineeAttendanceModel
    {
        public int Id { get; set; }
       public int TraineeId { get; set; }
        public int CourseDetailId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Percent { get; set; }
        public int Type { get; set; }
        public string Note { get; set; }
    }
}
