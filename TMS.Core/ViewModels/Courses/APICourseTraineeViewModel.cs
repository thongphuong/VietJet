using System;
using System.Collections.Generic;

namespace TMS.Core.ViewModels.Courses
{
    using DAL.Entities;
   
  

    public class APICourseTraineeViewModel
    {
        public int CourseDetailId { get; set; }
        public string SubjectCode { get; set; }
        public string CourseCode { get; set; }
        public string TraineeCode { get; set; }
        public bool? IsDeleted { get; set; }
        public bool IsActive { get; set; }


        public APICourseTraineeViewModel(TMS_Course_Member courseMember)
        {
            this.CourseDetailId = courseMember.Course_Details_Id ?? 0;
            this.SubjectCode = courseMember.Course_Detail?.SubjectDetail?.Code ?? "";
            this.CourseCode = courseMember.Course_Detail?.Course?.Code ?? "";
            this.TraineeCode = courseMember.Trainee?.str_Staff_Id ?? "";
            this.IsDeleted = courseMember.IsDelete;
            this.IsActive = (bool)courseMember.IsActive;
        }

        //public APICourseTraineeViewModel(Course_Result_Final courseResultFinal)
        //{
        //    this.SubjectId = courseResultFinal.Course.Course_Detail.FirstOrDefault().SubjectDetailId;
        //    this.SubjectCode = courseResultFinal.Course.Course_Detail.FirstOrDefault().SubjectDetail.Code;
        //    this.TraineeId = courseResultFinal.Trainee.Id;
        //    this.TraineeCode = courseResultFinal.Trainee.str_Staff_Id;
        //    this.IsDelete = courseResultFinal.IsDeleted;

        //}

        //public APICourseTraineeViewModel(Course course)
        //{

        //    this.SubjectId = course?.Course_Detail?.FirstOrDefault()?.SubjectDetailId;
        //    this.SubjectCode = course?.Course_Detail?.FirstOrDefault()?.SubjectDetail.Code;
        //    this.TraineeId = course?.Course_Result_Final?.FirstOrDefault()?.Trainee.Id;
        //    this.TraineeCode = course?.Course_Result_Final?.FirstOrDefault()?.Trainee.str_Staff_Id;
        //    this.IsDelete = course?.Course_Result_Final?.FirstOrDefault()?.IsDeleted;
        //}
    }

}
