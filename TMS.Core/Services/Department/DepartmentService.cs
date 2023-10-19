using System;
using System.Linq;
using TMS.Core.App_GlobalResources;

namespace TMS.Core.Services.Department
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.Departments;
    using TMS.Core.ViewModels.ViewModel;

    public class DepartmentService : BaseService, IDepartmentService
    {
        private readonly IRepository<Department> _repoDepartment;
        private readonly Expression<Func<Department, bool>> _departmentDefaultFilter = a => a.IsDeleted != true; 

        public DepartmentService(IUnitOfWork unitOfWork, IRepository<Department> repoDepartment, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoDepartment = repoDepartment;
        }

        public Department GetById(int? id = null)
        {
            return id.HasValue ? _repoDepartment.Get(id) : null;
        }

       

        public IQueryable<Department> Get()
        {
            return _repoDepartment.GetAll(_departmentDefaultFilter)/*.OrderBy(a => a.Ancestor)*/; 
        }
        public IQueryable<Department> ApiGet(Expression<Func<Department, bool>> query)
        {
            var entities = _repoDepartment.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities.OrderBy(a => a.Ancestor);
        }

        public IQueryable<Department> Get(Expression<Func<Department, bool>> query)
        {
            var entities = _repoDepartment.GetAll(_departmentDefaultFilter);
            if (query != null) entities = entities.Where(query);
            return entities.OrderBy(a=>a.Ancestor);
        }

        public IQueryable<Department> Get(Expression<Func<Department, bool>> query, IEnumerable<int> permissions)
        {
            if (permissions == null) return null;
            var entities = _repoDepartment.GetAll(a => a.IsDeleted==false && a.IsActive==true && permissions.Contains(a.Id));
            if (query != null) entities = entities.Where(query);
            return entities.OrderBy(a => a.Ancestor);
        }

        public void Import(Dictionary<string, DepartmentImportViewModel> importList)
        {
            var now = DateTime.Now;
            var list = importList.OrderBy(a => a.Value.ParentCode);
            var departments = new List<Department>();
            foreach (var dept in list)
            {
                var dateKey = dept.Key;
                var data = dept.Value;
                var department = _repoDepartment.Get(a => a.Code.Equals(dateKey) && a.IsDeleted==false && a.IsActive==true);
             
                var parentDept = data.ParentCode == null
                        ? null
                        : _repoDepartment.Get(a => a.Code.Equals(data.ParentCode) && a.IsDeleted==false && a.IsActive==true);
                if (parentDept == null)
                {
                    parentDept = departments.FirstOrDefault(a => a.Code.ToLower().Equals(data.ParentCode.ToLower()));
                }
                if (department == null)
                {
                    department = new Department()
                    {
                        Name = data.Name,
                        Code = data.Code,
                        IsActive = true,
                        IsDeleted = false,
                        Department2 = parentDept,
                        Description = data.Description,
                        CreatedDate = now,
                        is_training = data.IsTraining,
                    };
                    department.Ancestor = parentDept == null ? department.Code : parentDept.Ancestor + "!" + department.Code;
                    _repoDepartment.Insert(department);
                    departments.Add(department);
                }
                else
                {
                    var subdepts = _repoDepartment.GetAll(a => a.Ancestor.StartsWith(department.Ancestor + "!"));

                    department.Name = data.Name;
                    department.Department2 = parentDept;
                    department.Description = data.Description;
                    department.IsActive = true;
                    department.IsDeleted = false;
                    department.is_training = data.IsTraining;

                    var newAncestor = parentDept == null ? department.Code : parentDept.Ancestor + "!" + department.Code;
                    foreach (var subdept in subdepts)
                    {
                        subdept.Ancestor = subdept.Ancestor.Replace(department.Ancestor, newAncestor);
                        _repoDepartment.Update(subdept);
                    }
                    department.Ancestor = newAncestor;
                    _repoDepartment.Update(department);
                }
            }
            Uow.SaveChanges();
        }

        public void Update(Department entity)
        {
            _repoDepartment.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(Department entity)
        {
            _repoDepartment.Insert(entity);
            Uow.SaveChanges();
        }

        public void Modify(DepartmentModifyViewModel model)
        {
            model.Code = model.Code.Trim();
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            if (_repoDepartment.GetAll(a => a.Code.Equals(model.Code.Trim()) && a.Id != model.Id).Any())
            {
                throw new Exception(string.Format(Messege.EXISTING_CODE,  model.Code ));
            }
            var entity = model.Id.HasValue ? _repoDepartment.Get(model.Id) : new Department()
            {
                CreatedBy = model.CurrentUserId,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                Code = model.Code,
                
            };
            if (model.Id.HasValue)
            {
                entity.LastModifiedBy = model.CurrentUserId;
                entity.LastModifyDate = DateTime.Now;
            }
            entity.Name = model.Name;
            entity.Code = model.Code;
            entity.ParentId = model.ParentId;
            entity.Description = model.Description;
            entity.LastModifyDate = DateTime.Now;
            entity.is_training = model.is_training;
            if(model.is_training == true)
            {
                entity.headname = model.HeadName;
            }
           
            var parentDept = !model.ParentId.HasValue
                        ? null
                        : _repoDepartment.Get(a => a.Id == (model.ParentId) && a.IsDeleted==false);
            var newAncestor = parentDept == null ? entity.Code : parentDept.Ancestor + "!" + entity.Code;
            var subdepts = _repoDepartment.GetAll(a => a.Ancestor.StartsWith(entity.Ancestor + "!"));
            foreach (var subdept in subdepts)
            {
                subdept.LastModifyDate = DateTime.Now;
                subdept.LastModifyDate = DateTime.Now;
                subdept.LastModifiedBy = model.CurrentUserId;
                subdept.Ancestor = subdept.Ancestor.Replace(entity.Ancestor, newAncestor);
                _repoDepartment.Update(subdept);
            }
            entity.Ancestor = newAncestor;
            if(model.Id.HasValue && entity.Id > 0 )
                _repoDepartment.Update(entity);
            else
                _repoDepartment.Insert(entity);
            Uow.SaveChanges();
        }

        public bool Active(int id)
        {
            var entity = _repoDepartment.Get(id);
            if(entity == null) throw new Exception( string.Format(Messege.ERROR_NOTFOUND,Resource.DEPARTMENT) );
            entity.LastModifyDate = DateTime.Now;
            entity.IsActive = !entity.IsActive;
            if (entity.IsActive == false)
            {
                var entities = _repoDepartment.GetAll(a => a.Ancestor.StartsWith(entity.Ancestor + "!"));
                foreach (var dept in entities)
                {
                    dept.IsActive = false;
                    dept.LastModifyDate = DateTime.Now;
                    _repoDepartment.Update(dept);
                }
            }
            _repoDepartment.Update(entity);
            Uow.SaveChanges();
            return (bool)entity.IsActive;
        }
        public void Delete(int id)
        {
            var entity = _repoDepartment.Get(id);
            if(entity == null) throw new Exception( string.Format(Messege.ERROR_NOTFOUND, Resource.DEPARTMENT) );
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.Now;
            var entities = _repoDepartment.GetAll(a => a.Ancestor.StartsWith(entity.Ancestor + "!"));
            foreach (var dept in entities)
            {
                dept.IsDeleted = true;
                dept.IsActive = false;
                dept.DeletedDate = DateTime.Now;
                _repoDepartment.Update(dept);
            }
            _repoDepartment.Update(entity);
            Uow.SaveChanges();
        }

       
    }
}
