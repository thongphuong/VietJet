using System;

namespace TMS.Core.ViewModels.Common
{
    [Serializable()]
    public class AjaxResponseViewModel
    {
        // validate/complete return true overwise return false
        public bool result { get; set; }
        public bool Runs { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public object data1 { get; set; }
        public int type { get; set; }
        public object typecourse { get; set; }
        public int count { get; set; }
    }
}
