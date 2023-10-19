using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.PostNews
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Entities;
    using ViewModels.PostNews;

    public interface IPostCategoryService : IBaseService
    {
        Postnews_Category GetById(int? id);

        IQueryable<Postnews_Category> GetAll(Expression<Func<Postnews_Category, bool>> query = null, bool isApi = false);

        void Update(Postnews_Category model);

        void Modify(PostNewsCategoryModel model, string userName);
    }
}
