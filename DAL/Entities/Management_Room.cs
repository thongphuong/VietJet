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
    
    public partial class Management_Room
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Management_Room()
        {
            this.Management_Room_Item = new HashSet<Management_Room_Item>();
        }
    
        public int Id { get; set; }
        public Nullable<int> IdCourse { get; set; }
        public Nullable<int> IdMeeting { get; set; }
        public Nullable<int> TotalDay { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Management_Room_Item> Management_Room_Item { get; set; }
        public virtual Meeting Meeting { get; set; }
    }
}