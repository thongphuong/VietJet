using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIFlagLMS
    {

        public string TenHam { get; set; }
        public IEnumerable<APILms> Lms { get; set; }
        public class APILms
        {
            public string CourseDetailId { get; set; }
            public string CourseCode { get; set; }
            public string CourseId { get; set; }
            public string TraineeCode { get; set; }

            //CourseDetail
            public string SubjectCode { get; set; }

            //Group Subject
            public string GroupCode { get; set; }
            public string JobtitleCode { get; set; }
            public string TraineeHistoryId { get; set; }
            public int Status { get; set; }
            public string DepartmentCode { get; set; }
            public int Id { get; set; }
            public int SubjectId { get; set; }


        }

    }
}
