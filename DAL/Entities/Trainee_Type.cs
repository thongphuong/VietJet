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
    
    public partial class Trainee_Type
    {
        public int Id { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public Nullable<int> Type { get; set; }
    
        public virtual Trainee Trainee { get; set; }
    }
}