using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.ViewModel
{
    public class CourseDetailNoteViewModel
    {
        public int? Id { get; set; }
        [Required]
        [Range(0, int.MaxValue)]      
        public int? Subject { get; set; }
        [Required]
        [Range(0,int.MaxValue)]
        public int? CourseId { get; set; }

        public string Code { get; set; }
        public int? TrainingId { get; set; }
        [Required]
        public string Note { get; set; }
        public SelectList TrainingCenters { get; set; }
        public SelectList Courses { get; set; }
    }
}