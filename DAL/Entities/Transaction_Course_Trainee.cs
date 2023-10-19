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
    
    public partial class Transaction_Course_Trainee
    {
        public int Id { get; set; }
        public Nullable<int> CourseId { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public string TransactionCode { get; set; }
        public Nullable<decimal> CurrentPurchaseCost { get; set; }
        public Nullable<decimal> TransactionCost { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public Nullable<int> TransactionTypeId { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual Transaction_Type Transaction_Type { get; set; }
    }
}
