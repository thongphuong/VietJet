using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;

namespace TMS.Core.ViewModels.TraineeHistory
{
    using DAL.Entities;
    public class TraineeJobModel
    {

        //public Trainee Trainees;
        public int Type { get; set; }
        public IEnumerable<TraineeHistory> TraineeHistories { get; set; }
        //public List<int> ListSubjectAssign { get; set; }
        //public  List<int> SubjectCompleted { get; set; }

    }
}
