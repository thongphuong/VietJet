using ClientMonitorLibrary.Utilities;
using ClientMonitorLibrary.VietjetAPI_AppView;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace ClientMonitorLibrary
{
    public class Application
    {
        private static readonly Application _instance = new Application();
        string ComputerName;
        string AppName;
        private Application()
        {
            
        }


        public static Application GetInstance(string ComputerName, string AppName)
        {
            _instance.ComputerName = ComputerName;
            _instance.AppName = AppName;
            return _instance;
        }
        public static Application GetInstance()
        {           
            return _instance;
        }

        public void HeartBeat()
        {
            try
            {
                AppViewClient client = new AppViewClient();
                var result = client.HeartBeat(ComputerName, AppName);
                if (result.ResultCode == "Success")
                {
                    foreach (var item in result.Order)
                    {
                        DoOrder(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void DoOrder(OrderCode order)
        {
            if (order == OrderCode.Capture_Screen)
            {
                try
                {
                    var capture = new ScreenCapture();
                    var imgStream = capture.CaptureScreenToStream();
                    CookieContainer cookies = new CookieContainer();
                    //add or use cookies
                    NameValueCollection querystring = new NameValueCollection();
                    querystring.Add("ComputerName", ComputerName);
                    querystring.Add("AppName", AppName);
                    string outdata = WebClientExtention.UploadStream(imgStream, "http://api.vietjetair.com/UrlService/UploadCaptureImage", "fileUp", "image/pjpeg", querystring, cookies);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }
    }
}
