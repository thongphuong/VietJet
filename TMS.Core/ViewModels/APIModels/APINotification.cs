using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.APIModels
{
    public class APINotification
    {
        public string title { get; set; }
        public string content { get; set; }
        public string creation_date { get; set; }

        public int? teacher_id { get; set; }
        public int? type { get; set; }
      

        public IEnumerable<APINotificationDetail> notification_details { get; set; } 

  
        public class APINotificationDetail
        {
            public int? trainee_id { get; set; }
            public string date_send { get; set; }
            public int? status { get; set; }
            public APINotificationDetail(Notification_Detail notificationDetail)
            {
                this.trainee_id = notificationDetail.iddata.HasValue ? notificationDetail.iddata : -1; // <<<<<<<<<<<<<<<<<< nghi van iddata <> idtrainee
                this.date_send = notificationDetail.datesend?.ToString("dd/MM/yyyy");
                this.status = notificationDetail.status.HasValue ? notificationDetail.status : 0;


            }
        }

        //TODO : them cot Type trong database bang Notification
        public APINotification(Notification notification)
        {
            this.title = notification.Message;
            this.content = notification.MessageContent;
            this.creation_date = notification.Date?.ToString("dd/MM/yyyy");
           // this.teacher_id = notification.UserID.HasValue ? notification.UserID : -1;
           // this.type = notification.Status.HasValue ? notification.Status : -1; // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Fix Here Status = Type (UtilConstants.NotificationType)
            this.notification_details = notification.Notification_Detail.Select(a => new APINotificationDetail(a));
        }
    }

   
}
