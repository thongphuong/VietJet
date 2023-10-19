using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
   public class APITraineeHistory
    {
        public string TraineeCode { get; set; }
        public string JobTitleCode { get; set; }
        public int TraineeHistoryId { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Trainning> LearningCourses { get; set; }
        public IEnumerable<MissingTrainning> MissingCourses { get; set; }
        public IEnumerable<CourseCompleted> CompletedCourses { get; set; }

        public APITraineeHistory(DAL.Entities.TraineeHistory traineeHistory)
        {
            this.TraineeCode = traineeHistory.Trainee.str_Staff_Id ?? string.Empty;
            this.JobTitleCode = traineeHistory.JobTitle.Code;
            this.TraineeHistoryId = traineeHistory.Id;
            this.Courses = traineeHistory.TraineeHistory_Item.Select(a => new Course(a));
            this.LearningCourses = traineeHistory.TraineeHistory_Item.Where(a => a.Status == (int)UtilConstants.StatusTraineeHistory.Trainning).Select(a => new Trainning(a));
            this.MissingCourses = traineeHistory.TraineeHistory_Item.Where(a => a.Status == (int)UtilConstants.StatusTraineeHistory.Missing).Select(a => new MissingTrainning(a));
            this.CompletedCourses =
                traineeHistory.TraineeHistory_Item.Where(
                    a => a.Status == (int)UtilConstants.StatusTraineeHistory.Completed)
                    .Select(a => new CourseCompleted(a));
        }
        public class Course
        {
            public string CourseCode { get; set; }

            public Course(DAL.Entities.TraineeHistory_Item item)
            {
                this.CourseCode = item.SubjectDetail.Code;
            }
        }

        public class Trainning
        {
            public string CourseCode { get; set; }
            public Trainning(DAL.Entities.TraineeHistory_Item item)
            {
                this.CourseCode = item.SubjectDetail.Code;
            }
        }
        public class MissingTrainning
        {
            public string CourseCode { get; set; }
            public MissingTrainning(DAL.Entities.TraineeHistory_Item item)
            {
                this.CourseCode = item.SubjectDetail.Code;
            }
        }
        public class CourseCompleted
        {
            public string CourseCode { get; set; }
            public CourseCompleted(DAL.Entities.TraineeHistory_Item item)
            {
                this.CourseCode = item.SubjectDetail.Code;
            }

        }
    }
}
