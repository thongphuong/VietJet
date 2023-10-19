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
    
    public partial class Course_Detail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_Detail()
        {
            this.Course_Attendance = new HashSet<Course_Attendance>();
            this.Course_Blended_Learning = new HashSet<Course_Blended_Learning>();
            this.Course_Cost = new HashSet<Course_Cost>();
            this.Course_Detail_Course_Ingredients = new HashSet<Course_Detail_Course_Ingredients>();
            this.Course_Detail_Instructor = new HashSet<Course_Detail_Instructor>();
            this.Course_Detail_Room_Global = new HashSet<Course_Detail_Room_Global>();
            this.Course_Detail_Score = new HashSet<Course_Detail_Score>();
            this.Course_Detail_Subject_Note = new HashSet<Course_Detail_Subject_Note>();
            this.Course_Result = new HashSet<Course_Result>();
            this.Course_Result_Summary = new HashSet<Course_Result_Summary>();
            this.Course_Result_Temp = new HashSet<Course_Result_Temp>();
            this.Course_Detail_Room = new HashSet<Course_Detail_Room>();
            this.Management_Room = new HashSet<Management_Room>();
            this.Meetings = new HashSet<Meeting>();
            this.Payments = new HashSet<Payment>();
            this.Room_Booking = new HashSet<Room_Booking>();
            this.SurveyLMS = new HashSet<SurveyLM>();
            this.TMS_APPROVES = new HashSet<TMS_APPROVES>();
            this.TMS_APPROVES_LOG = new HashSet<TMS_APPROVES_LOG>();
            this.TMS_Course_Member = new HashSet<TMS_Course_Member>();
            this.TMS_SCHEDULE = new HashSet<TMS_SCHEDULE>();
            this.TMS_Subject_Grade = new HashSet<TMS_Subject_Grade>();
        }
    
        public int Id { get; set; }
        public Nullable<int> CourseId { get; set; }
        public Nullable<int> SubjectDetailId { get; set; }
        public Nullable<double> Duration { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string str_Created_By { get; set; }
        public string str_Modified_By { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public Nullable<int> RoomId { get; set; }
        public Nullable<System.DateTime> dtm_time_from { get; set; }
        public Nullable<System.DateTime> dtm_time_to { get; set; }
        public Nullable<double> PassScore { get; set; }
        public string time_from { get; set; }
        public string time_to { get; set; }
        public Nullable<int> type_leaning { get; set; }
        public Nullable<int> Approve_id { get; set; }
        public Nullable<bool> bit_Regisable { get; set; }
        public Nullable<int> LmsStatus { get; set; }
        public Nullable<System.DateTime> SyncedDate { get; set; }
        public Nullable<System.DateTime> SyncedBy { get; set; }
        public Nullable<int> SubjectDepartmentId { get; set; }
        public Nullable<int> AttemptsAllowed { get; set; }
        public Nullable<int> GradingMethod { get; set; }
        public Nullable<decimal> Allowance { get; set; }
        public Nullable<bool> Commitment { get; set; }
        public Nullable<double> CommitmetExpiredate { get; set; }
        public Nullable<System.DateTime> RegistryDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<int> Time { get; set; }
        public Nullable<int> TimeBlock { get; set; }
        public Nullable<bool> bit_Score_1st { get; set; }
        public Nullable<bool> bit_Score_2nd { get; set; }
        public Nullable<bool> bit_Course_Report { get; set; }
        public Nullable<bool> bit_Checked { get; set; }
        public string str_Report { get; set; }
        public Nullable<int> int_instructor { get; set; }
        public Nullable<int> int_status { get; set; }
        public string str_remark { get; set; }
        public Nullable<int> mark_type { get; set; }
    
        public virtual Course Course { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Attendance> Course_Attendance { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Blended_Learning> Course_Blended_Learning { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Cost> Course_Cost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Course_Ingredients> Course_Detail_Course_Ingredients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Instructor> Course_Detail_Instructor { get; set; }
        public virtual Room Room { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Room_Global> Course_Detail_Room_Global { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Score> Course_Detail_Score { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Subject_Note> Course_Detail_Subject_Note { get; set; }
        public virtual SubjectDetail SubjectDetail { get; set; }
        public virtual Trainee Trainee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result> Course_Result { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result_Summary> Course_Result_Summary { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Result_Temp> Course_Result_Temp { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Detail_Room> Course_Detail_Room { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Management_Room> Management_Room { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Meeting> Meetings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Room_Booking> Room_Booking { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyLM> SurveyLMS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_APPROVES> TMS_APPROVES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_APPROVES_LOG> TMS_APPROVES_LOG { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_Course_Member> TMS_Course_Member { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_SCHEDULE> TMS_SCHEDULE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TMS_Subject_Grade> TMS_Subject_Grade { get; set; }
    }
}
