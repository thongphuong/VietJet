using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using RestSharp;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Courses;

namespace TMS.Core.Services.Approves
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.UserModels;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Configs;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class ApproveService : BaseService, IApproveService
    {
        private readonly IRepository<TMS_APPROVES> _repoApprove;
        private readonly IRepository<TMS_Approval_Type> _repoApproveType;
        private readonly IRepository<Approve_Status> _repoApproveStatus;
        private readonly IRepository<TMS_APPROVES_LOG> _repoApproveLog;
        private readonly IRepository<TMS_APPROVES_HISTORY> _repoApproveHistory;
        private readonly IRepository<TMS_Course_Member> _repoTMSCourseMember;
        private readonly IRepository<USER> _repoUser;
        private readonly IRepository<Course_Detail> _repoCourseDetail;
        private readonly IRepository<Trainee> _repoTrainee;
        private readonly IRepository<Course_Result> _repoCourse_Result;
        private readonly IRepository<Course_Result_Temp> _repoCourse_Result_Temp;
        private readonly IRepository<Course_Result_Summary> _repoCourseResultSummary;
        private readonly IRepository<TMS_Course_Member> _repoCourseMember;
        private readonly IRepository<Subject_Score> _repoSubjecScore;
        private readonly IRepository<PROCESS_Approver> _repoProcessApprver;
        private readonly ICourseService courseService;
        private readonly IConfigService configService;
        private readonly ICourseDetailService courseDetailService;
        private readonly IEmployeeService employeeService;
        private readonly IUserContext userContext;
        private readonly IRepository<PROCESS_Steps> _repoPROCESS_Steps;
        private readonly IRepository<PROCESS_StepRequirement> _repoPROCESS_StepRequirement;
        private readonly IRepository<PROCESS_Approver> _repoApprover;
        private readonly IRepository<Notification> _repoNotification;
        private readonly IRepository<Course_Result_Course_Detail_Ingredient> _repodetailIngredient;
        private readonly IRepository<Course_Ingredients_Learning> _repoIngredient;

        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public ApproveService(IUnitOfWork uow, IRepository<PROCESS_Approver> repoProcessApprver, IRepository<Subject_Score> repoSubjecScore, IRepository<TMS_Course_Member> repoCourseMember, IRepository<Course_Result_Summary> repoCourseResultSummary, IRepository<Course_Result> repoCourse_Result, IRepository<Trainee> repoTrainee, IRepository<Course_Detail> repoCourseDetail, IRepository<TMS_APPROVES> repoApprove, IRepository<TMS_Approval_Type> repoApproveType, IRepository<Approve_Status> repoApproveStatus, IRepository<TMS_APPROVES_LOG> repoApproveLog, IRepository<TMS_APPROVES_HISTORY> repoApproveHistory, IRepository<TMS_Course_Member> repoTMSCourseMember, IRepository<USER> repoUser, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent, ICourseService _courseService, IConfigService _configService, IRepository<PROCESS_Steps> repoPROCESS_Steps, IRepository<PROCESS_StepRequirement> repoPROCESS_StepRequirement, IRepository<PROCESS_Approver> repoApprover, IRepository<Notification> repoNotification, ICourseDetailService _courseDetailService, IEmployeeService _employeeService, IUserContext _userContext, IRepository<Course_Result_Course_Detail_Ingredient> repodetailIngredient, IRepository<Course_Ingredients_Learning> repoIngredient, IRepository<Course_Result_Temp> repoCourse_Result_Temp) : base(uow, repoCourse, repoSYS_LogEvent)
        {
            _repoCourseResultSummary = repoCourseResultSummary;
            _repoCourse_Result = repoCourse_Result;
            _repoTrainee = repoTrainee;
            _repoCourseDetail = repoCourseDetail;
            _repoApprove = repoApprove;
            _repoApproveType = repoApproveType;
            _repoApproveStatus = repoApproveStatus;
            _repoApproveLog = repoApproveLog;
            _repoApproveHistory = repoApproveHistory;
            _repoTMSCourseMember = repoTMSCourseMember;
            _repoUser = repoUser;
            _repoCourseMember = repoCourseMember;
            _repoSubjecScore = repoSubjecScore;
            _repoProcessApprver = repoProcessApprver;
            courseService = _courseService;
            configService = _configService;
            _repoPROCESS_Steps = repoPROCESS_Steps;
            _repoPROCESS_StepRequirement = repoPROCESS_StepRequirement;
            _repoApprover = repoApprover;
            _repoNotification = repoNotification;
            courseDetailService = _courseDetailService;
            employeeService = _employeeService;
            userContext = _userContext;
            _repodetailIngredient = repodetailIngredient;
            _repoIngredient = repoIngredient;
            _repoCourse_Result_Temp = repoCourse_Result_Temp;
        }

        public SelectList GetApproveTypes()
        {
            return new SelectList(_repoApproveType.GetAll(), "id", "Name");
        }

        public SelectList GetApproveStatus()
        {
            return new SelectList(_repoApproveStatus.GetAll(), "id", "Name");
        }

        public IQueryable<TMS_APPROVES> GetCourseByType(int? courseId, int courseType, bool isMasterAdmin = false)
        {
            return isMasterAdmin
                ? _repoApprove.GetAll(a => a.int_Course_id == courseId && a.int_Type == courseType)
                : _repoApprove.GetAll(a => a.int_Course_id == courseId && a.int_Type == courseType && a.Course.IsActive == true);

        }

        public TMS_APPROVES GetById(int? id)
        {
            return !id.HasValue ? null : _repoApprove.Get(id.Value);
        }

        public TMS_APPROVES Get(int? courseId = -1, int? approveType = null, int? eStatus = null, int? courseDetailId = -1)
        {

            var dataStep = _repoPROCESS_Steps.Get(a => a.Step == approveType && a.IsActive == true);
            var entity = _repoApprove.Get(a =>
            (courseId == -1 || a.int_Course_id == courseId)
            && (approveType == null || a.int_Type == approveType)
            && (eStatus == null || a.int_id_status == eStatus)
            && (courseDetailId == -1 || a.int_courseDetails_Id == courseDetailId));
            entity = dataStep != null ? entity : null;
            return entity;
        }
        public IQueryable<TMS_APPROVES> Get(Expression<Func<TMS_APPROVES, bool>> query, int? approveType = null, int? eStatus = null)
        {
            var entities = _repoApprove.GetAll(query);
            var datastep = _repoPROCESS_Steps.Get(a => a.Step == approveType && a.IsActive == true);
            entities = datastep != null ?
                entities.Where(a => (eStatus == null || a.int_id_status == eStatus) &&
                (approveType == null || a.int_Type == approveType)) : entities;
            return entities;
        }
        public IQueryable<TMS_APPROVES_HISTORY> GetHistory(Expression<Func<TMS_APPROVES_HISTORY, bool>> query)
        {
            var entities = _repoApproveHistory.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void Update(TMS_APPROVES entity)
        {
            _repoApprove.Update(entity);
            Uow.SaveChanges();
        }
        public void Insert(TMS_APPROVES entity)
        {
            _repoApprove.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdateTMS_APPROVES_HISTORY(TMS_APPROVES_HISTORY entity)
        {
            _repoApproveHistory.Update(entity);
            Uow.SaveChanges();
        }
        public void InsertTMS_APPROVES_HISTORY(TMS_APPROVES_HISTORY entity)
        {
            _repoApproveHistory.Insert(entity);
            Uow.SaveChanges();
        }
        public IQueryable<TMS_APPROVES_HISTORY> GetHistoryBy(int? courseId, int type)
        {
            return _repoApproveHistory.GetAll(a => (!courseId.HasValue || a.int_Course_id == courseId) && a.int_Type == type);
        }

        public IQueryable<TMS_APPROVES_HISTORY> GetHistoryBy(int? courseId, int type, int status)
        {
            return _repoApproveHistory.GetAll(a => (!courseId.HasValue || a.int_Course_id == courseId) && a.int_Type == type && a.int_id_status == status);
        }

        public void InsertHistory(TMS_APPROVES_HISTORY entity)
        {
            _repoApproveHistory.Insert(entity);
            Uow.SaveChanges();
        }

        public IQueryable<TMS_APPROVES_LOG> GetLogs(int? approveType, int? courseId = -1, int? courseDetailId = -1)
        {
            var entities =
                _repoApproveLog.GetAll(
                    a =>
                        (courseId == -1 || a.int_course_id == courseId) && a.int_status != null
                        //(courseDetailId == -1 || a.int_course_detail_id == courseDetailId)
                        && a.int_type == approveType);

            return entities;

        }

        public void InsertLog(TMS_APPROVES_LOG entity)
        {
            _repoApproveLog.Insert(entity);
            Uow.SaveChanges();
        }

        public void UpdateApproves(int courseId)
        {
            var approves = _repoApprove.GetAll(a => a.int_Course_id == courseId).ToList();
            if (approves.Any())
            {
                var assignTraineeToEnd = approves.Where(a => a.int_Type != (int)UtilConstants.ApproveType.Course).ToList();
                if (assignTraineeToEnd.Any())
                {
                    foreach (var approve in assignTraineeToEnd)
                    {
                        approve.int_id_status = (int)UtilConstants.EStatus.EmptyTrainee;
                        _repoApprove.Update(approve);
                    }
                }
                Uow.SaveChanges();
            }

        }
        public void UpdateApprove(int courseId, int? coursedetailid, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus)
        {
            var firstOrDefault = _repoUser.Get(a => a.ID == 7);//first user (Administrator)
            var iType = (int)approveType;
            var approve = _repoApprove.Get(a => a.int_Course_id == courseId && a.int_Type == iType && (coursedetailid == null || a.int_courseDetails_Id == coursedetailid));
            if (approve != null)
            {
                approve.int_Course_id = courseId;
                //approve.int_Requested_by = currentUser.USER_ID;
                approve.int_id_status = (int)approveStatus;
                approve.int_Type = iType;
                approve.dtm_requested_date = DateTime.Now;
                approve.int_Approve_by = firstOrDefault?.ID;
                approve.int_courseDetails_Id = coursedetailid;
                _repoApprove.Update(approve);
            }
            else
            {
                // insert TMS_APPROVES
                approve = new TMS_APPROVES
                {
                    int_Course_id = courseId,
                    int_Requested_by = CurrentUser.USER_ID,
                    int_id_status = (int)approveStatus,
                    int_Type = iType,
                    dtm_requested_date = DateTime.Now,
                    int_Seq = 1,
                    int_version = 1,
                    int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a => a.UserRoles.Any(x => x.RoleId == 1))?.ID,
                    int_courseDetails_Id = coursedetailid
                };
                _repoApprove.Insert(approve);
            }
            var checkapprohistory = _repoApproveHistory.GetAll(a => a.int_Course_id == courseId && a.int_Type == iType);
            var approveHistory = new TMS_APPROVES_HISTORY
            {
                approves_id = approve.id,
                int_id_status = (int)approveStatus,
                int_Type = iType,
                int_Course_id = courseId,
                int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a => a.UserRoles.Any(x => x.RoleId == 1))?.ID,
                int_Requested_by = approve.int_Requested_by,
                dtm_requested_date = DateTime.Now,
                int_version = checkapprohistory.Count() + 1
            };
            _repoApproveHistory.Insert(approveHistory);
            if (approveType == UtilConstants.ApproveType.Course && approveStatus == UtilConstants.EStatus.Approve)
            {
                AssignTrainee(courseId);
            }
            //var course = RepoCourse.Get(courseId);
            //switch (approveType)
            //{
            //    case UtilConstants.ApproveType.Course:
            //        UpdateLmsStatus(course,UtilConstants.LMSStatus.Course);
            //        break;
            //    case UtilConstants.ApproveType.AssignedTrainee:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.AssignTrainee);
            //        break;
            //    case UtilConstants.ApproveType.SubjectResult:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.Result);
            //        break;
            //    case UtilConstants.ApproveType.CourseResult:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.Final);
            //        break;
            //}

            Uow.SaveChanges();
        }

        private void AssignTrainee(int courseId)
        {

            var courseDetail = _repoCourseDetail.GetAll(a => a.CourseId == courseId);
            var listMembers = courseDetail.FirstOrDefault()?.TMS_Course_Member.Select(b => b.Member_Id).Distinct();
            foreach (var detail in courseDetail)
            {
                if (detail.IsDeleted == true)
                {
                    var tmsMember = detail.TMS_Course_Member;
                    if (tmsMember.Any())
                    {
                        foreach (var member in tmsMember)
                        {
                            member.IsDelete = true;
                            member.IsActive = false;
                            member.LmsStatus = statusModify;
                            _repoCourseMember.Update(member);
                        }
                    }
                }
                else
                {
                    if (listMembers.Any())
                    {
                        foreach (var member in listMembers)
                        {
                            var model =
                                _repoCourseMember.GetAll(a => a.Member_Id == member && a.Course_Details_Id == detail.Id);
                            if (!model.Any())
                            {
                                var entity = new TMS_Course_Member();
                                entity.Member_Id = member;
                                entity.Course_Details_Id = detail.Id;
                                entity.IsDelete = false;
                                entity.IsActive = true;
                                entity.LmsStatus = statusModify;
                                // RegistTraineeToCourse(member, courseId, currentUser.USER_ID.ToString());
                                _repoCourseMember.Insert(entity);
                            }

                        }
                    }

                }
            }
            Uow.SaveChanges();

        }

        #region[API Result]
        public bool Result(APICourseResultViewModel[] model, string currentUser)
        {
            try
            {
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (model == null || !model.Any()) return false;
                var CourseDetailId = model.FirstOrDefault().CourseDetailId;
                var courseDetail =
                      _repoCourseDetail.Get(
                          a => a.Id == CourseDetailId);
                // check coursedetail
                if (courseDetail != null)
                {
                    var courseDetailId = courseDetail.Id;
                    var subject_score = courseDetail.SubjectDetail.Subject_Score.OrderBy(a => a.point_from);
                    var listFirstResult = model.Where(m => m.Time == 1).ToList();
                    var listReResult = model.Where(m => m.Time == 2).ToList();
                    foreach (var item in listFirstResult)
                    {
                        // check trainee
                        Trainee trainee = _repoTrainee.Get(a => a.str_Staff_Id.ToLower().Trim() == item.TraineeCode.ToLower().Trim() && a.IsDeleted != true);
                        if (trainee == null) continue;
                        var traineeId = trainee.Id;
                        TMS_Course_Member member = _repoCourseMember.Get(a =>
                            a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved) && a.Course_Details_Id == courseDetailId && a.Member_Id == traineeId);
                        //var memberId = trainee.TMS_Course_Member?.FirstOrDefault(a =>
                        //    a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved) && a.Course_Details_Id == courseDetailId && a.Member_Id == traineeId)?.Id;
                        var courseResult = _repoCourse_Result_Temp.Get(
                            a => a.CourseDetailId == courseDetailId && a.TraineeId == traineeId/* && a.times == item.Time*/);

                        var score = !string.IsNullOrEmpty(item.Score) ? double.Parse(Regex.Replace(item.Score, "[.,]", separator)) : -1;

                        if (courseResult == null)
                        {
                            courseResult = new Course_Result_Temp();

                            courseResult.CourseDetailId = item.CourseDetailId;
                            courseResult.times = item.Time;
                            courseResult.TraineeId = traineeId;
                            courseResult.Subject_Id = courseDetail.SubjectDetailId;
                            courseResult.First_Check_Score = Math.Round(score, 1);
                            courseResult.First_Check_Result = GetGrade(item.CourseDetailId, score);
                            courseResult.Re_Check_Score = null;
                            courseResult.Re_Check_Result = null;
                            courseResult.Remark = null;
                            courseResult.Score = null;
                            courseResult.Result = null;
                            var checkReResult = listReResult.Where(l => l.CourseDetailId == courseDetailId && l.TraineeCode.ToLower() == item.TraineeCode.ToLower()).FirstOrDefault();
                            if (checkReResult != null)
                            {
                                courseResult.times = checkReResult.Time;
                                score = !string.IsNullOrEmpty(checkReResult.Score) ? double.Parse(Regex.Replace(checkReResult.Score, "[.,]", separator)) : -1;
                                if (subject_score.Any())
                                {
                                    var score_pass = subject_score.FirstOrDefault();
                                    if (score >= score_pass.point_from)
                                    {
                                        courseResult.Re_Check_Score = score_pass.point_from;
                                        courseResult.Re_Check_Result = "P";
                                        courseResult.Remark = "Second try: " + score;
                                        courseResult.Score = score;
                                    }
                                    else
                                    {
                                        courseResult.Re_Check_Score = Math.Round(score, 1);
                                        courseResult.Re_Check_Result = GetGrade(item.CourseDetailId, score);
                                        courseResult.Score = Math.Round(score, 1);
                                    }
                                }
                                else
                                {
                                    courseResult.Re_Check_Score = Math.Round(score, 1);
                                    courseResult.Re_Check_Result = GetGrade(item.CourseDetailId, score);
                                    courseResult.Score = Math.Round(score, 1);
                                }

                            }

                            //courseResult.Remark = item.Remark,
                            courseResult.CreatedBy = currentUser;
                            courseResult.CreatedAt = DateTime.Now;
                            courseResult.inCourseMemberId = member?.Id;
                            //entity2.Result = itemresult.result;
                            _repoCourse_Result_Temp.Insert(courseResult);
                        }
                        else
                        {
                            courseResult.CourseDetailId = item.CourseDetailId;
                            courseResult.TraineeId = traineeId;
                            //courseResult.Remark = item.Remark;
                            courseResult.ModifiedBy = currentUser;
                            courseResult.ModifiedAt = DateTime.Now;
                            courseResult.inCourseMemberId = member?.Id;
                            courseResult.times = item.Time;
                            courseResult.Subject_Id = courseDetail.SubjectDetailId;
                            courseResult.First_Check_Score = Math.Round(score, 1);
                            courseResult.First_Check_Result = GetGrade(item.CourseDetailId, Math.Round(score, 1));
                            courseResult.Re_Check_Score = null;
                            courseResult.Re_Check_Result = null;
                            courseResult.Remark = null;
                            courseResult.Score = null;
                            courseResult.Result = null;
                            var checkReResult = listReResult.Where(l => l.CourseDetailId == courseDetailId && l.TraineeCode.ToLower() == item.TraineeCode.ToLower()).FirstOrDefault();
                            if (checkReResult != null)
                            {
                                courseResult.times = checkReResult.Time;
                                score = !string.IsNullOrEmpty(checkReResult.Score) ? double.Parse(Regex.Replace(checkReResult.Score, "[.,]", separator)) : -1;
                                if (subject_score.Any())
                                {
                                    var score_pass = subject_score.FirstOrDefault();
                                    if (score >= score_pass.point_from)
                                    {
                                        courseResult.Re_Check_Score = score_pass.point_from;
                                        courseResult.Re_Check_Result = "P";
                                        if (!string.IsNullOrEmpty(courseResult.Remark))
                                        {
                                            if (!courseResult.Remark.Contains("Second try:"))
                                            {
                                                courseResult.Remark = string.Format("{0}. {1}",
                                           "Second try: " + score, courseResult.Remark);
                                            }
                                            else
                                            {
                                                courseResult.Remark = "Second try: " + score;
                                            }

                                        }
                                        else
                                        {
                                            courseResult.Remark = "Second try: " + score;
                                        }
                                        courseResult.Score = score;
                                    }
                                    else
                                    {
                                        courseResult.Re_Check_Score = Math.Round(score, 1);
                                        courseResult.Re_Check_Result = GetGrade(item.CourseDetailId, score);
                                        courseResult.Score = Math.Round(score, 1);
                                    }
                                }
                                else
                                {
                                    courseResult.Re_Check_Score = Math.Round(score, 1);
                                    courseResult.Re_Check_Result = GetGrade(item.CourseDetailId, score);
                                    courseResult.Score = Math.Round(score, 1);
                                }
                            }


                            _repoCourse_Result_Temp.Update(courseResult);
                        }

                        #region [Update Status Approve]
                        //UpdateApproveAPI(courseDetail.CourseId, courseDetailId, UtilConstants.ApproveType.SubjectResult, UtilConstants.EStatus.Approve);
                        #endregion
                        // InsertOrUpdateStatusApi(courseId, UtilConstants.ApiStatus.Modify, UtilConstants.LMSStatus.Result);
                    }
                    Uow.SaveChanges();
                }
               


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        protected void UpdateApproveAPI(int courseId, int? coursedetailid, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus)
        {
            var firstOrDefault = _repoUser.Get(a => a.ID == 7);//first user (Administrator)
            var iType = (int)approveType;
            var approve = _repoApprove.Get(a => a.int_Course_id == courseId && a.int_Type == iType && (coursedetailid == null || a.int_courseDetails_Id == coursedetailid));
            if (approve != null)
            {
                approve.int_Course_id = courseId;
                //approve.int_Requested_by = currentUser.USER_ID;
                approve.int_id_status = (int)approveStatus;
                approve.int_Type = iType;
                approve.dtm_requested_date = DateTime.Now;
                approve.int_courseDetails_Id = coursedetailid;
                _repoApprove.Update(approve);
            }
            else
            {

                // insert TMS_APPROVES
                approve = new TMS_APPROVES
                {

                    int_Course_id = courseId,
                    int_id_status = (int)approveStatus,
                    int_Type = iType,
                    dtm_requested_date = DateTime.Now,
                    int_Seq = 1,
                    int_version = 1,
                    int_courseDetails_Id = coursedetailid
                };
                _repoApprove.Insert(approve);
            }
            var checkapprohistory = _repoApproveHistory.GetAll(a => a.int_Course_id == courseId && a.int_Type == iType);
            var approveHistory = new TMS_APPROVES_HISTORY
            {
                approves_id = approve.id,
                int_id_status = (int)approveStatus,
                int_Type = iType,
                int_Course_id = courseId,
                int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a => a.UserRoles.Any(x => x.RoleId == 1))?.ID,
                int_Requested_by = approve.int_Requested_by,
                dtm_requested_date = DateTime.Now,
                int_version = checkapprohistory.Count() + 1
            };
            _repoApproveHistory.Insert(approveHistory);
            //var course = RepoCourse.Get(courseId);
            //switch (approveType)
            //{
            //    case UtilConstants.ApproveType.Course:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.Course);
            //        break;
            //    case UtilConstants.ApproveType.AssignedTrainee:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.AssignTrainee);
            //        break;
            //    case UtilConstants.ApproveType.SubjectResult:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.Result);
            //        break;
            //    case UtilConstants.ApproveType.CourseResult:
            //        UpdateLmsStatus(course, UtilConstants.LMSStatus.Final);
            //        break;
            //}

            Uow.SaveChanges();
        }
        private string GetGrade(int courseDetailId, double score = -1)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            var courseDetail = _repoCourseDetail.Get(courseDetailId);
            var subjectDetailId = courseDetail.SubjectDetailId;
            //var getsubjectDetails = _repoSubjectDetail.GetAll(a => a.Id == subjectDetailId).Select(a => a.Id);
            if (subjectDetailId != null)
            {
                var getsubjectScore =
                    _repoSubjecScore.GetAll(a => subjectDetailId == a.subject_id)
                        .OrderByDescending(a => a.point_from);
                if (getsubjectScore.Any())
                {
                    foreach (var item in getsubjectScore)
                    {
                        if (score >= item.point_from)
                        {
                            result = item.grade;
                            break;
                        }

                    }

                }
            }
            var _return = result.Contains(UtilConstants.Grade.Fail.ToString())
                ? "F"
                : "P";
            return _return;
        }
        #endregion

        private string GetCulture()
        {
            var culture = "en";
            var cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            if (cultureCookie != null)
            {
                culture = cultureCookie.Value;
            }

            return culture;
        }

        public TMS_APPROVES Modify(bool? isApprove, Course course, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus, UtilConstants.ActionType actionType, int? courseDetailId = -1, string note = null, string remarkassign = null)
        {
            var now = DateTime.Now;
            var step = (int)approveType;
            var approve =
               _repoApprove.Get(
                   a =>
                       a.int_Type == step && a.int_Course_id == course.Id
                       && (courseDetailId == -1 || a.int_courseDetails_Id == courseDetailId));
            // var approver = _repoProcessApprver.Get(a => a.Step == step && a.isActive==true && a.IsDeteled==false);
            var firstOrDefault = _repoUser.Get(a => a.IsDeleted != true && a.ISACTIVE == 1 && a.UserRoles.Any(x => x.ROLE.NAME == "HOD" && x.ROLE.IsActive == true));
            if (approve != null)
            {
                approve.Course = course;

                if (actionType == UtilConstants.ActionType.Request)
                {
                    approve.int_Requested_by = CurrentUser.USER_ID;
                    approve.Date_Requested = now;
                    approve.str_Remark = remarkassign;
                }
                approve.int_id_status = (int)approveStatus;
                approve.int_Type = step;
                approve.dtm_requested_date = now;

                if (actionType == UtilConstants.ActionType.Approve)
                {
                    //approve.int_Approve_by = approver != null ? approver.UserId : CurrentUser.USER_ID;
                    approve.int_Approve_by = CurrentUser?.USER_ID;
                }

                //ngày approve (thêm mới vào 23/03/2018)
                approve.Date_Approved = now;
                //end


                if (courseDetailId != -1)
                {
                    approve.int_courseDetails_Id = courseDetailId;
                }

                //chỉnh sửa nội dung thì cập nhật to LMS
                if (step == (int)UtilConstants.ApproveType.Course && (approveStatus != UtilConstants.EStatus.Pending /*|| approveStatus != UtilConstants.EStatus.Reject*/))
                {
                    approve.Course.LMSStatus = (int)UtilConstants.ApiStatus.Modify;
                    //RepoCourse.Update(approve.Course);
                }

                _repoApprove.Update(approve);
            }
            else
            {
                // insert TMS_APPROVES
                approve = new TMS_APPROVES
                {
                    int_Requested_by = CurrentUser?.USER_ID,
                    int_id_status = (int)approveStatus,
                    int_Type = step,
                    dtm_requested_date = now,
                    Date_Requested = now,
                    //int_Approve_by = approver != null ? approver.UserId : userModel.USER_ID,
                    Course = course
                };

                if (courseDetailId != -1)
                {
                    approve.int_courseDetails_Id = courseDetailId;
                }
                //chỉnh sửa nội dung thì cập nhật to LMS
                if (step == (int)UtilConstants.ApproveType.Course && (approveStatus != UtilConstants.EStatus.Pending /*|| approveStatus != UtilConstants.EStatus.Reject*/))
                {
                    approve.Course.LMSStatus = (int)UtilConstants.ApiStatus.Modify;
                    //RepoCourse.Update(approve.Course);
                }
                approve.str_Remark = remarkassign;
                _repoApprove.Insert(approve);
            }
            if (step == (int)UtilConstants.ApproveType.AssignedTrainee &&
                approveStatus == UtilConstants.EStatus.Approve)
            {
                var courseDetails = course.Course_Detail.Where(a => a.IsDeleted != true && a.IsActive == true);
                var listID = courseDetails.Select(a => a.Id);


                var abc = _repoTMSCourseMember.GetAll(a => a.IsDelete != true && listID.Contains((int)a.Course_Details_Id) && !a.Approve_Id.HasValue).ToList();
                foreach (var member in abc)
                {
                    member.Approve_Id = approve.id;
                }
            }
            Uow.SaveChanges();
            if (isApprove != true)
            {
                var approveHistory = new TMS_APPROVES_HISTORY
                {

                    approves_id = approve.id,
                    int_id_status = (int)approveStatus,
                    int_Type = step,
                    int_Course_id = course.Id,
                    int_Approve_by = firstOrDefault?.ID ?? CurrentUser?.USER_ID, //approver != null ? approver.UserId : CurrentUser.USER_ID,
                    int_Requested_by = approve.int_Requested_by,
                    dtm_requested_date = now,

                };
                _repoApproveHistory.Insert(approveHistory);


                var str_log = Log(step, course, courseDetailId, note);
                var TMS_APPROVES_LOG_ = new TMS_APPROVES_LOG();
                TMS_APPROVES_LOG_.str_content = str_log;
                TMS_APPROVES_LOG_.dtm_create = DateTime.Now;
                TMS_APPROVES_LOG_.int_course_id = course.Id;
                TMS_APPROVES_LOG_.int_type = step;
                TMS_APPROVES_LOG_.int_status = (int)approveStatus;
                TMS_APPROVES_LOG_.int_course_detail_id = courseDetailId != -1 ? courseDetailId : null;
                TMS_APPROVES_LOG_.dtm_approved_request = DateTime.Now;
                _repoApproveLog.Insert(TMS_APPROVES_LOG_);
            }
            else
            {
                if((int)approveStatus == (int)UtilConstants.EStatus.Block)
                {
                    var approveHistory = new TMS_APPROVES_HISTORY
                    {

                        approves_id = approve.id,
                        int_id_status = (int)approveStatus,
                        int_Type = step,
                        int_Course_id = course.Id,
                        int_Approve_by = CurrentUser?.USER_ID, //approver != null ? approver.UserId : CurrentUser.USER_ID,
                        int_Requested_by = approve.int_Requested_by,
                        dtm_requested_date = now,

                    };
                    _repoApproveHistory.Insert(approveHistory);
                }
                else
                {
                    var approveHis = _repoApproveHistory.GetAll(a => a.int_Course_id == course.Id && a.int_Type == step && a.approves_id == approve.id).OrderByDescending(a => a.id).FirstOrDefault();
                    approveHis.dtm_requested_date = DateTime.Now;
                    approveHis.int_id_status = (int)approveStatus;
                    approveHis.int_Approve_by = CurrentUser?.USER_ID;
                    approveHis.int_courseDetails_Id = courseDetailId != -1 ? courseDetailId : null;
                    //approveHis.int_Approve_by = CurrentUser?.USER_ID ?? approver.UserId;
                    _repoApproveHistory.Update(approveHis);
                }
                


                var approveLog = _repoApproveLog.GetAll(a => a.int_course_id == course.Id && a.int_type == step).OrderByDescending(a => a.id).FirstOrDefault();
                approveLog.int_status = (int)approveStatus;
                approveLog.dtm_approved_request = DateTime.Now;
                _repoApproveLog.Update(approveLog);
            }


            Uow.SaveChanges();
            return approve;
        }

        //typelog = 1 = course
        //typelog = 2 = assigntrainee
        //typelog = 3 = subject result
        //typelog = 4 = course result
        private string Log(int? typelog, Course couse, int? courseDetailId = -1, string cancelRequestContent = null)
        {
            StringBuilder Html_ = new StringBuilder();
            var culture = GetCulture();
            #region[log course]
            if (typelog == 1 && couse != null)
            {
                if (couse != null)
                {
                    Html_.AppendFormat("<b>Course name:</b> {0}<br />", couse?.Name);
                    Html_.AppendFormat("<b>Course code:</b> {0}<br />", couse?.Code);
                    Html_.AppendFormat("<b>Venue:</b> {0}<br />", couse?.Venue);
                    Html_.AppendFormat("<b>No-trainee:</b> {0}<br />", couse?.NumberOfTrainee);
                    var cousetrainingcenter = courseService.GetTrainingCenters(a => a.course_id == couse.Id);
                    if (cousetrainingcenter.Any())
                    {
                        Html_.Append("<b>Training center:</b> ");
                        foreach (var item in cousetrainingcenter)
                        {
                            Html_.AppendFormat("- {0}.<br />", item?.Department?.Name);
                        }
                    }
                    Html_.AppendFormat("<b>Partner:</b> {0}<br />", couse?.Company?.str_Name);
                    Html_.AppendFormat("<b>Survey:</b> {0}<br />", couse?.Survey);
                    Html_.AppendFormat("<b>From-To:</b> {0} - {1}<br />", DateUtil.DateToString(couse?.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(couse?.EndDate, "dd/MM/yyyy"));
                    Html_.AppendFormat("<b>Type:</b> {0}<br />", couse?.TypeResult);
                    Html_.AppendFormat("<b>Note:</b> {0}<br />", couse?.Note);
                    var cousecoursedetail = _repoCourseDetail.GetAll(a => a.CourseId == couse.Id && a.IsDeleted == false);
                    if (cousecoursedetail.Any())
                    {
                        Html_.Append("--------------------------------------------------------------------<br />");

                        foreach (var item in cousecoursedetail)
                        {

                            string instructorName = string.Join(",", item.Course_Detail_Instructor.Select(a => ReturnDisplayLanguage(a?.Trainee?.FirstName, a?.Trainee?.LastName, culture)));
                            // string instructorName = string.Join(",", item.Course_Detail_Instructor.Select(a => a?.Trainee?.FirstName + " " + a?.Trainee?.LastName));

                            Html_.AppendFormat("<b>Subject:</b> {0}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Type learning: {1}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Instructor: {2}<br />", item?.SubjectDetail?.Name, item?.type_leaning, instructorName);
                            Html_.AppendFormat("<b>Date From-To:</b> {0} - {1}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Time From-To: {2} - {3}<br />", DateUtil.DateToString(item?.dtm_time_from, "dd/MM/yyyy"), DateUtil.DateToString(item?.dtm_time_to, "dd/MM/yyyy"), (item?.time_from != null ? (item?.time_from?.ToString().Substring(0, 2) + ":" + item?.time_from?.ToString().Substring(2)) : ""), (item?.time_to != null ? (item?.time_to?.ToString().Substring(0, 2) + ":" + item?.time_to?.ToString().Substring(2)) : ""));
                            Html_.AppendFormat("<b>Room:</b> {0}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Duration: {1}<br />", item?.Room?.str_Name, item?.SubjectDetail?.Duration);
                            Html_.Append("--------------------------------------------------------------------<br />");
                        }
                    }

                }
            }
            #endregion
            #region[log assigntrainee]
            else if (typelog == 2 && couse != null)
            {
                // get full mon
                var fullcourse = _repoTrainee.GetAll(a => a.Course_Result_Final.Any(x => x.IsDeleted == false && x.courseid == couse.Id) && a.IsDeleted == false && a.IsActive == true);
                if (fullcourse.Any())
                {
                    Html_.Append("<b>Learning full:</b><br />");
                    int count = 0;
                    foreach (var item in fullcourse)
                    {
                        count++;
                        //Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item?.str_Staff_Id,  item?.FirstName + " " + item?.LastName);
                        Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item?.str_Staff_Id, ReturnDisplayLanguage(item?.FirstName, item?.LastName, culture)
);
                    }
                }
                // get tung mon
                var courseDetails = _repoCourseDetail.GetAll(a => a.CourseId == couse.Id && a.IsDeleted == false && a.IsActive == true && a.Course.IsDeleted == false && a.Course.IsActive == true);
                if (courseDetails.Any())
                {
                    foreach (var item in courseDetails)
                    {
                        Html_.AppendFormat("<b>{0}</b><br />", item?.SubjectDetail?.Name);

                        if (item.TMS_Course_Member.Any())
                        {
                            int count = 0;
                            foreach (var member in item.TMS_Course_Member.Where(a => a.IsActive == true && a.IsDelete == false))
                            {
                                count++;
                                //Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, member?.Trainee?.str_Staff_Id, member?.Trainee?.FirstName + " " + member?.Trainee?.LastName);
                                Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, member?.Trainee?.str_Staff_Id, ReturnDisplayLanguage(member?.Trainee?.FirstName, member?.Trainee?.LastName, culture));
                            }
                        }
                        #region [----------ẩn -------------]

                        //var datacoursemenber = _repoCourseMember.GetAll(a => a.Course_Details_Id == item.Id);
                        //if (datacoursemenber.Any())
                        //{
                        //    int count = 0;
                        //    foreach (var item_ in datacoursemenber)
                        //    {
                        //        count++;
                        //        Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item_?.Trainee?.str_Staff_Id, item_?.Trainee?.FirstName + " " + item_?.Trainee?.LastName);
                        //    }
                        //}
                        #endregion
                    }
                }
            }
            #endregion
            #region[log subject result]
            else if (typelog == 3 && courseDetailId != -1)
            {
                var entity = _repoCourseResultSummary.GetAll(a => a.CourseDetailId == courseDetailId);
                if (entity.Any())
                {
                    var count = 0;
                    foreach (var item in entity)
                    {
                        count++;
                        //Html_.AppendFormat("<b>" + count + "&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", item?.Trainee?.FirstName.Trim() + " " + item?.Trainee?.LastName.Trim(), (item?.point == -1 ? "" : item?.point.ToString()), item?.Result);
                        Html_.AppendFormat("<b>" + count + "&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee.LastName), (item?.point == -1 ? "" : item?.point.ToString()), item?.Result);
                    }
                }
            }
            #endregion
            #region[log course result]
            else if (typelog == 4 && couse != null)
            {
                var data = courseService.GetCourseResultFinal(a => a.courseid == couse.Id && a.IsDeleted == false).OrderByDescending(a => a.id);

                if (data.Any())
                {
                    var count = 0;
                    foreach (var item in data)
                    {
                        var strGrade = "";
                        switch (item?.grade)
                        {
                            case (int)UtilConstants.Grade.Fail:
                                strGrade = "Fail";
                                break;
                            case (int)UtilConstants.Grade.Pass:
                                strGrade = "Pass";
                                break;
                            case (int)UtilConstants.Grade.Distinction:
                                strGrade = "Distinction";
                                break;
                        }
                        count++;

                        //Html_.AppendFormat("<b>" + count + "&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", item?.Trainee?.FirstName + " " + item?.Trainee?.LastName, item?.point, strGrade);
                        Html_.AppendFormat("<b>" + count + "&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee.LastName), item?.point, strGrade);
                    }
                }
            }
            #endregion
            Html_.AppendFormat("<b>by: <em>{0}</em></b>", CurrentUser?.FirstName + " " + CurrentUser?.LastName);
            if (!string.IsNullOrEmpty(cancelRequestContent))
            {
                Html_.AppendFormat("<br /><b>Content : <em>{0}</em></b>", cancelRequestContent);
            }
            return Html_.ToString();
        }

        public bool CheckApproval(UtilConstants.ApproveType approveType, UtilConstants.EStatus eStatus, Course course)
        {
            var return_ = false;
            var approvaltype = (int)approveType;
            var eStatu = (int)eStatus;
            var datastep = _repoPROCESS_Steps.Get(a => a.Step == approvaltype && a.IsActive == true);
            if (datastep != null)
            {
                var getallcourse = _repoApprove
                   .Get(a => a.int_id_status == approvaltype && a.int_Type == eStatu && a.int_Course_id == course.Id);
                if (getallcourse != null)
                    return_ = true;
            }
            return return_;
        }

        private string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            culture = "en";
            var cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            if (cultureCookie != null)
            {
                //culture = GetByKey("DisplayLanguage");
                culture = cultureCookie.Value;
            }
            switch (culture)
            {
                case "vi":
                    fullName = firstName + " " + lastName;
                    break;
                default:
                    fullName = lastName + " " + firstName;
                    break;
            }
            return fullName;
        }

        #region [PROCESS TMS APPROVE]
        public TMS_APPROVES GetStepTmsApprove(int type, int courseId)
        {
            return _repoApprove.Get(a => a.int_Type == type && a.int_Course_id == courseId);
        }

        public IQueryable<PROCESS_StepRequirement> GetProcessStepRequirement()
        {
            return _repoPROCESS_StepRequirement.GetAll();
        }
        public bool ProcessStep(int step)
        {
            var processStep = _repoPROCESS_Steps.Get(a => a.IsActive == true && a.Step == step);

            return processStep != null;
        }

        public bool ProcessStepRequirement(int lastStep, int stepNotEdit)
        {
            var processStep = _repoPROCESS_StepRequirement.Get(a => a.StepHere == lastStep && a.StepNotEdit == stepNotEdit);
            return processStep != null;
        }
        public void ActionTmsProcess(Course course, int processStep, UtilConstants.ProcessStepNotEdit processStepNotEdit, UtilConstants.ActionType actionType, UtilConstants.EStatus eStatus, int courseDetailId = -1, string cancelRequestContent = null)
        {
            // check process Step
            var step = (int)processStep;
            var stepNotEdit = (int)processStepNotEdit;
            var checkStep = ProcessStep(step);
            if (checkStep) return;
            var status = (int)eStatus;
            var approve = GetStepTmsApprove(step, course.Id);
            if (approve == null)
            {
                approve = new TMS_APPROVES();
                approve.int_id_status = status;
                approve.int_Course_id = course.Id;
                approve.int_Type = step;
                approve.int_Requested_by = CurrentUser.USER_ID;
                approve.dtm_requested_date = DateTime.Now;
                if (courseDetailId != -1)
                {
                    approve.int_courseDetails_Id = courseDetailId;
                }
                Insert(approve);
            }
            else
            {
                approve.int_id_status = status;
                approve.int_Approve_by = CurrentUser.USER_ID;
                Update(approve);
            }
            var approveHistory = new TMS_APPROVES_HISTORY
            {

                approves_id = approve.id,
                int_id_status = status,
                int_Type = step,
                int_Course_id = course.Id,
                int_Approve_by = approve.int_Approve_by,
                int_Requested_by = approve.int_Requested_by,
                dtm_requested_date = DateTime.Now,
            };
            InsertTMS_APPROVES_HISTORY(approveHistory);

            var strLog = Log(step, course, courseDetailId, cancelRequestContent);
            var tmsApprovesLog = new TMS_APPROVES_LOG();
            tmsApprovesLog.str_content = strLog;
            tmsApprovesLog.dtm_create = DateTime.Now;
            tmsApprovesLog.int_course_id = course.Id;
            tmsApprovesLog.int_type = step;
            tmsApprovesLog.int_status = status;
            _repoApproveLog.Insert(tmsApprovesLog);
        }


        public PROCESS_Approver GetApprover(UtilConstants.ApproveType processStep)
        {
            var step = (int)processStep;
            return _repoApprover.Get(a => a.IsDeteled == false && a.isActive == true && a.Step == step);
        }
        #endregion

        #region [APPROVE thông qua mail]

        public void ApproveFromEmail(int id, int type, string strStatus, string strSendMail, string note = "")
        {
            switch (type)
            {
                case (int)UtilConstants.ApproveType.Course:
                    ApproveCourse(id, strStatus, strSendMail, note);
                    break;
                case (int)UtilConstants.ApproveType.AssignedTrainee:
                    ApproveAssignTrainee(id, strStatus, strSendMail, note);
                    break;
                case (int)UtilConstants.ApproveType.SubjectResult:
                    ApproveSubjectResult(id, strStatus, strSendMail, note);
                    break;
                case (int)UtilConstants.ApproveType.CourseResult:
                    ApproveCourseResult(id, strStatus, strSendMail, note);
                    break;
                default:
                    break;
            }
        }
        private void ApproveCourse(int id, string strStatus, string strSendMail, string note = "")
        {
            var aModel = _repoApprove.Get(id);
            var course = aModel.Course;
            var updateProgam = UpdateStatus(course);
            var status = strStatus.ToLower().Equals("y") ? (int)UtilConstants.EStatus.Approve : (int)UtilConstants.EStatus.Reject;
            var lblStatus = strStatus == "y"
                   ? Resource.lblApproved
                   : Resource.lblReject;
            var checkSendEmail = strSendMail.ToLower().Equals("y") ? true : false;
            if (!updateProgam)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/Course", App_GlobalResources.Messege.ERROR_UPDATE_STATUS_UNSUCCESS);
            }
            Modify_TMS(true, course, (int)UtilConstants.ApproveType.Course, status, UtilConstants.ActionType.Approve, note);
            if (status == (int)UtilConstants.EStatus.Approve)
            {
                #region [------------------------------------Insert Member with new course----------------------------------------]
                UpdateMember(course);
                #endregion

                #region [-----------------------Sent Mail Approve Course-----------------------------]

                var checkSchedule = configService.GetScheduleByKey((int)UtilConstants.KeySend.ApproveCourse);

                //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");// CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);
                var usercheck = _repoCourseDetail.GetAll(a => a.CourseId == course.Id && a.IsDeleted != true);
                if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [ ----- sent mail cho gv & GV mantor----- ]
                    if (checkSchedule != null)
                    {
                        if (checkSendEmail)
                        {
                            if (usercheck.Any())
                            {
                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = courseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id).ToList();
                                    if (instructor_coursedetail.Any())
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = employeeService.GetById(details.Instructor_Id);
                                            if (instructor != null)
                                            {
                                                Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    }
                    #endregion
                }
                if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [ ----- sent mail cho gv & GV mantor----- ]
                    if (checkSchedule != null)
                    {
                        if (usercheck.Any())
                        {
                            foreach (var item in usercheck)
                            {
                                var instructor_coursedetail = courseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id).ToList();
                                if (instructor_coursedetail.Any())
                                {
                                    foreach (var details in instructor_coursedetail)
                                    {
                                        var instructor = employeeService.GetById(details.Instructor_Id);
                                        if (instructor != null)
                                        {
                                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                                        }
                                    }
                                }
                            }
                        }

                        Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    }
                    #endregion
                }
                if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Reject)
                {
                    var checkScheduleReject = configService.GetScheduleByKey((int)UtilConstants.KeySend.RejectApproveCourse);

                    #region [ ----- Reject sent mail cho gv & GV mantor ----- ]

                    if (checkScheduleReject != null)
                    {

                        if (usercheck.Any())
                        {
                            foreach (var item in usercheck)
                            {
                                var instructor_coursedetail = courseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                                if (instructor_coursedetail.Any())
                                {
                                    foreach (var details in instructor_coursedetail)
                                    {
                                        var instructor = employeeService.GetById(details.Instructor_Id);
                                        {
                                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.Reject);
                                        }
                                    }
                                }
                            }
                        }

                        Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.Reject);
                    }
                    #endregion
                }
                #endregion

                #region [--------CALL LMS (CRON PROGRAM)----------]
                var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                if (!callLms)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/Course", string.Format(Messege.SUBMIT_SUCCESS, lblStatus) + " " + Messege.ERROR_CALL_LMS);
                }
                else
                {
                    CallServices(UtilConstants.CRON_COURSE);
                }
                #endregion
            }
        }
        private void ApproveAssignTrainee(int id, string strStatus, string strSendMail, string note = "")
        {
            try
            {
                var approve = _repoApprove.Get(id);
                if (approve == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", App_GlobalResources.Messege.NO_DATA);
                    return;
                }
                var status = strStatus.ToLower().Equals("y") ? (int)UtilConstants.EStatus.Approve : (int)UtilConstants.EStatus.Reject;
                var checkSendEmail = strSendMail.ToLower().Equals("y") ? true : false;

                var lblStatus = strStatus.ToLower().Equals("y") ? Resource.lblApproved : Resource.lblReject;

                //var final = approve.Course.Course_Result_Final.ToList();
                //if (final.Any())
                //{
                //    foreach (var item in final)
                //    {
                //        item.MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Marked;
                //    }
                //}
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.AssignedTrainee, status, UtilConstants.ActionType.Approve, note);

                if (checkSendEmail)
                {
                    #region [---------Sent mail Assign trainee-------]

                    var checkApproveAssignTrainee =
                        configService.GetScheduleByKey((int)UtilConstants.KeySend.ApproveAssignTrainee);

                    // var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);
                    if (checkApproveAssignTrainee != null)
                    {
                        if (approve.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            var member = courseService.GetCourseResultFinal(a => a.courseid == approve.Course.Id && a.IsDeleted == false && a.MemberStatus == (int)UtilConstants.CourseResultFinalStatus.Pending);
                            if (member != null)
                            {
                                foreach (var item in member)
                                {
                                    //check LMS Assign = UtilConstants.APIAssign.Approved
                                    if (item.Trainee.TMS_Course_Member.Any(a => a.Status == (int)UtilConstants.APIAssign.Approved))
                                    {
                                        Sent_Email_TMS(null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee, true);
                                    }
                                    else if (item.Trainee.TMS_Course_Member.Any(a => a.Status == null))
                                    {
                                        Sent_Email_TMS(null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee);
                                    }
                                }
                            }
                        }

                    }

                    #endregion
                }

                #region [--------CALL LMS (CRON ASSIGN TRAINEE)----------]
                var callLms = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                if (!callLms)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", string.Format(Messege.SUBMIT_SUCCESS, lblStatus) + " " + Messege.ERROR_CALL_LMS);
                }
                #endregion

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", ex.Message);
            }
        }

        private void ApproveSubjectResult(int id, string strStatus, string strSendMail, string note = "")
        {
            var status = strStatus.ToLower().Equals("y")
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
            try
            {
                var approve = _repoApprove.Get(id);
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.SubjectResult, status, UtilConstants.ActionType.Approve, note, approve.int_courseDetails_Id);
                if (status == (int)UtilConstants.EStatus.Approve)
                {

                    var groupCertificate = courseService.GetAllGroupCertificate(a => a.IsDeleted == false && a.IsActive == true).ToArray();
                    if (groupCertificate.Any())
                    {
                        foreach (var group in groupCertificate)
                        {
                            group.Status = (int)UtilConstants.ApiStatus.Modify;
                            courseService.UpdateGroupCertificate(group);
                        }
                    }
                    //approve.Course_Detail.Course_Result.ForEach(a => a.LmsStatus = StatusModify);
                    //_repoApprove.Update(approve);


                    //#region [--------CALL LMS (CRON PROGRAM)----------]
                    //var callLms = CallServices(UtilConstants.CRON_PROGRAM"));
                    //if (!callLms)
                    //{
                    //    return Json(new AjaxResponseViewModel()
                    //    {
                    //        message =
                    //            string.Format(Messege.SUBMIT_SUCCESS, status) + "<br />" +
                    //            "but an error occurred while call LMS updating in process !",
                    //        result = false
                    //    }, JsonRequestBehavior.AllowGet);
                    //}
                    //#endregion
                    #region [--------CALL LMS (CronGet Course ResultSummary)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SubjectResult", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SubjectResult", ex.Message);
            }

        }
        private void ApproveCourseResult(int id, string strStatus, string strSendMail, string note = "")
        {
            try
            {
                var aModel = _repoApprove.Get(id);
                var status = strStatus.ToLower().Equals("y")
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = strStatus.ToLower().Equals("y")
                   ? Resource.lblApproved
                   : Resource.lblReject;
                var checkSendEmail = strSendMail.ToLower().Equals("y") ? true : false;
                var courseId = aModel.Course.Id;
                #region [---------Approve/Reject CourseFinal-------]
                Modify_TMS(true, aModel.Course, (int)UtilConstants.ApproveType.CourseResult, status, UtilConstants.ActionType.Approve, note);
                #endregion

                if (status == (int)UtilConstants.EStatus.Approve)
                {

                    var groupCertificate = courseService.GetAllGroupCertificate(a => a.IsDeleted == false && a.IsActive == true).ToArray();
                    if (groupCertificate.Any())
                    {
                        foreach (var group in groupCertificate)
                        {
                            group.Status = (int)UtilConstants.ApiStatus.Modify;
                            courseService.UpdateGroupCertificate(group);
                        }
                    }


                    #region [-------------Modify Course Result Sumary--------------]
                    var courseDetailIds = aModel.Course.Course_Detail.Select(a => a.Id).ToList();
                    var courseResultSummary =
                        courseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value)).ToList();
                    if (courseResultSummary.Any())
                    {
                        foreach (var summary in courseResultSummary)
                        {
                            summary.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            courseService.Update(summary);
                        }
                    }
                    #endregion

                    #region [--------CALL LMS (CronGet Course ResultSummary)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/CourseResult", string.Format(Messege.SUBMIT_SUCCESS, lblStatus) + " " + Messege.ERROR_CALL_LMS);

                    }
                    #endregion

                }


                #region [---------Sent mail trainee member course final -------]
                var checkApproveCourseResult =
                       configService.GetScheduleByKey((int)UtilConstants.KeySend.ApproveCourseResult);
                //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);
                if (aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                {

                    if (checkApproveCourseResult != null)
                    {
                        if (checkSendEmail)
                        {
                            #region [--------sent mail GV & GV mantor---------]
                            var usercheck = courseDetailService.GetByCourse(courseId);
                            if (usercheck.Any())
                            {
                                //var UserHannal = _repoUser.GetAll(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => a.FIRSTNAME + " " + a.LASTNAME);

                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = courseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                                    if (instructor_coursedetail.Any())
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = employeeService.GetById(details.Instructor_Id);
                                            Sent_Email_TMS(instructor, null, null, aModel.Course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region [--------sent mail trainee---------]
                            var data = courseService.GetCourseResultFinal(a => a.courseid == courseId && a.IsDeleted == false).OrderByDescending(a => a.courseid).ToList();

                            if (data.Any())
                            {
                                foreach (var trainees_final in data)
                                {
                                    Sent_Email_TMS(null, trainees_final.Trainee, null, aModel.Course, null, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/CourseResult", ex.Message);
            }
        }
        private bool UpdateStatus(Course course)
        {
            if (course == null) return false;
            course.LMSStatus = (int)UtilConstants.ApiStatus.Modify;
            var courseDetails = course.Course_Detail.ToList();
            if (!courseDetails.Any()) return false;
            foreach (var item in courseDetails)
            {
                item.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
            }
            courseService.Update(course);
            return true;
        }
        private void LogEvent(UtilConstants.LogType logType,
           UtilConstants.LogEvent logEvent, string logSourse, object messageEx)
        {
            var log = new
            {
                Message = messageEx,
                MessageDate = DateTime.Now,
                UserId = CurrentUser.USER_ID
            };
            configService.LogEvent(logType, logEvent, logSourse, log);
        }
        private void Modify_TMS(bool? isApprove, Course course, int type, int status, UtilConstants.ActionType actionType, string note = "", int? courseDetailId = -1)
        {
            courseDetailId = courseDetailId ?? -1;
            var approveType = UtilConstants.ApproveType.Course;
            var eStatus = UtilConstants.EStatus.Pending;
            var notiTemplate = string.Empty;
            var notiTemplateVn = string.Empty;
            var notiContent = string.Empty;
            var notiContentVn = string.Empty;
            #region [Request]
            if (actionType == UtilConstants.ActionType.Request)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Request_Course;
                        notiContentVn = UtilConstants.NotificationContent.Request_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Request_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Request_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_CourseResult_VN;
                        break;
                }
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    eStatus = UtilConstants.EStatus.Approve;
                }

                if (status == (int)UtilConstants.EStatus.CancelRequest)
                {
                    eStatus = UtilConstants.EStatus.CancelRequest;
                    notiTemplate = UtilConstants.NotificationTemplate.CancelRequest;
                    notiTemplateVn = UtilConstants.NotificationTemplate.CancelRequestVN;
                    notiContent = UtilConstants.NotificationContent.CancelRequest;
                    notiContentVn = UtilConstants.NotificationContent.CancelRequestVN;
                }
            }

            #endregion
            #region [Approve]

            if (actionType == UtilConstants.ActionType.Approve)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_Course;
                        notiContentVn = UtilConstants.NotificationContent.Approval_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Approval_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_CourseResult_VN;
                        break;
                }

                #region [status]
                switch (status)
                {
                    case (int)UtilConstants.EStatus.Pending:
                        eStatus = UtilConstants.EStatus.Pending;
                        break;
                    case (int)UtilConstants.EStatus.Approve:
                        eStatus = UtilConstants.EStatus.Approve;
                        break;
                    case (int)UtilConstants.EStatus.Reject:
                        eStatus = UtilConstants.EStatus.Reject;
                        break;
                    case (int)UtilConstants.EStatus.Block:
                        eStatus = UtilConstants.EStatus.Block;
                        break;
                }
                #endregion

                #region [Course]
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockCourse;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockCourseVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockCourse;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockCourseVn;
                }
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_Course;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_Course_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_Course;
                    notiContentVn = UtilConstants.NotificationContent.Reject_Course_VN;
                }
                #endregion
                #region [Assign]
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockAssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockAssignTraineeVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockAssign;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockAssignVn;
                }
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_AssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_AssignTrainee_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_AssignTrainee;
                    notiContentVn = UtilConstants.NotificationContent.Reject_AssignTrainee_VN;
                }
                #endregion
                #region [Subject]
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockSubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockSubjectResultVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockSubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockSubjectResultVn;
                }
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_SubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_SubjectResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_SubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_SubjectResult_VN;
                }
                #endregion
                #region [Final]
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockFinal;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockFinalVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockFinal;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockFinalVn;
                }
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_CourseResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_CourseResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_CourseResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_CourseResult_VN;
                }
                #endregion


            }
            #endregion


            var approve = Modify(isApprove, course, approveType, eStatus, actionType, courseDetailId, note);
            if (approve == null) throw new Exception(App_GlobalResources.Messege.WARNING_SENT_REQUEST_ERROR);
            var approver = GetApprover(approveType);
            //if(approver == null) throw new Exception(Messege.WARNING_NOT_FOUND_APPROVER);
            var approverId = approver?.UserId ?? 7; // admin = 7
            var toUser = actionType == UtilConstants.ActionType.Request ? approverId : approve.int_Requested_by;

            SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)approveType, approve.id, toUser, DateTime.Now, notiTemplate,
           (
           approveType == UtilConstants.ApproveType.SubjectResult
           ? string.Format(notiContent, approve.Course_Detail.SubjectDetail.Code, approve.Course.Code + " - " + approve.Course.Name, note)
           : string.Format(notiContent, approve.Course.Code + " - " + approve.Course.Name, note)),
           notiTemplateVn,
           (
           approveType == UtilConstants.ApproveType.SubjectResult
           ? string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Code, approve.Course.Code + " - " + approve.Course.Name, note)
           : string.Format(notiContentVn, approve.Course.Code + " - " + approve.Course.Name, note)));




        }
        protected void SendNotification(UtilConstants.NotificationType type, int? typelog, int? idApproval, int? to, DateTime? datesend, string messenge, string messengeContent, string messengeVn, string messengeContentVn)
        {
            Notification_Insert((int)type, typelog, idApproval, to ?? -1, datesend, messenge, messengeContent, messengeVn, messengeContentVn);
        }
        private void Notification_Insert(int typeNotification, int? typelog, int? idApproval, int to = -1, DateTime? datesend = null, string messenge = "", string messengeContent = "", string messenge_VN = "", string messengeContent_VN = "")
        {

            string url_ = "";
            #region[check url]
            switch (typelog)
            {
                case 1://request HOD course
                    url_ = "/Approve/Course/" + idApproval;
                    break;
                case 2://request HOD trainee
                    url_ = "/Approve/AssignTrainee/" + idApproval;
                    break;
                case 3://request HOD subject
                    url_ = "/Approve/SubjectResult/" + idApproval;
                    break;
                case 4://request HOD final
                    url_ = "/Approve/CourseResult/" + idApproval;
                    break;
                case 5://request user 
                    url_ = "/Course/Details/" + idApproval;
                    break;
            }
            #endregion
            //TODO : co thay doi iduser = iddata
            var currUser = CurrentUser;
            var notification = new Notification { Message = messenge, MessageContent = messengeContent, MessageVN = messenge_VN, MessageContentVN = messengeContent_VN, URL = url_, Type = typeNotification };
            ///////////////////////////////////////
            notification.Notification_Detail.Add(new Notification_Detail
            {
                //idmessenge = notification.MessageID,
                datesend = datesend,
                iddata = to,
                iduserfrom = currUser?.USER_ID,
                status = 0,
                IsDeleted = false,
                IsActive = true,
                Notification = notification
            });
            _repoNotification.Insert(notification);
            Uow.SaveChanges();
        }

        protected void UpdateMember(Course course)
        {

            var courseDetails = course.Course_Detail.ToList();
            var courseDetailIds = courseDetails.Select(a => a.Id);
            var tmsMember = courseService.GetTraineemember(a => courseDetailIds.Contains((int)a.Course_Details_Id)).ToList();
            if (tmsMember.Any())
            {
                var tmsMemberIds = tmsMember.Select(a => a.Member_Id).Distinct();
                foreach (var courseDetailId in courseDetailIds)
                {
                    var tmsMemberAvailable = tmsMember.Where(a => a.Course_Details_Id == courseDetailId);
                    if (!tmsMemberAvailable.Any())
                    {
                        foreach (var memberId in tmsMemberIds)
                        {
                            var entity = new TMS_Course_Member
                            {
                                Member_Id = memberId,
                                Course_Details_Id = courseDetailId,
                                IsDelete = false,
                                IsActive = true,
                                LmsStatus = (int)UtilConstants.ApiStatus.Modify
                            };
                            courseService.Modify(entity, courseDetailId, course.Id);
                        }
                    }
                }
            }
        }
        #region [------------------SENT EMAIL------------------]
        protected void Sent_Email_TMS(Trainee instructor, Trainee trainee, USER user, Course course, Course_Detail_Instructor details, int? int_Requested_by, int? actionType = -1, bool? LMSAssign = false)
        {//, UtilConstants.EStatus status
            #region [CodeNEw]

            var body_Ins = string.Empty;
            var mail_receiver = string.Empty;
            var TypeSentEmail = -1;
            //var checkHANNAH = CheckSiteConfig(UtilConstants.KEY_HANNAH);
            //var UserHannal = checkHANNAH ? UserContext.Get(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME)) : null;
            var checkHANNAH = configService.GetByKey("HANNAH");
            var UserHannal = checkHANNAH.Equals("1")
                ? employeeService.Get(a => a.Trainee_Type.Any(b => b.Type == (int)UtilConstants.TypeInstructor.Hannah))
                    .ToDictionary(a => a.Id, a => ReturnDisplayLanguage(a.FirstName, a.LastName))
                : null;

            #region [------Approve---------]
            #region [ApproveCourse]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor--------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedGV;
                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal;


                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = userContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse;

                }
            }
            #endregion

            #region [Assign Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.AssignTrainee)
            {

                if (trainee != null && course != null)
                {
                    if (LMSAssign == false)
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTrainee, null, trainee, course);
                        mail_receiver = trainee.str_Email;
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee;

                    }
                    else
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS, null, trainee, course);
                        mail_receiver = trainee.str_Email;
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS;


                    }

                }
            }
            #endregion

            #region [Approved Final Program]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor-------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV;

                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalMantor, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalMantor;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah;

                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (trainee != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalCourse, null, trainee, course);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse;


                }
            }

            #endregion
            #endregion

            #region [Create Password]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser)
            {
                if (user != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordUser, user, null, null);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordUser;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion


            #region [------Reject---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.Reject)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectGV;


                            break;
                        case (int)UtilConstants.TypeInstructor.Mentor:
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectMantor, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectMantor;


                            break;
                        case (int)UtilConstants.TypeInstructor.Hannah:
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah;


                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah_User;

                            //}
                            break;
                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = userContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectCourse;

                }
            }
            #endregion

            #region [------Cancel Request---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CancelRequest)
            {
                if (user != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailCancelRequest, user, null, course);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailCancelRequest;

                }

            }
            #endregion

            if (!string.IsNullOrEmpty(mail_receiver) && TypeSentEmail != -1 && !string.IsNullOrEmpty(body_Ins))
            {
                InsertSentMail(mail_receiver, TypeSentEmail, body_Ins, course?.Id);
            }
            #endregion
        }
        #endregion



        private string BodySendMail(UtilConstants.TypeSentEmail cAT_MAIL, USER uSER = null, Trainee trainee = null, Course Program = null)
        {
            var body = configService.GetMail(a => a.Type == (int)cAT_MAIL).FirstOrDefault()?.Content;
            if (body != null)
            {
                if (uSER != null)
                {
                    var pass = uSER.PASSWORD != null ? Common.DecryptString(uSER.PASSWORD) : "";
                    body = body.Replace(UtilConstants.MAIL_USER_USERNAME, uSER.USERNAME)
                        //.Replace(UtilConstants.MAIL_USER_FULLNAME, uSER.FIRSTNAME.Trim() + " " + uSER.LASTNAME.Trim())
                        .Replace(UtilConstants.MAIL_USER_FULLNAME, ReturnDisplayLanguage(uSER.FIRSTNAME.Trim(), uSER.LASTNAME.Trim()))

                        .Replace(UtilConstants.MAIL_USER_PASSWORD, pass)
                        .Replace(UtilConstants.MAIL_USER_EMAIL, uSER.EMAIL)
                        .Replace(UtilConstants.MAIL_USER_PHONE, uSER.PHONENO);
                }
                if (trainee != null)
                {
                    int? grade;
                    if (Program != null)
                    {
                        grade = trainee.Course_Result_Final?.Where(a => a.courseid == Program.Id).FirstOrDefault()?.grade;
                    }
                    else
                    {
                        grade = 0;
                    }
                    var pass = trainee.Password != null ? Common.DecryptString(trainee?.Password) : "";
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_PASSWORD, pass)
                       //.Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, trainee.FirstName + " " + trainee.LastName)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, ReturnDisplayLanguage(trainee.FirstName, trainee.LastName))
                       .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                       .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone)
                       .Replace(UtilConstants.MAIL_TRAINEE_GRADE, GetGrace(grade));
                }
                if (Program != null)
                {
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, Program.Name)
                       .Replace(UtilConstants.MAIL_PROGRAM_CODE, Program.Code)
                       .Replace(UtilConstants.MAIL_PROGRAM_STARTDATE, Program.StartDate.Value.ToString("dd/MM/yyyy")) ?? ""
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, Program.EndDate.Value.ToString("dd/MM/yyyy")) ?? ""
                       .Replace(UtilConstants.MAIL_PROGRAM_VENUE, Program.Venue)
                       .Replace(UtilConstants.MAIL_PROGRAM_MAXTRAINEE, Program.NumberOfTrainee.ToString())
                       .Replace(UtilConstants.MAIL_PROGRAM_NOTE, Program.Note);

                    var itemcourse = "";
                    var listCourse = Program.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse.Count() > 0)
                    {
                        var count = 0;
                        itemcourse = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                        itemcourse += "<tbody>";
                        itemcourse += "<tr>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblStt + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblCode + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblName + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblStartDate + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblEndDate + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblMethod + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblRoom + "</th>";
                        itemcourse += "<th>" + @App_GlobalResources.Resource.lblInstructor + "</th>";
                        itemcourse += "</tr>";
                        foreach (var item in listCourse)
                        {
                            var instructor = "";
                            var dbInstructor = item.Course_Detail_Instructor.ToList();
                            if (dbInstructor.Any())
                            {
                                //instructor = dbInstructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(b => b.Trainee.FirstName + " " + b.Trainee.LastName).Aggregate(instructor, (current, fullName) => current + (fullName + "<br />"));
                                instructor = dbInstructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(instructor, (current, fullName) => current + (fullName + "<br />"));
                            }
                            count++;
                            itemcourse += "<tr>";
                            itemcourse += "<td>" + count + "</td>";
                            itemcourse += "<td>" + item.SubjectDetail.Code + "</td>";
                            itemcourse += "<td>" + item.SubjectDetail.Name + "</td>";
                            itemcourse += "<td>" + item.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "" + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                            itemcourse += "<td>" + item.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? "" + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                            itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                            itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                            itemcourse += "<td>" + instructor + "</td>";
                            itemcourse += "</tr>";
                        }
                        itemcourse += "</tbody>";
                        itemcourse += "</table>";
                        body = body.Replace(UtilConstants.MAIL_LIST_COURSE, itemcourse);
                    }

                }

            }
            return body;
        }
        private string GetGrace(int? grade)
        {
            var strGrade = "";
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    strGrade = "Fail";
                    break;
                case (int)UtilConstants.Grade.Pass:
                    strGrade = "Pass";
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    strGrade = "Distinction";
                    break;
            }
            return strGrade;
        }
        private string TypeLearningName(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "Classroom";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "Online";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "Blended";
                    break;
            }
            return str_type;
        }
        private TMS_SentEmail InsertSentMail(string mail_receiver, int typeSentMail, string bodySentMail, int? courseId)
        {
            try
            {
                var entity = new TMS_SentEmail();

                entity.mail_receiver = mail_receiver;
                entity.type_sent = typeSentMail;
                entity.content_body = bodySentMail;
                entity.flag_sent = 0;
                entity.cat_mail_ID = configService.GetMail(a => a.Type == typeSentMail)?.FirstOrDefault()?.ID;
                entity.id_course = courseId;
                entity.Is_Deleted = false;
                entity.Is_Active = true;


                courseService.InsertSentMail(entity);

                return entity;
            }
            catch (Exception ex)
            {
                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogSourse.SendMail, UtilConstants.LogEvent.Insert, ex.Message);
                return null;
            }

        }
        private bool CallServices(string type)
        {
            var server = configService.GetByKey("SERVER");
            var token = configService.GetByKey("TOKEN");
            var function = configService.GetByKey("FUNCTION");
            var moodlewsrestformat = configService.GetByKey("moodlewsrestformat");
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);

            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("type", type);
            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {

                return false;
            }

            var responseContent = response.Content;
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                return true;
            }
            return false;
        }

        public IQueryable<TMS_APPROVES> Get(Expression<Func<TMS_APPROVES, bool>> query)
        {
            var entities = _repoApprove.GetAll(query);          
            return entities;
        }

        #endregion
    }
}
