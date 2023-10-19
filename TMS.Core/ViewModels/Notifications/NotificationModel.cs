using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.Notifications
{
    public class NotificationModel 
    {
        public int Count { get; set; }
        public IEnumerable<Notification_Detail> NotificationDetail { get; set; }
         
       
    }
}
