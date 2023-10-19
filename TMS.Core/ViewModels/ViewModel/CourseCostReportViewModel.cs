using System;

namespace TMS.Core.ViewModels.ViewModel
{
    public class CourseCostReportViewModel
    {
        public Nullable<int> Course_Id { get; set; }
        public string str_Name { get; set; }
        public Nullable<System.DateTime> dtm_StartDate { get; set; }
        public Nullable<System.DateTime> dtm_EndDate { get; set; }
        public Nullable<Decimal> Cost { get; set; }
        public string CourseCode { get; set; }
        public decimal? ExpectedCost { get; set; }
    }
}