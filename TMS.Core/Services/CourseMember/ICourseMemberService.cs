using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.CourseMember
{
    using System.Data.Objects;
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface ICourseMemberService : IBaseService
    {
        TMS_Course_Member GetById(int id);
        //a => a.Member_Id == id && a.Course_Details_Id == course
        TMS_Course_Member GetById(int memberId, int courseDetailId);
        //a.DeleteApprove == null && a.IsDelete == null
        IQueryable<TMS_Course_Member> GetCourses(IEnumerable<int> memberIds, bool isGetAll = false);
        IQueryable<TMS_Course_Member> GetCourses(int memberId, bool isGetAll = false);
        IQueryable<TMS_Course_Member> Get(Expression<Func<TMS_Course_Member,bool>> query, bool? notApproval = null);
        IQueryable<TMS_Course_Member> GetApi(Expression<Func<TMS_Course_Member, bool>> query);
        //ObjectResult<sp_GetSubjectResult_Result> sp_GetSubjectResult(string subjectId);

        IQueryable<TMS_Course_Member> GetCoursesMembers(int memberId, bool isGetAll = false);
        IQueryable<TMS_Course_Member_Remark> GetRemark(Expression<Func<TMS_Course_Member_Remark, bool>> query);
        void BulkUpdate(List<TMS_Course_Member> entity);

        void Update(TMS_Course_Member entity);
        void Insert(TMS_Course_Member entity);
        void UpdateRemark(TMS_Course_Member_Remark entity);
        void InsertRemark(TMS_Course_Member_Remark entity);

        void Delete(TMS_Course_Member entity);
        List<sp_GetSubjectResult_TV_Result> GetSubjectResult(int courseDetailId);
    }
}
