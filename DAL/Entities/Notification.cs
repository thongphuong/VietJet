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
    
    public partial class Notification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Notification()
        {
            this.Notification_Detail = new HashSet<Notification_Detail>();
        }
    
        public int MessageID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string Message { get; set; }
        public string MessageContent { get; set; }
        public string URL { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string MessageVN { get; set; }
        public string MessageContentVN { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notification_Detail> Notification_Detail { get; set; }
    }
}