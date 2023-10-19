using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingCenter.CustomAuthorizes
{
    using System.Security.Principal;
    using TMS.Core.Services.Users;

    public class CustomPrincipal : ICustomPrincipal
    {
        public  IUserContext UserContext { get; set; }
        private Dictionary<int,List<int?>> Roles { get;}
        public CustomPrincipal(string userName, Dictionary<int, List<int?>> roles)
        {
            Identity = new GenericIdentity(userName);
            Roles = roles;
        }
        public bool IsInRole(string role)
        {
            return UserContext.UserHasRole(role, Roles);
        }

        public IIdentity Identity
            { get; private set; }

        public bool HasPermission(int? permissionId)
        {
            throw new NotImplementedException();
        }
    
    }
}