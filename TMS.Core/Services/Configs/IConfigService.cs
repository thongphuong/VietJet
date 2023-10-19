using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DAL.Entities;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Schedule;
using TMS.Core.ViewModels.Survey;

namespace TMS.Core.Services.Configs
{
    using TMS.Core.ViewModels.ViewModel.RoleMenus;
    using TMS.Core.ViewModels.Mail;
    using TMS.Core.ViewModels.Certificate;

    public interface IConfigService : IBaseService
    {
        CONFIG GetById(int? id = null);
        MENU GetMenuById(int? id = null);
        MenuName GetMenuNameById(int? id = null);
        CAT_MAIL GetMailById(int? id = null);
        IQueryable<CAT_MAIL_KEY> GetAllCAT_MAIL_KEYs();
        Survey GetSurveyById(int? id = null);
        CAT_CERTIFICATE GetCertificateById(int? id = null);
        Nation GetNationByCode(string code);
        CAT_PROVINCES GetProvinceByName(string name);
        IQueryable<CONFIG> Get(Expression<Func<CONFIG, bool>> query = null);
        IQueryable<Nation> GetNation(Expression<Func<Nation, bool>> query = null);
        //IQueryable<CAT_MAIL> GetCAT_MAIL(Expression<Func<CAT_MAIL, bool>> query = null);
        IQueryable<CONFIG> Get();
        IQueryable<CAT_MAIL> GetMail();
        IQueryable<CAT_MAIL> GetMail(Expression<Func<CAT_MAIL, bool>> query = null);
        IQueryable<TMS_SentEmail> GetMemberMail();
        IQueryable<TMS_SentEmail> GetMemberMail(Expression<Func<TMS_SentEmail, bool>> query = null);
        TMS_SentEmail GetSendMailById(int? id = null);
        void ModifyTMS_SentEmail(SendMailViewModels model);
        void UpdateStatusTMS_SentEmail(TMS_SentEmail entity);
      
        string GetByKey(string key);
        IQueryable<ROLEMENU> GetAllowMenu(int roleId);
        IQueryable<GroupFunction> GetGroupFunctions(Expression<Func<GroupFunction,bool>> query = null);
        IQueryable<MENU> GetMenu(Expression<Func<MENU, bool>> query = null);
        IQueryable<MENU> GetMenu_Recruitment(Expression<Func<MENU, bool>> query = null);
        IQueryable<Function> GetMenuFunctions(Expression<Func<Function, bool>> query = null);
        Dictionary<int, string> GetProvince(Expression<Func<CAT_PROVINCES, bool>> query = null);
        IQueryable<Nation> GetNations(Expression<Func<Nation, bool>> query = null);
        void DeleteRoleMenu(IEnumerable<ROLEMENU> entities);
        void InsertRoleMenu(ROLEMENU entity);
        void UpdateConfig(CONFIG entity);
        void UpdateGroupFunction(GroupFunctionViewModel model);
        //void UpdateMenu(MenuViewModel entity);
        //void UpdateConfig(IEnumerable<int> configIds);

        Nation GetNationById(int? id);
        Payment_Status GetPaymentStatusById(int? id);
        void UpdateNation(Nation entity);
        void Insertnation(Nation entity);
        void UpdateMail(MailViewModels model);
        void InsertMail(MailViewModels model);

        void InsertPayments(Payment entity);
        void UpdatePayments(Payment entity);

        IQueryable<CAT_CERTIFICATE> GetCertificate(Expression<Func<CAT_CERTIFICATE, bool>> query = null);
        void UpdateCertificate(CertificateViewModels model);
        void UpdateCertificate(CAT_CERTIFICATE entity);
        void InsertCertificate(CertificateViewModels model);
        void InsertFunctions(ICollection<Function> entity);

        int InsertMenu(MenuViewModel model, UtilConstants.MenuType menuType);
        void UpdateMenu(MenuViewModel model);
        void DeleteMenu(int? id);
        void DeleteMailTemplate(CAT_MAIL model);
        void LogEvent(UtilConstants.LogType logType,
            UtilConstants.LogEvent logEvent, string logSourse, object content);

		void ModifyFunction(GroupFunctionViewModel model);
		 IQueryable<Function> GetAllFunctions(Expression<Func<Function, bool>> query = null);
        Function GetFunctionById(int? id = null);
        IQueryable<SYS_LogEvent> GetSys_Log(Expression<Func<SYS_LogEvent, bool>> query = null);
        SYS_LogEvent GetLogById(int? id);
        void UpdateLog(SYS_LogEvent entity);
        bool Delete(int? type);
        #region [------Schedule-------]
        IQueryable<Schedule> GetAllSchedule(Expression<Func<Schedule, bool>> query = null);
        Schedule GetScheduleById(int? id);
        Schedule GetScheduleByKey(int? key);
        Schedule Modify(ScheduleModify model);
        void UpdateSchedule(Schedule entity);

        #endregion
        IQueryable<Survey> GetSurvey(Expression<Func<Survey, bool>> query = null);
        IQueryable<Survey> GetSurvey_API(Expression<Func<Survey, bool>> query = null, bool isApi = false);
        void UpdateSurvey(SurveyModels models);
        void UpdateSurvey(Survey entity);
        void InsertSurvey(SurveyModels models);
    }
}
