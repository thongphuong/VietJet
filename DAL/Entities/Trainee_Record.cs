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
    
    public partial class Trainee_Record
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Trainee_Record()
        {
            this.Trainee_Upload_Files = new HashSet<Trainee_Upload_Files>();
        }
    
        public int Trainee_Record_Id { get; set; }
        public Nullable<int> Trainee_Id { get; set; }
        public string str_Subject { get; set; }
        public string str_Duration { get; set; }
        public string str_Location { get; set; }
        public string str_Trainer { get; set; }
        public string str_Result { get; set; }
        public Nullable<bool> bit_Deleted { get; set; }
        public Nullable<bool> isActive { get; set; }
        public string str_Created_By { get; set; }
        public string str_Modified_By { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public string str_note { get; set; }
        public Nullable<System.DateTime> dtm_time_from { get; set; }
        public Nullable<System.DateTime> dtm_time_to { get; set; }
        public string str_organization { get; set; }
    
        public virtual Trainee Trainee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Trainee_Upload_Files> Trainee_Upload_Files { get; set; }
    }
}
