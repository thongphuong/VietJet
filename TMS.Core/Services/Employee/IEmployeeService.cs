using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Employee;
using TMS.Core.ViewModels.Trainees;
using TMS.Core.ViewModels.UserModels;

namespace TMS.Core.Services.Employee
{
    using System.Data.Objects;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using TMS.Core.ViewModels.Subjects;
    using TMS.Core.ViewModels.ViewModel;

    public interface IEmployeeService : IBaseService
    {
        #region send mail to portal user
        void InsertSendMailUser(SentMailUser sentMailUser);
        void UpdateSendMailUser(SentMailUser sentMailUser);

        bool CheckExsistSendMailUser(string user, string email);

        SentMailUser GetByEmail(string email);
        #endregion

        List<int> UserPermissions { get; set; }
        Trainee GetById(int? id = null, bool isAdmin = false);
        Trainee GetByCode(string code = null);

        List<Trainee> GetAll();

        IQueryable<Trainee> GetEmp();
        IQueryable<Trainee> GetEmp(Expression<Func<Trainee, bool>> query);
        IQueryable<Instructor_Ability> GetInstruc_Ability();
        IQueryable<Instructor_Ability> GetInstruc_Ability(Expression<Func<Instructor_Ability, bool>> query);
        IQueryable<Trainee> Get(bool isApi = false);
        IQueryable<Trainee> GetInstructors(Expression<Func<Trainee, bool>> query, bool isApi = false);
        IQueryable<Trainee> GetInstructors(bool isApi = false);
        IQueryable<Trainee> Get(Expression<Func<Trainee, bool>> query, bool isApi = false);
        Trainee Modify(Trainee_Validation model, string password = null);

        List<Trainee> BulkInsertTrainee(List<Trainee_Validation> lstModel, int currentUserId, int count);


        void Update(Trainee entity);
        void Insert(Trainee entity);

        bool InsertImport(Trainee entity);
        Trainee InsertImportCustom(Trainee entity);
        void Update1(TraineeHistory entity);
        void Insert1(TraineeHistory entity);
        void Update2(TraineeFuture entity);
        void Insert2(TraineeFuture entity);
        bool UpdateApi(APIChangePasswordTrainee entity, string currentUser);
        bool UpdateApiUser(APIChangePasswordTrainee entity);
        APIStatusUpdateAPI UpdateApiEmployee(APIChangePasswordFromLMS entity);
        Trainee Modify(InstructorValidation model, string password = null);
        #region Employee Record
        IQueryable<Trainee_Record> GetRecord(Expression<Func<Trainee_Record, bool>> query);
        IQueryable<Trainee_Record> GetRecordByTraineeId(int traineeId);
        Trainee_Record GetRecordId(int id);
        void UpdateRecord(Trainee_Record entity);
        void InsertRecord(Trainee_Record entity);

        #endregion  
        #region Employee Contract

        IQueryable<Trainee_Contract> GetContract(Expression<Func<Trainee_Contract, bool>> query);
        IQueryable<Trainee_Contract> GetContractByTraineeId(int employeeId);
        Trainee_Contract GetContractId(int id);
        void UpdateContract(Trainee_Contract entity);
        void InsertContract(Trainee_Contract entity);
        void DeleteContract(Trainee_Contract entity);

        #endregion
        #region Employee Ability

        Instructor_Ability GetAbility(int id);
        IQueryable<Instructor_Ability> GetAbilityTraineeId(int employeeId);
        IQueryable<Instructor_Ability> GetAbility(Expression<Func<Instructor_Ability, bool>> query);
        IQueryable<Examiner_Ability> GetExaminerAbility(Expression<Func<Examiner_Ability, bool>> query);
        void UpdateAbility(Instructor_Ability entity);
        void InsertAbility(Instructor_Ability entity);
        void DeleteAbility(Instructor_Ability entity);
        void InsertAbilityLog(Instructor_Ability_LOG  entity);
        #endregion
        #region Employee Traineecenter

        IQueryable<Trainee_TrainingCenter> GetTraineeCenterByEmployeeId(int employeeId);
        Trainee_TrainingCenter GetTraineeCenter(int id);
        IQueryable<Trainee_TrainingCenter> GetTraineeCenter(Expression<Func<Trainee_TrainingCenter, bool>> query);
        void UpdateTraineeCenter(Trainee_TrainingCenter entity);
        void InsertTraineeCenter(Trainee_TrainingCenter entity);
        void DeleteTraineeCenter(Trainee_TrainingCenter entity);

        #endregion

        #region Storage

        //List<sp_GetTrainingHeader_Result> sp_GetTrainingHeader(string courseId, string departmentCode,
        //    DateTime? fromDateStart, DateTime? fromDateEnd,
        //    DateTime? toDateStart, DateTime? toDateEnd, string status);
        List<sp_GetTrainingDetail_Result> sp_GetTrainingDetail(string courseId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateEnd, string status);

        #endregion

        #region [-------------------------------- Group Trainee ---------------------------------------]
        GroupTrainee GetGroupById(int? id = null);
        GroupTrainee GetGroupTrainee();
        IQueryable<GroupTrainee> GetAllGroupTrainees(Expression<Func<GroupTrainee, bool>> query);
        void Update(GroupTrainee entity);
        void Insert(GroupTrainee entity);
        GroupTrainee Modify(EmployeeGroupModify model);

        #endregion


        #region [-----------DH DA NANG----------------]

        int InsertEmployee_DHDaNang(APIEmployeeDHDaNang[] model);
        AjaxResponseViewModel InsertContact(APIContact model, string currentUser);
        #endregion
        #region [ -------------------Trainee PDP---------------------------]

        IQueryable<TraineeFuture> GetAllPdp(Expression<Func<TraineeFuture, bool>> query);

        #endregion

        #region [ -------------------Trainee History---------------------------]

        IQueryable<TraineeHistory> GetAllTraineeHistories(Expression<Func<TraineeHistory, bool>> query);

        #endregion

        Trainee_Upload_Files FileGetById(int? id = null);
        void Update(Trainee_Upload_Files entity);

        Trainee Modify(EmployeeModelModify model, FormCollection form, string password = null);
        Trainee ModifyEmployee(int InstructorId, FormCollection form);

        Trainee ModifyExaminerAbility(EmployeeModelModify model);
        Trainee ModifyAllowance(int id, decimal allowance);

        List<sp_GetInstructorReport_TV_Result> GetInstructorReport(string subjectName, string subjectCode,
    string traineeName, string traineeCode, int departmentID, int jobtitleID, string sortDirection, string orderingFunction);
        List<sp_GetListEmployee_TV_Result> GetEmployeeList(string filterCodeOrName);
    }
}
