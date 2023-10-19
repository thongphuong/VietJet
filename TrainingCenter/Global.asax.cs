using ClientMonitorLibrary.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TrainingCenter
{
    using System.Data.Entity;
    using System.Globalization;
    using System.Threading;
    using Autofac;
    using Autofac.Integration.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Services;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels.UserModels;
    using Newtonsoft.Json;
    using TrainingCenter.Scheduler;
    using Ninject;
    using Quartz;
    using Quartz.Impl;
    using Autofac.Extras.Quartz;
    using System.Collections.Specialized;
    using System.IO;
    using System.IO.Compression;  

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            //sontt
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            //String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //#region [cache Sontt]
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Orientation");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Orientation_Kind_Of_Successor");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "ROLE");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "ROLEMENU");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Room");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Subject");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Instructor_Ability");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Subject_Score");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "SubjectDetail");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Detail_Instructor");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Title_Standard");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_Approval_Type");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_APPROVES");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Cost");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_APPROVES_HISTORY");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_APPROVES_LOG");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "MenuName");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_CONTRACTS");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Instructor_Ability_LOG");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_Course_Member");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "LMS_Assign");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Trainee");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Trainee_Contract");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Trainee_Record");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "JobtitleHeader");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Trainee_TrainingCenter");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TraineeHistory");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "USER");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "UserPermission");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "UserRole");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Approve_Status");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_CONTRACTOR");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_GROUPCOST");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_CONTRACTS_STATUS");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_CONTRACTS_TYPE");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_COSTS");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_MAIL");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_GROUPSUBJECT");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_GROUPSUBJECT_ITEM");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_CERTIFICATE");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_NATIONALITY");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_PARTNER");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "JobTitle");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_PROVINCES");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_UNITS");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_USER_GENDER");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_USER_TYPE");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Company");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CONFIG");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TMS_SentEmail");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Result");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "CAT_CERTIFICATE_ITEM");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Detail_Score");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Orientation_Item");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Detail_Subject_Note");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "ROLE2");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_LMS_STATUS");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TraineeHistory_Item");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Rec_Company");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Rec_Account");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Result_Final");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Rec_Company_Account");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Result_Summary");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TraineeFuture");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_TrainingCenter");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Rec_Level");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Type");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "TraineeFuture_Item");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Type_Result");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Rec_AdvertisingPartner");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Department");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Attendance");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Function");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupFunction");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupPermissionFunction");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupUser");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "AspNet_SqlCacheTablesForChangeNotification");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupUserAccess");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupTrainee_Item");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupUserMenu");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupUserPermission");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "GroupTrainee");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "SYS_LogEvent");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "JobtitleLevel");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "JobtitlePosition");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "LMS_REQUEST");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Course_Detail");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "MENU");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Nation");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Notification");
            //System.Web.Caching.SqlCacheDependencyAdmin.EnableTableForNotifications(connectionString, "Notification_Detail");
            //#endregion
            #region Autofac
            var builder = new ContainerBuilder();

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            // OPTIONAL: Enable action method parameter injection (RARE).
            //builder.InjectActionInvoker();
            builder.RegisterType<EFDbContext>().InstancePerRequest();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));


            //RegisterScheduler(builder);


            var assembly = typeof(BaseService);

            builder.RegisterAssemblyTypes(assembly.Assembly).InNamespaceOf<BaseService>()
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces();
            builder.RegisterType<UserContext>().As<IUserContext>();
            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
           
            #endregion        
            //JobScheduler.Start();
        }


        //Added by Haneef
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            string culture = "en";
            HttpCookie cookie = HttpContext.Current.Request.Cookies["language"];

            if (cookie != null)
            {
                culture = "en";// cookie.Value;
            }
            else
            {
                HttpCookie language = new HttpCookie("language");
                language.Value = culture;
                language.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(language);
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator = ".";
            //customCulture.NumberFormat.NumberDecimalSeparator = ".";
            //if (cookie != null && cookie.Value != null)
            //{
            //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cookie.Value);
            //    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cookie.Value);
            //    //culture = cookie.Value;
            //}
            //else
            //{

            //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("En");
            //    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("En");
            //}
            //if (cookie != null && cookie.Value == "en")
            //{
            //    CultureInfo newCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            //    newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            //    newCulture.DateTimeFormat.DateSeparator = "/";
            //    Thread.CurrentThread.CurrentCulture = newCulture;
            //}           
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            string acceptEncoding = app.Request.Headers["Accept-Encoding"];
            Stream prevUncompressedStream = app.Response.Filter;

            if (app.Context.CurrentHandler == null)
                return;

            if (!(app.Context.CurrentHandler is System.Web.UI.Page ||
                app.Context.CurrentHandler.GetType().Name == "SyncSessionlessHandler") ||
                app.Request["HTTP_X_MICROSOFTAJAX"] != null)
                return;

            if (acceptEncoding == null || acceptEncoding.Length == 0)
                return;

            acceptEncoding = acceptEncoding.ToLower();
            if (acceptEncoding.Contains("gzip"))
            {
                // gzip
                app.Response.Filter = new GZipStream(prevUncompressedStream,
                    CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "gzip");
            }
            else if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
            {
                // deflate
                app.Response.Filter = new DeflateStream(prevUncompressedStream,
                    CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "deflate");
            }
        }
        protected void Application_End()
        {
            SqlDependency.Stop(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
        void Session_Start(object sender, EventArgs e)
        {
            try
            {
                //WebServer monitor = WebServer.GetInstance();
                //monitor.Do_Session_Start();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }
        void Session_End(object sender, EventArgs e)
        {
            try
            {
               // WebServer monitor = WebServer.GetInstance();
                //monitor.Do_Session_End();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }



    }

}