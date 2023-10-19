using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class TraineePortalViewModel
    {
        public int Id { get; set; }
        public int? CourseID { get; set; }
        public int? TraineeID { get; set; }
        public string TraineeUserName { get; set; }
        public string TraineeFullName { get; set; }
        public string CourseCode { get; set; }
        public string SubjectCode { get; set; }
        public int? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string SubjectName { get; set; }
    }
}
