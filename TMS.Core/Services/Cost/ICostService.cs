using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels.Cost;

namespace TMS.Core.Services.Cost
{
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface ICostService : IBaseService
    {
        IQueryable<SubjectDetail> GetSubjectRp();
        CAT_COSTS GetById(int? id);
        IQueryable<CAT_COSTS> Get();
        IQueryable<CAT_UNITS> GetUnits();
        IQueryable<CAT_COSTS> Get(Expression<Func<CAT_COSTS, bool>> query);
        IQueryable<CAT_GROUPCOST> GetGroupCost();
        CAT_GROUPCOST GetGroupcostById(int? id);
        IQueryable<CAT_GROUPCOST> GetGroupCost(Expression<Func<CAT_GROUPCOST, bool>> query);
        CAT_GROUPCOST Modify(PartialGroupCostViewModify model);
        void Update(CAT_GROUPCOST entity);
        void Modify(CostModel model);
        void Update(CAT_COSTS entity);
        void Insert(CAT_COSTS entity);
        void Delete(CAT_COSTS entity);
        void Insert(Course_Cost entity, int type);
        void Update(Course_Cost entity);

        Course_Cost GetCourseCostById(int? id);
        IQueryable<Course_Cost> GetCourseCost();
        IQueryable<Course_Cost> GetCourseCost(Expression<Func<Course_Cost, bool>> query);

        List<sp_GetCostHeader_Result> sp_GetCostHeader(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd);
        List<sp_GetCostDetail_Result> sp_GetCostDetail(string listCourseID, DateTime? fromDateStart, DateTime? fromDateEnd,
            DateTime? toDateStart, DateTime? toDateOfEnd);
    }
}
