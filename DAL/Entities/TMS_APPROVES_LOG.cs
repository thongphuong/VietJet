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
    
    public partial class TMS_APPROVES_LOG
    {
        public int id { get; set; }
        public string str_content { get; set; }
        public Nullable<System.DateTime> dtm_create { get; set; }
        public Nullable<int> int_course_id { get; set; }
        public Nullable<int> int_course_detail_id { get; set; }
        public Nullable<int> int_type { get; set; }
        public Nullable<int> int_status { get; set; }
        public Nullable<System.DateTime> dtm_approved_request { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual Course_Detail Course_Detail { get; set; }
    }
}
