using System;

namespace TMS.Core.ViewModels.ViewModel
{
    public class CostViewModel
    {



        public string NameCost { get; set; }
        public string CodeCost { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<System.DateTime> createday { get; set; }

    }
}