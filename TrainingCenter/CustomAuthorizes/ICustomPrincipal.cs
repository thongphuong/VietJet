using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.CustomAuthorizes
{
    using System.Security.Principal;

    interface ICustomPrincipal : IPrincipal
    {
        bool HasPermission(int? permissionId);
       
    }
}
