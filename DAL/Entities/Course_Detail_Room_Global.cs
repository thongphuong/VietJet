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
    
    public partial class Course_Detail_Room_Global
    {
        public int ID { get; set; }
        public Nullable<int> CourseDetailID { get; set; }
        public Nullable<int> RoomID { get; set; }
        public string Remark { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
    }
}
