using TMS.Core.ViewModels.APIModels;

namespace TMS.Core.Services.CourseDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using DAL.Entities;
    using ViewModels.Courses;
    using TMS.Core.Utils;

    public interface ICourseDetailService : IBaseService
    {
        //TODO: !isdeleted 
        Course_Detail GetById(int? id);

        Course GetCourseById(int Id);

        Course_Detail GetCourseDetailById(int? id);

        void BulkUpdate(List<Course_Detail> entity);

        IQueryable<Course_Detail> GetByCourse(IEnumerable<int> courseIds);
        IQueryable<Course_Detail> GetByCourse(int courseId);
        IQueryable<Course_Blended_Learning> GetBlendedByCourseId(int courseDetailId);
        IQueryable<Course_Detail> GetBySubject(IEnumerable<int> subjectIds);
        IQueryable<Course_Detail> GetBySubject(int subjectId);
        IQueryable<Course_Detail> Get(DateTime? dateFrom, DateTime? dateTo);
        IQueryable<Course_Detail> Get(Expression<Func<Course_Detail,bool>> query, int[] approveType = null);
        bool checkapproval(Course_Detail course_Detail, int[] approveType = null);
        IQueryable<Course_Detail> GetAllApi(Expression<Func<Course_Detail, bool>> query);
        IQueryable<Course_Detail_Instructor> GetDetailInstructors(Expression<Func<Course_Detail_Instructor, bool>> query= null);
        IQueryable<Course_Detail_Subject_Note> GetDetailSubjectNote(Expression<Func<Course_Detail_Subject_Note, bool>> query);
        Course_Detail_Subject_Note GetDetailSubjectNoteById(int? id);

        Course_Detail_Instructor GetDetailInstructorBySubject(int? id);
        Course_Detail_Instructor GetDetailInstructorById(int? id);
        //a=>a.Course_Id == id_course && !a.bit_Deleted && !a.Subject.bit_Deleted && a.Subject.int_Course_Type != (int)Constants.CourseType.General && a.Subject.int_Parent_Id != null
        //SelectList GetSubjectData();
        void Update(Course_Detail entity);
        void UpdateLmsStatus( int courseId);
        void Update(Course_Detail_Subject_Note entity);
        void Insert(Course_Detail_Subject_Note entity);
        void Insert(Course_Detail entity);
        void Delete(Course_Detail entity);
        void Insert(Course_Detail_Instructor entity);
        string GetGradebyscore(int? courseDetailId, double? score);

        #region[API AssignTrainee]

        bool Assign(APIAssignLMS[] model, string currentUser);
        string AssignTrainee(APICourseTraineeLMSViewModel[] model, string currentUser);
        #endregion
        List<sp_GetDetail_TV_Result> GetTraining(string venue, string courseCode,
     string courseName, int? departmentid, DateTime? dateFrom, DateTime? dateTo, string courids, int? CourseID);
    }
}
