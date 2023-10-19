using System;
using System.Collections.Generic;
using System.Linq;
using DAL.UnitOfWork;
using System.Data.Objects.SqlClient;
using System.Linq.Expressions;
using DAL.Entities;
using DAL.Repositories;

namespace TMS.Core.Services.Orientation
{
    using Utils;
    using ViewModels.Courses;
    using ViewModels.Room;
    using ViewModels.UserModels;
    using DAL.Entities;
    using TMS.Core.ViewModels.Orientation;

    public class OrientationService : BaseService, IOrientationService
    {
        private readonly IRepository<Orientation> _repoOrientation;
        private readonly IRepository<PotentialSuccessor> _repoPotentialSuccessor;
        private readonly IRepository<PotentialSuccessors_Item> _repoPotentialSuccessorItem;
        private readonly IRepository<Orientation_Item> _repoOrientationItem;
        private readonly IRepository<Orientation_Kind_Of_Successor> _repoOrientationKindOfSuccessor;
        private readonly IRepository<Title_Standard> _repoTitle_Standard;
        private readonly IRepository<Course_Result_Final> _repoCourse_Result_Final;
        private readonly IRepository<Course_Detail> _repoCourse_Detail;
        private readonly IRepository<TraineeFuture> _repoTraineeFuture;
        private readonly IRepository<JobTitle> _repoJobTitle;
        private UserModel _currentUser = null;
        private const int statusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public OrientationService(IUnitOfWork unitOfWork, IRepository<TraineeFuture> repoTraineeFuture, IRepository<Orientation_Item> repoOrientationItem, IRepository<Title_Standard> repoTitle_Standard, IRepository<Course_Result_Final> repoCourse_Result_Final, IRepository<Course_Detail> repoCourse_Detail, IRepository<Orientation> repoOrientation, IRepository<Orientation_Kind_Of_Successor> repoOrientationKindOfSuccessor, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<PotentialSuccessor> repoPotentialSuccessor, IRepository<JobTitle> repoJobTitle, IRepository<PotentialSuccessors_Item> repoPotentialSuccessorItem) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoOrientation = repoOrientation;
            _repoOrientationKindOfSuccessor = repoOrientationKindOfSuccessor;
            _repoOrientationItem = repoOrientationItem;
            _repoTitle_Standard = repoTitle_Standard;
            _repoCourse_Result_Final = repoCourse_Result_Final;
            _repoCourse_Detail = repoCourse_Detail;
            _repoTraineeFuture = repoTraineeFuture;
            _repoPotentialSuccessor = repoPotentialSuccessor;
            _repoJobTitle = repoJobTitle;
            _repoPotentialSuccessorItem = repoPotentialSuccessorItem;
        }

