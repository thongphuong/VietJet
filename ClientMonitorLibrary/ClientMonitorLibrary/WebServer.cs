using ClientMonitorLibrary.VietjetAPI_SiteView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ClientMonitorLibrary.Web
{
    public class WebServer
    {
        private static readonly WebServer _instance = new WebServer();
        private int Number_Of_Current_Visitors;
        private int Number_Of_Request;
        private int SiteId;
        private String SiteName;
        private WebServer()
        {
            Number_Of_Current_Visitors = 0;
            Number_Of_Request = 0;
            Thread_Send_Request_Count(30).Start();
        }
        public static WebServer GetInstance(int siteId, string siteName)
        {
            _instance.SiteId = siteId;
            _instance.SiteName = siteName;
            return _instance;
        }
        public static WebServer GetInstance()
        {
            return _instance;
        }
        public void CountRequest()
        {
            Number_Of_Request = Number_Of_Request + 1;
        }
        public void Do_Session_Start()
        {
            Number_Of_Current_Visitors = Number_Of_Current_Visitors + 1;
            SendOnlineUser(Number_Of_Current_Visitors);
            //return Number_Of_Current_Visitors;
        }

        public void Do_Session_End()
        {
            Number_Of_Current_Visitors = Number_Of_Current_Visitors - 1;
            SendOnlineUser(Number_Of_Current_Visitors);
            //return Number_Of_Current_Visitors;
        }
        void SendOnlineUser(int OnlineUser)
        {
            try
            {
                SiteViewClient client = new SiteViewClient();
                UpdateOnlineUserRequest request = new UpdateOnlineUserRequest();
                request.SiteId = SiteId;
                request.SiteName = SiteName;
                request.OnlineUser = OnlineUser;
                client.UpdateOnlineUser(request);
            }
            catch (Exception ex) {
                ex.ToString();
            }
        }

        void SendNumberOfRequest()
        {
            try
            {
                SiteViewClient client = new SiteViewClient();
                UpdateRequestCountRequest request = new UpdateRequestCountRequest();
                request.SiteId = SiteId;
                request.SiteName = SiteName;
                request.RequestCount = Number_Of_Request;
                client.UpdateRequestCount(request);
                Number_Of_Request = 0;
            }
            catch (Exception ex) {
                ex.ToString();
            }
        }

        public Thread Thread_Send_Request_Count(int WaitingTime)
        {
            var Send_Request_Count = new Thread(() =>
            {
                while (true)
                {
                    SendNumberOfRequest();
                    Thread.Sleep(WaitingTime * 1000);
                }
            });

            Send_Request_Count.IsBackground = true;
            Send_Request_Count.Priority = ThreadPriority.BelowNormal;
            return Send_Request_Count;
        }


        public void SendActivitive(String ActionName, String Description,int Level)
        {
            try
            {
                SiteViewClient client = new SiteViewClient();
                UpdateActivitiveRequest request = new UpdateActivitiveRequest();
                request.SiteId = SiteId;
                request.SiteName = SiteName;
                request.ActionName = ActionName;
                request.Description = Description;
                request.Level = Level;
                client.UpdateActivitive(request);               
            }
            catch (Exception ex) {
                ex.ToString();
            }
        }

    }
}
