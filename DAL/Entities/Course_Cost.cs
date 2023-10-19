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
    
    public partial class Course_Cost
    {
        public int id { get; set; }
        public Nullable<int> cost_id { get; set; }
        public Nullable<decimal> cost { get; set; }
        public Nullable<int> unit_id { get; set; }
        public Nullable<System.DateTime> createdate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> coursedetail_id { get; set; }
        public Nullable<int> course_id { get; set; }
        public Nullable<int> subject_id { get; set; }
        public Nullable<decimal> ExpectedCost { get; set; }
        public string subject_code { get; set; }
        public string subject_name { get; set; }
    
        public virtual CAT_COSTS CAT_COSTS { get; set; }
        public virtual CAT_UNITS CAT_UNITS { get; set; }
        public virtual Course Course { get; set; }
        public virtual Course_Detail Course_Detail { get; set; }
        public virtual SubjectDetail SubjectDetail { get; set; }
    }
}