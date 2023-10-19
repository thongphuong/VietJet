using System;
using System.Collections.Generic;
using System.Linq;

namespace TMS.Core.Services.Jobtitle
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.Jobtitles;
    using TMS.Core.ViewModels.UserModels;

    public class JobtitleService : BaseService, IJobtitleService
    {
        private readonly IRepository<JobTitle> _repoJobtitle;
        private readonly IRepository<Title_Standard> _repotitleStandard;
        private readonly IRepository<JobtitleHeader> _repoJobHeader;
        private readonly IRepository<JobtitleLevel> _repoJobLevel;
        private readonly IRepository<JobtitlePosition> _repoJobPosition;
        private readonly Expression<Func<JobTitle, bool>> _defaultJobTitleFilter = a => a.IsDelete != true;
        private readonly Expression<Func<JobtitleHeader, bool>> _defaultJobTitleHeader = a => a.IsDelete != true;
        private readonly Expression<Func<JobtitlePosition, bool>> _defaultPossitionFilter = a => a.IsDelete != true;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public JobtitleService(IUnitOfWork unitOfWork, IRepository<JobTitle> repoJobtitle, IRepository<Title_Standard> repotitleStandard, IRepository<JobtitleHeader> repoJobHeader, IRepository<JobtitleLevel> repoJobLevel, IRepository<JobtitlePosition> repoJobPosition, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoJobtitle = repoJobtitle;
            _repotitleStandard = repotitleStandard;
            _repoJobHeader = repoJobHeader;
            _repoJobLevel = repoJobLevel;
            _repoJobPosition = repoJobPosition;
        }

        public JobTitle GetById(int id)
        {
            var entity = _repoJobtitle.Get(id);
            return entity == null || entity.IsDelete==true ? null : entity;
        }
        public JobtitleHeader GetByIdHeader(int id)
        {
            var entity = _repoJobHeader.Get(id);
            return entity == null || entity.IsDelete==true ? null : entity;
        }
        public JobtitlePosition GetByIdPosition(int id)
        {
            var entity = _repoJobPosition.Get(id);
            return entity == null || entity.IsDelete==true ? null : entity;
        }

        public IQueryable<JobTitle> Get(bool isView = false)
        {
            return isView ? _repoJobtitle.GetAll(_defaultJobTitleFilter) : _repoJobtitle.GetAll(_defaultJobTitleFilter).Where(a => a.IsActive==true);
        }

        public IQueryable<JobtitleLevel> GetJobLevel(bool? isView = false)
        {
            return isView == true
                ? _repoJobLevel.GetAll(a => a.IsDelete==false)
                : _repoJobLevel.GetAll(a => a.IsDelete==false && a.IsActive==true);
        }
        public IQueryable<JobtitleHeader> GetJobHeader(bool? isView = false)
        {
            return isView == true
                ? _repoJobHeader.GetAll(a => a.IsDelete==false)
                : _repoJobHeader.GetAll(a => a.IsDelete==false && a.IsActive==true);
        }

        public IQueryable<JobtitlePosition> GetJobPosition(bool? isView = false)
        {
            return isView == true
                ? _repoJobPosition.GetAll(a => a.IsDelete==false)
                : _repoJobPosition.GetAll(a => a.IsDelete==false && a.IsActive==true);
        }

        public IQueryable<JobTitle> Get(Expression<Func<JobTitle, bool>> query, bool isView = false, bool isApi = false)
        {

            var entities = _repoJobtitle.GetAll(isApi ? null : _defaultJobTitleFilter);
            if (query != null) entities = entities.Where(query);
            return isView ? entities.OrderBy(m => m.Name) : entities.Where(a => a.IsActive==true).OrderBy(m => m.Name);
        }
        public IQueryable<JobtitleHeader> GetJobHeader(Expression<Func<JobtitleHeader, bool>> query, bool isView = false)
        {
            var entities = _repoJobHeader.GetAll(_defaultJobTitleHeader);
            if (query != null) entities = entities.Where(query);
            return isView ? entities.OrderBy(m => m.Name) : entities.Where(a => a.IsActive==true).OrderBy(m => m.Name);
        }
        public IQueryable<JobtitlePosition> GetJobPosition(Expression<Func<JobtitlePosition, bool>> query, bool isView = false)
        {
            var entities = _repoJobPosition.GetAll(_defaultPossitionFilter);
            if (query != null) entities = entities.Where(query);
            return isView ? entities.OrderBy(m => m.Name) : entities.Where(a => a.IsActive==true).OrderBy(m => m.Name);
        }
        public void Update(JobTitle entity)
        {
            _repoJobtitle.Update(entity);
            Uow.SaveChanges();
        }
        public void Update(JobtitleHeader entity)
        {
            _repoJobHeader.Update(entity);
            Uow.SaveChanges();
        }
        public void Update(JobtitlePosition entity)
        {
            _repoJobPosition.Update(entity);
            Uow.SaveChanges();
        }
        public void Insert(JobTitle entity)
        {
            _repoJobtitle.Insert(entity);
            Uow.SaveChanges();
        }
        public void Insert(JobtitleHeader entity)
        {
            _repoJobHeader.Insert(entity);
            Uow.SaveChanges();
        }
        public void Insert(JobtitlePosition entity)
        {
            _repoJobPosition.Insert(entity);
            Uow.SaveChanges();
        }
        public Title_Standard GetTitleStandardById(int id)
        {
            return _repotitleStandard.Get(id);
        }

        public IQueryable<Title_Standard> GetTitleStandard()
        {
            return _repotitleStandard.GetAll();
        }

        public IQueryable<Title_Standard> GetTitleStandard(Expression<Func<Title_Standard, bool>> query)
        {
            return _repotitleStandard.GetAll(query);
        }

        public void UpdateTitleStandard(Title_Standard entity)
        {
            _repotitleStandard.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertTitleStandard(Title_Standard entity)
        {
            _repotitleStandard.Insert(entity);
            Uow.SaveChanges();
        }

        public void DeleteTitleStandard(IEnumerable<Title_Standard> entities)
        {
            _repotitleStandard.Delete(entities);
            Uow.SaveChanges();
        }

        public int ModifyJobHeader(JobtitleHeaderViewModel model)
        {
            var entity = _repoJobHeader.Get(model.Id);
            var now = DateTime.Now;
            if (!model.Id.HasValue)
            {

                entity = new JobtitleHeader()
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = true,
                    IsDelete = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUser.USER_ID.ToString(),

                };

                _repoJobHeader.Insert(entity);

            }
            else
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.UpdatedBy = CurrentUser.USER_ID.ToString();
                entity.UpdatedDate = now;
                // Cập nhật Status để gửi LMS
               // entity.JobTitles.ToList().ForEach(a => a.LmsStatus = (int)UtilConstants.ApiStatus.Modify);
                _repoJobHeader.Update(entity);
            }


            Uow.SaveChanges();
            return entity.Id;
        }
        public int ModifyJobPosition(JobtitlePositionViewModel model)
        {
            var entity = _repoJobPosition.Get(model.Id);
            var now = DateTime.Now;
            if (entity == null)
            {
                entity = new JobtitlePosition()
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = true,
                    IsDelete = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUser.USER_ID.ToString(),

                };
                _repoJobPosition.Insert(entity);

            }
            else
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.UpdatedBy = CurrentUser.USER_ID.ToString();
                entity.UpdatedDate = now;
                // Cập nhật Status để gửi LMS
                //entity.JobTitles.ToList().ForEach(a => a.LmsStatus = (int)UtilConstants.ApiStatus.Modify);
                _repoJobPosition.Update(entity);
            }


            Uow.SaveChanges();
            return entity.Id;
        }
        public void Modify(JobtitleModifyViewModel model)
        {
            var entity = _repoJobtitle.Get(model.Id);
            if (entity == null)
            {
                entity = new JobTitle();
                entity.IsDelete = false;
                if (model.check_hidden_level_position == false)
                {
                    //if (model.JobHeaderId == null)
                    //{
                    //    throw new Exception("Level is null ");
                    //}
                    //if (model.JobPositionId == null)
                    //{
                    //    throw new Exception("Position is null ");
                    //}
                    //if (model.JobHeaderId == null && model.JobPositionId == null)
                    //{
                    //    throw new Exception("Level and Position is null ");
                    //}        
                    //entity.JobPositionId = model.JobPositionId;
                }
                entity.JobHeaderId = 1;
                entity.Code = Guid.NewGuid().ToString();//model.Code,
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = CurrentUser.USER_ID.ToString();
                entity.IsActive = model.IsActive == (int)UtilConstants.BoolEnum.Yes;
                entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                entity.Title_Standard = model.AssignedSubjects?.Select(a => new Title_Standard()
                {
                    Subject_Id = a,
                }).ToList();
                _repoJobtitle.Insert(entity);



                //{
                //    IsDelete = false,

                //    JobHeaderId = model.JobHeaderId,
                //    JobPositionId = model.JobPositionId,
                //    Code = Guid.NewGuid().ToString(),//model.Code,
                //    Name = model.Name,
                //    Description = model.Description,
                //    CreatedDate = DateTime.Now,
                //    CreatedBy = CurrentUser.USER_ID.ToString(),
                //    IsActive = model.IsActive == (int)UtilConstants.BoolEnum.Yes,
                //    LmsStatus = (int)UtilConstants.ApiStatus.Modify,
                //    Title_Standard = model.AssignedSubjects?.Select(a => new Title_Standard()
                //    {
                //        Subject_Id = a,
                //    }).ToList()
                //};
                //_repoJobtitle.Insert(entity);
            }
            else
            {
                entity.IsActive = model.IsActive == (int)UtilConstants.BoolEnum.Yes;
                //entity.IsDelete = false;
                //entity.Code = model.Code;
                if (model.check_hidden_level_position == false)
                {
                    //if (model.JobHeaderId == null)
                    //{
                    //    throw new Exception("Level is null ");
                    //}
                    //if (model.JobPositionId == null)
                    //{
                    //    throw new Exception("Position is null ");
                    //}
                    //if (model.JobHeaderId == null && model.JobPositionId == null)
                    //{
                    //    throw new Exception("Level and Position is null ");
                    //}
                    entity.JobHeaderId = 1;
                    //entity.JobPositionId = model.JobPositionId;
                }
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.UpdatedDate = DateTime.Now;
                entity.UpdatedBy = CurrentUser.USER_ID.ToString();
                entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                var subjects = entity.Title_Standard.ToList();
                foreach (var subject in subjects)
                {
                    _repotitleStandard.Delete(subject);
                }
                entity.Title_Standard.Clear();
                if (model.AssignedSubjects != null)
                {
                    entity.Title_Standard = model.AssignedSubjects.Select(a => new Title_Standard()
                    {
                        JobTitle = entity,
                        Subject_Id = a,
                    }).ToList();
                }

                _repoJobtitle.Update(entity);
            }
            Uow.SaveChanges();
        }
    }
}
