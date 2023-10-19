using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using DAL.UnitOfWork;
using System.Data.Objects.SqlClient;
using System.Linq.Expressions;
using DAL.Entities;
using DAL.Repositories;
using TMS.Core.App_GlobalResources;
using TMS.Core.ViewModels.Cost;

namespace TMS.Core.Services.Cost
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.UserModels;

    public class CostService : BaseService, ICostService
    {
        private readonly IRepository<SubjectDetail> _repoSubject;
        private readonly IRepository<CAT_COSTS> _repoCAT_COSTS;
        private readonly IRepository<CAT_GROUPCOST> _repoCatGroupCost;
        private readonly IRepository<CAT_UNITS> _repoCAT_UNITS;
        private readonly IRepository<Course_Cost> _repoCourse_Cost;
        private readonly Expression<Func<CAT_COSTS, bool>> _catCostsDefaultFilter = a => a.IsDeleted != true && a.IsDeleted!= null;
        private readonly Expression<Func<CAT_GROUPCOST, bool>> _catGroupCostsDefaultFilter = a => a.IsDeleted == false;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public CostService(IUnitOfWork unitOfWork, IRepository<CAT_GROUPCOST> repoCatGroupCost, IRepository<CAT_COSTS> repoCAT_COSTS, IRepository<CAT_UNITS> repoCAT_UNITS, IRepository<Course_Cost> repoCourse_Cost, IRepository<Course> repoCourse, IRepository<SubjectDetail> repoSubject, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCAT_COSTS = repoCAT_COSTS;
            _repoCAT_UNITS = repoCAT_UNITS;
            _repoCourse_Cost = repoCourse_Cost;
            _repoCatGroupCost = repoCatGroupCost;
            _repoSubject = repoSubject;
        }
        public IQueryable<SubjectDetail> GetSubjectRp()
        {
            return _repoSubject.GetAll(a => a.IsDelete != true);
        }
        public CAT_COSTS GetById(int? id)
        {
            return id.HasValue ? _repoCAT_COSTS.Get(id) : null;
        }

       public CAT_GROUPCOST GetGroupcostById(int? id)
        {
            return id.HasValue ? _repoCatGroupCost.Get(id) : null;
        }
        public IQueryable<CAT_COSTS> Get()
        {
            return _repoCAT_COSTS.GetAll(_catCostsDefaultFilter);
        }

        public IQueryable<CAT_UNITS> GetUnits()
        {
            return _repoCAT_UNITS.GetAll();
        }

        public IQueryable<CAT_COSTS> Get(Expression<Func<CAT_COSTS, bool>> query)
        {
            var entities = _repoCAT_COSTS.GetAll(_catCostsDefaultFilter);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        public IQueryable<CAT_GROUPCOST> GetGroupCost()
        {
            return _repoCatGroupCost.GetAll(_catGroupCostsDefaultFilter);
        }
        public IQueryable<CAT_GROUPCOST> GetGroupCost(Expression<Func<CAT_GROUPCOST, bool>> query)
        {
            var entities = _repoCatGroupCost.GetAll(_catGroupCostsDefaultFilter);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public CAT_GROUPCOST Modify(PartialGroupCostViewModify model)
        {
            var now = DateTime.Now;
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            var duplicateMessage = string.Format(Messege.DataIsExists,Resource.lblCostCode,  model.Code );
            var costCode = _repoCAT_COSTS.Get(a => a.str_Code.ToLower().Trim() == model.Code.ToLower().Trim() && a.IsDeleted == false && a.id != model.Id);
            if (costCode != null)
            {
                throw new Exception( duplicateMessage );
            }

            var entity = _repoCatGroupCost.Get(model.Id);
            if (entity == null)
            {
                entity = new CAT_GROUPCOST
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreateBy = CurrentUser.USER_ID,
                    CreateDate = now
                };
                _repoCatGroupCost.Insert(entity);
            }
            else
            {
                entity.ModifiedBy = CurrentUser.USER_ID;
                entity.ModifiedDate = now;
                entity.Code = model.Code;
                entity.Name = model.Name;
                entity.Description = model.Description;
                _repoCatGroupCost.Update(entity);
            }
            Uow.SaveChanges();
            return entity;
        }
        public void Modify(CostModel model)
        {
            var now = DateTime.Now;
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            var duplicateMessage = string.Format(Messege.DataIsExists,Resource.lblCostCode,  model.Code );
            var costCode = _repoCAT_COSTS.Get(a => a.str_Code.ToLower().Trim() == model.Code.ToLower().Trim() && a.IsDeleted == false && a.id != model.Id);
            if (costCode != null)
            {
                throw new Exception( duplicateMessage );
            }
            var entity = _repoCAT_COSTS.Get(model.Id);
            if (entity == null)
            {

                entity = new CAT_COSTS();
                entity.str_Code = model.Code;
                entity.str_Name = model.Name;
                entity.str_Description = model.Description;
                entity.dtm_Dreated_date = now;
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.int_Created_by = CurrentUser.USER_ID;
                entity.GroupCostId = model.GroupCostId;

                _repoCAT_COSTS.Insert(entity);
            }
            else
            {
                if (!entity.str_Code.Equals("C001") || !entity.str_Code.Equals("C002"))
                {
                    entity.str_Code = model.Code;
                    entity.str_Name = model.Name;
                }
                entity.str_Description = model.Description;
                entity.dtm_Updated_date = now;
                entity.int_Updated_by = CurrentUser.USER_ID;
                entity.GroupCostId = model.GroupCostId;
                _repoCAT_COSTS.Update(entity);
            }

            Uow.SaveChanges();
        }
        public void Update(CAT_COSTS entity)
        {
            _repoCAT_COSTS.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(CAT_COSTS entity)
        {
            _repoCAT_COSTS.Insert(entity);
            Uow.SaveChanges();
        }

        public void Delete(CAT_COSTS entity)
        {
            _repoCAT_COSTS.Delete(entity);
            Uow.SaveChanges();
        }

        public void Insert(Course_Cost entity,int type)
        {
            _repoCourse_Cost.Insert(entity);
            Uow.SaveChanges();
            var _entity = _repoCourse_Cost.GetAll(a => a.id == entity.id).Select(a => new
            {
                a.id,
                coursename = a.Course.Name,
                subject = a.SubjectDetail.Name,
                costcode = a.CAT_COSTS.str_Code,
                costname = a.CAT_COSTS.str_Name,
                cost = a.cost,
                expectedcost = a.ExpectedCost,
                a.IsActive,
                a.IsDeleted
            });
            LogEvent(UtilConstants.LogType.EVENT_TYPE_INFORMATION, UtilConstants.LogSourse.CourseCost, type == 0 ? UtilConstants.LogEvent.Insert : UtilConstants.LogEvent.Update, _entity);
        }

        public void Update(CAT_GROUPCOST entity)
        {
            _repoCatGroupCost.Update(entity);
            Uow.SaveChanges();
        }
        public void Update(Course_Cost entity)
        {
            _repoCourse_Cost.Update(entity);
            Uow.SaveChanges();
            var _entity = _repoCourse_Cost.GetAll(a => a.id == entity.id).Select(a => new
            {
                a.id,
                coursename = a.Course.Name,
                subject = a.SubjectDetail.Name,
                costcode = a.CAT_COSTS.str_Code,
                costname = a.CAT_COSTS.str_Name,
                cost = a.cost,
                a.IsActive,
                a.IsDeleted
            });
            LogEvent(UtilConstants.LogType.EVENT_TYPE_INFORMATION, UtilConstants.LogSourse.CourseCost, UtilConstants.LogEvent.Update, _entity);
        }

        public Course_Cost GetCourseCostById(int? id)
        {
            return id.HasValue ? _repoCourse_Cost.Get(id) : null;
        }

        public IQueryable<Course_Cost> GetCourseCost()
        {
            return _repoCourse_Cost.GetAll();
        }

        public IQueryable<Course_Cost> GetCourseCost(Expression<Func<Course_Cost, bool>> query)
        {
            var entities = _repoCourse_Cost.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public List<sp_GetCostHeader_Result> sp_GetCostHeader(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd)
        {
            return Uow.sp_GetCostHeader_Result(listCourseID, fromDateStart, fromDateEnd, toDateStart, toDateOfEnd).ToList();
        }

        public List<sp_GetCostDetail_Result> sp_GetCostDetail(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd)
        {
            return Uow.sp_GetCostDetail_Result(listCourseID, fromDateStart, fromDateEnd, toDateStart, toDateOfEnd).ToList();
        }
    }
}
