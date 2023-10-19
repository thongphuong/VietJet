using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TMS.Core.Services.CourseResultSummary

{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;

    public class CourseResultSummaryService : BaseService, ICourseResultSummaryService
    {
        private readonly IRepository<Course_Result_Summary> _repoCourseResultSummary; 
       

        public CourseResultSummaryService(IUnitOfWork unitOfWork, IRepository<Course_Result_Summary> repoCourseResultSummary,  IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoCourseResultSummary = repoCourseResultSummary;
          
        }

      
        public IQueryable<Course_Result_Summary> Get(Expression<Func<Course_Result_Summary, bool>> query = null)
        {
           
            return query == null ? _repoCourseResultSummary.GetAll() :_repoCourseResultSummary.GetAll(query);
        }

        public Course_Result_Summary GetById(int traineeId, int courseDetailId)
        {
            return
                _repoCourseResultSummary.GetAll(
                    a =>
                        a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).FirstOrDefault();
        }

        public void Insert(Course_Result_Summary entity)
        {
            _repoCourseResultSummary.Insert(entity);
            Uow.SaveChanges();
        }
        public void Update(Course_Result_Summary entity)
        {
            _repoCourseResultSummary.Update(entity);
            Uow.SaveChanges();
        }
    }
}
