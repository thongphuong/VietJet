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
    
    public partial class CourseRemarkCheckFail_LMS
    {
        public int id { get; set; }
        public Nullable<int> courseid_LMS { get; set; }
        public Nullable<int> userid_LMS { get; set; }
        public string content { get; set; }
        public Nullable<System.DateTime> create_date { get; set; }
        public Nullable<int> CourseDetailid_TMS { get; set; }
        public string str_staff_code_Trainee { get; set; }
        public string str_staff_code_Instructor { get; set; }
    }
}