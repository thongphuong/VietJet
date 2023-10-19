using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class QueryData
    {
        public int id { get; set; }
       public int type { get; set; }
        public string strStatus { get; set; }
        public string strSendMail { get; set; }
        public string note { get; set; }
    }
}
