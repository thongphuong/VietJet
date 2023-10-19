using DAL.Entities;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.Courses;

namespace TMS.Core.Services.Approves
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.UserModels;

    public interface IApproveService : IBaseService
    {
        SelectList GetApproveTypes();
        SelectList GetApproveStatus();

        IQueryable<TMS_APPROVES> GetCourseByType(int? courseId,int courseType,bool isMasterAdmin = false);
        TMS_APPROVES GetById(int? id);

        TMS_APPROVES Get(int? courseId = -1,int? approveType = null, int? eStatus = null,int? courseDetaiId = -1);
        IQueryable<TMS_APPROVES> Get(Expression<Func<TMS_APPROVES,bool>> query, int? approveType = null, int? eStatus = null); 
        void Update(TMS_APPROVES entity);
        void Insert(TMS_APPROVES entity);
        void UpdateTMS_APPROVES_HISTORY(TMS_APPROVES_HISTORY entity);
        void InsertTMS_APPROVES_HISTORY(TMS_APPROVES_HISTORY entity);
        IQueryable<TMS_APPROVES_HISTORY> GetHistoryBy(int? courseId, int type);
        IQueryable<TMS_APPROVES_HISTORY> GetHistoryBy(int? courseId, int type, int status);
        IQueryable<TMS_APPROVES_HISTORY> GetHistory(Expression<Func<TMS_APPROVES_HISTORY, bool>> query);
        void InsertHistory(TMS_APPROVES_HISTORY entity);

        IQueryable<TMS_APPROVES_LOG> GetLogs(int? approveType, int? courseId = -1 , int? courseDetailId = -1);
        void InsertLog(TMS_APPROVES_LOG entity);

        void UpdateApprove(int courseId,int? coursedetailid, UtilConstants.ApproveType approveType,
            UtilConstants.EStatus approveStatus);
        void UpdateApproves(int courseId);
        #region[API Result]
        bool Result(APICourseResultViewModel[] model, string currentUser);


        #endregion

        TMS_APPROVES Modify(bool? isApprove, Course course, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus,UtilConstants.ActionType actionType ,int? courseDetailId = -1, string note = null,string remarkassign = null);



        bool CheckApproval(UtilConstants.ApproveType approveType,UtilConstants.EStatus eStatus, Course course);


        #region [PROCESS TMS APPROVE] 
        TMS_APPROVES GetStepTmsApprove(int type, int courseId);
        bool ProcessStep(int step);
        bool ProcessStepRequirement(int lastStep,int stepNotEdit);
        void ActionTmsProcess(Course course, int processStep,
            UtilConstants.ProcessStepNotEdit processStepNotEdit, UtilConstants.ActionType actionType,
            UtilConstants.EStatus eStatus, int courseDetailId = -1, string cancelRequestContent = null);
        PROCESS_Approver GetApprover(UtilConstants.ApproveType processStep);
        #endregion

        void ApproveFromEmail(int id,int type, string strStatus, string strSendMail, string note = "");
        IQueryable<TMS_APPROVES> Get(Expression<Func<TMS_APPROVES, bool>> query);
    }
}
