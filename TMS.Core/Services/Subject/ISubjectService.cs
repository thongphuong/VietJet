using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace TMS.Core.Services.Subject
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using TMS.Core.ViewModels.Subjects;
    

    public interface ISubjectService : IBaseService
    {//!a.bit_Deleted && a.int_Parent_Id != null
        Subject GetById(int id);
        SubjectDetail GetSDetailByCode(string code);
        SubjectDetail GetSubjectDetailById(int? id = null);
        IQueryable<Subject> GetByCode(string code);
        IQueryable<Subject> GetByName(string code);
        IQueryable<SubjectDetail> GetSubjectDetail(int subjectId);
        IQueryable<SubjectDetail> GetSubjectDetailByCode(string code);
        IQueryable<SubjectDetail> GetSubjectDetailByName(string name);
        //!a.bit_Deleted && a.int_Parent_Id != null && a.int_Course_Type != (int)Constants.CourseType.General
        IQueryable<Subject> Get(Expression<Func<Subject, bool>> query = null);
        IQueryable<SubjectDetail> GetSubjectDetail(Expression<Func<SubjectDetail, bool>> query = null);
        IQueryable<SubjectDetail> GetSubjectDetailApi(Expression<Func<SubjectDetail,bool>> query = null);
            //a.Course.int_Status == 0
      
        void ModifyGroupSubject(GroupSubjectViewModel model );
        void Insert(Subject entity);
        void Update(Subject entity);
        void Delete(Subject entity);
        //use when enable subject table
        void Modify(SubjectModifyViewModel model);
        void Modify(SubjectDetailModifyModel model  , FormCollection form);
        void Update(SubjectDetail entity);
        void Insert(SubjectDetail entity);
        void CreateGroup(SubjectDetailModifyModel model, FormCollection form);
        #region Cat_Groupsubject
        CAT_GROUPSUBJECT GetGroupSubjectById(int? type = null);
        IQueryable<CAT_GROUPSUBJECT> GetGroupSubject();
        IQueryable<CAT_GROUPSUBJECT> GetGroupSubject(Expression<Func<CAT_GROUPSUBJECT, bool>> query);
        IQueryable<CAT_GROUPSUBJECT> GetAPIGroupSubject(Expression<Func<CAT_GROUPSUBJECT, bool>> query);
        void UpdateGroupSubject(CAT_GROUPSUBJECT entity);
        void InsertGroupSubject(CAT_GROUPSUBJECT entity);
        #endregion

        #region Cat_GROUPSUBJECT_ITEMItem
        CAT_GROUPSUBJECT_ITEM GetGroupSubjectItemById(int? type = null);
        IQueryable<CAT_GROUPSUBJECT_ITEM> GetGroupSubjectItem();
        IQueryable<CAT_GROUPSUBJECT_ITEM> GetGroupSubjectItem(Expression<Func<CAT_GROUPSUBJECT_ITEM, bool>> query);

        void UpdateGroupSubjectItem(CAT_GROUPSUBJECT_ITEM entity);
        void InsertGroupSubjectItem(CAT_GROUPSUBJECT_ITEM entity);
        // remove
        void DeleteGroupSubjectItem(IEnumerable<CAT_GROUPSUBJECT_ITEM> entities);
        #endregion

        #region Subject score

      
        Subject_Score GetScoreById(int subjectScoreId);
        Subject_Score GetScore(int subjectId, string grade = "Pass");
        IQueryable<Subject_Score> GetScores(Expression<Func<Subject_Score,bool>> query = null);
        void InsertScore(Subject_Score entity);
        void UpdateScore(Subject_Score entity);

        #endregion
        #region Subject training center
        IQueryable<Subject_TrainingCenter> GetTrainingCenter(Expression<Func<Subject_TrainingCenter, bool>> query = null);


        #endregion
    }
}
