
using DAL.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Courses;

namespace TrainingCenter.Controllers
{
    public class TraineePortalController : BaseAdminController
    {
        private ICourseService _repoCourse;
        private IEmployeeService _employeeService;
        private ICourseDetailService _courseDetailService;
        private ICourseMemberService _courseMemberService;
        private ISubjectService _subjectService;
        private IApproveService _repoTmsApproves;

        public TraineePortalController(IConfigService configService,
            IUserContext userContext,
            INotificationService notificationService,
            ICourseMemberService courseMemberService,
            IEmployeeService employeeService,
            ICourseDetailService courseDetailService,
            IApproveService repoTmsApproves,
            ICourseService repoCourse,
            ISubjectService subjectService,
            IDepartmentService repoDepartment)
            : base(configService,
                userContext,
                notificationService,
                courseMemberService,
                employeeService,
                courseDetailService,
                repoDepartment,
                repoCourse,
                repoTmsApproves)
        {
            _repoTmsApproves = repoTmsApproves;
            this._repoCourse = repoCourse;
            this._employeeService = employeeService;
            this._courseDetailService = courseDetailService;
            this._courseMemberService = courseMemberService;
            this._subjectService = subjectService;
        }

        // GET: TraineePortal
        public ActionResult Index()
        {
            var model = new TraineePortalViewModel();
            return View(model);
        }

