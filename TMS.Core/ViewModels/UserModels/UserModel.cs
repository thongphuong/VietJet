using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.UserModels
{
    using DAL.Entities;

    [Serializable()]
    public class UserModel
    {
        public int USER_ID { get; set; }
        public string Avatar { get; set; }
        public string UserCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string Company { get; set; }
        public int? DepartmentId { get; set; }
        public string LanguageAbbreviation { get; set; }
        public int? UserState { get; set; }
        public DateTime? OnlineAt { get; set; }
        public DateTime? LastOnlineAt { get; set; }
        public int? IsActive { get; set; }
        public Dictionary<int, List<int?>> FunctionIds { get; set; }
        public List<int> PermissionIds { get; set; }
        public List<int> RoleId { get; set; }
        public bool IsMaster { get; set; }

        /// <summary>
        /// List check Configsite
        /// </summary>
        public List<string> ConfigSite { get; set; }


      
       
        public UserModel()
        {
        }

        public UserModel(USER user, IEnumerable<CONFIG> cONFIG)
        {
            this.USER_ID = user.ID;
            this.UserCode = "Admin"; //todo: get instructor code
            this.Username = user.USERNAME;
            this.Password = user.PASSWORD;
            this.FirstName = user.FIRSTNAME;
            this.LastName = user.LASTNAME;
            this.Email = user.EMAIL;
            this.Address = user.ADDRESS;
            this.PhoneNo = user.PHONENO;
            this.Company = user.COMPANY;
            this.LanguageAbbreviation = user.LANGUAGEABBREVIATION;
            this.RoleId = user.UserRoles.Select(a => (int)a.RoleId).ToList();
            this.UserState = user.USERSTATE;
            this.OnlineAt = user.ONLINEAT;
            this.LastOnlineAt = user.LASTONLINEAT;
            this.IsActive = user.ISACTIVE;
            this.IsMaster = user.UserRoles.Any(x => x.RoleId == 1); //is super admin
            this.Avatar = user.Avatar;
            var userFunctions = user.UserRoles.SelectMany(
                a =>
                    a.ROLE.ROLEMENUs.Where(b => b.IsActive == true).Select(x => new {x.GroupFunctionId, x.MenuType}))
                .ToList();

            var userPermissions =
                user.UserPermissions.Where(a => a.Department.IsDeleted==false && a.Department.IsActive==true)
                    .Select(a => (int) a.DepartmentId)
                    .ToList();
            var groupUser = user.GroupUserAccesses.Where(a=>a.GroupUser.IsActive == true).Select(a => a.GroupUser);
            userFunctions.AddRange(
                groupUser.SelectMany(
                    a =>
                        a.GroupUserMenus.Where(b => b.IsActive == true).Select(
                            x => new {GroupFunctionId = x.GroupFunctionId, MenuType = x.FunctionType})));
            userPermissions.AddRange(groupUser.SelectMany(a => a.GroupUserPermissions.Select(x => (int) x.DepartmentId)));

            this.FunctionIds = userFunctions.GroupBy(a => (int)a.GroupFunctionId, a => a.MenuType)
                .ToDictionary(a => a.Key, a => a.Where(x => x.HasValue).ToList());
            this.PermissionIds = userPermissions.Distinct().ToList();

            this.ConfigSite = cONFIG == null ?new List<string>() : cONFIG.Select(a => a.KEY).ToList();

        }
    }
}
