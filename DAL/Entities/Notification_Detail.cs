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
    
    public partial class Notification_Detail
    {
        public int id { get; set; }
        public Nullable<int> iduserfrom { get; set; }
        public Nullable<int> iddata { get; set; }
        public Nullable<int> idmessenge { get; set; }
        public Nullable<System.DateTime> datesend { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> idtrainee { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual Notification Notification { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual USER USER { get; set; }
    }
}
