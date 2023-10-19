using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.APIModels
{

    public class APICourseResultSummary
    {
        public int CourseDetailId { get; set; }
        public string CourseCode { get; set; }
        public string Grade { get; set; }
        public string Remark { get; set; }
        public string TraineeCode { get; set; }
        public string SubjectCode { get; set; }
        public double Point { get; set; }
        public  bool IsCompleted { get; set; }
        public int ExpireDate { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public int CreateDate { get; set; }
    }
}
