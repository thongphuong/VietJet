using System;

namespace TMS.Core.Services.Notifications
{
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.UserModels;

    public interface INotificationService : IBaseService
    {
        void Notification_Insert(int typeNotification, int? typelog, int? idApproval, int to = -1, DateTime? datesend = null, string messenge = "", string messengeContent = "", string messenge_VN = "", string messengeContent_VN = "");
        IQueryable<Notification_Detail> GetDetails();
        IQueryable<Notification_Detail> GetDetails(Expression<Func<Notification_Detail, bool>> query);
        IQueryable<Notification> GetNotification();
        IQueryable<Notification> GetNotification(Expression<Func<Notification, bool>> query);
        Notification_Detail GetDetailById(int id);
        Notification GetById(int id);
        void Update(Notification_Detail entity);
        void Insert(Notification entity);
    }
}
