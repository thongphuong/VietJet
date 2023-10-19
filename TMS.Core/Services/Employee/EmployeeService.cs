using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Web.Management;
using DAL.Entities;
using DAL.Repositories;
using DAL.UnitOfWork;
using TMS.Core.App_GlobalResources;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.ViewModel;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Employee;
using TMS.Core.ViewModels.Trainees;
using TMS.Core.ViewModels.UserModels;
using TMS.Core.Services.Configs;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using TMS.Core.ViewModels.Subjects;

namespace TMS.Core.Services.Employee
{

    public class EmployeeService : BaseService, IEmployeeService
    {
        private readonly IRepository<Trainee> _repoEmployee;
        private readonly IRepository<USER> _repoUser;
        private readonly IRepository<TraineeHistory> _repoTraineeHistory;
        private readonly IRepository<Trainee_Record> _repoTraineeRecord;
        private readonly IRepository<Trainee_Contract> _repoTraineeContract;
        private readonly IRepository<Instructor_Ability> _repoTraineeAbility;
        private readonly IRepository<Instructor_Ability_LOG> _repoTraineeAbility_Log;
        private readonly IRepository<Trainee_TrainingCenter> _repoTraineeCenter;
        private readonly IRepository<GroupTrainee> _repoGroupTrainee;
        private readonly IRepository<GroupTrainee_Item> _repoGroupTraineeItem;
        private readonly IRepository<TraineeFuture> _repoTraineeFuture;
        private readonly IRepository<Course_Result_Final> _repoCourseResultFinal;
        private readonly IRepository<Course_Detail> _repoCourseDetail;
        private readonly IRepository<TMS_APPROVES> _repoTmsApprove;
        private readonly IRepository<Trainee_Upload_Files> _repoTraineeUpload;
        private readonly IConfigService configService;
        private readonly IRepository<Trainee_Type> _reoiTraineeType;
        private readonly IRepository<INFO_CONTACT> _repoContact;
        private readonly IRepository<SentMailUser> _repoSentMailUser;
        private readonly IRepository<DAL.Entities.Department> _repoDepartMent;
        private readonly IRepository<JobTitle> _repoJobTitle;
        private readonly IRepository<Examiner_Ability> _repoExaminerAbility;
        private readonly IRepository<Examiner_Ability_LOG> _repoExaminerAbilityLog;
        private readonly IRepository<Monitor_Ability> _repoMonitorAbility;
        private readonly IRepository<Monitor_Ability_LOG> _repoMonitorAbilityLog;

        private readonly Expression<Func<GroupTrainee, bool>> _groupTraineeDefaultFilter = a => a.IsDeleted != true;
        private readonly Expression<Func<Trainee, bool>> _traineeDefaultFilter = a => a.IsDeleted != true;
        private readonly Expression<Func<Trainee_Record, bool>> _traineeRecordDefaultFilter =
            a => a.bit_Deleted == false && a.isActive == true;
        private readonly Expression<Func<Trainee_Contract, bool>> _traineeContractDefaultFilter =
            a => a.IsDeleted == false && a.IsActive == true;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public List<int> UserPermissions { get; set; } // currentUser permissions
        public EmployeeService(IUnitOfWork unitOfWork, IConfigService _configService,
             IRepository<SentMailUser> repoSentMailUser, IRepository<DAL.Entities.Department> repoDepartMent, IRepository<JobTitle> repoJobTitle,
            IRepository<TraineeFuture> repoTraineeFuture, IRepository<USER> repoUser, IRepository<GroupTrainee_Item> repoGroupTraineeItem, IRepository<GroupTrainee> repoGroupTrainee, IRepository<TraineeHistory> repoTraineeHistory, IRepository<Trainee> repoEmployee, IRepository<Trainee_Record> repoTraineeRecord, IRepository<Trainee_Contract> repoTraineeContract, IRepository<Instructor_Ability> repoTraineeAbility, IRepository<Trainee_TrainingCenter> repoTraineeCenter, IRepository<Course> repoCourse, IRepository<Instructor_Ability_LOG> repoTraineeAbility_Log, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<Course_Result_Final> repoCourseResultFinal, IRepository<Course_Detail> repoCourseDetail, IRepository<TMS_APPROVES> repoTmsApprove, IRepository<Trainee_Upload_Files> repoTraineeUpload, IRepository<Trainee_Type> reoiTraineeType, IRepository<INFO_CONTACT> repoContact, IRepository<Examiner_Ability> repoExaminerAbility, IRepository<Examiner_Ability_LOG> repoExaminerAbilityLog, IRepository<Monitor_Ability> repoMonitorAbility, IRepository<Monitor_Ability_LOG> repoMonitorAbilityLog) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoTraineeHistory = repoTraineeHistory;
            _repoSentMailUser = repoSentMailUser;
            _repoEmployee = repoEmployee;
            _repoJobTitle = repoJobTitle;
            _repoDepartMent = repoDepartMent;
            _repoTraineeRecord = repoTraineeRecord;
            _repoTraineeContract = repoTraineeContract;
            _repoTraineeAbility = repoTraineeAbility;
            _repoTraineeAbility_Log = repoTraineeAbility_Log;
            _repoTraineeCenter = repoTraineeCenter;
            _repoGroupTrainee = repoGroupTrainee;
            _repoGroupTraineeItem = repoGroupTraineeItem;
            _repoUser = repoUser;
            _repoTraineeFuture = repoTraineeFuture;
            _repoCourseResultFinal = repoCourseResultFinal;
            _repoCourseDetail = repoCourseDetail;
            _repoTmsApprove = repoTmsApprove;
            _repoTraineeUpload = repoTraineeUpload;
            configService = _configService;
            _reoiTraineeType = reoiTraineeType;
            _repoContact = repoContact;
            _repoExaminerAbility = repoExaminerAbility;
            _repoExaminerAbilityLog = repoExaminerAbilityLog;
            _repoMonitorAbility = repoMonitorAbility;
            _repoMonitorAbilityLog = repoMonitorAbilityLog;
        }

        #region send mail to portal user
        public void InsertSendMailUser(SentMailUser sentMailUser)
        {
            _repoSentMailUser.Insert(sentMailUser);
            Uow.SaveChanges();
        }

        public void UpdateSendMailUser(SentMailUser sentMailUser)
        {
            _repoSentMailUser.Update(sentMailUser);
            Uow.SaveChanges();
        }

        public SentMailUser GetByEmail(string email)
        {
            var query = _repoSentMailUser.GetAll().Where(x => x.Email.Equals(email)).ToList();
            return query[0];
        }

        public bool CheckExsistSendMailUser(string user, string email)
        {
            var query = _repoSentMailUser.GetAll().Where(x => x.Email.Equals(email) || x.Username.Equals(user)).ToList();
            if (query.Count > 0)
                return true;
            return false;
        }
        #endregion

        public Trainee GetById(int? id = null, bool isAdmin = false)
        {
            if (id == null) return null;
            var entity = _repoEmployee.Get(id);
            if (isAdmin)
            {
                return entity;
            }
            return entity == null || entity.IsDeleted == true ? null : entity;
        }
        public Trainee GetByCode(string code = null)
        {
            if (code == null) return null;
            var entity = _repoEmployee.Get(a => a.str_Staff_Id.ToLower().Trim() == code.ToLower().Trim());
            return entity == null || entity.IsDeleted == true ? null : entity;
        }
        public IQueryable<Trainee> GetInstructors(bool isApi = false)
        {
            const int instructorRole = (int)UtilConstants.ROLE.Instructor;
            IQueryable<Trainee> entities;
            if (isApi)
            {
                entities = _repoEmployee.GetAll(a => a.IsDeleted != true && a.IsActive == true && a.int_Role == instructorRole);
            }
            else
            {
                entities = UserPermissions == null ? new List<Trainee>().AsQueryable() : _repoEmployee.GetAll(a => a.IsDeleted == false && a.IsActive == true && UserPermissions.Contains(a.Department_Id.Value) && a.int_Role == instructorRole);
            }
            return entities;
        }
        public IQueryable<Trainee> GetInstructors(Expression<Func<Trainee, bool>> query, bool isApi = false)
        {
            const int instructorRole = (int)UtilConstants.ROLE.Instructor;
            IQueryable<Trainee> entities;
            if (isApi)
            {
                entities = _repoEmployee.GetAll(a => a.IsDeleted == false && a.IsActive == true && a.int_Role == instructorRole);
            }
            else
            {
                entities = (!isApi && UserPermissions == null)
                    ? new List<Trainee>().AsQueryable()
                    : _repoEmployee.GetAll(
                        a =>
                            a.IsDeleted == false && a.IsActive == true && UserPermissions.Contains(a.Department_Id.Value) &&
                            a.int_Role == instructorRole);
            }
            return query == null ? entities : entities.Where(query);
        }

