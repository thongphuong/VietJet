using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.App_GlobalResources;

namespace TMS.Core.Services.GroupUser
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using ViewModels.GroupUserModels;

    public class GroupUserService : BaseService, IGroupUserService
    {
        private readonly IRepository<GroupUser> _repoGroupUser;
        private readonly IRepository<GroupUserMenu> _repoGroupUserMenu;

        private readonly IRepository<GroupFunction> _repoGroupFunction;

        private readonly IRepository<GroupUserPermission> _repoGroupUserPermission;
        private readonly IRepository<Department> _repoDepartment;
        private readonly Expression<Func<GroupUser, bool>> _groupUserDefaultFilter = a => a.IsDeleted!=true;
        public GroupUserService(IUnitOfWork unitOfWork, IRepository<GroupUser> repoGroupUser, IRepository<GroupUserMenu> repoGroupUserMenu, IRepository<GroupUserPermission> repoGroupUserPermission, IRepository<GroupFunction> repoGroupFunction, IRepository<Department> repoDepartment, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoGroupUser = repoGroupUser;
            _repoGroupUserMenu = repoGroupUserMenu;
            _repoGroupUserPermission = repoGroupUserPermission;
            _repoGroupFunction = repoGroupFunction;
            _repoDepartment = repoDepartment;
        }

        public IQueryable<GroupUser> Get(Expression<Func<GroupUser, bool>> query = null)
        {
            return query == null ? _repoGroupUser.GetAll(_groupUserDefaultFilter) : _repoGroupUser.GetAll(_groupUserDefaultFilter).Where(query);
        }

        public GroupUser GetById(int? id)
        {
            if (!id.HasValue)
                return null;
            var entity = _repoGroupUser.Get(id);
            return entity.IsDeleted==true ? null : entity;
        }

        public int Insert(GroupUserModel model)
        {
            var entity = new GroupUser()
            {
                IsActive = true,
                IsDeleted = false,
                Name = model.Name,
                Title = model.Title
            };
            _repoGroupUser.Insert(entity);
            Uow.SaveChanges();
            return entity.Id;
        }

        public void Update(GroupUser entity)
        {
            _repoGroupUser.Update(entity);
            Uow.SaveChanges();
        }

        public void Update(int idGroupUser, int idMenu, int optiontype, int onoff)
        {
            var isActive = onoff == 1;
            var groupUser = _repoGroupUser.Get(a => a.IsDeleted==false && a.Id == idGroupUser);
            if (groupUser == null) throw new Exception(string.Format(Messege.ERROR_NOTFOUND, Resource.lblGroupUser));

            var groupFunction = _repoGroupFunction.Get(idMenu);
            if (groupFunction.IsActive==false || groupFunction == null) throw new Exception( "Invalid Menu data" );

            int[] menuTypes;
            if (optiontype == (int)UtilConstants.ROLE_FUNCTION.FullOption)
                menuTypes = groupFunction.GroupPermissionFunctions.Select(a => a.Function).Where(a => a.ActionType.HasValue).Select(a => (int)a.ActionType).Distinct().ToArray();
            else
                menuTypes = groupFunction.GroupPermissionFunctions.Select(a => a.Function).Where(a => a.ActionType == optiontype).Select(a => (int)a.ActionType).Distinct().ToArray();
            for (var i = 0; i < menuTypes.Count(); i++)
            {
                var menuType = menuTypes[i];
                var groupUserMenu = groupUser.GroupUserMenus.FirstOrDefault(a => a.GroupFunctionId == idMenu && a.FunctionType == menuType);
                if (groupUserMenu == null)
                {
                    groupUser.GroupUserMenus.Add(new GroupUserMenu()
                    {
                        IsActive = isActive,
                        GroupUser = groupUser,
                        GroupFunction = groupFunction,
                        FunctionType = menuType,

                    });
                }
                else
                {
                    groupUserMenu.IsActive = isActive;
                }
            }
            _repoGroupUser.Update(groupUser);
            Uow.SaveChanges();
        }

        public GroupUser Modify(GroupUserModel model)
        {
            if (model == null) return null;
            var entity = _repoGroupUser.Get(model.Id);
            if (_repoGroupUser.GetAll(m => m.Name == model.Name && m.Id != model.Id).Any())
            {
                throw new Exception(string.Format(Messege.DataIsExists,Resource.lblGroupUser,  model.Name ));
            }
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.Title = model.Title;
                _repoGroupUser.Update(entity);

            }
            else
            {
                entity = new GroupUser
                {
                    Name = model.Name,
                    Title = model.Title,
                    IsDeleted = false,
                    IsActive = true
                };

                _repoGroupUser.Insert(entity);
            }
            Uow.SaveChanges();
            return entity;
        }

        public void GrantPermission(int groupUserId, int permissionId)
        {
            var entity = _repoGroupUser.Get(groupUserId);
            if (entity == null) throw new Exception( "Data is not found" );
            var permission = entity.GroupUserPermissions.FirstOrDefault(a => a.DepartmentId == permissionId);
            if (permission == null)//update
            {
                var entityDepartment = _repoDepartment.Get(permissionId);
                var departments = _repoDepartment.GetAll(a => a.IsActive==true && a.IsDeleted==false && a.Ancestor.StartsWith(entityDepartment.Ancestor)).Select(a => a.Id).ToList();
                foreach (var department in departments.Where(department => entity.GroupUserPermissions.All(x => x.DepartmentId != department)))
                {
                    entity.GroupUserPermissions.Add(new GroupUserPermission()
                    {
                        GroupUserId = entity.Id,
                        DepartmentId = department,
                    });
                }
            }
            else
            {
                var department = _repoDepartment.GetAll(a => a.IsActive==true && a.IsDeleted==false && a.Ancestor.StartsWith(permission.Department.Ancestor)).Select(a => a.Id).ToList();
                var userDepartments = entity.GroupUserPermissions.Where(a => a.DepartmentId != null && department.Contains((int)a.DepartmentId)).ToList();
                foreach (var groupUserPermission in userDepartments)
                {
                    _repoGroupUserPermission.Delete(groupUserPermission);
                }
            }
            Uow.SaveChanges();
        }
        public void GrantPermissionAll(int groupUserId, int[] permissionIds, bool? isAll = false)
        {
            var entity = _repoGroupUser.Get(groupUserId);
            if (entity == null) throw new Exception( "Data is not found" );

            foreach (var permissionId in permissionIds)
            {
                if (isAll == false)
                {
                    var entityDepartment = _repoDepartment.Get(permissionId);
                    var departments = _repoDepartment.GetAll(a => a.IsActive==true && a.IsDeleted==false && a.Ancestor.StartsWith(entityDepartment.Ancestor)).Select(a => a.Id).ToList();
                    foreach (var department in departments.Where(department => entity.GroupUserPermissions.All(x => x.DepartmentId != department)))
                    {
                        entity.GroupUserPermissions.Add(new GroupUserPermission
                        {
                            GroupUserId = entity.Id,
                            DepartmentId = department,
                        });
                    }
                }
                else
                {


                    var permission = entity.GroupUserPermissions.FirstOrDefault(a => a.DepartmentId == permissionId);
                    if (permission != null)
                    {
                        var department = _repoDepartment.GetAll(a => a.IsActive==true && a.IsDeleted==false && a.Ancestor.StartsWith(permission.Department.Ancestor)).Select(a => a.Id).ToList();
                        var userDepartments = entity.GroupUserPermissions.Where(a => a.DepartmentId != null && department.Contains((int)a.DepartmentId)).ToList();
                        _repoGroupUserPermission.Delete(userDepartments);
                    }

                }
            }


            Uow.SaveChanges();
        }
    }
}
