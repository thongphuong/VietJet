using System;
using System.Collections.Generic;
using System.Linq;
using TMS.Core.App_GlobalResources;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Schedule;
using TMS.Core.ViewModels.Survey;

namespace TMS.Core.Services.Configs
{
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels.ViewModel.RoleMenus;
    using TMS.Core.ViewModels.Mail;
    using Newtonsoft.Json;
    using System.Web;
    using TMS.Core.ViewModels.Certificate;
    using TMS.Core.ViewModels.UserModels;

    public class ConfigService : BaseService, IConfigService
    {

        private readonly IRepository<CONFIG> _repoConfig;
        private readonly IRepository<ROLEMENU> _repoRolemenu;
        private readonly IRepository<MENU> _repoMenu;
        private readonly IRepository<MenuName> _repoMenuName;
        private readonly IRepository<Nation> _repoNation;
        private readonly IRepository<CAT_PROVINCES> _repoProvince;
        private readonly IRepository<Function> _repoFunction;
        private readonly IRepository<GroupFunction> _repoGroupFunction;
        private readonly IRepository<GroupPermissionFunction> _repoGroupPermissionFunction;
        private readonly IRepository<SYS_LogEvent> _repoSYS_LogEvent;
        private readonly IRepository<CAT_MAIL> _repoMail;
        private readonly IRepository<TMS_SentEmail> _repoSentMail;
        private readonly IRepository<CAT_MAIL_KEY> _repoCat_Mail_Key;
        private readonly IRepository<CAT_CERTIFICATE> _repoCertificate;

        private readonly IRepository<PROCESS_Steps> _repoProcessSteps;
        private readonly IRepository<PROCESS_Approver> _repoApprover;
        private readonly IRepository<Schedule> _repoSchedule;
        private readonly IRepository<Schedules_Method> _repoScheduleMethod;
        private readonly IRepository<Schedules_Type> _repoScheduleType;
        private readonly IRepository<Schedules_destination> _repoScheduleDestination;
        private readonly IRepository<Payment> _repoPayment;
        private readonly IRepository<Payment_Status> _repoPaymentStatus;
        private UserModel _currentUser = null;
        private readonly IRepository<Survey> _repoSurvey;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        private const string Prefix = "M";
        public ConfigService(IUnitOfWork unitOfWork, IRepository<PROCESS_Approver> repoApprover, IRepository<PROCESS_Steps> repoProcessSteps, IRepository<CONFIG> repoConfig, IRepository<ROLEMENU> repoRolemenu, IRepository<MENU> repoMenu, IRepository<CAT_PROVINCES> repoProvince
            , IRepository<Nation> repoNation, IRepository<Function> repoFunction, IRepository<GroupFunction> repoGroupFunction, IRepository<GroupPermissionFunction> repoGroupPermissionFunction, IRepository<Course> repoCourse, IRepository<MenuName> repoMenuName, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<CAT_MAIL> repoMail, IRepository<CAT_CERTIFICATE> repoCertificate, IRepository<TMS_SentEmail> repoSentMail, IRepository<Schedule> repoSchedule, IRepository<Schedules_Method> repoScheduleMethod, IRepository<Schedules_Type> repoScheduleType, IRepository<Schedules_destination> repoScheduleDestination, IRepository<Survey> repoSurvey, IRepository<CAT_MAIL_KEY> repoCat_Mail_Key, IRepository<Payment> repoPayment, IRepository<Payment_Status> repoPaymentStatus) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoConfig = repoConfig;
            _repoRolemenu = repoRolemenu;
            _repoMenu = repoMenu;
            _repoNation = repoNation;
            _repoFunction = repoFunction;
            _repoProvince = repoProvince;
            _repoGroupFunction = repoGroupFunction;
            _repoGroupPermissionFunction = repoGroupPermissionFunction;
            _repoMenuName = repoMenuName;
            _repoSYS_LogEvent = repoSYS_LogEvent;
            _repoMail = repoMail;
            _repoCertificate = repoCertificate;
            _repoProcessSteps = repoProcessSteps;
            _repoApprover = repoApprover;
            _repoSentMail = repoSentMail;
            _repoSchedule = repoSchedule;
            _repoScheduleMethod = repoScheduleMethod;
            _repoScheduleType = repoScheduleType;
            _repoScheduleDestination = repoScheduleDestination;
            _repoSurvey = repoSurvey;
            _repoCat_Mail_Key = repoCat_Mail_Key;
            _repoPayment = repoPayment;
            _repoPaymentStatus = repoPaymentStatus;
        }

