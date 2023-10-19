using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Companies
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Company;

    public interface ICompanyService
    {
        Company GetById(int? id);
        Company GetByName(string name);
        Company GetByCode(string code);
        IQueryable<Company> Get(Expression<Func<Company,bool>> query =null ); 
        void Insert(Company entity);
        void Update(Company entity);
    }
}
