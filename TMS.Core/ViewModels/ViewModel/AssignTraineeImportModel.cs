using System;
using DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ViewModel
{
    public class AssignTraineeImportModel
    {
        public int TraineeID { get; set; }
        public string EID { get; set; }
        public string Remark { get; set; }
        public string FullName { get; set; }
    }
}
