using System;
using System.Collections.Generic;
using System.Linq;
using DAL.UnitOfWork;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using DAL.Repositories;
using TMS.Core.ViewModels.PostNews;
using TMS.Core.Utils;
namespace TMS.Core.Services.PostNews
{
    public class PostCategoryService : BaseService, IPostCategoryService
    {
        private readonly IRepository<Postnews_Category> _repoCategory;

        private const string Prefix = "C";
        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        public PostCategoryService(IUnitOfWork unitOfWork, IRepository<Postnews_Category> repoCategory, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCategory = repoCategory;
        }

        public IQueryable<Postnews_Category> GetAll(Expression<Func<Postnews_Category, bool>> query = null, bool isApi = false)
        {
            var entities = isApi ? _repoCategory.GetAll() : _repoCategory.GetAll(a => a.IsDeleted==false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public Postnews_Category GetById(int? id)
        {
            return id.HasValue ? _repoCategory.Get(id) : null;
        }

        public void Modify(PostNewsCategoryModel model, string userName)
        {
            if (model.Id != -1)
            {
                var entity = _repoCategory.Get(model.Id);
                if (entity == null) throw new Exception("PostNews Category is not found");

                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.Icon = model.Icon;
                entity.Background = model.BackgroundColor;
                entity.LMSStatus = statusModify;
                entity.ModifyBy = userName;
                entity.ModifyDate = DateTime.Now;
                _repoCategory.Update(entity);

            }
            else
            {
                var entity = new Postnews_Category
                {
                    Code = GenCateCode(model.Sort ?? 0),
                    Name = model.Name,
                    Description = model.Description,
                    CreateBy = userName,
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Icon = model.Icon,
                    Background = model.BackgroundColor,
                    LMSStatus = statusModify,
                };
                var parentCode = _repoCategory.Get(model.ParentId);
                entity.Ancestor = parentCode == null ? entity.Code : parentCode.Ancestor + "_" + entity.Code;
                _repoCategory.Insert(entity);
            }

            Uow.SaveChanges();
        }

        public void Update(Postnews_Category entity)
        {
            _repoCategory.Update(entity);
            Uow.SaveChanges();
        }
        private string GenCateCode(int sort, int id = -1)
        {
            var entities = _repoCategory.GetAll();
            var cateId = entities.Any() ? entities.Max(a => a.Id) + 1 + "" : "00001";
            cateId = (id != -1) ? id + "" : cateId;
            while (cateId.Length < 5)
            {
                cateId = "0" + cateId;
            }
            var indexPrefix = sort + "";
            while (indexPrefix.Length < 5)
            {
                indexPrefix = "0" + indexPrefix;
            }
            return indexPrefix + Prefix + cateId;
        }
    }
}
