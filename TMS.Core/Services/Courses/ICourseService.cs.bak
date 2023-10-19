using TMS.Core.Utils;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.ReportModels;

namespace TMS.Core.Services.Courses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.UserModels;
    using ViewModels.Company;
    using ViewModels.Room;
    using TMS.Core.ViewModels.Nation;
    using TMS.Core.ViewModels.AjaxModels.AjaxAssignMember;

    public interface ICourseService : IBaseService
    {
        //int course type = 0
        Course GetById(int? id);

        Course GetByCourseCode(string code);

        IQueryable<Course> GetCoursesRp();
        IQueryable<Course_Cost> GetCost();
        IQueryable<Course> Get(bool? NotApproval = null);
        IQueryable<Course> Get(ICollection<string> code);

        List<int> GetListSubjectDetailIdByCourseId(int courseId);

        void Insert(TMS_Course_Member model, string currentUser, int courseDetailId, int courseId);
        void Insert(int Member_Id, int Course_Details_Id, int courseId, String AssignBy);

        List<TMS_Course_Member> GetListCourseMemberByMember(int traineeId, bool isDelete = false, bool isActive = true);
        List<TMS_Course_Member> GetListCourseMemberByMemberandCourseDetailId(int traineeId, int courseDetailId, bool isDelete = false, bool isActive = true);

        Trainee_Portal GetByCourseAndTrainneById(int id);

        List<Trainee_Portal> GetListCourseTrainne(string traineeName = "");


        void Delete(TMS_Course_Member entity, string currentUser, int courseId);

        IQueryable<Course> Get(string code, string name, string venue);
        IQueryable<Course> Get(string code, DateTime? dateFrom = null, DateTime? dateTo = null);
        IQueryable<Course> Get(string code, string name, string venue, int? type, int? status);
        IQueryable<Course> Get(Expression<Func<Course, bool>> query, bool? notApproval = null);
        IQueryable<Course> GetListCourse(Expression<Func<Course, bool>> query, bool? notApproval = null);
        IQueryable<Course> GetCreatEID(Expression<Func<Course, bool>> query);
        IQueryable<Course> GetCodeProgram(Expression<Func<Course, bool>> query);
        IQueryable<Course> Get_BondAgreement(Expression<Func<Course, bool>> query, int? approveType = null, int? eStatus = null);
        bool checkapproval(Course course, int[] approveType = null);
        IQueryable<Course_Cost> GetCost(Expression<Func<Course_Cost, bool>> query);
        IQueryable<Course_Detail_Instructor> GetAllowance(Expression<Func<Course_Detail_Instructor, bool>> query);
        IQueryable<TMS_Course_Member> GetTraineemember(Expression<Func<TMS_Course_Member, bool>> query);
        IQueryable<TMS_Course_Member> Lam_GetTraineemember_New(Expression<Func<TMS_Course_Member, bool>> query);
        IQueryable<Course> ApiGet(Expression<Func<Course, bool>> query);
        IQueryable<Course> GetApproveCourseByType(int type);
        IQueryable<Course> ApiGetAllCourses(Expression<Func<Course, bool>> query = null);

        bool UpdateCourseTrainnePending(Trainee_Portal item);

        bool InsertCourseTrainnePending(Trainee_Portal item);
        bool RemoveCourseTrainnePending(Trainee_Portal item);

        bool DeleteCourseTrainnePending(Trainee_Portal item);

        Course GetByCode(string code);

        Trainee_Portal GetByCourseAndTrainne(int courseID, int traineeID);

        IQueryable<Course_TrainingCenter> GetTrainingCenters(Expression<Func<Course_TrainingCenter, bool>> query = null);
        void DeleteTrainingCenters(IEnumerable<Course_TrainingCenter> entities);
        void InsertTrainingCenters(Course_TrainingCenter entity);

        void Update(Course entity);
        void Insert(Course entity);
        void Insert(TMS_Course_Member entity, int courseDetailId, int courseId, UtilConstants.ApproveType approveType, int processStep, int typerequest = -1);
        void Insert_Custom(TMS_Course_Member entity, Course course,UtilConstants.ApproveType approveType, int processStep, int typerequest = -1);
        void Insert_Custom_new(int CourseID ,IEnumerable<int> courseDetail, object[] idtrainee, IList<RemarkTrainee> LtsRemarkTrainee, int typerequest = -1);
        void Delete(TMS_Course_Member entity, int courseId);
        void Modify(TMS_Course_Member entity, int courseDetailId, int courseId);
        void UpdateTmsMember(TMS_Course_Member entity, int courseDetailId, int courseId);
        void InsertSentMail(TMS_SentEmail entity);


        // **!a.bit_Deleted 
        #region Course result 

        IQueryable<Course_Result_Final> ApiGetAll(Expression<Func<Course_Result_Final, bool>> query = null);
        IQueryable<Course_Result> GetCourseResult(int traineeId);
        //last or default
        Course_Result GetCourseResult(int? traineeId, int? courseDetailId);

        IQueryable<Course_Result> GetCourseResult(Expression<Func<Course_Result, bool>> query = null);
        IQueryable<CourseRemarkCheckFail> GetCourseResultCheckFail(Expression<Func<CourseRemarkCheckFail, bool>> query = null);
        IQueryable<Subject_Score> GetScores(Expression<Func<Subject_Score, bool>> query = null);
        Course_Result GetCourseResultById(int courseResultId);
        void UpdateCourseResult(Course_Result entity);
        void UpdateCourseResultCheckFail(CourseRemarkCheckFail entity);
        void InsertCourseResult(Course_Result entity);
        void InsertCourseResultCheckFail(CourseRemarkCheckFail entity);

        #endregion
        #region course result final

        IQueryable<Course_Result_Final> GetCourseResultFinal(Expression<Func<Course_Result_Final, bool>> query = null, int[] approveType = null, int? eStatus = null);
        Course_Result_Final GetCourseResultFinalById(int? id);
        CourseRemarkCheckFail GetCourseRemarkCheckFailById(int? id);

        void UpdateCourseResultFinal(Course_Result_Final entity);
        Course_Result_Final UpdateCourseResultFinalReturnEntity(Course_Result_Final entity);
        void InsertCourseResultFinal(Course_Result_Final entity);
        IQueryable<Course_Result_Final> GetCourseMembers(int courseId, bool isViewAll = false);
        IQueryable<Course_Detail_Room> GetCourseDetailRooms(Expression<Func<Course_Detail_Room, bool>> query = null);
        IQueryable<Course_Detail_Room_Global> GetCourseDetailRoomsGlobal(Expression<Func<Course_Detail_Room_Global, bool>> query = null);
        Course_Detail_Room_Global GetCourseDetailGlobalById(int IdGlobal);
        #endregion
        List<Course_Type> GetCourseTypes();
        List<Course_Type_Result> GetCourseTypeResult();
        //Room
        Room GetRoomById(int? id);
        IQueryable<Room> GetRoom(Expression<Func<Room, bool>> query = null);
        IQueryable<Room> GetRoom_Course(Expression<Func<Room, bool>> query = null);
        IQueryable<Room_Type> GetRoomType(Expression<Func<Room_Type, bool>> query = null);
        IQueryable<Management_Room_Item> GetManagement_Room_Item(Expression<Func<Management_Room_Item, bool>> query = null);
        void UpdateRoom(Room entity);
        void InsertRoom(Room entity);
        void ModifyRoom(Room entity);
        void Delete(List<Course_Detail_Room> entity);
        //Company
        Company GetCompanyById(int? id);
        IQueryable<Company> GetCompany(Expression<Func<Company, bool>> query = null);
        void UpdateCompany(Company entity);
        void InsertCompany(Company entity);
        void UpdateCourseDetailRoomGlobal(Course_Detail_Room_Global entity);
        void InsertCourseDetailRoomGlobal(Course_Detail_Room_Global entity);
        void ModifyCompany(Company entity);
        //Nationality
        Nation GetNationById(int? id);
        IQueryable<Nation> GetNation(Expression<Func<Nation, bool>> query = null);
        void UpdateNation(Nation entity);
        void InsertNation(Nation entity);
        void ModifyNation(Nation entity);
        Course ModifyReturnModel(CourseModifyModel model);

        void ModifyCourseDetailRoom(Course_Detail_Room model);
        List<sp_GetTrainingHeaderTV_Result> GetTrainingHeader(string listId, string departmentCode,
    DateTime? fromDateStart, DateTime? fromDateEnd, DateTime? toDateStart, DateTime? toDateOfEnd, string status);
        List<sp_Reminder_TV_Result> GetReminder(string departlist, string listJobtitle,
    string subjectName, string subjectCode, DateTime dateFrom, int nom);
        void Update(RoomModels model);
        void InsertRoom(RoomModels model);
        void ModifyRoom(List<RoomModels> models);
        void Update(CompanyModels model);
        void InsertCompany(CompanyModels model);

        void Update(NationModels model);
        void InsertNation(NationModels model);

        IQueryable<Course_Result_Summary> GetCourseResultSummaries(Expression<Func<Course_Result_Summary, bool>> query = null, bool? NotApproval = null);
        Course_Result_Summary GetCourseResultSummary(int? traineeId, int? courseDetailId);
        void Update(Course_Result_Summary entity);

        #region Course_LMS_STATUS
        Course_LMS_STATUS GetCourseLms(int courseid, UtilConstants.LMSStatus steps);
        IQueryable<Course_LMS_STATUS> GetCourseLmsApi(Expression<Func<Course_LMS_STATUS, bool>> query = null);
        void InsertCourseLmsStatus(Course_LMS_STATUS model);
        void UpdateCourseLmsStatus(Course_LMS_STATUS model);
        void UpdateCourseLmsStatusApi(Course_LMS_STATUS model);

        IQueryable<LMS_Assign> GetLmsAssign(Expression<Func<LMS_Assign, bool>> query);

        void UpdateLmsAssgin(int traineeId, int courseId);

        bool UpdateFlagLMS(APIFlagLMS[] model);
        int UpdateFlagLMSReturnInt(APIFlagLMS[] model, string currentUser);
        APIReturn UpdateFlagLMSReturnModel(APIFlagLMS[] model, string currentUser);
        bool ApplyAssignTrainee(APIAssignLMS[] model, string currentUser);
        #endregion

        int InsertTraineeFuture(APITraineeFuture[] model, string currenUser);
        TraineeFuture GetTraineeFutureById(int? id);
        void UpdateTraineeFuture(TraineeFuture entity);
        IQueryable<TraineeHistory_Item> GetHistoryItems(Expression<Func<TraineeHistory_Item, bool>> query = null);
        void Update(TraineeHistory_Item entity);
        #region [------------Điểm danh--------Attendance-----------------------]
        IQueryable<Course_Attendance> GetAllTraineeAttendance(Expression<Func<Course_Attendance, bool>> query);
        Course_Attendance GetTraineeAttendance(Expression<Func<Course_Attendance, bool>> query);

        void InsertAttendance(Course_Attendance entity);
        void UpdateAttendance(Course_Attendance entity);
        #endregion


        #region [---------------------------CAT_CERTIFICATE------------------------------------]

        CAT_CERTIFICATE GetCatCertificateById(int? id);

        IQueryable<CAT_CERTIFICATE> GetCatCertificates(Expression<Func<CAT_CERTIFICATE, bool>> query);
        #endregion

        #region  [Meeting Room Management]
        Meeting GetMeetingById(int? id);
        IQueryable<Meeting> GetMeeting(Expression<Func<Meeting, bool>> query = null);
        void UpdateMeeting(Meeting entity);
        void InsertMeeting(Meeting entity);
        void ModifyMeeting(Meeting entity);
        Meeting UpdateMeeting(MeetingModels model);
        Meeting InsertMeeting(MeetingModels model);
        Meeting_Participants GetParticipantById(int? id);
        IQueryable<Meeting_Participants> GetMeetingParticipants(Expression<Func<Meeting_Participants, bool>> query = null);
        void UpdateMeetingParticipant(Meeting_Participants entity);
        #endregion
        #region [Certificate for course]
        IQueryable<Group_Certificate> GetAllGroupCertificate(Expression<Func<Group_Certificate, bool>> query = null);
        Group_Certificate GetGroupCertificateById(int? id);

        Group_Certificate ModifyGroupCertificate(GroupCertificateModel model);
        void ModifyGroupCertificateSchedule(Group_Certificate_Schedule model);
        void UpdateGroupCertificate(Group_Certificate entity);
        IQueryable<Group_Certificate_Schedule> GetAllGroupCertificateSchedule(Expression<Func<Group_Certificate_Schedule, bool>> query = null);
        IQueryable<TMS_CertificateApproved> GetAllGroupCertificateApprove(Expression<Func<TMS_CertificateApproved, bool>> query = null);
        TMS_CertificateApproved ModifyTMSCertificateAppovedEntity(TMS_CertificateApproved entity);
        #endregion

        #region Ingredients

        Course_Ingredients_Learning GetIngredientsById(int? id);
        Course_Ingredients_Learning Modify(PartialIngredientsViewModify model);
        void InsertIngredient(Course_Ingredients_Learning model);
        void UpdateIngredient(Course_Ingredients_Learning model);
        IQueryable<Course_Ingredients_Learning> GetCourseIngredients(Expression<Func<Course_Ingredients_Learning, bool>> query);
        Course_Result_Course_Detail_Ingredient GetResultIngreById(int? id);
        #endregion
        IEnumerable<Course_Result> GetCourseResult_huy(Expression<Func<Course_Result, bool>> query = null);
        APIReturn InsertSentmailAPI(APISentMailModel model);
    }
}
