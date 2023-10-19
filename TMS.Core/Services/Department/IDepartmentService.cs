using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Department
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Departments;
    using TMS.Core.ViewModels.ViewModel;

    public interface IDepartmentService : IBaseService
    {
        Department GetById(int? id = null);

       
        IQueryable<Department> Get();
        IQueryable<Department> ApiGet(Expression<Func<Department, bool>> query);
        IQueryable<Department> Get(Expression<Func<Department, bool>> query);
        IQueryable<Department> Get(Expression<Func<Department, bool>> query,IEnumerable<int> permissions);
        void Import(Dictionary<string,DepartmentImportViewModel> importList);
        void Update(Department entity);
        void Insert(Department entity);
        void Modify(DepartmentModifyViewModel model);
        bool Active(int id);
        void Delete(int id);
    }
}
