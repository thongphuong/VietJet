using System.Collections.Generic;
using DAL.Entities;
namespace TMS.Core.ViewModels.ViewModel
{
    public class SubjectResultViewModel
    {
        public Course course { get; set; }
        public Subject subject { get; set; }
        public Course_Detail courseDetail { get; set; }
        public List<Course_Result> attendantList { get; set; }
    }
}