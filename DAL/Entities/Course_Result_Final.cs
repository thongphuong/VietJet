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
    
    public partial class Course_Result_Final
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_Result_Final()
        {
            this.TMS_CertificateApproved = new HashSet<TMS_CertificateApproved>();
        }
    
        public int id { get; set; }
        public Nullable<int> courseid { get; set; }
        public Nullable<int> traineeid { get; set; }
        public Nullable<double> point { get; set; }
        public Nullable<int> grade { get; set; }
        public string SRNO { get; set; }
        public string ATO { get; set; }
        public Nullable<System.DateTime> createday { get; set; }
        public string createby { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<int> MemberStatus { get; set; }
        public Nullable<System.DateTime> CreateCertificateDate { get; set; }
        public Nullable<int> CreateCertificateBy { get; set; }
        public string Path { get; set; }
        public Nullable<int> LmsStatus { get; set; }
        public Nullable<int> CAT_CERTIFICATE_ID { get; set; }
        public string certificatefinal { get; set; }
        public Nullable<int> statusCertificate { get; set; }
        public string backgroundcertificate { get; set; }
        public string remark { get; set; }
    
        public virtual CAT_CERTIFICATE CAT_CERTIFICATE { get; set; }
        public virtual Course Course { get; set; }
        public virtual Trainee Trainee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_CertificateApproved> TMS_CertificateApproved { get; set; }
    }
}