﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Serveices;
using System.Text;
using ClosedXML.Excel;
using DAL.Entities;
using TMS.Core.ViewModels.AjaxModels.AjaxTMS_APPROVE;
using TMS.Core.ViewModels.Approve;
using TMS.Core.ViewModels.Common;

namespace TrainingCenter.Controllers
{
    using global::Utilities;
    using System.Configuration;
    using System.IO;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.Services.Subject;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ReportModels;
    using Utilities;
    using TMS.Core.ViewModels.Courses;
    using System.Drawing;
    using NReco.ImageGenerator;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using TMS.Core.ViewModels.ViewModel;
    using System.Diagnostics;
    using System.Web;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class ApproveController : BaseAdminController
    {
        #region Init
        private readonly IUserService _repoUser;
        private readonly IApproveService _repoApprove;
        private readonly IApproveService _approveService;
        private readonly IJobtitleService _repoJobtitle;
        private readonly ISubjectService _repoSubject;
        #endregion
        #region Index
        public ApproveController(IConfigService configService, IUserContext userContext, IApproveService approveService, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService repoApprove, IUserService repoUser, IJobtitleService repoJobtitle, ISubjectService repoSubject) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, repoApprove)
        {
            _repoApprove = repoApprove;
            _approveService = approveService;
            _repoUser = repoUser;
            _repoJobtitle = repoJobtitle;
            _repoSubject = repoSubject;
        }
        public ActionResult Index()
        {
            var model = new ApproveIndex();
            #region[Status]
            //var status = _repoApprove.GetApproveStatus().Where(item => item.Value != "1" && item.Value.Trim() != "Complete");
            //var selectListItems = status as SelectListItem[] ?? status.ToArray();
            //if (selectListItems.Any())
            //{
            //    foreach (var item in selectListItems.Where(item => item.Value.Trim() == "Block"))
            //    {
            //        item.Value = "Edit";
            //    }
            //}
            #endregion
            //ViewBag.Status = new SelectList(selectListItems, "Value", "Text");

            model.Status = UtilConstants.EStatusTypeDictionary();
            model.Types = UtilConstants.ApproveTypeDictionary();

            #region[ApproveType]
            var approveType = _repoApprove.GetApproveTypes();
            if (approveType.Any())
            {
                foreach (var item in approveType)
                {
                    switch (item.Value)
                    {
                        case "AssignTrainee":
                            item.Value = "Trainee";
                            break;
                        case "SubjectResult":
                            item.Value = "Result";
                            break;
                        case "CourseResult":
                            item.Value = "Final";
                            break;
                    }
                }
            }
            #endregion


            // ViewBag.CourseTypeList = approveType;// new SelectList(ApproveType, "id", "Name");

            return View(model);
        }
        public ActionResult Log(int? id)
        {
            var deps = _repoApprove.GetById(id);
            var model = new ApproveLogViewModel();
            var department = DepartmentService.Get().Select(a => a.Id);
            model.data = _repoApprove.GetLogs(deps.int_Type, deps.int_Course_id, deps.int_courseDetails_Id).OrderByDescending(a => a.id).ToList();
            model.approveHis = _repoApprove.GetHistory(a => a.int_Course_id == deps.int_Course_id && a.int_Type == deps.int_Type).OrderByDescending(a => a.id).ToList();
            model.approveuser = UserContext.Get(null, department).ToList();
            model.approveStatus = UtilConstants.EStatusTypeDictionary();
            return View(model);
        }
        public ActionResult AjaxHandlerList(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseType = Request.QueryString.GetValues("CourseType[]");
                var statusList = string.IsNullOrEmpty(Request.QueryString["StatusList"]) ? -1 : Convert.ToInt32(Request.QueryString["StatusList"].Trim());
                var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var courseTypes = courseType?.Select(a => !string.IsNullOrEmpty(a) ? Convert.ToInt32(a) : 0).ToList() ?? new List<int>();
                var Stringparam = "?valueparam=&coursetype=" + (courseType != null ? string.Join(",", courseTypes) : "") + "&statuslist=" + statusList + "&code=" + fCode + "&fName=" + fName;
                var ApprovalID = string.IsNullOrEmpty(Request.QueryString["ApprovalID"]) ? -1 : Convert.ToInt32(Request.QueryString["ApprovalID"].Trim());
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                IEnumerable<Course> filtered = null;

                if (statusList == -1 && courseTypes.Count() == 0)
                {
                    filtered = CourseService.Get(a => 
                    (string.IsNullOrEmpty(fCode) && string.IsNullOrEmpty(fName) ? a.StartDate >= timenow : true) &&
                    a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id) && (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) && (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)),true);

                }
                else if (statusList != -1 && courseTypes.Count() > 0)
                {
                    filtered = CourseService.Get(a =>
                      (string.IsNullOrEmpty(fCode) && string.IsNullOrEmpty(fName) ? a.StartDate >= timenow : true) &&
                    a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id) && (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) && (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)) && a.TMS_APPROVES.FirstOrDefault(x => x.int_Type.HasValue && courseTypes.Contains(x.int_Type.Value)).int_id_status == statusList,true);

                }
                else if (statusList == -1)
                {
                    filtered = CourseService.Get(a =>
                    (string.IsNullOrEmpty(fCode) && string.IsNullOrEmpty(fName) ? a.StartDate >= timenow : true) &&
                    a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id) && (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) && (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
                          courseTypes.Contains(a.TMS_APPROVES.Any() ? a.TMS_APPROVES.FirstOrDefault().int_Type.Value : 0),true);
                }
                else
                {
                    filtered = CourseService.Get(a =>
                     (string.IsNullOrEmpty(fCode) && string.IsNullOrEmpty(fName) ? a.StartDate >= timenow : true) &&
                    a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.int_id_status == statusList) && (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) && (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)),true);

                }

                if (ApprovalID != -1)
                {
                    filtered = filtered.Where(a => a.TMS_APPROVES.Any(b => b.id == ApprovalID));
                }


                //var filtered = CourseService.Get(true).Where(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id) && (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) && (string.IsNullOrEmpty(fName) || a.Name.Contains(fName))).Where(FilterCourseStatus(courseTypes, statusList));

                //var filtered = CourseService.Get(a => 
                //    a.Course_TrainingCenter.Any(b => CurrentUser.PermissionIds.Any(c => c == b.khoidaotao_id) &&
                //    (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) &&
                //    (string.IsNullOrEmpty(fName) || a.Name.Contains(fName))
                //    ), true
                //    ).Where(FilterCourseStatus(courseTypes, statusList));

                filtered = filtered.OrderByDescending(c => c.TMS_APPROVES.FirstOrDefault().id);
                //var enumerable = filtered;
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                 string.Empty,
                              ReturnColumn(c,Stringparam).ToString()
                              //RenderRazorViewToString("_Partials/_ProccessBar",c)
                        };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AjaxHandlerList", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public ActionResult AjaxHandlerlistsubject(jQueryDataTableParamModel param)
        {
            var courseId = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
            var courseType = Request.QueryString.GetValues("CourseType[]");
            var statusList = string.IsNullOrEmpty(Request.QueryString["StatusList"]) ? -1 : Convert.ToInt32(Request.QueryString["StatusList"].Trim());
            var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var courseTypes = courseType?.Select(a => !string.IsNullOrEmpty(a) ? Convert.ToInt32(a) : 0).ToList() ?? new List<int>();
            var Stringparam = "?valueparam=&coursetype=" + (courseType != null ? string.Join(",", courseTypes) : "") + "&statuslist=" + statusList + "&code=" + fCode + "&fName=" + fName;
            try
            {
                var data =
                    _repoApprove.Get(
                        a => a.int_Course_id == courseId && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.Course_Detail.IsDeleted != true)
                        .ToList();
                var verticalBar = GetByKey("VerticalBar");
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                IEnumerable<TMS_APPROVES> filtered = data;
                Func<TMS_APPROVES, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Course.Code
                                              : sortColumnIndex == 2 ? c.Course.Name
                                              : c?.Course_Detail.dtm_time_from.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                        : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from a in displayed
                             select new object[] {
                       string.Empty,
                       "<a href='" + @Url.Action("SubjectResult", "Approve", new { a?.id }) + (string.IsNullOrEmpty(Stringparam) ? "" : Stringparam) + "'>"+

                       ( a?.Course_Detail?.SubjectDetail?.IsActive != true ? "<span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ a?.Course_Detail?.SubjectDetail?.Code + "</span> - <span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ a?.Course_Detail?.SubjectDetail?.Name + "</span>" : a?.Course_Detail?.SubjectDetail?.Code + " - " + a?.Course_Detail?.SubjectDetail?.Name) +

                       "</a>" + " " + ReturnStatus(a?.int_id_status),
                       string.Format("{0:dd/MM/yyyy}", a?.Course_Detail?.dtm_time_from.Value) + " - " + string.Format("{0:dd/MM/yyyy}", a?.Course_Detail?.dtm_time_to.Value),
                       //(a?.Date_Requested.HasValue == true ? string.Format("{0:dd/MM/yyyy}", a?.Date_Requested.Value) : string.Empty) + " - " + a?.USER1.LASTNAME + " "+  a?.USER1.FIRSTNAME,
                        string.IsNullOrEmpty(a?.Course_Detail?.str_remark ) ?"": Regex.Replace(a?.Course_Detail?.str_remark , "[\r\n]", "<br/>"),
                         ReturnColumnOptionInSubject(a?.int_Course_id, a?.int_courseDetails_Id, a?.int_id_status, a?.id, Stringparam, a.int_Type , (int)UtilConstants.ApproveType.SubjectResult) + verticalBar + ReturnColumnLog(a?.id, a?.int_Type),



                           //c?.Course,
                           //c?.FromTo,
                           //c?.RequestBy+ " - " + ReturnDisplayLanguage(c?.firstName,c?.lastName),
                           //c?.Remark,
                           //c?.Action
                                 };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AjaxHandlerlistsubject", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        private StringBuilder ReturnStatus(int? eStatus)
        {
            var html = new StringBuilder();
            switch (eStatus)
            {
                case (int)UtilConstants.EStatus.Pending:
                    html.Append(
                        " - <i class='zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i> <span class='label label-default'>" + Resource.lblPending + "</span>");
                    break;
                case (int)UtilConstants.EStatus.Approve:
                    html.Append(
                        " - <i class='fa fa-check i-in-bs-wizard' aria-hidden='true'></i> <span class='label label-success'>" + Resource.lblApproved + "</span>");
                    break;
                case (int)UtilConstants.EStatus.Reject:
                    html.Append(
                        " - <i class='fa fa-times' aria-hidden='true'></i> <span class='label label-danger'>" + Resource.lblReject + "</span>");
                    break;
                case (int)UtilConstants.EStatus.Block:
                    html.Append(
                        " - <i class='fa fa-unlock btnIcon_orange' aria-hidden='true'></i> <span class='label label-warning'>" + Resource.lblUnBlocked + "</span>");
                    break;
                default:
                    html.Append(
                        " - <i class='zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i> <span class='label label-primary'>" + Resource.lblProcessing + "</span>");
                    break;
            }
            return html;
        }
        private StringBuilder ReturnColumn(Course data, string param = "")
        {
            var html = new StringBuilder();
            if (data == null) return html;

            html.Append("<div class='col-md-12 notice notice-info course-detail-0'>");
            html.AppendFormat("<div class='hidden-xs col-md-3 col-xs-12'><b>" + Resource.lblCode + ": </b> {0}</div><div class='hidden-xs col-md-9  col-xs-12'><b>" + Resource.lblFromTo + ": </b>{1} - {2}</div>", data.Code, DateUtil.DateToString(data.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(data.EndDate, "dd/MM/yyyy"));
            html.AppendFormat("<div class='visible-xs col-md-3 col-xs-12' style='font-size: 11px;'><b>" + Resource.lblCode + ": </b> {0}</div><div class='visible-xs col-md-9  col-xs-12'  style='font-size: 11px;margin-bottom: 5px;'><b>" + Resource.lblFromTo + ": </b>{1} - {2}</div>", data.Code, DateUtil.DateToString(data.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(data.EndDate, "dd/MM/yyyy"));
            html.AppendFormat("<div class='hidden-xs col-md-12'><h4><b>{0}</b></h4></div>", data.Name);
            html.AppendFormat("<div class='visible-xs  col-md-12' style='margin-bottom: 15px;'><h5><b>{0}</b></h5></div>", data.Name);
            html.AppendFormat("{0}", ReturnProcessSteps(data, param));
            html.Append("</div>");

            return html;
        }
        [AllowAnonymous]
        public string CheckFile(int approvalId)
        {
            try
            {
                var sda = CourseService.Get(true).FirstOrDefault(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.id == approvalId));
                var list = sda != null ? ReturnProcessSteps(sda) : null;


                return list.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private StringBuilder ReturnProcessSteps(Course course, string param = "")
        {
            var html = new StringBuilder();
            html.Append("<div class='row bs-wizard' style='border-bottom:0;'>");
            //complete active disabled
            var dataTypeApprovalType = UtilConstants.ApproveTypeDictionary();

            var data_TMS_APPROVES = course.TMS_APPROVES;
            int Count_SubjectApproved = _repoApprove.Get(a => a.int_Course_id == course.Id && a.int_courseDetails_Id != null, (int)UtilConstants.ApproveType.SubjectResult).Count();
            int Count_SubjectDetail = CourseDetailService.GetByCourse(course.Id).Count();
            foreach (var item in dataTypeApprovalType) // type course,trainee,subject,final
            {
                #region[check process]

                var key = item.Key;
                var requestInfo = "";
                var status = "disabled";//disabled  active complete
                var icon = "";
                var display = "none";
                var iconEdit = "";
                var labelStatus = "";
                var class_status = "";
                var log = "";
                var data = data_TMS_APPROVES.FirstOrDefault(a => a.int_Type == key);
                if (data != null)
                {
                    log = ReturnColumnLog(data.id, data.int_Type);

                    requestInfo = "<p style='font-size:10px;    margin-bottom: 0px;'><i class='fa fa-user' aria-hidden='true'></i>  " + (ReturnDisplayLanguage(data.USER1?.FIRSTNAME, data.USER1?.LASTNAME)) + "</p><p style='font-size:10px;    margin-bottom: 0px;'>  <i class='far fa-clock' aria-hidden='true'></i>  " + (data.Date_Requested.HasValue ? data.Date_Requested?.ToString("dd/MM/yyyy") : data.dtm_requested_date?.ToString("dd/MM/yyyy")) + "</p>";
                    switch (data.int_id_status)
                    {
                        case (int)UtilConstants.EStatus.Approve:
                            status = "complete approval ";
                            class_status = "success";
                            icon = "<i class='visible-xs fa fa-check i-in-bs-wizard' style='font-size:12px;' aria-hidden='true'></i><i class='hidden-xs fa fa-check i-in-bs-wizard' aria-hidden='true'></i>";
                            display = "block";
                            if (key != (int)UtilConstants.ApproveType.AssignedTrainee)//(key != (int)UtilConstants.ApproveType.AssignedTrainee)
                            {
                                iconEdit = (User.IsInRole("/Approve/" + item.Value + "") ? "<a href='javascript:void(0)' onclick='RemoveApprove(" + data.int_Course_id + "," + key + ")'><i class='fa fa-unlock btnIcon_orange'></i></a>" : "");
                            }

                            labelStatus = "<span class='label label-success hidden-xs'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Pending:
                            class_status = "default";
                            status = " pending ";
                            if (Count_SubjectApproved < Count_SubjectDetail)
                            {
                                icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                            }
                            display = "block";
                            iconEdit = User.IsInRole("/Approve/" + item.Value + "") ? "<a href='" + @Url.Action("" + item.Value + "", "Approve", new { data.id }) + (string.IsNullOrEmpty(param) ? "" : param) + "'><i class='fas fa-edit btnIcon_green'></i></a>" : "";
                            labelStatus = "<span class='label label-default hidden-xs  '>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Block:
                            class_status = "warning";
                            status = " block ";
                            labelStatus = "<span class='label label-warning hidden-xs' aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Reject:
                            status = " reject ";
                            labelStatus = "<span class='label label-danger hidden-xs' aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.CancelRequest:
                            status = " CancelRequest ";
                            labelStatus = "<span class='label label-blueviolet hidden-xs' aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            display = "block";
                            if (key != (int)UtilConstants.ApproveType.AssignedTrainee)//(key != (int)UtilConstants.ApproveType.AssignedTrainee)
                            {
                                iconEdit = User.IsInRole("/Approve/" + item.Value + "") ? "<a href='javascript:void(0)' onclick='RemoveApprove(" + data.int_Course_id + "," + key + ")'><i class='fa fa-unlock btnIcon_orange'></i></a>" : "";
                            }

                            break;
                    }
                }

                #region [đặt lại tên]
                var name = "";
                if (item.Value.Any())
                {
                    switch (item.Value)
                    {
                        case "AssignTrainee":
                            name = Resource.lblTRAINEE;
                            break;
                        case "SubjectResult":
                            name = Resource.lblRESULT;
                            break;
                        case "CourseResult":
                            name = Resource.lblFinal;
                            break;
                        case "Course":
                            name = Resource.lblCourse;
                            break;
                    }
                }
                #endregion

                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    iconEdit = "<span data-value='" + course.Id + "' class='expand' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true' style='font-size: 16px; color: green; '></i></span>";
                    log = "";
                    var db = data_TMS_APPROVES.Where(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (db.Any())
                    {
                        foreach (var item2 in db)
                        {
                            if (item2.int_id_status != (int)UtilConstants.EStatus.Approve)
                            {
                                class_status = "primary";
                                display = "block";
                                status = " processing ";
                                labelStatus = "<span class='label label-primary hidden-xs  'aria-hidden='true'>Processing</span>";
                                if (Count_SubjectApproved < Count_SubjectDetail)
                                {
                                    icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                                }
                                break;
                            }
                        }
                    }

                }
                var url = "javascript:void(0)";
                if (item.Value != "SubjectResult")
                {

                    url = "/Approve/" + item.Value + "/" + data?.id + "" + (string.IsNullOrEmpty(param) ? "" : param);
                }
                #endregion
                var htmlstyle = "<p class='' style='top: 22%; position: absolute; left: 56%;font-size: 16px;'>{0}&nbsp;&nbsp;{1}</p> ";
                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    log = "";
                    requestInfo = "";
                    htmlstyle = "<p class='' style='top: 30%; position: absolute; left: 56%;font-size: 16px;'>{0}&nbsp;&nbsp;{1}</p> ";
                }

                html.AppendFormat("<div class='col-md-3 col-sm-3 col-xs-12 bs-wizard-step {0}'>", status);
                html.AppendFormat("<div class=' text-center bs-wizard-stepnum'><h4><b>{0}</b></h4></div>", name);
                html.Append("<div class='progress'><div class='progress-bar'></div></div>");
                html.AppendFormat(" <a href='{0}' class=' bs-wizard-dot'><div style='width: 75%; height: 75%; margin-top: 12%; margin-left: 13%; background: #4CAF50; border-radius: 50%;'><b class='' style='font-size: 22px; margin-left: 8px;'>{1}</b><b class='visible-xs' style='font-size: 16px; margin-left: 6px;'>{2}</b></div></a>", url, key, key);
                html.AppendFormat(htmlstyle, icon, labelStatus);
                html.AppendFormat("<div class='bs-wizard-info text-center'><div style='display:{0}'>{1}&nbsp;&nbsp;{2}</div>{3}</div>", display, iconEdit, log, requestInfo);
                html.AppendFormat("</div>");
            }
            html.Append("</div>");
            return html;
        }
        private string ReturnColumnOptionInSubject(int? courseId, int? courseDetailId, int? status, int? approveId, string param, int? laststep, int? stepNotEdit)
        {
            var html = string.Empty;
            switch (status)
            {
                case (int)UtilConstants.EStatus.Pending:
                    html += (User.IsInRole("/Approve/SubjectResult") ? "<a href='" + @Url.Action("SubjectResult", "Approve", new { id = approveId }) + (string.IsNullOrEmpty(param) ? "" : param) + "'><i class='fas fa-edit btnIcon_green' ></i></a>" : "");
                    break;
                case (int)UtilConstants.EStatus.Approve:
                    html += User.IsInRole("/Approve/SubjectResult")
                              ? "<a href='javascript:void(0)' onclick='RemoveApproveSubject(" + courseId + "," +
                                (int)UtilConstants.ApproveType.SubjectResult + "," + courseDetailId +
                                ")'><i class='fa fa-unlock btnIcon_orange'></i></a>"
                              : string.Empty;
                    break;
                default:
                    html += "";
                    break;
            }
            return html;
        }
        private string ReturnColumnLog(int? approveId, int? approveType = -1)
        {
            var _return = "";
            var approve = _repoApprove.GetById(approveId);
            if (approve != null)
            {
                var data = _repoApprove.GetLogs(approveType, approve.int_Course_id).OrderByDescending(a => a.id);
                if (data.Any())
                {
                    _return = "<a href='" + @Url.Action("Log", "Approve", new { id = approveId }) + "'><i class='fa fa-sticky-note btnIcon_gray'></i></a>";
                }
            }

            return _return;
        }
        private bool CheckApprove(int courseId, UtilConstants.ApproveType approveType, UtilConstants.EStatus approveStatus)
        {
            var tmsApprove = _approveService.Get(a => a.int_Course_id == courseId && a.int_Type == (int)approveType && a.int_id_status == (int)approveStatus);
            if (tmsApprove.Any())
                return true;
            return false;
        }
        #endregion
        #region[cancel approval]
        [HttpPost]
        public async Task<ActionResult> RemoveApproval(int id = -1, int type = -1, string feedback = "")
        {
            try
            {
                // id = courseId

                //var checkApproved = CheckApprove(id, UtilConstants.ApproveType.CourseResult,
                //    UtilConstants.EStatus.Approve);
                //if (checkApproved)
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = Messege.VALIDATION_CANCEL_APPROVE,
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                var approve = _repoApprove.Get(id, type);
                if (approve != null)
                {
                    var step = UtilConstants.ApproveType.Course;
                    var notiTemplate = string.Empty;
                    var notiTemplateVn = string.Empty;
                    var notiContent = string.Empty;
                    var notiContentVn = string.Empty;
                    switch (type)
                    {
                        case (int)UtilConstants.ApproveType.Course:
                            step = UtilConstants.ApproveType.Course;
                            notiTemplate = UtilConstants.NotificationTemplate.UnBlockCourse;
                            notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockCourseVn;
                            notiContent = UtilConstants.NotificationContent.UnBlockCourse;
                            notiContentVn = UtilConstants.NotificationContent.UnBlockCourseVn;
                            break;
                        case (int)UtilConstants.ApproveType.AssignedTrainee:
                            step = UtilConstants.ApproveType.AssignedTrainee;
                            notiTemplate = UtilConstants.NotificationTemplate.UnBlockAssignTrainee;
                            notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockAssignTraineeVn;
                            notiContent = UtilConstants.NotificationContent.UnBlockAssign;
                            notiContentVn = UtilConstants.NotificationContent.UnBlockAssignVn;
                            break;
                        case (int)UtilConstants.ApproveType.SubjectResult:
                            step = UtilConstants.ApproveType.SubjectResult;
                            notiTemplate = UtilConstants.NotificationTemplate.UnBlockSubjectResult;
                            notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockSubjectResultVn;
                            notiContent = UtilConstants.NotificationContent.UnBlockSubjectResult;
                            notiContentVn = UtilConstants.NotificationContent.UnBlockSubjectResultVn;
                            break;
                        case (int)UtilConstants.ApproveType.CourseResult:
                            step = UtilConstants.ApproveType.CourseResult;
                            notiTemplate = UtilConstants.NotificationTemplate.UnBlockFinal;
                            notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockFinalVn;
                            notiContent = UtilConstants.NotificationContent.UnBlockFinal;
                            notiContentVn = UtilConstants.NotificationContent.UnBlockFinalVn;
                            break;
                    }
                    var approveSuccess = _repoApprove.Modify(true, approve.Course, step,
                         UtilConstants.EStatus.Block, UtilConstants.ActionType.Approve);
                    if (approveSuccess != null)
                    {
                        SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)step, approveSuccess.id, (approveSuccess.int_Requested_by ?? 7), DateTime.Now, notiTemplate,
                       string.Format(notiContent, approveSuccess.Course.Code + " - " + approveSuccess.Course.Name, feedback),
                       notiTemplateVn,
                       string.Format(notiContentVn, approveSuccess.Course.Code + " - " + approveSuccess.Course.Name, feedback));

                        //await Task.Run(() =>
                        //{
                        //    #region [--------CALL LMS (CRON PROGRAM)----------]
                        //    var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                        //    if (!callLms)
                        //    {

                        //        //return Json(new AjaxResponseViewModel()
                        //        //{
                        //        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lbl_SENT, "request"),
                        //        //    result = false
                        //        //}, JsonRequestBehavior.AllowGet);
                        //    }
                        //    #endregion

                        //});
                        return Json(new AjaxResponseViewModel()
                        {
                            message = Messege.WARNING_SENT_REQUEST_SUCCESS,
                            result = true
                        }, JsonRequestBehavior.AllowGet);

                    }


                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/RemoveApproval", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.UNSUCCESS,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult RemoveApprovalSubject(int courseid = -1, int type = -1, int courseDetailId = -1, string feedback = "")
        {
            try
            {
                // id = Course_Detail_Id

                //var checkApproved = CheckApprove(courseid, UtilConstants.ApproveType.CourseResult,
                //    UtilConstants.EStatus.Approve);
                //if (checkApproved)
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = Messege.VALIDATION_CANCEL_APPROVE,
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                var approve = _repoApprove.Get(courseid, type, (int)UtilConstants.EStatus.Approve, courseDetailId);
                if (approve != null)
                {
                    var step = UtilConstants.ApproveType.SubjectResult;
                    const string notiTemplate = UtilConstants.NotificationTemplate.UnBlockSubjectResult;
                    const string notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockSubjectResultVn;
                    const string notiContent = UtilConstants.NotificationContent.UnBlockSubjectResult;
                    const string notiContentVn = UtilConstants.NotificationContent.UnBlockSubjectResultVn;

                    var approveSuccess = _repoApprove.Modify(true, approve.Course, step,
                         UtilConstants.EStatus.Block, UtilConstants.ActionType.Approve, courseDetailId, feedback);
                    if (approveSuccess != null)
                    {
                        //var courseDetailIds = approve.Course.Course_Detail.Select(a => a.Id);
                        var courseResultSummary =
                            CourseService.GetCourseResultSummaries(a => a.CourseDetailId == courseDetailId, true);
                        if (courseResultSummary.Any())
                        {
                            foreach (var summary in courseResultSummary)
                            {
                                Dictionary<string, object> dic = new Dictionary<string, object>();
                                dic.Add("LmsStatus", (int)UtilConstants.ApiStatus.Modify);
                                if (CMSUtils.UpdateDataSQLNoLog("Id", summary.Id + "", "Course_Result_Summary", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                {

                                }
                            }
                        }
                        SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)step, approveSuccess.id, (approveSuccess.int_Requested_by ?? -1), DateTime.Now, notiTemplate,
                       string.Format(notiContent, approve.Course_Detail.SubjectDetail.Code, approveSuccess.Course.Code + " - " + approveSuccess.Course.Name, feedback),
                       notiTemplateVn,
                       string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Code, approveSuccess.Course.Code + " - " + approveSuccess.Course.Name, feedback));

                        return Json(new AjaxResponseViewModel()
                        {
                            message = Messege.WARNING_SENT_REQUEST_SUCCESS,
                            result = true
                        });

                    }


                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/RemoveApprovalSubject", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.UNSUCCESS,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Approve Course  
        public ActionResult Course(int id)
        {
            var mModel = _repoApprove.GetById(id);

            #region[page load]
            ViewBag.CLQ = ReturnColumn(mModel.Course);
            ViewBag.Course_Detail_Instructor = CourseDetailService.GetDetailInstructors();// _repoCourse_Detail_Instructor.GetAll();
            ViewBag.ConfigSendMail = GetByKey(UtilConstants.KEY_SENDEMAILGV) ?? "0";
            #endregion

            return View(mModel);
        }
        [HttpPost]
        public async Task<ActionResult> Course(FormCollection form)
        {
            try
            {
                bool checkResp = string.IsNullOrEmpty(form["checkResp"]) ? false : true;
                var id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);
                var course = aModel.Course;
                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["RejectReason"]) ? string.Empty : form["RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;

                Modify_TMS(true, course, (int)UtilConstants.ApproveType.Course, status, UtilConstants.ActionType.Approve, note);
                var userID = CurrentUser.USER_ID;
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    UpdateStatus(course);
                    UpdateMember(course, userID);
                    #region [-----------------------Sent Mail Approve Course-----------------------------]
                    await Task.Run(() =>
                    {
                        
                        var usercheck = course.Course_Detail.Where(p => p.IsDeleted != true);
                        var body = ConfigService.GetMail();
                        var user_create = UserContext.GetById((int)aModel.int_Requested_by);
                        if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            #region [ ----- sent mail cho gv & GV mantor----- ]
                            if (checkResp)
                            {
                                if (usercheck.Count() > 0)
                                {
                                    var body_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedGV)?.Content;
                                    var cat_mail_ID = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedGV)?.ID;
                                    foreach (var item in usercheck)
                                    {
                                        var instructor_coursedetail = item.Course_Detail_Instructor;
                                        if (instructor_coursedetail.Count() > 0)
                                        {

                                            foreach (var details in instructor_coursedetail)
                                            {
                                                var instructor = details.Trainee;
                                                if (instructor != null)
                                                {
                                                    var bodysendmail = BodySendMail_Custom(body_, null, instructor, aModel.Course, null, 1);

                                                    Sent_Email_TMS_Custom(cat_mail_ID, bodysendmail, instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //var body__ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse)?.Content;
                            //var cat_mail_ID_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse)?.ID;
                            //var bodysendmail_ = BodySendMail_Custom(body__, user_create, null, course);
                            //Sent_Email_TMS_Custom(cat_mail_ID_, bodysendmail_, null, null, user_create, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                            #endregion
                        }
                        if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Reject)
                        {
                            #region [ ----- Reject sent mail cho gv & GV mantor ----- ]
                            if (checkResp)
                            {
                                if (usercheck.Count() > 0)
                                {
                                    var body_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectGV)?.Content;
                                    var cat_mail_ID = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectGV)?.ID;
                                    foreach (var item in usercheck)
                                    {
                                        var instructor_coursedetail = item.Course_Detail_Instructor;
                                        if (instructor_coursedetail.Any())
                                        {
                                            foreach (var details in instructor_coursedetail)
                                            {
                                                var instructor = details.Trainee;
                                                {
                                                    var bodysendmail = BodySendMail_Custom(body_, null, item.Trainee, aModel.Course, null, 1);
                                                    Sent_Email_TMS_Custom(cat_mail_ID, bodysendmail, instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.Reject);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //var body__ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectCourse)?.Content;
                            //var cat_mail_ID_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectCourse)?.ID;
                            //var bodysendmail_ = BodySendMail_Custom(body__, user_create, null, course);
                            //Sent_Email_TMS_Custom(cat_mail_ID_, bodysendmail_, null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.Reject);
                            #endregion
                        }
                    }).ConfigureAwait(false);

                    #endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.SUBMIT_SUCCESS, lblStatus),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/Course", ex.Message);

                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<ActionResult> ProccessingClient(FormCollection form, bool checkResp = true)
        {
            try
            {
                var id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);
                var course = aModel.Course;
                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["RejectReason"]) ? string.Empty : form["RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;
                var userID = CurrentUser.USER_ID;
                Modify_TMS(true, course, (int)UtilConstants.ApproveType.Course, status, UtilConstants.ActionType.Approve, note);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    UpdateStatus(course);
                    UpdateMember(course, userID);
                    #region [-----------------------Sent Mail Approve Course-----------------------------]
                    var usercheck = course.Course_Detail;

                    if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                    {
                        #region [ ----- sent mail cho gv & GV mantor----- ]
                        if (checkResp)
                        {
                            if (usercheck.Count() > 0)
                            {
                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = item.Course_Detail_Instructor;
                                    if (instructor_coursedetail.Count() > 0)
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = details.Trainee;
                                            if (instructor != null)
                                            {
                                                Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                        #endregion
                    }
                    if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Reject)
                    {
                        #region [ ----- Reject sent mail cho gv & GV mantor ----- ]
                        if (checkResp)
                        {
                            if (usercheck.Count() > 0)
                            {
                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = item.Course_Detail_Instructor;
                                    if (instructor_coursedetail.Any())
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = details.Trainee;
                                            {
                                                Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.Reject);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.Reject);
                        #endregion
                    }
                    #endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.SUBMIT_SUCCESS, lblStatus),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/Course", ex.Message);

                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<bool> ProccessingServer(FormCollection form, bool checkResp = true)
        {
            try
            {
                var id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);
                var course = aModel.Course;
                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["RejectReason"]) ? string.Empty : form["RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;

                if (status == (int)UtilConstants.EStatus.Approve)
                {

                    #region [--------CALL LMS (CRON PROGRAM)----------]
                    var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                    if (callLms)
                    {
                        var check = CallServices(UtilConstants.CRON_COURSE);
                        if (check)
                        {
                            CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                        }
                    }
                    #endregion
                    #region [-----------------------Sent Mail Approve Course-----------------------------]
                    //var usercheck = CourseDetailService.GetByCourse(courseId);

                    //if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                    //{
                    //    #region [ ----- sent mail cho gv & GV mantor----- ]
                    //    if (checkResp)
                    //    {
                    //        if (usercheck.Any())
                    //        {
                    //            foreach (var item in usercheck)
                    //            {
                    //                var instructor_coursedetail = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id).ToList();
                    //                if (instructor_coursedetail.Any())
                    //                {
                    //                    foreach (var details in instructor_coursedetail)
                    //                    {
                    //                        var instructor = EmployeeService.GetById(details.Instructor_Id);
                    //                        if (instructor != null)
                    //                        {
                    //                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }

                    //    Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    //    #endregion
                    //}
                    //if (aModel != null && aModel.int_id_status == (int)UtilConstants.EStatus.Reject)
                    //{
                    //    #region [ ----- Reject sent mail cho gv & GV mantor ----- ]
                    //    if (checkResp)
                    //    {
                    //        if (usercheck.Any())
                    //        {
                    //            foreach (var item in usercheck)
                    //            {
                    //                var instructor_coursedetail = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                    //                if (instructor_coursedetail.Any())
                    //                {
                    //                    foreach (var details in instructor_coursedetail)
                    //                    {
                    //                        var instructor = EmployeeService.GetById(details.Instructor_Id);
                    //                        {
                    //                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.Reject);
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }

                    //    Sent_Email_TMS(null, null, null, course, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.Reject);
                    //    #endregion
                    //}
                    #endregion

                }
                return true;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/Course", ex.Message);

                return false;

            }
        }
        #endregion
        #region Approve AssignTrainee
        public ActionResult AssignTrainee(int id)
        {
            var model = _repoApprove.GetById(id);
            return View(model);
        }
        [Custom("Ajax")]
        [HttpPost]
        public ActionResult SentAssignTrainee(int courseId)
        {
            try
            {
                var course_details = CourseDetailService.GetByCourse(courseId).Select(a => a.Id).ToList();
                var course = CourseService.GetById(courseId);
                if (course_details.Any())
                {

                    var member = CourseService.GetCourseResultFinal(a => a.courseid == course.Id && a.IsDeleted == false && a.MemberStatus == (int)UtilConstants.CourseResultFinalStatus.Pending).ToList();
                    if (member.Any())
                    {
                        foreach (var trainee in member)
                        {
                            //string body = string.Format(loadTemplate, trainee.Trainee.LastName + " " + trainee.Trainee.FirstName, item.SubjectDetail.Name, trainee.Course_Detail.Course.Name);
                            //string subject = "Assign Trainee";
                            //string emailTo = trainee.Trainee.str_Email;
                            //MailUtil.SendMail(emailTo, subject, body);
                            //check LMS Assign = UtilConstants.APIAssign.Approved
                            var checkExist = ConfigService.GetMemberMail(a => a.id_course == course.Id && trainee.Trainee.str_Email.Trim() == a.mail_receiver.Trim())?.FirstOrDefault();
                            if (checkExist == null)
                            {
                                if (trainee.Trainee.TMS_Course_Member.Any(a => a.Status == (int)UtilConstants.APIAssign.Approved && course_details.Contains(a.Course_Details_Id.Value)))
                                {
                                    Sent_Email_TMS(null, trainee.Trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee, true);
                                }
                                else if (trainee.Trainee.TMS_Course_Member.Any(a => a.Status == null && course_details.Contains(a.Course_Details_Id.Value)))
                                {
                                    Sent_Email_TMS(null, trainee.Trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee);
                                }
                            }
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            message = Messege.NO_DATA,
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        message = Messege.NO_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AssignTrainee(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var ListTrainee = form["CheckTrainee"].ToString();
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();

                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;

                var sendEmail = form["SendMail"];
                var checkSendEmail = sendEmail == null
                   ? false
                   : true;
                var approve = _repoApprove.GetById(approveId);
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.AssignedTrainee, status, UtilConstants.ActionType.Approve, note);
                if(status == (int)UtilConstants.EStatus.Approve)
                {
                    UpdateMember_New(approve.Course);
                    await Task.Run(() =>
                    {                      
                        if (checkSendEmail)
                        {
                            #region [---------Sent mail Assign trainee-------]

                            if (approve.int_id_status == (int)UtilConstants.EStatus.Approve)
                            {
                                var detail = approve.Course.Course_Detail.Where(p => p.IsDeleted != true).Select(a => a.Id);
                                //var list_member = CourseService.Lam_GetTraineemember_New(a => detail.Contains((int)a.Course_Details_Id) && a.Status != 1 && a.IsDelete != true && a.IsActive == true && (a.DeleteApprove != 1 || (a.DeleteApprove == 1 && a.LmsStatus != 99))).Select(a => a.Member_Id);
                                var member = CourseService.GetCourseResultFinal(a => a.courseid == approve.Course.Id && a.IsDeleted != true && a.MemberStatus == (int)UtilConstants.CourseResultFinalStatus.Pending).ToList();
                                if (!string.IsNullOrEmpty(ListTrainee))
                                {
                                    member = member.Where(a => ListTrainee.Contains(a.traineeid.ToString().Trim())).ToList();
                                }
                                if (member.Count() > 0)
                                {
                                    var mail_title = "Thư mời học/ Course Invitation";
                                    var body = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee).FirstOrDefault();
                                    var body_ = body?.Content;
                                    var cat_mail_ID = body?.ID ?? -1;
                                    foreach (var item in member)
                                    {
                                        var mail_receiver = item.Trainee.str_Email;
                                        //check LMS Assign = UtilConstants.APIAssign.Approved
                                        if (item.Trainee.TMS_Course_Member.Any(a => a.Member_Id == item.traineeid && detail.Contains((int)a.Course_Details_Id) && a.Status != 1 && a.IsDelete != true && a.IsActive == true && (a.DeleteApprove != 1 || (a.DeleteApprove == 1 && a.LmsStatus != 99))))
                                        {
                                            var bodysendmail = BodySendMail_Custom(body_, null, item.Trainee, approve.Course, null, 2);         
                                            if (!string.IsNullOrEmpty(mail_receiver) && cat_mail_ID != -1 && !string.IsNullOrEmpty(bodysendmail))
                                            {
                                                InsertSentMail_Custom(cat_mail_ID, mail_receiver, (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee, bodysendmail, approve.Course?.Id, mail_title);
                                            }
                                        }
                                      
                                    }
                                }
                            }
                            #endregion
                        }
                    }).ConfigureAwait(false);
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);



                //Task<ActionResult> task1 = ProccessingAssignClient(form);
                ////Task<bool> task2 = ProccessingAssignServer(form);
                ////await Task.WhenAny(task1, task2);
                //await Task.WhenAny(task1);
                //return Json(new AjaxResponseViewModel()
                //{
                //    result = true,
                //    message = Messege.APPROVE_SUCCESS
                //});
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<ActionResult> ProccessingAssignClient(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();

                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;

                var sendEmail = form["SendMail"];
                var checkSendEmail = sendEmail == null
                   ? false
                   : true;
                var approve = _repoApprove.GetById(approveId);
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.AssignedTrainee, status, UtilConstants.ActionType.Approve, note);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    UpdateMember_New(approve.Course);
                    if (checkSendEmail)
                    {
                        #region [---------Sent mail Assign trainee-------]

                        if (approve.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            var member = CourseService.GetCourseResultFinal(a => a.courseid == approve.Course.Id && a.IsDeleted == false && a.MemberStatus == (int)UtilConstants.CourseResultFinalStatus.Pending);
                            if (member != null)
                            {
                                var body = ConfigService.GetMail();
                                foreach (var item in member)
                                {
                                    //check LMS Assign = UtilConstants.APIAssign.Approved
                                    //if (item.Trainee.TMS_Course_Member.Any(a => a.Status == (int)UtilConstants.APIAssign.Approved))
                                    //{
                                    //    var body_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee)?.Content;
                                    //    var bodysendmail = BodySendMail_Custom(body_, null, item.Trainee, approve.Course, null, 2);
                                    //    Sent_Email_TMS_Custom(bodysendmail,null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee,false);
                                    //}
                                    //else if (item.Trainee.TMS_Course_Member.Any(a => a.Status == null))
                                    //{
                                    //    var body_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS)?.Content;
                                    //    var bodysendmail = BodySendMail_Custom(body_, null, item.Trainee, approve.Course, null, 2);
                                    //    Sent_Email_TMS_Custom(bodysendmail,null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee,true);
                                    //}
                                }
                            }
                        }
                        #endregion
                    }
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<bool> ProccessingAssignServer(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();
                var approve = _repoApprove.GetById(approveId);
                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;
                var sendEmail = form["SendMail"];
                var checkSendEmail = sendEmail == null
                   ? false
                   : true;

                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [--------CALL LMS (CRON ASSIGN TRAINEE)----------]
                    var callLms = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                    #endregion

                    //if (checkSendEmail)
                    //{
                    //    #region [---------Sent mail Assign trainee-------]

                    //    if (approve.int_id_status == (int)UtilConstants.EStatus.Approve)
                    //    {
                    //        var member = CourseService.GetCourseResultFinal(a => a.courseid == approve.Course.Id && a.IsDeleted == false && a.MemberStatus == (int)UtilConstants.CourseResultFinalStatus.Pending).ToList();
                    //        if (member != null)
                    //        {
                    //            foreach (var item in member)
                    //            {
                    //                //check LMS Assign = UtilConstants.APIAssign.Approved
                    //                if (item.Trainee.TMS_Course_Member.Any(a => a.Status == (int)UtilConstants.APIAssign.Approved))
                    //                {
                    //                    Sent_Email_TMS(null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee, true);
                    //                }
                    //                else if (item.Trainee.TMS_Course_Member.Any(a => a.Status == null))
                    //                {
                    //                    Sent_Email_TMS(null, item.Trainee, null, approve.Course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee);
                    //                }
                    //            }
                    //        }
                    //    }
                    //    #endregion
                    //}
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AssignTrainee", ex.Message);

                return false;

            }
        }
        #endregion
        #region Approve Subject Result
        public ActionResult SubjectResult(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index", "Approve");
            }
            var mModel = _repoApprove.GetById(id);

            #region[page load]
            if (mModel != null)
            {
                var instructor = "";
                var examiner = "";
                var monitor = "";
                var dbInstructor = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == mModel.int_courseDetails_Id);
                if (dbInstructor != null)
                {
                    foreach (var item in dbInstructor)
                    {

                        var trainee = item.Trainee;
                        //var fullName = trainee?.FirstName + " " + trainee?.LastName;
                        var fullName = ReturnDisplayLanguageCustom(trainee?.FirstName, trainee?.LastName, null, item.Duration.HasValue ? item.Duration.Value : 0);
                        switch (item.Type)
                        {
                            case (int)UtilConstants.TypeInstructor.Instructor:
                                instructor += fullName + "<br/>";
                                break;
                            case (int)UtilConstants.TypeInstructor.Mentor:
                                monitor += fullName + "<br/>";
                                break;
                            case (int)UtilConstants.TypeInstructor.Hannah:
                                examiner += fullName + "<br/>";
                                break;
                            default:
                                break;
                        }

                    }
                }
                ViewBag.score_pass = 1;//return_score_pass(id);
                ViewBag.Instructors = instructor;
                ViewBag.Examiner = examiner;
                ViewBag.Monitor = monitor;
            }
            else
            {
                return RedirectToAction("Index", "Approve");
            }
            #endregion


            return View(mModel);
        }
        [HttpPost]
        public ActionResult SubjectResult(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();

                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;
                var approve = _repoApprove.GetById(approveId);
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.SubjectResult, status, UtilConstants.ActionType.Approve, note, approve.int_courseDetails_Id);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [-------------Modify Course Result Sumary--------------]
                    var courseDetailIds = approve.Course.Course_Detail.Select(a => a.Id);
                    var courseResultSummary =
                        CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value), true);
                    if (courseResultSummary.Any())
                    {
                        foreach (var summary in courseResultSummary)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("LmsStatus", 1);
                            if (CMSUtils.UpdateDataSQLNoLog("Id", summary.Id + "", "Course_Result_Summary", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                        }
                    }
                    #endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<ActionResult> ProccessingSubjectClient(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();

                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;
                var approve = _repoApprove.GetById(approveId);
                Modify_TMS(true, approve.Course, (int)UtilConstants.ApproveType.SubjectResult, status, UtilConstants.ActionType.Approve, note, approve.int_courseDetails_Id);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [-------------Modify Course Result Sumary--------------]
                    var courseDetailIds = approve.Course.Course_Detail.Select(a => a.Id);
                    var courseResultSummary =
                        CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value));
                    if (courseResultSummary.Any())
                    {
                        foreach (var summary in courseResultSummary)
                        {
                            summary.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            CourseService.Update(summary);
                        }
                    }
                    #endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<bool> ProccessingSubjectServer(FormCollection form)
        {
            try
            {
                var approveId = string.IsNullOrEmpty(form["id"]) ? -1 : Convert.ToInt32(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var note = string.IsNullOrEmpty(form["note"]) ? string.Empty : form["note"].Trim();

                var status = approval == (int)UtilConstants.EStatus.Approve
                       ? (int)UtilConstants.EStatus.Approve
                       : (int)UtilConstants.EStatus.Reject;
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [--------CALL LMS (CronGet Course ResultSummary)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SubjectResult", ex.Message);

                return false;

            }
        }
        #endregion
        #region Approve Course Result
        public ActionResult CourseResult(int id)
        {
            var mModel = _repoApprove.GetById(id);
            ViewBag.courseid = mModel.int_Course_id;
            return View(mModel);
        }
        [HttpPost]
        public ActionResult CourseResult(FormCollection form, bool checkResp = true)
        {
            try
            {
                #region [------get value form-------]
                int id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);

                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["str_RejectReason"]) ? string.Empty : form["str_RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;
                #endregion
                Modify_TMS(true, aModel.Course, (int)UtilConstants.ApproveType.CourseResult, status, UtilConstants.ActionType.Approve, note);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [-------------Modify Course Result Sumary--------------]
                    var courseDetailIds = aModel.Course.Course_Detail.Select(a => a.Id);
                    var courseResultSummary =
                        CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value));
                    if (courseResultSummary.Any())
                    {
                        foreach (var summary in courseResultSummary)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("LmsStatus", 1);
                            if (CMSUtils.UpdateDataSQLNoLog("Id", summary.Id + "", "Course_Result_Summary", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                        }
                    }
                    #endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/CourseResult", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<ActionResult> ProccessingFinalClient(FormCollection form, bool checkResp = true)
        {
            try
            {
                #region [------get value form-------]
                int id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);

                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["str_RejectReason"]) ? string.Empty : form["str_RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;
                #endregion
                Modify_TMS(true, aModel.Course, (int)UtilConstants.ApproveType.CourseResult, status, UtilConstants.ActionType.Approve, note);
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    #region [-------------Modify Course Result Sumary--------------]
                    var courseDetailIds = aModel.Course.Course_Detail.Select(a => a.Id);
                    var courseResultSummary =
                        CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value));
                    if (courseResultSummary.Any())
                    {
                        foreach (var summary in courseResultSummary)
                        {
                            summary.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            CourseService.Update(summary);
                        }
                    }
                    #endregion
                    //#region [---------Sent mail trainee member course final -------]
                    //if (aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                    //{
                    //    if (checkResp)
                    //    {
                    //        #region [--------sent mail GV & GV mantor---------]
                    //        var usercheck = CourseDetailService.GetByCourse(courseId);
                    //        if (usercheck.Any())
                    //        {
                    //            foreach (var item in usercheck)
                    //            {
                    //                var instructor_coursedetail = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                    //                if (instructor_coursedetail.Any())
                    //                {
                    //                    foreach (var details in instructor_coursedetail)
                    //                    {
                    //                        var instructor = EmployeeService.GetById(details.Instructor_Id);
                    //                        Sent_Email_TMS(instructor, null, null, aModel.Course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        #endregion
                    //        #region [--------sent mail trainee---------]
                    //        var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.IsDeleted == false).OrderByDescending(a => a.courseid).ToList();
                    //        if (data.Any())
                    //        {
                    //            foreach (var trainees_final in data)
                    //            {
                    //                Sent_Email_TMS(null, trainees_final.Trainee, null, aModel.Course, null, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                    //            }
                    //        }
                    //        #endregion
                    //    }
                    //}
                    //#endregion
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/CourseResult", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }
        // Khôgn dùng đến do đã chạy cron ẩn
        public async Task<bool> ProccessingFinalServer(FormCollection form, bool checkResp = true)
        {
            try
            {
                #region [------get value form-------]
                int id = int.Parse(form["id"]);
                var approval = int.Parse(form["radioroption"]);
                var aModel = _repoApprove.GetById(id);

                var courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);
                var note = string.IsNullOrEmpty(form["str_RejectReason"]) ? string.Empty : form["str_RejectReason"].Trim();
                var status = approval == (int)UtilConstants.EStatus.Approve
                   ? (int)UtilConstants.EStatus.Approve
                   : (int)UtilConstants.EStatus.Reject;
                var lblStatus = approval == (int)UtilConstants.EStatus.Approve
                   ? Resource.lblApproved
                   : Resource.lblReject;
                #endregion

                if (status == (int)UtilConstants.EStatus.Approve)
                {

                    #region [--------CALL LMS (CronGet Course ResultSummary)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                    #endregion
                    #region [---------Sent mail trainee member course final -------]
                    //if (aModel.int_id_status == (int)UtilConstants.EStatus.Approve)
                    //{
                    //    if (checkResp)
                    //    {
                    //        #region [--------sent mail GV & GV mantor---------]
                    //        var usercheck = CourseDetailService.GetByCourse(courseId);
                    //        if (usercheck.Any())
                    //        {
                    //            foreach (var item in usercheck)
                    //            {
                    //                var instructor_coursedetail = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                    //                if (instructor_coursedetail.Any())
                    //                {
                    //                    foreach (var details in instructor_coursedetail)
                    //                    {
                    //                        var instructor = EmployeeService.GetById(details.Instructor_Id);
                    //                        Sent_Email_TMS(instructor, null, null, aModel.Course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        #endregion
                    //        #region [--------sent mail trainee---------]
                    //        var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.IsDeleted == false).OrderByDescending(a => a.courseid).ToList();
                    //        if (data.Any())
                    //        {
                    //            foreach (var trainees_final in data)
                    //            {
                    //                Sent_Email_TMS(null, trainees_final.Trainee, null, aModel.Course, null, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                    //            }
                    //        }
                    //        #endregion
                    //    }
                    //}
                    #endregion
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/CourseResult", ex.Message);

                return false;

            }
        }
        #endregion
        #region [Thanh tiến trình các bước]

        [AllowAnonymous]
        public ActionResult ProccessBar(Course course)
        {
            var model = new ProccessBarModel();
            model.Html = ReturnColumnProcessBar(course);
            return PartialView("_Partials/_ProccessBar", model);

        }

        [AllowAnonymous]
        public ActionResult ProccessBar2(int approvalId)
        {
            var courseType = string.Empty;
            var statusList = 0;
            var fCode = string.Empty;
            var fName = string.Empty;
            if (Request.UrlReferrer != null)
            {
                courseType = HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["coursetype"];
                statusList = string.IsNullOrEmpty(HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["statuslist"]) ? -1 : Convert.ToInt32(HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["statuslist"].Trim());
                fCode = string.IsNullOrEmpty(HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["code"]) ? "" : HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["code"].Trim();
                fName = string.IsNullOrEmpty(HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["fName"]) ? "" : HttpUtility.ParseQueryString(Request.UrlReferrer.AbsoluteUri)["fName"].Trim();
            }
            var course = CourseService.Get(true).FirstOrDefault(a => a.TMS_APPROVES.Any(b => b.int_Course_id == a.Id && b.id == approvalId));
            var Stringparam = "?valueparam=&coursetype=" + courseType + "&statuslist=" + statusList + "&code=" + fCode + "&fName=" + fName;
            var model = new ProccessBarModel();
            model.Html = ReturnColumnProcessBar(course, Stringparam);
            return PartialView("_Partials/_ProccessBar", model);
        }

        [AllowAnonymous]
        public ActionResult AjaxHandlerlistsubjectProccessBar(jQueryDataTableParamModel param)
        {
            var courseId = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
            try
            {
                var modeltemp =
                    _repoApprove.Get(
                        a => a.int_Course_id == courseId);

                var model = modeltemp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);


                var verticalBar = GetByKey("VerticalBar");
                var data = model.Where(b => b.int_Type == (int)UtilConstants.ApproveType.SubjectResult).Select(a => new AjaxListSubjectDetail()
                {
                    Id = a?.id.ToString(),
                    Course = "<a href='" + @Url.Action("SubjectResult", "Approve", new { a?.id }) + "'>" +
                    (a?.Course_Detail?.SubjectDetail?.IsActive != true ? "<span style='color:" + UtilConstants.String_DeActive_Color + ";')>" + a?.Course_Detail?.SubjectDetail?.Code + "</span> - <span style='color:" + UtilConstants.String_DeActive_Color + ";')>" + a?.Course_Detail?.SubjectDetail?.Name + "</span>" : a?.Course_Detail?.SubjectDetail?.Code + " - " + a?.Course_Detail?.SubjectDetail?.Name) +
                     "</a>" + " " + ReturnStatus(a?.int_id_status),
                    FromTo = a?.Course_Detail?.dtm_time_from.Value.ToString(Resource.lbl_FORMAT_DATE) + " - " + a?.Course_Detail?.dtm_time_to.Value.ToString(Resource.lbl_FORMAT_DATE),
                    //RequestBy = (a?.Date_Requested != null ? a?.Date_Requested.Value.ToString(Resource.lbl_FORMAT_DATE) : string.Empty) + " - " + ReturnDisplayLanguage(a?.USER1?.FIRSTNAME, a?.USER1?.LASTNAME),
                    Action = ReturnColumnOptionInSubjectProccessBar(a?.int_Course_id, a?.int_courseDetails_Id, a?.int_id_status, a?.id, a.int_Type, (int)UtilConstants.ApproveType.SubjectResult) + verticalBar +
                             ReturnColumnLog(a?.id, a?.int_Type)
                });

                var filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxListSubjectDetail, string> orderingFunction = (a => sortColumnIndex == 1 ? a.Course
                : sortColumnIndex == 2 ? a.FromTo
                : sortColumnIndex == 3 ? a.Action
                : a.Id)
                ;
                var sortDirection = Request["sSortDir_0"] ?? "desc"; // asc or desc
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                       : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                       string.Empty,
                           c?.Course,
                           c?.FromTo,
                           c?.Action
                                 };

                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = modeltemp.Count(),
                    iTotalDisplayRecords = modeltemp.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/AjaxHandlerlistsubject", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private string ReturnColumnOptionInSubjectProccessBar(int? courseId, int? courseDetailId, int? status, int? approveId, int? laststep, int? stepNotEdit)
        {
            var html = string.Empty;
            switch (status)
            {
                case (int)UtilConstants.EStatus.Pending:
                    html += (User.IsInRole("/Approve/SubjectResult") ? "<a href='" + @Url.Action("SubjectResult", "Approve", new { id = approveId }) + "'><i class='fas fa-edit btnIcon_green' ></i></a>" : "");
                    break;
                case (int)UtilConstants.EStatus.Approve:
                    html += User.IsInRole("/Approve/SubjectResult")
                              ? "<a href='javascript:void(0)' onclick='RemoveApproveSubjectt(" + courseId + "," +
                                (int)UtilConstants.ApproveType.SubjectResult + "," + courseDetailId +
                                ")'><i class='fa fa-unlock btnIcon_orange'></i></a>"
                              : string.Empty;
                    break;
                default:
                    html += "";
                    break;
            }
            return html;
        }
        private StringBuilder ReturnColumnProcessBar(Course data, string param = "")
        {
            var html = new StringBuilder();
            if (data == null) return html;

            html.Append("<div class='col-md-12 '>");
            //html.AppendFormat("<div class='hidden-xs col-md-3 col-xs-12'><b>{3}: </b> {0}</div><div class='hidden-xs col-md-9  col-xs-12'><b>{4}: </b>{1} - {2}</div>", data.Code, DateUtil.DateToString(data.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(data.EndDate, "dd/MM/yyyy"), Resource.lblCode, Resource.lblTime);
            //html.AppendFormat("<div class='visible-xs col-md-3 col-xs-12' style='font-size: 11px;'><b>{3}: </b> {0}</div><div class='visible-xs col-md-9  col-xs-12'  style='font-size: 11px;margin-bottom: 5px;'><b>{4}: </b>{1} - {2}</div>", data.Code, DateUtil.DateToString(data.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(data.EndDate, "dd/MM/yyyy"), Resource.lblCode, Resource.lblTime);
            //html.AppendFormat("<div class='hidden-xs col-md-12'><h4><b>{0}</b></h4></div>", data.Name);
            //html.AppendFormat("<div class='visible-xs  col-md-12' style='margin-bottom: 15px;'><h5><b>{0}</b></h5></div>", data.Name);
            html.AppendFormat("{0}", ReturnProcessStepsBar(data, param));
            html.Append("</div><div id='detail" + data.Id + "'></div><hr>");
            return html;
        }

        private StringBuilder ReturnProcessStepsBar(Course course, string param = "")
        {
            var html = new StringBuilder();
            html.Append("<div class='row bs-wizard' style='border-bottom:0;'>");
            //complete active disabled
            var dataTypeApprovalType = UtilConstants.ApproveTypeDictionary();

            var data_TMS_APPROVES = course.TMS_APPROVES;
            int Count_SubjectApproved = _repoApprove.Get(a => a.int_Course_id == course.Id && a.int_courseDetails_Id != null, (int)UtilConstants.ApproveType.SubjectResult).Count();
            int Count_SubjectDetail = CourseDetailService.GetByCourse(course.Id).Count();
            foreach (var item in dataTypeApprovalType) // type course,trainee,subject,final
            {
                #region[check process]

                var key = item.Key;
                var requestInfo = "";
                var status = "disabled";//disabled  active complete
                var icon = "";
                var display = "none";
                var iconEdit = "";
                var labelStatus = "";
                var log = "";
                var data = data_TMS_APPROVES.FirstOrDefault(a => a.int_Type == key);
                if (data != null)
                {
                    log = ReturnColumnLog(data.id, data.int_Type);

                    requestInfo = "<p style='font-size:10px;    margin-bottom: 0px;'><i class='fa fa-user' aria-hidden='true'></i>  " + (ReturnDisplayLanguage(data.USER1?.FIRSTNAME, data.USER1?.LASTNAME)) + "</p><p style='font-size:10px;    margin-bottom: 0px;'>  <i class='far fa-clock' aria-hidden='true'></i>  " + (data.Date_Requested.HasValue ? data.Date_Requested?.ToString("dd/MM/yyyy") : data.dtm_requested_date?.ToString("dd/MM/yyyy")) + "</p>";
                    switch (data.int_id_status)
                    {
                        case (int)UtilConstants.EStatus.Approve:
                            status = "complete approval ";
                            icon = "<i class=' fa fa-check i-in-bs-wizard' aria-hidden='true'></i>";
                            display = "block";
                            if (key != (int)UtilConstants.ApproveType.AssignedTrainee)
                            {
                                iconEdit = User.IsInRole("/Approve/" + item.Value + "") ? "<a href='javascript:void(0)' onclick='RemoveApproveProcessBar(" + data.int_Course_id + "," + key + ")'><i class='fa fa-unlock btnIcon_orange'></i></a>" : string.Empty;
                            }

                            labelStatus = "<span class='label label-success hidden-xs  '>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Pending:
                            status = " pending ";
                            if (Count_SubjectApproved < Count_SubjectDetail)
                            {
                                icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                            }
                            display = "block";
                            iconEdit = User.IsInRole("/Approve/" + item.Value + "") ? "<a href='" + @Url.Action("" + item.Value + "", "Approve", new { data.id }) + (string.IsNullOrEmpty(param) ? "" : param) + "'><i class='fas fa-edit btnIcon_green'></i></a>" : "";
                            labelStatus = "<span class='label label-default hidden-xs  '>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Block:
                            status = " block ";
                            labelStatus = "<span class='label label-warning hidden-xs  ' aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.Reject:
                            status = " reject ";
                            labelStatus = "<span class='label label-danger hidden-xs  'aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            break;
                        case (int)UtilConstants.EStatus.CancelRequest:
                            status = " CancelRequest ";
                            labelStatus = "<span class='label label-blueviolet hidden-xs  'aria-hidden='true'>" + (data.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                            display = "block";
                            if (1 == 1)//(key != (int)UtilConstants.ApproveType.AssignedTrainee)
                            {
                                iconEdit = User.IsInRole("/Approve/" + item.Value + "") ? "<a href='javascript:void(0)' onclick='RemoveApproveProcessBar(" + data.int_Course_id + "," + key + ")'><i class='fa fa-unlock btnIcon_orange' aria-hidden='true'></i></a>" : "";
                            }

                            break;
                    }
                }

                #region [đặt lại tên]
                var name = "";
                if (item.Value.Any())
                {
                    switch (item.Value)
                    {
                        case "AssignTrainee":
                            name = Resource.lblTRAINEE;
                            break;
                        case "SubjectResult":
                            name = Resource.lblRESULT;
                            break;
                        case "CourseResult":
                            name = Resource.lblFinal;
                            break;
                        case "Course":
                            name = Resource.lblCourse;
                            break;
                    }
                }
                #endregion

                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    iconEdit = "<span data-value='" + course.Id + "' class='expand' style='cursor: pointer;' onclick='clonetable(" + course.Id + ")' ><i class='fa fa-search' aria-hidden='true' style='font-size: 16px; color: green; '></i></span>";
                    log = "";

                    var db = data_TMS_APPROVES.Where(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (db.Any())
                    {

                        foreach (var item2 in db)
                        {
                            if (item2.int_id_status != (int)UtilConstants.EStatus.Approve)
                            {
                                display = "block";
                                status = " processing ";
                                labelStatus = "<span class='label label-primary hidden-xs  'aria-hidden='true'>Processing</span>";
                                if (Count_SubjectApproved < Count_SubjectDetail)
                                {
                                    icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                                }
                                break;
                            }
                        }
                    }

                }
                var url = "javascript:void(0)";
                if (item.Value != "SubjectResult")
                {
                    url = "/Approve/" + item.Value + "/" + data?.id + "" + (string.IsNullOrEmpty(param) ? "" : param);
                }
                #endregion
                var htmlstyle = "<p class='' style='top: 22%; position: absolute; left: 56%;font-size: 16px;'>{0}&nbsp;&nbsp;{1}</p> ";
                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    log = "";
                    requestInfo = "";
                    htmlstyle = "<p class='' style='top: 30%; position: absolute; left: 56%;font-size: 16px;'>{0}&nbsp;&nbsp;{1}</p> ";
                }
                html.AppendFormat("<div class='col-md-3 col-sm-3 col-xs-12 bs-wizard-step {0}'>", status);
                html.AppendFormat("<div class=' text-center bs-wizard-stepnum'><h4><b>{0}</b></h4></div>", name);
                html.Append("<div class='progress'><div class='progress-bar'></div></div>");
                html.AppendFormat(" <a href='{0}' class=' bs-wizard-dot'><div style='width: 75%; height: 75%; margin-top: 12%; margin-left: 13%; background: #4CAF50; border-radius: 50%;'><b class='' style='font-size: 22px; margin-left: 8px;'>{1}</b><b class='visible-xs' style='font-size: 16px; margin-left: 6px;'>{2}</b></div></a>", url, key, key);
                html.AppendFormat(htmlstyle, icon, labelStatus);
                html.AppendFormat("<div class='bs-wizard-info text-center'><div style='display:{0}'>{1}&nbsp;&nbsp;{2}</div>{3}</div>", display, iconEdit, log, requestInfo);
                html.AppendFormat("</div>");
            }
            html.Append("</div>");
            return html;
        }
        #endregion

        #region [duyệt cấp chứng chỉ]

        [AllowAnonymous]
        		public ActionResult Certificate()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new FinalCourseResultModel()
            {
                Courses = CourseService.Get(a => a.StartDate >= timenow && a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.Course && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.IsDeleted != true).OrderByDescending(b => b.StartDate).ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name)),
                //Courses =
                //    _repoApprove.Get(
                //        a =>
                //            a.int_Type == (int)UtilConstants.ApproveType.Course &&
                //            a.int_id_status == (int)UtilConstants.EStatus.Approve && a.Course.IsDeleted == false)
                //        .OrderByDescending(b => b.Course.StartDate)
                //        .ToDictionary(c => c.Course.Id, c => string.Format("{0} - {1}", c.Course.Code, c.Course.Name)),
                //Certificates = CourseService.GetCatCertificates(a => a.IsActive == true).ToDictionary(b => b.ID, b => b.Name),
                //Departments = DepartmentService.Get(null, CurrentUser.PermissionIds).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name),
                //JobTitles = _repoJobtitle.Get(null, true).ToDictionary(a => a.Id, a => a.Name),
                //GroupCertificates = CourseService.GetAllGroupCertificate(a => a.IsActive == true).ToDictionary(a => a.Id, a => a.Name)
            };
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult AjaxHandleLisInputCertificate(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var data__ = CourseService.GetCourseResultFinal(a => a.courseid == courseList && a.IsDeleted == false, new int[] { (int)UtilConstants.ApproveType.CourseResult }, (int)UtilConstants.EStatus.Approve).Select(a => a.id);
                var model_ = CourseService.GetAllGroupCertificateApprove(a => a.CoureResultFinalID.HasValue && data__.Contains((int)a.CoureResultFinalID) && a.status != (int)UtilConstants.StatusApiApprove.Approved).ToList();
                var model = model_.Select(a => new AjaxCourseResultFinalModel
                {
                    TraineeCode = a.Trainee?.str_Staff_Id,
                    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    DepartmentName = a.Trainee?.Department?.Name,
                    DateFromTo = DateUtil.DateToString(a.Course_Result_Final?.Course?.StartDate, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Result_Final?.Course?.EndDate, "dd/MM/yyyy"),
                    Id = a?.ID ?? 0,
                    TraineeId = a.Trainee?.Id ?? -1,
                    CourseDetailId = a?.Course_Result?.CourseDetailId ?? -1,
                    Point = a.Course_Result_Final?.point.ToString() ?? "",
                    Grade = GetGrade(a.Course_Result_Final?.grade),
                    codecertificate = a.certificatefinal,
                    checkstatus = a.status,
                    Path = a.Path
                });

                IEnumerable<AjaxCourseResultFinalModel> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxCourseResultFinalModel, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.TraineeCode
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.DepartmentName
                                                            : sortColumnIndex == 4 ? c.DateFromTo
                                                            : sortColumnIndex == 5 ? c.Point
                                                            //: sortColumnIndex == 6 ? c.Remark
                                                            : c.TraineeCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var domainName = Request.Url.Authority;
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c?.TraineeCode,
                                                c?.FullName,
                                                c?.DepartmentName,
                                                c?.DateFromTo,
                                                c?.Point,
                                                c?.Grade,
                                                c.codecertificate,

                                                  !string.IsNullOrEmpty(c?.codecertificate)
                            ? ( (bool)c?.Path.StartsWith("<div") ? "<a onclick='Blank_Review("+c?.TraineeId+")' title='View'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i> </a> "+
                              "<div id='c"+c?.TraineeId+"' style='display:none;'><span id='a"+c?.TraineeId+"' class='widget'>" + c?.Path+ "</span></div>" : "<a  href='//"+domainName + GetByKey("PathFileExtImage") +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>")
                            : "",
                                                      "<input type='hidden' class='form-control' name='IdApprove' value='"+c?.Id+"'/>" +
                                                 (c?.checkstatus == (int)UtilConstants.StatusApiApprove.Pending ? "Pending" : (c?.checkstatus == (int)UtilConstants.StatusApiApprove.Approved ? "Approved" : "Reject"))  ,


                                                //"<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + c?.Id +")'><i class='fas fa-edit' aria-hidden='true' style='color: " + (c?.Type == true ? "red" : "black" ) +" ; '></i></a>"

                                               };
                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandleLisInputCetificate", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectCertificate(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var ddl_subject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                var data = CourseService.GetCourseResult(a => a.Course_Detail.CourseId == courseList && a.Course_Detail.SubjectDetailId == ddl_subject).Select(a => a.Id);
                var model_temp = CourseService.GetAllGroupCertificateApprove(a => a.CourseResultID.HasValue && data.Contains((int)a.CourseResultID) && a.status != (int)UtilConstants.StatusApiApprove.Approved).ToList();


                var model_ = model_temp.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = model_temp.Count();


                var model = model_.Select(a => new AjaxCourseResultFinalModel
                {
                    TraineeCode = a.Trainee?.str_Staff_Id,
                    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    DepartmentName = a.Trainee?.Department?.Name,
                    DateFromTo = DateUtil.DateToString(a.Course_Result?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Result?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                    Grade = returnpointgrade(2, a?.IdTrainee, a?.Course_Result?.CourseDetailId),
                    Id = a?.ID ?? 0,
                    TraineeId = a.Trainee?.Id ?? -1,
                    CourseDetailId = a?.Course_Result?.CourseDetailId ?? -1,
                    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Result?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a),
                    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Result?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a),
                    codecertificate = a.certificatefinal,
                    checkstatus = a.status,
                    Path = a.Path
                });

                IEnumerable<AjaxCourseResultFinalModel> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxCourseResultFinalModel, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.TraineeCode
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.DepartmentName
                                                            : sortColumnIndex == 4 ? c.DateFromTo
                                                            : sortColumnIndex == 5 ? c.Point
                                                            //: sortColumnIndex == 6 ? c.Remark
                                                            : c.TraineeCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered;
                var domainName = Request.Url.Authority;
                var result = from c in displayed
                             select new object[] {
                                                string.Empty,
                                                c?.TraineeCode,
                                                c?.FullName,
                                                c?.DepartmentName,
                                                c?.DateFromTo,

                                                c?.FirstResultCertificate,
                                                c?.ReResultCertificate,
                                                c?.Grade,
                                                c.codecertificate,

                                                 !string.IsNullOrEmpty(c?.codecertificate)
                            ? ( (bool)c?.Path.StartsWith("<div") ? "<a onclick='Blank_Review("+c?.TraineeId+")' title='View'  data-toggle='tooltip'><i class='fa fa-search btnIcon_green' ></i> </a> <a onclick='Print("+c?.TraineeId+")' title='Print'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i> </a> "+
                              "<div id='c"+c?.TraineeId+"' style='display:none;'><span id='a"+c?.TraineeId+"' class='widget'>" + c?.Path+ "</span></div>" 
                                                                 : "<a  href='//"+domainName + GetByKey("PathFileExtImage") +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>")
                            : "",
                                                "<input type='hidden' class='form-control' name='IdApprove' value='"+c?.Id+"'/>" +
                                                 (c?.checkstatus == (int)UtilConstants.StatusApiApprove.Pending ? "Pending" : (c?.checkstatus == (int)UtilConstants.StatusApiApprove.Approved ? "Approved" : "Reject")),
                                                //"<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + c?.Id +")'><i class='fas fa-edit' aria-hidden='true' style='color: " + (c?.Type == true ? "red" : "black" ) +" ; '></i></a>"

                                               };
                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = tempcount,
                    iTotalDisplayRecords = tempcount,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ListNoteChangeCourseCodeReturnSubject", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public string returnpointgrade(int? type, int? Trainee_Id, int? Course_Details_Id)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            string _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            if (data != null)
            {
                if (type == 1)
                {
                    #region [get điểm]
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString();
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString();
                        }
                    }
                    #endregion
                }
                else
                {
                    if (data.Re_Check_Result != null)
                    {
                        if (data.Re_Check_Result == null && data.First_Check_Result == null)
                            _return = "Fail";
                        if (data.Re_Check_Result == "P")
                        {
                            _return = "Pass";
                            var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Subject_Id).OrderByDescending(a => a.point_from);
                            foreach (var item in check_distintion)//.OrderBy(a => a.point_from)
                            {
                                if (data.Re_Check_Score >= item.point_from)
                                {
                                    _return = item.grade;
                                    if (data.First_Check_Result != "F")
                                    {
                                        break;
                                    }

                                }
                            }

                        }
                        else
                        {
                            _return = "Fail";
                        }
                    }
                    else
                    {
                        if (data.First_Check_Result != null)
                        {
                            if (data.First_Check_Result == "P")
                            {
                                _return = "Pass";
                                var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Subject_Id);
                                foreach (var item in check_distintion.OrderBy(a => a.point_from))
                                {
                                    if (data.First_Check_Score >= item.point_from)
                                    {
                                        _return = item.grade;
                                    }
                                }
                            }
                            else
                            {
                                _return = "Fail";
                            }
                        }
                        else
                        {
                            _return = "Fail";
                        }
                    }



                }
            }

            return _return;
        }
        protected object ReturnTraineePoint(bool isFirstCheck, bool? isAvarage, TMS_CertificateApproved rs)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            object point = null;
            if (rs.Course_Result == null) return null;
            if (isFirstCheck)
            {
                if (isAvarage.HasValue && isAvarage.Value)
                {
                    point = rs.Course_Result.First_Check_Score;
                }
                else
                {
                    switch (rs.Course_Result.First_Check_Result)
                    {
                        case "P":
                            point = "Pass";
                            break;
                        case "F":
                            point = "Fail";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (isAvarage.HasValue && isAvarage.Value)
                {
                    point = rs.Course_Result.Re_Check_Score;
                }
                else
                {
                    switch (rs.Course_Result.Re_Check_Result)
                    {
                        case "P":
                            point = "Pass";
                            break;
                        case "F":
                            point = "Fail";
                            break;
                        default:
                            break;
                    }
                }
            }
            return point;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SaveCertifiCateForGroup(FormCollection form)
        {
            try
            {
                var id = form.GetValues("IdApprove");
                var i = 0;
                if (id == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SaveCertifiCateForGroup", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                int count = 0;
                var model = new Course_Result_Final();
                foreach (var item in id)
                {
                    var fId = int.Parse(item);
                    var fResult = CourseService.GetCourseResultById(fId);
                    var path = string.Empty;
                    //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                    var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.ID == fId && a.status != (int)UtilConstants.StatusApiApprove.Approved);
                    foreach (var certificate in listcertificateapproved)
                    {

                        var path_temp = ReplaceCertificateNo(certificate.Path, certificate.certificatefinal, 1);
                        var stream = HTMLToImage(path_temp);
                        Stream filestream = new MemoryStream(stream);
                        path = SaveImage(filestream, certificate.Trainee.str_Staff_Id);
                    }
                    var entity =
                      new TMS_CertificateApproved();
                    if (listcertificateapproved.Any())
                    {
                        entity = listcertificateapproved.FirstOrDefault();
                        entity.Date_Of_Issue = DateTime.Now;
                        entity.status = (int)UtilConstants.StatusApiApprove.Approved; //đã duyệt
                        entity.ApproveAt = DateTime.Now;
                        entity.ApproveBy = CurrentUser.USER_ID;
                        CourseService.ModifyTMSCertificateAppovedEntity(entity);

                        model = CourseService.GetCourseResultFinalById((int)entity.CoureResultFinalID);
                        if (model != null)
                        {
                            model.Path = path;
                            model.CAT_CERTIFICATE_ID = entity.CAT_CERTIFICATE_ID;
                            model.statusCertificate = entity.IsRevoked != true ? 0 : 1;
                            model.certificatefinal = entity.certificatefinal;
                            model.CreateCertificateDate = entity.Date_Of_Issue;
                            model.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            CourseService.UpdateCourseResultFinalReturnEntity(model);
                        }
                        count++;

                    }
                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = "Data is Unavailable!!!",
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CronGet Cron GetCertificate)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_CERTIFICATE_CATEGORY);
                    if (!callLms)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SaveCertifiCateForGroup", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SaveCertificate(FormCollection form)
        {
            try
            {

                var id = form.GetValues("IdApprove");
                var i = 0;
                if (id == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SaveCertifiCate", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                int count = 0;
                var model = new Course_Result();
                foreach (var item in id)
                {
                    var fId = int.Parse(item);
                    var fResult = CourseService.GetCourseResultById(fId);
                    var path = string.Empty;
                    //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                    var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.ID == fId && a.status != (int)UtilConstants.StatusApiApprove.Approved);
                    foreach (var certificate in listcertificateapproved)
                    {
                        var path_temp = ReplaceCertificateNo(certificate.Path, certificate.certificatefinal, 0);
                        var stream = HTMLToImage(path_temp);
                        Stream filestream = new MemoryStream(stream);
                        path = SaveImage(filestream, certificate.Trainee.str_Staff_Id);
                    }
                    var entity =
                      new TMS_CertificateApproved();
                    if (listcertificateapproved.Any())
                    {
                        entity = listcertificateapproved.FirstOrDefault();
                        entity.Date_Of_Issue = DateTime.Now;
                        entity.status = (int)UtilConstants.StatusApiApprove.Approved; //đã duyệt
                        entity.ApproveAt = DateTime.Now;
                        entity.ApproveBy = CurrentUser.USER_ID;
                        CourseService.ModifyTMSCertificateAppovedEntity(entity);

                        model = CourseService.GetCourseResultById((int)entity.CourseResultID);
                        if (model != null)
                        {
                            model.Path = path;
                            model.CertificateID = entity.CAT_CERTIFICATE_ID;
                            model.StatusCertificate = entity.IsRevoked != true ? 0 : 1;
                            model.CertificateSubject = entity.certificatefinal;
                            model.CreateCertificateAt = entity.Date_Of_Issue;
                            model.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            CourseService.UpdateCourseResult(model);
                        }
                        count++;
                    }
                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = "Data is Unavailable!!!",
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CronGet Cron GetCertificate)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_CERTIFICATE_COURSE);
                    if (!callLms)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Approve/SaveCertifiCate", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult RejectSaveCertifiCate(FormCollection form)
        {
            try
            {
                var id = form.GetValues("IdApprove");
                var i = 0;
                if (id == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                int count = 0;
                var model = new Course_Result_Final();
                foreach (var item in id)
                {
                    var fId = int.Parse(item);

                    var path = string.Empty;
                    //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                    var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.ID == fId && a.status != (int)UtilConstants.StatusApiApprove.Approved);
                    //foreach (var certificate in listcertificateapproved)
                    //{
                    //    var stream = HTMLToImage(certificate.Path);
                    //    Stream filestream = new MemoryStream(stream);
                    //    path = SaveImage(filestream, certificate.Trainee.str_Staff_Id);
                    //}
                    var entity =
                      new TMS_CertificateApproved();
                    if (listcertificateapproved.Any())
                    {
                        entity = listcertificateapproved.FirstOrDefault();
                        //entity.Date_Of_Issue = DateTime.Now;
                        entity.status = (int)UtilConstants.StatusApiApprove.Reject; //chuw duyệt
                        CourseService.ModifyTMSCertificateAppovedEntity(entity);
                        count++;

                    }



                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = "Data is Unavailable!!!",
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                #region [--------CALL LMS (CronGet Cron GetCertificate)----------]
                //var callLms = CallServices(UtilConstants.CRON_GET_CERTIFICATE);
                //if (!callLms)
                //{
                //    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA + " " + Messege.ERROR_CALL_LMS);
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                //}
                #endregion

                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult RejectSaveCertificateForGroup(FormCollection form)
        {
            try
            {

                var id = form.GetValues("IdApprove");
                var i = 0;
                if (id == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                int count = 0;
                var model = new Course_Result();
                foreach (var item in id)
                {
                    var fId = int.Parse(item);
                    var fResult = CourseService.GetCourseResultById(fId);
                    var path = string.Empty;
                    //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                    var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.ID == fId && a.status != (int)UtilConstants.StatusApiApprove.Approved);
                    //foreach (var certificate in listcertificateapproved)
                    //{
                    //    var stream = HTMLToImage(certificate.Path);
                    //    Stream filestream = new MemoryStream(stream);
                    //    path = SaveImage(filestream, certificate.Trainee.str_Staff_Id);
                    //}
                    var entity =
                      new TMS_CertificateApproved();
                    if (listcertificateapproved.Any())
                    {
                        entity = listcertificateapproved.FirstOrDefault();
                        //entity.Date_Of_Issue = DateTime.Now;
                        entity.status = (int)UtilConstants.StatusApiApprove.Reject;
                        CourseService.ModifyTMSCertificateAppovedEntity(entity);

                        count++;

                    }



                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = "Data is Unavailable!!!",
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                #region [--------CALL LMS (CronGet Cron GetCertificate)----------]
                //var callLms = CallServices(UtilConstants.CRON_GET_CERTIFICATE);
                //if (!callLms)
                //{
                //    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", Messege.ISVALID_DATA + " " + Messege.ERROR_CALL_LMS);
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                //}
                #endregion

                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCate", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }


        public byte[] HTMLToImage(string html)
        {
            string read = @"<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset =UTF-8""/></head><body>" + html + "</body></html>";
            var htmlToImageConv = new NReco.ImageGenerator.HtmlToImageConverter();
            var image = htmlToImageConv.GenerateImage(read, ImageFormat.Png);
            return image;
        }

        protected string SaveImage(Stream filestream, string eid)
        {
            var path = "/Uploads/Certificate/";
            var random = new Random();
            var nameFile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
            nameFile = eid + "Cert" + "_" + nameFile + random.Next(1, 100) + ".png";

            var useAws = AppUtils.getAppSetting("UseAws");
            if (useAws == "1")
            {
                path = path.Substring(1);
                if (!AWSUtils.AWS_CheckFolderExists(path))
                {
                    AWSUtils.AWS_RunFolderCreationDemo(path);
                }
                AWSUtils.AWS_PutObject(path + nameFile, filestream);
            }
            else
            {
                CreateFolderIfExists(string.Empty, path);

                var pathForSaving = Server.MapPath(path);
                var pf = Path.Combine(pathForSaving, nameFile);
                Image img = Image.FromStream(filestream);
                img.Save(pf);
            }
            return path + nameFile;
        }

        private string GetGrade(int? grade)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    result = UtilConstants.Grade.Fail.ToString();
                    break;
                case (int)UtilConstants.Grade.Pass:
                    result = UtilConstants.Grade.Pass.ToString();
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    result = UtilConstants.Grade.Distinction.ToString();
                    break;
            }
            return result;
        }
        #endregion
        [AllowAnonymous]
        public ActionResult AjaxHandlerscheduleDetail(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var model = CourseDetailService.Get(a => a.CourseId == id);

                IEnumerable<Course_Detail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.SubjectDetail?.Code
                                                                        : sortColumnIndex == 2 ? c?.SubjectDetail?.Name
                                                                        : sortColumnIndex == 3 ? c?.SubjectDetail?.Course_Type.str_Name
                                                                        : sortColumnIndex == 4 ? c?.SubjectDetail?.Duration
                                                                        : sortColumnIndex == 5 ? c?.type_leaning
                                                                        : sortColumnIndex == 5 ? c?.Room?.str_Name
                                                                        //: sortColumnIndex == 6 ? c?.Trainee?.str_Fullname
                                                                        : sortColumnIndex == 7 ? c?.dtm_time_from
                                                                        : sortColumnIndex == 8 ? (object)c?.time_from
                                                                        : c?.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction).ThenBy(c => (object)c?.time_from).ThenBy(c => (object)c?.time_to)
                                    : filtered.OrderByDescending(orderingFunction).ThenBy(c => (object)c?.time_from).ThenBy(c => (object)c?.time_to);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var result = from c in displayed
                                 //TODO: change model
                                 //let instructor = string.Join(" ", c.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).OrderBy(a => a.Trainee.FirstName).Select(a => a.Trainee.FirstName + " " + a.Trainee.LastName).ToArray())
                             let instructor = string.Join("<br /> ", c.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).OrderBy(a => a.Trainee.FirstName).Select(a => ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName)).ToArray())
                             select new object[] {
                             string.Empty,
                             c?.SubjectDetail?.Code,

                             c.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline ? "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+" data-value='"+c.Id+"' class='expand'><a>" +c?.SubjectDetail?.Name +"</a></span>" : "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.SubjectDetail?.Name +"</span>",
                            c.SubjectDetail.CourseTypeId.HasValue ? courseTypes[c.SubjectDetail.CourseTypeId.Value] : "",
                            c?.SubjectDetail?.Duration,
                            TypeLearningName(c.type_leaning.Value),
                            c?.Room?.str_Name,
                            instructor,
                            DateUtil.DateToString(c?.dtm_time_from,"dd/MM/yyyy")  +" - "+ DateUtil.DateToString(c?.dtm_time_to,"dd/MM/yyyy"),
                            (c?.time_from != null ? c?.time_from?.ToString() : "") +" - "+ (c?.time_to != null ? c?.time_to?.ToString() : ""),
                            !string.IsNullOrEmpty(c?.str_remark) ?  Regex.Replace(c?.str_remark, "[\r\n]", "<br/>") : ""
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerscheduleDetail", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectBlended(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                //var model = CourseDetailService.GetByCourse(id).Where(a=> !a.bit_Deleted);
                var model = CourseDetailService.GetBlendedByCourseId(id);


                IEnumerable<Course_Blended_Learning> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Blended_Learning, object> orderingFunction = (c => sortColumnIndex == 1 ? c.LearningType
                                                          : sortColumnIndex == 2 ? c.Duration
                                                          : sortColumnIndex == 4 ? c.DateFrom
                                                          : sortColumnIndex == 5 ? c.DateTo
                                                          : sortColumnIndex == 6 ? c.Room?.str_Name
                                                          : sortColumnIndex == 7 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                                        : (object)c.Id);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                            string.Empty,
                            c.LearningType,
                            c.Duration,
                            c.DateFrom.Value.ToString("dd/MM/yyyy"),
                            c.DateTo.Value.ToString("dd/MM/yyyy"),
                            c.RoomId.HasValue ? c?.Room?.str_Name : "",
                            ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerSubject", ex.Message);
                return Json(new
                {

                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}
