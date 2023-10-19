using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.ViewModels.PostNews;
using DAL.Repositories;
using DAL.UnitOfWork;
using TMS.Core.Utils;
namespace TMS.Core.Services.PostNews
{
    public class PostNewsService : BaseService, IPostNewsService
    {
        private readonly IRepository<Postnew> _repoPostNew;
        private readonly IRepository<PostNew_GroupTrainee> _repoPostNewGroupTrainee;
        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        public PostNewsService(IUnitOfWork unitOfWork, IRepository<Postnew> repoPostNew, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<PostNew_GroupTrainee> repoPostNewGroupTrainee) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoPostNew = repoPostNew;
            _repoPostNewGroupTrainee = repoPostNewGroupTrainee;
        }
        public IQueryable<Postnew> GetAllPostNews(Expression<Func<Postnew, bool>> query = null, bool isApi = false)
        {
            var entities = isApi ? _repoPostNew.GetAll() : _repoPostNew.GetAll(a => a.IsDeleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public Postnew GetPostNewById(int? id)
        {
            return id.HasValue ? _repoPostNew.Get(id) : null;
        }

        public IQueryable<PostNew_GroupTrainee> GetPostNewsGroup(Expression<Func<PostNew_GroupTrainee, bool>> query = null)
        {
            var entities = _repoPostNewGroupTrainee.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }

        public void ModifyPostNew(PostNewsModel model, string userName, string fullName)
        {
            var entity = _repoPostNew.Get(a => a.Id == model.Id);
            if (entity != null)
            {
                entity.Title = model.Title;
                entity.Content = model.Content;
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;
                entity.ModifyDate = DateTime.Now;
                entity.ModifyBy = userName;
                //entity.Sort = model.Sort;                
                entity.Image = string.IsNullOrEmpty(model.ImgName) ? entity.Image : model.ImgName;
                entity.Type = model.Type;
                entity.CategoryID = model.Category;
                entity.Description = model.Description;
                entity.LMSStatus = statusModify;
                _repoPostNew.Update(entity);
            }
            else
            {
                entity = new Postnew
                {
                    Title = model.Title,
                    Content = model.Content,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreationDate = DateTime.Now,
                    CreationBy = userName,
                    PostBy = fullName,
                    IsActive = true,
                    IsDeleted = false,
                    Image = model.ImgName,
                    Type = model.Type,
                    CategoryID = model.Category,
                    Description = model.Description,
                    LMSStatus = statusModify,
                };
                //entity.Sort = model.Sort;
                _repoPostNew.Insert(entity);
            }
            if (model.GroupTraineeListID != null)
            {
                _repoPostNewGroupTrainee.Delete(entity.PostNew_GroupTrainee);
                foreach (var item in model.GroupTraineeListID)
                {
                    entity.PostNew_GroupTrainee.Add(new PostNew_GroupTrainee
                    {
                        PostID = entity.Id,
                        GroupTraineeID = item,
                    });
                }
            }
            else
            {
                _repoPostNewGroupTrainee.Delete(entity.PostNew_GroupTrainee);
            }
            Uow.SaveChanges();
        }

        public void UpdatePostNew(Postnew entity)
        {
            _repoPostNew.Update(entity);
            Uow.SaveChanges();
        }
    }
}