        //Company
        public IQueryable<Nation> GetNation(Expression<Func<Nation, bool>> query = null)
        {
            var entities = _repoNation.GetAll(a => a.bit_Deleted==false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(m => m.Nation_Name);
        }
        /////////

        public CONFIG GetById(int? id = null)
        {
            return !id.HasValue ? null : _repoConfig.Get(id.Value);
        }

        public MENU GetMenuById(int? id = null)
        {
            return !id.HasValue ? null : _repoMenu.Get(id.Value);
        }
        public MenuName GetMenuNameById(int? id = null)
        {
            //return !id.HasValue ? null : _repoMenuName.Get(id.Value);
            return _repoMenuName.Get(a => a.MenuId == id);
        }
        public CAT_MAIL GetMailById(int? id = null)
        {
            //return !id.HasValue ? null : _repoMenuName.Get(id.Value);
            return _repoMail.Get(a => a.ID == id);
        }
        
        public IQueryable<CONFIG> Get(Expression<Func<CONFIG, bool>> query = null)
        {
            var entities = _repoConfig.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(m => m.ID);
        }
        public IQueryable<Function> GetMenuFunctions(Expression<Func<Function, bool>> query = null)
        {
            return query == null ? _repoFunction.GetAll() : _repoFunction.GetAll(query);
        }

        public Dictionary<int, string> GetProvince(Expression<Func<CAT_PROVINCES, bool>> query = null)
        {
            return query == null ? _repoProvince.GetAll().OrderBy(a => a.name).ToDictionary(a => a.id, a => a.name) : _repoProvince.GetAll(query).OrderBy(a => a.name).ToDictionary(a => a.id, a => a.name);
        }

        public string GetByKey(string key)
        {
            var entity = _repoConfig.Get(a => a.KEY.Equals(key));
            return entity != null ? entity.VALUE : "";
        }

        public IQueryable<ROLEMENU> GetAllowMenu(int roleId)
        {
            return _repoRolemenu.GetAll(a => a.ROLE_ID == roleId);
        }

        public IQueryable<GroupFunction> GetGroupFunctions(Expression<Func<GroupFunction, bool>> query = null)
        {
            var entities = _repoGroupFunction.GetAll(a => a.IsActive==true);
            return query == null ? entities : entities.Where(query);
        }

        public IQueryable<MENU> GetMenu(Expression<Func<MENU, bool>> query = null)
        {
            var entities = _repoMenu.GetAll(a => a.ISACTIVE == 1 && (a.Type == (int)UtilConstants.MenuType.System || a.Type == (int)UtilConstants.MenuType.TraningCenter));
            //var entities = _repoMenu.GetAll(a => a.ISACTIVE == 1 );
            if (query != null) entities = entities.Where(query);
            return entities.OrderBy(a => a.Ancestor);
        }
        public IQueryable<MENU> GetMenu_Recruitment(Expression<Func<MENU, bool>> query = null)
        {
            var entities = _repoMenu.GetAll(a => a.ISACTIVE == 1 && (a.Type == (int)UtilConstants.MenuType.System || a.Type == (int)UtilConstants.MenuType.Recruitment));
            if (query != null) entities = entities.Where(query);
            return entities.OrderBy(a => a.Ancestor);
        }

        public void DeleteRoleMenu(IEnumerable<ROLEMENU> entities)
        {
            _repoRolemenu.Delete(entities);
            Uow.SaveChanges();
        }

        public void InsertRoleMenu(ROLEMENU entity)
        {
            _repoRolemenu.Insert(entity);
            Uow.SaveChanges();
        }
        public IQueryable<Nation> GetNations(Expression<Func<Nation, bool>> query = null)
        {
            var entities = _repoNation.GetAll(a => a.isactive == true && a.bit_Deleted == false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(a => a.Nation_Name);
        }
        public Nation GetNationByCode(string code)
        {
            return _repoNation.Get(a => (string.IsNullOrEmpty(code.Trim().ToLower()) || a.Nation_Name.Contains(code.Trim().ToLower())));
        }
        public CAT_PROVINCES GetProvinceByName(string name)
        {
            return _repoProvince.Get(a => (string.IsNullOrEmpty(name.Trim().ToLower()) || a.name.Contains(name.Trim().ToLower())));
        }
        public void UpdateConfig(CONFIG entity)
        {
            _repoConfig.Update(entity);
            Uow.SaveChanges();
        }

        public void ModifyTMS_SentEmail(SendMailViewModels model)
        {
            var entity = _repoSentMail.Get(a => a.Id == model.Id);
            if (entity == null) throw new Exception(Messege.NO_DATA);
            entity.mail_receiver = model.Email;
            entity.subjectname = model.Subject;
            entity.content_body = model.TemplateMail;
            _repoSentMail.Update(entity);
            Uow.SaveChanges();
        }
        public void UpdateGroupFunction(GroupFunctionViewModel model)
        {
            var entity = _repoGroupFunction.Get(a => a.Id == model.Id && a.IsActive==true);
            if (entity == null) throw new Exception(Messege.NO_DATA);
            entity.IsActive = model.IsActive;
            entity.Name = model.Name;
            entity.DefaultUrl = model.DefaultUrl;
            var referenceFunctions = entity.GroupPermissionFunctions;
            _repoGroupPermissionFunction.Delete(referenceFunctions);
            entity.GroupPermissionFunctions.Clear();
            if (model.CurrentFunctions != null)
            {
                foreach (var currentFunction in model.CurrentFunctions)
                {
                    entity.GroupPermissionFunctions.Add(new GroupPermissionFunction()
                    {
                        FunctionId = currentFunction
                    });
                }
            }
            _repoGroupFunction.Update(entity);
            Uow.SaveChanges();
        }

        public Nation GetNationById(int? id)
        {
            return id.HasValue ? _repoNation.Get(id) : null;
        }
        public Payment_Status GetPaymentStatusById(int? id)
        {
            return id.HasValue ? _repoPaymentStatus.Get(id) : null;
        }
        
        public void UpdateNation(Nation entity)
        {
            _repoNation.Update(entity);
            Uow.SaveChanges();
        }

        public void Insertnation(Nation entity)
        {
            _repoNation.Insert(entity);
            Uow.SaveChanges();
        }
        public void InsertPayments(Payment entity)
        {
            _repoPayment.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdatePayments(Payment entity)
        {
            _repoPayment.Update(entity);
            Uow.SaveChanges();
        }

        public void UpdateMail(MailViewModels model)
        {
            var entity = _repoMail.Get(a => a.ID == model.Id);
            if (entity == null) throw new Exception("Template Mail is not found");

            entity.UpdateAt = DateTime.Now;
            entity.Name = model.Name;
            entity.IsActive = true;
            entity.IsDelete = false;
            entity.Content = model.TemplateMail;
            entity.SubjectMail = model.Subject_Mail;
            entity.Code = model.Code;
            entity.UpdateBy = CurrentUser.USER_ID;
            _repoMail.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertMail(MailViewModels model)
        {
            if (model == null) throw new Exception("No have data to insert");
            var entity = new CAT_MAIL()
            {
                Name = model.Name,             
                IsActive = true,
                IsDelete = false,
                Content = model.TemplateMail,
                SubjectMail = model.Subject_Mail,
                Code = model.Code,
                CreateAt = DateTime.Now,
                CreateBy = CurrentUser.USER_ID,
            };
            _repoMail.Insert(entity);
            Uow.SaveChanges();
        }


        #region [---------------CAT_CERTIFICATE-----------------------]
        public CAT_CERTIFICATE GetCertificateById(int? id = null)
        {
            return _repoCertificate.Get(a => a.ID == id);
        }

        public void UpdateCertificate(CertificateViewModels model)
        {
            var entity = _repoCertificate.Get(a => a.ID == model.Id);
            if (entity == null) throw new Exception(
"Template Mail is not found");

            entity.UpdateAt = DateTime.Now;
            entity.Name = model.Name;
            entity.IsActive = true;
            entity.IsDelete = false;
            entity.Content = model.Template;
            entity.UpdateBy = CurrentUser.USER_ID;
            entity.Type = model.TypeCertificate;
            _repoCertificate.Update(entity);
            Uow.SaveChanges();
        }
        public void UpdateCertificate(CAT_CERTIFICATE entity)
        {
            _repoCertificate.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertCertificate(CertificateViewModels model)
        {
            if (model == null) throw new Exception(
Messege.NO_DATA);
            var entity = new CAT_CERTIFICATE()
            {
                Name = model.Name,
                IsActive = true,
                IsDelete = false,
                Content = model.Template,
                CreateAt = DateTime.Now,
                CreateBy = CurrentUser.USER_ID,
                Type = model.TypeCertificate,
            };
            _repoCertificate.Insert(entity);
            Uow.SaveChanges();
        }

        public IQueryable<CAT_CERTIFICATE> GetCertificate(Expression<Func<CAT_CERTIFICATE, bool>> query = null)
        {
            var entities = _repoCertificate.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(a => a.Name);
        }
        #endregion


        public void InsertFunctions(ICollection<Function> entities)
        {
            _repoFunction.Insert(entities);
            Uow.SaveChanges();
        }

        public int InsertMenu(MenuViewModel model, UtilConstants.MenuType menuType)
        {
            var entity = new MENU()
            {
                CREATION_DATE = DateTime.Now,
                ICON = model.Icon,
                ISACTIVE = 1,
                NAME = model.MenuTitle,
                ISMENU = 1,
                TITLE = model.MenuTitle,
                CREATED_BY = model.UserId,
                PARENT_ID = model.ParentId,
                URL = model.Url,
                SHOWORDER = model.MenuIndex,
                Type = (int)menuType
                //TODO: get the lastest id + 1
            };
            var parentCode = _repoMenu.Get(model.ParentId);

            entity.Code = GenMenuCode(entity.SHOWORDER ?? 0);
            entity.Ancestor = parentCode == null ? entity.Code : parentCode.Ancestor + "_" + entity.Code;
            _repoMenu.Insert(entity);
            Uow.SaveChanges();
            return entity.ID;
        }

        private string GenMenuCode(int orderIndex, int? id = null)
        {
            var entities = _repoMenu.GetAll();
            var menuId = entities.Any() ? _repoMenu.GetAll().Select(a => a.ID).Max() + 1 + "" : "00001";
            menuId = id.HasValue ? id + "" : menuId;
            while (menuId.Length < 5)
            {
                menuId = "0" + menuId;
            }
            var indexPrefix = orderIndex + "";
            while (indexPrefix.Length < 5)
            {
                indexPrefix = "0" + indexPrefix;
            }
            return indexPrefix + Prefix + menuId;
        }
        public void UpdateMenu(MenuViewModel model)
        {
            var parentCode = _repoMenu.Get(model.ParentId);
            var entity = _repoMenu.Get(a => a.ID == model.Id);
            if (entity == null)
            {
                entity = new MENU
                {
                    CREATION_DATE = DateTime.Now,
                    ICON = model.Icon,
                    ISACTIVE = 1,
                    NAME = model.MenuTitle,
                    ISMENU = 1,
                    TITLE = model.MenuTitle,
                    CREATED_BY = model.UserId,
                    PARENT_ID = model.ParentId,
                    URL = model.Url,
                    SHOWORDER = model.MenuIndex,
                    GroupFunctionId = model.Function,
                    Type = 1
                };
                //TODO: get the lastest id + 1

                entity.Code = GenMenuCode(entity.SHOWORDER ?? 0);
                entity.Ancestor = parentCode == null ? entity.Code : parentCode.Ancestor + "_" + entity.Code;
                _repoMenu.Insert(entity);
            }
            else
            {
                entity.LAST_UPDATE_DATE = DateTime.Now;
                entity.ICON = model.Icon;
                entity.ISACTIVE = 1;
                entity.NAME = model.MenuTitle;
                entity.TITLE = model.MenuTitle;
                entity.LAST_UPDATED_BY = model.UserId;
                entity.PARENT_ID = model.ParentId;
                entity.URL = model.Url;
                entity.SHOWORDER = model.MenuIndex;
                entity.GroupFunctionId = model.Function;
                entity.ISMENU = 1;

                entity.Code = GenMenuCode(model.MenuIndex ?? 0, entity.ID);
                var newAncestor = parentCode == null ? entity.Code : parentCode.Ancestor + "_" + entity.Code;
                var children = _repoMenu.GetAll(a => a.Ancestor.StartsWith(entity.Ancestor + "_")).ToList();
                foreach (var child in children)
                {
                    child.Ancestor = child.Ancestor.Replace(entity.Ancestor, newAncestor);
                }
                _repoMenu.Update(children);
                entity.Ancestor = newAncestor;
                _repoMenu.Update(entity);
            }

            Uow.SaveChanges();
        }

        public void DeleteMenu(int? id)
        {
            var entity = _repoMenu.Get(id);
            if (entity == null) throw new Exception("data is not found");
            if (entity.MENU1.Any())
            {
                _repoMenu.Delete(entity.MENU1);
            }
            _repoMenu.Delete(entity);
            Uow.SaveChanges();
        }

        public void DeleteMailTemplate(CAT_MAIL model)
        {
            _repoMail.Update(model);
            Uow.SaveChanges();
        }

        public IQueryable<CONFIG> Get()
        {
            return _repoConfig.GetAll();
        }
        public IQueryable<CAT_MAIL> GetMail()
        {
            return _repoMail.GetAll(a => a.IsActive == true && a.IsDelete == false).OrderByDescending(a=>a.ID);
        }
        public IQueryable<TMS_SentEmail> GetMemberMail()
        {
            return _repoSentMail.GetAll();
        }
        public IQueryable<TMS_SentEmail> GetMemberMail(Expression<Func<TMS_SentEmail, bool>> query = null)
        {
           
            if (query != null)
            {
                var entities = _repoSentMail.GetAll(query).OrderByDescending(a => a.Id).Take(1000);
                return entities;
            }
            else
            {
                var entities = _repoSentMail.GetAll().OrderByDescending(a => a.Id).Take(1000);
                return entities;
            }
            
        }

        public TMS_SentEmail GetSendMailById(int? id = null)
        {
            //return !id.HasValue ? null : _repoMenuName.Get(id.Value);
            return _repoSentMail.Get(a => a.Id == id);
        }
        public IQueryable<CAT_MAIL> GetMail(Expression<Func<CAT_MAIL, bool>> query = null)
        {
            var entities = _repoMail.GetAll(a=>a.IsActive==true && a.IsDelete==false);
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderByDescending(a => a.ID);
        }

        public IQueryable<SYS_LogEvent> GetSys_Log(Expression<Func<SYS_LogEvent, bool>> query = null)
        {
            var entities = _repoSYS_LogEvent.GetAll();
            if (query != null)
            {
                entities = entities.Where(query);
            }
            return entities.OrderBy(a => a.CreateDay);
        }
        public void LogEvent(UtilConstants.LogType logType,
             UtilConstants.LogEvent logEvent, string logSourse, object content)
        {
            var _content = JsonConvert.SerializeObject(content);

            //var _content = "đang test";
            var _ip = HttpContext.Current.Request.UserHostAddress;
            var _rawurl = HttpContext.Current.Request.RawUrl;
            var _fullurl = HttpContext.Current.Request.Url.ToString();
            var _servername = System.Environment.MachineName;
            var _clientinfo = HttpContext.Current.Request.UserAgent;
            var entity = new SYS_LogEvent();
            entity.LogType = (int)logType;
            entity.LogEvent = (int)logEvent;
            entity.Source = logSourse;
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

        public Function GetFunctionById(int? id = null)
        {
            return !id.HasValue ? null : _repoFunction.Get(id.Value);
        }
        public IQueryable<Function> GetAllFunctions(Expression<Func<Function, bool>> query = null)
        {
            var entities = _repoFunction.GetAll();
            return query == null ? entities : entities.Where(query);
        }


        public void ModifyFunction(GroupFunctionViewModel model)
        {
            //insert function
            var entityFunction =
                _repoFunction.Get(a => a.Id == model.FunctionId);
            if (entityFunction != null)
            {
                entityFunction.Name = model.FunctionName;
                entityFunction.UrlAddress = model.FunctionUrlName;
                entityFunction.ActionType = model.FunctionActionType;
                _repoFunction.Update(entityFunction);
            }
            else
            {
                if (!string.IsNullOrEmpty(model.FunctionName) && !string.IsNullOrEmpty(model.FunctionUrlName))
                {
                    entityFunction = new Function
                    {
                        Name = model.FunctionName,
                        UrlAddress = model.FunctionUrlName,
                        ActionType = model.FunctionActionType,
                        ActionMethod = 0
                    };
                    _repoFunction.Insert(entityFunction);
                }

            }
            Uow.SaveChanges();

        }

        public SYS_LogEvent GetLogById(int? id)
        {
            return !id.HasValue ? null : _repoSYS_LogEvent.Get(id.Value);
        }

        public bool Delete(int? type)
        {
            if (!type.HasValue) return false;
            var listLogs = _repoSYS_LogEvent.GetAll(a => a.LogEvent == type);
            _repoSYS_LogEvent.Delete(listLogs);
            Uow.SaveChanges();
            return true;
        }
        public void UpdateLog(SYS_LogEvent entity)
        {

            _repoSYS_LogEvent.Update(entity);
            Uow.SaveChanges();
        }

        #region [----Schedule------]
        public IQueryable<Schedule> GetAllSchedule(Expression<Func<Schedule, bool>> query = null)
        {
            var entities = _repoSchedule.GetAll(a => a.IsDelete == false);// && !a.IsDefault.HasValue
            return query == null ? entities : entities.Where(query);
        }
        public Schedule GetScheduleById(int? id)
        {
            return !id.HasValue ? null : _repoSchedule.Get(a => a.id == id && a.IsDelete == false);
        }
        public Schedule GetScheduleByKey(int? key)
        {
            return !key.HasValue ? null : _repoSchedule.Get(a => a.IsDefault == key && a.IsActive == true && a.IsDelete == false);
        }
        private void ModifySchedule(ScheduleModify model, out Schedule entity)
        {
            entity = _repoSchedule.Get(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                if (model.MethodId == (int) UtilConstants.ScheduleMethod.Mail)
                {
                    if (model.TemplateId != null)
                    {
                        entity.IdTemplateMail = model.TemplateId;
                        entity.Content = null;
                    }
                    else
                    {
                        entity.IdTemplateMail = null;
                        entity.Content = model.Content;
                    }
                }
                else if(model.MethodId == (int)UtilConstants.ScheduleMethod.Notification)
                {
                    entity.IdTemplateMail = null;
                    entity.Content = model.Content;
                }
                else if (model.MethodId == (int)UtilConstants.ScheduleMethod.Sms)
                {
                    entity.IdTemplateMail = null;
                    entity.Content = model.Content;
                }

                _repoSchedule.Update(entity);

            }
            else
            {
                entity = new Schedule();
                entity.Name = model.Name;
                if (model.MethodId == (int)UtilConstants.ScheduleMethod.Mail)
                {
                    if (model.TemplateId != null)
                    {
                        entity.IdTemplateMail = model.TemplateId;
                    }
                    else
                    {
                        entity.IdTemplateMail = null;
                        entity.Content = model.Content;
                    }
                }
                else if (model.MethodId == (int)UtilConstants.ScheduleMethod.Notification)
                {
                    entity.IdTemplateMail = null;
                    entity.Content = model.Content;
                }
                else if (model.MethodId == (int)UtilConstants.ScheduleMethod.Sms)
                {
                    entity.IdTemplateMail = null;
                    entity.Content = model.Content;
                }

                entity.IsActive = true;
                entity.IsDelete = false;
                _repoSchedule.Insert(entity);
            }
            Uow.SaveChanges();
        }

        private void Add_Method_Type_Detination(ref Schedule entity, ScheduleModify model)
        {
            if (entity.Schedules_Method.Any())
            {
                _repoScheduleMethod.Delete(entity.Schedules_Method);
                entity.Schedules_Method.Clear();
            }
            entity.Schedules_Method.Add(new Schedules_Method()
            {
                IdMethod = model.MethodId
            });
            if (entity.Schedules_Type.Any())
            {
                _repoScheduleType.Delete(entity.Schedules_Type);
                entity.Schedules_Type.Clear();
            }
            var timeremind = 0;
            if (model.TimeRemind < 0)
            {
                throw new Exception(Messege.WARNING_TIMEREPEAT);
            }
            else
            {
                timeremind = model.TimeRemind;
            }
            if (model.TypeId == (int)UtilConstants.ScheduleType.Repeat)
            {
                if (model.TimeRepeat <= 0)
                {
                    throw new Exception(Messege.WARNING_TIMEREPEAT);
                }
                if (!model.TimeMarkId.HasValue)
                {
                    throw new Exception(Messege.WARNING_TIMEMARK);
                }

                var value = string.Empty;
                switch (model.TimeMarkId)
                {
                    //case (int)UtilConstants.ScheduleTimeMark.Second:
                    //    value = model.TimeRepeat.ToString();
                    //    break;
                    case (int)UtilConstants.ScheduleTimeMark.Hour:
                        value = (model.TimeRepeat * 60 * 60).ToString();
                        break;
                    case (int)UtilConstants.ScheduleTimeMark.Day:
                        value = (model.TimeRepeat * 60 * 60 * 24).ToString();
                        break;
                    case (int)UtilConstants.ScheduleTimeMark.Month:
                        value = (model.TimeRepeat * 60 * 60 * 24 * 30).ToString();
                        break;
                }

                entity.Schedules_Type.Add(new Schedules_Type()
                {
                    IdType = model.TypeId,
                    Value = value,
                    IdTimeMark = model.TimeMarkId,
                    LastAccess = DateTime.Now,
                    TimeRemind = timeremind
                });
            }
            else if (model.TypeId == (int)UtilConstants.ScheduleType.SetCalendar)
            {
                if (!model.DateCalendar.HasValue)
                {
                    throw new Exception(Messege.WARNING_DATECALENDAR);
                }
                entity.Schedules_Type.Add(new Schedules_Type()
                {
                    IdType = model.TypeId,
                    Value = model.DateCalendar.ToString(),
                });

            }
            else if (model.TypeId == (int)UtilConstants.ScheduleType.Periodic)
            {
                if (!model.DayValues.Any())
                {
                    throw new Exception(Messege.WARNING_DAY);
                }
                if (string.IsNullOrEmpty(model.TimePeriodic))
                {
                    throw new Exception(Messege.WARNING_TIMEPERIODIC);
                }
                entity.Schedules_Type.Add(new Schedules_Type()
                {
                    IdType = model.TypeId,
                    Value = string.Join(",", model.DayValues) + "|" + model.TimePeriodic

                });
            }
            if (model.Catmail_code != "SendMailReminderFinalCourse" && model.Catmail_code != "SendMailReminderCourse")
            {
                

                if (entity.Schedules_destination.Any())
                {
                    _repoScheduleDestination.Delete(entity.Schedules_destination);
                    entity.Schedules_destination.Clear();
                }

                if (model.IsAll != null && model.IsAll.Equals("on"))
                {
                    entity.Schedules_destination.Add(new Schedules_destination()
                    {
                        IsUser = model.UserTypeId == (int)UtilConstants.UserType.UserSystem,
                        IsAll = model.IsAll.Equals("on")
                    });
                }
                else
                {
                    if (model.UserTypeId == (int)UtilConstants.UserType.UserSystem)
                    {
                        if (string.IsNullOrEmpty(model.NUserId))
                        {
                            throw new Exception(Messege.WARNING_USERSYSTEM);
                        }
                        var users = model.NUserId.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var userId in users)
                        {
                            if (!string.IsNullOrEmpty(userId))
                            {
                                entity.Schedules_destination.Add(new Schedules_destination()
                                {
                                    IsUser = model.UserTypeId == (int)UtilConstants.UserType.UserSystem,
                                    IdUser = int.Parse(userId)
                                });
                                Uow.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.NEmployeeId))
                        {
                            throw new Exception(Messege.WARNING_EMPLOYEE);
                        }

                        var emps = model.NEmployeeId.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var empId in emps)
                        {
                            if (!string.IsNullOrEmpty(empId))
                            {
                                entity.Schedules_destination.Add(new Schedules_destination()
                                {
                                    IsUser = false,
                                    IdEmp = int.Parse(empId),
                                });
                                Uow.SaveChanges();
                            }

                        }
                    }
                }
            }
           
        }
        public Schedule Modify(ScheduleModify model)
        {
            Schedule entity;
            ModifySchedule(model, out entity);
            Add_Method_Type_Detination(ref entity, model);
            Uow.SaveChanges();
            return entity;
        }

        public void UpdateSchedule(Schedule entity)
        {
            _repoSchedule.Update(entity);
            Uow.SaveChanges();
        }
        #endregion
        public IQueryable<Survey> GetSurvey(Expression<Func<Survey, bool>> query = null)
        {
            var entities = _repoSurvey.GetAll(a => a.Is_Deleted == false);
            //var entities = _repoMenu.GetAll(a => a.ISACTIVE == 1 );
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public IQueryable<Survey> GetSurvey_API(Expression<Func<Survey, bool>> query = null, bool isApi = false)
        {
            var entities = isApi ? _repoSurvey.GetAll() : _repoSurvey.GetAll(a => a.Is_Deleted == false);
            //var entities = _repoMenu.GetAll(a => a.ISACTIVE == 1 );
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public Survey GetSurveyById(int? id = null)
        {
            //return !id.HasValue ? null : _repoMenuName.Get(id.Value);
            return _repoSurvey.Get(a => a.Id == id);
        }

        public void InsertSurvey(SurveyModels model)
        {          
            var entity = new Survey()
            {
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                OpenDate = model.StartDate,
                CloseDate = model.EndDate,
                Created_At = DateTime.Now,
                Created_By = CurrentUser.USER_ID,
                Is_Active = true,
                Is_Deleted = false,
                Resp_View = 0,
                LMSStatus = 1,
            };
            _repoSurvey.Insert(entity);
            Uow.SaveChanges();
        }
        public void UpdateSurvey(SurveyModels model)
        {
            var entity = _repoSurvey.Get(a => a.Id == model.Id);
            if (entity == null) throw new Exception(
"Template Mail is not found");

            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.OpenDate = model.StartDate;
            entity.CloseDate = model.EndDate;
            entity.Modify_At = DateTime.Now;
            entity.Modify_By = CurrentUser.USER_ID;
            entity.Is_Active = model.IsActive;
            entity.Is_Deleted = false;
            entity.Resp_View = 0;
            entity.LMSStatus = 1;
            _repoSurvey.Update(entity);
            Uow.SaveChanges();                      
        }
        public void UpdateSurvey(Survey entity)
        {
            _repoSurvey.Update(entity);
            Uow.SaveChanges();
        }

        public void UpdateStatusTMS_SentEmail(TMS_SentEmail entity)
        {
            _repoSentMail.Update(entity);
            Uow.SaveChanges();
        }

        public IQueryable<CAT_MAIL_KEY> GetAllCAT_MAIL_KEYs()
        {
           return _repoCat_Mail_Key.GetAll();
        }
    }
}
