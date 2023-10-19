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
    
    public partial class Room_Booking
    {
        public int Room_Booking_Id { get; set; }
        public Nullable<int> Course_Detail_Id { get; set; }
        public Nullable<int> Room_Id { get; set; }
        public Nullable<int> Instructor1_Id { get; set; }
        public Nullable<int> Instructor2_Id { get; set; }
        public Nullable<System.DateTime> dtm_From { get; set; }
        public Nullable<System.DateTime> dtm_To { get; set; }
        public Nullable<System.TimeSpan> tim_Start_Time { get; set; }
        public Nullable<System.TimeSpan> tim_End_Time { get; set; }
        public string str_Description { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public string str_Created_By { get; set; }
        public string str_Modified_By { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public Nullable<bool> bit_Deleted { get; set; }
        public Nullable<int> int_Duration { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
        public virtual Instructor Instructor { get; set; }
        public virtual Instructor Instructor1 { get; set; }
        public virtual Room Room { get; set; }
    }
}
