using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    using System.Data.Entity.Core.Objects;
    using DAL.Entities;

    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();
        IRepository<T> Repository<T>() where T : class;
        ObjectResult<sp_GetTrainingHeaderTV_Result> sp_GetTrainingHeader_Result(string listId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd, string status);
        ObjectResult<sp_GetTrainingDetail_Result> sp_GetTrainingDetail_Result(string listId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd, string status);
        ObjectResult<sp_GetCostHeader_Result> sp_GetCostHeader_Result(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd);
        ObjectResult<sp_GetCostDetail_Result> sp_GetCostDetail_Result(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd);
        ObjectResult<sp_Reminder_TV_Result> sp_GetReminder_Result(string departlist, string listJobtitle,
    string subjectName, string subjectCode, DateTime dateFrom, int nom);
        ObjectResult<sp_GetSubjectResult_TV_Result> sp_GetSubjectResult_TV_Result(int courseDetailId);
        ObjectResult<sp_GetInstructorReport_TV_Result> sp_GetInstructorReport_TV_Result(string subjectName, string subjectCode,
    string traineeName, string traineeCode, int departmentID, int jobtitleID, string sortDirection, string orderingFunction);
        ObjectResult<sp_GetListEmployee_TV_Result> sp_GetEmployeeList_TV_Result(string filterCodeOrName);
        ObjectResult<sp_GetDetail_TV_Result> sp_GetDetail_Result(string venue, string courseCode, string courseName, int? departmentid, DateTime? dateFrom, DateTime? dateTo, string courids,int? CourseID);
    }
    
}
