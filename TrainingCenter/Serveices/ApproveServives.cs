using System;
using TrainingCenter.Utilities;
using DAL.Entities;

namespace TrainingCenter.Serveices
{
    using System.Linq;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.UserModels;

    public partial class ApproveServives
    {

        #region Init
        readonly IUnitOfWork _uow ;
        private readonly DAL.Repositories.IRepository<TMS_APPROVES> _repoApproves;
        private readonly DAL.Repositories.IRepository<TMS_APPROVES_HISTORY> _repoApprovesHistory;
        private readonly DAL.Repositories.IRepository<USER> _repoUser;
        private readonly DAL.Repositories.IRepository<CONFIG> _repoConfig;

        public ApproveServives(IUnitOfWork unitofwork)
        {
            _uow = unitofwork;
            this._repoApproves = _uow.Repository<TMS_APPROVES>();
            this._repoApprovesHistory = _uow.Repository<TMS_APPROVES_HISTORY>();
            this._repoUser = _uow.Repository<USER>();
            _repoConfig = _uow.Repository<CONFIG>();
        }
        #endregion



        #region Create Approve Course
        public bool ApproveCoures(UserModel userModel, Course course, int type)
        {
            try
            {
                var hodRoleName = _repoConfig.Get(a=>a.KEY == "HOD").VALUE;
                var firstOrDefault = _repoUser.GetAll(a => a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName)).FirstOrDefault();
                var approve = new TMS_APPROVES();
                // kiem tra courseid vs type approve co hay chua
                var checkappro = _repoApproves.Get(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.Course);
                if (checkappro != null)
                {
                    approve = _repoApproves.Get(checkappro.id);
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.Course;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a => a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    _repoApproves.Update(approve);
                    _uow.SaveChanges();
                }
                else
                {
                    // insert TMS_APPROVES
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.Course;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Seq = 1;
                    approve.int_version = 1;
                    approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    _repoApproves.Insert(approve);
                    _uow.SaveChanges();
                }
                var approve_history = new TMS_APPROVES_HISTORY();
                // Lưu history
                var checkapprohistory = _repoApprovesHistory.GetAll(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.Course);
                if (checkapprohistory.Any())
                {

                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.Course;
                    approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = checkapprohistory.Count() + 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                else
                {
                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.Course;
                    approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Create Approve AssignTrainees

        public bool ApproveAssignTrainees(UserModel userModel, Course course, int type)
        {
            try
            {
                var hodRoleName =  _repoConfig.Get(a=>a.KEY == "HOD").VALUE;
                var firstOrDefault = _repoUser.Get(a =>a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName));
                var approve = new TMS_APPROVES();
                // kiem tra courseid vs type approve co hay chua
                var checkappro = _repoApproves.Get(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee);
                if (checkappro != null)
                {
                    approve = _repoApproves.Get(checkappro.id);
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.AssignedTrainee;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    _repoApproves.Update(approve);
                    _uow.SaveChanges();
                }
                else
                {
                    // insert TMS_APPROVES
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.AssignedTrainee;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Seq = 1;
                    approve.int_version = 1;
                    approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    _repoApproves.Insert(approve);
                    _uow.SaveChanges();
                }





                var approve_history = new TMS_APPROVES_HISTORY();
                // Lưu history
                var checkapprohistory = _repoApprovesHistory.GetAll(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee);
                if (checkapprohistory.Any())
                {

                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.AssignedTrainee;
                    approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = checkapprohistory.Count() + 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                else
                {
                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.AssignedTrainee;
                    approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region CreateApproveCourseResultRecore

        public bool CreateApproveCourseResultRecore(UserModel userModel, Course course,int type)
        {
            try
            {
                var approve = new TMS_APPROVES();
                var checkappro = _repoApproves.Get(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.CourseResult);
                if (checkappro != null)
                {
                    approve = _repoApproves.Get(checkappro.id);
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.CourseResult;
                    approve.dtm_requested_date = DateTime.Now;
                    _repoApproves.Update(approve);
                    _uow.SaveChanges();
                }
                else
                {
                    // insert TMS_APPROVES
                    approve.int_Course_id = course.Id;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.CourseResult;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Seq = 1;
                    approve.int_version = 1;
                    _repoApproves.Insert(approve);
                    _uow.SaveChanges();
                }





                var approve_history = new TMS_APPROVES_HISTORY();
                // Lưu history
                var checkapprohistory =
                    _repoApprovesHistory.GetAll(
                        a =>
                            a.int_Course_id == course.Id &&
                            a.int_Type == (int)UtilConstants.ApproveType.CourseResult);
                if (checkapprohistory.Count() > 0)
                {

                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.CourseResult;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = checkapprohistory.Count() + 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                else
                {
                    approve_history.approves_id = approve.id;
                    approve_history.int_id_status = type;
                    approve_history.int_Course_id = course.Id;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.CourseResult;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.dtm_requested_date = DateTime.Now;
                    approve_history.int_version = 1;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region CreateApproveSubjectResultRecore
        //Lv truyen tĩnh, 1 lúc nhap result ,2 là luc lv1 dc approve
        public bool CreateApproveSubjectResultRecore(UserModel userModel, Course_Detail course, int lv, int type)
        {
            try
            {
                var approve = new TMS_APPROVES();
                var hodRoleName =  _repoConfig.Get(a=>a.KEY == "HOD").VALUE;
                // kiem tra courseid vs type approve co hay chua
                var checkappro = _repoApproves.Get(a => a.int_Course_id == course.CourseId && a.int_courseDetails_Id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                if (checkappro != null)
                {
                    approve = _repoApproves.Get(checkappro.id);
                    approve.int_Course_id = course.CourseId;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.SubjectResult;
                    approve.dtm_requested_date = DateTime.Now;
                    if (lv != 1)
                    {
                        var firstOrDefault = _repoUser.Get(a =>a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName));
                        approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    }
                    approve.int_courseDetails_Id = course.Id;
                    _repoApproves.Update(approve);
                    _uow.SaveChanges();

                }
                else
                {
                    approve.int_Course_id = course.CourseId;
                    approve.int_Requested_by = (int)userModel.USER_ID;
                    approve.int_id_status = type;
                    approve.int_Type = (int)UtilConstants.ApproveType.SubjectResult;
                    approve.dtm_requested_date = DateTime.Now;
                    approve.int_Seq = 1;
                    approve.int_version = 1;
                    if (lv != 1)
                    {
                        var firstOrDefault = _repoUser.Get(a =>a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName));
                        approve.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    }
                    approve.int_courseDetails_Id = course.Id;
                    _repoApproves.Insert(approve);
                    _uow.SaveChanges();

                }



                var approve_history = new TMS_APPROVES_HISTORY();
                // Lưu history
                var checkapprohistory = _repoApprovesHistory.GetAll(a => a.int_Course_id == course.CourseId && a.int_Type == (int)UtilConstants.ApproveType.Course);
                if (checkapprohistory.Any())
                {

                    approve_history.int_Course_id = course.CourseId;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.int_id_status = type;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.SubjectResult;
                    approve_history.dtm_requested_date = DateTime.Now;
                    //approve.int_Seq = 1;
                    approve_history.int_version = checkapprohistory.Count() + 1;
                    if (lv != 1)
                    {
                        var firstOrDefault = _repoUser.Get(a =>a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName));
                        approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    }
                    approve_history.int_courseDetails_Id = course.Id;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                else
                {

                    approve_history.int_Course_id = course.CourseId;
                    approve_history.int_Requested_by = (int)userModel.USER_ID;
                    approve_history.int_id_status = type;
                    approve_history.int_Type = (int)UtilConstants.ApproveType.SubjectResult;
                    approve_history.dtm_requested_date = DateTime.Now;
                    //approve.int_Seq = 1;
                    approve_history.int_version = 1;
                    if (lv != 1)
                    {
                        var firstOrDefault = _repoUser.Get(a =>a.UserRoles.Any(x=>x.ROLE.NAME == hodRoleName));
                        approve_history.int_Approve_by = firstOrDefault?.ID ?? _repoUser.Get(a =>a.UserRoles.Any(x=>x.RoleId==1))?.ID;
                    }
                    approve_history.int_courseDetails_Id = course.Id;
                    _repoApprovesHistory.Insert(approve_history);
                    _uow.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

    }
}