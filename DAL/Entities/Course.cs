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
    
    public partial class Course
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course()
        {
            this.Course_Cost = new HashSet<Course_Cost>();
            this.Course_Detail = new HashSet<Course_Detail>();
            this.Course_Detail_Instructor = new HashSet<Course_Detail_Instructor>();
            this.Course_LMS_STATUS = new HashSet<Course_LMS_STATUS>();
            this.Course_Result_Final = new HashSet<Course_Result_Final>();
            this.Course_Subject_Item = new HashSet<Course_Subject_Item>();
            this.Course_TrainingCenter = new HashSet<Course_TrainingCenter>();
            this.TMS_APPROVES = new HashSet<TMS_APPROVES>();
            this.TMS_APPROVES_LOG = new HashSet<TMS_APPROVES_LOG>();
            this.TMS_Course_History = new HashSet<TMS_Course_History>();
            this.TMS_Course_Member_Remark = new HashSet<TMS_Course_Member_Remark>();
            this.TrainingProgam_Cost = new HashSet<TrainingProgam_Cost>();
            this.Transaction_Course_Trainee = new HashSet<Transaction_Course_Trainee>();
            this.Trainees = new HashSet<Trainee>();
            this.Course_Result_Final_Temp = new HashSet<Course_Result_Final_Temp>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Venue { get; set; }
        public Nullable<int> TypeResult { get; set; }
        public Nullable<bool> CustomerType { get; set; }
        public Nullable<bool> Survey { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> NumberOfTrainee { get; set; }
        public string Note { get; set; }
        public Nullable<int> GroupSubjectId { get; set; }
        public Nullable<int> CourseTypeId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public Nullable<int> LMSStatus { get; set; }
        public Nullable<System.DateTime> LMSRequestDate { get; set; }
        public string LMSRequestBy { get; set; }
        public Nullable<int> MinTrainee { get; set; }
        public string LastName { get; set; }
        public Nullable<bool> IsPublic { get; set; }
        public Nullable<decimal> ProgramCost { get; set; }
        public Nullable<int> MaxGrade { get; set; }
        public Nullable<bool> IsBindSubject { get; set; }
        public Nullable<int> parent_course_id { get; set; }
        public Nullable<int> is_data_new { get; set; }
        public Nullable<int> int_manager_id { get; set; }
        public Nullable<int> Job_Title_Id { get; set; }
        public Nullable<int> int_StatusNew { get; set; }
        public Nullable<bool> IsBlockApproval { get; set; }
    
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Cost> Course_Cost { get; set; }
        public virtual Course_Type_Result Course_Type_Result { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail> Course_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Instructor> Course_Detail_Instructor { get; set; }
        public virtual JobTitle JobTitle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_LMS_STATUS> Course_LMS_STATUS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result_Final> Course_Result_Final { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Subject_Item> Course_Subject_Item { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_TrainingCenter> Course_TrainingCenter { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_APPROVES> TMS_APPROVES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_APPROVES_LOG> TMS_APPROVES_LOG { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_Course_History> TMS_Course_History { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_Course_Member_Remark> TMS_Course_Member_Remark { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TrainingProgam_Cost> TrainingProgam_Cost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_Course_Trainee> Transaction_Course_Trainee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Trainee> Trainees { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result_Final_Temp> Course_Result_Final_Temp { get; set; }
    }
}
