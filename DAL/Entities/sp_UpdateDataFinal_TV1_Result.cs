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
    
    public partial class sp_UpdateDataFinal_TV1_Result
    {
        public Nullable<int> CourseDetailId { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public Nullable<int> subjectid { get; set; }
        public Nullable<bool> IsAverageCalculate { get; set; }
        public Nullable<double> First_Check_Score { get; set; }
        public string First_Check_Result { get; set; }
        public Nullable<double> Re_Check_Score { get; set; }
        public string Re_Check_Result { get; set; }
        public Nullable<System.DateTime> coursestartdate { get; set; }
        public Nullable<System.DateTime> courseenddate { get; set; }
    }
}
