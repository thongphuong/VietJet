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
    
    public partial class Instructor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Instructor()
        {
            this.Room_Booking = new HashSet<Room_Booking>();
            this.Room_Booking1 = new HashSet<Room_Booking>();
        }
    
        public int Instructor_Id { get; set; }
        public string str_Fullname { get; set; }
        public string str_Phone_No { get; set; }
        public string str_Email { get; set; }
        public Nullable<int> Pay_per_Hour { get; set; }
        public Nullable<bool> bit_Deleted { get; set; }
        public string str_Created_By { get; set; }
        public string str_Modified_By { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Room_Booking> Room_Booking { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Room_Booking> Room_Booking1 { get; set; }
    }
}
