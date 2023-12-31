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
    
    public partial class PotentialSuccessors_Item
    {
        public int Id { get; set; }
        public Nullable<int> SuccessorsID { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public Nullable<int> JobHistoryId { get; set; }
        public Nullable<System.DateTime> CreateDay { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string Remark { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public Nullable<int> ModifyBy { get; set; }
        public Nullable<System.DateTime> ModifyAt { get; set; }
    
        public virtual JobTitle JobTitle { get; set; }
        public virtual PotentialSuccessor PotentialSuccessor { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual USER USER { get; set; }
        public virtual USER USER1 { get; set; }
    }
}
