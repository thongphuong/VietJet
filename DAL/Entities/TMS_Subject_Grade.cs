//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class TMS_Subject_Grade
    {
        public int Id { get; set; }
        public Nullable<int> Course_Details_Id { get; set; }
        public Nullable<int> Grade_Id { get; set; }
        public Nullable<int> Score { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
        public virtual TMS_Grade TMS_Grade { get; set; }
    }
}
