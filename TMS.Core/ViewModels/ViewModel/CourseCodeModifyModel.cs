using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.ViewModel
{
    public class CourseCodeModifyModel
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = @"lblCourseName", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? CourseId { get; set; }
        public string CourseName { get; set; }
        [Required]
        [Display(Name = @"lblType", ResourceType = typeof(App_GlobalResources.Resource))]
        public int? Cost { get; set; }

        [Display(Name = @"lblCost", ResourceType = typeof(App_GlobalResources.Resource))]
        [Range(0,Int32.MaxValue)]
        public decimal? CostValue { get; set; }
        [Display(Name = @"lblTime", ResourceType = typeof(App_GlobalResources.Resource))]
        public string DateFromTo { get; set; }
        [Required]
        [Display(Name = @"lblSubject", ResourceType = typeof(App_GlobalResources.Resource))]
        public int CourseDetailId { get; set; }
        public string SubjectName { get; set; }
        public SelectList Courses { get; set; }
        public SelectList Costes { get; set; }

        public SelectList Subjects { get; set; }

        [Display(Name = @"lblExpectedCost", ResourceType = typeof(App_GlobalResources.Resource))]
        [Range(0, Int32.MaxValue)]
        public decimal? ExpectedCost { get; set; }
    }
}