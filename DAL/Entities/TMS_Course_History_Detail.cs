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
    
    public partial class TMS_Course_History_Detail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TMS_Course_History_Detail()
        {
            this.TMS_Course_History_Result = new HashSet<TMS_Course_History_Result>();
        }
    
        public int Course_Detail_Id { get; set; }
        public Nullable<int> Course_Id { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public Nullable<bool> bit_Score_1st { get; set; }
        public Nullable<bool> bit_Score_2nd { get; set; }
        public Nullable<bool> bit_Course_Report { get; set; }
        public Nullable<bool> bit_Checked { get; set; }
        public Nullable<bool> bit_Deleted { get; set; }
        public string str_Created_By { get; set; }
        public string str_Modified_By { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public string str_Report { get; set; }
    
        public virtual SubjectDetail SubjectDetail { get; set; }
        public virtual TMS_Course_History TMS_Course_History { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_Course_History_Result> TMS_Course_History_Result { get; set; }
    }
}