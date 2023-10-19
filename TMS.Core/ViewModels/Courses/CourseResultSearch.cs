using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
   public class CourseResultSearch
    {
       public string Code { get; set; }
       public string Name { get; set; }
        public Dictionary<int,string> GroupCosts { get; set; } 
    }
}
