using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.AjaxModels.AjaxRoom
{
    public class AjaxMeeting
    {
        public string Name { get; set; }
        public string Participants { get; set; }
        public string Room { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string Option { get; set; }
    }
}
