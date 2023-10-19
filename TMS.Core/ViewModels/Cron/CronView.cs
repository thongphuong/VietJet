using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Cron
{
    public class CronView
    {
    
            public string Name { get; set; }
            public bool IsSync { get; set; }
            public int count { get; set; }
            public int count_noreponse { get; set; }
    }
}
