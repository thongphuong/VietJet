using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Mail
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.Nation;
    using TMS.Core.ViewModels.UserModels;

    public class MailService : BaseService, IMailService
    {
        private readonly IRepository<TMS_SentEmail> _repoTMS_SentEmail;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public MailService(IUnitOfWork unitOfWork, IRepository<TMS_SentEmail> repoTMS_SentEmail, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoTMS_SentEmail = repoTMS_SentEmail;
        }

        public TMS_SentEmail GetById(int? id)
        {
            return !id.HasValue ? null : _repoTMS_SentEmail.Get(id.Value);
        }

      
        public IQueryable<TMS_SentEmail> Get(Expression<Func<TMS_SentEmail, bool>> query = null)
        {
            var entities = _repoTMS_SentEmail.GetAll(a => (bool)!a.Is_Active);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities;
        }
        
        public void Update(TMS_SentEmail entity)
        {
            _repoTMS_SentEmail.Update(entity);
            Uow.SaveChanges();
        }

    }
}
