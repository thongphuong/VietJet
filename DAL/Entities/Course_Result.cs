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
    
    public partial class Course_Result
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_Result()
        {
            this.Course_Result_Course_Detail_Ingredient = new HashSet<Course_Result_Course_Detail_Ingredient>();
            this.CourseRemarkCheckFails = new HashSet<CourseRemarkCheckFail>();
            this.LMS_Expiredate = new HashSet<LMS_Expiredate>();
            this.TMS_CertificateApproved = new HashSet<TMS_CertificateApproved>();
        }
    
        public int Id { get; set; }
        public Nullable<int> CourseDetailId { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public Nullable<double> Score { get; set; }
        public string Result { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<System.DateTime> ModifiedAt { get; set; }
        public Nullable<int> inCourseMemberId { get; set; }
        public Nullable<int> times { get; set; }
        public Nullable<int> LmsStatus { get; set; }
        public Nullable<bool> Type { get; set; }
        public Nullable<int> Subject_Id { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public string CertificateSubject { get; set; }
        public Nullable<System.DateTime> CreateCertificateAt { get; set; }
        public Nullable<int> StatusCertificate { get; set; }
        public string Backgroundcertificate { get; set; }
        public Nullable<double> First_Check_Score { get; set; }
        public string First_Check_Result { get; set; }
        public Nullable<double> Re_Check_Score { get; set; }
        public string Re_Check_Result { get; set; }
        public string Deleted_By { get; set; }
        public Nullable<System.DateTime> Deleted_At { get; set; }
        public Nullable<int> CertificateID { get; set; }
        public string Path { get; set; }
    
        public virtual CAT_CERTIFICATE CAT_CERTIFICATE { get; set; }
        public virtual Course_Detail Course_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result_Course_Detail_Ingredient> Course_Result_Course_Detail_Ingredient { get; set; }
        public virtual SubjectDetail SubjectDetail { get; set; }
        public virtual TMS_Course_Member TMS_Course_Member { get; set; }
        public virtual Trainee Trainee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourseRemarkCheckFail> CourseRemarkCheckFails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LMS_Expiredate> LMS_Expiredate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_CertificateApproved> TMS_CertificateApproved { get; set; }
    }
}
