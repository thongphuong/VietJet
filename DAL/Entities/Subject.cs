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
    
    public partial class Subject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Subject()
        {
            this.Subject1 = new HashSet<Subject>();
            this.SubjectDetails = new HashSet<SubjectDetail>();
        }
    
        public int Subject_Id { get; set; }
        public string str_Name { get; set; }
        public string str_Code { get; set; }
        public Nullable<int> int_Refresh_Cycle { get; set; }
        public Nullable<bool> bit_Deleted { get; set; }
        public Nullable<double> Excellent_Score { get; set; }
        public Nullable<double> Pass_Score { get; set; }
        public Nullable<bool> bit_ScoreOrResult { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public string str_Created_by { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public string str_Modified_By { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<int> int_Parent_Id { get; set; }
        public Nullable<int> int_GroupSubject_id { get; set; }
        public string Ancesstor { get; set; }
        public Nullable<bool> isDeleted { get; set; }
        public Nullable<bool> isActive { get; set; }
    
        public virtual CAT_GROUPSUBJECT CAT_GROUPSUBJECT { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Subject> Subject1 { get; set; }
        public virtual Subject Subject2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
    }
}