using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Configs;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.ViewModel.RoleMenus;
using TMS.Core.ViewModels.Common;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TrainingCenter.Controllers
{
    public class TestServicesController : Controller
    {
        private IConfigService configService;

        public TestServicesController(IConfigService ConfigService)
        {
            configService = ConfigService;
        }

        // GET: TestServices
        public ActionResult Index(string type)
        {
            var list = new List<object>();

           
            foreach (string key in Session.Keys)
            {

                list.Add(Session[key]);
                //Response.Write(key + " - " + Session[key] + "<br />");
            }   
            var temp = JsonConvert.SerializeObject(list);
            ViewBag.ListTemp = temp;
            SaveBigData();
            ReadData();
            return View(list);
        }

        [HttpPost]
        public ActionResult LoadFunction(int? id)
        {
            var model = configService.GetFunctionById(id);
            return Json(new
            {
                result = model != null,
                Id = model?.Id,
                Name = model?.Name,
                UrlName = model?.UrlAddress,
                ActionType= model?.ActionType
            });
        }
        [HttpPost]
        public ActionResult Modify(GroupFunctionViewModel model)
        {
          
            try
            {
                configService.ModifyFunction(model);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
          
        }
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            var url = string.IsNullOrEmpty(Request.QueryString["furl"]) ? string.Empty : Request.QueryString["furl"].ToLower().Trim(); 
            var data = configService.GetAllFunctions(a => a.UrlAddress.Contains(url)).OrderByDescending(b => b.Id);


            IEnumerable<Function> filtered = data;

            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<Function, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name
                                                      : sortColumnIndex == 2 ? c.UrlAddress
                                                      : sortColumnIndex == 3 ? c.ActionType.ToString()
                                                      : c.Name);


            var sortDirection = Request["sSortDir_0"]; // asc or desc

            if (sortDirection == null)
            {
                sortDirection = "asc";
            }
            filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                              : filtered.OrderByDescending(orderingFunction);

            var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayed.ToArray()
                         select new object[]
                         {
                    string.Empty,
                    c?.Name,
                    c?.UrlAddress,
                    "<a href='javascript:void(0)' onclick='modifyfunction("+c?.Id+")' class='btn btn-danger' title='Modify'><i class='fa fa-pencil'></i> </a>"
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filtered.Count(),
                iTotalDisplayRecords = filtered.Count(),
                aaData = result
            },
          JsonRequestBehavior.AllowGet);
        }

        public bool CallServices(string type)
        {
            var server = ConfigurationManager.AppSettings["SERVER"]; // địa chỉ
            var token = ConfigurationManager.AppSettings["TOKEN"];
            var function = ConfigurationManager.AppSettings["FUNCTION"]; // tên function
            var moodlewsrestformat = ConfigurationManager.AppSettings["moodlewsrestformat"]; // value : json
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("type", type);
            var response = restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var str_response = response.Content;
                if (str_response.Contains("exception") || str_response.Contains("warnings"))
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }  
        }
        [AllowAnonymous]
        public ActionResult Upload()
        {
            ViewBag.Message = "Upload new videos here";

            string new_video_title = "VIDEO_TITLE";          // This should be obtained from DB
            string api_secret = "zcrBQL4HAemP7nH4hUZzPVpunIps5HMmo0MtldPZveURRjFObOq1oE4mw5vNvAjw";
            string uri = "https://api.vdocipher.com/v2/uploadPolicy/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                writer.Write("clientSecretKey=" + api_secret + "&title=" + new_video_title);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            dynamic otp_data;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string json_otp = reader.ReadToEnd();
                otp_data = JObject.Parse(json_otp);
            }
            ViewBag.upload_data = otp_data;
            return View();
        }

        public void ReadData()
        {
            var cache = MyRedisConnectorHelper.Connection.GetDatabase();
            var devicesCount = 10000;
            var texxt = "";
            for (int i = 0; i < devicesCount; i++)
            {
                var value = cache.StringGet($"Device_Status:{i}");
                texxt += value + Environment.NewLine;
                //Console.WriteLine($"Valor={value}");
            }
        }
        public void SaveBigData()
        {
            var devicesCount = 10000;
            var rnd = new Random();
            var cache = MyRedisConnectorHelper.Connection.GetDatabase();

            for (int i = 1; i < devicesCount; i++)
            {
                var value = rnd.Next(0, 10000);
                cache.StringSet($"Device_Status:{i}", value);
            }
        }

    }
}