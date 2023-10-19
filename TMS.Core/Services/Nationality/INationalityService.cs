using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Nationality
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Nation;

    public interface INationalityService : IBaseService
    {
        Nation GetById(int? id);
        Nation GetByName(string name);
        Nation GetByCode(string code);
        IQueryable<Nation> Get(Expression<Func<Nation, bool>> query = null);
        void Insert(Nation entity);
        void Update(Nation entity);
    }
}
