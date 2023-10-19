using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Mail
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Nation;

    public interface IMailService
    {
        TMS_SentEmail GetById(int? id);
        IQueryable<TMS_SentEmail> Get(Expression<Func<TMS_SentEmail, bool>> query = null);
        void Update(TMS_SentEmail entity);
    }
}
