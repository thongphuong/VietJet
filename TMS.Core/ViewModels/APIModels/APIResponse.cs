using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
   public class APIResponse
    {
        public bool Result { get; set; }
        public string StatusResponse { get; set; }
        public string Message { get; set; }
        public string Content { get; set; }
    }
}
