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
    
    public partial class Meeting_Participants
    {
        public int Id { get; set; }
        public Nullable<int> Meeting_Id { get; set; }
        public Nullable<int> Participant_Id { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual Meeting Meeting { get; set; }
        public virtual Trainee Trainee { get; set; }
    }
}
