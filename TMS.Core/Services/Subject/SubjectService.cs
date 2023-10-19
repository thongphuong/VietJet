using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace TMS.Core.Services.Subject
{
    using System.Linq.Expressions;
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.Subjects;
    using TMS.Core.ViewModels.UserModels;


    public class SubjectService : BaseService, ISubjectService
    {
        private readonly IRepository<Subject> _repoSubject;
        private readonly IRepository<SubjectDetail> _repoSubjectDetail;
        private readonly IRepository<Subject_Score> _repoSubjectScore;
        private readonly IRepository<Instructor_Ability> _repoInstructorAbility;
        private readonly IRepository<Instructor_Ability_LOG> _repoTraineeAbility_Log;
        private readonly IRepository<CAT_GROUPSUBJECT> _repoGroupSubject;
        private readonly IRepository<CAT_GROUPSUBJECT_ITEM> _repoGroupSubjectItem;
        private readonly IRepository<Course_Detail_Instructor> _repoCourseDetailInstructor;
        private readonly IRepository<Trainee> _repoTrainee;
        private readonly IRepository<Subject_TrainingCenter> _repoSubject_TrainingCenter;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        private readonly Expression<Func<Subject, bool>> _defaultQuerySubject =
            a => a.bit_Deleted==false && a.isActive==true && a.isDeleted==false && (a.int_Parent_Id == null || a.Subject2.bit_Deleted==false);

        private readonly Expression<Func<SubjectDetail, bool>> _defaultQuerySubjectDetail =
            a => a.IsDelete != true;
        private readonly Expression<Func<CAT_GROUPSUBJECT, bool>> _defaultQueryCatGroupSubject =
           a => a.IsDeleted != true;
        public SubjectService(IUnitOfWork unitOfWork, IRepository<Subject> repoSubject, IRepository<Course_Detail_Instructor> repoCourseDetailInstructor, IRepository<Subject_Score> repoSubjectScore, IRepository<CAT_GROUPSUBJECT> repoGroupSubject, IRepository<CAT_GROUPSUBJECT_ITEM> repoGroupSubjectItem, IRepository<SubjectDetail> repoSubjectDetail, IRepository<Instructor_Ability> repoInstructorAbility, IRepository<Course> repoCourse, IRepository<Instructor_Ability_LOG> repoTraineeAbility_Log, IRepository<Trainee> repoTrainee, IRepository<SYS_LogEvent> repoSYS_LogEvent, IRepository<Subject_TrainingCenter> repoSubject_TrainingCenter) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoTrainee = repoTrainee;
            _repoSubject = repoSubject;
            _repoSubjectScore = repoSubjectScore;
            _repoGroupSubject = repoGroupSubject;
            _repoGroupSubjectItem = repoGroupSubjectItem;
            _repoSubjectDetail = repoSubjectDetail;
            _repoInstructorAbility = repoInstructorAbility;
            _repoTraineeAbility_Log = repoTraineeAbility_Log;
            _repoCourseDetailInstructor = repoCourseDetailInstructor;
            _repoSubject_TrainingCenter = repoSubject_TrainingCenter;
        }

        public SubjectDetail GetSDetailByCode(string code)
        {
            var subjectDetail = _repoSubjectDetail.GetAll().Where(c => c.Code.Equals(code)).FirstOrDefault();
            return subjectDetail;
        }

        public Subject GetById(int id)
        {
            var entity = _repoSubject.Get(id);
            return entity == null || entity.bit_Deleted==true ? null : entity;
        }

        public SubjectDetail GetSubjectDetailById(int? id = null)
        {
            if (id == null) return null;
            
            var entity = _repoSubjectDetail.Get(id);
            return entity == null || entity.IsDelete==true ? null : entity;
        }

        public IQueryable<Subject> GetByCode(string code)
        {
            return _repoSubject.GetAll(_defaultQuerySubject).Where(a => a.str_Code.Contains(code));
        }
         
        public IQueryable<Subject> GetByName(string code)
        {
            return _repoSubject.GetAll(_defaultQuerySubject).Where(a => a.str_Code.Contains(code));
        }

        public IQueryable<SubjectDetail> GetSubjectDetail(int subjectId)
        {
            return _repoSubjectDetail.GetAll(_defaultQuerySubjectDetail).Where(a => a.SubjectHeaderId == subjectId);
        }

        public IQueryable<SubjectDetail> GetSubjectDetailByCode(string code)
        {
            return _repoSubjectDetail.GetAll(_defaultQuerySubjectDetail).Where(a => a.Code.Contains(code));
        }

        public IQueryable<Subject> GetByName(string name, bool getGeneral = false)
        {
            return _repoSubject.GetAll(_defaultQuerySubject).Where(a => a.str_Name.Contains(name));
        }

        public IQueryable<SubjectDetail> GetSubjectDetailByName(string name)
        {
            return _repoSubjectDetail.GetAll(_defaultQuerySubjectDetail).Where(a => a.Name.Contains(name));
        }

        public IQueryable<Subject> Get(Expression<Func<Subject, bool>> query = null)
        {
            var entities = _repoSubject.GetAll(_defaultQuerySubject);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public IQueryable<SubjectDetail> GetSubjectDetail(Expression<Func<SubjectDetail, bool>> query = null)
        {
            var entities = _repoSubjectDetail.GetAll(_defaultQuerySubjectDetail);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public IQueryable<SubjectDetail> GetSubjectDetailApi(Expression<Func<SubjectDetail, bool>> query = null)
        {
            return query == null ? _repoSubjectDetail.GetAll() : _repoSubjectDetail.GetAll().Where(query);
        }

        public IQueryable<SubjectDetail> GetSubjectDetails(Expression<Func<SubjectDetail, bool>> query = null)
        {
            var entities = _repoSubjectDetail.GetAll(_defaultQuerySubjectDetail);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

      

        public void ModifyGroupSubject(GroupSubjectViewModel model)
        {


            var entity = model.Id.HasValue ? _repoGroupSubject.Get(model.Id) : null;
            
            var groupSubject =
                   _repoGroupSubject.Get(a => a.Code.ToLower().Trim() == model.Code.ToLower().Trim() && a.IsDeleted == false && a.id != model.Id);
            if (groupSubject != null)
            {
                var duplicateMessage = string.Format(Messege.DataIsExists, Resource.lblGroupCourse,  model.Code );
                throw new Exception(duplicateMessage);
            }
            var now = DateTime.Now;
            if (entity == null)
            {
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    throw new Exception(codeHasSpaceMessage);
                }
                entity = new CAT_GROUPSUBJECT()
                {
                    CreatedDate = now,
                    CreatedBy = CurrentUser.USER_ID.ToString(),
                    Code = model.Code,
                    IsDeleted = false,
                    Name = model.Name,
                    IsActive = true,
                    Description = model.Description,
                    CertificateCode = model.CertificateCode,
                    CertificateName = model.CertificateName,
                    LmsStatus = (int)UtilConstants.ApiStatus.Modify

                };
                _repoGroupSubject.Insert(entity);
                if (model.AssignedSubjects != null && model.AssignedSubjects.Any())
                {
                    for (var i = 0; i < model.AssignedSubjects.Count(); i++)
                    {
                        entity.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { CAT_GROUPSUBJECT = entity, id_subject = model.AssignedSubjects.ElementAt(i) });
                    }
                }
            }
            else
            {

                entity.UpdatedDate = now;
                entity.UpdatedBy = CurrentUser.USER_ID.ToString();
                entity.Code = model.Code;
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.CertificateCode = model.CertificateCode;
                entity.CertificateName = model.CertificateName;
                entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                _repoGroupSubject.Update(entity);
                var subjectItems = entity.CAT_GROUPSUBJECT_ITEM.ToList();
                _repoGroupSubjectItem.Delete(subjectItems);
                entity.CAT_GROUPSUBJECT_ITEM.Clear();
                if (model.AssignedSubjects != null && model.AssignedSubjects.Any())
                {
                    for (var i = 0; i < model.AssignedSubjects.Count(); i++)
                    {
                        entity.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { CAT_GROUPSUBJECT = entity, id_subject = model.AssignedSubjects.ElementAt(i) });
                    }
                }
            }
            Uow.SaveChanges();
        }

        public void Insert(Subject entity)
        {
            _repoSubject.Insert(entity);
            Uow.SaveChanges();
        }

        public void Modify(SubjectDetailModifyModel model, FormCollection form)
        {
            var conflictErrorMessage = string.Format(Messege.DataIsExists, Resource.lblSubject,  model.Code );
            SubjectDetail entity;
            SubjectDetail subject;
            Instructor_Ability_LOG log;
            var point_from = !string.IsNullOrEmpty(form["point_from"])
                          ? form["point_from"].Split(',') : null;
            var grade_name = !string.IsNullOrEmpty(form["grade_name"])
                ? form["grade_name"].Split(',') : null;
            var Subject_Score_hd_id = !string.IsNullOrEmpty(form["Subject_Score_hd_id"])
                ? form["Subject_Score_hd_id"].Split(',') : null;
            var int_khoidaotao = !String.IsNullOrEmpty(form["int_khoidaotao"]) ? form["int_khoidaotao"].Split(',') : null;
            string str_Name_Temp = form["Name_Temp"];
            int Editparent = !String.IsNullOrEmpty(form["levelsubject"]) ? Int32.Parse(form["levelsubject"].Trim()) : 1;
            int int_Parent_Id = !String.IsNullOrEmpty(form["int_Parent_Id"]) ? Int32.Parse(form["int_Parent_Id"].Trim()) : -1;
            object[] int_subject_type_ = !String.IsNullOrEmpty(form["int_subject_type_"])
                ? form["int_subject_type_"].Split(',') : null;
            object[] type_duration = !String.IsNullOrEmpty(form["type_duration"])
                ? form["type_duration"].Split(',') : null;
            object[] type_recurrent = !String.IsNullOrEmpty(form["type_recurrent"])
                ? form["type_recurrent"].Split(',') : null;
            object[] str_CertificateName = !String.IsNullOrEmpty(form["CertificateName"]) ? form["CertificateName"].Split(',') : null;
            object[] str_CertificateCode = !String.IsNullOrEmpty(form["CertificateCode"]) ? form["CertificateCode"].Split(',') : null;
            object[] str_CertificateName1 = !String.IsNullOrEmpty(form["CertificateName1"]) ? form["CertificateName1"].Split(',') : null;
            object[] str_CertificateCode1 = !String.IsNullOrEmpty(form["CertificateCode1"]) ? form["CertificateCode1"].Split(',') : null;
            object[] int_GroupSubject_id_ = !String.IsNullOrEmpty(form["int_GroupSubject_id_"])
               ? form["int_GroupSubject_id_"].Split(',') : null;
            object[] int_GroupSubject_id_edit = !String.IsNullOrEmpty(form["int_GroupSubject_id_edỉt"])
             ? form["int_GroupSubject_id_edỉt"].Split(',') : null;
            int_subject_type_ = SetObjectNull(int_subject_type_);
            type_duration = SetObjectNull(type_duration);

            str_CertificateName = SetObjectNull(str_CertificateName);
            str_CertificateCode = SetObjectNull(str_CertificateCode);
            str_CertificateName1 = SetObjectNull(str_CertificateName1);
            str_CertificateCode1 = SetObjectNull(str_CertificateCode1);
            type_recurrent = SetObjectNull(type_recurrent);
            int_GroupSubject_id_ = SetObjectNull(int_GroupSubject_id_);
            int_GroupSubject_id_edit = SetObjectNull(int_GroupSubject_id_edit);

            if (!model.Id.HasValue)
            {
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    throw new Exception(codeHasSpaceMessage);
                }
                var checkCode = _repoSubjectDetail.Get(a => a.IsDelete != true && a.Code.ToLower().Trim().Equals(model.Code.ToLower().Trim()));
                if (checkCode != null)
                    throw new Exception(conflictErrorMessage);
                entity = new SubjectDetail()
                {
                    Name = model.Name,
                    Code = model.Code,
                    CreatedBy = CurrentUser.USER_ID.ToString(),
                    IsActive = true,
                    IsDelete = false,
                    CreatedDate = DateTime.Now,
                    CourseTypeId = (int)TMS.Core.Utils.UtilConstants.CourseTypes.General,
                    //cập nhật trạng thai gửi LMS
                    LmsStatus = 0,//(int)UtilConstants.ApiStatus.Modify,
                    IsAverageCalculate = model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes
                };
                _repoSubjectDetail.Insert(entity);
                Uow.SaveChanges();
                if (int_khoidaotao != null) // check <> null
                {
                    foreach (var item in int_khoidaotao)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                            var intkhoidatao_ = Convert.ToInt32(item);
                            instructorsubject.subject_id = entity.Id;
                            instructorsubject.khoidaotao_id = intkhoidatao_;
                            _repoSubject_TrainingCenter.Insert(instructorsubject);
                        }
                    }
                }
                //thêm dũ liệu cấp con
                List<int> intsubtype = new List<int>();
                if(int_subject_type_ != null)
                {
                    foreach (var item in int_subject_type_)
                    {
                        intsubtype.Add(int.Parse(item.ToString()));
                    }
                }
                
                var y = 0;
                foreach (var arr_typesubject in intsubtype)
                {
                    #region add new

                    subject = new SubjectDetail
                    {
                        Name = model.Name,
                        Code = model.Code,
                        CreatedBy = CurrentUser.USER_ID.ToString(),
                        IsActive = true,
                        IsDelete = false,
                    };

                    subject.CourseTypeId = arr_typesubject;
                    if (str_CertificateCode?[y] != null)
                    {
                        subject.CertificateCode = str_CertificateCode[y].ToString();
                    }
                    if(entity.int_Parent_Id.HasValue)
                    {
                        subject.int_Parent_Id = entity.int_Parent_Id;
                    }
                    if (str_CertificateName?[y] != null)
                    {
                        subject.CertificateName = str_CertificateName[y].ToString();
                    }
                    subject.IsAverageCalculate = model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes;
                    if (type_duration?[y] != null)
                    {
                        subject.Duration = double.Parse(type_duration[y].ToString());
                    }
                    if (type_recurrent?[y] != null)
                    {
                        subject.RefreshCycle = int.Parse(type_recurrent[y].ToString());
                    }
                    _repoSubjectDetail.Insert(subject);
                    if (int_GroupSubject_id_ != null && int_GroupSubject_id_.Any())
                    {
                        for (var i = 0; i < int_GroupSubject_id_.Count(); i++)
                        {
                            subject.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { id_groupsubject = Convert.ToInt32(int_GroupSubject_id_.ElementAt(i)), SubjectDetail = subject });
                        }
                    }
                    Uow.SaveChanges();
                    // check for add new genaral typen to anti bug (big) -- cai nay tuyet vong lam, hay can than
                    #endregion

                    if (int_khoidaotao != null) // check <> null
                    {
                        foreach (var item in int_khoidaotao)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                                var intkhoidatao_ = Convert.ToInt32(item);
                                instructorsubject.subject_id = subject.Id;
                                instructorsubject.khoidaotao_id = intkhoidatao_;
                                _repoSubject_TrainingCenter.Insert(instructorsubject);
                            }
                        }
                    }
                    y++;
                }
                Uow.SaveChanges();
            }
            else
            {
                entity = _repoSubjectDetail.Get(model.Id.Value);

                if (entity == null) throw new Exception( "Data is not found" );
                entity.Name = model.Name;
                //khong cho chinh sua COde
                //entity.Code = model.Code;
                entity.IsAverageCalculate = model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes;
                entity.RefreshCycle = (model.Recurrent > 0) ? model.Recurrent : 0;
                entity.ModifyDate = DateTime.Now;
                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                _repoSubject_TrainingCenter.Delete(entity.Subject_TrainingCenter);
                if (int_khoidaotao != null) // check <> null
                {
                    foreach (var item in int_khoidaotao)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                            var intkhoidatao_ = Convert.ToInt32(item);
                            instructorsubject.subject_id = entity.Id;
                            instructorsubject.khoidaotao_id = intkhoidatao_;
                            _repoSubject_TrainingCenter.Insert(instructorsubject);
                        }
                    }
                }

                if (Editparent == 1)
                {
                    var subjectToUpdates = _repoSubjectDetail.GetAll(a =>a.Code == entity.Code && a.CourseTypeId.HasValue && a.CourseTypeId != 6 && a.IsDelete != true).OrderBy(a=>a.Id);
                    var i = 0;
                    if(subjectToUpdates.Count()> 0)
                    {
                        foreach (var item in subjectToUpdates)
                        {
                            var subjectToUpdate = _repoSubjectDetail.Get(item.Id);
                            subjectToUpdate.Name = model.Name;
                            subjectToUpdate.Code = model.Code;
                            if (str_CertificateCode1 != null)
                            {

                                subjectToUpdate.CertificateCode = str_CertificateCode1[i] == null ? null : str_CertificateCode1[i].ToString();
                            }
                            if (str_CertificateName1 != null)
                            {
                                subjectToUpdate.CertificateName = str_CertificateName1[i] == null ? null : str_CertificateName1[i].ToString();
                            }
                            _repoSubjectDetail.Update(subjectToUpdate);
                            i++;
                        }
                        Uow.SaveChanges();
                        List<int> intsubtype_edit = new List<int>();
                        if (int_subject_type_ != null)
                        {
                            foreach (var item in int_subject_type_)
                            {
                                intsubtype_edit.Add(int.Parse(item.ToString()));
                            }
                        }

                        var y = 0;
                        foreach (var arr_typesubject in intsubtype_edit)
                        {
                            #region add new

                            subject = new SubjectDetail
                            {
                                Name = model.Name,
                                Code = model.Code,
                                CreatedBy = CurrentUser.USER_ID.ToString(),
                                IsActive = true,
                                IsDelete = false,
                            };

                            subject.CourseTypeId = arr_typesubject;
                            if (str_CertificateCode?[y] != null)
                            {
                                subject.CertificateCode = str_CertificateCode[y].ToString();
                            }
                            if (entity.int_Parent_Id.HasValue)
                            {
                                subject.int_Parent_Id = entity.int_Parent_Id;
                            }
                            if (str_CertificateName?[y] != null)
                            {
                                subject.CertificateName = str_CertificateName[y].ToString();
                            }
                            subject.IsAverageCalculate = model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes;
                            if (type_duration?[y] != null)
                            {
                                subject.Duration = double.Parse(type_duration[y].ToString());
                            }
                            if (type_recurrent?[y] != null)
                            {
                                subject.RefreshCycle = int.Parse(type_recurrent[y].ToString());
                            }
                            _repoSubjectDetail.Insert(subject);
                            if (int_GroupSubject_id_ != null && int_GroupSubject_id_.Any())
                            {
                                for (var ii = 0; ii < int_GroupSubject_id_.Count(); ii++)
                                {
                                    subject.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { id_groupsubject = Convert.ToInt32(int_GroupSubject_id_.ElementAt(ii)), SubjectDetail = subject });
                                }
                            }
                            Uow.SaveChanges();
                            // check for add new genaral typen to anti bug (big) -- cai nay tuyet vong lam, hay can than
                            #endregion
                            if (int_khoidaotao != null) // check <> null
                            {
                                foreach (var item in int_khoidaotao)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                                        var intkhoidatao_ = Convert.ToInt32(item);
                                        instructorsubject.subject_id = subject.Id;
                                        instructorsubject.khoidaotao_id = intkhoidatao_;
                                        _repoSubject_TrainingCenter.Insert(instructorsubject);
                                    }
                                }
                            }
                            y++;
                        }
                    }
                    else 
                    {
                        List<int> intsubtype = new List<int>();
                        if(int_subject_type_ != null)
                        {
                            foreach (var item in int_subject_type_)
                            {
                                intsubtype.Add(int.Parse(item.ToString()));
                            }
                        }
                        var y = 0;
                        foreach (var arr_typesubject in intsubtype)
                        {
                            #region add new

                            subject = new SubjectDetail
                            {
                                Name = model.Name,
                                Code = model.Code,
                                CreatedBy = CurrentUser.USER_ID.ToString(),
                                IsActive = true,
                                IsDelete = false,
                            };

                            subject.CourseTypeId = arr_typesubject;
                            if (str_CertificateCode?[y] != null)
                            {
                                subject.CertificateCode = str_CertificateCode[y].ToString();
                            }
                            if (entity.int_Parent_Id.HasValue)
                            {
                                subject.int_Parent_Id = entity.int_Parent_Id;
                            }
                            
                            if (str_CertificateName?[y] != null)
                            {
                                subject.CertificateName = str_CertificateName[y].ToString();
                            }
                            subject.IsAverageCalculate = model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes;
                            if (type_duration?[y] != null)
                            {
                                subject.Duration = double.Parse(type_duration[y].ToString());
                            }
                            if (type_recurrent?[y] != null)
                            {
                                subject.RefreshCycle = int.Parse(type_recurrent[y].ToString());
                            }
                            _repoSubjectDetail.Insert(subject);
                            if (int_GroupSubject_id_ != null && int_GroupSubject_id_.Any())
                            {
                                for (var ii = 0; ii < int_GroupSubject_id_.Count(); ii++)
                                {
                                    subject.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { id_groupsubject = Convert.ToInt32(int_GroupSubject_id_.ElementAt(ii)), SubjectDetail = subject });
                                }
                            }
                            Uow.SaveChanges();
                            // check for add new genaral typen to anti bug (big) -- cai nay tuyet vong lam, hay can than
                            #endregion

                            if (int_khoidaotao != null) // check <> null
                            {
                                foreach (var item in int_khoidaotao)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                                        var intkhoidatao_ = Convert.ToInt32(item);
                                        instructorsubject.subject_id = subject.Id;
                                        instructorsubject.khoidaotao_id = intkhoidatao_;
                                        _repoSubject_TrainingCenter.Insert(instructorsubject);
                                    }
                                }
                            }
                            y++;
                        }
                    }
                    Uow.SaveChanges();
                }
                else
                {
                    entity.Duration = model.Duration;
                    entity.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                    if (model.IsAverageCaculate == (int)UtilConstants.BoolEnum.Yes)
                    {
                        entity.PassScore = model.PassScore;
                        if (point_from == null)
                        {
                            throw new Exception(Messege.VALIDATION_SUBJECT_POINT);
                        }
                        if (grade_name == null)
                        {
                            throw new Exception(Messege.VALIDATION_SUBJECT_GRADE);
                        }
                        _repoSubjectScore.Delete(entity.Subject_Score);
                        if (point_from != null && grade_name != null)
                        {
                            var i = 0;
                            foreach (var arr_c in grade_name)
                            {
                                if (!string.IsNullOrEmpty(point_from[i]) && !string.IsNullOrEmpty(grade_name[i]))
                                {
                                    Subject_Score subjectScore = new Subject_Score();
                                    if (0 <= int.Parse(point_from[i]) && int.Parse(point_from[i]) <= 100)
                                    {
                                        subjectScore.point_from = float.Parse(point_from[i]);
                                    }
                                    else
                                    {
                                        throw new Exception(Messege.VALIDATION_SUBJECT_RANGEPOINT);
                                    }
                                    subjectScore.grade = grade_name[i];
                                    entity.Subject_Score.Add(subjectScore);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(point_from[i]))
                                    {
                                        throw new Exception(Messege.VALIDATION_SUBJECT_POINT);
                                    }
                                    if (string.IsNullOrEmpty(grade_name[i]))
                                    {
                                        throw new Exception(Messege.VALIDATION_SUBJECT_GRADE);
                                    }
                                }
                                i++;
                            }
                            Uow.SaveChanges();
                        }
                    }
                    else
                    {
                        entity.PassScore = null;
                        _repoSubjectScore.Delete(entity.Subject_Score);
                        Uow.SaveChanges();
                    }
                    #region
                    if (model.ListInstructors != null)
                    {
                        _repoInstructorAbility.Delete(entity.Instructor_Ability);
                        foreach (var ability in model.ListInstructors)
                        {
                            entity.Instructor_Ability.Add(new Instructor_Ability { Allowance = ability.Allowance, InstructorId = ability.InstructorId, SubjectDetailId = ability.SubjectId, CreateBy = CurrentUser.USER_ID, CreateDate = DateTime.Now });
                        }
                    }
                    else
                    {
                        _repoInstructorAbility.Delete(entity.Instructor_Ability);
                    }
                    #endregion

                    var subjectItems = entity.CAT_GROUPSUBJECT_ITEM.ToList();
                    _repoGroupSubjectItem.Delete(subjectItems);
                    entity.CAT_GROUPSUBJECT_ITEM.Clear();
                    if (model.ListGroupCourses != null && model.ListGroupCourses.Any())
                    {
                        for (var i = 0; i < model.ListGroupCourses.Count(); i++)
                        {
                            entity.CAT_GROUPSUBJECT_ITEM.Add(new CAT_GROUPSUBJECT_ITEM() { id_groupsubject = model.ListGroupCourses.ElementAt(i), SubjectDetail = entity });
                        }
                    }
                    #region
                    if (model.ListInstructors != null)
                    {
                        var Listlog = _repoTraineeAbility_Log.GetAll(a => a.SubjectDetailId == entity.Id && a.IsDeleted == false);
                        if (Listlog.Any())
                        {
                            foreach (var item in Listlog)
                            {
                                item.ModifyBy = CurrentUser.USER_ID;
                                item.ModifyDate = DateTime.Now;
                                item.IsDeleted = true;
                                _repoTraineeAbility_Log.Update(item);
                            }
                        }
                        foreach (var ability in model.ListInstructors)
                        {
                            log = new Instructor_Ability_LOG()
                            {
                                InstructorId = ability.InstructorId,
                                SubjectDetailId = entity.Id,
                                Allowance = ability.Allowance ?? 0,
                                CreateDate = DateTime.Now,
                                CreateBy = CurrentUser.USER_ID,
                                IsDeleted = false
                            };
                            _repoTraineeAbility_Log.Insert(log);
                        }
                    }
                    else
                    {
                        var Listlog = _repoTraineeAbility_Log.GetAll(a => a.SubjectDetailId == entity.Id && a.IsDeleted == false);
                        if (Listlog.Any())
                        {
                            foreach (var item in Listlog)
                            {
                                item.IsDeleted = true;
                                item.ModifyBy = CurrentUser.USER_ID;
                                item.ModifyDate = DateTime.Now;
                                _repoTraineeAbility_Log.Update(item);
                            }
                        }
                    }
                    _repoSubject_TrainingCenter.Delete(entity.Subject_TrainingCenter);
                    if (int_khoidaotao != null) // check <> null
                    {
                        foreach (var item in int_khoidaotao)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                                var intkhoidatao_ = Convert.ToInt32(item);
                                instructorsubject.subject_id = entity.Id;
                                instructorsubject.khoidaotao_id = intkhoidatao_;
                                _repoSubject_TrainingCenter.Insert(instructorsubject);
                            }
                        }
                    }
                    #endregion
                    Uow.SaveChanges();
                }
            }
            Uow.SaveChanges();
        }
        public void Modify(SubjectModifyViewModel model)
        {
            Subject entity;
            if (!model.Id.HasValue)
            {
                entity = new Subject()
                {
                    str_Name = model.Name,
                    str_Code = model.Code,
                    str_Created_by = CurrentUser.USER_ID.ToString(),
                    dtm_Created_At = DateTime.Now,
                    int_Parent_Id = model.ParentId
                };
                _repoSubject.Insert(entity);
            }
            else
            {
                entity = _repoSubject.Get(model.Id.Value);
                if (entity == null) throw new Exception( "Data is not found" );
                entity.str_Name = model.Name;
                entity.str_Code = model.Code;
                entity.int_Parent_Id = model.ParentId;
                entity.dtm_Modified_At = DateTime.Now;
                entity.str_Modified_By = CurrentUser.USER_ID.ToString();
                _repoSubject.Update(entity);
            }
            var details = model.SubjectDetailModel;
            foreach (var detail in entity.SubjectDetails.Where(a => details.All(x => x.Id != a.Id)))
            {
                detail.DeletedDate = DateTime.Now; ;
                detail.DeletedBy = CurrentUser.USER_ID.ToString();
                detail.IsDelete = true;
            }
            foreach (var detail in details)
            {
                var subjectDetail = entity.SubjectDetails.FirstOrDefault(a => a.Id == detail.Id);
                if (subjectDetail == null)
                {
                    subjectDetail = new SubjectDetail()
                    {
                        Name = detail.Name,
                        Code = detail.Code,
                        IsAverageCalculate = detail.IsAverageCaculate == 1,
                        RefreshCycle = detail.Recurrent,
                        CreatedDate = DateTime.Now,
                        CreatedBy = CurrentUser.USER_ID.ToString(),
                        Duration = detail.Duration
                    };
                    foreach (var instructor in detail.InstructorAbility)
                    {
                        subjectDetail.Instructor_Ability.Add(new Instructor_Ability() { InstructorId = instructor });
                    }
                   
                    if (detail.SubjectScoreModels.Any())
                    {
                        // insert diem chuan
                        
                        foreach (var score in detail.SubjectScoreModels)
                        {
                            subjectDetail.Subject_Score.Add(new Subject_Score() { grade = score.Grade, point_from = score.Point });
                        }
                    }
                    entity.SubjectDetails.Add(subjectDetail);
                }
                else
                {
                    subjectDetail.Name = detail.Name;
                    subjectDetail.Code = detail.Code;
                    subjectDetail.IsAverageCalculate = detail.IsAverageCaculate == 1;
                    subjectDetail.RefreshCycle = detail.Recurrent;
                    subjectDetail.ModifyDate = DateTime.Now;
                    subjectDetail.ModifiedBy = CurrentUser.USER_ID.ToString();
                    subjectDetail.Duration = detail.Duration;
                    var scores = subjectDetail.Subject_Score.ToList();
                    foreach (var score in scores)
                    {
                        _repoSubjectScore.Delete(score);
                    }
                    subjectDetail.Subject_Score.Clear();
                    foreach (var score in detail.SubjectScoreModels)
                    {
                        subjectDetail.Subject_Score.Add(new Subject_Score() { grade = score.Grade, point_from = score.Point });
                    }
                    var abilities = subjectDetail.Instructor_Ability.ToList();
                    foreach (var ability in abilities)
                    {
                        _repoInstructorAbility.Delete(ability);
                    }
                    subjectDetail.Instructor_Ability.Clear();
                    foreach (var instructor in detail.InstructorAbility)
                    {
                        subjectDetail.Instructor_Ability.Add(new Instructor_Ability() { InstructorId = instructor });
                    }
                }
            }
            Uow.SaveChanges();
        }
        public void CreateGroup(SubjectDetailModifyModel model, FormCollection form)
        {
            SubjectDetail entity;
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
            var conflictErrorMessage = string.Format(Messege.DataIsExists, Resource.lblSubject, model.Code);
            var int_khoidaotao = !String.IsNullOrEmpty(form["int_khoidaotao"]) ? form["int_khoidaotao"].Split(',') : null;
            if (model.Code.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            
            if (!model.Id.HasValue)
            {
                var checkCode = _repoSubjectDetail.Get(a => a.IsActive == true && a.Code.ToLower().Trim().Equals(model.Code.ToLower().Trim()));
                if (checkCode != null)
                    throw new Exception(conflictErrorMessage);
                entity = new SubjectDetail()
                {
                    Name = model.Name,
                    Code = model.Code,
                    CreatedBy = CurrentUser.USER_ID.ToString(),
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDelete = false,
                };
                _repoSubjectDetail.Insert(entity);
            }
            else
            {
                entity = _repoSubjectDetail.Get(model.Id.Value);
                if (entity == null) throw new Exception("Data is not found");
                entity.Name = model.Name;
                entity.Code = model.Code;
                entity.ModifyDate = DateTime.Now;
                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                _repoSubjectDetail.Update(entity);
            }
            _repoSubject_TrainingCenter.Delete(entity.Subject_TrainingCenter);
            if (int_khoidaotao != null) // check <> null
            {
                foreach (var item in int_khoidaotao)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        Subject_TrainingCenter instructorsubject = new Subject_TrainingCenter();
                        var intkhoidatao_ = Convert.ToInt32(item);
                        instructorsubject.subject_id = entity.Id;
                        instructorsubject.khoidaotao_id = intkhoidatao_;
                        _repoSubject_TrainingCenter.Insert(instructorsubject);
                    }
                }
            }
            Uow.SaveChanges();
            var listchild = _repoSubjectDetail.GetAll(a=>a.int_Parent_Id == entity.Id);
            if (listchild.Count() > 0)
            {
                foreach (var item in listchild)
                {
                    item.int_Parent_Id = null;
                    _repoSubjectDetail.Update(item);
                }
                Uow.SaveChanges();
            }
            if (model.SubjectIdList != null)
            {
                foreach (var item in model.SubjectIdList)
                {
                    var temp = _repoSubjectDetail.Get(item);
                    if (temp != null)
                    {
                        temp.int_Parent_Id = entity.Id;
                        _repoSubjectDetail.Update(temp);
                    }
                }
                Uow.SaveChanges();
            }
        }
        public void Update(Subject entity)
        {
            _repoSubject.Update(entity);
            Uow.SaveChanges();
        }
        public void Update(SubjectDetail entity)
        {
            _repoSubjectDetail.Update(entity);
            Uow.SaveChanges();
        }
        public void Insert(SubjectDetail entity)
        {
            _repoSubjectDetail.Insert(entity);
            Uow.SaveChanges();
        }

        public void Delete(Subject entity)
        {
            //_repoSubject.Delete(entity);
            entity.bit_Deleted = true;
            _repoSubject.Update(entity);
            Uow.SaveChanges();
        }

        public CAT_GROUPSUBJECT GetGroupSubjectById(int? id = null)
        {
            if (!id.HasValue) return null;

            var entity = _repoGroupSubject.Get(id);
            return entity?.IsDeleted == null ? null : entity;
        }

        public IQueryable<CAT_GROUPSUBJECT> GetGroupSubject()
        {
            return _repoGroupSubject.GetAll(_defaultQueryCatGroupSubject);
        }

        public IQueryable<CAT_GROUPSUBJECT> GetGroupSubject(Expression<Func<CAT_GROUPSUBJECT, bool>> query)
        {
            var entities = _repoGroupSubject.GetAll(_defaultQueryCatGroupSubject);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public IQueryable<CAT_GROUPSUBJECT> GetAPIGroupSubject(Expression<Func<CAT_GROUPSUBJECT, bool>> query)
        {
            var entities = _repoGroupSubject.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
        public void UpdateGroupSubject(CAT_GROUPSUBJECT entity)
        {
            _repoGroupSubject.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertGroupSubject(CAT_GROUPSUBJECT entity)
        {
            _repoGroupSubject.Insert(entity);
            Uow.SaveChanges();
        }

        public CAT_GROUPSUBJECT_ITEM GetGroupSubjectItemById(int? id = null)
        {
            return !id.HasValue ? null : _repoGroupSubjectItem.Get(id);
        }

        public IQueryable<CAT_GROUPSUBJECT_ITEM> GetGroupSubjectItem()
        {
            return _repoGroupSubjectItem.GetAll();
        }

        public IQueryable<CAT_GROUPSUBJECT_ITEM> GetGroupSubjectItem(Expression<Func<CAT_GROUPSUBJECT_ITEM, bool>> query)
        {
            return query == null
                ? _repoGroupSubjectItem.GetAll()
                : _repoGroupSubjectItem.GetAll(query);
        }

        public void UpdateGroupSubjectItem(CAT_GROUPSUBJECT_ITEM entity)
        {
            _repoGroupSubjectItem.Update(entity);
            Uow.SaveChanges();
        }

        public void InsertGroupSubjectItem(CAT_GROUPSUBJECT_ITEM entity)
        {
            _repoGroupSubjectItem.Insert(entity);
            Uow.SaveChanges();
        }

        public void DeleteGroupSubjectItem(IEnumerable<CAT_GROUPSUBJECT_ITEM> entities)
        {
            _repoGroupSubjectItem.Delete(entities);
            Uow.SaveChanges();
        }


        public Subject_Score GetScoreById(int subjectScoreId)
        {
            return _repoSubjectScore.Get(subjectScoreId);
        }

        public Subject_Score GetScore(int subjectId, string grade = "Pass")
        {
            return _repoSubjectScore.Get(a => a.SubjectDetail.IsDelete==false && a.subject_id == subjectId && a.grade.Equals(grade));
        }

        public IQueryable<Subject_Score> GetScores(Expression<Func<Subject_Score, bool>> query = null)
        {
            var entities = _repoSubjectScore.GetAll(a => a.SubjectDetail.IsDelete==false);
            if (query != null) entities = entities.Where(query);
            return entities;
        }

        public void InsertScore(Subject_Score entity)
        {
            _repoSubjectScore.Insert(entity);
            Uow.SaveChanges();
        }

        public void UpdateScore(Subject_Score entity)
        {
            _repoSubjectScore.Update(entity);
            Uow.SaveChanges();
        }
        public static object[] SetObjectNull(object[] arr_coursefrom)
        {
            if (!IsNull(arr_coursefrom))
            {
                for (int i = 0; i < arr_coursefrom.Length; i++)
                {
                    if (IsNull(arr_coursefrom[i]))
                    {
                        arr_coursefrom[i] = null;
                    }
                    else
                    {
                        if (arr_coursefrom[i].ToString() == "-1")
                        {
                            arr_coursefrom[i] = null;
                        }
                    }
                }
            }
            return arr_coursefrom;
        }
        public static bool IsNull(object value)
        {
            bool result = true;

            if (value != DBNull.Value && value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = false;
            }

            return result;
        }

        public IQueryable<Subject_TrainingCenter> GetTrainingCenter(Expression<Func<Subject_TrainingCenter, bool>> query = null)
        {
            var entities = _repoSubject_TrainingCenter.GetAll();
            if (query != null) entities = entities.Where(query);
            return entities;
        }
    }
}
