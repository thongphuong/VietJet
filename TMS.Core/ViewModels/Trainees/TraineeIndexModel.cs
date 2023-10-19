using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Trainees
{
   public  class TraineeIndexModel
    {
        public string Departments { get; set; }
        public Dictionary<int, string> Genders { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public SelectList Status { get; set; }
        public SelectList Type { get; set; }
    }
}
