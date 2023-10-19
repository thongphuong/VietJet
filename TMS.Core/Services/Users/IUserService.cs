using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels;

namespace TMS.Core.Services.Users
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.UserModels;

    public interface IUserService : IBaseService
    {
        IQueryable<USER> GetAll(Expression<Func<USER, bool>> query = null, IEnumerable<int> departments = null,bool isMater = false);
        USER Get(Expression<Func<USER, bool>> query = null);
        USER GetById(int? id);

        USER GetByName(string username);
        void Insert(USER entity);
        void Update(USER entity);
        void GrantPermission(int userId,int permissionId);
        int Insert(TMS.Core.ViewModels.UserModels.UserProfile model);
        void Update(TMS.Core.ViewModels.UserModels.UserProfile model);
        USER Modify(TMS.Core.ViewModels.UserModels.UserProfile model, string pass, string token);
        USER ChangeProfile(ChangeUserProfile model );


       
    }
}
