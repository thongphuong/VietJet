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
    
    public partial class Schedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Schedule()
        {
            this.Schedules_Method = new HashSet<Schedules_Method>();
            this.Schedules_destination = new HashSet<Schedules_destination>();
            this.Schedules_Type = new HashSet<Schedules_Type>();
            this.TMS_SentEmail = new HashSet<TMS_SentEmail>();
        }
    
        public int id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> IsDefault { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public Nullable<int> IdTemplateMail { get; set; }
    
        public virtual CAT_MAIL CAT_MAIL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedules_Method> Schedules_Method { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedules_destination> Schedules_destination { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedules_Type> Schedules_Type { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_SentEmail> TMS_SentEmail { get; set; }
    }
}