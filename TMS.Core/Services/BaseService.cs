using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services
{
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using Newtonsoft.Json;
    using System.Web;
    using System.Web.Script.Serialization;
    using TMS.Core.Services.Configs;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.UserModels;

    public class BaseServiceBase
    {
    }

    public class BaseService : BaseServiceBase, IBaseService
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IRepository<Course> RepoCourse;
        protected readonly IRepository<SYS_LogEvent> _repoSYS_LogEvent;
        public BaseService(IUnitOfWork unitOfWork, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent)
        {
            Uow = unitOfWork;
            RepoCourse = repoCourse;
            _repoSYS_LogEvent = repoSYS_LogEvent;
        }

        public BaseService()
        {
        }
       
        public void Dispose()
        {
            Uow.Dispose();
        }


        public void LogEvent(UtilConstants.LogType LogType, UtilConstants.LogSourse logSourse,
            UtilConstants.LogEvent LogEvent, object content)
        {
            var _content = JsonConvert.SerializeObject(content);

            //var _content = "đang test";
            var _ip = HttpContext.Current.Request.UserHostAddress;
            var _rawurl = HttpContext.Current.Request.RawUrl;
            var _fullurl = HttpContext.Current.Request.Url.ToString();
            var _servername = System.Environment.MachineName;
            var _clientinfo = HttpContext.Current.Request.UserAgent;
            var entity = new SYS_LogEvent();
            entity.LogType = (int)LogType;
            entity.LogEvent = (int)LogEvent;
            entity.Source = logSourse.ToString();
            entity.UserName = GetUser().USER_ID;
            entity.IP = _ip;
            entity.Content = _content;
            //entity.PageID
            entity.RawUrl = _rawurl;
            entity.FullUrl = _fullurl;
            entity.ServerName = _servername;
            entity.ClientInfo = _clientinfo;
            entity.CreateDay = DateTime.Now;
            entity.IsDeleted = false;
            _repoSYS_LogEvent.Insert(entity);
            Uow.SaveChanges();
        }

        protected UserModel GetUser()
        {
            var userModel = System.Web.HttpContext.Current.Session?["UserA"];

            return (UserModel)userModel;
        }
    }
}
