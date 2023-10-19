using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
  public  class RemarkComment
    {
         public int Id { get; set; }
        public int CourseDetailId { get; set; }
        public int TraineeId { get; set; }
        public string EId { get; set; }
        public string FullName { get; set; }
        public bool Type { get; set; }
        public string Remark { get; set; }
        public string Result { get; set; }
        public double? Score { get; set; }
    }
}
