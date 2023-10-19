using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.ViewModels.ViewModel;

namespace TMS.Core.ViewModels.ReportModels
{
   public class CostModelRp
    {
        public IEnumerable<Course_Cost> CourseCosts { get; set; }
    }
}
