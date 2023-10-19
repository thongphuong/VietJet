using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Approve
{
   public class ApproveIndex
    {
        public Dictionary<int ,string> Types { get; set; }
        public Dictionary<int, string> Status { get; set; }

    }
}
