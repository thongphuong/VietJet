using System;
namespace TMS.Core.Services.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using DAL.Entities;
    using System.Linq.Expressions;

    public interface IUserContext : IBaseService
    {
        IQueryable<USER> Get(Expression<Func<USER, bool>> query = null, IEnumerable<int> departments = null);
        USER GetById(int id);
        USER GetByUserNameOrEmail(string userName, string email);
        List<USER> GetByUserName_FirstName_LastName_Email_Phone_GroupUser(string userName, string firstName, string lastName, string email, string phone, List<int> GroupUser = null);
        USER GetByUser(string userName);
        USER GetByEmail(string email);

        USER Attempt(string userName);
        USER Login(string userName,string password);
        Dictionary<int, string> UserPermissions(List<int> functionIds);
        MENU GetMenu(string menu);
        Function GetFunctionByUrl(string url);
        bool UserHasRole(string url, Dictionary<int, List<int?>> userRoles);
        void Update(USER entity);
        bool CheckConfigSite(string key);
    }
}
