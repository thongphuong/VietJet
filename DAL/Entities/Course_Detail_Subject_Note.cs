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
    
    public partial class Course_Detail_Subject_Note
    {
        public int id { get; set; }
        public Nullable<int> SubjectDetailId { get; set; }
        public Nullable<int> course_detail_id { get; set; }
        public string note { get; set; }
        public Nullable<System.DateTime> createdate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
        public virtual SubjectDetail SubjectDetail { get; set; }
    }
}
