using System;
using DAL.Entities;

namespace TMS.Core.ViewModels.ViewModel
{
    public class ResultViewModels
    {

        public int? Traineeid { get; set; }
        public int? CourseDetailId { get; set; }
        public int? SubjectDetailId { get; set; }
        public bool? bit_Average_Calculate { get; set; }
        public float? First_Check_Score { get; set; }
        public string First_Check_Result { get; set; }
        public float? Re_Check_Score { get; set; }
        public string Re_Check_Result { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DeptCode { get; set; }
        public Nullable<System.DateTime> from { get; set; }
        public Nullable<System.DateTime> to { get; set; }
        public Nullable<float> score { get; set; }
        public string Grade { get; set; }
        public object FirstCheck { get; set; }
        public object Recheck { get; set; }
        public string Remark { get; set; }
        public object Point { get; set; }
        public Course_Result course_result { get;set; }
        public bool? type_checkfail { get; set; }
        public string remark_checkfail { get; set; }
        public int? ResultId { get; set; }
    }
}