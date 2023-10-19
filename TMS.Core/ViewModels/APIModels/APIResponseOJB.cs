using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
   public class APIResponseOJB
    {

        public IEnumerable<warning> warnings { get; set; }

        public class warning
        {
             public string warningcode { get; set; }
            public string message { get; set; }
        }

      
       
    }
}
