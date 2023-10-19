using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Core.ViewModels.ViewModel
{
    public class ApprovalType
    {
        public ApprovalType()
        {
            
        }
        
        public Nullable<int> Department_Id { get; set; }
        public string str_Name { get; set; }
      
    }
}