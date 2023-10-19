using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.PostNews
{
    public class AjaxCategory
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameParent { get; set; }
        public string Description { get; set; }
        public string Ancestor { get; set; }
        public string Status { get; set; }
        public string Option { get; set; }

    }
}
