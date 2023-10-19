using System;
using System.Collections.Generic;

namespace TMS.Core.ViewModels.Courses
{
    using DAL.Entities;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Utils;

    public class APICourseTraineeLMSViewModel
    {
        [Required]
        public int CourseDetailId { get; set; }
        public IEnumerable<AssignTrainee> assignTrainee { get; set; }
        public class AssignTrainee
        {
            [Required]
            public string TraineeCode { get; set; }
        }
    }

}
