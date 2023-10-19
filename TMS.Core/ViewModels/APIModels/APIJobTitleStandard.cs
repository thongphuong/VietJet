using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIJobTitleStandard
    {

        public string TraineeCode { get; set; }
        public IEnumerable<JobTitle> JobTitles { get; set; }


        public APIJobTitleStandard(Course_Result_Final final)
        {
            this.TraineeCode = final.Trainee.str_Staff_Id ?? string.Empty;
            var courseIdAssign = final.Course.Course_Detail.Select(a => (int)a.SubjectDetailId);
            var courseCompleted = final.Course.TMS_APPROVES.Where(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve).Select(a => (int)a.Course_Detail.SubjectDetailId);
            var traineeHistories = final.Trainee.TraineeHistories.ToList();
            this.JobTitles = traineeHistories.Select(a => new JobTitle(a, courseIdAssign , courseCompleted));
        }
        public class JobTitle
        {
            public string JobTitleCode { get; set; }
            public IEnumerable<Course> Courses { get; set; }
            public IEnumerable<Trainning> LearningCourses { get; set; }
            public IEnumerable<MissingTrainning> MissingCourses { get; set; }
            public IEnumerable<CourseCompleted> CompletedCourses { get; set; } 
            public JobTitle(DAL.Entities.TraineeHistory traineeHistory, IEnumerable<int> courseIdAssign , IEnumerable<int> courseCompleted)
            {
                this.JobTitleCode = traineeHistory.JobTitle.Code ?? string.Empty;
                var courses = traineeHistory.JobTitle.Title_Standard.Select(a => a.SubjectDetail).ToArray();
               


                this.Courses = courses.Select(a => new Course(a, traineeHistory)).ToArray();

                this.MissingCourses = courses.Select(a => new MissingTrainning(a, traineeHistory, courseIdAssign , courseCompleted)).Where(b => !string.IsNullOrEmpty(b.CourseCode)).ToArray();

                this.LearningCourses = courses.Select(a => new Trainning(a, traineeHistory, courseIdAssign , courseCompleted)).Where(b => !string.IsNullOrEmpty(b.CourseCode)).ToArray();

                this.CompletedCourses = courses.Select(a => new CourseCompleted(a, traineeHistory, courseCompleted)).Where(b => !string.IsNullOrEmpty(b.CourseCode)).ToArray();

            }
        }
        public class Course
        {
            public string CourseCode { get; set; }

            public Course(DAL.Entities.SubjectDetail course, DAL.Entities.TraineeHistory jobHistory)
            {
                if (course.Title_Standard.Any(a => a.Job_Title_Id == jobHistory.Job_Title_Id))
                {
                    this.CourseCode = course.Code;
                }
            }
        }

        public class Trainning
        {
            public string CourseCode { get; set; }

            public Trainning(DAL.Entities.SubjectDetail course, DAL.Entities.TraineeHistory jobHistory, IEnumerable<int> courseIdAssign, IEnumerable<int> courseCompleted)
            {
                if (course.Title_Standard.Any(a => a.Job_Title_Id == jobHistory.Job_Title_Id 
                                                && courseIdAssign.Contains((int)a.Subject_Id)
                                                 && !courseCompleted.Contains((int)a.Subject_Id)))
                {
                    this.CourseCode = course.Code;
                }
            }
        }
        public class MissingTrainning
        {
            public string CourseCode { get; set; }
            public MissingTrainning(DAL.Entities.SubjectDetail course, DAL.Entities.TraineeHistory jobHistory, IEnumerable<int> courseIdAssign , IEnumerable<int> courseCompleted)
            {
                if (course.Title_Standard.Any(a => a.Job_Title_Id == jobHistory.Job_Title_Id 
                                                && !courseIdAssign.Contains((int)a.Subject_Id) 
                                                && !courseCompleted.Contains((int)a.Subject_Id)))
                {
                    this.CourseCode = course.Code;
                }
            }
        }
        public class CourseCompleted 
        {
            public string CourseCode { get; set; }
            public CourseCompleted(DAL.Entities.SubjectDetail course, DAL.Entities.TraineeHistory jobHistory, IEnumerable<int> courseCompleted)
            {
                if (course.Title_Standard.Any(a => a.Job_Title_Id == jobHistory.Job_Title_Id
                                                && courseCompleted.Contains((int)a.Subject_Id)))
                {
                    this.CourseCode = course.Code;
                }
            }
        }
    }
}
