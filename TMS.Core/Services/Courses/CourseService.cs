﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using DAL.UnitOfWork;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.ModelBinding;
using DAL.Entities;
using DAL.Repositories;
using TMS.Core.App_GlobalResources;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.ReportModels;

namespace TMS.Core.Services.Courses
{
    using Utils;
    using ViewModels.Courses;
    using ViewModels.Room;
    using ViewModels.Company;
    using ViewModels.UserModels;
    using TMS.Core.ViewModels.Nation;
    using System.Text;
    using Newtonsoft.Json;
    using TMS.Core.Services.Configs;
    using TMS.Core.ViewModels.AjaxModels.AjaxAssignMember;
    using System.Web;

    public class CourseService : BaseService, ICourseService
    {
        private readonly IRepository<Course_Cost> _repoCost;
        private readonly IRepository<Course_TrainingCenter> _repoCouseTrainingCenter;
        private readonly IRepository<Course_Result> _repoCourseResult;
        private readonly IRepository<Course_Result_Final> _repoCourseResultFinal;
        private readonly IRepository<Course_Type> _repoCourseType;
        private readonly IRepository<Course_Type_Result> _repoCourseTypeResult;
        private readonly IRepository<Trainee> _repoTrainee;
        private readonly IRepository<Room> _repoRoom;
        private readonly IRepository<Company> _repoCompany;
        private readonly IRepository<Nation> _repoNation;
        private readonly IRepository<SubjectDetail> _repoSubjectDetail;
        private readonly IRepository<CAT_GROUPSUBJECT> _repoGroupSubject;
        private readonly IRepository<Course_Detail_Instructor> _repoCourseDetailInstructor;
        private readonly IRepository<TMS_Course_Member> _repoCourseMember;
        private readonly IRepository<TMS_APPROVES> _repoTmsApprove;
        private readonly IRepository<TMS_APPROVES_LOG> _repoTmsApproveLog;
        private readonly IRepository<Course_Result_Summary> _repoCourseResultSummary;
        private readonly IRepository<Course_LMS_STATUS> _repoCourseLmsStatus;
        private readonly IRepository<LMS_Assign> _repoLmsAssign;
        private readonly IRepository<Instructor_Ability> _repoInstructorAbility;
        private readonly IRepository<Course_Attendance> _repoCourseAttendance;
        private readonly IRepository<TMS_SentEmail> _repoSentMail;
        private readonly IRepository<Subject_Score> _repoSubjectScore;
        private readonly IRepository<JobTitle> _repoJobTitle;
        private readonly IRepository<CAT_CERTIFICATE> _repoCatCertificate;
        private readonly IConfigService configService;
        private readonly IRepository<TraineeHistory_Item> _repoTraineeHistoryItem;
        private readonly IRepository<TraineeHistory> _repoTraineeHistory;
        private readonly IRepository<TraineeFuture> _repoTraineeTraineeFuture;
        private readonly IRepository<Management_Room_Item> _repoManagement_Room_Item;

        private readonly IRepository<PROCESS_Steps> _repoPROCESS_Steps;
        private readonly IRepository<CAT_GROUPSUBJECT> _repoCatGroupSubject;
        private readonly IRepository<Postnew> _repoPostNew;
        private readonly IRepository<Postnews_Category> _repoPostNewCategory;
        private readonly IRepository<TMS_APPROVES_HISTORY> _repoApproveHistory;
        private readonly IRepository<DAL.Entities.Department> _repoDepartment;
        private readonly IRepository<Group_Certificate> _repoGroupCertificate;
        private readonly IRepository<Group_Certificate_Schedule> _repoGroupCertificateSchedule;
        private readonly IRepository<Group_Certificate_Subjects> _repoGroupCertificateSubject;
        private readonly IRepository<Trainee_Portal> _repoCourseTrainne;
        private readonly IRepository<Course_Detail> _repoCourseDetail;
        private readonly IRepository<Course_Blended_Learning> _repoblended;
        private readonly IRepository<Course_Ingredients_Learning> _repoIngredients;
        private readonly IRepository<Course_Detail_Course_Ingredients> _repoDetailIngredients;
        private readonly IRepository<Course_Result_Course_Detail_Ingredient> _repoIngreIndient;
        private readonly IRepository<Course_Subject_Item> _repoCourseSubjectitem;
        private readonly IRepository<Course_Detail_Room> _repoCourseDetailRoom;
        private readonly IRepository<Course_Detail_Room_Global> _repoCourseDetailRoomGlobal;
        private readonly Expression<Func<Course, bool>> _courseDefaultfilter =
            a => a.IsActive == true && a.IsDeleted != true;
        private readonly Expression<Func<Course_Result_Final, bool>> _finalDefaultfilter = a => a.IsDeleted == false;
        private readonly Expression<Func<Group_Certificate, bool>> _groupCertificateDefaultfilter = a => a.IsDeleted == false;
        private readonly Expression<Func<Group_Certificate_Schedule, bool>> _groupCertificateScheduleDefaultfilter = a => a.IsDeleted == false;
        private readonly Expression<Func<Course_Ingredients_Learning, bool>> _ingredientDefaultFilter = a => a.IsDeleted == false;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<TMS_Course_Member_Remark> _repoCourseMemberRemark;
        private readonly IRepository<CourseRemarkCheckFail> _repoCourseRemarkCheckFail;
        private readonly IRepository<Room_Type> _repoRoomType;
        private readonly IRepository<TMS_CertificateApproved> _repoTMSCertificateAppoved;
        private const int statusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        private const int statusAddNewTMS = (int)UtilConstants.ApiStatus.AddNewTMS;
        protected int StatusUnSuccessfully = (int)UtilConstants.ApiStatus.UnSuccessfully;
        private const int statusIsNoResponse = (int)UtilConstants.ApiStatus.NoResponse;
        private readonly IRepository<Survey> _repoSurvey;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }

        private readonly IRepository<Meeting> _repoMeeting;
        private readonly IRepository<Meeting_Participants> _repoMeetingParticipant;

