using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.App_GlobalResources;

namespace TMS.Core.Services.Companies
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.Company;

    public class CompanyService : BaseService, ICompanyService
    {
        private readonly IRepository<Company> _repoCompany;
        public CompanyService(IUnitOfWork unitOfWork, IRepository<Company> repoCompany, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCompany = repoCompany;
        }

        public Company GetById(int? id)
        {
            return !id.HasValue ? null : _repoCompany.Get(id.Value);
        }

        public Company GetByName(string name)
        {
            return _repoCompany.Get(a => (string.IsNullOrEmpty(name.Trim().ToLower()) || a.str_Name.Contains(name.Trim().ToLower())));
        }

        public Company GetByCode(string code)
        {
            return _repoCompany.Get(a => (string.IsNullOrEmpty(code.Trim().ToLower()) || a.str_code.Contains(code.Trim().ToLower())));
        }

        public IQueryable<Company> Get(Expression<Func<Company, bool>> query = null)
        {
            var entities = _repoCompany.GetAll(a => a.bit_Deleted==false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(m => m.str_Name);
        }

        public void Insert(Company entity)
        {
            _repoCompany.Insert(entity);
            Uow.SaveChanges();
        }

        public void Update(Company entity)
        {
            _repoCompany.Update(entity);
            Uow.SaveChanges();
        }

        public Company GetCompanyById(int? id)
        {
            return _repoCompany.Get(id);
        }

        public IQueryable<Company> GetCompany(Expression<Func<Company, bool>> query = null)
        {
            var entities = _repoCompany.GetAll(a => a.bit_Deleted==false && a.bit_Deleted==false);
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
        public void Update(CompanyModels model, string currentUser)
        {
            var entity = _repoCompany.Get(model.Id);
            if (entity == null)
            {
                throw new Exception(string.Format(Messege.ERROR_NOTFOUND,Resource.lblCompany));
            }
            entity.str_code = model.Code;
            entity.str_Name = model.Name;
            entity.dicsription = model.Description;
            entity.dtm_Modified_At = DateTime.Now;
            entity.str_Modified_By = currentUser;

            _repoCompany.Update(entity);
            Uow.SaveChanges();

        }

        public void InsertCompany(CompanyModels model, string currentUser)
        {
            var entity = new Company
            {
                str_code = model.Code,
                str_Name = model.Name,
                dicsription = model.Description,
                dtm_Created_At = DateTime.Now,
                str_Created_By = currentUser,
                bit_Deleted = false
            };

            _repoCompany.Insert(entity);
            Uow.SaveChanges();
        }
    }
}
