using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Roles
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.Utils;

    public interface IRoleService : IBaseService
    {
        ROLE GetById(int? id);
        IQueryable<ROLE> Get();
        IQueryable<ROLE> Get(Expression<Func<ROLE,bool>> query);
        IQueryable<ROLE> GetAjaxHandler(Expression<Func<ROLE, bool>> query);
        void Insert(ROLE entity);
        void Update(ROLE entity, UtilConstants.LogEvent logEvent);
        void Update(int idrole, int idpage, int optiontype, int onoff);


    }
}
