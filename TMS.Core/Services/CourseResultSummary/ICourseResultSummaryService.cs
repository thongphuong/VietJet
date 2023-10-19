using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.Services.CourseResultSummary
{
    using System.Linq.Expressions;
    using DAL.Entities;

    public interface ICourseResultSummaryService : IBaseService
    {



        IQueryable<Course_Result_Summary> Get(Expression<Func<Course_Result_Summary, bool>> query = null);

        Course_Result_Summary GetById(int traineeId , int courseDetailId);
        void Insert(Course_Result_Summary entity);
        void Update(Course_Result_Summary entity);
    }
}
