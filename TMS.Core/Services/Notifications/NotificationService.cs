using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.Notifications
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.UserModels;
    using TMS.Core.Utils;

    public class NotificationService : BaseService, INotificationService
    {
        private readonly IRepository<Notification> _repoNotification;
        private readonly IRepository<Notification_Detail> _repoNotificationDetail;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public NotificationService(IUnitOfWork unitOfWork, IRepository<Notification_Detail> repoNotificationDetail, IRepository<Notification> repoNotification, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoNotificationDetail = repoNotificationDetail;
            _repoNotification = repoNotification;
        }

        public void Notification_Insert( int typeNotification, int? typelog, int? idApproval, int to = -1, DateTime? datesend = null, string messenge = "", string messengeContent = "", string messenge_VN = "", string messengeContent_VN = "")
        {
           
                string url_ = "";
                #region[check url]
                switch (typelog)
                {
                    case 1://request HOD course
                        url_ = "/Approve/Course/" + idApproval;
                        break;
                    case 2://request HOD trainee
                        url_ = "/Approve/AssignTrainee/" + idApproval;
                        break;
                    case 3://request HOD subject
                        url_ = "/Approve/SubjectResult/" + idApproval;
                        break;
                    case 4://request HOD final
                        url_ = "/Approve/CourseResult/" + idApproval;
                        break;
                    case 5://request user 
                        url_ = "/Course/Details/" + idApproval;
                        break;
                }
                #endregion
                //TODO : co thay doi iduser = iddata
                var currUser = CurrentUser;
                var notification = new Notification {Message = messenge, MessageContent = messengeContent,MessageVN = messenge_VN,MessageContentVN = messengeContent_VN, URL = url_ ,Type = typeNotification };
                ///////////////////////////////////////
                notification.Notification_Detail.Add(new Notification_Detail
                {
                    idmessenge = notification.MessageID,
                    datesend = datesend,
                    iddata = to,
                    iduserfrom = currUser?.USER_ID,
                    status = 0,
                    IsDeleted = false,
                    IsActive = true
                });
                _repoNotification.Insert(notification);
                Uow.SaveChanges();
        }
        public IQueryable<Notification_Detail> GetDetails()
        {
            return _repoNotificationDetail.GetAll(c => c.IsDeleted == false && c.IsActive == true).OrderByDescending(c => c.idmessenge);
        }
        public IQueryable<Notification_Detail> GetDetails(Expression<Func<Notification_Detail, bool>> query = null)
        {
            var entities = _repoNotificationDetail.GetAll(c => c.iddata == CurrentUser.USER_ID && c.IsDeleted == false && c.IsActive == true);
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Notification> GetNotification()
        {
            return _repoNotification.GetAll().OrderBy(c => c.MessageID);
        }
        public IQueryable<Notification> GetNotification(Expression<Func<Notification, bool>> query = null)
        {
            var entities = _repoNotification.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public Notification_Detail GetDetailById(int id)
        {
            return _repoNotificationDetail.Get(id);
        }
        public Notification GetById(int id)
        {
            return _repoNotification.Get(id);
        }
        public void Update(Notification_Detail entity)
        {
            _repoNotificationDetail.Update(entity);
            Uow.SaveChanges();
        }

        public void Insert(Notification entity)
        {
            _repoNotification.Insert(entity);
            Uow.SaveChanges();
        }
    }
}
