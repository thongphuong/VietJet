using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.Utils;
using TMS.Core.ViewModels.APIModels;

namespace TMS.Core.Services.CourseDetails
{
    using System.Data.Entity.SqlServer;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using ViewModels.Courses;
    using TMS.Core.ViewModels.UserModels;

    public class CourseDetailService : BaseService, ICourseDetailService
    {
        private readonly IRepository<Course_Detail> _repoCourseDetail;
        private readonly IRepository<Course_Detail_Subject_Note> _repoCourseDetailSubjectNote;
        private readonly IRepository<Course_Detail_Instructor> _repoCourseDetailInstructor;
        private readonly IRepository<Course_Blended_Learning> _repoCourseBlendedLearning;
        private readonly IRepository<Trainee> _repoTrainee;
        private readonly IRepository<TMS_Course_Member> _repoTMS_Course_Member;
        private readonly IRepository<Course_Result> _repoCourse_Result;
        private readonly IRepository<LMS_REQUEST> _repoLmsRequest;
        private readonly IRepository<SubjectDetail> _repoSubjectDetail;
        private readonly IRepository<Course_Result_Final> _repoCourseResultFinal;
        private readonly IRepository<Course> _repoCourse;

        private readonly IRepository<Course_Result_Summary> _repoCourseResultSummary;
        private readonly IRepository<Course_LMS_STATUS> _repoCourseLmsStatus;
        private readonly IRepository<LMS_Assign> _repoLmsAssign;
        private readonly IRepository<TMS_Course_Member> _repoTmsCourseMember;
        private readonly IRepository<TMS_APPROVES> _repoTmsApprove;
        private readonly IRepository<PROCESS_Steps> _repositoryPROCESS_Steps;
        //Config side
        private readonly IRepository<CONFIG> _repoConfig;

        private const int statusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
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

        public CourseDetailService(IUnitOfWork unitOfWork,
            IRepository<PROCESS_Steps> repositoryPROCESS_Steps, IRepository<CONFIG> repoConfig, IRepository<TMS_APPROVES> repoTmsApprove, IRepository<Course_Result_Final> repoCourseResultFinal, IRepository<TMS_Course_Member> repoTmsCourseMember, IRepository<LMS_Assign> repoLmsAssign, IRepository<LMS_REQUEST> repoLmsRequest, IRepository<Course_Detail> repoCourseDetail, IRepository<Course_Detail_Subject_Note> repoCourseDetailSubjectNote, IRepository<Course_Detail_Instructor> repoCourseDetailInstructor, IRepository<Course_Blended_Learning> repoCourseBlendedLearning, IRepository<Trainee> repoTrainee, IRepository<TMS_Course_Member> repoTMS_Course_Member, IRepository<Course_Result> repoCourse_Result, IRepository<SubjectDetail> repoSubjectDetail, IRepository<Course_Result_Summary> repoCourseResultSummaryService, IRepository<Course_LMS_STATUS> repoCourseLmsStatus, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCourse = repoCourse;
            _repoCourseDetail = repoCourseDetail;
            _repoCourseDetailSubjectNote = repoCourseDetailSubjectNote;
            _repoCourseDetailInstructor = repoCourseDetailInstructor;
            _repoCourseBlendedLearning = repoCourseBlendedLearning;
            _repoTrainee = repoTrainee;
            _repoTMS_Course_Member = repoTMS_Course_Member;
            _repoCourse_Result = repoCourse_Result;
            _repoLmsRequest = repoLmsRequest;
            _repoSubjectDetail = repoSubjectDetail;
            _repoCourseResultSummary = repoCourseResultSummaryService;
            _repoCourseLmsStatus = repoCourseLmsStatus;
            _repoLmsAssign = repoLmsAssign;
            _repoTmsCourseMember = repoTmsCourseMember;
            _repoCourseResultFinal = repoCourseResultFinal;
            _repoTmsApprove = repoTmsApprove;
            _repoConfig = repoConfig;
            _repositoryPROCESS_Steps = repositoryPROCESS_Steps;
        }

        public Course_Detail GetCourseDetailById(int? id)
        {
            return _repoCourseDetail.Get(id);
        }

        public void BulkUpdate(List<Course_Detail> entity)
        {
            _repoCourseDetail.Update(entity);
            Uow.SaveChanges();
        }

        public Course GetCourseById(int Id)
        {
            return _repoCourse.Get(x => x.Id == Id);
        }

