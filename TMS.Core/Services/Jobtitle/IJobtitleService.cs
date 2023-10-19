using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace TMS.Core.Services.Jobtitle
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Jobtitles;

    public interface IJobtitleService : IBaseService
    {
        JobTitle GetById(int id);
        JobtitleHeader GetByIdHeader(int id);
        JobtitlePosition GetByIdPosition(int id);
        IQueryable<JobTitle> Get(bool isView = false);
        IQueryable<JobtitleLevel> GetJobLevel(bool? isView = false);
        IQueryable<JobtitlePosition> GetJobPosition(bool? isView = false);
        IQueryable<JobtitlePosition> GetJobPosition(Expression<Func<JobtitlePosition, bool>> query, bool isView = false);
        IQueryable<JobtitleHeader> GetJobHeader(bool? isView = false);
        IQueryable<JobtitleHeader> GetJobHeader(Expression<Func<JobtitleHeader, bool>> query, bool isView = false);
        IQueryable<JobTitle> Get(Expression<Func<JobTitle, bool>> query, bool isView = false,bool isApi = false);
        int ModifyJobHeader(JobtitleHeaderViewModel model);
        int ModifyJobPosition(JobtitlePositionViewModel model  );
        void Update(JobTitle entity);
        void Insert(JobTitle entity);
        void Update(JobtitleHeader entity);
        void Insert(JobtitleHeader entity);
        void Update(JobtitlePosition entity);
        void Insert(JobtitlePosition entity);
        #region Title standard
        Title_Standard GetTitleStandardById(int id);
        IQueryable<Title_Standard> GetTitleStandard();
        IQueryable<Title_Standard> GetTitleStandard(Expression<Func<Title_Standard, bool>> query);
        void Modify(JobtitleModifyViewModel model);
        void UpdateTitleStandard(Title_Standard entity);
        void InsertTitleStandard(Title_Standard entity);
        //remove entities
        void DeleteTitleStandard(IEnumerable<Title_Standard> entities);
        #endregion
    }
}