        public IQueryable<Trainee> Get(bool isApi = false)
        {
            IQueryable<Trainee> entities;


            if (isApi)
            {
                entities = _repoEmployee.GetAll(_traineeDefaultFilter);
            }
            else
            {
                entities = UserPermissions == null ? new List<Trainee>().AsQueryable() : _repoEmployee.GetAll(a => a.IsDeleted == false && UserPermissions.Contains(a.Department_Id.Value));
            }
            return entities;
        }

        public IQueryable<Trainee> Get(Expression<Func<Trainee, bool>> query, bool isApi = false)
        {
            IQueryable<Trainee> entities;
            if (isApi)
            {
                entities = _repoEmployee.GetAll();
            }
            else
            {
                entities = UserPermissions == null ? new List<Trainee>().AsQueryable() : _repoEmployee.GetAll(a => a.IsDeleted == false && UserPermissions.Contains(a.Department_Id.Value));
            }
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public List<Trainee> GetAll()
        {
            return _repoEmployee.GetAll(a => a.IsDeleted == false).OrderBy(a => a.Id).ToList();
        }

        public IQueryable<Trainee> GetEmp()
        {
            return _repoEmployee.GetAll(a => a.IsDeleted == false && a.int_Role == (int)UtilConstants.ROLE.Trainee).OrderBy(a => a.Id);

        }

        public IQueryable<Trainee> GetEmp(Expression<Func<Trainee, bool>> query)
        {
            var entities = _repoEmployee.GetAll(a => a.IsDeleted == false && a.int_Role == (int)UtilConstants.ROLE.Trainee);
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Instructor_Ability> GetInstruc_Ability()
        {
            return _repoTraineeAbility.GetAll().OrderBy(a => a.id);
        }
        public IQueryable<Instructor_Ability> GetInstruc_Ability(Expression<Func<Instructor_Ability, bool>> query)
        {
            var entities = _repoTraineeAbility.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public List<Trainee> BulkInsertTrainee(List<Trainee_Validation> lstModel, int currentUserId, int count)
        {
            List<Trainee> lstTrainee = new List<Trainee>();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var model = lstModel[i];
                    Trainee entity;
                    if (!model.Id.HasValue)
                    {

                        var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id == model.eid && a.IsDeleted == false && a.IsActive == true);
                        if (datacheckEID.Any())
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(model.mail) && _repoEmployee.GetAll(a => a.IsDeleted == false && a.IsActive == true && a.str_Email.Equals(model.mail)).Any())
                        {
                            continue;
                        }
                        entity = new Trainee
                        {
                            dtm_Created_At = DateTime.Now,
                            str_Created_By = currentUserId.ToString(),
                            IsDeleted = false,
                            IsActive = true,
                            Password = model.password
                        };
                    }
                    else
                    {
                        var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id == model.eid && a.Id != model.Id && a.IsDeleted == false && a.IsActive == true);
                        if (datacheckEID.Any())
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(model.mail) && _repoEmployee.GetAll(a => a.IsDeleted == false && a.IsActive == true && a.str_Email.Equals(model.mail) && a.Id != model.Id).Any())
                        {
                            continue;
                        }
                        entity = _repoEmployee.Get(model.Id);
                        entity.dtm_Last_Modified_At = DateTime.Now;
                        entity.str_Last_Modified_By = currentUserId.ToString();
                        entity.Password = model.password;
                        _repoEmployee.Update(entity);

                        ////////////////////////////////////////
                    }
                    entity.int_Role = model.Role == (int)UtilConstants.ROLE.Instructor
                        ? (int)UtilConstants.ROLE.Instructor
                        : (int)UtilConstants.ROLE.Trainee;
                    entity.bit_Internal = model.type == (int)UtilConstants.CourseAreas.Internal;

                    var fullnamelength = model.FullName.Split(' ').Length;