        public void EntityToModel(Trainee_Portal entity, TraineePortalViewModel model)
        {
            model.Id = entity.Id;
            model.CourseID = entity.CourseID;
            model.TraineeID = entity.TraineeID;
            model.SubjectCode = entity.SubjectCode;
            model.CourseCode = entity.CourseCode;
            model.TraineeFullName = entity.TraineeFullName;
            model.IsDeleted = entity.IsDeleted;
            model.IsActive = entity.IsActive;
            model.TraineeUserName = entity.TraineeUserName;
            model.SubjectName = _subjectService.GetSDetailByCode(entity.SubjectCode).Name;
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();

                var lstCourseTrainee = _repoCourse.GetListCourseTrainne(fStaffId);
                List<TraineePortalViewModel> lstModel = new List<TraineePortalViewModel>();
                foreach (var item in lstCourseTrainee)
                {
                    var model = new TraineePortalViewModel();
                    EntityToModel(item, model);
                    lstModel.Add(model);
                }

                IEnumerable<TraineePortalViewModel> filtered = lstModel;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TraineePortalViewModel, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.TraineeUserName : "");

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                      : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c.TraineeUserName ?? "",
                                    c.TraineeFullName ?? "",
                                    c.CourseCode ?? "",
                                     c.SubjectName ?? "",
                                    StatusEnroll(c.IsActive),
                                    (c.IsDeleted == true ? "&nbsp;<i class='fa fa-toggle-on' onclick='Remove_Trainee_Enroll("+c.Id+")' aria-hidden='true' title='remove enroll' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-off'  onclick='Set_Trainee_Enroll("+c.Id+")' aria-hidden='true' title='enroll' style='cursor: pointer;'></i>") ,
                                    ((User.IsInRole("/TraineePortal/Delete")) ? "<a title='Delete' href='javascript:void(0)'  onclick='callDelete(" + c.Id  + ")'><i class='fas fa-trash' aria-hidden='true' style=' font-size: 16px; color: black; '></i></a>" :""),
                                     "<input type='checkbox' value='"+c.Id+"' class='chk'>"
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
                JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public void AddtoLogFile(string filename, string msg)
        {
            // string filename = "tms_log.txt";

            string path = System.IO.Path.Combine(Server.MapPath("~/Template"), filename);

            if (System.IO.File.Exists(path))
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path, true))
                {
                    writer.WriteLine(msg);
                }
            }
            else
            {
                System.IO.StreamWriter writer = System.IO.File.CreateText(path);
                writer.WriteLine(msg);
                writer.Close();
            }
        }

        private string StatusEnroll(int? active)
        {
            StringBuilder HTML = new StringBuilder();
            var class_status = "none";
            var name = "";
            if (active == 0)
            {
                class_status = "danger";
                name = "New Register";
            }
            else if (active == 1)
            {
                class_status = "success";
                name = "Enrolled";
            }
            else if (active == 2)
            {
                class_status = "warning";
                name = "Admin Remove";
            }

            HTML.AppendFormat("<span class='label label-{0}'>{1}</span>", class_status, name);
            return HTML.ToString();
        }
        [HttpPost]
        public ActionResult RemoveEnroll(int id)
        {
            try
            {
                var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);
                if (courseTrainee == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                var removeTrainee = new TMS_Course_Member() { Member_Id = courseTrainee.TraineeID };
                //CourseMemberService.Get(a => a.Member_Id == id && a.Course_Details_Id == course).FirstOrDefault();
                CourseService.Delete(removeTrainee, CurrentUser.USER_ID.ToString(), courseTrainee.CourseID.GetValueOrDefault());

                courseTrainee.IsActive = 2;
                courseTrainee.IsDeleted = false;
                _repoCourse.UpdateCourseTrainnePending(courseTrainee);

                List<Trainee_Portal> lstPending = new List<Trainee_Portal>();
                lstPending.Add(courseTrainee);
               

                var trainee = _employeeService.GetById(courseTrainee.TraineeID);

                //if (CheckUserEnrol(courseTrainee.CourseCode, Session["auth_token"].ToString(), courseTrainee.CourseName, trainee.str_Staff_Id))
                //{
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format("Successfully!!!"),
                    result = true
                });


            }
            catch
            {
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format("Error!!!"),
                    result = false
                });
            }

        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);
                var trainee = _employeeService.GetById(courseTrainee.TraineeID);

                if (courseTrainee.IsDeleted == false)
                {
                    ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 0);

                    _repoCourse.RemoveCourseTrainnePending(courseTrainee);
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format("Successfully!!!"),
                        result = true
                    });
                }
                else
                {
                    courseTrainee.IsDeleted = false;
                    _repoCourse.UpdateCourseTrainnePending(courseTrainee);

                    List<Trainee_Portal> lstPending = new List<Trainee_Portal>();
                    lstPending.Add(courseTrainee);
                    SyncEnrollToLMS(lstPending, true);

                    ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 3);

                    _repoCourse.RemoveCourseTrainnePending(courseTrainee);
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format("Successfully!!!"),
                        result = true
                    });

                }


            }
            catch (Exception e)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(e.ToString()),
                    result = false
                });
            }
        }

        [HttpPost]
        public ActionResult Enroll(int id)
        {
            try
            {
                //course name la subject code
                var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);
                if (courseTrainee == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                var trainee = _employeeService.GetById(courseTrainee.TraineeID);
                //   var subjectDetail = _subjectService.GetSubjectDetailById(courseTrainee.CourseID.GetValueOrDefault());


                var lstCourseDetail = _courseDetailService.Get(a => a.CourseId == courseTrainee.CourseID)
                       .Select(p => p.Id)
                       .Distinct().ToArray();

                for (int i = 0; i < lstCourseDetail.Length; i++)
                {
                    int courseDetailId = int.Parse(lstCourseDetail[i].ToString());
                    var removeTrainee = new TMS_Course_Member() { Member_Id = trainee.Id };
                    CourseService.Insert(removeTrainee, CurrentUser.USER_ID.ToString(), courseDetailId,
                                      courseTrainee.CourseID.GetValueOrDefault());
                    //Thread.Sleep(5);
                }


                List<Trainee_Portal> lstPending = new List<Trainee_Portal>();
                lstPending.Add(courseTrainee);
                SyncEnrollToLMS(lstPending, false);



                if (CheckUserEnrol(courseTrainee.CourseCode, Session["auth_token"].ToString(), courseTrainee.SubjectCode, trainee.str_Staff_Id))
                {
                    ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 1);

                    courseTrainee.IsActive = 1;
                    courseTrainee.IsDeleted = true;
                    _repoCourse.UpdateCourseTrainnePending(courseTrainee);

                    //var course = CourseService.GetByCourseCode(courseTrainee.CourseCode);

                    //Sent_Email_TMS(null, trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee);

                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.APPROVE_TRAINEE_COURSE, trainee.str_Staff_Id),
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.APPROVE_TRAINEE_COURSE_ERROR, trainee.str_Staff_Id),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format("Error!!!"),
                    result = false
                });
            }

        }

        [HttpPost]
        public ActionResult Approved(int id)
        {
            try
            {
                var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);
                if (courseTrainee == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                var trainee = _employeeService.GetById(courseTrainee.TraineeID);
                var subjectDetail = _subjectService.GetSubjectDetailById(courseTrainee.CourseID.GetValueOrDefault());

                var lstCourseDetail = _courseDetailService.Get(a => a.CourseId == courseTrainee.CourseID)
                       .Select(p => p.Id)
                       .Distinct().ToArray();

                for (int i = 0; i < lstCourseDetail.Length; i++)
                {
                    int courseDetailId = int.Parse(lstCourseDetail[i].ToString());
                    var removeTrainee = new TMS_Course_Member() { Member_Id = trainee.Id };
                    CourseService.Insert(removeTrainee, CurrentUser.USER_ID.ToString(), courseDetailId,
                                      courseTrainee.CourseID.GetValueOrDefault());
                    Thread.Sleep(5);
                }


                List<Trainee_Portal> lstPending = new List<Trainee_Portal>();
                lstPending.Add(courseTrainee);
                SyncEnrollToLMS(lstPending, false);



                if (CheckUserEnrol(courseTrainee.CourseCode, Session["auth_token"].ToString(), courseTrainee.SubjectCode, trainee.str_Staff_Id))
                {
                    ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 1);

                    _repoCourse.DeleteCourseTrainnePending(courseTrainee);
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.APPROVE_TRAINEE_COURSE, trainee.str_Staff_Id),
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.APPROVE_TRAINEE_COURSE_ERROR, trainee.str_Staff_Id),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult ApproveAll(string checkListId)
        {
            try
            {

                var listID = new JavaScriptSerializer().Deserialize<List<int>>(checkListId);
                if (listID.Count == 0)
                {
                    var lstCourseTrainee = _repoCourse.GetListCourseTrainne();
                    List<int> lstID = new List<int>();
                    foreach (var item in lstCourseTrainee)
                    {
                        lstID.Add(item.Id);
                    }
                    foreach (var id in lstID)
                    {
                        var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);


                        var trainee = _employeeService.GetById(courseTrainee.TraineeID);
                        var course = _repoCourse.GetById(courseTrainee.CourseID);

                        var lstCourseDetail = _courseDetailService.Get(a => a.CourseId == courseTrainee.CourseID)
                               .Select(p => p.Id)
                               .Distinct().ToArray();

                        for (int i = 0; i < lstCourseDetail.Length; i++)
                        {
                            int courseDetailId = int.Parse(lstCourseDetail[i].ToString());
                            var removeTrainee = new TMS_Course_Member() { Member_Id = trainee.Id };
                            CourseService.Insert(removeTrainee, CurrentUser.USER_ID.ToString(), courseDetailId,
                                              courseTrainee.CourseID.GetValueOrDefault());
                            Thread.Sleep(5);
                        }

                        SyncEnrollToLMS(lstCourseTrainee, false);
                        ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 1);


                        _repoCourse.DeleteCourseTrainnePending(courseTrainee);
                    }
                }
                else
                {
                    foreach (var id in listID)
                    {
                        var courseTrainee = _repoCourse.GetByCourseAndTrainneById(id);


                        var trainee = _employeeService.GetById(courseTrainee.TraineeID);
                        var course = _repoCourse.GetById(courseTrainee.CourseID);

                        var lstCourseDetail = _courseDetailService.Get(a => a.CourseId == courseTrainee.CourseID)
                               .Select(p => p.Id)
                               .Distinct().ToArray();

                        for (int i = 0; i < lstCourseDetail.Length; i++)
                        {
                            int courseDetailId = int.Parse(lstCourseDetail[i].ToString());
                            var removeTrainee = new TMS_Course_Member() { Member_Id = trainee.Id };
                            CourseService.Insert(removeTrainee, CurrentUser.USER_ID.ToString(), courseDetailId,
                                              courseTrainee.CourseID.GetValueOrDefault());
                            Thread.Sleep(5);
                        }

                        List<Trainee_Portal> lstPending = new List<Trainee_Portal>();
                        lstPending.Add(courseTrainee);
                        SyncEnrollToLMS(lstPending, false);

                        Thread.Sleep(5);
                        ApprovedForPortal(trainee.str_Staff_Id, courseTrainee.CourseCode, courseTrainee.SubjectCode, 1);


                        _repoCourse.DeleteCourseTrainnePending(courseTrainee);
                    }
                }

                // SyncDataToLMS();
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.APPROVE_TRAINEE_COURSE, ""),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }



    }
}