using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Nationality
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.Nation;
    using TMS.Core.ViewModels.UserModels;

    public class NationalityService : BaseService, INationalityService
    {
        private readonly IRepository<Nation> _repoNation;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public NationalityService(IUnitOfWork unitOfWork, IRepository<Nation> repoNation, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoNation = repoNation;
        }

        public Nation GetById(int? id)
        {
            return !id.HasValue ? null : _repoNation.Get(id.Value);
        }

        public Nation GetByName(string name)
        {
            return _repoNation.Get(a => (string.IsNullOrEmpty(name) || a.Nation_Name.Contains(name)));
        }

        public Nation GetByCode(string code)
        {
            return _repoNation.Get(a => (string.IsNullOrEmpty(code) || a.Nation_Code.Contains(code)));
        }

        public IQueryable<Nation> Get(Expression<Func<Nation, bool>> query = null)
        {
            var entities = _repoNation.GetAll(a => (bool)!a.isactive);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(m => m.Nation_Name);
        }

        public void Insert(Nation entity)
        {
            _repoNation.Insert(entity);
            Uow.SaveChanges();
        }

        public void Update(Nation entity)
        {
            _repoNation.Update(entity);
            Uow.SaveChanges();
        }

        public Nation GetCompanyById(int? id)
        {
            return _repoNation.Get(id);
        }

        public IQueryable<Nation> GetCompany(Expression<Func<Nation, bool>> query = null)
        {
            var entities = _repoNation.GetAll(a => (bool)!a.isactive && (bool)!a.isactive);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void UpdateCompany(Nation entity)
        {
            _repoNation.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertCompany(Nation entity)
        {
            _repoNation.Insert(entity);
            Uow.SaveChanges();
        }
        public void ModifyCompany(Nation entity)
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
        public void Update(NationModels model  )
        {
            var entity = _repoNation.Get(model.Id);
            if (entity == null)
            {
                throw new Exception( "data is not found" );
            }
            entity.Nation_Code = model.Code;
            entity.Nation_Name = model.Name;
            entity.description = model.Description;
            entity.createday = DateTime.Now;
            entity.createuser = CurrentUser.USER_ID;

            _repoNation.Update(entity);
            Uow.SaveChanges();

        }

        public void InsertCompany(NationModels model  )
        {
            var entity = new Nation
            {
                Nation_Code = model.Code,
                Nation_Name = model.Name,
                description = model.Description,
                createday = DateTime.Now,
                createuser = CurrentUser.USER_ID,
                isactive = false
            };

            _repoNation.Insert(entity);
            Uow.SaveChanges();
        }
    }
}
