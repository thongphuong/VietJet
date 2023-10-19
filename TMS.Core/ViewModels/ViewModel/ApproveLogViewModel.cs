using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.ViewModel
{
    public class ApproveLogViewModel
    {
        public IList<TMS_APPROVES_LOG> data { get; set; }
        public IList<TMS_APPROVES_HISTORY> approveHis { get; set; }
        public IList<USER> approveuser { get; set; }
        public Dictionary<int, string> approveStatus { get; set; }
    }
}