                    var firstName = model.FirstName;
                    var lastMame = model.LastName;

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastMame))
                    {
                        continue;
                    }


                    entity.str_Staff_Id = model.eid;
                    entity.Passport = model.passport;
                    entity.PersonalId = model.str_id;
                    entity.FirstName = firstName;
                    entity.LastName = lastMame;
                    entity.dtm_Birthdate = model.dtm_Birthdate;
                    entity.Gender = model.gender ?? 3;
                    entity.str_Place_Of_Birth = model.str_Place_Of_Birth;
                    entity.str_Email = model.mail;
                    entity.Nation = model.nation;
                    entity.str_Cell_Phone = model.phone;
                    entity.Job_Title_id = model.Job_Title_id;

                    entity.Department_Id = model.Department_Id;
                    entity.Company_Id = model.Company_Id;
                    entity.dtm_Join_Date = model.dtm_Join_Date;
                    entity.non_working_day = model.Resignation_Date;
                    entity.avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.avatar : model.ImgAvatar;

                    // API sang LMS
                    entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;

                    lstTrainee.Add(entity);

                }

                int countlstTrainee = lstTrainee.Count;

                if (countlstTrainee > 0)
                {
                    _repoEmployee.Insert(lstTrainee);
                    Uow.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return lstTrainee;
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
        public Trainee Modify(Trainee_Validation model, string password = null)
        {
            var culture = GetCulture();
            Trainee entity;
            if (!model.Id.HasValue)
            {
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == (model.eid.ToLower().Trim() + "") /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.eid));
                }
                if (!string.IsNullOrEmpty(model.mail) && _repoEmployee.GetAll(a => /*!a.IsDeleted && a.IsActive == true &&*/ a.str_Email.Equals(model.mail)).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.mail));
                }
                entity = new Trainee
                {
                    str_Staff_Id = model.eid,
                    dtm_Created_At = DateTime.Now,
                    str_Created_By = CurrentUser.USER_ID.ToString(),
                    IsDeleted = false,
                    IsActive = true,
                    Password = password
                };
                _repoEmployee.Insert(entity);
            }
            else
            {
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == (model.eid.ToLower().Trim() + "") && a.Id != model.Id /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.EXISTING_EID, model.eid));
                }
                if (!string.IsNullOrEmpty(model.mail) && _repoEmployee.GetAll(a => /*!a.IsDeleted && a.IsActive == true &&*/ a.str_Email.Equals(model.mail) && a.Id != model.Id).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.mail));
                }
                entity = _repoEmployee.Get(model.Id);
                entity.dtm_Last_Modified_At = DateTime.Now;
                entity.str_Last_Modified_By = CurrentUser.USER_ID.ToString();
                //entity.Password = password;
                _repoEmployee.Update(entity);
            }
            entity.int_Role = model.Role == (int)UtilConstants.ROLE.Instructor
                ? (int)UtilConstants.ROLE.Instructor
                : (int)UtilConstants.ROLE.Trainee;
            entity.bit_Internal = model.type == (int)UtilConstants.CourseAreas.Internal;

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firstName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastMame = model.FullName.Split(' ')[fullnamelength - 1];
            //if (firstName == "" || firstName == " " || lastMame == "" || lastMame == " ") throw new Exception( Messege.VALIDATION_FULLNAME );
            var checkstr = model.FullName.Replace('\u00A0', ' ');
            var firstName = culture == "vi" ? checkstr.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim() : checkstr.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            var lastName = model.FullName.Replace(firstName, "").Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }

            //Khong cho sua Code
            //entity.str_Staff_Id = model.eid;
            entity.Passport = model.passport;
            entity.PersonalId = model.str_id;
            entity.FirstName = lastName;
            entity.LastName = firstName;
            entity.dtm_Birthdate = model.dtm_Birthdate;
            entity.Gender = model.gender ?? 3;
            entity.str_Place_Of_Birth = model.str_Place_Of_Birth;
            entity.str_Email = model.mail;
            entity.Nation = model.nation;
            entity.str_Cell_Phone = model.phone;
            entity.Job_Title_id = model.Job_Title_id;
            entity.Department_Id = model.Department_Id;
            entity.Company_Id = model.Company_Id;
            entity.dtm_Join_Date = model.dtm_Join_Date;
            entity.non_working_day = model.Resignation_Date;
            entity.avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.avatar : model.ImgAvatar;

            // API sang LMS
            entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;

            UpdateEmployeeTraineeRecord(model.Educations, ref entity);


            /////////////////////   Job History     //////////////////////
            var create = false;
            var dataCheckJobId = _repoTraineeHistory.GetAll(a => a.Trainee_Id == model.Id).OrderByDescending(a => a.Id).FirstOrDefault();
            if (dataCheckJobId == null)
            {
                create = true;
            }
            else
            {
                if (dataCheckJobId.Job_Title_Id != model.Job_Title_id)
                {
                    create = true;
                }
            }

            Uow.SaveChanges();



            if (create)
            {
                var entityHistory = new TraineeHistory()
                {
                    Job_Title_Id = model.Job_Title_id,
                    Trainee_Id = model.Id ?? entity.Id,
                    dtm_Create_At = DateTime.Now,
                };
                _repoTraineeHistory.Insert(entityHistory);

            }

            if (create)
            {
                UpdateHistoryItem();
            }

            Uow.SaveChanges();
            return entity;
        }


        private void UpdateHistoryItem()
        {
            var traineeHistories = _repoTraineeHistory.GetAll(a => (a.Status == null || a.Status == (int)UtilConstants.StatusScheduler.Modify)).OrderByDescending(a => a.Id).Take(5).ToList();
            if (traineeHistories.Any())
            {
                foreach (var traineeHistory in traineeHistories)
                {
                    var titleStandard = traineeHistory.JobTitle?.Title_Standard?.ToList();

                    if (titleStandard != null)
                    {
                        if (titleStandard.Any())
                        {
                            var subjectIds = titleStandard?.Select(a => a.Subject_Id).ToArray();
                            var getCourseid = _repoCourseResultFinal.GetAll(a => a.traineeid == traineeHistory.Trainee_Id && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid).ToArray();
                            var courseidAssign = _repoCourseDetail.GetAll(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct().ToArray();
                            var courseDetailCompleted = _repoTmsApprove.GetAll(a => getCourseid.Contains(a.int_Course_id) && a.int_Type == (int)UtilConstants.ApproveType.CourseResult && a.int_id_status == (int)UtilConstants.EStatus.Approve).Select(a => a.Course.Course_Detail);

                            var subjectIdCompleted = new int[] { };

                            if (courseDetailCompleted.Any())
                            {
                                foreach (var item in courseDetailCompleted)
                                {
                                    subjectIdCompleted = item.Select(a => (int)a.SubjectDetailId).ToArray();
                                }
                            }
                            if (subjectIds != null)
                            {
                                foreach (var subjectId in subjectIds)
                                {
                                    var traineeHistoryItem = new TraineeHistory_Item();
                                    traineeHistoryItem.SubjectId = subjectId;
                                    traineeHistoryItem.TraineeHistoryId = traineeHistory.Id;

                                    if (subjectIdCompleted.Length != 0)
                                    {
                                        traineeHistoryItem.Status = (int)UtilConstants.StatusTraineeHistory.Completed;
                                    }
                                    else
                                    {
                                        traineeHistoryItem.Status = courseidAssign.Contains(subjectId)
                                            ? (int)UtilConstants.StatusTraineeHistory.Trainning
                                            : (int)UtilConstants.StatusTraineeHistory.Missing;
                                    }
                                    traineeHistory.TraineeHistory_Item.Add(traineeHistoryItem);

                                }
                            }
                        }
                        

                    }
                    traineeHistory.Status = (int)UtilConstants.StatusScheduler.Synchronize;
                    traineeHistory.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                    _repoTraineeHistory.Update(traineeHistory);
                }
            }
        }



        private void UpdateEmployeeTraineeRecord(IEnumerable<TraineeEducation> educations, ref Trainee trainee)
        {
            var now = DateTime.Now;
            //if (trainee.Trainee_Record.Any())
            //{
            //    _repoTraineeRecord.Delete(trainee.Trainee_Record);
            //}
            //trainee.Trainee_Record.Clear();
            #region [------education-------]

            if (educations != null)
            {
                foreach (var traineeRecord in educations.Where(a => !string.IsNullOrEmpty(a.CourseName)))
                {
                    var listRecordFile = new List<Trainee_Upload_Files>();



                    foreach (var fileUpload in traineeRecord.FileUploads.Where(fileUpload => fileUpload?.ListNameImage[0] != null))
                    {
                        listRecordFile.AddRange(fileUpload.ListNameImage.Select(item => new Trainee_Upload_Files()
                        {
                            CreationDate = now,
                            Name = item,
                            IsDeleted = false
                        }));
                    }
                    if (traineeRecord.Id.HasValue)
                    {
                        var modelRecord = trainee.Trainee_Record;
                        var model = modelRecord.FirstOrDefault(a => a.Trainee_Record_Id == traineeRecord.Id);
                        if (model != null)
                        {
                            model.str_Subject = traineeRecord.CourseName;
                            model.str_organization = traineeRecord.Organization;
                            model.str_note = traineeRecord.Note;
                            model.dtm_time_from = traineeRecord.TimeFrom;
                            model.dtm_time_to = traineeRecord.TimeTo;
                            model.dtm_Modified_At = now;
                            if (listRecordFile.Any())
                            {
                                model.Trainee_Upload_Files = listRecordFile;
                            }
                            _repoTraineeRecord.Update(model);
                        }
                    }
                    else
                    {
                        trainee.Trainee_Record.Add(new Trainee_Record()
                        {
                            Trainee = trainee,
                            dtm_time_from = traineeRecord.TimeFrom,
                            dtm_time_to = traineeRecord.TimeTo,
                            str_Subject = traineeRecord.CourseName,
                            str_organization = traineeRecord.Organization,
                            str_note = traineeRecord.Note,
                            dtm_Created_At = now,
                            isActive = true,
                            bit_Deleted = false,
                            Trainee_Upload_Files = listRecordFile.Any() ? listRecordFile : null
                        });
                    }



                }
            }
            #endregion
        }


        public void Update(Trainee entity)
        {
            _repoEmployee.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(Trainee entity)
        {
            var depart = _repoDepartMent.GetAll(x => x.Id == entity.Department_Id).FirstOrDefault();
            var jobtitle = _repoJobTitle.GetAll(x => x.Id == entity.Job_Title_id).FirstOrDefault();
            _repoEmployee.Insert(entity);
            Uow.SaveChanges();
        }

        public bool InsertImport(Trainee entity)
        {
            var depart = _repoDepartMent.GetAll(x => x.Id == entity.Department_Id).FirstOrDefault();
            var jobtitle = _repoJobTitle.GetAll(x => x.Id == entity.Job_Title_id).FirstOrDefault();
            _repoEmployee.Insert(entity);
            Uow.SaveChanges();
            return true;

        }
        public Trainee InsertImportCustom(Trainee entity)
        {
            //var depart = _repoDepartMent.GetAll(x => x.Id == entity.Department_Id).FirstOrDefault();
            //var jobtitle = _repoJobTitle.GetAll(x => x.Id == entity.Job_Title_id).FirstOrDefault();
            _repoEmployee.Insert(entity);
            Uow.SaveChanges();
            return entity;

        }

        public void Update1(TraineeHistory entity)
        {
            _repoTraineeHistory.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert1(TraineeHistory entity)
        {
            _repoTraineeHistory.Insert(entity);
            Uow.SaveChanges();
        }
        public void Update2(TraineeFuture entity)
        {
            _repoTraineeFuture.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert2(TraineeFuture entity)
        {
            _repoTraineeFuture.Insert(entity);
            Uow.SaveChanges();
        }
        public Trainee Modify(InstructorValidation model, string password = null)
        {
            var culture = GetCulture();
            Trainee entity;
            Instructor_Ability_LOG log;

            if (!model.Id.HasValue)
            {
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == (model.Eid.ToLower().Trim() + "") /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.Eid));
                }
                if (!string.IsNullOrEmpty(model.Mail) && _repoEmployee.GetAll(a => /*!a.IsDeleted && a.IsActive == true &&*/ a.str_Email.Equals(model.Mail)).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.Mail));
                }
                entity = new Trainee { str_Staff_Id = model.Eid, dtm_Created_At = DateTime.Now, IsActive = true, IsDeleted = false, str_Created_By = CurrentUser.USER_ID.ToString(), int_Role = (int)UtilConstants.ROLE.Instructor, Password = password };
                _repoEmployee.Insert(entity);
            }
            else
            {
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == (model.Eid.ToLower().Trim() + "") && a.Id != model.Id /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.Eid));
                }
                if (!string.IsNullOrEmpty(model.Mail) && _repoEmployee.GetAll(a => /*!a.IsDeleted && a.IsActive == true &&*/ a.str_Email.Equals(model.Mail) && a.Id != model.Id).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.Mail));
                }
                entity = _repoEmployee.Get(model.Id);
                entity.dtm_Last_Modified_At = DateTime.Now;
                entity.str_Last_Modified_By = CurrentUser.USER_ID.ToString();
                entity.Password = password;
                _repoEmployee.Update(entity);
                /////////////////////   Job History     //////////////////////
                var dataCheckJobId = _repoTraineeHistory.GetAll();
                if (dataCheckJobId.Any(a => a.Job_Title_Id == model.JobTitleId && a.Trainee_Id == model.Id))
                {

                }
                else
                {
                    var entityHistory = new TraineeHistory()
                    {
                        Job_Title_Id = model.JobTitleId,
                        Trainee_Id = model.Id,
                        dtm_Create_At = DateTime.Now,

                    };
                    _repoTraineeHistory.Insert(entityHistory);
                }
                ////////////////////////////////////////
            }

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firtName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastName = model.FullName.Split(' ')[fullnamelength - 1];
            //if (firtName == "" || firtName == " " || lastName == "" || lastName == " ") throw new Exception( Messege.VALIDATION_FULLNAME );

            var firstName = culture == "vi" ? model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim() : model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            var lastName = model.FullName.Replace(firstName, "").Trim();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }

            entity.bit_Internal = model.EmployeeType == 1;
            entity.avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.avatar : model.ImgAvatar;

            //entity.str_Staff_Id = model.Eid;
            entity.Passport = model.Passport;
            entity.PersonalId = model.PersonalId;
            entity.FirstName = lastName;
            entity.LastName = firstName;
            entity.dtm_Birthdate = model.Birthdate;
            entity.Gender = model.Gender ?? 3;
            entity.str_Place_Of_Birth = model.PlaceOfBirth;
            entity.str_Email = model.Mail;
            entity.Nation = model.Nation;
            entity.str_Cell_Phone = model.Phone;
            entity.Job_Title_id = model.JobTitleId;
            entity.Department_Id = model.Department_Id;
            entity.Company_Id = model.CompanyId;
            entity.dtm_Join_Date = model.JoinedDate;
            entity.Allowance = Convert.ToDouble(model.TrainingAllowance);

            //update lmsStatus
            entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;

            #region[Insert edu]


            UpdateEmployeeTraineeRecord(model.Educations, ref entity);
            //if (entity.Trainee_Record.Any())
            //{
            //    _repoTraineeRecord.Delete(entity.Trainee_Record);
            //}
            //entity.Trainee_Record.Clear();
            //if (model.Educations != null)
            //{
            //    foreach (var edu in model.Educations)
            //    {
            //        entity.Trainee_Record.Add(new Trainee_Record
            //        {
            //            Trainee = entity,
            //            dtm_time_from = edu.TimeFrom,
            //            dtm_time_to = edu.TimeTo,
            //            str_Subject = edu.CourseName,
            //            str_organization = edu.Organization,
            //            str_note = edu.Note,
            //            dtm_Created_At = DateTime.Now,
            //            isActive = true,
            //            bit_Deleted = false
            //        });
            //    }
            //}
            #endregion
            #region[Insert Contract]

            if (entity.Trainee_Contract.Any())
            {
                _repoTraineeContract.Delete(entity.Trainee_Contract);
            }
            entity.Trainee_Contract.Clear();
            if (model.Contracts != null)
            {
                foreach (var contract in model.Contracts)
                {
                    entity.Trainee_Contract.Add(new Trainee_Contract
                    {
                        dtm_Created_At = DateTime.Now,
                        expire_date = contract.ExpireDate,
                        contractno = contract.ContractNo,
                        description = contract.Description,
                        IsActive = true,
                        IsDeleted = false
                    });
                }

            }
            #endregion
            #region[Insert Instructor_Ability]

            if (model.InstructorSubject != null)
            {
                _repoTraineeAbility.Delete(entity.Instructor_Ability);
                foreach (var ability in model.InstructorSubject)
                {
                    entity.Instructor_Ability.Add(new Instructor_Ability { Allowance = ability.Allowance, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
                }
            }
            else
            {
                _repoTraineeAbility.Delete(entity.Instructor_Ability);
            }
            #endregion

            Uow.SaveChanges();
            if (model.InstructorSubject != null)
            {
                var Listlog = _repoTraineeAbility_Log.GetAll(a => a.InstructorId == entity.Id && a.IsDeleted == false);
                if (Listlog.Any())
                {
                    foreach (var item in Listlog)
                    {
                        item.ModifyBy = CurrentUser.USER_ID;
                        item.ModifyDate = DateTime.Now;
                        item.IsDeleted = true;
                        _repoTraineeAbility_Log.Update(item);
                    }
                }
                foreach (var ability in model.InstructorSubject)
                {
                    log = new Instructor_Ability_LOG()
                    {
                        InstructorId = entity.Id,
                        SubjectDetailId = ability.SubjectId,
                        Allowance = ability.Allowance ?? 0,
                        CreateDate = DateTime.Now,
                        CreateBy = CurrentUser.USER_ID,
                        IsDeleted = false
                    };
                    _repoTraineeAbility_Log.Insert(log);
                }
            }
            else
            {
                var Listlog = _repoTraineeAbility_Log.GetAll(a => a.InstructorId == entity.Id && a.IsDeleted == false);
                if (Listlog.Any())
                {
                    foreach (var item in Listlog)
                    {
                        item.IsDeleted = true;
                        item.ModifyBy = CurrentUser.USER_ID;
                        item.ModifyDate = DateTime.Now;
                        _repoTraineeAbility_Log.Update(item);
                    }
                }
            }
            Uow.SaveChanges();


            return entity;
        }

        public IQueryable<Trainee_Record> GetRecord(Expression<Func<Trainee_Record, bool>> query)
        {
            var entities = _repoTraineeRecord.GetAll(_traineeRecordDefaultFilter);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public IQueryable<Trainee_Record> GetRecordByTraineeId(int traineeId)
        {
            return _repoTraineeRecord.GetAll(a => a.Trainee_Id == traineeId);
        }

        public Trainee_Record GetRecordId(int id)
        {
            return _repoTraineeRecord.Get(id);
        }

        public void UpdateRecord(Trainee_Record entity)
        {
            _repoTraineeRecord.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertRecord(Trainee_Record entity)
        {
            _repoTraineeRecord.Insert(entity);
            Uow.SaveChanges();
        }

        public IQueryable<Trainee_Contract> GetContract(Expression<Func<Trainee_Contract, bool>> query)
        {
            var entities = _repoTraineeContract.GetAll(_traineeContractDefaultFilter);
            if (query != null) entities = entities.Where(query);
            return entities;

        }

        public IQueryable<Trainee_Contract> GetContractByTraineeId(int employeeId)
        {
            return _repoTraineeContract.GetAll(a => a.Trainee_Id == employeeId);
        }

        public Trainee_Contract GetContractId(int id)
        {
            return _repoTraineeContract.Get(id);
        }

        public void UpdateContract(Trainee_Contract entity)
        {
            _repoTraineeContract.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertContract(Trainee_Contract entity)
        {
            _repoTraineeContract.Insert(entity);
            Uow.SaveChanges();
        }

        public void DeleteContract(Trainee_Contract entity)
        {
            _repoTraineeContract.Delete(entity);
            Uow.SaveChanges();
        }

        public IQueryable<Instructor_Ability> GetAbilityTraineeId(int employeeId)
        {
            return _repoTraineeAbility.GetAll(a => a.InstructorId == employeeId);
        }

        public Instructor_Ability GetAbility(int id)
        {
            return _repoTraineeAbility.Get(id);
        }

        public IQueryable<Instructor_Ability> GetAbility(Expression<Func<Instructor_Ability, bool>> query)
        {
            return _repoTraineeAbility.GetAll(query);
        }

        public IQueryable<Examiner_Ability> GetExaminerAbility(Expression<Func<Examiner_Ability, bool>> query)
        {
            return _repoExaminerAbility.GetAll(query);
        }
        public void UpdateAbility(Instructor_Ability entity)
        {
            _repoTraineeAbility.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertAbility(Instructor_Ability entity)
        {
            _repoTraineeAbility.Insert(entity);
            Uow.SaveChanges();
        }

        public void DeleteAbility(Instructor_Ability entity)
        {
            _repoTraineeAbility.Delete(entity);
            Uow.SaveChanges();
        }

        public void InsertAbilityLog(Instructor_Ability_LOG entity)
        {
            _repoTraineeAbility_Log.Insert(entity);
            Uow.SaveChanges();
        }
        public IQueryable<Trainee_TrainingCenter> GetTraineeCenterByEmployeeId(int employeeId)
        {
            return _repoTraineeCenter.GetAll(a => a.instructor_id == employeeId && a.Trainee.IsDeleted == false);
        }

        public Trainee_TrainingCenter GetTraineeCenter(int id)
        {
            var entity = _repoTraineeCenter.Get(id);
            return entity?.instructor_id == null || entity.Trainee.IsDeleted == true ? null : entity;
        }

        public IQueryable<Trainee_TrainingCenter> GetTraineeCenter(Expression<Func<Trainee_TrainingCenter, bool>> query)
        {
            var entities = _repoTraineeCenter.GetAll(a => a.Trainee.IsDeleted == false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateTraineeCenter(Trainee_TrainingCenter entity)
        {
            _repoTraineeCenter.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertTraineeCenter(Trainee_TrainingCenter entity)
        {
            _repoTraineeCenter.Insert(entity);
            Uow.SaveChanges();
        }

        public void DeleteTraineeCenter(Trainee_TrainingCenter entity)
        {
            _repoTraineeCenter.Delete(entity);
            Uow.SaveChanges();
        }

        public List<sp_GetTrainingHeaderTV_Result> sp_GetTrainingHeader(string courseId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateEnd, string status)
        {
            return Uow.sp_GetTrainingHeader_Result(courseId, departmentCode, fromDateStart, fromDateEnd, toDateStart, toDateEnd, status).ToList();
        }

        public List<sp_GetTrainingDetail_Result> sp_GetTrainingDetail(string courseId, string departmentCode, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateEnd, string status)
        {
            return Uow.sp_GetTrainingDetail_Result(courseId, departmentCode, fromDateStart, fromDateEnd, toDateStart, toDateEnd, status).ToList();
        }
        #region [--------------------Trainee PDP-----------------------------]

        public IQueryable<TraineeFuture> GetAllPdp(Expression<Func<TraineeFuture, bool>> query)
        {
            var entities = _repoTraineeFuture.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        #endregion
        #region [--------------------Trainee History-----------------------------]

        public IQueryable<TraineeHistory> GetAllTraineeHistories(Expression<Func<TraineeHistory, bool>> query)
        {
            var entities = _repoTraineeHistory.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        #endregion



        #region [-------------------------------- Group Trainee ---------------------------------------]
        public GroupTrainee GetGroupById(int? id = null)
        {
            if (id == null) return null;
            var entity = _repoGroupTrainee.Get(id);
            return entity == null || entity.IsDeleted == true ? null : entity;
        }

        public GroupTrainee GetGroupTrainee()
        {
            return _repoGroupTrainee.Get(_groupTraineeDefaultFilter);
        }

        public IQueryable<GroupTrainee> GetAllGroupTrainees(Expression<Func<GroupTrainee, bool>> query)
        {
            var entities = _repoGroupTrainee.GetAll(_groupTraineeDefaultFilter);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void Update(GroupTrainee entity)
        {
            _repoGroupTrainee.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(GroupTrainee entity)
        {
            _repoGroupTrainee.Insert(entity);
            Uow.SaveChanges();
        }

        public GroupTrainee Modify(EmployeeGroupModify model)
        {
            if (model == null) return null;
            var now = DateTime.Now;
            var entity = _repoGroupTrainee.Get(model.Id);
            if (_repoGroupTrainee.GetAll(m => m.Code == model.Code && m.Id != model.Id).Any())
            {
                throw new Exception(string.Format(Messege.DataIsExists, Resource.lblGroupTrainee, model.Code));
            }
            if (entity != null)
            {
                entity.Code = model.Code;
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.ModifiedDate = now;
                entity.ModifiedBy = CurrentUser.USER_ID;
                _repoGroupTrainee.Update(entity);

                var groupItem = entity.GroupTrainee_Item.ToList();
                foreach (var traineeId in groupItem.Where(a => model.TraineeIds.All(x => x != a.TraineeId)))
                {
                    _repoGroupTraineeItem.Delete(traineeId);
                }
                //add groups
                foreach (var traineeId in model.TraineeIds.Where(a => groupItem.All(x => x.TraineeId != a)))
                {
                    entity.GroupTrainee_Item.Add(new GroupTrainee_Item()
                    {
                        GroupTrainee = entity,
                        TraineeId = traineeId,
                    });
                }
            }
            else
            {
                entity = new GroupTrainee
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    IsDeleted = false,
                    IsActived = true,
                    CreatedDate = now,
                    CreatedBy = CurrentUser.USER_ID

                };

                //add group
                foreach (var traineeId in model.TraineeIds)
                {
                    entity.GroupTrainee_Item.Add(new GroupTrainee_Item()
                    {
                        GroupTrainee = entity,
                        TraineeId = traineeId,
                    });
                }
                _repoGroupTrainee.Insert(entity);
            }

            Uow.SaveChanges();
            return entity;
        }


        #endregion


        #region [--------------------DH DA NANG-----------------------]

        public int InsertEmployee_DHDaNang(APIEmployeeDHDaNang[] model)
        {
            try
            {
                if (model == null || !model.Any()) return (int)UtilConstants.StatusApiDHDaNang.Null;
                foreach (var item in model)
                {
                    if (string.IsNullOrEmpty(item.Username)) return (int)UtilConstants.StatusApiDHDaNang.UsernameIsNull;
                    if (string.IsNullOrEmpty(item.Password)) return (int)UtilConstants.StatusApiDHDaNang.PasswordIsNull;

                    //UserName(Email)  la Code(str_Staff_Id)
                    var entity = _repoEmployee.Get(a => a.str_Staff_Id.ToLower().Trim() == item.Username);
                    if (entity != null)
                    {
                        entity.str_Staff_Id = item.Username;
                        entity.Password = Common.EncryptString(item.Password);
                        entity.str_Email = item.Username;
                        entity.FirstName = item.Lastname;
                        entity.LastName = item.Firstname;
                        entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                        entity.int_Role = (int)UtilConstants.ROLE.Trainee;
                        entity.IsDeleted = false;
                        entity.IsActive = true;
                        entity.str_Created_By = CurrentUser.USER_ID.ToString();
                        entity.dtm_Created_At = DateTime.Now;
                        entity.Gender = (int)UtilConstants.Gender.Others;
                        _repoEmployee.Update(entity);
                        Uow.SaveChanges();
                        return (int)UtilConstants.StatusApiDHDaNang.Modify;
                    }
                    entity = new Trainee()
                    {
                        str_Staff_Id = item.Username,
                        Password = Common.EncryptString(item.Password),
                        str_Email = item.Username,
                        FirstName = item.Lastname,
                        LastName = item.Firstname,
                        LmsStatus = (int)UtilConstants.ApiStatus.Modify,
                        int_Role = (int)UtilConstants.ROLE.Trainee,
                        IsDeleted = false,
                        IsActive = true,
                        str_Created_By = CurrentUser.USER_ID.ToString(),
                        dtm_Created_At = DateTime.Now,
                        Gender = (int)UtilConstants.Gender.Others
                    };
                    _repoEmployee.Insert(entity);
                    Uow.SaveChanges();
                    return (int)UtilConstants.StatusApiDHDaNang.Insert;
                }
            }
            catch (Exception ex)
            {

                return (int)UtilConstants.StatusApiDHDaNang.Undefined;
            }
            return (int)UtilConstants.StatusApiDHDaNang.Undefined;
        }
        public AjaxResponseViewModel InsertContact(APIContact model, string currentUser)
        {
            var result = new AjaxResponseViewModel();
            try
            {

                if (string.IsNullOrEmpty(model?.FullName))
                {
                    result.result = false;
                    result.message = Messege.VALIDATION_FULLNAME_REQUIED;
                    return result;
                }
                if (string.IsNullOrEmpty(model?.Email))
                {
                    result.result = false;
                    result.message = Messege.VALIDATION_EMAIL_REQUIED;
                    return result;
                }
                if (string.IsNullOrEmpty(model?.Subject))
                {
                    result.result = false;
                    result.message = Messege.VALIDATION_CONTENT_MAIL;
                    return result;
                }
                var entity = new INFO_CONTACT();
                entity.FullName = model?.FullName;
                entity.Phone = model?.Phone;
                entity.Email = model?.Email;
                entity.Subject = model?.Subject;
                entity.Content = model?.Content;
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.CreationDate = DateTime.Now;
                _repoContact.Insert(entity);
                Uow.SaveChanges();
                result.result = true;
                result.message = Messege.SUCCESS;
                return result;
            }
            catch (Exception)
            {
                result.result = false;
                result.message = string.Format(Messege.ERROR_NOTFOUND, "Server");
                return result;
            }
        }
        #endregion

        #region [------------------Chang Password tu LMS----------------------------]


        public bool UpdateApi(APIChangePasswordTrainee entity, string currentUser)
        {
            try
            {
                if (entity == null) return false;
                var model =
                    _repoEmployee.Get(a => a.str_Staff_Id.ToLower().Trim() == entity.TraineeCode.ToLower().Trim());
                if (model == null) return false;
                var password = string.IsNullOrEmpty(entity.Password) ? Common.EncryptString("Tinhvan@123") : Common.EncryptString(entity.Password);
                model.Password = password;
                model.str_Last_Modified_By = currentUser;
                model.dtm_Last_Modified_At = DateTime.Now;
                _repoEmployee.Update(model);
                var userModel = model.USERs.FirstOrDefault();
                if (userModel != null)
                {
                    userModel.PASSWORD = password;
                    userModel.LAST_UPDATED_BY = currentUser;
                    userModel.LAST_UPDATE_DATE = DateTime.Now;
                    _repoUser.Update(userModel);
                }
                Uow.SaveChanges();
                return true;

            }
            catch (Exception)
            {

                return false;
            }
        }
        private void CheckFileDefault(string name)
        {
            var path = configService.GetByKey("PathServerAdmin");
            var isExists = System.IO.Directory.Exists(path);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var pathFile = path + name;
            var imgDefaultIsExists = System.IO.File.Exists(pathFile);
            if (!imgDefaultIsExists)
            {
                System.IO.File.Copy(path + "/Content/img/" + name, pathFile);
            }
        }
        //Save img with extension
        private string SaveImg(string url, string Eid)
        {
            var nameImg = string.Empty;
            var extensionUrl = url.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last();
            var path = configService.GetByKey("PathServerAdmin");
            if (Common.CheckImgFile("." + extensionUrl))
            {
                CreateFolderIfExists(path);
                var random = new Random();
                var suffDate = configService.GetByKey("SuffixesDateFormat");
                var name = DateTime.Now.ToString(suffDate);
                nameImg = Eid + "_" + name + random.Next(1, 100) + "." + extensionUrl;
                var webClient = new WebClient();
                var combine = Path.Combine(path, nameImg);
                webClient.DownloadFile(url, combine);
                webClient.Dispose();
            }
            return nameImg;
        }
        //Save img without extension
        private string SaveImgWithoutExtension(string url, string Eid)
        {
            var nameImg = string.Empty;
            var path = configService.GetByKey("PathServerAdmin");
            CreateFolderIfExists(path);
            var random = new Random();
            var suffDate = configService.GetByKey("SuffixesDateFormat");
            var name = DateTime.Now.ToString(suffDate);
            nameImg = Eid + "_" + name + random.Next(1, 100) + ".png";
            var webClient = new WebClient();
            var combine = Path.Combine(path, nameImg);
            webClient.DownloadFile(url, combine);
            webClient.Dispose();
            return nameImg;
        }
        private bool CreateFolderIfExists(string parentFolder)
        {
            var path = parentFolder;
            bool isExists = System.IO.Directory.Exists(path);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }
        public bool UpdateApiUser(APIChangePasswordTrainee entity)
        {
            try
            {
                if (entity == null) return false;
                var model =
                    _repoEmployee.Get(a => a.str_Staff_Id.ToLower().Trim() == entity.TraineeCode.ToLower().Trim());
                if (model == null) return false;
                if (!string.IsNullOrEmpty(entity.UrlAvatar))
                {
                    var checkName = SaveImgWithoutExtension(entity.UrlAvatar, model.str_Staff_Id);
                    var defaultNameImg = string.IsNullOrEmpty(checkName) ? model.avatar : checkName;
                    model.avatar = defaultNameImg;
                }
                model.str_Last_Modified_By = model.str_Staff_Id;
                model.dtm_Last_Modified_At = DateTime.Now;
                model.Passport = entity.Passport;
                model.dtm_Birthdate = entity.BirthDay.HasValue ? DateUtil.UnixTimeToDateTime(entity.BirthDay.Value) : (DateTime?)null;
                model.dtm_Join_Date = entity.JoinDate.HasValue ? DateUtil.UnixTimeToDateTime(entity.JoinDate.Value) : (DateTime?)null;
                model.Join_Party_Date = entity.JoinPartyDate.HasValue ? DateUtil.UnixTimeToDateTime(entity.JoinPartyDate.Value) : (DateTime?)null;
                model.FirstName = entity.LastName;
                model.LastName = entity.FirstName;
                model.str_Email = entity.Email;
                _repoEmployee.Update(model);
                Uow.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public APIStatusUpdateAPI UpdateApiEmployee(APIChangePasswordFromLMS entity)
        {
            try
            {
                if (entity == null) return new APIStatusUpdateAPI() { Status = 0, Error = "Data is empty!!!" };

                var model =
                    _repoEmployee.Get(a => a.str_Staff_Id.ToLower().Trim() == entity.TraineeCode.ToLower().Trim());
                if (model != null)
                {
                    var pass = " ";
                    if (!string.IsNullOrEmpty(entity.CurrentPassword))
                    {
                        pass = Common.EncryptString(entity.CurrentPassword.Trim());
                    }
                    if (string.IsNullOrEmpty(model.Password) || model.Password.Equals(pass))
                    {
                        if (!string.IsNullOrEmpty(entity.NewPassword))
                        {
                            var checkPassword = IsPasswordAllowed(entity.NewPassword ?? string.Empty);
                            if (!checkPassword)
                            {
                                return new APIStatusUpdateAPI() { Status = 0, Error = Messege.RegEx_PASSWORD };
                            }
                            model.Password = Common.EncryptString(entity.NewPassword.Trim());
                            _repoEmployee.Update(model);
                            Uow.SaveChanges();
                            return new APIStatusUpdateAPI() { Status = 1, Error = "Save Change successfully" };
                        }

                        return new APIStatusUpdateAPI() { Status = 0, Error = "Data is empty!!!" };
                    }
                    return new APIStatusUpdateAPI() { Status = 0, Error = "Current Password is not correct !!!" };
                }

                return new APIStatusUpdateAPI() { Status = 0, Error = "Data is empty!!!" };
            }
            catch (Exception ex)
            {

                return new APIStatusUpdateAPI() { Status = 0, Error = "Data is empty!!!" };
            }
        }
        protected bool IsPasswordAllowed(string password)
        {

            return Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$");
        }
        #endregion


        public Trainee_Upload_Files FileGetById(int? id = null)
        {
            var entity = _repoTraineeUpload.Get(id);
            return entity;
        }
        public void Update(Trainee_Upload_Files entity)
        {
            _repoTraineeUpload.Update(entity);
            Uow.SaveChanges();
        }

        public Trainee Modify(EmployeeModelModify model, FormCollection form, string password = null)
        {
            var CheckRole = string.IsNullOrEmpty(form["CheckRole"]) ? 2 : int.Parse(form["CheckRole"]);
            IFormatProvider cultures = new System.Globalization.CultureInfo("vi-VN", true);
            var assignFunc_ids = form.GetValues("InstructorAbility");
            if (!string.IsNullOrEmpty(model.dtm_Birthdate))
            {
                model.Birthdate = DateTime.Parse(model.dtm_Birthdate, cultures, System.Globalization.DateTimeStyles.AssumeLocal);
            }
            if (!string.IsNullOrEmpty(model.dtm_Join_Date))
            {
                model.JoinedDate = DateTime.Parse(model.dtm_Join_Date, cultures, System.Globalization.DateTimeStyles.AssumeLocal);
            }
            Trainee entity;
            Instructor_Ability_LOG log;
            var flag = false;
            if (!model.Id.HasValue)
            {

                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Eid);
                if (model.Eid.Contains(" "))
                {
                    throw new Exception(codeHasSpaceMessage);
                }
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == (model.Eid.ToLower().Trim()) /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.Eid));
                }
                if (!string.IsNullOrEmpty(model.Email) && _repoEmployee.GetAll(a => a.IsDeleted == false && a.IsActive == true && a.str_Email.Equals(model.Email)).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.Email));
                }
                var randomPass = Common.RandomCharecter();
                entity = new Trainee
                {
                    str_Staff_Id = model.Eid,
                    dtm_Created_At = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    str_Created_By = CurrentUser.USER_ID.ToString(),
                    int_Role = model.Role == (int)UtilConstants.ROLE.Instructor
                ? (int)UtilConstants.ROLE.Instructor
                : (int)UtilConstants.ROLE.Trainee,
                    Password = !string.IsNullOrEmpty(password) ? Common.EncryptString(password) : Common.EncryptString(randomPass),
                    str_Email = !string.IsNullOrEmpty(model.Email) ? model.Email : string.Empty,
                    IsExaminer = model.IsExaminer == true ? true : false,
                    Passport = !string.IsNullOrEmpty(model.Passport) ? model.Passport : string.Empty,
                    dtm_Birthdate = model.Birthdate,
                    dtm_Join_Date = model.JoinedDate
                };
                _repoEmployee.Insert(entity);
            }
            else
            {
                var datacheckEID = _repoEmployee.GetAll(a => a.str_Staff_Id.ToLower().Trim() == model.Eid.ToLower().Trim() && a.Id != model.Id /*&& !a.IsDeleted && a.IsActive == true*/);
                if (datacheckEID.Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.Eid));
                }
                if (!string.IsNullOrEmpty(model.Email) && _repoEmployee.GetAll(a => /*&& a.IsActive == true &&*/ a.str_Email.Equals(model.Email) && a.Id != model.Id).Any())
                {
                    throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.Email));
                }
                entity = _repoEmployee.Get(model.Id);
                entity.dtm_Last_Modified_At = DateTime.Now;
                entity.str_Last_Modified_By = CurrentUser.USER_ID.ToString();
                entity.str_Email = !string.IsNullOrEmpty(model.Email) ? model.Email : string.Empty;
                if (CheckRole == (int)UtilConstants.ROLE.Trainee)
                {
                    entity.IsExaminer = model.IsExaminer == true ? true : false;
                    entity.int_Role = model.Role == (int)UtilConstants.ROLE.Instructor
                    ? (int)UtilConstants.ROLE.Instructor
                    : (int)UtilConstants.ROLE.Trainee;
                }
                //entity.Password = string.IsNullOrEmpty(password) ? entity.Password : Common.EncryptString(password);
                entity.Passport = !string.IsNullOrEmpty(model.Passport) ? model.Passport : string.Empty;
                entity.dtm_Join_Date = model.JoinedDate;
                entity.dtm_Birthdate = model.Birthdate;
                _repoEmployee.Update(entity);
                flag = true;
            }

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firtName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastName = model.FullName.Split(' ')[fullnamelength - 1];
            //if (firtName == "" || firtName == " " || lastName == "" || lastName == " ") throw new Exception( Messege.VALIDATION_FULLNAME );

            var culture = "en";
            //var cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            //if (cultureCookie != null)
            //{
            //    //culture = GetByKey("DisplayLanguage");
            //    culture = cultureCookie.Value;
            //}
            var firstName = " ";
            if (model.FullName.Contains(" "))
            {
                firstName = culture == "vi"
               ? model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim()
               : model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            }
            var lastName = model.FullName.Replace(firstName, "").Trim();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }

            entity.bit_Internal = model.EmployeeType == (int)UtilConstants.CourseAreas.Internal;
            entity.avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.avatar : model.ImgAvatar;
            entity.non_working_day = model.Role == (int)UtilConstants.ROLE.Trainee ? model.ResignationDate : null;

            //entity.str_Staff_Id = model.Eid;
            entity.Passport = model.Passport;
            entity.PersonalId = model.PersonalId;
            entity.FirstName = lastName;
            entity.LastName = firstName;
            entity.dtm_Birthdate = model.Birthdate;
            entity.Gender = model.Gender ?? 3;
            entity.str_Place_Of_Birth = model.PlaceOfBirth;
            entity.Nation = model.Nation;
            entity.str_Cell_Phone = model.Phone;
            entity.Job_Title_id = model.JobTitleId;
            entity.Department_Id = model.DepartmentId;
            entity.Company_Id = model.CompanyId;
            entity.dtm_Join_Date = model.JoinedDate;
            entity.Allowance = Convert.ToDouble(model.TrainingAllowance);
            entity.Join_Party_Date = model.JoinedPartyDate;
            entity.str_Email = model.Email;
            //update lmsStatus
            entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;


            #region[Insert hannah mentor]

            var checkHannahMentor = configService.GetByKey("MENTOR");
            if (checkHannahMentor.Equals("1"))
            {
                if (entity.Trainee_Type.Any())
                {
                    _reoiTraineeType.Delete(entity.Trainee_Type);
                }
                entity.Trainee_Type.Clear();
                if (model.InstructorType != null)
                {
                    foreach (var type in model.InstructorType)
                    {
                        if (type != 0)
                        {
                            entity.Trainee_Type.Add(new Trainee_Type()
                            {
                                Type = type
                            });
                        }
                    }
                }
            }

            #endregion
            #region[Insert edu]

            if (model.Educations != null)
            {
                UpdateEmployeeTraineeRecord(model.Educations, ref entity);
            }
            #endregion
            #region[Insert Contract]
            if (CheckRole == (int)UtilConstants.ROLE.Instructor)
            {
                if (entity.Trainee_Contract.Any())
                {
                    _repoTraineeContract.Delete(entity.Trainee_Contract);
                }
                entity.Trainee_Contract.Clear();
                if (model.Contracts != null)
                {
                    foreach (var contract in model.Contracts)
                    {
                        entity.Trainee_Contract.Add(new Trainee_Contract
                        {
                            dtm_Created_At = DateTime.Now,
                            expire_date = contract.ExpireDate,
                            contractno = contract.ContractNo,
                            description = contract.Description,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                }
            }
            #endregion
            #region[Insert Instructor_Ability]
            //if(assignFunc_ids != null)
            //{
            //    if (model.InstructorSubjects != null)
            //    {
            //        _repoTraineeAbility.Delete(entity.Instructor_Ability);
            //        foreach (var ability in model.InstructorSubjects)
            //        {
            //            if(assignFunc_ids.Contains(ability.SubjectId.ToString()))
            //            {
            //                entity.Instructor_Ability.Add(new Instructor_Ability { Allowance = ability.Allowance, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
            //            }

            //        }
            //    }
            //    else
            //    {
            //        _repoTraineeAbility.Delete(entity.Instructor_Ability);
            //    }
            //}
            if (CheckRole == (int)UtilConstants.ROLE.Instructor)
            {
                if (model.InstructorSubjects != null)
                {
                    _repoTraineeAbility.Delete(entity.Instructor_Ability);
                    foreach (var ability in model.InstructorSubjects)
                    {
                        //if (assignFunc_ids.Contains(ability.SubjectId.ToString()))
                        //{
                        //    entity.Instructor_Ability.Add(new Instructor_Ability { Allowance = ability.Allowance, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
                        //}
                        entity.Instructor_Ability.Add(new Instructor_Ability { Allowance = ability.Allowance, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
                    }
                }
                else
                {
                    _repoTraineeAbility.Delete(entity.Instructor_Ability);
                }
            }
            #endregion

            #region [-----insert Relevant Department----]
            if (CheckRole == (int)UtilConstants.ROLE.Instructor)
            {
                _repoTraineeCenter.Delete(entity.Trainee_TrainingCenter);
                if (model.RelevantDepartmentId != null)
                {

                    foreach (var ability in model.RelevantDepartmentId)
                    {
                        entity.Trainee_TrainingCenter.Add(new Trainee_TrainingCenter
                        {
                            Trainee = entity,
                            khoidaotao_id = ability
                        });
                    }
                }
            }
            #endregion
            #region [Check and insert Job History]
            var create = false;
            var dataCheckJobId = _repoTraineeHistory.GetAll(a => a.Trainee_Id == model.Id).OrderByDescending(a => a.Id).FirstOrDefault();
            if (dataCheckJobId == null)
            {
                create = true;
            }
            else
            {
                if (dataCheckJobId.Job_Title_Id != model.JobTitleId)
                {
                    create = true;
                }
            }

            if (create)
            {
                var entityHistory = new TraineeHistory()
                {
                    Job_Title_Id = model.JobTitleId,
                    Trainee_Id = model.Id ?? entity.Id,
                    dtm_Create_At = DateTime.Now,
                };
                _repoTraineeHistory.Insert(entityHistory);

            }

            if (create)
            {
                UpdateHistoryItem();
            }

            #endregion

            #region [Insert Log Instructor]

            if (CheckRole == (int)UtilConstants.ROLE.Instructor)
            {
                if (model.InstructorSubjects != null)
                {
                    var Listlog = _repoTraineeAbility_Log.GetAll(a => a.InstructorId == entity.Id && a.IsDeleted == false);
                    if (Listlog.Any())
                    {
                        foreach (var item in Listlog)
                        {
                            item.ModifyBy = CurrentUser.USER_ID;
                            item.ModifyDate = DateTime.Now;
                            item.IsDeleted = true;
                            _repoTraineeAbility_Log.Update(item);
                        }
                    }
                    foreach (var ability in model.InstructorSubjects)
                    {
                        log = new Instructor_Ability_LOG()
                        {
                            InstructorId = entity.Id,
                            SubjectDetailId = ability.SubjectId,
                            Allowance = ability.Allowance ?? 0,
                            CreateDate = DateTime.Now,
                            CreateBy = CurrentUser.USER_ID,
                            IsDeleted = false
                        };
                        _repoTraineeAbility_Log.Insert(log);
                    }
                }
                else
                {
                    var Listlog = _repoTraineeAbility_Log.GetAll(a => a.InstructorId == entity.Id && a.IsDeleted == false);
                    if (Listlog.Any())
                    {
                        foreach (var item in Listlog)
                        {
                            item.IsDeleted = true;
                            item.ModifyBy = CurrentUser.USER_ID;
                            item.ModifyDate = DateTime.Now;
                            _repoTraineeAbility_Log.Update(item);
                        }
                    }
                }
            }
            #endregion
            //password = string.IsNullOrEmpty(entity.Password) ? password : Common.DecryptString(entity.Password);
            //if (flag)
            //{
            //    if (Authenticate_CAS_Tho(model.Eid, password))
            //    {
            //        Uow.SaveChanges();
            //        return entity;
            //    }
            //}
            //else
            //{
            //    var depart = _repoDepartMent.GetAll(x => x.Id == entity.Department_Id).FirstOrDefault();
            //    var jobtitle = _repoJobTitle.GetAll(x => x.Id == entity.Job_Title_id).FirstOrDefault();
            //    //password = string.IsNullOrEmpty(entity.Password) ? password : Common.DecryptString(entity.Password);
            //    if (Register_CAS_Tho(model.Eid, password, firstName, "", lastName, model.Birthdate.ToString(), "", model.Email,
            //        "", model.PlaceOfBirth, model.Phone, "", depart.Code, depart.Name, jobtitle.Code, jobtitle.Name, entity.int_Role.GetValueOrDefault()))
            //    {
            //        Uow.SaveChanges();
            //        return entity;
            //    }
            //}


            Uow.SaveChanges();
            return entity;
        }
        public Trainee ModifyEmployee(int InstructorId, FormCollection form)
        {

            Instructor_Ability_LOG log;
            var entity = _repoEmployee.Get(InstructorId);
            var int_khoidaotao = !String.IsNullOrEmpty(form["int_khoidaotao"]) ? form["int_khoidaotao"].Split(',') : null;
            if (entity != null)
            {
                if (int_khoidaotao != null) // check <> null
                {
                    foreach (var item in int_khoidaotao)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var intkhoidatao_ = Convert.ToInt32(item);
                            var temp = _repoTraineeCenter.Get(a => a.instructor_id == InstructorId && a.khoidaotao_id == intkhoidatao_);
                            if (temp == null)
                            {
                                Trainee_TrainingCenter instructorsubject = new Trainee_TrainingCenter();
                                instructorsubject.instructor_id = InstructorId;
                                instructorsubject.khoidaotao_id = intkhoidatao_;
                                _repoTraineeCenter.Insert(instructorsubject);
                            }

                        }
                    }
                }
            }
            Uow.SaveChanges();
            return entity;
        }
        public Trainee ModifyExaminerAbility(EmployeeModelModify model)
        {
            Trainee entity;
            Examiner_Ability_LOG log;

            entity = _repoEmployee.Get(model.Id);
            #region[Insert Instructor_Ability]

            if (model.InstructorSubjects != null)
            {
                _repoExaminerAbility.Delete(entity.Examiner_Ability);
                foreach (var ability in model.InstructorSubjects)
                {
                    entity.Examiner_Ability.Add(new Examiner_Ability { Allowance = ability.Allowance, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
                }
            }
            else
            {
                _repoExaminerAbility.Delete(entity.Examiner_Ability);
            }
            #endregion

            #region [Insert Log Instructor]

            if (model.InstructorSubjects != null)
            {
                var Listlog = _repoExaminerAbilityLog.GetAll(a => a.ExaminerId == entity.Id && a.IsDeleted == false);
                if (Listlog.Any())
                {
                    foreach (var item in Listlog)
                    {
                        item.ModifyBy = CurrentUser.USER_ID;
                        item.ModifyDate = DateTime.Now;
                        item.IsDeleted = true;
                        _repoExaminerAbilityLog.Update(item);
                    }
                }
                foreach (var ability in model.InstructorSubjects)
                {
                    log = new Examiner_Ability_LOG()
                    {
                        ExaminerId = entity.Id,
                        SubjectDetailId = ability.SubjectId,
                        Allowance = ability.Allowance ?? 0,
                        CreateDate = DateTime.Now,
                        CreateBy = CurrentUser.USER_ID,
                        IsDeleted = false
                    };
                    _repoExaminerAbilityLog.Insert(log);
                }
            }
            else
            {
                var Listlog = _repoExaminerAbilityLog.GetAll(a => a.ExaminerId == entity.Id && a.IsDeleted == false);
                if (Listlog.Any())
                {
                    foreach (var item in Listlog)
                    {
                        item.IsDeleted = true;
                        item.ModifyBy = CurrentUser.USER_ID;
                        item.ModifyDate = DateTime.Now;
                        _repoExaminerAbilityLog.Update(item);
                    }
                }
            }
            #endregion


            Uow.SaveChanges();
            return entity;
        }
        public Trainee ModifyAllowance(int id, decimal allowance)
        {
            Trainee entity;
            Monitor_Ability_LOG log;

            entity = _repoEmployee.Get(id);
            #region[Insert Monitor_Ability]
            _repoMonitorAbility.Delete(entity.Monitor_Ability);
            entity.Monitor_Ability.Add(new Monitor_Ability { Allowance = allowance, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });

            #endregion

            #region [Insert Log Monitor]

            var Listlog = _repoMonitorAbilityLog.GetAll(a => a.MonitorID == entity.Id && a.IsDeleted == false);
            if (Listlog.Any())
            {
                foreach (var item in Listlog)
                {
                    item.ModifyBy = CurrentUser.USER_ID;
                    item.ModifyDate = DateTime.Now;
                    item.IsDeleted = true;
                    _repoMonitorAbilityLog.Update(item);
                }
            }

            log = new Monitor_Ability_LOG()
            {
                MonitorID = entity.Id,
                Allowance = allowance,
                CreateDate = DateTime.Now,
                CreateBy = CurrentUser.USER_ID,
                IsDeleted = false
            };
            _repoMonitorAbilityLog.Insert(log);
            #endregion


            Uow.SaveChanges();
            return entity;
        }

        public void AddtoLogFile(string filename, string msg)
        {
            string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Template"), filename);

            if (System.IO.File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(msg);
                }
            }
            else
            {
                StreamWriter writer = System.IO.File.CreateText(path);
                writer.WriteLine(msg);
                writer.Close();
            }
        }

        private void UpdateEmployeeTraineeRecord(IEnumerable<EmployeeModelModify.EmployeeEducation> educations, ref Trainee trainee)
        {
            var now = DateTime.Now;
            //if (trainee.Trainee_Record.Any())
            //{
            //    _repoTraineeRecord.Delete(trainee.Trainee_Record);
            //}
            //trainee.Trainee_Record.Clear();
            #region [------education-------]

            if (educations != null)
            {
                foreach (var traineeRecord in educations.Where(a => !string.IsNullOrEmpty(a.CourseName)))
                {
                    if (traineeRecord.IsDeleted == (int)UtilConstants.BoolEnum.No)
                    {
                        var listRecordFile = new List<Trainee_Upload_Files>();
                        if (traineeRecord.ListNameImage.Any())
                        {
                            listRecordFile.AddRange(traineeRecord.ListNameImage.Select(item => new Trainee_Upload_Files()
                            {
                                CreationDate = now,
                                Name = item,
                                IsDeleted = false
                            }));
                        }

                        if (traineeRecord.Id.HasValue && traineeRecord.Id != -1)
                        {
                            var modelRecord = trainee.Trainee_Record;
                            var model = modelRecord.FirstOrDefault(a => a.Trainee_Record_Id == traineeRecord.Id);
                            if (model != null)
                            {
                                model.str_Subject = traineeRecord.CourseName;
                                model.str_organization = traineeRecord.Organization;
                                model.str_note = traineeRecord.Note;
                                model.dtm_time_from = traineeRecord.TimeFrom;
                                model.dtm_time_to = traineeRecord.TimeTo;
                                model.dtm_Modified_At = now;
                                model.str_Modified_By = CurrentUser.USER_ID.ToString();
                                if (listRecordFile.Any())
                                {
                                    model.Trainee_Upload_Files = listRecordFile;
                                }
                                _repoTraineeRecord.Update(model);
                            }
                        }
                        else
                        {
                            trainee.Trainee_Record.Add(new Trainee_Record()
                            {
                                Trainee = trainee,
                                dtm_time_from = traineeRecord.TimeFrom,
                                dtm_time_to = traineeRecord.TimeTo,
                                str_Subject = traineeRecord.CourseName,
                                str_organization = traineeRecord.Organization,
                                str_note = traineeRecord.Note,
                                dtm_Created_At = now,
                                str_Created_By = CurrentUser.USER_ID.ToString(),
                                isActive = true,
                                bit_Deleted = false,
                                Trainee_Upload_Files = listRecordFile.Any() ? listRecordFile : null
                            });
                        }
                    }
                    else
                    {
                        var modelRecord = trainee.Trainee_Record;
                        var model = modelRecord.FirstOrDefault(a => a.Trainee_Record_Id == traineeRecord.Id);
                        if (model != null)
                        {
                            model.dtm_Modified_At = now;
                            model.bit_Deleted = true;
                            model.isActive = false;
                            model.dtm_Deleted_At = now;
                            model.str_Deleted_By = CurrentUser.USER_ID.ToString();
                            model.str_Modified_By = CurrentUser.USER_ID.ToString();
                            model.Trainee_Upload_Files.ToList().ForEach(a => a.IsDeleted = true);
                            _repoTraineeRecord.Update(model);
                        }
                    }
                }
            }
            #endregion
        }

        public List<sp_GetInstructorReport_TV_Result> GetInstructorReport(string subjectName, string subjectCode, string traineeName, string traineeCode, int departmentID, int jobtitleID, string sortDirection, string orderingFunction)
        {
            return Uow.sp_GetInstructorReport_TV_Result(subjectName, subjectCode, traineeName, traineeCode, departmentID, jobtitleID, sortDirection, orderingFunction).ToList();
        }

        public List<sp_GetListEmployee_TV_Result> GetEmployeeList(string filterCodeOrName)
        {
            return Uow.sp_GetEmployeeList_TV_Result(filterCodeOrName).ToList();
        }
    }
}