        public Course_Detail GetById(int? id)
        {
            var entity = id.HasValue ? _repoCourseDetail.Get(id) : null;
            return entity != null && entity.IsDeleted != true ? entity : null;

        }

        public IQueryable<Course_Detail> GetByCourse(IEnumerable<int> courseIds)
        {
            return courseIds == null ? new List<Course_Detail>().AsQueryable() : _repoCourseDetail.GetAll(a => a.IsDeleted != true && courseIds.Contains((int)a.CourseId));
        }

        public IQueryable<Course_Detail> GetByCourse(int courseId)
        {
            return _repoCourseDetail.GetAll(a => a.CourseId == courseId && a.IsDeleted != true);
        }
        public IQueryable<Course_Blended_Learning> GetBlendedByCourseId(int courseDetailId)
        {
            return _repoCourseBlendedLearning.GetAll(a => a.Course_Detail_Id == courseDetailId && a.IsActive == true && a.IsDeleted == false);
        }
        public IQueryable<Course_Detail> GetBySubject(IEnumerable<int> subjectIds)
        {
            return _repoCourseDetail.GetAll(a => a.IsDeleted != true && subjectIds.Contains((int)a.SubjectDetailId));
        }

        public IQueryable<Course_Detail> GetBySubject(int subjectId)
        {
            return _repoCourseDetail.GetAll(a => a.IsDeleted != true && a.SubjectDetailId == subjectId);
        }

        public IQueryable<Course_Detail> Get(DateTime? dateFrom, DateTime? dateTo)
        {
            return
                _repoCourseDetail.GetAll(
                    a => a.IsDeleted != true &&
                        SqlFunctions.DateDiff("day", dateFrom, a.dtm_time_from) >= 0 &&
                        SqlFunctions.DateDiff("day", a.dtm_time_to, dateTo) >= 0);
        }

        public IQueryable<Course_Detail> Get(Expression<Func<Course_Detail, bool>> query, int[] approveType = null)
        {
            var entities = query == null ? _repoCourseDetail.GetAll(a => a.IsDeleted != true) : _repoCourseDetail.GetAll(a => a.IsDeleted != true).Where(query);

            if (approveType != null)
            {
                foreach (var item in approveType)
                {
                    var datastep = _repositoryPROCESS_Steps.Get(a => a.Step == item && a.IsActive==true);
                    if (item == (int)UtilConstants.ApproveType.SubjectResult)
                    {
                        entities = datastep != null ? entities.Where(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.CourseId && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == item)) : entities;
                    }
                    else
                    {
                        entities = datastep != null ? entities.Where(a => a.Course.TMS_APPROVES.Any(b => b.int_Course_id == a.CourseId && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == item)) : entities;
                    }


                }
            }

