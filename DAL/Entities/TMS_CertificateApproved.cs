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
    
    public partial class TMS_CertificateApproved
    {
        public int ID { get; set; }
        public Nullable<int> IdTrainee { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> Date_Of_Issue { get; set; }
        public Nullable<System.DateTime> Expiration_Date { get; set; }
        public string certificatefinal { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string Path { get; set; }
        public Nullable<int> CoureResultFinalID { get; set; }
        public Nullable<int> CourseResultID { get; set; }
        public Nullable<int> TypeCertificate { get; set; }
        public Nullable<int> ApproveBy { get; set; }
        public Nullable<System.DateTime> ApproveAt { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> CAT_CERTIFICATE_ID { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public Nullable<int> ModifyBy { get; set; }
        public Nullable<bool> IsRevoked { get; set; }
    
        public virtual CAT_CERTIFICATE CAT_CERTIFICATE { get; set; }
        public virtual Course_Result Course_Result { get; set; }
        public virtual Course_Result_Final Course_Result_Final { get; set; }
        public virtual USER USER { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual Trainee Trainee1 { get; set; }
        public virtual USER USER1 { get; set; }
    }
}