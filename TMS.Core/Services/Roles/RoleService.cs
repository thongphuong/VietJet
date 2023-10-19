using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Roles
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.App_GlobalResources;

    public class RoleService : BaseService, IRoleService
    {
        private readonly IRepository<ROLE> _repoRole; 
        private readonly IRepository<GroupFunction> _repoGroupFunction;
        private readonly Expression<Func<ROLE, bool>> _roleDefaultFilter = a => a.IsDeleted!= true && a.IsActive==true; 
        public RoleService(IUnitOfWork unitOfWork, IRepository<ROLE> repoRole, IRepository<GroupFunction> repoGroupFunction, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoRole = repoRole;
            _repoGroupFunction = repoGroupFunction;
        }

        public ROLE GetById(int? id)
        {
            return _repoRole.Get(id);
        }

        public IQueryable<ROLE> Get()
        {
            return _repoRole.GetAll(_roleDefaultFilter);
        }

        public IQueryable<ROLE> Get(Expression<Func<ROLE, bool>> query)
        {
            return query == null ? _repoRole.GetAll(_roleDefaultFilter) : _repoRole.GetAll(_roleDefaultFilter).Where(query);
           
        }
        public IQueryable<ROLE> GetAjaxHandler(Expression<Func<ROLE, bool>> query)
        {
            return query == null ? _repoRole.GetAll(a => a.IsDeleted==false).OrderBy(m=>m.ID) : _repoRole.GetAll(a => a.IsDeleted==false).Where(query).OrderBy(m=>m.ID);

        }
        public void Insert(ROLE entity)
        {
            _repoRole.Insert(entity);
            Uow.SaveChanges();
            var _entity = _repoRole.GetAll(a => a.ID == entity.ID).Select(a => new { a.ID, a.NAME, a.DESCRIPTION, a.IsDeleted, a.IsActive });
            LogEvent(UtilConstants.LogType.EVENT_TYPE_INFORMATION,UtilConstants.LogSourse.Role, UtilConstants.LogEvent.Insert, _entity);
        }

        public void Update(ROLE entity, UtilConstants.LogEvent logEvent)
        {
            var model = _repoRole.Get(entity.ID);
            if (model != null)
            {
                _repoRole.Update(entity);
            }
            else
            {
                entity.CREATED_BY = GetUser().USER_ID;
                entity.CREATION_DATE = DateTime.Now;
                entity.IsDeleted = false;
                entity.IsActive = true;
                _repoRole.Insert(entity);
            }
            Uow.SaveChanges();
            var _entity = _repoRole.GetAll(a => a.ID == entity.ID).Select(a => new { a.ID, a.NAME, a.DESCRIPTION, a.IsDeleted, a.IsActive });
            LogEvent(UtilConstants.LogType.EVENT_TYPE_INFORMATION, UtilConstants.LogSourse.Role, logEvent, _entity);
        }
        public void Update(int idrole, int idGroup, int optiontype, int onoff)
        {
            var isActive = onoff == 1;
            var role = _repoRole.Get(a => a.ID == idrole);
            if (role == null) throw new Exception(string.Empty +  Messege.VALIDATION_ROLE_DATA );
            var groupFunction = _repoGroupFunction.Get(idGroup);
            if(groupFunction.IsActive==false || groupFunction == null) throw new Exception(string.Empty +  Messege.VALIDATION_ROLE_MENU_DATA );
            int[] menuTypes;
            if (optiontype == (int) UtilConstants.ROLE_FUNCTION.FullOption)
                menuTypes = groupFunction.GroupPermissionFunctions.Select(a=>a.Function).Where(a => a.ActionType.HasValue).Select(a => (int)a.ActionType).Distinct().ToArray();
            else
                menuTypes = groupFunction.GroupPermissionFunctions.Select(a=>a.Function).Where(a => a.ActionType == optiontype).Select(a => (int)a.ActionType).Distinct().ToArray();
            for (var i = 0; i < menuTypes.Count(); i++)
            {
                var menuType = menuTypes[i];
                var roleMenu = role.ROLEMENUs.FirstOrDefault(a => a.GroupFunctionId == idGroup && a.MenuType == menuType);
                if (roleMenu == null)
                {
                    role.ROLEMENUs.Add(new ROLEMENU()
                    {
                        IsActive = isActive,
                        ROLE = role,
                        GroupFunction = groupFunction,
                        MenuType = menuType,
                    });
                }
                else
                {
                    roleMenu.IsActive = isActive;
                }
            }
            _repoRole.Update(role);
            Uow.SaveChanges();
        }
    }
}
