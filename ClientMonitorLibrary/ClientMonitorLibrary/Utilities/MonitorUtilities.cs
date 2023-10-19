using ClientMonitorLibrary.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientMonitorLibrary
{
    public static class MonitorUtilities
    {
        public static void Send_Query_Data_Message(String Message, String ActionUser)
        {
            try
            {
                WebServer monitor = WebServer.GetInstance();
                monitor.SendActivitive(Message, "Action by: " + ActionUser, 0);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }

        public static void Send_Modify_Data_Message(String Message, String ActionUser)
        {
            try
            {
                WebServer monitor = WebServer.GetInstance();
                monitor.SendActivitive(Message, "Action by: " + ActionUser, 1);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }

        public static void Send_System_Message(String Message, String Description)
        {
            try
            {
                WebServer monitor = WebServer.GetInstance();
                monitor.SendActivitive(Message, Description, 2);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }
    }
}