        public IQueryable<Orientation_Kind_Of_Successor> GetKind()
        {
            return _repoOrientationKindOfSuccessor.GetAll().OrderByDescending(c => c.Name);
        }
        public IQueryable<Orientation_Kind_Of_Successor> GetKind(Expression<Func<Orientation_Kind_Of_Successor, bool>> query = null)
        {
            var entities = _repoOrientationKindOfSuccessor.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public IQueryable<Orientation> Get()
        {
            return _repoOrientation.GetAll().OrderByDescending(c => c.Id);
        }
        public IQueryable<Orientation> Get(Expression<Func<Orientation, bool>> query)
        {
            var entities = _repoOrientation.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<PotentialSuccessor> Get(Expression<Func<PotentialSuccessor, bool>> query)
        {
            var entities = _repoPotentialSuccessor.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<Orientation_Item> GetItem()
        {
            return _repoOrientationItem.GetAll().OrderByDescending(c => c.Id);
        }
        public IQueryable<Orientation_Item> GetItem(Expression<Func<Orientation_Item, bool>> query)
        {
            var entities = _repoOrientationItem.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public int ModifyOrientation(OrientationViewModel model, int? idtrainee, int? idjobtitle, DateTime? expectDate, int? idjob)
        {
            var entity = _repoOrientation.Get(a => a.TraineeId == idtrainee && a.JobHistoryId == idjobtitle && a.JobFutureId == idjob && a.ExpectedDate == expectDate);

            var now = DateTime.Now;
            if (entity == null)
            {
                entity = new Orientation()
                {
                    JobHistoryId = idjobtitle,
                    JobFutureId = idjob,
                    TraineeId = idtrainee,
                    CreateDay = now,
                    CreateBy = CurrentUser.USER_ID,
                    ExpectedDate = expectDate
                };
                _repoOrientation.Insert(entity);
            }
            else
            {
                _repoOrientation.Update(entity);
            }
            entity.IdKindOfSuccessor = model.IdKindOfSuccessor;
            entity.Remark = model.Remark;
            Uow.SaveChanges();

            // xóa hết item
            var OriItem = entity.Orientation_Item.ToList();
            foreach (var item in OriItem)
            {
                _repoOrientationItem.Delete(item);
            }
            entity.Orientation_Item.Clear();

            //// insert lại item
            var getCourseid = _repoCourse_Result_Final.GetAll(a => a.traineeid == idtrainee && a.Course.IsDeleted != true && a.Course.IsActive == true)?.Select(a => a.courseid);
            var subject = _repoCourse_Detail.GetAll(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct();
            var data_ = _repoTitle_Standard.GetAll(a => a.Job_Title_Id == idjob && !subject.Contains(a.Subject_Id));
            if (data_.Any())
            {
                foreach (var item in data_)
                {
                    var entityOri = new Orientation_Item()
                    {
                        OrientationId = entity.Id,
                        SubjectId = item.Subject_Id
                    };
                    _repoOrientationItem.Insert(entityOri);
                }
            }
            Uow.SaveChanges();

            ModifyTraineeFuture(idtrainee, idjob, UtilConstants.StatusApiApprove.Pending);


            return entity.Id;
        }
        public int ModifyOrientationPDP(OrientationPDPViewModel model,  int? idtrainee, int? idjobtitle, DateTime? expectDate, int? idjob)
        {
            var entity = _repoOrientation.Get(a => a.TraineeId == idtrainee && a.JobHistoryId == idjobtitle && a.JobFutureId == idjob && a.ExpectedDate == expectDate);

            var now = DateTime.Now;
            if (entity == null)
            {
                entity = new Orientation()
                {
                    JobHistoryId = idjobtitle,
                    JobFutureId = idjob,
                    TraineeId = idtrainee,
                    CreateDay = now,
                    CreateBy = CurrentUser.USER_ID,
                    ExpectedDate = expectDate
                };
                _repoOrientation.Insert(entity);
            }
            else
            {
                _repoOrientation.Update(entity);
            }
            entity.IdKindOfSuccessor = model.IdKindOfSuccessor;
            entity.Remark = model.Remark;
            Uow.SaveChanges();

            // xóa hết item
            var OriItem = entity.Orientation_Item.ToList();
            foreach (var item in OriItem)
            {
                _repoOrientationItem.Delete(item);
            }
            entity.Orientation_Item.Clear();

            //// insert lại item
            var getCourseid = _repoCourse_Result_Final.GetAll(a => a.traineeid == idtrainee && a.Course.IsDeleted != true && a.Course.IsActive == true)?.Select(a => a.courseid);
            var subject = _repoCourse_Detail.GetAll(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct();
            var data_ = _repoTitle_Standard.GetAll(a => a.Job_Title_Id == idjob && !subject.Contains(a.Subject_Id));
            if (data_.Any())
            {
                foreach (var item in data_)
                {
                    var entityOri = new Orientation_Item()
                    {
                        OrientationId = entity.Id,
                        SubjectId = item.Subject_Id
                    };
                    _repoOrientationItem.Insert(entityOri);
                }
            }
            Uow.SaveChanges();

            ModifyTraineeFuture(idtrainee, idjob, UtilConstants.StatusApiApprove.Pending);


            return entity.Id;
        }
        private void ModifyTraineeFuture(int? traineeId, int? jobTitleId, UtilConstants.StatusApiApprove type)
        {
            var model =
                _repoTraineeFuture.Get(
                    a => a.TraineeId == traineeId && a.JobTitleId == jobTitleId && a.Status == (int)type);
            if (model == null) return;
            model.Status = (int)UtilConstants.StatusApiApprove.Approved;
            //model.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
            model.Schedule = (int) UtilConstants.ApiStatus.Modify;
            _repoTraineeFuture.Update(model);
            Uow.SaveChanges();
        }

        public void Insert(Orientation entity)
        {
            _repoOrientation.Insert(entity);
            Uow.SaveChanges();
        }

        public void ModifySuccessor(List<OrientationModify> model, int? id, int? idjobfuture)
        {
            try
            {
                PotentialSuccessor entity;
                if(!id.HasValue || id == 0)
                {
                    var job = _repoJobTitle.Get(idjobfuture ?? 0);
                    entity = new PotentialSuccessor();
                    if(job != null)
                    {
                        entity.JobtitleId = idjobfuture;
                        entity.CreatedBy = CurrentUser.USER_ID;
                        entity.CreatedAt = DateTime.Now;
                        entity.Status = statusModify;
                        entity.IsActive = true;
                        entity.IsDeleted = false;
                        _repoPotentialSuccessor.Insert(entity);
                        foreach (var item in model)
                        {
                            entity.PotentialSuccessors_Item.Add( new PotentialSuccessors_Item
                                {
                                TraineeId = item.EmployeeID,
                                JobHistoryId = item.JobTitleID,
                                CreateBy = CurrentUser.USER_ID,
                                CreateDay = DateTime.Now,
                                IsActive = item.SelectedValue,
                                IsDelete = false,
                                Status = statusModify, //lưu ý
                            });
                        }
                    }
                }
                else
                {
                    entity = _repoPotentialSuccessor.Get(id);
                    var job = _repoJobTitle.Get(idjobfuture ?? 0);
                    if (entity != null)
                    {
                        entity.ModifyAt = DateTime.Now;
                        entity.ModifyBy = CurrentUser.USER_ID;
                        _repoPotentialSuccessor.Update(entity);
                       
                        if (job != null)
                        {
                            if (entity.PotentialSuccessors_Item.Count() == 0)
                            {
                                foreach (var item in model)
                                {
                                    entity.PotentialSuccessors_Item.Add(new PotentialSuccessors_Item
                                    {
                                        TraineeId = item.EmployeeID,
                                        JobHistoryId = item.JobTitleID,
                                        CreateBy = CurrentUser.USER_ID,
                                        CreateDay = DateTime.Now,
                                        IsActive = item.SelectedValue,
                                        IsDelete = false,
                                        Status = statusModify, //lưu ý
                                    });
                                }
                            }
                            else
                            {
                                foreach (var item in model)
                                {
                                    var check = entity.PotentialSuccessors_Item.FirstOrDefault(a => a.TraineeId == item.EmployeeID);
                                    if (check != null)
                                    {
                                        check.JobHistoryId = item.JobTitleID;
                                        check.IsActive = item.SelectedValue;
                                        check.ModifyAt = DateTime.Now;
                                        check.ModifyBy = CurrentUser.USER_ID;
                                        _repoPotentialSuccessorItem.Update(check);
                                    }
                                    else
                                    {
                                        entity.PotentialSuccessors_Item.Add(new PotentialSuccessors_Item
                                        {
                                            TraineeId = item.EmployeeID,
                                            JobHistoryId = item.JobTitleID,
                                            CreateBy = CurrentUser.USER_ID,
                                            CreateDay = DateTime.Now,
                                            IsActive = item.SelectedValue,
                                            IsDelete = false,
                                            Status = statusModify, //lưu ý
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                Uow.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ApproveySuccessor(List<OrientationModify> model, int? id, int typeApprove)
        {
            try
            {
                PotentialSuccessor entity;
                entity = _repoPotentialSuccessor.Get(id);
                {
                    entity.ModifyAt = DateTime.Now;
                    entity.ModifyBy = CurrentUser.USER_ID;
                    entity.Status = typeApprove;
                    _repoPotentialSuccessor.Update(entity);

                    if (entity.PotentialSuccessors_Item.Count() > 0)
                    {
                        foreach (var item in model)
                        {
                            var check = entity.PotentialSuccessors_Item.FirstOrDefault(a => a.TraineeId == item.EmployeeID);
                            if (check != null)
                            {
                                check.ModifyAt = DateTime.Now;
                                check.ModifyBy = CurrentUser.USER_ID;
                                check.Status = typeApprove;
                                _repoPotentialSuccessorItem.Update(check);
                            }
                        }
                    }
                }
                Uow.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public PotentialSuccessor GetbyId(int? id)
        {
            return id.HasValue ? _repoPotentialSuccessor.Get(id) : null;
        }
        public IQueryable<PotentialSuccessors_Item> GetItembyId(int? id)
        {
            return _repoPotentialSuccessorItem.GetAll(a => a.SuccessorsID == id);
        }

    }
}
