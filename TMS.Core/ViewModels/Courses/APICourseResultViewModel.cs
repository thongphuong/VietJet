using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS.Core.ViewModels.Courses
{
    public class APICourseResultViewModel
    {

        public int CourseDetailId { get; set; }
        //[Required]
        ////CourseDetail
        //public string SubjectCode { get; set; }
        //[Required]
        //public string CourseCode { get; set; }
        [Required]
        public int? Time { get; set; }
        [Required]
        public string TraineeCode { get; set; }
        public string Score { get; set; }
        //public string Remark { get; set; }
        //public IEnumerable<APIIngredient> Ingredient {get; set; }

        public class APIIngredient
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public int Weigth { get; set; }
            public string Score { get; set; }
                 
        }
    }
}
