using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.GroupUser
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.UserModels;
    using ViewModels.GroupUserModels;

    public interface IGroupUserService
    {

        IQueryable<GroupUser> Get(Expression<Func<GroupUser, bool>> query = null);
        GroupUser GetById(int? id);
        int Insert(GroupUserModel model);
        void Update(GroupUser entity);

        GroupUser Modify(GroupUserModel model);

        void GrantPermission(int groupUser , int permissionId);
        void GrantPermissionAll(int groupUser, int[] permissionId, bool? isall = false);

        void Update(int idGroupUser, int idMenu, int optiontype, int onoff);
    }
}