            return entities;
        }
        public bool checkapproval(Course_Detail course_Detail, int[] approveType = null)
        {
            var return_ = false;

            if (approveType != null)
            {
                foreach (var item in approveType)
                {
                    var datastep = _repositoryPROCESS_Steps.Get(a => a.Step == item && a.IsActive==true);
                    if (item == (int)UtilConstants.ApproveType.SubjectResult)
                        return_ = datastep != null ? course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == item) ? true : false
                            : true;
                    return_ = datastep != null ? course_Detail.Course.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == item) ? true : false
                       : true;
                }
            }

            return return_;
        }
        public IQueryable<Course_Detail> GetAllApi(Expression<Func<Course_Detail, bool>> query)
        {
            return query == null ? _repoCourseDetail.GetAll() : _repoCourseDetail.GetAll().Where(query);
        }

        public IQueryable<Course_Detail_Instructor> GetDetailInstructors(Expression<Func<Course_Detail_Instructor, bool>> query = null)
        {
            var entities = _repoCourseDetailInstructor.GetAll();
            if(query != null)
            {
                entities = entities.Where(query);
            }
            return entities;

        }

        public IQueryable<Course_Detail_Subject_Note> GetDetailSubjectNote(Expression<Func<Course_Detail_Subject_Note, bool>> query)
        {
            return query == null ? _repoCourseDetailSubjectNote.GetAll() : _repoCourseDetailSubjectNote.GetAll(a => !a.IsDeleted.HasValue || !a.IsDeleted.Value).Where(query);
        }

        public Course_Detail_Subject_Note GetDetailSubjectNoteById(int? id)
        {
            if (!id.HasValue) return null;
            var entity = _repoCourseDetailSubjectNote.Get(id);
            return entity == null || (entity.IsDeleted.HasValue && entity.IsDeleted.Value) ? null : entity;
        }
        public Course_Detail_Instructor GetDetailInstructorBySubject(int? id)
        {
            if (!id.HasValue) return null;
            var entity = _repoCourseDetailInstructor.Get(a => a.Course_Detail_Id == id);
            return entity;
        }
        public Course_Detail_Instructor GetDetailInstructorById(int? id)
        {
            if (!id.HasValue) return null;
            var entity = _repoCourseDetailInstructor.Get(id);
            return entity;
        }

        public void Update(Course_Detail entity)
        {
            _repoCourseDetail.Update(entity);
            Uow.SaveChanges();
        }

        public void UpdateLmsStatus(int courseId)
        {
            var courseDetails = _repoCourseDetail.GetAll(a => a.CourseId == courseId);
            foreach (var item in courseDetails)
            {
                item.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                _repoCourseDetail.Update(item);
            }
            Uow.SaveChanges();
        }

        public void Update(Course_Detail_Subject_Note entity)
        {
            _repoCourseDetailSubjectNote.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(Course_Detail_Subject_Note entity)
        {
            _repoCourseDetailSubjectNote.Insert(entity);
            Uow.SaveChanges();
        }

        public void Insert(Course_Detail entity)
        {
            _repoCourseDetail.Insert(entity);
            Uow.SaveChanges();
        }

        public void Delete(Course_Detail entity)
        {
            //_repoCourseDetail.Delete(entity);
            entity.IsDeleted = true;
            Uow.SaveChanges();
        }
        public void Insert(Course_Detail_Instructor entity)
        {
            _repoCourseDetailInstructor.Insert(entity);
            Uow.SaveChanges();
        }
        //TODO: process result
        public string GetGradebyscore(int? courseDetailId, double? score)
        {

            return "";
        }
        #region[API AssignTrainee]
        public bool Assign(APIAssignLMS[] model, string currentUser)
        {
            try
            {
                if (model == null || !model.Any()) return false;
                foreach (var item in model)
                {
                    //Course_Detail_Id
                    var courseDetail =
                        _repoCourseDetail.Get(a => a.Id == item.CourseDetailId);
                    if (courseDetail == null) continue;

                    var courseDetailId = courseDetail.Id;



                    /////////////////////////////////////////CHECK CONDITION ASSIGN TRaINEE////////////////////////////////////////////////////////
                    var checkConfigSite = CheckSiteConfig(UtilConstants.KEY_AUTO_ASSIGN_TRAINEE);
                    var course = courseDetail.Course;
                    var recommend = courseDetail.bit_Regisable == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No;

                    var maxTrainee = course.NumberOfTrainee ?? 0;
                    var countTmsMember =
                        courseDetail.TMS_Course_Member.Count(a => a.IsDelete == false);

                    if (checkConfigSite)
                    {
                        if (recommend == (int)UtilConstants.BoolEnum.Yes && countTmsMember + 1 > maxTrainee)
                        {
                            return false;
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////
                    var trainee =
                        _repoTrainee.Get(
                            a => a.IsDeleted==false && a.str_Staff_Id.ToLower().Trim() == item.TraineeCode.ToLower().Trim());
                    if (trainee == null) continue;
                    var traineeId = trainee.Id;

                    TMS_Course_Member entity;
                    entity =
                       _repoTmsCourseMember.Get(
                           a =>
                               a.Course_Details_Id == courseDetailId && a.Member_Id == traineeId);
                    if (entity != null)
                    {
                        entity.Status = (int)UtilConstants.APIAssign.Pending;
                        entity.AssignBy = currentUser;
                        entity.IsActive = true;
                        entity.IsDelete = false;
                        entity.LmsStatus = (int)UtilConstants.ApiStatus.Synchronize;
                        _repoTMS_Course_Member.Update(entity);
                        Uow.SaveChanges();
                    }
                    else
                    {
                        entity = new TMS_Course_Member();
                        entity.Course_Details_Id = courseDetailId;
                        entity.Member_Id = traineeId;
                        entity.AssignBy = currentUser;
                        entity.IsActive = true;
                        entity.IsDelete = false;
                        entity.Status = (int)UtilConstants.APIAssign.Pending;
                        entity.LmsStatus = (int)UtilConstants.ApiStatus.Synchronize;
                        _repoTMS_Course_Member.Insert(entity);
                        Uow.SaveChanges();
                    }

                    /////////////////////////////////////////AUTO ASSIGN TRaINEE////////////////////////////////////////////////////////

                    if (checkConfigSite)
                    {
                        var members = _repoTMS_Course_Member.GetAll(a => a.Course_Details_Id == courseDetailId && a.IsDelete == false).ToList();
                        var countfinalmembers = members.Count();
                        if (recommend == (int)UtilConstants.BoolEnum.Yes && countfinalmembers == maxTrainee)
                        {
                            var courseId = course.Id;
                            foreach (var member in members)
                            {
                                member.Status = (int)UtilConstants.ApiStatus.Synchronize;
                                member.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                                _repoTMS_Course_Member.Update(member);
                            }
                            Uow.SaveChanges();
                            RegistTraineeToCourse(members, courseId);
                            Uow.SaveChanges();
                            return true;
                        }
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        private void RegistTraineeToCourse(IEnumerable<TMS_Course_Member> trainee, int courseId)
        {
            foreach (var traineeId in trainee)
            {
                var entity =
                   _repoCourseResultFinal.Get(
                       a => a.traineeid == traineeId.Member_Id && a.courseid == courseId);

                if (entity == null)
                {
                    _repoCourseResultFinal.Insert(new Course_Result_Final()
                    {
                        traineeid = traineeId.Member_Id,
                        courseid = courseId,
                        createby = CurrentUser.USER_ID.ToString(),
                        IsDeleted = false,
                        createday = DateTime.Now,
                        LmsStatus = statusIsSync,
                        MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Pending
                    });

                }
                else
                {
                    entity.IsDeleted = false;
                    entity.LmsStatus = statusIsSync;
                    entity.MemberStatus = (int)UtilConstants.CourseResultFinalStatus.Pending;
                    _repoCourseResultFinal.Update(entity);
                }
                // check tms approve
                var assignType = (int)UtilConstants.ApproveType.AssignedTrainee;
                var approve = _repoTmsApprove.Get(a => a.int_Course_id == courseId && a.int_Type == assignType);
                if (approve == null)
                {
                    approve = new TMS_APPROVES()
                    {
                        int_Course_id = courseId,
                        int_Type = assignType,
                        int_id_status = (int)UtilConstants.EStatus.Approve,
                        int_Seq = 1,
                    };
                    _repoTmsApprove.Insert(approve);
                }
                else
                {
                    approve.int_id_status = (int)UtilConstants.EStatus.Approve;
                    _repoTmsApprove.Update(approve);
                }
            }
            Uow.SaveChanges();
        }
        public string AssignTrainee(APICourseTraineeLMSViewModel[] model, string currentUser)
        {
            try
            {
                if (model == null || !model.Any()) return "Invalid data";


                foreach (var item in model)
                {
                    // check coursedetail
                    var entityCoursedetail = _repoCourseDetail.Get(item.CourseDetailId);
                    if (entityCoursedetail == null)
                        return "CourseDetailId " + item.CourseDetailId;
                    foreach (var itemtrainee in item.assignTrainee)
                    {
                        // check trainee
                        var entitytrainee = _repoTrainee.Get(a => a.str_Staff_Id.Contains(itemtrainee.TraineeCode));
                        if (entitytrainee == null)
                            return "TraineeCode " + itemtrainee.TraineeCode;
                        var entity = entityCoursedetail.TMS_Course_Member.Where(a => a.Member_Id == entitytrainee.Id);
                        TMS_Course_Member entity2 = new TMS_Course_Member();
                        if (entity.Any())
                        {
                            entity2 = _repoTMS_Course_Member.Get(entity.FirstOrDefault().Id);
                            entity2.IsDelete = null;
                            _repoTMS_Course_Member.Update(entity2);
                        }
                        else
                        {

                            entity2.Course_Details_Id = item.CourseDetailId;
                            entity2.Member_Id = entitytrainee.Id;
                            _repoTMS_Course_Member.Insert(entity2);
                        }
                    }
                }
                Uow.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion
        private bool CheckSiteConfig(string key)
        {
            var config = _repoConfig.Get(a => a.KEY.ToLower().Trim() == key.ToLower().Trim());
            return config != null;
        }

        public List<sp_GetDetail_TV_Result> GetTraining(string venue, string courseCode, string courseName, int? departmentid, DateTime? dateFrom, DateTime? dateTo, string courids,int? CourseID)
        {
            return Uow.sp_GetDetail_Result(venue, courseCode, courseName, departmentid, dateFrom, dateTo, courids, CourseID).ToList();
        }
    }



}