        public CourseService(IUnitOfWork unitOfWork, IRepository<TraineeHistory> repoTraineeHistory,
            IRepository<Trainee_Portal> repoCourseTrainne,
            IRepository<TraineeFuture> repoTraineeTraineeFuture, IRepository<TraineeHistory_Item> repoTraineeHistoryItem, IRepository<CAT_CERTIFICATE> repoCatCertificate, IRepository<JobTitle> repoJobTitle, IRepository<Subject_Score> repoSubjectScore, IRepository<Course_Attendance> repoCourseAttendance, IRepository<Course> repoCourse, IRepository<CAT_GROUPSUBJECT> repoGroupSubject, IRepository<Course_Result> repoCourseResult, IRepository<Course_Result_Final> repoCourseResultFinal, IRepository<Course_TrainingCenter> repoCouseTrainingCenter, IRepository<Course_Type_Result> repoCourseTypeResult, IRepository<Room> repoRoom, IRepository<Course_Type> repoCourseType, IRepository<Trainee> repoTrainee, IRepository<SubjectDetail> repoSubjectDetail, IRepository<Course_Detail_Instructor> repoCourseDetailInstructor, IRepository<TMS_Course_Member> repoCourseMember, IRepository<TMS_APPROVES> repoTmsApprove, IRepository<Course_Result_Summary> repoCourseResultSummary, IRepository<Course_LMS_STATUS> repoCourseLmsStatus, IRepository<Company> repoCompany, IRepository<LMS_Assign> repoLmsAssign, IRepository<Nation> repoNation, IRepository<Instructor_Ability> repoInstructorAbility, IRepository<Course_Detail> repoCourseDetail, IRepository<Course_Cost> repoCost, IRepository<TMS_APPROVES_LOG> repoTmsApproveLog, IRepository<SYS_LogEvent> repoSYS_LogEvent, IConfigService _configService, IRepository<TMS_SentEmail> repoSentMail, IRepository<PROCESS_Steps> repoPROCESS_Steps, IRepository<CAT_GROUPSUBJECT> repoCatGroupSubject, IRepository<Meeting> repoMeeting, IRepository<Meeting_Participants> repoMeetingParticipant, IRepository<Postnew> repoPostNew, IRepository<Postnews_Category> repoPostNewCategory, IRepository<TMS_APPROVES_HISTORY> repoApproveHistory, IRepository<DAL.Entities.Department> repoDepartment, IRepository<Management_Room_Item> repoManagement_Room_Item, IRepository<Group_Certificate> repoGroupCertificate, IRepository<Group_Certificate_Schedule> repoGroupCertificateSchedule, IRepository<Group_Certificate_Subjects> repoGroupCertificateSubject, IRepository<Course_Blended_Learning> repoblended, IRepository<Course_Ingredients_Learning> repoIngredients, IRepository<Course_Detail_Course_Ingredients> repoDetailIngredients, IRepository<Room_Type> repoRoomType, IRepository<Survey> repoSurvey, IRepository<Course_Result_Course_Detail_Ingredient> repoIngreIndient, IRepository<Course_Subject_Item> repoCourseSubjectitem, IRepository<TMS_Course_Member_Remark> repoCourseMemberRemark, IRepository<CourseRemarkCheckFail> repoCourseRemarkCheckFail, IRepository<TMS_CertificateApproved> repoTMSCertificateAppoved, IRepository<Course_Detail_Room> repoCourseDetailRoom, IRepository<Course_Detail_Room_Global> repoCourseDetailRoomGlobal) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCost = repoCost;
            _repoCourseAttendance = repoCourseAttendance;
            _repoCourseLmsStatus = repoCourseLmsStatus;
            _repoCourseResult = repoCourseResult;
            _repoCourseTrainne = repoCourseTrainne;
            _repoCourseResultFinal = repoCourseResultFinal;
            _repoCouseTrainingCenter = repoCouseTrainingCenter;
            _repoCourseTypeResult = repoCourseTypeResult;
            _repoRoom = repoRoom;
            _repoCompany = repoCompany;
            _repoNation = repoNation;
            _repoCourseType = repoCourseType;
            _repoTrainee = repoTrainee;
            _repoSubjectDetail = repoSubjectDetail;
            _repoCourseDetailInstructor = repoCourseDetailInstructor;
            _repoCourseMember = repoCourseMember;
            _repoTmsApprove = repoTmsApprove;
            _repoCourseResultSummary = repoCourseResultSummary;
            _repoLmsAssign = repoLmsAssign;
            _repoInstructorAbility = repoInstructorAbility;
            _repoCourseDetail = repoCourseDetail;
            _repoGroupSubject = repoGroupSubject;
            _unitOfWork = unitOfWork;
            _repoTmsApproveLog = repoTmsApproveLog;
            _repoSentMail = repoSentMail;
            configService = _configService;
            _repoSubjectScore = repoSubjectScore;
            _repoJobTitle = repoJobTitle;
            _repoCatCertificate = repoCatCertificate;
            _repoTraineeHistoryItem = repoTraineeHistoryItem;
            _repoTraineeTraineeFuture = repoTraineeTraineeFuture;
            _repoTraineeHistory = repoTraineeHistory;
            _repoPROCESS_Steps = repoPROCESS_Steps;
            _repoCatGroupSubject = repoCatGroupSubject;
            _repoMeeting = repoMeeting;
            _repoMeetingParticipant = repoMeetingParticipant;
            _repoPostNew = repoPostNew;
            _repoPostNewCategory = repoPostNewCategory;
            _repoApproveHistory = repoApproveHistory;
            _repoDepartment = repoDepartment;
            _repoManagement_Room_Item = repoManagement_Room_Item;
            _repoGroupCertificate = repoGroupCertificate;
            _repoGroupCertificateSchedule = repoGroupCertificateSchedule;
            _repoGroupCertificateSubject = repoGroupCertificateSubject;
            _repoblended = repoblended;
            _repoIngredients = repoIngredients;
            _repoDetailIngredients = repoDetailIngredients;
            _repoRoomType = repoRoomType;
            _repoSurvey = repoSurvey;
            _repoIngreIndient = repoIngreIndient;
            _repoCourseSubjectitem = repoCourseSubjectitem;
            _repoCourseMemberRemark = repoCourseMemberRemark;
            _repoCourseRemarkCheckFail = repoCourseRemarkCheckFail;
            _repoTMSCertificateAppoved = repoTMSCertificateAppoved;
            _repoCourseDetailRoom = repoCourseDetailRoom;
            _repoCourseDetailRoomGlobal = repoCourseDetailRoomGlobal;
        }
        public Course GetById(int? id)
        {
            return id.HasValue ? RepoCourse.Get(id) : null;
        }

        public Course GetByCourseCode(string code)
        {
            return RepoCourse.GetAll(a => a.IsDeleted != true && a.Code.Equals(code)).FirstOrDefault();
        }
        public IQueryable<Course> GetCoursesRp()
        {
            return RepoCourse.GetAll(a => a.IsDeleted != true);
        }
        public IQueryable<TMS_Course_Member> GetTraineemember(Expression<Func<TMS_Course_Member, bool>> query)
        {
            var entities = _repoCourseMember.GetAll(a => a.IsDelete != true && a.Course_Detail.IsDeleted != true);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<Course_Cost> GetCost()
        {
            return _repoCost.GetAll();
        }
        public IQueryable<Course> Get(bool? NotApproval = null)
        {
            //TODO: CORE to VIETJET: source core có dòng where bên dưới
            var entities = RepoCourse.GetAll(_courseDefaultfilter);//.Where(a => a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
            if (NotApproval == null)
            {
                var datastep = _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ApproveType.Course && a.IsActive == true);
                entities = datastep != null ? entities.Where(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course)) : entities;
            }
            return entities;
        }
        public IQueryable<Course> ApiGetAllCourses(Expression<Func<Course, bool>> query = null)
        {
            return query == null
                ? RepoCourse.GetAll()
                : RepoCourse.GetAll(query);
        }

        public bool UpdateCourseTrainnePending(Trainee_Portal item)
        {
            if (item == null)
                return false;
            _repoCourseTrainne.Update(item);
            _unitOfWork.SaveChanges();
            return true;
        }

        public bool RemoveCourseTrainnePending(Trainee_Portal item)
        {
            _repoCourseTrainne.Delete(item);
            _unitOfWork.SaveChanges();
            return true;
        }

        public bool DeleteCourseTrainnePending(Trainee_Portal item)
        {
            if (item == null)
                return false;

            item.IsDeleted = true;

            _repoCourseTrainne.Update(item);
            _unitOfWork.SaveChanges();
            return true;
        }

        public bool InsertCourseTrainnePending(Trainee_Portal item)
        {
            if (item == null)
                return false;
            _repoCourseTrainne.Insert(item);
            _unitOfWork.SaveChanges();
            return true;
        }

        public Course GetByCode(string code)
        {
            return RepoCourse.GetAll(a => a.IsDeleted != true).Where(c => c.Code.Equals(code)).FirstOrDefault();
        }

        public Trainee_Portal GetByCourseAndTrainne(int courseID, int traineeID)
        {
            var query = _repoCourseTrainne.GetAll(a => a.IsDeleted == false).Where(c => c.CourseID == courseID && c.TraineeID == traineeID).FirstOrDefault();
            return query;
        }

        public List<int> GetListSubjectDetailIdByCourseId(int courseId)
        {
            var query = _repoCourseDetail.GetAll().Where(x => x.CourseId == courseId).Select(m => (int)m.SubjectDetailId).ToList();
            return query;
        }

        public IQueryable<Course> Get(ICollection<string> code)
        {
            return RepoCourse.GetAll(_courseDefaultfilter).Where(a => code.Contains(a.Code) && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
        }

        public List<TMS_Course_Member> GetListCourseMemberByMember(int traineeId, bool isDelete = false, bool isActive = true)
        {
            var lst = _repoCourseMember.GetAll(x => x.Member_Id == traineeId && x.IsDelete == isDelete && x.IsActive == isActive).ToList();
            return lst;
        }


        public void Insert(TMS_Course_Member model, string currentUser, int courseDetailId, int courseId)
        {
            var course = RepoCourse.Get(courseId);
            var courseDetails = course.Course_Detail;
            foreach (var courseDetail in courseDetails.Where(a => a.IsDeleted == false))
            {
                var detail = courseDetail;
                //insert trùng
                //var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id && a.IsDelete == false);
                var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id);
                if (entity == null)
                {
                    entity = new TMS_Course_Member()
                    {
                        IsDelete = false,
                        IsActive = true,
                        Member_Id = model.Member_Id,
                        LmsStatus = statusModify,
                        AssignBy = currentUser,
                        Status = (int)UtilConstants.ApiStatus.Synchronize,
                        Course_Details_Id = detail.Id
                    };
                    _repoCourseMember.Insert(entity);
                }
                else
                {
                    entity.IsDelete = false;
                    entity.IsActive = true;
                    entity.AssignBy = currentUser;
                    entity.LmsStatus = statusModify;
                    _repoCourseMember.Update(entity);
                }
            }
            if (model.Member_Id != null)
            {
                //UpdateLmsStatus(course, UtilConstants.LMSStatus.AssignTrainee);
                RegistTraineeToCourse(model.Member_Id.Value, courseId);
            }
            Uow.SaveChanges();
        }
        public void Insert(int Member_Id, int Course_Details_Id, int courseId, String AssignBy)
        {
            var entity = _repoCourseMember.Get(a => a.Member_Id == Member_Id && a.Course_Details_Id == Course_Details_Id);
            if (entity == null)
            {
                entity = new TMS_Course_Member()
                {
                    IsDelete = false,
                    IsActive = true,
                    Member_Id = Member_Id,
                    LmsStatus = statusModify,
                    AssignBy = AssignBy,
                    Status = (int)UtilConstants.ApiStatus.Synchronize,
                    Course_Details_Id = Course_Details_Id
                };
                _repoCourseMember.Insert(entity);
            }
            else
            {
                entity.IsDelete = false;
                entity.IsActive = true;
                entity.AssignBy = AssignBy;
                entity.LmsStatus = statusModify;
                _repoCourseMember.Update(entity);
            }

            if (Member_Id != null)
            {
                //UpdateLmsStatus(course, UtilConstants.LMSStatus.AssignTrainee);
                RegistTraineeToCourse(Member_Id, courseId);
            }
            Uow.SaveChanges();
        }

        public Trainee_Portal GetByCourseAndTrainneById(int id)
        {
            return _repoCourseTrainne.Get(id);
        }

        public void Delete(TMS_Course_Member model, string currentUser, int courseId)
        {
            var course = RepoCourse.Get(courseId);
            var courseDetails = course.Course_Detail.Where(a => a.IsDeleted == false).ToList();
            foreach (var courseDetail in courseDetails)
            {
                var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == courseDetail.Id);
                if (entity != null)
                {
                    entity.IsDelete = true;
                    entity.IsActive = false;

                    entity.LmsStatus = statusModify;
                    _repoCourseMember.Update(entity);
                }
            }
            Uow.SaveChanges();
            var final = _repoCourseResultFinal.Get(a => a.courseid == courseId && a.traineeid == model.Member_Id);
            if (final != null)
            {
                final.IsDeleted = true;
                final.DeletedBy = currentUser;
                final.DeletedDate = DateTime.Now;
                final.LmsStatus = statusIsSync;
                final.MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Removed;
                // UpdateLmsStatus(final.Course, UtilConstants.LMSStatus.AssignTrainee);
                _repoCourseResultFinal.Update(final);
                Uow.SaveChanges();
            }
        }

        public List<Trainee_Portal> GetListCourseTrainne(string traineeName = "")
        {
            if (!string.IsNullOrEmpty(traineeName))
            {
                return _repoCourseTrainne.GetAll(c => c.TraineeUserName.Contains(traineeName)).OrderByDescending(x => x.Id).ToList();
            }
            return _repoCourseTrainne.GetAll().OrderByDescending(x => x.Id).ToList();
        }

        public IQueryable<Course> Get(string code, string name, string venue)
        {
            return RepoCourse.GetAll(_courseDefaultfilter).Where(a => (string.IsNullOrEmpty(code) || a.Code.Contains(code)) && (string.IsNullOrEmpty(name) || a.Name.Contains(name)) && (string.IsNullOrEmpty(venue) || a.Venue.Contains(venue)) && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
        }

        public IQueryable<Course> Get(string code, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return RepoCourse.GetAll(_courseDefaultfilter).Where(a => (string.IsNullOrEmpty(code) || a.Code.Contains(code)) && (!dateFrom.HasValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) && (!dateTo.HasValue || SqlFunctions.DateDiff("day", a.StartDate, dateTo) >= 0) && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
        }

        public IQueryable<Course> Get(string code, string name, string venue, int? type, int? status)
        {
            return RepoCourse.GetAll(_courseDefaultfilter).Where(a => (string.IsNullOrEmpty(code) || a.Code.Contains(code)) && (string.IsNullOrEmpty(name) || a.Name.Contains(name)) && (string.IsNullOrEmpty(venue) || a.Venue.Contains(venue)) /*&& (!status.HasValue ||  a.int_Status == status)*/ && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
        }

        public IQueryable<Course> Get(Expression<Func<Course, bool>> query, bool? notApproval = null)
        {
            var entities = RepoCourse.GetAll(a => a.IsDeleted == false && a.IsActive == true /*&& a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))*/);
            var x = entities.Count();
            if (query != null)
            {
                entities = entities.Where(query);
            }

            if (notApproval == null)
            {
                var datastep = _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ApproveType.Course && a.IsActive == true);
                entities = datastep != null ? entities.Where(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course)) : entities;
                var xxx = entities.Count();
            }
            return entities;
        }
        public IQueryable<Course> GetListCourse(Expression<Func<Course, bool>> query, bool? notApproval = null)
        {
            var entities = RepoCourse.GetAll(a => a.IsDeleted == false /*&& a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))*/);
            var x = entities.Count();
            if (query != null)
            {
                entities = entities.Where(query);
            }

            if (notApproval == null)
            {
                var datastep = _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ApproveType.Course && a.IsActive == true);
                entities = datastep != null ? entities.Where(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course)) : entities;
                var xxx = entities.Count();
            }
            return entities;
        }
        public IQueryable<Course> GetCreatEID(Expression<Func<Course, bool>> query)
        {
            var entities = RepoCourse.GetAll(a => a.IsDeleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public IQueryable<Course> GetCodeProgram(Expression<Func<Course, bool>> query)
        {
            var entities = query == null ? RepoCourse.GetAll() : RepoCourse.GetAll(query);
            return entities;
        }
        public IQueryable<Course> Get_BondAgreement(Expression<Func<Course, bool>> query, int? approveType = null, int? eStatus = null)
        {
            var entities = RepoCourse.GetAll(query).Where(a => a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
            var datastep = _repoPROCESS_Steps.Get(a => a.Step == approveType && a.IsActive == true);
            entities = datastep != null ?
                entities.Where(a => a.TMS_APPROVES.Any(b => (eStatus == null || b.int_id_status == eStatus) &&
                (approveType == null || b.int_Type == approveType))) : entities;
            return entities;
        }

        public bool checkapproval(Course course, int[] approveType = null)
        {
            var return_ = false;
            if (approveType != null)
            {
                foreach (var item in approveType)
                {
                    var datastep = _repoPROCESS_Steps.Get(a => a.Step == item && a.IsActive == true);
                    return_ = datastep != null ? course.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == item) ? true : false
                        : true;
                }
            }
            return return_;
        }
        public IQueryable<Course_Cost> GetCost(Expression<Func<Course_Cost, bool>> query)
        {
            var entities = _repoCost.GetAll(a => a.IsDeleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<Course_Detail_Instructor> GetAllowance(Expression<Func<Course_Detail_Instructor, bool>> query)
        {
            var entities = _repoCourseDetailInstructor.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<Course> ApiGet(Expression<Func<Course, bool>> query)
        {
            var entities = RepoCourse.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public IQueryable<Course> GetApproveCourseByType(int type)
        {
            return RepoCourse.GetAll(_courseDefaultfilter).Where(a => a.TMS_APPROVES.Any(x => x.int_Type == type) && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)));
        }

        public IQueryable<Course_Result_Final> GetCourseMembers(int courseId, bool isViewAll = false)
        {
            var entities = _repoCourseResultFinal.GetAll(a => a.courseid == courseId);
            return isViewAll ? entities : entities.Where(a => a.IsDeleted == false);
        }

        public IQueryable<Course_Result_Final> ApiGetAll(Expression<Func<Course_Result_Final, bool>> query = null)
        {
            return query == null
                ? _repoCourseResultFinal.GetAll()
                : _repoCourseResultFinal.GetAll(query);

        }

        public IQueryable<Course_TrainingCenter> GetTrainingCenters(Expression<Func<Course_TrainingCenter, bool>> query = null)
        {
            return query == null
                ? _repoCouseTrainingCenter.GetAll()
                : _repoCouseTrainingCenter.GetAll(query);

        }

        public void DeleteTrainingCenters(IEnumerable<Course_TrainingCenter> entities)
        {
            _repoCouseTrainingCenter.Delete(entities);
            Uow.SaveChanges();
        }

        public void InsertTrainingCenters(Course_TrainingCenter entity)
        {
            _repoCouseTrainingCenter.Insert(entity);
            Uow.SaveChanges();
        }

        public void Update(Course entity)
        {
            RepoCourse.Update(entity);
            Uow.SaveChanges();
        }
        public void UpdateTmsMember(TMS_Course_Member entity, int courseDetailId, int courseId)
        {
            _repoCourseMember.Update(entity);

            RegistTraineeToCourse(entity.Member_Id.Value, courseId);
            //UpdateStatusTraineeHistoryItem(entity.Member_Id.Value, courseDetailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
            Uow.SaveChanges();
        }


        public void Modify(TMS_Course_Member entity, int courseDetailId, int courseId)
        {
            _repoCourseMember.Insert(entity);

            RegistTraineeToCourse(entity.Member_Id.Value, courseId);
            //ApproveAssign(courseId,UtilConstants.ApproveType.AssignedTrainee);
            Uow.SaveChanges();
        }

        public void Insert(TMS_Course_Member model, int courseDetailId, int courseId, UtilConstants.ApproveType approve, int processStep, int typerequest = -1)
        {
            var course = RepoCourse.Get(courseId);
            var courseDetails = course.Course_Detail;
            foreach (var courseDetail in courseDetails.Where(a => a.IsDeleted == false))
            {
                var detail = courseDetail;
                //insert trùng
                //var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id && a.IsDelete == false);
                var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id);
                TMS_Course_Member_Remark entity_remark = _repoCourseMemberRemark.Get(a => a.TraineeId == model.Member_Id && a.CourseId == courseId);
                if (entity == null)
                {
                    entity = new TMS_Course_Member()
                    {
                        IsDelete = false,
                        IsActive = true,
                        Member_Id = model.Member_Id,
                        LmsStatus = statusAddNewTMS,//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync,
                        AssignBy = CurrentUser.USER_ID.ToString(),

                        Course_Details_Id = detail.Id
                    };
                    if (typerequest == 0) // gửi request 
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                    }
                    else if (typerequest == 1)
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Approved;// da gui  phê duyệt
                    }
                    _repoCourseMember.Insert(entity);
                }
                else
                {
                    entity.IsDelete = false;
                    entity.IsActive = true;
                    entity.AssignBy = CurrentUser.USER_ID.ToString();
                    entity.LmsStatus = statusAddNewTMS;//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync;
                    if (typerequest == 0) // gửi request
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                    }
                    else if (typerequest == 1)
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Approved;// chưa gửi phê duyệt
                    }

                    _repoCourseMember.Update(entity);


                }

                if (entity_remark == null)
                {
                    entity_remark = new TMS_Course_Member_Remark()
                    {
                        TraineeId = (int)model.Member_Id,
                        CourseId = courseId,
                        remark = model.Remark,
                    };
                    _repoCourseMemberRemark.Insert(entity_remark);
                }
                else
                {
                    entity_remark.TraineeId = (int)model.Member_Id;
                    entity_remark.CourseId = courseId;
                    entity_remark.remark = model.Remark;
                    _repoCourseMemberRemark.Update(entity_remark);
                }
            }
            if (typerequest == 1 || typerequest == -1) // gửi approve
            {
                if (model.Member_Id != null)
                {
                    RegistTraineeToCourse(model.Member_Id.Value, courseId);
                    //ActionTmsProcess(courseId, UtilConstants.ApproveType.AssignedTrainee);
                    //Update Status traineeHistory_Item
                    //UpdateStatusTraineeHistoryItem(model.Member_Id.Value, courseDetailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
                }
            }

            Uow.SaveChanges();
        }
        public void Insert_Custom(TMS_Course_Member model, Course course, UtilConstants.ApproveType approve, int processStep, int typerequest = -1)
        {
            var courseDetails = course.Course_Detail;
            foreach (var detail in courseDetails)
            {
                //insert trùng
                //var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id && a.IsDelete == false);
                var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == detail.Id);

                if (entity == null)
                {
                    entity = new TMS_Course_Member()
                    {
                        IsDelete = false,
                        IsActive = true,
                        Member_Id = model.Member_Id,
                        LmsStatus = statusAddNewTMS,//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync,
                        AssignBy = CurrentUser.USER_ID.ToString(),

                        Course_Details_Id = detail.Id
                    };
                    if (typerequest == 0) // gửi request 
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                    }
                    else if (typerequest == 1)
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Approved;// da gui  phê duyệt
                    }
                    _repoCourseMember.Insert(entity);
                }
                else
                {
                    entity.IsDelete = false;
                    entity.IsActive = true;
                    entity.AssignBy = CurrentUser.USER_ID.ToString();
                    entity.LmsStatus = statusAddNewTMS;//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync;
                    if (typerequest == 0) // gửi request
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                    }
                    else if (typerequest == 1)
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Approved;// da gui  phê duyệt
                    }

                    _repoCourseMember.Update(entity);


                }
                if (!string.IsNullOrEmpty(model.Remark))
                {
                    TMS_Course_Member_Remark entity_remark = _repoCourseMemberRemark.Get(a => a.TraineeId == model.Member_Id && a.CourseId == course.Id);
                    if (entity_remark == null)
                    {
                        entity_remark = new TMS_Course_Member_Remark()
                        {
                            TraineeId = (int)model.Member_Id,
                            CourseId = course.Id,
                            remark = model.Remark,
                        };
                        _repoCourseMemberRemark.Insert(entity_remark);
                    }
                    else
                    {
                        entity_remark.TraineeId = (int)model.Member_Id;
                        entity_remark.CourseId = course.Id;
                        entity_remark.remark = model.Remark;
                        _repoCourseMemberRemark.Update(entity_remark);
                    }
                }
            }
            if (typerequest == 1 || typerequest == -1) // gửi approve
            {
                if (model.Member_Id != null)
                {
                    RegistTraineeToCourse(model.Member_Id.Value, course.Id);
                    //ActionTmsProcess(courseId, UtilConstants.ApproveType.AssignedTrainee);
                    //Update Status traineeHistory_Item
                    //UpdateStatusTraineeHistoryItem(model.Member_Id.Value, courseDetailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
                }
            }

            Uow.SaveChanges();
        }


        public void Delete(TMS_Course_Member model, int courseId)
        {
            var course = RepoCourse.Get(courseId);
            var courseDetails = course.Course_Detail.Where(a => a.IsDeleted != true).ToList();
            foreach (var courseDetail in courseDetails)
            {
                var entity = _repoCourseMember.Get(a => a.Member_Id == model.Member_Id && a.Course_Details_Id == courseDetail.Id);
                if (entity != null)
                {
                    entity.IsDelete = true;
                    entity.IsActive = false;

                    entity.LmsStatus = statusModify;
                    _repoCourseMember.Update(entity);

                    //Remove Status TraineeHistory Item
                    // UpdateStatusTraineeHistoryItem(model.Member_Id.Value, courseDetail.Id, (int)UtilConstants.StatusTraineeHistory.Missing);
                }
            }
            Uow.SaveChanges();
            var final = _repoCourseResultFinal.Get(a => a.courseid == courseId && a.traineeid == model.Member_Id);
            if (final != null)
            {
                final.IsDeleted = true;
                final.DeletedBy = CurrentUser.USER_ID.ToString();
                final.DeletedDate = DateTime.Now;
                final.LmsStatus = statusIsSync;
                final.MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Removed;
                // UpdateLmsStatus(final.Course, UtilConstants.LMSStatus.AssignTrainee);
                _repoCourseResultFinal.Update(final);
                Uow.SaveChanges();
            }
            TMS_Course_Member_Remark entity_remark = _repoCourseMemberRemark.Get(a => a.TraineeId == model.Member_Id && a.CourseId == courseId);
            if (entity_remark != null)
            {
                _repoCourseMemberRemark.Delete(entity_remark);
                Uow.SaveChanges();
            }
        }

        public void UpdateLmsAssgin(int traineeId, int courseId)
        {


            var course = RepoCourse.Get(courseId);
            var courseDetails = course.Course_Detail.Where(a => a.IsDeleted != true).ToList();
            if (courseDetails.Count > 0)
            {
                foreach (var detail in courseDetails)
                {
                    var detailId = detail.Id;
                    var entity =
                _repoCourseMember.Get(
                    a =>
                        a.Course_Details_Id == detailId && a.Member_Id == traineeId);
                    if (entity != null)
                    {
                        entity.Member_Id = traineeId;
                        entity.Course_Details_Id = detailId;
                        entity.IsDelete = false;
                        entity.AssignBy = CurrentUser.USER_ID.ToString();
                        entity.Status = (int)UtilConstants.APIAssign.Approved;
                        //subject available
                        entity.IsActive = true;
                        //update lms status
                        entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;

                        _repoCourseMember.Update(entity);
                        //UpdateStatusTraineeHistoryItem(traineeId, detailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
                    }
                    else
                    {
                        entity = new TMS_Course_Member();
                        entity.Member_Id = traineeId;
                        entity.Course_Details_Id = detailId;
                        entity.IsDelete = false;
                        entity.Status = (int)UtilConstants.APIAssign.Approved;
                        entity.AssignBy = CurrentUser.USER_ID.ToString();

                        //subject not available
                        entity.IsActive = false;
                        //update lms status
                        entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;

                        _repoCourseMember.Insert(entity);
                        //UpdateStatusTraineeHistoryItem(traineeId, detailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
                    }



                }

                RegistTraineeToCourse(traineeId, courseId);
                Uow.SaveChanges();
            }

        }

        private void UpdateStatusTraineeHistoryItem(int traineeId, int courseDetaiId, int type)
        {
            var subjectId = _repoCourseDetail.Get(a => a.Id == courseDetaiId).SubjectDetailId;
            var entity =
                _repoTraineeHistoryItem.GetAll(a => a.SubjectId == subjectId && a.TraineeHistory.Trainee_Id == traineeId).OrderByDescending(a => a.Id).FirstOrDefault();
            if (entity == null) return;
            entity.Status = type;
            entity.TraineeHistory.LmsStatus = statusModify;
            _repoTraineeHistoryItem.Update(entity);
            Uow.SaveChanges();
        }
        //TODO: Chua xai toi
        private void RemoveStatusTraineeHistoryItem(int traineeId, int courseDetaiId)
        {
            var subjectId = _repoCourseDetail.Get(a => a.Id == courseDetaiId).SubjectDetailId;
            var entity =
                _repoTraineeHistoryItem.GetAll(a => a.SubjectId == subjectId && a.TraineeHistory.Trainee_Id == traineeId).OrderByDescending(a => a.Id).FirstOrDefault();
            if (entity == null) return;
            entity.Status = (int)UtilConstants.StatusTraineeHistory.Missing;
            _repoTraineeHistoryItem.Update(entity);
            Uow.SaveChanges();
        }

        private void RegistTraineeToCourse(int traineeId, int courseId)
        {
            var entity =
                    _repoCourseResultFinal.Get(
                        a => a.traineeid == traineeId && a.courseid == courseId);

            if (entity == null)
            {
                _repoCourseResultFinal.Insert(new Course_Result_Final()
                {
                    traineeid = traineeId,
                    courseid = courseId,
                    createby = CurrentUser?.USER_ID.ToString(),
                    IsDeleted = false,
                    createday = DateTime.Now,
                    LmsStatus = statusAddNewTMS,
                    MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Pending
                });

            }
            else
            {
                entity.IsDeleted = false;
                entity.LmsStatus = statusAddNewTMS;
                entity.MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Pending;
                _repoCourseResultFinal.Update(entity);
            }
            Uow.SaveChanges();

        }


        public void Insert(Course entity)
        {
            RepoCourse.Insert(entity);
            Uow.SaveChanges();
        }


        public void InsertSentMail(TMS_SentEmail entity)
        {
            _repoSentMail.Insert(entity);
            Uow.SaveChanges();
        }



        public IQueryable<Course_Result> GetCourseResult(int traineeId)
        {
            return _repoCourseResult.GetAll(a => a.TraineeId == traineeId);
        }
        public Course_Result GetCourseResult(int? traineeId, int? courseDetailId)
        {
            return _repoCourseResult.GetAll(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).OrderByDescending(a => a.Id).FirstOrDefault();
        }
        public IQueryable<Course_Result> GetCourseResult(Expression<Func<Course_Result, bool>> query = null)
        {
            var entities = _repoCourseResult.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IEnumerable<Course_Result> GetCourseResult_huy(Expression<Func<Course_Result, bool>> query = null)
        {
            var entities = _repoCourseResult.GetAll(query).ToList();
            return entities;
        }
        public IQueryable<CourseRemarkCheckFail> GetCourseResultCheckFail(Expression<Func<CourseRemarkCheckFail, bool>> query = null)
        {
            var entities = _repoCourseRemarkCheckFail.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Subject_Score> GetScores(Expression<Func<Subject_Score, bool>> query = null)
        {
            var entities = _repoSubjectScore.GetAll(a => a.SubjectDetail.IsDelete == false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }



        public Course_Result GetCourseResultById(int courseResultId)
        {
            return _repoCourseResult.Get(courseResultId);
        }

        public void UpdateCourseResult(Course_Result entity)
        {
            _repoCourseResult.Update(entity);
            Uow.SaveChanges();
        }
        public void UpdateCourseResultCheckFail(CourseRemarkCheckFail entity)
        {
            _repoCourseRemarkCheckFail.Update(entity);
            Uow.SaveChanges();
        }
        public void InsertCourseResult(Course_Result entity)
        {
            _repoCourseResult.Insert(entity);
            Uow.SaveChanges();
        }
        public void InsertCourseResultCheckFail(CourseRemarkCheckFail entity)
        {
            _repoCourseRemarkCheckFail.Insert(entity);
            Uow.SaveChanges();
        }
        public IQueryable<Course_Result_Final> GetCourseResultFinal(Expression<Func<Course_Result_Final, bool>> query = null, int[] approveType = null, int? eStatus = null)
        {
            var entities = query == null ? _repoCourseResultFinal.GetAll(_finalDefaultfilter) : _repoCourseResultFinal.GetAll(_finalDefaultfilter).Where(query);

            if (approveType != null)
            {
                foreach (var item in approveType)
                {
                    var datastep = _repoPROCESS_Steps.Get(a => a.Step == item && a.IsActive == true);
                    entities = datastep != null ? entities.Where(a => a.Course.TMS_APPROVES.Any(b => b.int_Course_id == a.courseid && (eStatus == null || b.int_id_status == eStatus) && b.int_Type == item)) : entities;
                }
            }

            return entities;
        }

        public Course_Result_Final GetCourseResultFinalById(int? id)
        {
            return _repoCourseResultFinal.Get(id);
        }
        public CourseRemarkCheckFail GetCourseRemarkCheckFailById(int? id)
        {
            return _repoCourseRemarkCheckFail.Get(id);
        }
        public Course_Detail_Room_Global GetCourseDetailGlobalById(int id)
        {
            return _repoCourseDetailRoomGlobal.Get(id);
        }
        public void UpdateCourseResultFinal(Course_Result_Final entity)
        {
            _repoCourseResultFinal.Update(entity);
            Uow.SaveChanges();
        }

        public Course_Result_Final UpdateCourseResultFinalReturnEntity(Course_Result_Final entity)
        {
            _repoCourseResultFinal.Update(entity);
            Uow.SaveChanges();
            return entity;
        }
        public TMS_CertificateApproved ModifyTMSCertificateAppovedEntity(TMS_CertificateApproved entity)
        {
            if (entity.ID > 0)
            {
                _repoTMSCertificateAppoved.Update(entity);
            }
            else
            {
                _repoTMSCertificateAppoved.Insert(entity);
            }

            Uow.SaveChanges();
            return entity;
        }
        public void InsertCourseResultFinal(Course_Result_Final entity)
        {
            _repoCourseResultFinal.Insert(entity);
            Uow.SaveChanges();
        }

        public List<Course_Type> GetCourseTypes()
        {
            return _repoCourseType.GetAll().ToList();
        }

        public List<Course_Type_Result> GetCourseTypeResult()
        {
            return _repoCourseTypeResult.GetAll().ToList();
        }

        //Nationality
        public Nation GetNationById(int? id)
        {
            return _repoNation.Get(id);
        }

        public IQueryable<Nation> GetNation(Expression<Func<Nation, bool>> query = null)
        {
            var entities = _repoNation.GetAll(a => a.bit_Deleted == false && a.bit_Deleted == false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateNation(Nation entity)
        {
            _repoNation.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertNation(Nation entity)
        {
            _repoNation.Insert(entity);
            Uow.SaveChanges();
        }
        public void ModifyNation(Nation entity)
        {
            if (entity.id == 0)
            {
                _repoNation.Insert(entity);
            }
            else
            {
                _repoNation.Update(entity);
            }
            Uow.SaveChanges();
        }
        public void ModifyCourseDetailRoom(Course_Detail_Room entity)
        {
            if (entity.Id == 0)
            {
                _repoCourseDetailRoom.Insert(entity);
            }
            else
            {
                _repoCourseDetailRoom.Update(entity);
            }
            Uow.SaveChanges();
        }
        public void Update(NationModels model)
        {
            var entity = _repoNation.Get(model.Id);
            if (entity == null)
            {
                throw new Exception("data is not found");
            }
            entity.Nation_Code = model.Code;
            entity.Nation_Name = model.Name;
            entity.description = model.Description;
            entity.createday = DateTime.Now;
            entity.createuser = CurrentUser.USER_ID;
            entity.isactive = model.isActive;
            _repoNation.Update(entity);

            Uow.SaveChanges();


        }

        public void InsertNation(NationModels model)
        {
            var entity = new Nation
            {
                Nation_Code = model.Code,
                Nation_Name = model.Name,
                description = model.Description,
                createday = DateTime.Now,
                createuser = CurrentUser.USER_ID,
                isactive = model.isActive
            };

            _repoNation.Insert(entity);

            Uow.SaveChanges();

        }

        //Company
        public Company GetCompanyById(int? id)
        {
            return _repoCompany.Get(id);
        }

        public IQueryable<Company> GetCompany(Expression<Func<Company, bool>> query = null)
        {
            var entities = _repoCompany.GetAll(a => a.bit_Deleted == false && a.IsActive == true);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateCompany(Company entity)
        {
            _repoCompany.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertCompany(Company entity)
        {
            _repoCompany.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdateCourseDetailRoomGlobal(Course_Detail_Room_Global entity)
        {
            _repoCourseDetailRoomGlobal.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertCourseDetailRoomGlobal(Course_Detail_Room_Global entity)
        {
            _repoCourseDetailRoomGlobal.Insert(entity);
            Uow.SaveChanges();
        }
        public void ModifyCompany(Company entity)
        {
            if (entity.Company_Id == 0)
            {
                _repoCompany.Insert(entity);
            }
            else
            {
                _repoCompany.Update(entity);
            }
            Uow.SaveChanges();
        }
        public void Update(CompanyModels model)
        {
            var entity = _repoCompany.Get(model.Id);
            if (entity == null)
            {
                throw new Exception(Messege.NO_DATA);
            }
            entity.str_code = model.Code;
            entity.str_Name = model.Name;
            entity.dicsription = model.Description;
            entity.dtm_Modified_At = DateTime.Now;
            entity.str_Modified_By = CurrentUser.USER_ID.ToString();

            _repoCompany.Update(entity);
            Uow.SaveChanges();

        }

        public void InsertCompany(CompanyModels model)
        {
            var entity = new Company
            {
                str_code = model.Code,
                str_Name = model.Name,
                dicsription = model.Description,
                dtm_Created_At = DateTime.Now,
                str_Created_By = CurrentUser.USER_ID.ToString(),
                bit_Deleted = false
            };

            _repoCompany.Insert(entity);
            Uow.SaveChanges();
        }

        //Room
        public Room GetRoomById(int? id)
        {
            return _repoRoom.Get(id);
        }

        public IQueryable<Room> GetRoom(Expression<Func<Room, bool>> query = null)
        {
            var entities = _repoRoom.GetAll(a => a.bit_Deleted == false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Room> GetRoom_Course(Expression<Func<Room, bool>> query = null)
        {
            var entities = _repoRoom.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Room_Type> GetRoomType(Expression<Func<Room_Type, bool>> query = null)
        {
            var entities = _repoRoomType.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Management_Room_Item> GetManagement_Room_Item(Expression<Func<Management_Room_Item, bool>> query = null)
        {
            var entities = _repoManagement_Room_Item.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Course_Detail_Room> GetCourseDetailRooms(Expression<Func<Course_Detail_Room, bool>> query = null)
        {
            var entities = _repoCourseDetailRoom.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Course_Detail_Room_Global> GetCourseDetailRoomsGlobal(Expression<Func<Course_Detail_Room_Global, bool>> query = null)
        {
            var entities = _repoCourseDetailRoomGlobal.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public void UpdateRoom(Room entity)
        {
            _repoRoom.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertRoom(Room entity)
        {
            _repoRoom.Insert(entity);
            Uow.SaveChanges();
        }
        public void ModifyRoom(Room entity)
        {
            if (entity.Room_Id == 0)
            {
                _repoRoom.Insert(entity);
            }
            else
            {
                _repoRoom.Update(entity);
            }
            Uow.SaveChanges();
        }


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

        public Course ModifyReturnModel(CourseModifyModel model)
        {
            Course entity;
            CreateOrUpdateCourse(model, out entity);
            AddOrUpdateCourseDetail(ref entity, model.CourseDetailModels, model.Id);

            Uow.SaveChanges();


            var lang = GetCulture();
            var _entity = RepoCourse.GetAll(a => a.Id == entity.Id).Select(a => new
            {
                a.Id,
                a.Name,
                a.Code,
                a.Venue,
                a.NumberOfTrainee,
                Partner = a.Company.str_Name,
                a.Survey,
                a.StartDate,
                a.EndDate,
                a.TypeResult,
                a.Note,
                Department = a.Course_TrainingCenter.Select(c => c.Department.Name),
                Subject = a.Course_Detail.Select(c => new
                {
                    coursedetailId = c.Id,
                    SubjectName = c.SubjectDetail.Name,
                    SubjectId = c.SubjectDetail.Id,
                    c.type_leaning,
                    Instructor = c.Course_Detail_Instructor.Select(b => new
                    {
                        Course_Detail_Instructor = b.Id,
                        Instructor = lang == "vi" ? b.Trainee.FirstName + " " + b.Trainee.LastName : b.Trainee.LastName + " " + b.Trainee.FirstName,
                        //Instructor = UtilConstants.ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName),
                        b.Allowance
                    }),

                    c.bit_Regisable,
                    c.dtm_time_from,
                    c.dtm_time_to,
                    c.time_from,
                    c.time_to,
                    c.Commitment,
                    c.Allowance,
                    c.AttemptsAllowed,
                    c.CommitmetExpiredate,
                    c.GradingMethod
                })
            });
            LogEvent(UtilConstants.LogType.EVENT_TYPE_INFORMATION, UtilConstants.LogSourse.Course, model.Id == null ? UtilConstants.LogEvent.Insert : UtilConstants.LogEvent.Update, _entity);
            return entity;
        }

        private bool IsNumberAllowed(string text)
        {

            return Regex.IsMatch(text, @"^[0-9]+$");
        }
        private void CreateOrUpdateCourse(CourseViewModel model, out Course entity)
        {
            var now = DateTime.Now;
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            if (model.MinTranineeMembers >= model.MaxTranineeMembers)
            {
                var minmax = string.Format(Messege.WARNING_MIN_MAX_TRAINEE, model.MinTranineeMembers,
                    model.MaxTranineeMembers);
                throw new Exception(minmax);
            }
            var duplicateMessage = string.Format(Messege.DataIsExists, Resource.lblCourseCode, model.Code);
            var courseCode = RepoCourse.Get(a => a.Code.ToLower().Trim() == model.Code.ToLower().Trim() && a.Id != model.Id && a.IsDeleted == false);
            if (courseCode != null)
            {
                throw new Exception(duplicateMessage);
            }
            if (model.DepartmentIds[0] == 0)
            {
                throw new Exception(Messege.VALIDATION_COURSE_DEPARTMENT);
            }
            if (model.BeginDate > model.EndDate)
            {
                throw new Exception(Messege.VALIDATION_COURSE_FROM_THAN_TO);
            }
            var dtm_enddate = model.EndDate;
            if (model.Id == null)
            {
                entity = new Course()
                {
                    CourseTypeId = model.CourseType,
                    CompanyId = model.PartnerId,
                    CreatedDate = now,
                    CreatedBy = CurrentUser.USER_ID.ToString(),
                    EndDate = dtm_enddate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    StartDate = model.BeginDate,
                    IsActive = true,
                    IsDeleted = false,
                    LMSStatus = statusAddNewTMS,
                    GroupSubjectId = model.GroupSubjectId,
                    NumberOfTrainee = model.MaxTranineeMembers,
                    MinTrainee = model.MinTranineeMembers,
                    Survey = model.Survey.HasValue && model.Survey.Value == 1,
                    CustomerType = model.Customer.HasValue && model.Customer.Value == 1,
                    Note = model.Note,
                    //Name = model.GroupSubjectId.HasValue ==false && string.IsNullOrEmpty(model.Name) ? string.Empty : model.GroupSubjectId.HasValue == false ? model.Name.Trim() : string.IsNullOrEmpty(model.Name) ? _repoGroupSubject.Get(model.GroupSubjectId)?.Name :  _repoGroupSubject.Get(model.GroupSubjectId)?.Name + " - " + model.Name.Trim(),
                    LastName = model.Name,
                    Code = model.Code,
                    Venue = model.Venue,
                    IsPublic = model.IsPublic == (int)UtilConstants.BoolEnum.Yes ? true : false,
                    IsBindSubject = model.IsBindSubject == (int)UtilConstants.BoolEnum.Yes ? true : false,
                    MaxGrade = model.MaxGrade
                };
                string namecourse = "";
                if (model.GroupSubjectId.HasValue)
                {
                    var datanamecourse = model.GroupSubjectId.HasValue ? _repoGroupSubject.Get(model.GroupSubjectId) : null;
                    if (datanamecourse != null)
                    {
                        namecourse = datanamecourse.Name + (!string.IsNullOrEmpty(model.Name) ? " - " + model.Name.Trim() : model.Name);
                    }

                }
                else
                {
                    if (model.CourseDetailModels != null || model.CourseDetailModels.Any())
                    {
                        if (model.CourseDetailModels.Count() - 1 > 1)
                        {
                            namecourse = !string.IsNullOrEmpty(model.Name) ? model.Name.Trim() : model.Name;
                        }
                        else
                        {
                            var subject = model.CourseDetailModels.FirstOrDefault();
                            var name = _repoSubjectDetail.Get(subject?.SubjectId);
                            namecourse = name.Name + (!string.IsNullOrEmpty(model.Name) ? " - " + model.Name.Trim() : model.Name);
                        }
                    }
                }

                entity.Name = namecourse;

                if (model.ProgramCost != null)
                {
                    var programCost = new TrainingProgam_Cost { Cost = model.ProgramCost, Course = entity };
                    entity.TrainingProgam_Cost.Add(programCost);
                }
                foreach (var departmentId in model.DepartmentIds)
                {
                    entity.Course_TrainingCenter.Add(new Course_TrainingCenter()
                    {
                        Course = entity,
                        khoidaotao_id = departmentId,
                    });
                }
                if (model.IsBindSubject == (int)UtilConstants.BoolEnum.Yes)
                {
                    if (model.BindToSubject != null)
                    {
                        foreach (var SubjectId in model.BindToSubject)
                        {
                            entity.Course_Subject_Item.Add(new Course_Subject_Item()
                            {
                                Course = entity,
                                SubjectId = SubjectId,
                            });
                        }
                    }
                }


                RepoCourse.Insert(entity);

            }
            else
            {
                entity = RepoCourse.Get(model.Id);
                if (entity == null)
                {
                    throw new Exception(string.Format(Messege.COURSE_IS_NOT_FOUND, Resource.lblCourse, model.Code));
                }
                entity.Code = model.Code;
                entity.CourseTypeId = model.CourseType;
                entity.CompanyId = model.PartnerId;
                entity.EndDate = dtm_enddate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                entity.StartDate = model.BeginDate;
                entity.GroupSubjectId = model.GroupSubjectId;
                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                entity.ModifiedDate = now;
                entity.NumberOfTrainee = model.MaxTranineeMembers;
                entity.MinTrainee = model.MinTranineeMembers;
                entity.Survey = model.Survey.HasValue && model.Survey.Value == 1;
                entity.CustomerType = model.Customer.HasValue && model.Customer.Value == 1;
                entity.Note = model.Note;
                entity.IsPublic = model.IsPublic == (int)UtilConstants.BoolEnum.Yes ? true : false;
                entity.IsBindSubject = model.IsBindSubject == (int)UtilConstants.BoolEnum.Yes ? true : false;
                entity.MaxGrade = model.MaxGrade;
                //var groupName = model.GroupSubjectId.HasValue ? _repoGroupSubject.Get(model.GroupSubjectId).Name.Trim() : string.Empty;
                //var newName = string.IsNullOrEmpty(model.Name) ? string.Empty : model.Name.Trim();
                //entity.Name = string.IsNullOrEmpty(groupName) && string.IsNullOrEmpty(newName) ? string.Empty : string.IsNullOrEmpty(groupName) ? newName.Trim() : string.IsNullOrEmpty(newName) ? groupName : groupName + " - " + newName.Trim();

                string namecourse = "";
                if (model.GroupSubjectId.HasValue)
                {
                    var datanamecourse = model.GroupSubjectId.HasValue ? _repoGroupSubject.Get(model.GroupSubjectId) : null;
                    if (datanamecourse != null)
                    {
                        namecourse = datanamecourse.Name + (!string.IsNullOrEmpty(model.Name) ? " - " + model.Name.Trim() : model.Name);
                    }

                }
                else
                {
                    if (model.CourseDetailModels != null || model.CourseDetailModels.Any())
                    {
                        if (model.CourseDetailModels.Count() - 1 > 1)
                        {
                            namecourse = !string.IsNullOrEmpty(model.Name) ? model.Name.Trim() : model.Name;
                        }
                        else
                        {
                            var subject = model.CourseDetailModels.FirstOrDefault();
                            var name = _repoSubjectDetail.Get(subject?.SubjectId);
                            namecourse = name.Name + (!string.IsNullOrEmpty(model.Name) ? " - " + model.Name.Trim() : model.Name);
                        }
                    }
                }

                entity.Name = namecourse;
                entity.LastName = !string.IsNullOrEmpty(model.Name) ? model.Name.Trim() : model.Name;
                // var dataName = entity.Name.Split(new [] { UtilConstants.SplitNameProgram } , StringSplitOptions.None).Last().Trim();
                var dataName = entity.LastName;
                //if (string.IsNullOrEmpty(dataName) || !dataName.Equals(newName))
                //{
                //    entity.Name = string.IsNullOrEmpty(groupName) && string.IsNullOrEmpty(newName) ? string.Empty : string.IsNullOrEmpty(groupName) ? newName.Trim() : string.IsNullOrEmpty(newName) ?  groupName : groupName + " - " + newName.Trim();
                //    entity.LastName = model.Name;
                //}

                //khong cho chinh sua Code
                //entity.Code = model.Code;
                entity.Venue = model.Venue;
                // trạng thái : 1 : chua gui qua lms , 0 : gui qua lms
                entity.LMSStatus = statusAddNewTMS;
                var courseDepartments = entity.Course_TrainingCenter.ToList();
                //remove departments
                foreach (var department in courseDepartments)
                {
                    _repoCouseTrainingCenter.Delete(department);
                }
                //add departments
                foreach (var departmentId in model.DepartmentIds)//.Where(a => courseDepartments.All(x => x.khoidaotao_id != a)))
                {
                    entity.Course_TrainingCenter.Add(new Course_TrainingCenter()
                    {
                        Course = entity,
                        khoidaotao_id = departmentId,
                    });
                }
                if (model.ProgramCost != null && model.ProgramCost != entity.TrainingProgam_Cost?.LastOrDefault()?.Cost)
                {
                    var programCost = new TrainingProgam_Cost { Cost = model.ProgramCost, Course = entity };
                    entity.TrainingProgam_Cost.Add(programCost);
                }
                if (model.IsBindSubject == (int)UtilConstants.BoolEnum.Yes)
                {
                    if (model.BindToSubject != null)
                    {
                        var groupItem = entity.Course_Subject_Item.ToList();
                        foreach (var SubjectId in groupItem?.Where(a => model.BindToSubject.All(x => x != a.SubjectId)))
                        {
                            _repoCourseSubjectitem.Delete(SubjectId);
                        }
                        //add groups
                        foreach (var SubjectId in model.BindToSubject.Where(a => groupItem.All(x => x.SubjectId != a)))
                        {
                            entity.Course_Subject_Item.Add(new Course_Subject_Item()
                            {
                                Course = entity,
                                SubjectId = SubjectId,
                            });
                        }
                    }
                    else
                    {
                        var groupItem = entity.Course_Subject_Item.ToList();
                        if (groupItem.Count() != 0)
                        {
                            foreach (var SubjectId in groupItem)
                            {
                                _repoCourseSubjectitem.Delete(SubjectId);
                            }
                        }
                    }
                }
                else
                {
                    var groupItem = entity.Course_Subject_Item.ToList();
                    if (groupItem.Count() != 0)
                    {
                        foreach (var SubjectId in groupItem)
                        {
                            _repoCourseSubjectitem.Delete(SubjectId);
                        }
                    }
                }
                RepoCourse.Update(entity);

            }
        }


        private void CheckInstructorIsExist(DateTime? from, DateTime? to, string time_from, string time_to, int? courseDetailId, IEnumerable<int> instructorIds, Course entity, int? idCourse)
        {
            if (from.HasValue && to.HasValue)
            {
                var courseDetails = _repoCourseDetail.GetAll(a => a.Id != courseDetailId && a.IsDeleted == false &&
                    (from >= a.dtm_time_from && to <= a.dtm_time_to) && !a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.CourseResult && b.int_id_status == (int)UtilConstants.EStatus.Approve));
                if (instructorIds.Any() || instructorIds != null)
                {
                    foreach (var instructorid in instructorIds)
                    {
                        var timeFrom = DateTime.ParseExact(time_from, "HH:mm", CultureInfo.InvariantCulture);
                        var timeTo = DateTime.ParseExact(time_to, "HH:mm", CultureInfo.InvariantCulture);
                        var checkInstructor = courseDetails.Where(a => a.Course_Detail_Instructor.Any(b => b.Instructor_Id == instructorid && b.Type == (int)UtilConstants.TypeInstructor.Instructor));
                        if (checkInstructor.Any())
                        {
                            foreach (var courseDetail in checkInstructor)
                            {
                                if (!string.IsNullOrEmpty(courseDetail.time_from) || !string.IsNullOrEmpty(courseDetail.time_to))
                                {
                                    var timeFromConvert = DateTime.ParseExact(courseDetail.time_from, "HH:mm", CultureInfo.InvariantCulture);
                                    var timeToConvert = DateTime.ParseExact(courseDetail.time_to, "HH:mm", CultureInfo.InvariantCulture);
                                    if ((timeFromConvert >= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert >= timeTo && timeTo <= timeToConvert))
                                    {
                                        var model = _repoTrainee.Get(instructorid);
                                        var fullName = model.str_Staff_Id + " - " + ReturnDisplayLanguage(model.FirstName, model.LastName);
                                        var dateCourseDetail = from + " - " + to;
                                        var timeCourseDetail = time_from + " - " + time_to;
                                        var message = string.Format(Messege.CHECK_INSTRUCTOR_EXIST, fullName,
                                            dateCourseDetail, timeCourseDetail);
                                        if (!idCourse.HasValue)
                                        {
                                            RemoveEntity(entity);
                                        }
                                        throw new Exception(message);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        private void RemoveEntity(Course entity)
        {
            if (entity.Course_TrainingCenter.Any())
            {
                _repoCouseTrainingCenter.Delete(entity.Course_TrainingCenter);
                entity.Course_TrainingCenter.Clear();
            }

            if (entity.Course_Detail.Any())
            {
                _repoCourseDetail.Delete(entity.Course_Detail);
                entity.Course_Detail.Clear();
            }
            //RepoCourse.Unchanged(entity);
            RepoCourse.Delete(entity);

        }
        //không gọi đc bên BaseAdmin
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
        private void AddOrUpdateCourseDetail(ref Course entity, IEnumerable<CourseDetailModel> courseDetails, int? idCourse)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
            var now = DateTime.Now;
            foreach (var removedCourseDetail in entity.Course_Detail.Where(a => a.IsDeleted == false && courseDetails.All(x => x.Id != a.Id)))
            {
                removedCourseDetail.IsDeleted = true;
                removedCourseDetail.IsActive = false;
                removedCourseDetail.str_Deleted_By = CurrentUser.USER_ID.ToString();
                removedCourseDetail.dtm_Deleted_At = now;
                removedCourseDetail.str_Modified_By = CurrentUser.USER_ID.ToString();
                removedCourseDetail.dtm_Modified_At = now;
                removedCourseDetail.LmsStatus = statusAddNewTMS;
            }
            if (courseDetails == null)
            {
                if (!idCourse.HasValue)
                {
                    RemoveEntity(entity);
                }
                throw new Exception(string.Empty + Messege.VALIDATION_COURSE_GROUPCOURSE);
            }
            foreach (var courseDetail in courseDetails)
            {
                if (!string.IsNullOrEmpty(courseDetail.dtm_time_from))
                {
                    courseDetail.DateFrom = DateTime.Parse(courseDetail.dtm_time_from, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }
                if (!string.IsNullOrEmpty(courseDetail.dtm_time_from))
                {
                    courseDetail.DateTo = DateTime.Parse(courseDetail.dtm_time_to, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }


                var courseError = _repoSubjectDetail.Get(a => a.Id == courseDetail.SubjectId);
                var courseName = courseError.Code + " - " + courseError.Name;
                if (courseError.IsDelete == true /*|| courseError.IsActive == false*/) throw new Exception(string.Format(Messege.VALIDATE_SELECT_COURSE, courseName, Resource.lblSubject));

                if (courseDetail.SubjectInstructors == null)
                {
                    if (!idCourse.HasValue)
                    {
                        RemoveEntity(entity);
                    }
                    throw new Exception(string.Empty + Resource.lblSubject + " : " + courseName + Messege.VALIDATION_COURSEDETAIL_INSTRUCTOR);

                }
                //check ngay registryDate > fromdate
                //if (courseDetail.RegistryDate < courseDetail.DateFrom)
                //{
                //    throw new Exception(string.Format(Messege.VALIDATION_REGISTER_FROM_TO,courseName + "<br />",Resource.lblFromDate,Resource.lblRegistryDate));
                //}
                //if (courseDetail.RegistryDate > courseDetail.DateTo)
                //{
                //    throw new Exception(string.Format(Messege.VALIDATION_REGISTER_FROM_TO, courseName + "<br />", Resource.lblRegistryDate, Resource.lblToDate));
                //}
                //check trùng time giáo viên
                // var instructorId = courseDetail.SubjectInstructors?.Select(a => a.InstructorId);
                //CheckInstructorIsExist(courseDetail.DateFrom, courseDetail.DateTo, courseDetail.TimeFrom, courseDetail.TimeTo, courseDetail.Id, instructorId, entity, idCourse);
                //////////
                if (courseDetail.DateFrom > courseDetail.DateTo)
                {
                    if (!idCourse.HasValue)
                    {
                        RemoveEntity(entity);
                    }
                    throw new Exception(string.Empty + Resource.lblSubject + " : " + courseName + Messege.VALIDATION_DATE_FROM_TO);
                }
                if (courseDetail.Commitment == 1 && courseDetail.CommitmentExpiredate == null)
                {
                    if (!idCourse.HasValue)
                    {
                        RemoveEntity(entity);
                    }
                    throw new Exception(string.Empty + Resource.lblSubject + " : " + courseName + Resource.lblCommitmentPeriod + ":" + Messege.VALIDATION_COURSE_COMMITMENTEXPIREDATE);
                }
                if (courseDetail.Commitment == 1 && courseDetail.CommitmentExpiredate < 1)
                {
                    if (!idCourse.HasValue)
                    {
                        RemoveEntity(entity);
                    }
                    throw new Exception(string.Empty + Resource.lblSubject + " : " + courseName + Resource.lblCommitmentPeriod + " :" + Messege.VALIDATION_COURSE_LIMIT_COMMITMENTEXPIREDATE);
                }
                var subject = _repoSubjectDetail.Get(courseDetail.SubjectId);
                //var instructorIds = courseDetail.SubjectInstructors.Select(a => a.InstructorId);
                //var employeeAllowances = _repoTrainee.GetAll(a => instructorIds.Any(x => x == a.Id)).ToDictionary(a => a.Id, a => a.Allowance);

                var detail = entity.Course_Detail.FirstOrDefault(a => a.IsDeleted == false && a.IsActive == true && a.Id == courseDetail.Id);

                if (detail == null)
                {

                    detail = new Course_Detail()
                    {
                        LmsStatus = statusAddNewTMS,
                        IsDeleted = false,
                        IsActive = true,
                        Course = entity,
                        Duration = subject.Duration ?? 0,
                        SubjectDetailId = subject.Id,
                        bit_Regisable = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes,
                        dtm_time_from = Convert.ToDateTime(courseDetail.DateFrom),
                        dtm_time_to = Convert.ToDateTime(courseDetail.DateTo),
                        time_from = courseDetail.TimeFrom,
                        time_to = courseDetail.TimeTo,
                        type_leaning = courseDetail.LearningType,
                        dtm_Created_At = now,
                        str_Created_By = CurrentUser.USER_ID.ToString(),
                        RoomId = courseDetail.Room == -1 ? null : courseDetail.Room,
                        Commitment = courseDetail.Commitment == 1,
                        CommitmetExpiredate = courseDetail.Commitment == 0 ? 0 : courseDetail.CommitmentExpiredate,
                        AttemptsAllowed = courseDetail.Attempts,
                        GradingMethod = courseDetail.Grademethod,
                        Allowance = courseDetail.Allowance ?? 0,
                        //RegistryDate = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes ? Convert.ToDateTime(courseDetail.RegistryDate) : (DateTime?)null,
                        //ExpiryDate = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes ? Convert.ToDateTime(courseDetail.ExpiryDate) : (DateTime?)null,

                        //phan them moi
                        Time = (courseDetail.Time != null && IsNumberAllowed(courseDetail.Time.ToString()) && courseDetail.Time > 0) ? courseDetail.Time : 1,
                        TimeBlock = (courseDetail.TimeBlock != null && courseDetail.TimeBlock > 0) ? courseDetail.TimeBlock : 5,
                        mark_type = courseDetail.LearningType == (int)UtilConstants.LearningTypes.Online ? (int)UtilConstants.MarkTypes.Auto : courseDetail.MarkType,
                        str_remark = courseDetail.str_remark,
                    };

                    entity.Course_Detail.Add(detail);
                }
                else
                {
                    detail.IsDeleted = false;
                    detail.IsActive = true;
                    detail.Duration = subject.Duration ?? 0;
                    detail.LmsStatus = statusAddNewTMS;
                    detail.SubjectDetailId = subject.Id;
                    detail.bit_Regisable = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes;
                    detail.dtm_time_from = Convert.ToDateTime(courseDetail.DateFrom);
                    detail.dtm_time_to = Convert.ToDateTime(courseDetail.DateTo);
                    detail.time_from = courseDetail.TimeFrom;
                    detail.time_to = courseDetail.TimeTo;
                    detail.type_leaning = courseDetail.LearningType;
                    detail.dtm_Modified_At = now;
                    detail.str_Modified_By = CurrentUser.USER_ID.ToString();
                    detail.RoomId = courseDetail.Room == -1 ? null : courseDetail.Room;
                    detail.Commitment = courseDetail.Commitment == 1;
                    detail.CommitmetExpiredate = courseDetail.Commitment == 0 ? 0 : courseDetail.CommitmentExpiredate;
                    detail.AttemptsAllowed = courseDetail.Attempts;
                    detail.GradingMethod = courseDetail.Grademethod;
                    detail.Allowance = courseDetail.Allowance;
                    //detail.RegistryDate = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes
                    //    ? Convert.ToDateTime(courseDetail.RegistryDate)
                    //    : (DateTime?)null;
                    //detail.ExpiryDate = courseDetail.Registable == (int)UtilConstants.BoolEnum.Yes
                    //    ? Convert.ToDateTime(courseDetail.ExpiryDate)
                    //    : (DateTime?)null;
                    //phan them moi
                    detail.Time = (courseDetail.Time != null && IsNumberAllowed(courseDetail.Time.ToString()) && courseDetail.Time > 0) ? courseDetail.Time : 1;
                    detail.TimeBlock = (courseDetail.TimeBlock != null && courseDetail.TimeBlock > 0)
                        ? courseDetail.TimeBlock
                        : 5;
                    detail.mark_type = courseDetail.LearningType == (int)UtilConstants.LearningTypes.Online ? (int)UtilConstants.MarkTypes.Auto : courseDetail.MarkType;
                    detail.str_remark = courseDetail.str_remark;
                }

                var detailInstructors = detail.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).ToList();
                foreach (var instructor in detailInstructors)
                {
                    _repoCourseDetailInstructor.Delete(instructor);
                }
                foreach (var subjectInstructor in courseDetail.SubjectInstructors)
                {

                    var instructorAbility =
                        _repoInstructorAbility.Get(
                            a =>
                                a.SubjectDetailId == courseDetail.SubjectId &&
                                a.InstructorId == subjectInstructor.InstructorId)?.Allowance;

                    var allowance = instructorAbility != null ? Convert.ToDecimal(instructorAbility) : 0;
                    detail.Course_Detail_Instructor.Add(new Course_Detail_Instructor()
                    {
                        Allowance = allowance,
                        //Allowance = Convert.ToDecimal(employeeAllowances[subjectInstructor.InstructorId]),
                        Course = entity,
                        Instructor_Id = subjectInstructor.InstructorId,
                        Duration = subjectInstructor.Duration ?? 0,
                        CreateDate = now,
                        Course_Detail = detail,
                        Type = (int)UtilConstants.TypeInstructor.Instructor
                    });
                }
                if (courseDetail.Mentor != null)
                {

                    var teachingAssistants = detail.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor).ToList();
                    if (teachingAssistants.Any())
                    {
                        foreach (var support in teachingAssistants)
                        {
                            _repoCourseDetailInstructor.Delete(support);
                        }

                    }

                    //type monitor
                    if (courseDetail.Mentor != -1)
                    {
                        detail.Course_Detail_Instructor.Add(new Course_Detail_Instructor()
                        {
                            Course = entity,
                            Instructor_Id = courseDetail.Mentor,
                            CreateDate = now,
                            Course_Detail = detail,
                            Type = (int)UtilConstants.TypeInstructor.Mentor,
                            Allowance = courseDetail.MonitorAllowance ?? 0,
                            Duration = courseDetail.MonitorDuration ?? 0,
                        });
                    }


                }
                if (courseDetail.Hannah != null)
                {
                    var teachingAssistants = detail.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah).ToList();
                    if (teachingAssistants.Any())
                    {
                        foreach (var support in teachingAssistants)
                        {
                            _repoCourseDetailInstructor.Delete(support);
                        }

                    }
                    if (courseDetail.Hannah != -1)
                    {
                        detail.Course_Detail_Instructor.Add(new Course_Detail_Instructor()
                        {
                            Course = entity,
                            Instructor_Id = courseDetail.Hannah,
                            CreateDate = now,
                            Course_Detail = detail,
                            Type = (int)UtilConstants.TypeInstructor.Hannah,
                            Allowance = courseDetail.ExaminerAllowance ?? 0,
                            Duration = courseDetail.ExaminerDuration ?? 0,
                        });
                    }


                }
                if (courseDetail.LearningType == (int)UtilConstants.LearningTypes.OfflineOnline)
                {
                    var blendedLearning = detail.Course_Blended_Learning.Where(a => a.IsActive == true && a.IsDeleted == false).ToList();
                    if (blendedLearning.Any())
                    {
                        foreach (var item in blendedLearning)
                        {
                            item.IsDeleted = true;
                            item.IsActive = false;
                            item.ModifyBy = CurrentUser.USER_ID;
                            item.ModifyDate = DateTime.Now;
                            _repoblended.Update(item);
                        }
                    }
                    if (courseDetail.Blended != null)
                    {
                        foreach (var item in courseDetail.Blended)
                        {
                            item.DateFrom = DateTime.Parse(item.dtm_DateFrom_blend, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                            item.DateTo = DateTime.Parse(item.dtm_DateTo_blend, culture, System.Globalization.DateTimeStyles.AssumeLocal);

                            if (item.DateFrom.HasValue && item.DateTo.HasValue)
                            {
                                var dateFrom = item.DateFrom?.Date ?? null;
                                var dateTo = item.DateTo?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? null;
                                //var model = new Course_Blended_Learning();
                                //model.Course_Detail = detail;
                                //model.IsActive = true;
                                //model.IsDeleted = false;
                                //model.CreateBy = CurrentUser.USER_ID;
                                //model.CreationDate = DateTime.Now;
                                //model.DateFrom = dateFrom;
                                //model.DateTo = dateTo;
                                //model.LearningType = item.LearningTypeName;
                                //model.ExaminerId = item.ExaminerId;
                                //model.mark_type_cro = item.MarkTypecRo;

                                detail.Course_Blended_Learning.Add(new Course_Blended_Learning()
                                {
                                    Course_Detail = detail,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreateBy = CurrentUser.USER_ID,
                                    CreationDate = DateTime.Now,
                                    //RoomId = item.LearningTypeId == (int)UtilConstants.LearningTypes
                                    //    .Offline
                                    //    ? item.RoomId
                                    //    : null,
                                    RoomId = item.RoomId,
                                    DateFrom = dateFrom,
                                    DateTo = dateTo,
                                    LearningType = item.LearningTypeName,
                                    ExaminerId = item.ExaminerId != -1 ? item.ExaminerId : null,
                                    mark_type_cro = item.MarkTypecRo,
                                    Duration = item.BlendedDuration ?? 0,
                                    Allowance = item.BlendedAllowance,
                                });
                            }
                        }
                    }
                    else
                    {
                        var dateFrom = courseDetail.DateFrom?.Date ?? null;
                        var dateTo = courseDetail.DateTo?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? null;

                        detail.Course_Blended_Learning.Add(new Course_Blended_Learning()
                        {
                            Course_Detail = detail,
                            IsActive = true,
                            IsDeleted = false,
                            CreateBy = CurrentUser.USER_ID,
                            CreationDate = DateTime.Now,
                            RoomId = courseDetail.LearningType == (int)UtilConstants.LearningTypes.Offline ? courseDetail.Room : null,
                            DateFrom = dateFrom,
                            DateTo = dateTo,
                            LearningType = courseDetail.LearningType == (int)UtilConstants.LearningTypes.Offline ? "Classroom" : "Online",
                            mark_type_cro = courseDetail.MarkType,
                            Duration = subject.Duration ?? 0,
                        });
                    }
                }
            }

        }
        public List<sp_GetTrainingHeaderTV_Result> GetTrainingHeader(string listId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd, string status)
        {
            return Uow.sp_GetTrainingHeader_Result(listId, departmentCode, fromDateStart, fromDateEnd, toDateStart, toDateOfEnd, status).ToList();
        }

        public void Update(RoomModels model)
        {

            var entity = _repoRoom.Get(model.Id);
            if (entity == null)
            {
                throw new Exception("data is not found");
            }
            entity.str_code = model.Code;
            entity.str_Name = model.Name;
            entity.int_Capacity = model.Capacity;
            entity.str_Equipment = model.Equipment;
            entity.int_Area = model.Area;
            entity.str_Location = model.Location;
            entity.dtm_Modified_At = DateTime.Now;
            entity.str_Modified_By = CurrentUser.USER_ID.ToString();
            entity.is_Meeting = model.Is_Meeting;
            entity.RoomTypeId = model.Is_Meeting;
            _repoRoom.Update(entity);
            Uow.SaveChanges();

        }

        public void InsertRoom(RoomModels model)
        {
            var entity = new Room
            {
                str_code = model.Code,
                str_Name = model.Name,
                int_Capacity = model.Capacity,
                str_Equipment = model.Equipment,
                int_Area = model.Area,
                str_Location = model.Location,
                dtm_Created_At = DateTime.Now,
                str_Created_By = CurrentUser.USER_ID.ToString(),
                RoomTypeId = model.Is_Meeting,
                is_Meeting = model.Is_Meeting,
                isActive = true,
                isDeleted = false,

            };

            _repoRoom.Insert(entity);
            Uow.SaveChanges();
        }

        public void ModifyRoom(List<RoomModels> models)
        {
            try
            {
                foreach (var item in models)
                {
                    if (item.Id != null)
                    {
                        var entity = _repoRoom.Get(item.Id);
                        if (entity == null)
                        {
                            throw new Exception("data is not found");
                        }
                        if (_repoRoom.GetAll(a => a.str_code.ToLower() == item.Code.ToLower() && a.Room_Id != item.Id).Any())
                        {
                            throw new Exception(Messege.EXISTING_CODE);
                            //return Json(new { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            throw new Exception(Messege.VALIDATION_NAME);
                        }
                        entity.str_code = item.Code;
                        entity.str_Name = item.Name;
                        entity.int_Capacity = item.Capacity;
                        entity.str_Equipment = item.Equipment;
                        entity.int_Area = item.Area;
                        entity.str_Location = item.Location;
                        entity.dtm_Modified_At = DateTime.Now;
                        entity.str_Modified_By = CurrentUser.USER_ID.ToString();
                        entity.is_Meeting = item.Is_Meeting;
                        entity.RoomTypeId = item.Is_Meeting;
                        _repoRoom.Update(entity);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            throw new Exception(Messege.VALIDATION_NAME);
                        }
                        if (string.IsNullOrEmpty(item.Code))
                        {
                            throw new Exception(Messege.VALIDATION_CODE);
                        }
                        var entity = new Room
                        {
                            str_code = item.Code,
                            str_Name = item.Name,
                            int_Capacity = item.Capacity,
                            str_Equipment = item.Equipment,
                            int_Area = item.Area,
                            str_Location = item.Location,
                            dtm_Created_At = DateTime.Now,
                            str_Created_By = CurrentUser.USER_ID.ToString(),
                            is_Meeting = item.Is_Meeting,
                            RoomTypeId = item.Is_Meeting,
                            isActive = true,
                            isDeleted = false,

                        };
                        _repoRoom.Insert(entity);
                    }
                }
                Uow.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }




        }
        public void Update(Course_Result_Summary entity)
        {
            _repoCourseResultSummary.Update(entity);
            Uow.SaveChanges();
        }
        public IQueryable<Course_Result_Summary> GetCourseResultSummaries(Expression<Func<Course_Result_Summary, bool>> query = null, bool? NotApproval = null)
        {
            var entities = _repoCourseResultSummary.GetAll();
            if (query != null) entities = entities.Where(query);

            if (NotApproval == null)
            {
                var datastep = _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ApproveType.SubjectResult && a.IsActive == true);
                entities = datastep != null ? entities.Where(a => a.Course_Detail.TMS_APPROVES.Any(b => b.int_Course_id == a.Course_Detail.CourseId && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult)) : entities;
            }

            return entities;
        }
        public Course_Result_Summary GetCourseResultSummary(int? traineeId, int? courseDetailId)
        {
            return _repoCourseResultSummary.Get(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId);
        }

        public IQueryable<TraineeHistory_Item> GetHistoryItems(Expression<Func<TraineeHistory_Item, bool>> query = null)
        {
            var entities = _repoTraineeHistoryItem.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void Update(TraineeHistory_Item entity)
        {
            _repoTraineeHistoryItem.Update(entity);
            Uow.SaveChanges();
        }

        #region Crouse_LMS_STATUS

        public Course_LMS_STATUS GetCourseLms(int courseid, UtilConstants.LMSStatus steps)
        {
            var entity = _repoCourseLmsStatus.Get(a => a.CourseId == courseid && a.Steps == (int)steps);

            return entity;
        }

        public IQueryable<Course_LMS_STATUS> GetCourseLmsApi(Expression<Func<Course_LMS_STATUS, bool>> query = null)
        {
            var entities = _repoCourseLmsStatus.GetAll();
            if (query != null)
                entities = entities.Where(query);
            return entities;
        }

        public void InsertCourseLmsStatus(Course_LMS_STATUS model)
        {

            _repoCourseLmsStatus.Insert(model);
            Uow.SaveChanges();
        }

        public void UpdateCourseLmsStatus(Course_LMS_STATUS model)
        {

            _repoCourseLmsStatus.Update(model);
            Uow.SaveChanges();
        }


        public void UpdateCourseLmsStatusApi(Course_LMS_STATUS model)
        {

            _repoCourseLmsStatus.Update(model);

        }
        #endregion
        #region API LMS 
        public IQueryable<LMS_Assign> GetLmsAssign(Expression<Func<LMS_Assign, bool>> query)
        {
            var entities = _repoLmsAssign.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public bool ApplyAssignTrainee(APIAssignLMS[] model, string currentUser)
        {
            try
            {
                if (model == null || !model.Any()) return false;
                foreach (var item in model)
                {
                    var trainee =
                       _repoTrainee.Get(
                           a => a.str_Staff_Id.ToLower().Trim() == item.TraineeCode.ToLower().Trim() && a.IsDeleted == false);
                    if (trainee == null) continue;
                    var traineeId = trainee.Id;

                    var courseDetail =
                           _repoCourseDetail.Get(a => a.Id == item.CourseDetailId &&
                              a.Course.IsDeleted == false &&
                              a.SubjectDetail.IsDelete == false);
                    if (courseDetail == null) continue;
                    var courseDetailLmsId = courseDetail.Id;
                    //var course = RepoCourse.Get(a => a.Code.ToLower().Trim() == item.CourseCode.ToLower().Trim());
                    var course = courseDetail.Course;
                    if (course == null) continue;

                    var checkStepAssign =
                        _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ProcessStep.AssignedTrainee && a.IsActive == false);
                    if (checkStepAssign != null)
                    {
                        Modify(course, UtilConstants.ApproveType.AssignedTrainee, UtilConstants.EStatus.Approve, UtilConstants.ActionType.Approve);
                    }
                    var maxTrainee = course.NumberOfTrainee;

                    var courseDetails = course.Course_Detail;

                    if (courseDetails.Any())
                    {
                        foreach (var detail in courseDetails)
                        {
                            var recommend = detail.bit_Regisable == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No;

                            var countTmsMember = detail.TMS_Course_Member.Count(a => a.IsDelete == false);
                            if (recommend == (int)UtilConstants.BoolEnum.Yes && countTmsMember >= maxTrainee)
                                return false;

                            var courseDetailId = detail.Id;
                            var entity = _repoCourseMember.Get(
                                a => a.Course_Details_Id == courseDetailId && a.Member_Id == traineeId);
                            if (entity != null)
                            {
                                entity.Member_Id = traineeId;
                                entity.Course_Details_Id = courseDetailId;
                                entity.IsDelete = false;

                                entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                                entity.AssignBy = currentUser;
                                entity.IsActive = courseDetailLmsId == courseDetailId;
                                entity.Status = (int)UtilConstants.ApiStatus.Synchronize;
                                _repoCourseMember.Update(entity);
                            }
                            else
                            {
                                entity = new TMS_Course_Member();
                                entity.Member_Id = traineeId;
                                entity.Course_Details_Id = courseDetailId;
                                entity.IsDelete = false;
                                entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                                entity.AssignBy = currentUser;
                                entity.Status = (int)UtilConstants.ApiStatus.Synchronize;
                                entity.IsActive = courseDetailLmsId == courseDetailId;
                                entity.Status = (int)UtilConstants.ApiStatus.Synchronize;
                                _repoCourseMember.Insert(entity);
                            }

                            var courseId = (int)detail.CourseId;
                            RegistTraineeToCourse(traineeId, courseId);
                            //UpdateStatusTraineeHistoryItem(traineeId, item.CourseDetailId, (int)UtilConstants.StatusTraineeHistory.Trainning);
                        }
                    }
                }
                Uow.SaveChanges();



                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //hàm này của API
        private void Modify(Course course, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus, UtilConstants.ActionType actionType, int? courseDetailId = -1, string note = null)
        {
            var now = DateTime.Now;
            var step = (int)approveType;
            var approve =
               _repoTmsApprove.Get(
                   a =>
                       a.int_Type == step && a.int_Course_id == course.Id);
            if (approve != null)
            {
                approve.Course = course;
                approve.int_Requested_by = CurrentUser?.USER_ID;
                approve.Date_Requested = now;
                approve.int_id_status = (int)approveStatus;
                approve.int_Type = step;
                approve.dtm_requested_date = now;

                //ngày approve (thêm mới vào 23/03/2018)
                approve.Date_Approved = now;
                //end

                //chỉnh sửa nội dung thì cập nhật to LMS
                if (step == (int)UtilConstants.ApproveType.Course && (approveStatus != UtilConstants.EStatus.Pending /*|| approveStatus != UtilConstants.EStatus.Reject*/))
                {
                    approve.Course.LMSStatus = (int)UtilConstants.ApiStatus.Modify;
                    //RepoCourse.Update(approve.Course);
                }
                _repoTmsApprove.Update(approve);
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
                    Date_Approved = now,
                    //int_Approve_by = approver != null ? approver.UserId : userModel.USER_ID,
                    Course = course
                };

                //chỉnh sửa nội dung thì cập nhật to LMS
                if (step == (int)UtilConstants.ApproveType.Course && (approveStatus != UtilConstants.EStatus.Pending /*|| approveStatus != UtilConstants.EStatus.Reject*/))
                {
                    approve.Course.LMSStatus = (int)UtilConstants.ApiStatus.Modify;
                    //RepoCourse.Update(approve.Course);
                }
                _repoTmsApprove.Insert(approve);
            }
            if (step == (int)UtilConstants.ApproveType.AssignedTrainee &&
                approveStatus == UtilConstants.EStatus.Approve)
            {
                var courseDetails = course.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                foreach (var member in courseDetails.Select(detail => detail.TMS_Course_Member.Where(a => a.IsDelete == false).ToList()).SelectMany(members => members))
                {
                    member.LmsStatus = statusModify;
                }
            }
            Uow.SaveChanges();

            var approveHistory = new TMS_APPROVES_HISTORY
            {

                approves_id = approve.id,
                int_id_status = (int)approveStatus,
                int_Type = step,
                int_Course_id = course.Id,
                // int_Approve_by = approver != null ? approver.UserId : CurrentUser.USER_ID,
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
            TMS_APPROVES_LOG_.int_course_detail_id = courseDetailId != -1 ? courseDetailId : null;
            _repoTmsApproveLog.Insert(TMS_APPROVES_LOG_);

            Uow.SaveChanges();

        }
        //Log này của API
        private string Log(int? typelog, Course couse, int? courseDetailId = -1, string cancelRequestContent = null)
        {
            StringBuilder Html_ = new StringBuilder();
            var culture = GetCulture();
            #region[log assigntrainee]
            if (typelog == 2 && couse != null)
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


            Html_.AppendFormat("<b>by: <em>{0}</em></b>", CurrentUser?.FirstName + " " + CurrentUser?.LastName);
            if (!string.IsNullOrEmpty(cancelRequestContent))
            {
                Html_.AppendFormat("<br /><b>Content : <em>{0}</em></b>", cancelRequestContent);
            }
            return Html_.ToString();
        }
        public bool UpdateFlagLMS(APIFlagLMS[] model)
        {
            try
            {
                if (model == null || !model.Any()) return false;
                foreach (var item in model)
                {
                    if (item.Lms != null && item.Lms.Any())
                    {
                        switch (item.TenHam.ToLower().Trim())
                        {
                            case "getlistemployee":
                                #region [GetListEmployee]
                                foreach (var item2 in item.Lms)
                                {
                                    var trainee =
                                        _repoTrainee.Get(
                                            a =>
                                            a.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    if (trainee == null) continue;
                                    trainee.LmsStatus = statusIsSync;
                                    _repoTrainee.Update(trainee);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            //getlistcourse ThaiVietjet
                            //getlistprogram new
                            case "getlistprogram":
                                #region [GetListCourse]
                                foreach (var item2 in item.Lms)
                                {
                                    var course = RepoCourse.Get(a =>
                                        a.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim());
                                    if (course == null) continue;
                                    course.LMSStatus = statusIsSync;
                                    RepoCourse.Update(course);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            //getlistsubject ThaiVietjet
                            //getlistcourseofprogram new
                            case "getlistcourseofprogram":
                                #region [GetListSubject]
                                foreach (var item2 in item.Lms)
                                {
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code == item2.CourseCode &&
                                    //            a.SubjectDetail.Code == item2.SubjectCode);
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted == false);
                                    if (courseDetail == null) continue;
                                    courseDetail.LmsStatus = statusIsSync;
                                    _repoCourseDetail.Update(courseDetail);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getassigntrainee":
                                #region ["GetAssignTrainee"]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted != true);
                                    if (courseDetail == null) continue;
                                    var tmsCourseMember =
                                        _repoCourseMember.Get(
                                            a =>
                                                a.Course_Details_Id == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    tmsCourseMember.LmsStatus = statusIsSync;
                                    _repoCourseMember.Update(tmsCourseMember);

                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getcourseresultsumary":
                                #region [GetCourseResultSumary]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted == false);
                                    if (courseDetail == null) continue;
                                    var courseResultSummary =
                                        _repoCourseResultSummary.Get(
                                            a =>
                                                a.CourseDetailId == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    courseResultSummary.LmsStatus = statusIsSync;

                                    _repoCourseResultSummary.Update(courseResultSummary);
                                }
                                _unitOfWork.SaveChanges();

                                break;
                            #endregion
                            case "getcertificate":
                                #region [GetCertificate]
                                foreach (var item2 in item.Lms)
                                {
                                    var course =
                                        RepoCourse.Get(a => a.IsDeleted == false && a.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim());
                                    if (course == null) continue;
                                    var courseId = course.Id;
                                    var courseResulFinal =
                                        _repoCourseResultFinal.Get(
                                            a => a.IsDeleted == false &&
                                                a.courseid == courseId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() ==
                                                item2.TraineeCode.ToLower().Trim());
                                    courseResulFinal.LmsStatus = statusIsSync;
                                    _repoCourseResultFinal.Update(courseResulFinal);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getlistcategories":
                                #region [GetListCategories]

                                foreach (var item2 in item.Lms)
                                {
                                    var groupSubject =
                                        _repoGroupSubject.Get(
                                            a =>
                                            a.Code.ToLower().Trim() == item2.GroupCode.ToLower().Trim());
                                    if (groupSubject == null) continue;
                                    groupSubject.LmsStatus = statusIsSync;
                                    _repoGroupSubject.Update(groupSubject);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion

                            #region [---------------------------------an neu la ThaiVietjet----------------------------------------------]

                            case "getjobtitle":
                                #region [GetJobTitle]

                                foreach (var item2 in item.Lms)
                                {
                                    var job =
                                        _repoJobTitle.Get(
                                            a => a.Code.ToLower().Trim() == item2.JobtitleCode.ToLower().Trim());
                                    if (job != null)
                                    {
                                        job.LmsStatus = statusIsSync;
                                        _repoJobTitle.Update(job);
                                    }
                                }
                                _unitOfWork.SaveChanges();

                                break;
                            #endregion
                            case "gettraineehistory":
                                #region [GetJobTitle]

                                foreach (var item2 in item.Lms)
                                {
                                    var final =
                                        _repoCourseResultFinal.GetAll(
                                            a =>
                                                a.Trainee.str_Staff_Id.ToLower().Trim() ==
                                                item2.TraineeCode.ToLower().Trim()).ToList();
                                    foreach (var courseResultFinal in final)
                                    {
                                        courseResultFinal.LmsStatus = statusIsSync;
                                        _repoCourseResultFinal.Update(courseResultFinal);
                                    }
                                    _unitOfWork.SaveChanges();
                                }
                                break;
                            #endregion
                            case "getlistsubject":
                                #region [GetListSubject]

                                foreach (var item2 in item.Lms)
                                {
                                    var subjectDetail =
                                        _repoSubjectDetail.Get(a => a.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    if (subjectDetail == null) continue;
                                    subjectDetail.LmsStatus = statusIsSync;
                                    _repoSubjectDetail.Update(subjectDetail);
                                    _unitOfWork.SaveChanges();
                                }
                                break;
                            #endregion
                            #endregion
                            default:
                                return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        public int UpdateFlagLMSReturnInt(APIFlagLMS[] model, string currentUser)
        {
            try
            {
                if (model == null || !model.Any()) return 0;
                foreach (var item in model)
                {
                    if (item.Lms != null && item.Lms.Any())
                    {
                        switch (item.TenHam.ToLower().Trim())
                        {
                            case "getlistemployee":
                                #region [GetListEmployee]
                                foreach (var item2 in item.Lms)
                                {
                                    var trainee =
                                        _repoTrainee.Get(
                                            a =>
                                            a.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    if (trainee == null) continue;
                                    trainee.LmsStatus = statusIsSync;
                                    _repoTrainee.Update(trainee);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            //getlistcourse ThaiVietjet
                            //getlistprogram new
                            case "getlistprogram":
                                #region [GetListCourse]
                                foreach (var item2 in item.Lms)
                                {
                                    var course = RepoCourse.Get(a =>
                                        a.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim());
                                    if (course == null) continue;
                                    course.LMSStatus = statusIsSync;
                                    RepoCourse.Update(course);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            //getlistsubject ThaiVietjet
                            //getlistcourseofprogram new
                            case "getlistcourseofprogram":
                                #region [GetListSubject]
                                foreach (var item2 in item.Lms)
                                {
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code == item2.CourseCode &&
                                    //            a.SubjectDetail.Code == item2.SubjectCode);
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId);
                                    if (courseDetail == null) continue;
                                    courseDetail.LmsStatus = statusIsSync;
                                    _repoCourseDetail.Update(courseDetail);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getassigntrainee":
                                #region ["GetAssignTrainee"]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted != true);
                                    if (courseDetail == null) continue;
                                    var tmsCourseMember =
                                        _repoCourseMember.Get(
                                            a =>
                                                a.Course_Details_Id == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    tmsCourseMember.LmsStatus = statusIsSync;
                                    _repoCourseMember.Update(tmsCourseMember);

                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getcourseresultsumary":
                                #region [GetCourseResultSumary]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted == false);
                                    if (courseDetail == null) continue;
                                    var courseResultSummary =
                                        _repoCourseResultSummary.Get(
                                            a =>
                                                a.CourseDetailId == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    courseResultSummary.LmsStatus = statusIsSync;

                                    _repoCourseResultSummary.Update(courseResultSummary);
                                }
                                _unitOfWork.SaveChanges();

                                break;
                            #endregion
                            case "getcertificate":
                                #region [GetCertificate]
                                foreach (var item2 in item.Lms)
                                {
                                    var course =
                                        RepoCourse.Get(a => a.IsDeleted == false && a.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim());
                                    if (course == null) continue;
                                    var courseId = course.Id;
                                    var courseResulFinal =
                                        _repoCourseResultFinal.Get(
                                            a => a.IsDeleted == false &&
                                                a.courseid == courseId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() ==
                                                item2.TraineeCode.ToLower().Trim());
                                    courseResulFinal.LmsStatus = statusIsSync;
                                    _repoCourseResultFinal.Update(courseResulFinal);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion
                            case "getlistcategories":
                                #region [GetListCategories]

                                foreach (var item2 in item.Lms)
                                {
                                    var groupSubject =
                                        _repoGroupSubject.Get(
                                            a =>
                                            a.Code.ToLower().Trim() == item2.GroupCode.ToLower().Trim());
                                    if (groupSubject == null) continue;
                                    groupSubject.LmsStatus = statusIsSync;
                                    _repoGroupSubject.Update(groupSubject);
                                }
                                _unitOfWork.SaveChanges();
                                break;
                            #endregion

                            #region [---------------------------------an neu la ThaiVietjet----------------------------------------------]

                            case "getjobtitle":
                                #region [GetJobTitle]

                                foreach (var item2 in item.Lms)
                                {
                                    var job =
                                        _repoJobTitle.Get(
                                            a => a.Code.ToLower().Trim() == item2.JobtitleCode.ToLower().Trim());
                                    if (job != null)
                                    {
                                        job.LmsStatus = statusIsSync;
                                        _repoJobTitle.Update(job);
                                    }
                                }
                                _unitOfWork.SaveChanges();

                                break;
                            #endregion
                            case "gettraineehistory":
                                #region [GetTraineeHistory]

                                foreach (var item2 in item.Lms)
                                {
                                    var id = int.Parse(item2.TraineeHistoryId);
                                    var traineeHistory = _repoTraineeHistory.Get(id);
                                    if (traineeHistory != null)
                                    {
                                        traineeHistory.LmsStatus = statusIsSync;
                                        _repoTraineeHistory.Update(traineeHistory);
                                        Uow.SaveChanges();
                                    }
                                }
                                break;
                            #endregion
                            case "gettraineepdp":
                                #region [GetTraineePDP]

                                foreach (var item2 in item.Lms)
                                {
                                    var traineeFuture = _repoTraineeTraineeFuture.GetAll(a =>
                                        a.JobTitle.Code.Trim().ToLower() == item2.JobtitleCode.Trim().ToLower() &&
                                        a.Trainee.str_Staff_Id.Trim().ToLower() == item2.TraineeCode.Trim().ToLower() &&
                                        a.Status == (int)UtilConstants.StatusApiApprove.Approved).OrderByDescending(a => a.Id).FirstOrDefault();

                                    if (traineeFuture != null)
                                    {
                                        traineeFuture.LmsStatus = statusIsSync;
                                        _repoTraineeTraineeFuture.Update(traineeFuture);
                                    }
                                    Uow.SaveChanges();
                                }
                                break;
                            #endregion
                            case "getlistsubject":
                                #region [GetListSubject]

                                foreach (var item2 in item.Lms)
                                {
                                    var subjectDetail =
                                        _repoSubjectDetail.Get(a => a.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    if (subjectDetail == null) continue;
                                    subjectDetail.LmsStatus = statusIsSync;
                                    _repoSubjectDetail.Update(subjectDetail);
                                    _unitOfWork.SaveChanges();
                                }
                                break;
                            #endregion

                            #endregion
                            default:
                                return 0;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public APIReturn UpdateFlagLMSReturnModel(APIFlagLMS[] model, string currentUser)
        {
            var returnModel = new APIReturn();
            var total = 0;
            returnModel.Status = 0;
            returnModel.Count = total;
            if (model == null || !model.Any()) return returnModel;
            try
            {


                foreach (var item in model)
                {
                    if (item.Lms != null && item.Lms.Any())
                    {
                        switch (item.TenHam.ToLower().Trim())
                        {
                            case "getlistemployee":
                                #region [GetListEmployee]
                                foreach (var item2 in item.Lms)
                                {
                                    var trainee =
                                        _repoTrainee.Get(
                                            a => a.LmsStatus != 4 &&
                                            a.str_Staff_Id.ToLower() == item2.TraineeCode.ToLower().Trim());
                                    if (trainee == null) continue;
                                    trainee.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;

                                    _repoTrainee.Update(trainee);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoTrainee.GetAll(a => (a.LmsStatus == statusModify || a.LmsStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            //getlistcourse ThaiVietjet
                            //getlistprogram new
                            case "getlistprogram":
                                #region [GetListCourse]
                                foreach (var item2 in item.Lms)
                                {
                                    var courseId = int.Parse(item2.CourseId);
                                    var course = RepoCourse.Get(a =>
                                        a.Id == courseId);
                                    if (course == null) continue;
                                    course.LMSStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    RepoCourse.Update(course);
                                }
                                _unitOfWork.SaveChanges();

                                total = RepoCourse.GetAll(a => (a.LMSStatus == statusModify || a.LMSStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            //getlistsubject ThaiVietjet
                            //getlistcourseofprogram new
                            case "getlistcourseofprogram":
                                #region [GetListSubject]
                                foreach (var item2 in item.Lms)
                                {
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code == item2.CourseCode &&
                                    //            a.SubjectDetail.Code == item2.SubjectCode);
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId);
                                    if (courseDetail == null) continue;
                                    courseDetail.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoCourseDetail.Update(courseDetail);
                                }

                                _unitOfWork.SaveChanges();

                                total = _repoCourseDetail.GetAll(a =>
                                (a.LmsStatus == statusModify || a.LmsStatus == null) && a.Course.LMSStatus == statusIsSync && a.Course.IsDeleted == false && a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getassigntrainee":
                                #region ["GetAssignTrainee"]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted != true);
                                    if (courseDetail == null) continue;
                                    var tmsCourseMember =
                                        _repoCourseMember.Get(
                                            a =>
                                                a.Course_Details_Id == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() == item2.TraineeCode.ToLower().Trim());
                                    if (tmsCourseMember == null) continue;
                                    tmsCourseMember.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoCourseMember.Update(tmsCourseMember);

                                }
                                _unitOfWork.SaveChanges();


                                total = _repoCourseMember.GetAll(a => (a.LmsStatus == statusModify || a.LmsStatus == null) &&
                                                                      ((a.Trainee.LmsStatus == statusIsSync || a.Trainee.LmsStatus == null) &&
                                                                       a.Trainee.IsDeleted != true) &&
                                                                      (a.Course_Detail.LmsStatus == statusIsSync &&
                                                                       a.Course_Detail.IsDeleted != true) &&
                                                                      a.Course_Detail.Course.IsDeleted != true).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getcourseresultsumary":
                                #region [GetCourseResultSumary]
                                foreach (var item2 in item.Lms)
                                {
                                    //Course_Detail_Id
                                    //var courseDetail =
                                    //    _repoCourseDetail.Get(
                                    //        a => !a.IsDeleted &&
                                    //            a.Course.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim() &&
                                    //            a.SubjectDetail.Code.ToLower().Trim() == item2.SubjectCode.ToLower().Trim());
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var courseDetail = _repoCourseDetail.Get(a => a.Id == courseDetailId && a.IsDeleted == false && a.LmsStatus == statusIsSync);
                                    if (courseDetail == null) continue;
                                    var courseResultSummary =
                                        _repoCourseResultSummary.Get(
                                            a =>
                                                a.CourseDetailId == courseDetailId &&
                                                a.Trainee.str_Staff_Id.ToLower() == item2.TraineeCode.ToLower().Trim());
                                    courseResultSummary.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;

                                    _repoCourseResultSummary.Update(courseResultSummary);
                                }
                                _unitOfWork.SaveChanges();


                                total = _repoCourseResultSummary.GetAll(a => a.Course_Detail.IsDeleted == false && a.Course_Detail.IsActive == true &&
                            (a.LmsStatus == statusModify || a.LmsStatus == null) && a.Trainee.LmsStatus == statusIsSync &&
                            a.Trainee.IsDeleted == false).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getcertificate":
                                #region [GetCertificate]
                                foreach (var item2 in item.Lms)
                                {
                                    var course =
                                        RepoCourse.Get(a => a.IsDeleted == false && a.Code.ToLower().Trim() == item2.CourseCode.ToLower().Trim());
                                    if (course == null) continue;
                                    var courseId = course.Id;
                                    var courseResulFinal =
                                        _repoCourseResultFinal.Get(
                                            a => a.IsDeleted == false &&
                                                a.courseid == courseId &&
                                                a.Trainee.str_Staff_Id.ToLower().Trim() ==
                                                item2.TraineeCode.ToLower().Trim());
                                    courseResulFinal.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoCourseResultFinal.Update(courseResulFinal);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoCourseResultFinal.GetAll(a => a.LmsStatus == statusModify && a.Course.LMSStatus == statusIsSync &&
                            a.Course.IsDeleted == false && a.Trainee.LmsStatus == statusIsSync && a.Trainee.IsDeleted == false).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getlistcategories":
                                #region [GetListCategories]

                                foreach (var item2 in item.Lms)
                                {
                                    var groupSubject =
                                        _repoGroupSubject.Get(
                                            a =>
                                            a.Code.ToLower().Trim() == item2.GroupCode.ToLower().Trim());
                                    if (groupSubject == null) continue;
                                    groupSubject.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoGroupSubject.Update(groupSubject);
                                }
                                _unitOfWork.SaveChanges();


                                total = _repoCatGroupSubject.GetAll(a => (a.LmsStatus == statusModify || a.LmsStatus == null) && a.CAT_GROUPSUBJECT_ITEM.Any()).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getallpostnews":
                                #region [GetPostNews]

                                foreach (var item2 in item.Lms)
                                {
                                    var postNew =
                                        _repoPostNew.Get(item2.Id);
                                    if (postNew == null) continue;
                                    postNew.LMSStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoPostNew.Update(postNew);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoPostNew.GetAll(a => (a.LMSStatus == statusModify || a.LMSStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getallcategorypostnews":
                                #region [GetCategoryPostNews]

                                foreach (var item2 in item.Lms)
                                {
                                    var postNew =
                                        _repoPostNewCategory.Get(item2.Id);
                                    if (postNew == null) continue;
                                    postNew.LMSStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoPostNewCategory.Update(postNew);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoPostNewCategory.GetAll(a => (a.LMSStatus == statusModify || a.LMSStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getsurveyglobal":
                                #region [GetSurveyGlobal]

                                foreach (var item2 in item.Lms)
                                {
                                    var survey =
                                        _repoSurvey.Get(item2.Id);
                                    if (survey == null) continue;
                                    survey.LMSStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoSurvey.Update(survey);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoPostNewCategory.GetAll(a => (a.LMSStatus == statusModify || a.LMSStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getcertificatecourse":
                                #region [GetCertificateCourse]

                                foreach (var item2 in item.Lms)
                                {
                                    var courseDetailId = int.Parse(item2.CourseDetailId);
                                    var survey =
                                        _repoCourseResult.Get(a => a.CourseDetailId == courseDetailId && a.Trainee.str_Staff_Id.Equals(item2.TraineeCode));
                                    if (survey == null) continue;
                                    survey.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoCourseResult.Update(survey);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoCourseResult.GetAll(a => (a.LmsStatus == statusModify || a.LmsStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getcertificatecategory":
                                #region [GetCertificateCategory]

                                foreach (var item2 in item.Lms)
                                {
                                    var courseId = int.Parse(item2.CourseId);
                                    var survey =
                                        _repoCourseResultFinal.Get(a => a.courseid == courseId && a.Trainee.str_Staff_Id.Equals(item2.TraineeCode));
                                    if (survey == null) continue;
                                    survey.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoCourseResultFinal.Update(survey);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoCourseResultFinal.GetAll(a => (a.LmsStatus == statusModify || a.LmsStatus == null)).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            #region [---------------------------------an neu la ThaiVietjet----------------------------------------------]

                            case "getjobtitle":
                                #region [GetJobTitle]

                                foreach (var item2 in item.Lms)
                                {
                                    var job =
                                        _repoJobTitle.Get(
                                            a => a.Code.ToLower().Trim() == item2.JobtitleCode.ToLower().Trim());
                                    if (job == null) continue;
                                    job.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoJobTitle.Update(job);
                                }
                                _unitOfWork.SaveChanges();

                                total = _repoJobTitle.GetAll(a => a.LmsStatus == null || a.LmsStatus == statusModify).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "gettraineehistory":
                                #region [GetTraineeHistory]

                                foreach (var item2 in item.Lms)
                                {
                                    var id = int.Parse(item2.TraineeHistoryId);
                                    var traineeHistory = _repoTraineeHistory.Get(id);
                                    if (traineeHistory == null) continue;
                                    traineeHistory.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoTraineeHistory.Update(traineeHistory);
                                    Uow.SaveChanges();
                                }


                                total = _repoTraineeHistory.GetAll(a => a.LmsStatus == null || a.LmsStatus == statusModify).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;

                                break;
                            #endregion
                            case "gettraineepdp":
                                #region [GetTraineePDP]

                                foreach (var item2 in item.Lms)
                                {
                                    var traineeFuture = _repoTraineeTraineeFuture.GetAll(a =>
                                        a.JobTitle.Code.Trim().ToLower() == item2.JobtitleCode.Trim().ToLower() &&
                                        a.Trainee.str_Staff_Id.Trim().ToLower() == item2.TraineeCode.Trim().ToLower() &&
                                        a.Status == (int)UtilConstants.StatusApiApprove.Approved).OrderByDescending(a => a.Id).FirstOrDefault();

                                    if (traineeFuture != null)
                                    {
                                        traineeFuture.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                        _repoTraineeTraineeFuture.Update(traineeFuture);
                                    }
                                    Uow.SaveChanges();
                                }

                                total = _repoTraineeTraineeFuture.GetAll(a => a.LmsStatus == null || a.LmsStatus == statusModify).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getlistsubject":
                                #region [GetListSubject]

                                foreach (var item2 in item.Lms)
                                {
                                    var subjectDetail =
                                        _repoSubjectDetail.Get(a => a.Id == item2.SubjectId);
                                    if (subjectDetail == null) continue;
                                    subjectDetail.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoSubjectDetail.Update(subjectDetail);
                                    _unitOfWork.SaveChanges();
                                }


                                total = _repoSubjectDetail.GetAll(a => a.LmsStatus == null || a.LmsStatus == statusModify).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            case "getlistdepartment":
                                #region [GetListDepartment]

                                foreach (var item2 in item.Lms)
                                {
                                    var department =
                                        _repoDepartment.Get(a => a.Code.ToLower().Trim() == item2.DepartmentCode.ToLower().Trim());
                                    if (department == null) continue;
                                    department.LmsStatus = item2.Status == (int)UtilConstants.ApiStatus.UnSuccessfully
                                        ? (int)UtilConstants.ApiStatus.UnSuccessfully
                                        : statusIsSync;
                                    _repoDepartment.Update(department);
                                    _unitOfWork.SaveChanges();
                                }
                                total = _repoDepartment.GetAll(a => a.LmsStatus == null || a.LmsStatus == statusModify).Count();
                                returnModel.Status = 1;
                                returnModel.Count = total;
                                break;
                            #endregion
                            #endregion
                            default:
                                return returnModel;
                        }
                    }
                }
                return returnModel;
            }
            catch (Exception ex)
            {

                return returnModel;
            }
        }
        #endregion

        #region [---------------------------- My PDP-------------------------------------]
        public TraineeFuture GetTraineeFutureById(int? id)
        {
            return _repoTraineeTraineeFuture.Get(id);
        }

        public void UpdateTraineeFuture(TraineeFuture entity)
        {
            _repoTraineeTraineeFuture.Update(entity);
            Uow.SaveChanges();
        }
        public int InsertTraineeFuture(APITraineeFuture[] model, string currenUser)
        {
            try
            {
                if (model == null || !model.Any()) return 0;
                foreach (var item in model)
                {
                    if (string.IsNullOrEmpty(item.TraineeCode)) return 0;
                    if (string.IsNullOrEmpty(item.JobTitleCode)) return 0;


                    var traineeFuture =
                        _repoTraineeTraineeFuture.Get(
                            a => a.JobTitle.Code.Trim().ToLower() == item.JobTitleCode.Trim().ToLower() &&
                                 a.Trainee.str_Staff_Id.Trim().ToLower() == item.TraineeCode.Trim().ToLower() &&
                                 a.Status == (int)UtilConstants.StatusApiApprove.Pending);
                    if (traineeFuture != null)
                    {
                    }
                    else
                    {
                        var trainee =
                       _repoTrainee.Get(a => a.str_Staff_Id.Trim().ToLower() == item.TraineeCode.Trim().ToLower());
                        if (trainee == null) return 0;

                        var jobTitle = _repoJobTitle.Get(a => a.Code.Trim().ToLower() == item.JobTitleCode.Trim().ToLower());
                        if (jobTitle == null) return 0;

                        var entity = new TraineeFuture();
                        entity.Trainee = trainee;
                        entity.JobTitle = jobTitle;
                        entity.CreationDate = DateTime.Now;
                        entity.CreateBy = currenUser;
                        entity.Status = (int)UtilConstants.StatusApiApprove.Pending;
                        entity.LmsStatus = statusIsSync;
                        entity.Schedule = (int)UtilConstants.StatusApiApprove.Pending;
                        _repoTraineeTraineeFuture.Insert(entity);
                    }

                }
                _unitOfWork.SaveChanges();
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }
        }
        #endregion
        #region [------------Điểm danh--------Attendance-----------------------]
        public Course_Attendance GetTraineeAttendance(Expression<Func<Course_Attendance, bool>> query)
        {
            var entities = _repoCourseAttendance.Get(query);
            return entities;
        }
        public IQueryable<Course_Attendance> GetAllTraineeAttendance(Expression<Func<Course_Attendance, bool>> query)
        {
            var entities = _repoCourseAttendance.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void InsertAttendance(Course_Attendance entity)
        {
            _repoCourseAttendance.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdateAttendance(Course_Attendance entity)
        {
            _repoCourseAttendance.Update(entity);
            Uow.SaveChanges();
        }
        #endregion
        #region [------------------------CAT_CERTIFICATE--------------------------------]

        public CAT_CERTIFICATE GetCatCertificateById(int? id)
        {
            return _repoCatCertificate.Get(id);
        }
        public IQueryable<CAT_CERTIFICATE> GetCatCertificates(Expression<Func<CAT_CERTIFICATE, bool>> query)
        {
            var entities = _repoCatCertificate.GetAll(a => a.IsDelete == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        #endregion
        #region [Meeting Room Management]

        public Meeting GetMeetingById(int? id)
        {
            return _repoMeeting.Get(id);
        }

        public IQueryable<Meeting> GetMeeting(Expression<Func<Meeting, bool>> query = null)
        {
            var entities = _repoMeeting.GetAll(a => a.IsDeleted == false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateMeeting(Meeting entity)
        {
            _repoMeeting.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertMeeting(Meeting entity)
        {
            _repoMeeting.Insert(entity);
            Uow.SaveChanges();
        }

        public void ModifyMeeting(Meeting entity)
        {
            if (entity.id == 0)
            {
                _repoMeeting.Insert(entity);
            }
            else
            {
                _repoMeeting.Update(entity);
            }
            Uow.SaveChanges();
        }

        public Meeting UpdateMeeting(MeetingModels model)
        {
            var entity = _repoMeeting.Get(model.Id);
            if (entity == null)
            {
                throw new Exception("data is not found");
            }
            if (model.StartDate > model.EndDate)
            {
                throw new Exception(@Messege.VALIDATION_COURSE_FROM_THAN_TO);
            }
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.EndDate = model.EndDate;
            entity.StartDate = model.StartDate;
            entity.TimeFrom = model.TimeFrom;
            entity.TimeTo = model.TimeTo;
            entity.RoomID = model.RoomID;
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = CurrentUser.USER_ID;

            _repoMeeting.Update(entity);

            foreach (var item in model.ListParticipant)
            {
                int id_par = int.Parse(item.ToString());
                var _entity = new Meeting_Participants();
                _entity.Meeting_Id = entity.id;
                _entity.Participant_Id = id_par;
                _repoMeetingParticipant.Insert(_entity);
            }

            Uow.SaveChanges();
            return entity;



        }

        public Meeting InsertMeeting(MeetingModels model)
        {
            if (model.StartDate > model.EndDate)
            {
                throw new Exception(@Messege.VALIDATION_COURSE_FROM_THAN_TO);
            }
            var entity = new Meeting
            {
                Name = model.Name,
                Description = model.Description,
                EndDate = model.EndDate,
                StartDate = model.StartDate,
                TimeFrom = model.TimeFrom,
                TimeTo = model.TimeTo,
                RoomID = model.RoomID,
                CreatedDate = DateTime.Now,
                CreatedBy = CurrentUser.USER_ID,
                IsActive = true,
                IsDeleted = false,
            };
            _repoMeeting.Insert(entity);

            foreach (var item in model.ListParticipant)
            {
                int id_par = int.Parse(item.ToString());
                var _entity = new Meeting_Participants();
                _entity.Meeting_Id = entity.id;
                _entity.Participant_Id = id_par;
                _entity.IsActive = true;
                _entity.IsDeleted = false;
                _repoMeetingParticipant.Insert(_entity);
            }

            Uow.SaveChanges();
            return entity;
        }

        public Meeting_Participants GetParticipantById(int? id)
        {
            return _repoMeetingParticipant.Get(id);
        }

        public IQueryable<Meeting_Participants> GetMeetingParticipants(Expression<Func<Meeting_Participants, bool>> query = null)
        {
            var entities = _repoMeetingParticipant.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateMeetingParticipant(Meeting_Participants entity)
        {
            _repoMeetingParticipant.Update(entity);
            Uow.SaveChanges();
        }

        #endregion
        #region [Certificate for course]

        public IQueryable<Group_Certificate> GetAllGroupCertificate(
            Expression<Func<Group_Certificate, bool>> query = null)
        {
            var entity = query == null ? _repoGroupCertificate.GetAll(_groupCertificateDefaultfilter) : _repoGroupCertificate.GetAll(_groupCertificateDefaultfilter).Where(query);

            return entity;
        }

        public IQueryable<Group_Certificate_Schedule> GetAllGroupCertificateSchedule(
            Expression<Func<Group_Certificate_Schedule, bool>> query = null)
        {
            var entity = query == null ? _repoGroupCertificateSchedule.GetAll(_groupCertificateScheduleDefaultfilter) : _repoGroupCertificateSchedule.GetAll(_groupCertificateScheduleDefaultfilter).Where(query);

            return entity;
        }
        public IQueryable<TMS_CertificateApproved> GetAllGroupCertificateApprove(
            Expression<Func<TMS_CertificateApproved, bool>> query = null)
        {
            var entity = query == null ? _repoTMSCertificateAppoved.GetAll(a => a.IsDeleted != true) : _repoTMSCertificateAppoved.GetAll(a => a.IsDeleted != true).Where(query);

            return entity;
        }
        public Group_Certificate GetGroupCertificateById(int? id)
        {
            return _repoGroupCertificate.Get(id);
        }
        public void UpdateGroupCertificate(Group_Certificate entity)
        {
            _repoGroupCertificate.Update(entity);
            Uow.SaveChanges();
        }

        public void ModifyGroupCertificateSchedule(Group_Certificate_Schedule model)
        {
            _repoGroupCertificateSchedule.Update(model);
            Uow.SaveChanges();

        }

        public Group_Certificate ModifyGroupCertificate(GroupCertificateModel model)
        {
            var entity = _repoGroupCertificate.Get(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.ModifiedBy = CurrentUser.USER_ID;
                entity.ModifiedDate = DateTime.Now;
                entity.IdCertificate = model.CertificateId;
                entity.Status = statusModify;
                _repoGroupCertificate.Update(entity);
            }
            else
            {
                entity = new Group_Certificate();
                entity.CreateDate = DateTime.Now;
                entity.CreateBy = CurrentUser.USER_ID;
                entity.Name = model.Name;
                entity.IdCertificate = model.CertificateId;
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.Status = statusModify;
                _repoGroupCertificate.Insert(entity);
            }
            if (entity.Group_Certificate_Subjects.Any())
            {
                _repoGroupCertificateSubject.Delete(entity.Group_Certificate_Subjects);
                entity.Group_Certificate_Subjects.Clear();
            }
            if (model.ArrSubjectId != null)
            {
                foreach (var id in model.ArrSubjectId)
                {
                    if (id != -1)
                    {
                        entity.Group_Certificate_Subjects.Add(new Group_Certificate_Subjects()
                        {
                            IdSubject = id
                        });
                    }
                }
            }
            Uow.SaveChanges();
            return entity;
        }
        #endregion

        #region Ingredients

        public Course_Ingredients_Learning GetIngredientsById(int? id)
        {
            return id.HasValue ? _repoIngredients.Get(id) : null;
        }

        public Course_Result_Course_Detail_Ingredient GetResultIngreById(int? id)
        {
            return id.HasValue ? _repoIngreIndient.Get(id) : null;

        }
        public Course_Ingredients_Learning Modify(PartialIngredientsViewModify model)
        {
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            var now = DateTime.Now;
            var entity = _repoIngredients.Get(model.Id);
            if (entity == null && model.IsCreate.Equals("on"))
            {
                entity = new Course_Ingredients_Learning
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = CurrentUser.USER_ID,
                    CreatedDate = now
                };
                _repoIngredients.Insert(entity);
            }
            else
            {
                entity.ModifiedBy = CurrentUser.USER_ID;
                entity.ModifiedDate = now;
                entity.Name = model.Name;
                entity.Description = model.Description;
                _repoIngredients.Update(entity);
            }
            Uow.SaveChanges();
            return entity;
        }

        public void InsertIngredient(Course_Ingredients_Learning model)
        {
            _repoIngredients.Insert(model);
            Uow.SaveChanges();
        }

        public void UpdateIngredient(Course_Ingredients_Learning model)
        {
            _repoIngredients.Insert(model);
            Uow.SaveChanges();
        }

        public IQueryable<Course_Ingredients_Learning> GetCourseIngredients(Expression<Func<Course_Ingredients_Learning, bool>> query)
        {
            var entities = _repoIngredients.GetAll(_ingredientDefaultFilter);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public List<TMS_Course_Member> GetListCourseMemberByMemberandCourseDetailId(int traineeId, int courseDetailId, bool isDelete = false, bool isActive = true)
        {

            var lst = _repoCourseMember.GetAll(x => x.Member_Id == traineeId && x.Course_Details_Id == courseDetailId && x.IsDelete == isDelete && x.IsActive == isActive).ToList();
            return lst;

        }

        public void Delete(List<Course_Detail_Room> entity)
        {
            foreach (var item in entity)
            {
                _repoCourseDetailRoom.Delete(item);
            }
            Uow.SaveChanges();
        }


        #endregion
        public List<sp_Reminder_TV_Result> GetReminder(string departlist, string listJobtitle, string subjectName, string subjectCode, DateTime dateFrom, int nom)
        {
            return Uow.sp_GetReminder_Result(departlist, listJobtitle, subjectName, subjectCode, dateFrom, nom).ToList();
        }

        public void Insert_Custom_new(int CourseID, IEnumerable<int> courseDetail, object[] idtrainee, IList<RemarkTrainee> LtsRemarkTrainee, int typerequest = -1)
        {
            if (courseDetail.Any())
            {
                foreach (var item in courseDetail)
                {
                    var y = 0;
                    var entities = _repoCourseMember.GetAll(a => a.Course_Details_Id == item).ToList();
                    foreach (var arr_c in idtrainee)
                    {
                        var id = Convert.ToInt32(idtrainee[y]);
                        string remark = LtsRemarkTrainee?.FirstOrDefault(a => a.idTrainee == id)?.remarkTrainee;
                        var entity = entities.FirstOrDefault(a => a.Member_Id == id);
                        if (entity == null)
                        {
                            entity = new TMS_Course_Member()
                            {
                                IsDelete = false,
                                IsActive = true,
                                Member_Id = id,
                                LmsStatus = statusAddNewTMS,//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync,
                                AssignBy = CurrentUser.USER_ID.ToString(),

                                Course_Details_Id = item
                            };
                            if (typerequest == 0) // gửi request 
                            {
                                entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                            }
                            else if (typerequest == 1)
                            {
                                entity.Status = (int)UtilConstants.APIAssign.Approved;// da gui  phê duyệt
                            }
                            _repoCourseMember.Insert(entity);
                        }
                        else
                        {
                            entity.IsDelete = false;
                            entity.IsActive = true;
                            entity.AssignBy = CurrentUser.USER_ID.ToString();
                            entity.LmsStatus = statusAddNewTMS;//processStep == (int)UtilConstants.BoolEnum.No ? statusModify : statusIsSync;
                            if (typerequest == 0) // gửi request
                            {
                                entity.Status = (int)UtilConstants.APIAssign.Pending;// chưa gửi phê duyệt
                            }
                            else if (typerequest == 1)
                            {
                                entity.Status = (int)UtilConstants.APIAssign.Approved;// da gui  phê duyệt
                            }

                            _repoCourseMember.Update(entity);


                        }
                        if (!string.IsNullOrEmpty(remark))
                        {
                            TMS_Course_Member_Remark entity_remark = _repoCourseMemberRemark.Get(a => a.TraineeId == id && a.CourseId == CourseID);
                            if (entity_remark == null)
                            {
                                entity_remark = new TMS_Course_Member_Remark()
                                {
                                    TraineeId = id,
                                    CourseId = CourseID,
                                    remark = remark,
                                };
                                _repoCourseMemberRemark.Insert(entity_remark);
                            }
                            else
                            {
                                entity_remark.TraineeId = id;
                                entity_remark.CourseId = CourseID;
                                entity_remark.remark = remark;
                                _repoCourseMemberRemark.Update(entity_remark);
                            }
                        }
                        y++;
                    }
                }
                // Uow.SaveChanges();
            }
        }

        public APIReturn InsertSentmailAPI(APISentMailModel model)
        {
            var returnModel = new APIReturn();
            var total = 0;
            returnModel.Status = 0;
            returnModel.Count = total;
            var cat_mail = configService.GetMail(a => a.Code == "SendMailReminderCourse")?.FirstOrDefault();
            string[] listmail = !string.IsNullOrEmpty(model.arr_user) ? model.arr_user.Split(',') : null;
            if (listmail != null)
            {
                foreach (var item in listmail)
                {

                    var entity = new TMS_SentEmail();

                    entity.mail_receiver = item;
                    entity.type_sent = (int)UtilConstants.TypeSentEmail.SendMailReminderCourse;
                    entity.content_body = HttpUtility.HtmlDecode(cat_mail.Content.Replace("MAIL_COURSE_NAME", model.coursename).Replace("MAIL_COURSE_ENDDATE", model.enddate));
                    entity.flag_sent = 0;
                    entity.cat_mail_ID = cat_mail?.ID;
                    //entity.id_course = courseId;
                    entity.Is_Deleted = false;
                    entity.Is_Active = true;
                    entity.subjectname = cat_mail.SubjectMail;
                    _repoSentMail.Insert(entity);
                    total++;
                }
                Uow.SaveChanges();
                returnModel.Status = 0;
                returnModel.Count = total;
            }

            //var count = 0;

            return returnModel;
        }

        public IQueryable<TMS_Course_Member> Lam_GetTraineemember_New(Expression<Func<TMS_Course_Member, bool>> query)
        {
            var entities = _repoCourseMember.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
    }
}
