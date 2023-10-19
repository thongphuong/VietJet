using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Jobtitles
{
    using TMS.Core.ViewModels.Common;

    public class JobtitleRouteEmployeeViewModel
    {
        public IEnumerable<CustomDataListModel> Subjects { get; set; }
        public IEnumerable<JobTitleCoincideTrainee> TrainedEmployee { get; set; }
    }

    public class JobTitleCoincideTrainee
    {
        public int? Id { get; set; }
        public string FullName { get; set; }
        public int AmountSubjects { get; set; }
    }
    public class JobTitleAvalibleSubject
    {
        public int SubjectId { get; set; }
        public bool Status { get; set; }
    }
}
