using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels.PostNews;

namespace TMS.Core.Services.PostNews
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface IPostNewsService : IBaseService
    {
        Postnew GetPostNewById(int? id);

        IQueryable<Postnew> GetAllPostNews(Expression<Func<Postnew, bool>> query = null,bool isApi = false);

        void ModifyPostNew(PostNewsModel model, string userName, string fullName);
        void UpdatePostNew(Postnew model);

        IQueryable<PostNew_GroupTrainee> GetPostNewsGroup(Expression<Func<PostNew_GroupTrainee, bool>> query = null);
    }
}
