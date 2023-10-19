using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APICourse
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public IEnumerable<APIDepartment> Departments { get; set; }
        public int GroupSubjectId { get; set; }
        public int CourseType { get; set; }
        public int BeginDate { get; set; }
        public int EndDate { get; set; }
        public bool Customer { get; set; }
        public string Venue { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int MaxTranineeMembers { get; set; }
        public bool Survey { get; set; }
        public string Note { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int IsPublic { get; set; }
        public decimal Cost { get; set; }
        public int Maxgrade { get; set; }
        public class APIDepartment
        {
             public string DepartmentCode { get; set; }
            public string DepartmentName { get; set; }
            public string Description { get; set; }

            public APIDepartment(Course_TrainingCenter trainingCenter)
            {
                this.DepartmentCode = trainingCenter.Department?.Code ?? "";
                this.DepartmentName = trainingCenter.Department?.Name ?? "";
                this.Description = trainingCenter.Department?.Description ?? "";
            }
        }

        public APICourse (Course course)
        {
            this.CourseId = course.Id;
            this.CourseCode = course.Code;
            this.CourseName = course.Name;
            this.Departments = course.Course_TrainingCenter?.Select(a => new APIDepartment(a));
            this.GroupSubjectId = course.GroupSubjectId ?? 0;
            this.CourseType = course.CourseTypeId ?? 0;
            this.BeginDate =  (int)DateUtil.ConvertToUnixTime((DateTime)course.StartDate);
            this.EndDate = (int)DateUtil.ConvertToUnixTime((DateTime)course.EndDate);
            this.Customer = course.CustomerType ?? false;
            this.Venue = course.Venue ?? "";
            this.CompanyCode =  course.Company?.str_code ?? "";
            this.CompanyName = course.Company?.str_Name ?? "";
            this.MaxTranineeMembers = course.NumberOfTrainee ?? 0;
            this.Survey = course.Survey ?? false;
            this.Note = course.Note ?? "";
            this.IsDeleted = course.IsDeleted ?? false;
            this.IsPublic = course.IsPublic == true ? 0 : 1;
            this.Cost = course.TrainingProgam_Cost?.LastOrDefault()?.Cost ?? 0;
            this.Maxgrade = course.MaxGrade == null ? 100 : course.MaxGrade.Value;
            var checkApproveProgram = course.TMS_APPROVES.Any(
                a =>
                    a.int_Type == (int) UtilConstants.ApproveType.Course &&
                    a.int_id_status != (int) UtilConstants.EStatus.Approve);
            if (checkApproveProgram)
            {
                this.IsActive = false;
            }
            else
            {
                this.IsActive = course.IsActive ?? false;
            }
           
        }
    }

   
}
