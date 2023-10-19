using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class DetailCourseCostModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Venue { get; set; }
        public string Note { get; set; }
        public string Date { get; set; }
        public string Cost { get; set; }
        
    }
}
