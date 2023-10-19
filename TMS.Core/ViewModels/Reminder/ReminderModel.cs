using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace TMS.Core.ViewModels.Reminder
{
   public class ReminderModel
    {
        public Dictionary<int,string> Subjects { get; set; }
        public SelectList JobTitleList { get; set; }
        public string Departments { get; set; }
        public IEnumerable<SubjectDetail> SubjectList { get; set; }
    }
}
