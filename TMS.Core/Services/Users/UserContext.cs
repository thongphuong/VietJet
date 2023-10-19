using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
namespace TMS.Core.Services.Users
{
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using System.Linq.Expressions;

    public class UserContext : BaseService, IUserContext
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<USER> _repositoryUser;
        private readonly IRepository<MENU> _repositoryMenu;
        private readonly IRepository<Function> _repositoryFunction;

        public UserContext(IUnitOfWork unitOfWork, IUnitOfWork uow, IRepository<USER> repositoryUser,
            IRepository<MENU> repositoryMenu, IRepository<Function> repositoryFunction, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _uow = uow;
            _repositoryUser = repositoryUser;
            _repositoryMenu = repositoryMenu;
            _repositoryFunction = repositoryFunction;
        }

        public IQueryable<USER> Get(Expression<Func<USER, bool>> query = null, IEnumerable<int> departments = null)
        {
            if (departments == null) return null;
            var entities = _repositoryUser.GetAll(/*a => a.UserPermissions.Any(x => departments.Any(z => z == x.DepartmentId))*/);
            return query != null
                ? entities.Where(query)
                : entities;
        }
        public USER GetById(int id)
        {
            return _repositoryUser.Get(id);
        }

        public USER GetByUser(string userName)
        {
            var user = _repositoryUser.GetAll().Where(x => x.USERNAME.Equals(userName)).FirstOrDefault();
            return user;
        }

        public USER GetByUserNameOrEmail(string userName, string email)
        {
            return _repositoryUser.Get(a => a.USERNAME.Equals(userName) || a.EMAIL.Equals(email));
        }
        public USER GetByEmail(string email)
        {
            return _repositoryUser.Get(a => a.EMAIL.Equals(email));
        }
        public USER Attempt(string userName)
        {
            return _repositoryUser.Get(a => a.USERNAME.Equals(userName));
        }
        public USER Login(string userName, string password)
        {
            return _repositoryUser.Get(a => a.USERNAME.Equals(userName) && a.PASSWORD.Equals(password));
        }

        public Dictionary<int, string> UserPermissions(List<int> functionIds)
        {
            return _repositoryFunction.GetAll(a => functionIds.Any(x => a.Id == x)).ToDictionary(a => a.Id, a => a.UrlAddress);
        }

        public MENU GetMenu(string menu)
        {
            return _repositoryMenu.Get(a => a.URL == menu);
        }

        public Function GetFunctionByUrl(string url)
        {
            return _repositoryFunction.Get(a => a.UrlAddress.Equals(url));
        }

        public bool UserHasRole(string url, Dictionary<int, List<int?>> userRoles)
        {
            var function = _repositoryFunction.Get(a => a.UrlAddress.Equals(url));
            return function != null && (function.ActionType == 0 || userRoles.Any(a => function.GroupPermissionFunctions.Any(x => x.GroupFunction.IsActive==true && x.GroupFunctionId == a.Key) && a.Value.Any(x => x == function.ActionType)));
        }

        public void Update(USER entity)
        {
            _repositoryUser.Update(entity);
            _uow.SaveChanges();
        }
        public bool CheckConfigSite(string key)
        {
            var user = GetUser();
            if (user.ConfigSite.Exists(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Filter Users by params. By ThuanNguyen - 04/10/2019
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="GroupUser"></param>
        /// <returns>List of Users</returns>
        public List<USER> GetByUserName_FirstName_LastName_Email_Phone_GroupUser(string userName, string firstName, string lastName, string email, string phone, List<int> GroupUser)
        {
            List<int> entities = new List<int>();
            if (GroupUser.Count > 0)
            {
                foreach (var item in GroupUser)
                {
                    var user = _repositoryUser.GetAll().Where(x => x.GroupUserAccesses.Any(y => y.GroupUserId == item)).Select(x => x.ID).ToList();
                    entities.AddRange(user);
                }
            }


            List<USER> entitiesAll = _repositoryUser.GetAll().Where(x => x.IsDeleted==false && x.ISACTIVE == 1 && (entities.Count > 0 && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone) 
            ? entities.Contains(x.ID)
            : (
            (string.IsNullOrEmpty(userName) || x.USERNAME.Trim().ToLower().Contains(userName)) 
            && (string.IsNullOrEmpty(firstName) || x.FIRSTNAME.Trim().ToLower().Contains(firstName)) 
            && (string.IsNullOrEmpty(lastName) || x.LASTNAME.Trim().ToLower().Contains(lastName)) 
            && (string.IsNullOrEmpty(email) || x.EMAIL.Trim().ToLower().Contains(email)) 
            && (string.IsNullOrEmpty(phone) || 
            (!string.IsNullOrEmpty(x.PHONENO) ? x.PHONENO.Trim().Contains(phone) : false)
            )
            ) 
            || entities.Contains(x.ID))).ToList();
            return entitiesAll.Distinct().ToList();
        }


    }
}
