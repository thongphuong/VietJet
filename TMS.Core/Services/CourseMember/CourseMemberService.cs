using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.CourseMember
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;

    public class CourseMemberService : BaseService,ICourseMemberService
    {
        private readonly IRepository<TMS_Course_Member> _repoCourseMember;
        private readonly IRepository<PROCESS_Steps> _repoPROCESS_Steps;
        private readonly IRepository<TMS_Course_Member_Remark> _repoCourseMemberRemark;
        public CourseMemberService(IUnitOfWork unitOfWork, IRepository<TMS_Course_Member> repoCourseMember, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<PROCESS_Steps> repoPROCESS_Steps, IRepository<TMS_Course_Member_Remark> repoCourseMemberRemark) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCourseMember = repoCourseMember;
            _repoPROCESS_Steps = repoPROCESS_Steps;
            _repoCourseMemberRemark = repoCourseMemberRemark;
        }

        public TMS_Course_Member GetById(int id)
        {
            return _repoCourseMember.Get(id);
        }

        public TMS_Course_Member GetById(int memberId, int courseDetailId)
        {
            return
                _repoCourseMember.GetAll(
                    a =>
                        a.Member_Id == memberId && a.Course_Details_Id == courseDetailId &&
                        (!a.IsDelete.HasValue || a.IsDelete.Value == false)).FirstOrDefault();
        }

        public IQueryable<TMS_Course_Member> GetCourses(IEnumerable<int> memberIds, bool isGetAll = false)
        {
            var entities = _repoCourseMember.GetAll(a => a.Member_Id.HasValue && memberIds.Contains(a.Member_Id.Value));
            return isGetAll
              ? entities
              : entities.Where(a => !a.IsDelete.HasValue || a.IsDelete.Value == false);
        }

        public IQueryable<TMS_Course_Member> Get(Expression<Func<TMS_Course_Member, bool>> query, bool? notApproval = null)
        {
            var entities = _repoCourseMember.GetAll(a => (!a.IsDelete.HasValue || a.IsDelete == false) && a.Course_Detail.IsDeleted != true );
            entities = query == null
              ? entities
              : entities.Where(query);
            if (notApproval == null)
            {
                var datastep = _repoPROCESS_Steps.Get(a => a.Step == (int)UtilConstants.ApproveType.AssignedTrainee && a.IsActive==true);
                entities = datastep != null ? entities.Where(a => a.Course_Detail.Course.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee)) : entities;
            }
            return entities;
        }

        public IQueryable<TMS_Course_Member> GetCoursesMembers(int memberId, bool isGetAll = false)
        {
            var entities = _repoCourseMember.GetAll(a => a.Member_Id == memberId);
            return entities.Where(a => a.IsDelete == isGetAll);
        }

        public void BulkUpdate(List<TMS_Course_Member> entity)
        {
            _repoCourseMember.Update(entity);
            Uow.SaveChanges();
        }

        public IQueryable<TMS_Course_Member> GetApi(Expression<Func<TMS_Course_Member, bool>> query)
        {
            var entities = _repoCourseMember.GetAll();
            return query == null
              ? entities
              : entities.Where(query);
        }
        public void Update(TMS_Course_Member entity)
        {
            _repoCourseMember.Update(entity);
            Uow.SaveChanges();
        }

        public IQueryable<TMS_Course_Member> GetCourses(int memberId, bool isGetAll = false)
        {
            var entities = _repoCourseMember.GetAll(a =>a.Member_Id == memberId);
            return isGetAll
              ? entities
              : entities.Where(a=> !a.IsDelete.HasValue || a.IsDelete.Value == false);
        }

        public void Insert(TMS_Course_Member entity)
        {
            _repoCourseMember.Insert(entity);
            Uow.SaveChanges();
        }

        public void Delete(TMS_Course_Member entity)
        {
            //_repoCourseMember.Delete(entity);
            entity.IsDelete = true;
            _repoCourseMember.Update(entity);
            
            Uow.SaveChanges();
        }

        public List<sp_GetSubjectResult_TV_Result> GetSubjectResult(int courseDetailId)
        {
            return Uow.sp_GetSubjectResult_TV_Result(courseDetailId).ToList();
        }

        public IQueryable<TMS_Course_Member_Remark> GetRemark(Expression<Func<TMS_Course_Member_Remark, bool>> query)
        {
            var entities = _repoCourseMemberRemark.GetAll();
            return query == null
              ? entities
              : entities.Where(query);
           
        }

        public void UpdateRemark(TMS_Course_Member_Remark entity)
        {
            _repoCourseMemberRemark.Insert(entity);
            Uow.SaveChanges();
        }

        public void InsertRemark(TMS_Course_Member_Remark entity)
        {
            _repoCourseMemberRemark.Update(entity);
            Uow.SaveChanges();
        }
    }
}
