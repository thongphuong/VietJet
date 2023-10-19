using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DAL.Entities;

namespace TMS.Core.ViewModels.ReportModels
{

   public class TrainingPlanModel
    {
       public Dictionary<string,string> DictionaryCourses { get; set; }
    }

    public class TrainingPlanPrint
    {
        
        public string Intructors { get; set; }
        public IEnumerable<Course> Courses { get; set; } 
        public string[] Title { get; set; }

    }

   
}
