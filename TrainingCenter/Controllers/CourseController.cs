namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using RestSharp;
    using RestSharp.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.SqlServer;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Linq.Expressions;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Companies;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.Cost;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.CourseResultSummary;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Orientation;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.AjaxModels.AjaxAssignMember;
    using TMS.Core.ViewModels.AjaxModels.AjaxCourse;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.ReportModels;
    using TMS.Core.ViewModels.Subjects;
    using TMS.Core.ViewModels.ViewModel;
    using TrainingCenter.Utilities;
    using DataTable = System.Data.DataTable;

    /// <summary>
    /// Defines the <see cref="CourseController" />.
    /// </summary>
    public class CourseController : BaseAdminController
    {
        /// <summary>
        /// Defines the _repoDepartment.
        /// </summary>
        private readonly IDepartmentService _repoDepartment;

        /// <summary>
        /// Defines the _repoSubject.
        /// </summary>
        private readonly ISubjectService _repoSubject;

        /// <summary>
        /// Defines the _courseServiceCost.
        /// </summary>
        private readonly ICostService _courseServiceCost;

        /// <summary>
        /// Defines the _repoTmsApproves.
        /// </summary>
        private readonly IApproveService _repoTmsApproves;

        /// <summary>
        /// Defines the _repoJobTiltle.
        /// </summary>
        private readonly IJobtitleService _repoJobTiltle;

        /// <summary>
        /// Defines the _repoCompany.
        /// </summary>
        private readonly ICompanyService _repoCompany;

        /// <summary>
        /// Defines the _repoUser.
        /// </summary>
        private readonly IUserService _repoUser;

        /// <summary>
        /// Defines the _courseDetailService.
        /// </summary>
        private readonly ICourseDetailService _courseDetailService;

        /// <summary>
        /// Defines the _repoCourseResultSummaryService.
        /// </summary>
        private readonly ICourseResultSummaryService _repoCourseResultSummaryService;

        /// <summary>
        /// Defines the _repoOrientationService.
        /// </summary>
        private readonly IOrientationService _repoOrientationService;

        /// <summary>
        /// Defines the _repoCourseDetail.
        /// </summary>
        private readonly ICourseDetailService _repoCourseDetail;

        /// <summary>
        /// Defines the _repoEmployeeService.
        /// </summary>
        private readonly IEmployeeService _repoEmployeeService;

        /// <summary>
        /// Defines the _repoCourse.
        /// </summary>
        private readonly ICourseService _repoCourse;

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseController"/> class.
        /// </summary>
        /// <param name="configService">The configService<see cref="IConfigService"/>.</param>
        /// <param name="userContext">The userContext<see cref="IUserContext"/>.</param>
        /// <param name="notificationService">The notificationService<see cref="INotificationService"/>.</param>
        /// <param name="courseMemberService">The courseMemberService<see cref="ICourseMemberService"/>.</param>
        /// <param name="employeeService">The employeeService<see cref="IEmployeeService"/>.</param>
        /// <param name="courseDetailService">The courseDetailService<see cref="ICourseDetailService"/>.</param>
        /// <param name="repoCourse">The repoCourse<see cref="ICourseService"/>.</param>
        /// <param name="repoDepartment">The repoDepartment<see cref="IDepartmentService"/>.</param>
        /// <param name="repoSubject">The repoSubject<see cref="ISubjectService"/>.</param>
        /// <param name="repoCourseCost">The repoCourseCost<see cref="ICostService"/>.</param>
        /// <param name="repoJobTiltle">The repoJobTiltle<see cref="IJobtitleService"/>.</param>
        /// <param name="repoTmsApproves">The repoTmsApproves<see cref="IApproveService"/>.</param>
        /// <param name="repoCompany">The repoCompany<see cref="ICompanyService"/>.</param>
        /// <param name="repoUser">The repoUser<see cref="IUserService"/>.</param>
        /// <param name="repoCourseResultSummaryService">The repoCourseResultSummaryService<see cref="ICourseResultSummaryService"/>.</param>
        /// <param name="repoOrientationService">The repoOrientationService<see cref="IOrientationService"/>.</param>
        /// <param name="repoEmployeeService">The repoEmployeeService<see cref="IEmployeeService"/>.</param>
        /// <param name="repoCourseDetail">The repoCourseDetail<see cref="ICourseDetailService"/>.</param>
        public CourseController(IConfigService configService,
            IUserContext userContext,
            INotificationService notificationService,
            ICourseMemberService courseMemberService,
            IEmployeeService employeeService,
            ICourseDetailService courseDetailService,
            ICourseService repoCourse,
            IDepartmentService repoDepartment,

            ISubjectService repoSubject,
            ICostService repoCourseCost,
            IJobtitleService repoJobTiltle,
            IApproveService repoTmsApproves,
            ICompanyService repoCompany,
            IUserService repoUser,
            ICourseResultSummaryService repoCourseResultSummaryService,
            IOrientationService repoOrientationService,
            IEmployeeService repoEmployeeService,
            ICourseDetailService repoCourseDetail
            ) : base(configService,
                userContext,
                notificationService,
                courseMemberService,
                employeeService,
                courseDetailService,
                repoDepartment,
                repoCourse,
                repoTmsApproves)
        {
            _courseDetailService = courseDetailService;
            _repoDepartment = repoDepartment;
            _repoSubject = repoSubject;
            _courseServiceCost = repoCourseCost;
            _repoJobTiltle = repoJobTiltle;
            _repoTmsApproves = repoTmsApproves;
            _repoCompany = repoCompany;
            _repoUser = repoUser;
            //_configService = configService;
            _repoCourseResultSummaryService = repoCourseResultSummaryService;
            _repoOrientationService = repoOrientationService;
            _repoCourseDetail = repoCourseDetail;
            _repoEmployeeService = repoEmployeeService;
            _repoCourse = repoCourse;
        }

        /// <summary>
        /// The ExportEXCEL.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        public FileResult ExportEXCEL(FormCollection form)
        {
            var filecontent = ExportEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "CourseList.xlsx");
            }
            return null;
        }

        /// <summary>
        /// The ExportEXCEL.
        /// </summary>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private byte[] ExportEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/ProgramList.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);
            var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


            var data = CourseService.Get(a => a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) &&
                        (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
                        (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                        (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0), true);

            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;

            using (excelPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                int startRow = 9;
                //var Row = 0;
                int count = 0;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        int col = 2;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellCode = worksheet.Cells[startRow + 1, ++col];
                        cellCode.Value = item?.Code;
                        cellCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = item?.Name;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellTime = worksheet.Cells[startRow + 1, ++col];
                        cellTime.Value = (item.StartDate.HasValue ? item?.StartDate.Value.ToString("dd/MM/yyyy") : "") + " - " + (item.EndDate.HasValue ? item.EndDate.Value.ToString("dd/MM/yyyy") : "");
                        cellTime.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellTime.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTime.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        //ExcelRange cellApproval = worksheet.Cells[startRow + 1, ++col];
                        //cellApproval.Value = ReturnColumnEXCEl(item);
                        //cellApproval.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //cellApproval.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //cellApproval.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellActive = worksheet.Cells[startRow + 1, ++col];
                        cellActive.Value = item?.IsActive == true ? "Active" : "DeActive";
                        cellActive.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellActive.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellActive.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        startRow++;

                    }
                }
                bytes = excelPackage.GetAsByteArray();
            }


            return bytes;
        }

        /// <summary>
        /// The Index.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Index()
        {

            ViewBag.DepartmentList = _repoDepartment.Get().OrderBy(m => m.Name);
            ViewBag.JobTitleList = new SelectList(_repoJobTiltle.Get().OrderBy(m => m.Name), "Id", "Name");
            ViewBag.CurrentMonthOfRoomView = DateTime.Today;
            var Status = _repoTmsApproves.GetApproveStatus().Where(item => item.Value != "1" && item.Text.Trim() != "Complete");
            if (Status.Any())
            {
                foreach (var item in Status)
                {
                    switch (item.Text)
                    {
                        case "Approve ":
                            item.Text = "Edit";
                            break;
                    }
                }
            }
            ViewBag.Status = Status;
            var ApproveType = _repoTmsApproves.GetApproveTypes();
            if (ApproveType.Any())
            {
                foreach (var item in ApproveType)
                {
                    switch (item.Text)
                    {
                        case "AssignTrainee":
                            item.Text = "Trainee";
                            break;
                        case "Course":
                            item.Text = "Course";
                            break;
                        case "SubjectResult":
                            item.Text = "Result";
                            break;
                        case "CourseResult":
                            item.Text = "Final";
                            break;
                    }
                }
            }
            ViewBag.CourseTypeList = ApproveType;// new SelectList(ApproveType, "id", "Name");

            return View();
        }

        /// <summary>
        /// The Details.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Details(int id)
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }
            if (CMSUtils.IsNull(id))
            {
                return RedirectToAction("Index", "Home");
            }
            var entity = CourseService.GetById(id);
            var model = new CourseViewModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                BeginDate = entity.StartDate,
                EndDate = entity.EndDate,
                Venue = entity.Venue,
                MaxTranineeMembers = entity.NumberOfTrainee,
                MinTranineeMembers = entity.MinTrainee,
                Note = entity.Note,
                process = Returnprocess(id).ToString(),
                // DictionaryDepartments = GetDepartmentModel(false).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.DepartmentName)),
                Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster, entity.Course_TrainingCenter.Select(a => a.khoidaotao_id).ToList()),
                DepartmentIds = entity.Course_TrainingCenter.Select(a => (int)a.khoidaotao_id).ToArray()
            };
            return View(model);
        }

        /// <summary>
        /// The ReturnColumnEXCEl.
        /// </summary>
        /// <param name="course">The course<see cref="Course"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReturnColumnEXCEl(Course course)//ApprovalID
        {
            StringBuilder HTML = new StringBuilder();
            //HTML.Append("<div class='row bs-wizard' style='border-bottom:0;'>");//<div class='container'> 
            //complete active disabled
            var data_type_approval_type = UtilConstants.ApproveTypeDictionary();
            foreach (var item in data_type_approval_type)
            {

                var key = item.Key;
                var class_status = "none";

                var data = course.TMS_APPROVES.FirstOrDefault(a => a.int_Type == key);
                if (data != null)
                {
                    switch (data.int_id_status)
                    {
                        case (int)UtilConstants.EStatus.Approve:
                            class_status = "success";
                            break;
                        case (int)UtilConstants.EStatus.Pending:
                            class_status = "default";
                            break;
                        case (int)UtilConstants.EStatus.Block:
                            class_status = "warning";
                            break;
                        case (int)UtilConstants.EStatus.Reject:
                            class_status = "danger";
                            break;
                        case (int)UtilConstants.EStatus.CancelRequest:
                            class_status = "blueviolet";
                            break;
                    }
                }

                var name = "";
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

                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    var db = _repoTmsApproves.Get(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (db.Any(item_db => item_db.int_id_status != (int)UtilConstants.EStatus.Approve))
                    {
                        class_status = "primary";
                        name = "Processing";
                    }
                }
                if (class_status != "none")
                {
                    HTML.AppendFormat("{0} ", name);
                }
                else
                {
                    HTML.AppendFormat("{0} ", "");
                }
            }
            return HTML.ToString();
        }

        /// <summary>
        /// The ReturnColumn.
        /// </summary>
        /// <param name="course">The course<see cref="Course"/>.</param>
        /// <returns>The <see cref="StringBuilder"/>.</returns>
        private StringBuilder ReturnColumn(Course course)//ApprovalID
        {
            var HTML = new StringBuilder();
            //HTML.Append("<div class='row bs-wizard' style='border-bottom:0;'>");//<div class='container'> 
            //complete active disabled
            var data_type_approval_type = UtilConstants.ApproveTypeDictionary();
            foreach (var item in data_type_approval_type)
            {

                var key = item.Key;
                var class_status = "none";

                var data = course.TMS_APPROVES.FirstOrDefault(a => a.int_Type == key);
                if (data != null)
                {
                    switch (data.int_id_status)
                    {
                        case (int)UtilConstants.EStatus.Approve:
                            class_status = "success";
                            break;
                        case (int)UtilConstants.EStatus.Pending:
                            class_status = "default";
                            break;
                        case (int)UtilConstants.EStatus.Block:
                            class_status = "warning";
                            break;
                        case (int)UtilConstants.EStatus.Reject:
                            class_status = "danger";
                            break;
                        case (int)UtilConstants.EStatus.CancelRequest:
                            class_status = "blueviolet";
                            break;
                    }
                }

                var name = "";
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

                if (key == (int)UtilConstants.ApproveType.SubjectResult)
                {
                    var db = _repoTmsApproves.Get(a => a.int_Course_id == course.Id && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (db.Any(item_db => item_db.int_id_status != (int)UtilConstants.EStatus.Approve))
                    {
                        class_status = "primary";
                        name = "Processing";
                    }
                }
                HTML.AppendFormat("<span class='label label-{0}'>{1}</span>", class_status, name);
            }
            return HTML;
        }

        /// <summary>
        /// The ReturnStatusText.
        /// </summary>
        /// <param name="approves">The approves<see cref="ICollection{TMS_APPROVES}"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReturnStatusText(ICollection<TMS_APPROVES> approves)
        {
            var approve = approves.FirstOrDefault();
            if (approve == null || (approve.int_Type.HasValue && approve.int_Type.Value != (int)UtilConstants.ApproveType.SubjectResult))
                return string.Format("<span class=\"label label-{0}\">{1}</span>", "default", "");
            var displayText = "";
            var status = approve.int_id_status;
            displayText = string.IsNullOrEmpty(displayText) && UtilConstants.StatusDictionary().Keys.Contains(status.Value) ? UtilConstants.StatusDictionary()[status.Value] : "";

            switch (status.Value)
            {
                case (int)UtilConstants.EStatus.Approve:
                    return string.Format("<span class=\"label label-{0}\">{1}</span>", "success", displayText);
                case (int)UtilConstants.EStatus.Block:
                    return string.Format("<span class=\"label label-{0}\">{1}</span>", "warning", displayText);
                case (int)UtilConstants.EStatus.Reject:
                    return string.Format("<span class=\"label label-{0}\">{1}</span>", "danger", displayText);
                default:
                    return string.Format("<span class=\"label label-{0}\">{1}</span>", "default", displayText);

            }
        }

        /// <summary>
        /// The Returnprocess.
        /// </summary>
        /// <param name="CourseID">The CourseID<see cref="int?"/>.</param>
        /// <returns>The <see cref="StringBuilder"/>.</returns>
        private StringBuilder Returnprocess(int? CourseID)//ApprovalID
        {
            var HTML = new StringBuilder();
            HTML.Append("<div class='row bs-wizard' style='border-bottom:0;'>");
            var data_type_approval_type = UtilConstants.ApproveTypeDictionary();
            var data_ = CourseService.GetById(CourseID);
            var data_TMS_APPROVES = data_.TMS_APPROVES;
            int Count_SubjectApproved = ApproveService.Get(a => a.int_Course_id == data_.Id && a.int_courseDetails_Id != null, (int)UtilConstants.ApproveType.SubjectResult).Count();
            int Count_SubjectDetail = CourseDetailService.GetByCourse(data_.Id).Count();

            if (data_ != null)
            {
                foreach (var item in data_type_approval_type)
                {

                    var key = item.Key;
                    string request_info = "";
                    string status = "disabled";//disabled  active complete
                    string icon = "";
                    string display = "none";
                    //string icon_edit = "";
                    string label_status = "";
                    //var data = _repoTMS_APPROVES.Get(a => a.int_Course_id == CourseID && a.int_Type == key).FirstOrDefault();

                    var data = data_.TMS_APPROVES.FirstOrDefault(a => a.int_Type == key && a.int_Course_id == CourseID);
                    //var checkCourseFinal =
                    //    data_.TMS_APPROVES.Any(
                    //        a =>
                    //            a.int_Type == (int)UtilConstants.ApproveType.CourseResult &&
                    //            a.int_id_status == (int)UtilConstants.EStatus.Approve);




                    if (data != null)
                    {
                        // get user request,time request
                        //request_info = "<h5><i class='fa fa-user' aria-hidden='true'></i>  " + data?.USER1?.FIRSTNAME + " " + data?.USER?.LASTNAME + "</h5><h5>  <i class='far fa-clock' aria-hidden='true'></i>  " + data?.dtm_requested_date?.ToString("dd/MM/yyyy") + "</h5>" ?? "";
                        request_info = "<h5><i class='fa fa-user' aria-hidden='true'></i>  " + ReturnDisplayLanguage(data?.USER1?.LASTNAME, data?.USER1?.FIRSTNAME) + "</h5><h5>  <i class='far fa-clock' aria-hidden='true'></i>  " + data?.dtm_requested_date?.ToString("dd/MM/yyyy") + "</h5>" ?? "";



                        switch (data.int_id_status)
                        {
                            case (int)UtilConstants.EStatus.Approve:
                                status = "complete approval ";
                                icon = "<i class='fa fa-check i-in-bs-wizard' aria-hidden='true'></i>";
                                display = "block";
                                if (key != (int)UtilConstants.ApproveType.AssignedTrainee)
                                {
                                    //icon_edit = ((CMSUtils.Is_View("/Approve/" + item?.Name + "")) ? "<a href='javascript:void(0)' onclick='RemoveApprove(" + ApprovalID + "," + item?.id + ")'><i class='fa fa-unlock'></i></a>" : "");
                                }

                                label_status = "<span class='label label-success'>" + (data?.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                                break;
                            case (int)UtilConstants.EStatus.Pending:
                                status = " pending ";
                                icon = "<i class='zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                                display = "block";
                                //icon_edit = ((CMSUtils.Is_View("/Approve/" + item?.Name + "")) ? "<a href='" + @Url.Action("" + item?.Name + "", "Approve", new { id = ApprovalID }) + "'><i class='fa fa-pencil'></i></a>" : "");
                                if (Count_SubjectApproved < Count_SubjectDetail)
                                {
                                    icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                                }
                                label_status = "<span class='label label-default'>" + (data?.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                                break;
                            case (int)UtilConstants.EStatus.Block:
                                status = " block ";
                                label_status = "<span class='label label-warning'>" + (data?.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                                break;
                            case (int)UtilConstants.EStatus.Reject:
                                status = " reject ";
                                label_status = "<span class='label label-danger'>" + (data?.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                                break;
                            case (int)UtilConstants.EStatus.CancelRequest:
                                status = " CancelRequest ";
                                label_status = "<span class='label label-blueviolet'>" + (data?.int_id_status == (int)UtilConstants.EStatus.Block ? "Edit" : UtilConstants.StatusDictionary()[data.int_id_status.Value]) + "</span>";
                                break;
                        }
                    }



                    var name = "";
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

                    var iconEdit = "";
                    if (key == (int)UtilConstants.ApproveType.SubjectResult)
                    {
                        request_info = "";
                        //var db = _repoTmsApproves.Get(a => a.int_Course_id == CourseID && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                        //var datasubject = CourseDetailService.Get(a => a.CourseId == data_.Id);
                        iconEdit = "<span data-value='" + data_.Id + "' class='expand' style='cursor: pointer;' onclick='clonetableDetail(" + data_.Id + ")' ><i class='fa fa-search' aria-hidden='true' style='font-size: 16px; color: green; '></i></span>";
                       
                        var db = data_TMS_APPROVES.Where(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                        if (db.Any())
                        {
                            foreach (var item2 in db)
                            {
                                if (item2.int_id_status != (int)UtilConstants.EStatus.Approve)
                                {
                                    display = "block";
                                    status = " processing ";
                                    label_status = "<span class='label label-primary'>Processing</span>";
                                    if (Count_SubjectApproved < Count_SubjectDetail)
                                    {
                                        icon = "<i class=' zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                                    }
                                    //checkfinal = 0;
                                    break;
                                }
                            }
                            //    var checkfinal = 1;
                            //    var data2 = data_.TMS_APPROVES.FirstOrDefault(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee
                            //&& a.int_id_status == (int)UtilConstants.EStatus.Approve);
                            //if (data2 != null)
                            //{

                            //    //if (checkfinal == 0)
                            //    //{
                            //    //    display = "block";
                            //    //    status = " processing ";
                            //    //    label_status = "<span class='label label-primary'>Processing</span>";
                            //    //    icon = "<i class='zmdi zmdi-toys zmdi-hc-spin i-in-bs-wizard' style='color:red;'></i>";
                            //    //}
                            //}
                        }
                    }
                    HTML.AppendFormat("<div class='col-md-3 col-sm-3 col-xs-12 bs-wizard-step " + status + "'><div class='text-center bs-wizard-stepnum'><h4><b>" + name + "</b></h4></div> <div class='progress'><div class='progress-bar'></div></div> <a href='javascript:void(0)' class='bs-wizard-dot'><div style='width: 75%; height: 75%; margin-top: 12%; margin-left: 13%; background: #4CAF50; border-radius: 50%;'><b style='font-size: 22px; margin-left: 8px;'>" + key + "</b><b class='visible-xs' style='font-size: 16px; margin-left: 6px;'>" + key + "</b></div></a><p style='top: 17%; position: absolute; left: 56%;font-size: 16px;'>" + icon + "&nbsp;&nbsp;" + label_status + "</p> <div class='bs-wizard-info text-center'>");//icon
                    HTML.AppendFormat("<div style='display:{0};margin-top: -20px;margin-left: 117px;'>", display);
                    HTML.AppendFormat("{0}&nbsp;&nbsp;", ""); //icon_edit
                    HTML.AppendFormat("{0}", ""); //ReturnColumnLog(ApprovalID)
                    HTML.AppendFormat("</div>{0}", request_info + iconEdit);
                    HTML.Append("</div></div>");
                }
                HTML.Append("</div><div id='detail" + data_.Id + "'></div>");//</div>
            }
            return HTML;
        }

        /// <summary>
        /// The AjaxHandler.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
                var fCourseType = string.IsNullOrEmpty(Request.QueryString["fCourseType"]) ? -1 : Convert.ToInt32(Request.QueryString["fCourseType"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fstatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fstatus"].Trim());
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
                DateTime.TryParse(fSearchDateTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);

                var data = CourseService.GetListCourse(a => /*a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)) &&*/
                (string.IsNullOrEmpty(fCode) && string.IsNullOrEmpty(fName) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue && fCourseType == -1 && fStatus == -1 ? a.StartDate >= timenow : true)
                &&
                            (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) &&
                            (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
                            (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                            (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) &&
                            (fCourseType == -1 || a.TMS_APPROVES.Any(m => m.int_Type == fCourseType)) &&
                            (fStatus == -1 || a.TMS_APPROVES.Any(m => m.int_id_status == fStatus))
                            , true);
                IEnumerable<Course> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                            : sortColumnIndex == 3 ? (object)c.StartDate
                                                            : sortColumnIndex == 4 ? (object)c.IsActive
                                                            : (object)c.StartDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//UtilConstants.VerticalBar");
                var result = from c in displayed
                             select new object[] {
                                    string.Empty,

                                    c.Code,

                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+ c.Name+"</a></span><br />"+  (c.StartDate.HasValue ? c.StartDate.Value.ToString("dd/MM/yyyy") : "") +" - "+(c.EndDate.HasValue ?  c.EndDate.Value.ToString("dd/MM/yyyy") : ""),


                                    ReturnColumn(c).ToString(),

                                   (bool)!c.IsActive ?
                                   "<i class='fa fa-toggle-off' title='InActive' onclick='Set_Participate_Course(0,"+c.Id+")' aria-hidden='true' style='cursor: pointer;'></i>" :
                                   "<i class='fa fa-toggle-on' title='Active'  onclick='Set_Participate_Course(1,"+c.Id+")' aria-hidden='true' style='cursor: pointer;'></i>",


                                      ((HttpContext.User.IsInRole("/Course/Details")) ?
                                      "<a title='View' href='"+@Url.Action("Details",new{id = c.Id})+"')' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>" : "") +
                                      ((HttpContext.User.IsInRole("/Course/Create")) ?
                                       verticalBar +"<a title='Edit' href='"+@Url.Action("Create",new{id = c.Id})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" :"") +
                                      (GetUser().IsMaster ?
                                      verticalBar + ReturnDelete(c) : "")  +
                                       
                                      //verticalBar + "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle btnIcon_gray' aria-hidden='true' style=' font-size: 16px; '></i></span>" +


                                      "<span class='dropdown'><a title='View' href='javascript:void(0);' data-toggle='dropdown' id='menu"+c.Id+"'><i class='fa fa-ellipsis-h btnIcon_gray font-byhoa' aria-hidden='true' ></i></a><ul class='dropdown-menu' role='menu' aria-labelledby='menu"+c.Id+"' style='margin-left: -100px;'><li role = 'presentation' >"
                                      + ((HttpContext.User.IsInRole("/Course/Note")) ?
                                      verticalBar + "<a title='View Note' href='"+@Url.Action("Note",new{id = c.Id})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i> View Note</a>" :"") +"</li><li role = 'presentation' >"
                                      +((HttpContext.User.IsInRole("/Course/Cost")) ?
                                       verticalBar +"<a title='View Cost' href='"+@Url.Action("Cost",new{id = c.Id})+"')' data-toggle='tooltip'><i class='fas fa-search-dollar btnIcon_green font-byhoa' aria-hidden='true'></i> View Cost</a>" :"")+"</li><li role = 'presentation' >"+((HttpContext.User.IsInRole("/Course/Create")) ?verticalBar +"<a title='Duplicate course' style='display:none;' href='javascript:void(0)' onclick='duplicate_course("+c.Id+")'  data-toggle='tooltip'><i class='fa fa-clone btnIcon_green' aria-hidden='true'></i> Duplicate course</a>" :"")+"</li></ul></span>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxSelectGroupCourse.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <param name="pageIndex">The pageIndex<see cref="int"/>.</param>
        /// <param name="pageSize">The pageSize<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxSelectGroupCourse(int? id, int pageIndex, int pageSize)
        {
            if (id == null)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxSelectGroupCourse", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel() { data = null, result = true, message = Messege.ISVALID_DATA });
            }
            try
            {
                var group = _repoSubject.GetGroupSubjectItem(a => a.id_groupsubject == id && a.SubjectDetail.IsDelete != true && a.SubjectDetail.IsActive == true).AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize);
                //var group2 = _repoSubject.GetGroupSubjectById(id);
                //var group_ = group2.CAT_GROUPSUBJECT_ITEM.Where(b => b.SubjectDetail.IsDelete != true && b.SubjectDetail.IsActive == true).Skip(pageIndex * pageSize).Take(pageSize);
                if (group != null && group.Count() > 0)
                {
                    var groupSubject = group.Select(a => a.id_subject);
                    var subjectType = group.FirstOrDefault()?.SubjectDetail?.CourseTypeId;
                    return Json(new AjaxResponseViewModel() { data = groupSubject, result = true, typecourse = (int)subjectType, Runs = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var subjectType = -1;
                    return Json(new AjaxResponseViewModel() { data = null, result = true, typecourse = subjectType, Runs = false }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxSelectGroupCourse", ex.Message);
                return Json(new AjaxResponseViewModel() { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ReturnDelete.
        /// </summary>
        /// <param name="course">The course<see cref="Course"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReturnDelete(Course course)
        {
            string _return = string.Empty;
            int status = -1;
            var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.Approve);
            var data = dataCourseProccess.FirstOrDefault(a => a.int_Course_id == course.Id);
            if (data != null)
            {
                status = (int)data.int_id_status;
            }
            if (CurrentUser.IsMaster)
            {
                _return = "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + course.Id +
                          ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>";
            }
            else
            {
                _return = ((HttpContext.User.IsInRole("/Course/Index") && !course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.CourseResult && a.int_id_status == (int)UtilConstants.EStatus.Approve)) ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + course.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "");
            }
            return _return;
        }

        /// <summary>
        /// The AjaxHandlTraineeResultHasInsert.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlTraineeResultHasInsert(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var course = _courseDetailService.GetById(id);
                IEnumerable<AjaxSubDetailTrainee> data = new List<AjaxSubDetailTrainee>();
                if (course != null)
                {
                    data = course.TMS_Course_Member.Where(a => a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).OrderBy(a => a.Trainee?.str_Staff_Id).AsEnumerable().Select(a => new AjaxSubDetailTrainee()
                    {
                        Traineeid = a?.Member_Id,
                        StaffId = a?.Trainee?.str_Staff_Id,
                        FullName = ReturnDisplayLanguage(a?.Trainee.FirstName, a?.Trainee.LastName),
                        DeptCode = a?.Trainee?.Department?.Code,
                        From = a.Course_Detail.dtm_time_from.HasValue ? a?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy") : string.Empty,
                        To = a.Course_Detail.dtm_time_to.HasValue ? a?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy") : string.Empty,
                        FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                        ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                        //Remark = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Remark.Replace(System.Environment.NewLine, "<br/>") ?? string.Empty,
                        Remark = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id).Type == true ? GetRemarkCheckFail(a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id).Id) : (a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id).Remark != null ? a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Remark?.Replace("!!!!!", "<br />") : ""),
                        Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                    });
                }

                //var data = CourseMemberService.Get(b => b.Course_Details_Id == id && b.Trainee.IsActive == true).AsEnumerable().Select(a => new AjaxSubDetailTrainee()
                //{
                //    Traineeid = a?.Member_Id,
                //    //CourseDetailId = a?.Course_Details_Id,
                //    //bit_Average_Calculate = a?.Course_Detail?.SubjectDetail?.IsAverageCalculate ?? false,
                //    StaffId = a?.Trainee?.str_Staff_Id,
                //    //FullName = string.Format("{0} {1}", a?.FirstName, a?.LastName),
                //    FullName = ReturnDisplayLanguage(a?.Trainee.FirstName, a?.Trainee.LastName),
                //    DeptCode = a?.Trainee?.Department?.Code,
                //    From = a.Course_Detail.dtm_time_from.HasValue ? a?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy") : string.Empty,
                //    To = a.Course_Detail.dtm_time_to.HasValue ? a?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy") : string.Empty,
                //    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    Remark = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Remark ?? string.Empty,
                //    Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                //});

                //var data = EmployeeService.Get(a => a.IsActive == true && a.TMS_Course_Member.Any(b => b.Course_Details_Id == id && b.IsDelete == false && b.IsActive == true)).ToList()
                //        .Select(a => new AjaxSubDetailTrainee()
                //        {
                //            Traineeid = a?.Id,
                //            //CourseDetailId = a?.Course_Details_Id,
                //            //bit_Average_Calculate = a?.Course_Detail?.SubjectDetail?.IsAverageCalculate ?? false,
                //            StaffId = a?.str_Staff_Id,
                //            //FullName = string.Format("{0} {1}", a?.FirstName, a?.LastName),
                //            FullName = ReturnDisplayLanguage(a?.FirstName, a?.LastName),
                //            DeptCode = a?.Department?.Code,
                //            From = a.TMS_Course_Member.FirstOrDefault().Course_Detail.dtm_time_from.HasValue ? a?.TMS_Course_Member?.FirstOrDefault()?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy") : string.Empty,
                //            To = a.TMS_Course_Member.FirstOrDefault().Course_Detail.dtm_time_to.HasValue ? a?.TMS_Course_Member?.FirstOrDefault()?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy") : string.Empty,
                //            Point = a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.point != null && a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.point != -1 ? string.Format("{0:0.#}", a.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.point) : string.Empty,

                //            Grade = a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.point != null ? ReturnResult(a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.Course_Detail?.SubjectDetail?.Subject_Score, a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.point, a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.Result) : a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.Result,


                //            Remark = a?.Course_Result_Summary?.FirstOrDefault(b => b.CourseDetailId == id)?.Remark ?? string.Empty
                //        });

                IEnumerable<AjaxSubDetailTrainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxSubDetailTrainee, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.StaffId
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.DeptCode
                                                            : sortColumnIndex == 4 ? c.From
                                                             : sortColumnIndex == 5 ? c.FirstResultCertificate
                                                             : sortColumnIndex == 6 ? c.ReResultCertificate
                                                            : sortColumnIndex == 7 ? c.Grade
                                                            : sortColumnIndex == 7 ? c.Remark
                                                            : c.Point);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                //filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                //                    : filtered.OrderByDescending(orderingFunction);
                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(p => p.FirstResultCertificate).ThenByDescending(p => p.ReResultCertificate).ThenBy(a => a.StaffId).ThenBy(orderingFunction)
                                   : filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(p => p.FirstResultCertificate).ThenByDescending(p => p.ReResultCertificate).ThenBy(a => a.StaffId).ThenByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                     string.Empty,
                                                     c?.StaffId,
                                                     c?.FullName,
                                                     c?.DeptCode,
                                                     c?.From  +" - "+ c?.To,
                                                     c?.FirstResultCertificate,
                                                     c?.ReResultCertificate,
                                                     c?.Grade,
                                                     c?.Remark
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlTraineeResultHasInsert", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerSubjectBlended.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
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

        /// <summary>
        /// The AjaxHandlerSubject.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerSubject(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                //var model = CourseDetailService.GetByCourse(id).Where(a=> !a.bit_Deleted);
                var model = CourseDetailService.GetByCourse(id).OrderBy(a => a.SubjectDetail.Name);

                IEnumerable<Course_Detail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c => sortColumnIndex == 1 ? c.SubjectDetail?.Code
                                                          : sortColumnIndex == 2 ? c.SubjectDetail?.Name
                                                          : sortColumnIndex == 4 ? c.SubjectDetail?.RefreshCycle
                                                          : sortColumnIndex == 5 ? c.type_leaning
                                                          : sortColumnIndex == 6 ? (object)c.SubjectDetail.Subject_Score.Max(x => x.point_from)
                                                                        : c.SubjectDetail.Subject_Score.Max(x => x.point_from));


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
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.SubjectDetail?.Code +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.SubjectDetail?.Name +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +(c.Course.CourseTypeId.HasValue ? courseTypes[c.Course.CourseTypeId.Value] : "") +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +(c.SubjectDetail?.IsAverageCalculate == true ? "Yes" : "No") +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +TypeLearningName(c.type_leaning.Value) +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +return_score_pass(c.SubjectDetail) +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.SubjectDetail?.RefreshCycle +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.SubjectDetail?.Duration +"</span>",
                            "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +ReturnStatusText(c?.TMS_APPROVES) +"</span>",
                                     !string.IsNullOrEmpty(c?.str_remark) ? "<a title='View Remark' onclick='viewremark("+c?.Id+")' data-toggle='tooltip'><i class='fas fa-sticky-note btnIcon_green font-byhoa' aria-hidden='true'></i></a>" : "",
                           // c.SubjectDetail?.Code,
                           // c.SubjectDetail?.Name,
                           // c.Course.CourseTypeId.HasValue ? courseTypes[c.Course.CourseTypeId.Value] : "",
                           // c.SubjectDetail?.IsAverageCalculate == true ? "Yes" : "No",
                           //TypeLearningName(c.type_leaning.Value),
                           // return_score_pass(c.SubjectDetail),
                           // c.SubjectDetail?.RefreshCycle,
                           // c.SubjectDetail?.Duration,
                           // ReturnStatusText(c?.TMS_APPROVES),
                           // !string.IsNullOrEmpty(c?.str_remark) ? "<a title='View Remark' onclick='viewremark("+c?.Id+")' data-toggle='tooltip'><i class='fas fa-sticky-note btnIcon_green font-byhoa' aria-hidden='true'></i></a>" : "",

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

        /// <summary>
        /// The return_score_pass.
        /// </summary>
        /// <param name="subject">The subject<see cref="SubjectDetail"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string return_score_pass(SubjectDetail subject)
        {
            string return_ = "";
            var data = subject?.Subject_Score.OrderBy(a => a.point_from).Select(a => a.point_from).FirstOrDefault();
            if (data != null)
            {
                return_ = data.ToString();
            }
            return return_;
        }
        /// <summary>
        /// The CheckRoom.
        /// </summary>
        /// <param name="date_form">The date_form<see cref="string"/>.</param>
        /// <param name="date_to">The date_to<see cref="string"/>.</param>
        /// <param name="time_from">The time_from<see cref="string"/>.</param>
        /// <param name="time_to">The time_to<see cref="string"/>.</param>
        /// <param name="room_id">The room_id<see cref="int?"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult CheckRoom(string date_form, string date_to, string time_from, string time_to, int? room_id)
        {
            var check = false;
            if (!string.IsNullOrEmpty(date_form) || !string.IsNullOrEmpty(date_to))
            {
                var dateFrom = DateUtil.StringToDate(date_form, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(date_to, DateUtil.DATE_FORMAT_OUTPUT);
                var timeFrom = DateTime.ParseExact(time_from, "HH:mm", CultureInfo.InvariantCulture);
                var timeTo = DateTime.ParseExact(time_to, "HH:mm", CultureInfo.InvariantCulture);

                if (room_id != null)
                {
                    var checklist = CourseDetailService.Get(a => a.RoomId == room_id && ((a.dtm_time_from >= dateFrom && dateFrom <= a.dtm_time_to) || (a.dtm_time_from >= dateTo && dateTo <= a.dtm_time_to)));
                    if (checklist.Any())
                    {
                        foreach (var item in checklist)
                        {
                            if (!string.IsNullOrEmpty(item.time_from) || !string.IsNullOrEmpty(item.time_to))
                            {
                                var timeFromConvert = DateTime.ParseExact(item.time_from?.ToString().Substring(0, 2) + ":" + item.time_from?.ToString().Substring(2, 2), "HH:mm", CultureInfo.InvariantCulture);
                                var timeToConvert = DateTime.ParseExact(item.time_to?.ToString().Substring(0, 2) + ":" + item.time_to?.ToString().Substring(2, 2), "HH:mm", CultureInfo.InvariantCulture);
                                if ((timeFromConvert >= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert >= timeTo && timeTo <= timeToConvert))
                                {
                                    check = true;
                                    break;
                                }
                            }
                        }
                    }
                    // && ((((!string.IsNullOrEmpty(a.time_from) ? int.Parse(a.time_from) : 0 ) > timeFrom) || ((!string.IsNullOrEmpty(a.time_to) ? int.Parse(a.time_to) : 0) < timeFrom)) && (((!string.IsNullOrEmpty(a.time_from) ? int.Parse(a.time_from) : 0) > timeTo) || ((!string.IsNullOrEmpty(a.time_to) ? int.Parse(a.time_to) : 0) < timeTo)))
                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                data = check,
                result = true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The AjaxHandlerNoteDetail.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerNoteDetail(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var datacoursedetail = CourseDetailService.Get(a => a.CourseId == id).Select(a => a.Id);
                var data = CourseDetailService.GetDetailSubjectNote(a => datacoursedetail.Contains((int)a.course_detail_id));

                List<Course_Detail_Subject_Note> models = data.ToList();
                IEnumerable<Course_Detail_Subject_Note> filtered = models;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail_Subject_Note, string> orderingFunction = (c
                                                          => sortColumnIndex == 0 ? c?.Course_Detail?.Course?.Name
                                                            : sortColumnIndex == 1 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 2 ? c?.note
                                                          : c.id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }

                // var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                 //string.Empty,
                                   c?.Course_Detail?.Course?.Name,
                                   c?.Course_Detail?.SubjectDetail?.Name,
                                    c?.note//,
                                    //"<a href='javascript:void(0)'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px; '></i></a>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerNoteDetail", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerTraineeDetail.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerTraineeDetail(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var model = CourseDetailService.Get(a => a.CourseId == id);

                IEnumerable<Course_Detail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.SubjectDetail?.Code
                                                                        : sortColumnIndex == 2 ? c?.SubjectDetail?.Name
                                                                        : sortColumnIndex == 3 ? c?.SubjectDetail?.Duration
                                                                        : sortColumnIndex == 6 ? (object)c?.time_from?.ToString()
                                                                        : c.Id);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                      : filtered.OrderByDescending(orderingFunction);

                var courseTypes = UtilConstants.CourseTypesDictionary();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                             string.Empty,
                             "<span data-value='"+c?.Id+"' class='expand' style='cursor: pointer;'><a>"+c?.SubjectDetail?.Code+"</a></span>",
                              "<span data-value='"+c?.Id+"' class='expand' style='cursor: pointer;'><a>"+c?.SubjectDetail?.Name+"</a></span>",
                                courseTypes[(c?.SubjectDetail?.CourseTypeId ?? 1)] ?? string.Empty,
                             c?.SubjectDetail?.Duration,
                            DateUtil.DateToString(c?.dtm_time_from,"dd/MM/yyyy")  +" - "+ DateUtil.DateToString(c?.dtm_time_to,"dd/MM/yyyy"),
                            TypeLearningName(c.type_leaning.Value),
                           "<span data-value='"+c?.Id+"' class='expand' style='cursor: pointer;'><a><i class='fas fa-plus-circle btnIcon_gray font-byhoa' aria-hidden='true' ></i><a/></span>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerTraineeDetail", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerscheduleDetail.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
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
                                                                        : c?.SubjectDetail?.Name);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                if (sortColumnIndex == 0)
                {
                    filtered = filtered.OrderByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Offline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Online)
                                .ThenBy(a => a.dtm_time_from).ThenBy(a => a.dtm_time_to).ThenBy(a => (object)a?.time_from).ThenBy(a => (object)a?.time_to);
                }
                else
                {
                    filtered = (sortDirection == "asc") ? 
                    filtered.OrderBy(orderingFunction).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Offline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Online)
                                .ThenBy(a => a.dtm_time_from).ThenBy(a => a.dtm_time_to).ThenBy(a => (object)a?.time_from).ThenBy(a => (object)a?.time_to)
                  : filtered.OrderByDescending(orderingFunction).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Offline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Online)
                                .ThenBy(a => a.dtm_time_from).ThenBy(a => a.dtm_time_to).ThenBy(a => (object)a?.time_from).ThenBy(a => (object)a?.time_to);
                }
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var result = from c in displayed
                                 //TODO: change model
                                 //let instructor = string.Join(" ", c.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).OrderBy(a => a.Trainee.FirstName).Select(a => a.Trainee.FirstName + " " + a.Trainee.LastName).ToArray())
                             let instructor = string.Join("<br /> ", c.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).OrderBy(a => a.Trainee.FirstName).Select(a => ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName)).ToArray())
                             select new object[] {
                             string.Empty,
                             c?.SubjectDetail?.Code,

                             c.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline ?
"<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+" data-value='"+c.Id+"' class='expand'><a>" +c?.SubjectDetail?.Name +"</a></span>"
:  "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.SubjectDetail?.Name +"</span>",
                            c.SubjectDetail.CourseTypeId.HasValue ? courseTypes[c.SubjectDetail.CourseTypeId.Value] : "",
                            c?.SubjectDetail?.Duration,
                            TypeLearningName(c.type_leaning.Value),
                            c?.Room?.str_Name,
                            instructor,
                            DateUtil.DateToString(c?.dtm_time_from,"dd/MM/yyyy")  +" - "+ DateUtil.DateToString(c?.dtm_time_to,"dd/MM/yyyy"),
                            (c?.time_from != null ? c?.time_from?.ToString() : "") +" - "+ (c?.time_to != null ? c?.time_to?.ToString() : ""),
                            c?.str_remark?.Replace("\n","\r\n").Replace(System.Environment.NewLine,"<br/>"),
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

        /// <summary>
        /// The Create.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult Create(int? id)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            //var checkCommitment = GetByKey(UtilConstants.KEY_COMMITMENT);
            //var checkCostPerPerson = GetByKey(UtilConstants.KEY_COSTPERPERSON);
            var model = new CourseModifyModel()
            {
                IsDraft = true,
                IsApproved = false,
                //BeginDate =  now,
                //EndDate = now,
                DepartmentIds = new int[0],
                ProcessStep = ProcessStep((int)UtilConstants.ApproveType.Course),
                ProcessStepRequirement = false,
                // ẩn hiện Commitment
                CheckCommitment = false,// !string.IsNullOrEmpty(checkCommitment) && checkCommitment.Equals("1"),
                CheckCostPerPerson = false, //!string.IsNullOrEmpty(checkCostPerPerson) && checkCostPerPerson.Equals("1"),
                CourseDetailModels = new List<CourseDetailModel>(),
                //được phép chỉnh subject
                IsEditsubject = true,
            };
            var entity = CourseService.GetById(id);
            if (entity != null)
            {
                var courseDetails = entity.Course_Detail.Select(b => b.Id);
                var datacheckinprocess = CourseService.GetCourseResult(a => courseDetails.Contains((int)a.CourseDetailId));

                var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false && a.int_Course_id == entity.Id);

                //check step approve
                var tmsApprove = dataCourseProccess.OrderByDescending(a => a.id).FirstOrDefault();
                var lastStep = tmsApprove?.int_Type ?? (int)UtilConstants.ApproveType.Course;

                model.ProcessStepRequirement = ProcessStepRequirement(lastStep, (int)UtilConstants.ApproveType.Course);

                model.GroupSubjectId = entity.GroupSubjectId;
                model.Customer = entity.CustomerType == true ? 1 : 0;
                model.PartnerId = entity.CompanyId;
                model.CourseType = entity.CourseTypeId;
                model.MaxTranineeMembers = entity.NumberOfTrainee;
                model.MinTranineeMembers = entity.MinTrainee;
                model.Code = entity.Code;
                model.ProgramCost = entity.TrainingProgam_Cost?.LastOrDefault()?.Cost;
                model.IsPublic = entity.IsPublic == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No;
                model.IsBindSubject = entity.IsBindSubject == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No;
                model.MaxGrade = entity.MaxGrade == null ? 100 : entity.MaxGrade;
                //model.Name = entity.Name.Trim();
                //var valueName = entity.Name.Split('-');
                //var lengthName = valueName.Length;
                //if (lengthName > 1)
                //{
                //    var name = valueName.Where(a => a != valueName[0]).ToArray();
                //    var vitri = entity.Name.IndexOf(name[0]);
                //    model.Name = entity.Name.Substring(5)?.Trim();
                //}

                //model.Name = entity.Name.Split(new []{ UtilConstants.SplitNameProgram }, StringSplitOptions.None).Last().Trim();
                model.Name = entity.LastName.HasValue() ? entity.LastName.Trim() : entity.LastName;
                //model.BeginDate = entity.StartDate;
                //model.EndDate = entity.EndDate;
                model.Venue = entity.Venue;
                model.dtm_startdate = entity.StartDate.HasValue ? entity.StartDate.Value.ToString("dd/MM/yyyy") : "";
                model.dtm_enddate = entity.EndDate.HasValue ? entity.EndDate.Value.ToString("dd/MM/yyyy") : "";
                model.Note = entity.Note;
                model.Survey = entity.Survey == true ? 1 : 0;
                model.CourseType = entity.CourseTypeId;
                model.DepartmentIds = entity.Course_TrainingCenter.Select(a => (int)a.khoidaotao_id).ToArray();

                model.InProcess = datacheckinprocess.Any();
                model.CourseId = entity.Id;
                model.IsApproved = dataCourseProccess.Any(a => a.int_Type == (int)UtilConstants.ApproveType.Course && a.int_id_status == (int)UtilConstants.EStatus.Approve);
                model.HiddenButton = dataCourseProccess.Any(a => a.int_Type == (int)UtilConstants.ApproveType.Course && (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));
                model.IsFinalApproved =
                    entity.TMS_APPROVES.Any(
                        a =>
                            a.int_Type == (int)UtilConstants.ApproveType.Course &&
                            a.int_id_status == (int)UtilConstants.EStatus.Approve);
                model.IsEditsubject = entity.TMS_APPROVES.Any(
                        a =>
                            a.int_Type == (int)UtilConstants.ApproveType.Course &&
                            a.int_id_status != (int)UtilConstants.EStatus.Approve && a.int_id_status != (int)UtilConstants.EStatus.Pending);
                // CM department
                model.DepartmentId = entity.Course_TrainingCenter.Select(a => a.khoidaotao_id).ToList();


                model.CourseDetailModels = entity.Course_Detail?.Where(a => a.IsDeleted == false).Select(a => new CourseDetailModel()
                {
                    Id = a.Id,
                    TimeTo = a.time_to,
                    TimeFrom = a.time_from,
                    DateFrom = a.dtm_time_from,
                    DateTo = a.dtm_time_to,
                    dtm_time_from = a.dtm_time_from.HasValue ? a.dtm_time_from.Value.ToString("dd/MM/yyyy") : "",
                    dtm_time_to = a.dtm_time_to.HasValue ? a.dtm_time_to.Value.ToString("dd/MM/yyyy") : "",
                    LearningType = a.type_leaning.Value,
                    Registable = a.bit_Regisable == true ? 1 : 0,
                    Room = a.RoomId ?? 0,
                    SubjectId = (int)a.SubjectDetailId,
                    MarkType = a.mark_type,
                    //phan them moi
                    Time = a.Time == null ? 1 : a.Time,
                    TimeBlock = a.TimeBlock == null ? 5 : a.TimeBlock,

                    //lấy tạm 1 Teaching Assistant
                    Mentor = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Mentor)?.Instructor_Id,
                    MonitorDuration = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Mentor)?.Duration ?? 0,
                    MonitorAllowance = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Mentor)?.Allowance ?? 0,
                    Hannah = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Hannah)?.Instructor_Id,
                    ExaminerDuration = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Hannah)?.Duration ?? 0,
                    ExaminerAllowance = a.Course_Detail_Instructor?.FirstOrDefault(t => t.Type == (int)UtilConstants.TypeInstructor.Hannah)?.Allowance ?? 0,
                    SubjectInstructors = a.Course_Detail_Instructor?.Where(x => x.Trainee.IsDeleted == false && x.Type == (int)UtilConstants.TypeInstructor.Instructor /*&& x.Course_Detail.SubjectDetail.Instructor_Ability.Any(b => b.InstructorId == x.Instructor_Id)*/).Select(x => new TMS.Core.ViewModels.Courses.SubjectInstructor
                    {
                        Duration = x?.Duration,
                        Id = x?.Id,
                        //Name = x?.Trainee?.FirstName.Trim() + " " + x?.Trainee?.LastName.Trim(),
                        Name = ReturnDisplayLanguage(x?.Trainee?.FirstName, x?.Trainee?.LastName),
                        SubjectDetailId = (int)x?.Course_Detail.SubjectDetailId,
                        InstructorId = x?.Instructor_Id ?? 0,
                        InstructorAllowance = x?.Allowance ?? 0,
                    }),
                    ListInstructorBySubject = a.SubjectDetail?.Instructor_Ability?.Where(z => z.Trainee.IsDeleted == false).GroupBy(z => z.InstructorId).Select(z => z.FirstOrDefault()).ToDictionary(x => (int)x.InstructorId, x => ReturnDisplayLanguage(x.Trainee.FirstName, x.Trainee.LastName)),

                    //Blended
                    Blended = a.Course_Blended_Learning?.Where(b => b.IsActive == true && b.IsDeleted == false).OrderBy(d => d.DateFrom).Select(c => new Blended()
                    {
                        Id = c?.Id,
                        DateFrom = c?.DateFrom,
                        DateTo = c?.DateTo,
                        dtm_DateFrom_blend = c.DateFrom.HasValue ? c.DateFrom.Value.ToString("dd/MM/yyyy") : "",
                        dtm_DateTo_blend = c.DateTo.HasValue ? c.DateTo.Value.ToString("dd/MM/yyyy") : "",
                        Room = c?.Room?.str_Name,
                        RoomId = c?.RoomId,
                        LearningTypeName = c?.LearningType,
                        ExaminerId = c?.ExaminerId,
                        MarkTypecRo = c?.mark_type_cro,
                        Examiner = c.ExaminerId.HasValue ? ReturnDisplayLanguage(c?.Trainee.FirstName, c?.Trainee.LastName) : "",
                        BlendedDuration = c?.Duration,
                        BlendedAllowance = c?.Allowance ?? 0,
                    }),
                    str_remark = a.str_remark,
                });

            }
            else
            {
                model.BeginDate = DateTime.Now;
                model.EndDate = DateTime.Now;
                model.MaxGrade = 100;
            }
            loaddata(model, id);
            return View(model);
        }

        /// <summary>
        /// The loaddata.
        /// </summary>
        /// <param name="model">The model<see cref="CourseModifyModel"/>.</param>
        /// <param name="id">The id<see cref="int?"/>.</param>
        private void loaddata(CourseModifyModel model, int? id)
        {
            model.DictionaryLearningTypes = UtilConstants.LearningTypesDictionary();
            model.DictionaryAttemptsAllowed = UtilConstants.AttemptsAllowedDictionary();
            model.DictionaryGradingMethod = UtilConstants.GradingMethodDictionary();
            if (id > 0)
            {
                model.DictionaryRooms = CourseService.GetRoom_Course(/*a => a.is_Meeting == 3*/).Select(a=>new { 
                    Room_Id = a.Room_Id,
                    str_Name = a.bit_Deleted != true ? (a.isActive == true ? a.str_Name : "(Deactived) " + a.str_Name) : "(Deleted) " + a.str_Name,
                    type = a.bit_Deleted != true ? (a.isActive == true ? 1 : 2) : 3,
                }).OrderBy(a=>a.type).ThenBy(a => a.str_Name).ToDictionary(a => a.Room_Id, a => a.str_Name);
            }
            else
            {
                model.DictionaryRooms = CourseService.GetRoom(/*a => a.is_Meeting == 3*/).OrderBy(a => a.str_Name).ToDictionary(a => a.Room_Id, a => a.bit_Deleted != true ? (a.isActive == true ? a.str_Name : "(Deactived) " + a.str_Name) : "(Deleted) " + a.str_Name);
            }

            model.DictionaryIngredients = CourseService.GetCourseIngredients(null).OrderBy(b => b.Name).ToDictionary(a => a.Id, a => a.Name);

            //model.DictionaryMentor = _repoEmployeeService.GetInstructors(a => a.int_Role == (int)UtilConstants.ROLE.Instructor, true).OrderBy(a => a.LastName).ToDictionary(a => a.Id, a => a.FirstName + " " + a.LastName);
            //model.DictionaryMentor = _repoEmployeeService.GetInstructors(a => a.int_Role == (int)UtilConstants.ROLE.Instructor, true).OrderBy(a => a.LastName).ToDictionary(a => a.Id,a => ReturnDisplayLanguage(a.FirstName,a.LastName));
            /* model.DictionaryHannah = _repoUser.GetAll(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).OrderBy(a => a.LASTNAME).ToDictionary(a => a.InstructorId == null ? a.ID : (int)a.InstructorId, a => a.FIRSTNAME + " " + a.LASTNAME);*///&& a.InstructorId != null 
                                                                                                                                                                                                                                                                                                      //model.DictionaryHannah = _repoUser.GetAll(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).OrderBy(a => a.LASTNAME).ToDictionary(a => a.InstructorId == null ? a.ID : (int)a.InstructorId, a => ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME));
            model.DictionaryHannah =
                EmployeeService.Get(a => a.IsExaminer == true && a.IsActive == true)
                    .ToDictionary(b => b.Id, b => ReturnDisplayLanguage(b.FirstName, b.LastName));
            //model.DictionaryMentor = EmployeeService.Get(a => a.Trainee_Type.Any(c => c.Type == (int)UtilConstants.TypeInstructor.Mentor))
            //        .ToDictionary(b => b.Id, b => ReturnDisplayLanguage(b.FirstName, b.LastName));

            model.ListSubjects = _repoSubject.GetSubjectDetail(a => /*a.IsActive == true &&*/ a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General).OrderBy(a => a.Name).Select(a => new TMS.Core.ViewModels.Courses.SubjectDetailInfoViewModel() { Id = a.Id, Duration = (double)a.Duration, Name = a.IsDelete != true ? (a.IsActive == true ? a.Name : "(Deactived) " + a.Name) : "(Deleted) " + a.Name, Code = a.Code, int_Course_Type = a.CourseTypeId, Average_Calculator = (bool)a.IsAverageCalculate, string_Course_Type = a.Course_Type.str_Name, IsActive = (bool)a.IsActive, PassScore = a.Subject_Score.Any() ? a.Subject_Score.OrderBy(b => b.point_from).Select(b => b.point_from).FirstOrDefault() : 0, Recurrent = a.RefreshCycle });
            model.DictionaryCourseAreas = UtilConstants.CourseCourseAreasDictionary();
            model.DictionaryCourseTypes = UtilConstants.CourseTypesDictionary();
            model.DictionarySurvey = UtilConstants.YesNoDictionary();
            model.DictionaryCompanies = _repoCompany.Get(a => a.IsActive == true).OrderBy(a => a.str_Name).ToDictionary(a => a.Company_Id, a => string.Format("{0} - {1}", a.str_code, a.str_Name));
            model.DictionaryGroupSubjects = _repoSubject.GetGroupSubject(a => a.IsActive == true).OrderBy(m => m.Name).ToDictionary(a => a.id, a => a.Name);
            //model.DictionaryDepartments = GetDepartmentModel(false).OrderBy(a => a.Ancestor).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.DepartmentName));
            //CM Department
            model.Departments = GetDepartmentAcestorModelCustom(CurrentUser.IsMaster, model.DepartmentId);
            model.Code = model.Code;//string.IsNullOrEmpty(model.Code) ? CreateCodeProgram() : model.Code;
            model.DictionaryLearningTypeBlended = new Dictionary<int, string>()
            {
              {(int)UtilConstants.LearningTypes.Offline,UtilConstants.LearningTypes.Offline.ToString()} ,
                 {(int)UtilConstants.LearningTypes.Online,UtilConstants.LearningTypes.Online.ToString()}
            };
        }

        /// <summary>
        /// The CreateCodeProgram.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string CreateCodeProgram()//Create
        {
            string str_start = GetByKey("CodeProgram") ?? "";
            string EID = "";
            // string EndsWith = DateTime.Now.Year.ToString();
            var data = CourseService.GetCodeProgram(a => a.Code.Trim().ToLower().StartsWith(str_start.Trim().ToLower()));
            for (int i = data.Count() + 1; ; i++)
            {
                //if (i <= 9)
                //{
                //    EID = str_start + "000000000" + i.ToString();
                //}
                //else if (i > 9 && i <= 99)
                //{
                //    EID = str_start + "00000000" + i.ToString();

                //}
                //else if (i > 99 && i <= 999)
                //{
                //    EID = str_start + "0000000" + i.ToString();
                //}
                //else if (i > 999 && i <= 9999)
                //{
                //    EID = str_start + "000000" + i.ToString();
                //}
                //else if (i > 9999 && i <= 99999)
                //{
                //    EID = str_start + "00000" + i.ToString();
                //}
                //else if (i > 99999 && i <= 999999)
                //{
                //    EID = str_start + "0000" + i.ToString();
                //}
                //else if (i > 999999 && i <= 9999999)
                //{
                //    EID = str_start + "000" + i.ToString();
                //}
                //else if (i > 9999999 && i <= 99999999)
                //{
                //    EID = str_start + "00" + i.ToString();
                //}
                //if (i <= 9)
                //{
                //    EID = str_start+"-" + "000" + i.ToString() + "-" + DateTime.Now.Year;
                //}
                //else 

                if (i <= 9)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + "00" + i;

                }
                else if (i > 9 && i <= 99)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + "0" + i;
                }
                else if (i > 99 && i <= 999)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + i;
                }

                var data_ = CourseService.GetCodeProgram(a => a.Code.Trim().ToLower() == EID.Trim().ToLower());
                if (!data_.Any())
                {
                    break;
                }
            }

            return EID;
        }

        /// <summary>
        /// The GetGroupSubjectType.
        /// </summary>
        /// <param name="val">The val<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult GetGroupSubjectType(string val)
        {
            var type = CourseService.GetById(int.Parse(val)).CourseTypeId;
            return Json(type, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The Return_DDL_Subject.
        /// </summary>
        /// <param name="id_element">The id_element<see cref="string"/>.</param>
        /// <param name="id_subject">The id_subject<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string Return_DDL_Subject(string id_element, int id_subject = -1)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<label>Subject</label><select class='form-control subject_filter searchText1'  style='width:300px' id='form_addsubject_subject_" + id_element + "' name='form_addsubject_subject' onchange='onchangesubject(this)' >");
            html.Append("<option>--Subject--</option>");
            var data = _repoSubject.Get(a => a.bit_Deleted == false && a.int_Parent_Id != null);
            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    if (id_subject == int.Parse(item.Subject_Id.ToString()))
                    {
                        html.AppendFormat("<option value='{0}' selected>{1}</option>", item.Subject_Id, item.str_Name);
                    }
                    else
                    {
                        html.AppendFormat("<option value='{0}'>{1}</option>", item.Subject_Id, item.str_Name);
                    }
                }
            }
            html.Append("</select>");
            return html.ToString();
        }

        /// <summary>
        /// The Return_DDL_INSTRUCTOR.
        /// </summary>
        /// <param name="id_element">The id_element<see cref="string"/>.</param>
        /// <param name="id_subject">The id_subject<see cref="int"/>.</param>
        /// <param name="durantion">The durantion<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string Return_DDL_INSTRUCTOR(string id_element, int id_subject = -1, int durantion = -1)
        {
            StringBuilder html = new StringBuilder();

            var data = EmployeeService.GetAbility(a => a.SubjectDetailId == id_subject).Select(b => b.InstructorId);
            var data2 = EmployeeService.Get(a => a.IsDeleted == false && data.Contains(a.Id));
            html.Append("<div hidden class='col-md-12 instructor_h' ><div  class='form-group col-md-6'><label>Instructors</label><select class='form-control InstructorSubject intructor_dura' style='height:200px' multiple='multiple'  id='form_addsubject_instructor_" + id_element + "' name='form_addsubject_instructor'>");
            if (data2.Count() > 0)
            {
                foreach (var item in data2)
                {
                    //var fullName = item.FirstName + " " + item.LastName;
                    var fullName = ReturnDisplayLanguage(item.FirstName, item.LastName);
                    html.AppendFormat("<option  data-none='true' value='{0}'> {1}</option>", item.Id, fullName);
                }
            }
            else
            {
                html.Append("<option value='-1'>--Instructors--</option>");
            }
            html.Append("</select></div><div class='form-group col-md-6 intrustorList'> <div></div>");
            return html.ToString();
        }

        /// <summary>
        /// The Return_DDL_INSTRUCTORForEdit.
        /// </summary>
        /// <param name="id_element">The id_element<see cref="string"/>.</param>
        /// <param name="id_subject">The id_subject<see cref="int"/>.</param>
        /// <param name="CourseIndtructor">The CourseIndtructor<see cref="IEnumerable{Course_Detail_Instructor}"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string Return_DDL_INSTRUCTORForEdit(string id_element, int id_subject = -1, IEnumerable<Course_Detail_Instructor> CourseIndtructor = null)
        {
            StringBuilder html = new StringBuilder();

            var data = EmployeeService.GetAbility(a => a.SubjectDetailId == id_subject).Select(b => b.InstructorId);
            var data2 = EmployeeService.Get(a => a.IsDeleted == false && data.Contains(a.Id));
            html.Append("<div hidden class='col-md-12 intructor_duraforedit' ><div  class='form-group col-md-6'><label>Instructors</label><select class='form-control InstructorSubject intructor_dura' style='height:200px' multiple='multiple'  id='form_addsubject_instructor_" + id_element + "' name='form_addsubject_instructor'>");
            if (data2.Count() > 0)
            {
                foreach (var item in data2)
                {
                    //var fullName = item.FirstName + " " + item.LastName;
                    var fullName = ReturnDisplayLanguage(item.FirstName, item.LastName);
                    if (CourseIndtructor != null && CourseIndtructor.Select(a => a.Instructor_Id).Contains(item.Id))
                    {
                        html.AppendFormat("<option data-none='true' data-itDuration='{2}' selected='selected' value='{0}'>{1}</option>", item.Id, fullName, CourseIndtructor?.FirstOrDefault(a => a.Instructor_Id == item.Id)?.Duration);
                    }
                    else
                    {
                        html.AppendFormat("<option data-none='false' value='{0}'>{1}</option>", item.Id, fullName);
                    }
                    // html.AppendFormat("<option  data-none='true' value='{0}'> {1}</option>", item.Instructor_Id ?? item.Trainee_Id, item.str_Fullname);
                }
            }
            else
            {
                html.Append("<option value='-1'>--Instructors--</option>");
            }
            html.Append("</select></div><div class='form-group col-md-6 intrustorList'> <div></div>");
            return html.ToString();
        }

        /// <summary>
        /// The Return_DDL_Room.
        /// </summary>
        /// <param name="room">The room<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string Return_DDL_Room(int? room = null)
        {
            StringBuilder html_Room = new StringBuilder();
            html_Room.Append("<label>Room</label><select  class='form-control' onchange='checkRoom(this)' id='form_addsubject_room' name='form_addsubject_room'>");
            html_Room.Append("<option value='-1'>--Room--</option>");
            DataTable db_Room = CMSUtils.GetDataSQL("", "Room", "Room_Id,str_Name", "bit_Deleted=0 and is_Meeting=0", "str_Name");
            if (db_Room.Rows.Count > 0)
            {
                foreach (DataRow dr in db_Room.Rows)
                {
                    var id = dr["Room_Id"].ToString();
                    html_Room.AppendFormat("<option value='{0}' {2}>{1}</option>", id, dr["str_Name"], (room.HasValue && room.Value.ToString() == id ? "selected" : ""));
                }
            }
            html_Room.Append("</select>");
            return html_Room.ToString();
        }

        /// <summary>
        /// The GetValueInstructor.
        /// </summary>
        /// <param name="value_subject">The value_subject<see cref="int"/>.</param>
        /// <param name="coursedetailID">The coursedetailID<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult GetValueInstructor(int value_subject, int coursedetailID)
        {
            try
            {
                var html = new StringBuilder();
                var null_instructor = 1;
                var data = EmployeeService.GetAbility(a => a.SubjectDetailId == value_subject && a.Trainee.IsDeleted == false);
                var dataFirt = data.FirstOrDefault();
                var duration = dataFirt?.SubjectDetail.Duration ?? 0;
                var allowance = dataFirt?.Allowance ?? 0;
                var data_instructor = data.Select(a => a.Trainee).Distinct();

                if (data_instructor.Any())
                {
                    null_instructor = 0;
                    if (coursedetailID == 0)
                    {
                        foreach (var dr in data_instructor)
                        {
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                            html.AppendFormat(
                                "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}'>{1}</option>",
                                dr.Id, fullName, duration, allowance);
                        }
                    }
                    else
                    {
                        var datainstructor = data_instructor.Select(a => a.Id);
                        var courseDetail = CourseDetailService.GetById(coursedetailID);

                        foreach (var dr in data_instructor)
                        {
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                            if (courseDetail != null && courseDetail.Course_Detail_Instructor.Any(a =>
                                    a.Instructor_Id == dr.Id &&
                                    a.Type == (int)UtilConstants.TypeInstructor.Instructor))
                            {
                                html.AppendFormat(
                                    "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}' selected >{1}</option>",
                                    dr.Id, fullName, duration, allowance);
                            }
                            else
                            {
                                html.AppendFormat(
                                    "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}'>{1}</option>",
                                    dr.Id, fullName, duration, allowance);
                            }
                        }
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    result = true,
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/GetValueInstructor", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetValueMonitor.
        /// </summary>
        /// <param name="value_subject">The value_subject<see cref="int"/>.</param>
        /// <param name="monitor">The monitor<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [AllowAnonymous]
        public JsonResult GetValueMonitor(int value_subject, int monitor = 0)
        {
            try
            {
                var html = new StringBuilder();
                //html.Append("<option value='-1'>--" + Resource.lblMonitor + "--</option>");
                var null_instructor = 1;
                //var data = _repoSubject.GetSubjectDetailById(value_subject);

                var data_instructor = _repoDepartment.Get(a =>
                    a.Subject_TrainingCenter.Select(b => b.subject_id).Contains(value_subject));

                //var data_instructorxx = data.Subject_TrainingCenter.Select(a => a.Department).Distinct();

                if (data_instructor.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data_instructor)
                    {
                        var idInstructor = item.Trainee;

                        if (idInstructor != null)
                        {
                            var fullName = ReturnDisplayLanguage(idInstructor.FirstName, idInstructor.LastName);
                            if (idInstructor.Id == monitor)
                            {

                                html.AppendFormat("<option value='{0}' data-allowance='{1}' selected >{2}</option>", idInstructor.Id, idInstructor?.Monitor_Ability?.FirstOrDefault()?.Allowance ?? 0, fullName);
                            }
                            else
                            {
                                html.AppendFormat("<option value='{0}' data-allowance='{1}' >{2}</option>", idInstructor.Id, idInstructor?.Monitor_Ability?.FirstOrDefault()?.Allowance ?? 0, fullName);
                            }
                        }

                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    result = true,
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/GetValueInstructor", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetValueExaminer.
        /// </summary>
        /// <param name="value_subject">The value_subject<see cref="int"/>.</param>
        /// <param name="coursedetailID">The coursedetailID<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [AllowAnonymous]
        public JsonResult GetValueExaminer(int value_subject, int coursedetailID)
        {
            try
            {
                var html = new StringBuilder();
                html.Append("<option value='-1'>--" + Resource.lblExaminer + "--</option>");
                var null_instructor = 1;
                var data = EmployeeService.GetExaminerAbility(a => a.SubjectDetailId == value_subject && a.Trainee.IsDeleted == false);
                var dataFirt = data.FirstOrDefault();
                var duration = dataFirt?.SubjectDetail.Duration ?? 0;
                var allowance = dataFirt?.Allowance ?? 0;
                var data_instructor = data.Select(a => a.Trainee).Distinct();

                if (data_instructor.Any())
                {
                    null_instructor = 0;
                    if (coursedetailID == 0)
                    {
                        foreach (var dr in data_instructor)
                        {
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                            html.AppendFormat(
                                "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                dr.Id, fullName, duration, allowance);
                        }
                    }
                    else
                    {
                        var datainstructor = data_instructor.Select(a => a.Id);
                        var courseDetail = CourseDetailService.GetById(coursedetailID);

                        foreach (var dr in data_instructor)
                        {
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                            if (courseDetail != null && courseDetail.Course_Detail_Instructor.Any(a =>
                                    a.Instructor_Id == dr.Id &&
                                    a.Type == (int)UtilConstants.TypeInstructor.Hannah))
                            {
                                html.AppendFormat(
                                    "<option data-duration={2} data-allowance={3} value='{0}' selected >{1}</option>",
                                    dr.Id, fullName, duration, allowance);
                            }
                            else
                            {
                                html.AppendFormat(
                                    "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                    dr.Id, fullName, duration, allowance);
                            }
                        }
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    result = true,
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/GetValueInstructor", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetValueTeacher.
        /// </summary>
        /// <param name="value_subject">The value_subject<see cref="int"/>.</param>
        /// <param name="coursedetailID">The coursedetailID<see cref="int"/>.</param>
        /// <param name="monitor">The monitor<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [AllowAnonymous]
        public JsonResult GetValueTeacher(int value_subject, int coursedetailID = 0, int monitor = 0)
        {
            try
            {
                var html = new StringBuilder();
                var html_metor = new StringBuilder();
                var html_exam = new StringBuilder();
                var null_instructor = 1;
                var null_exam = 1;
                var null_mentor = 1;
                //------------Get mentor-----------------

                DataTable db_moni = CMSUtils.GetDataSQL("", "Department d INNER JOIN Subject_TrainingCenter st ON st.khoidaotao_id = d.Id INNER JOIN Trainee t ON t.Id = d.headname LEFT JOIN Monitor_Ability ma ON ma.MonitorID = t.Id", "ma.Allowance as Allowance,t.Id as ID,t.FirstName as FirstName, t.LastName as LastName", string.Format("st.subject_id = {0} AND d.headname is not null AND ISNULL(d.IsDeleted,0) <> 1", value_subject), "");
                if (db_moni.Rows.Count > 0)
                {

                    null_mentor = 0;

                    foreach (DataRow dr in db_moni.Rows)
                    {
                        var allowance = !string.IsNullOrEmpty(dr["Allowance"] + "") ? dr["Allowance"] : 0;
                        //var fullName = dr.FirstName + " " + dr.LastName;
                        var fullName = ReturnDisplayLanguage(dr["FirstName"] + "", dr["LastName"] + "");
                        if (Convert.ToInt32(dr["Id"]) == monitor)
                        {
                            html_metor.AppendFormat("<option value='{0}' data-allowance='{1}' selected >{2}</option>", dr["Id"], allowance, fullName);
                        }
                        else
                        {
                            html_metor.AppendFormat("<option value='{0}' data-allowance='{1}' >{2}</option>", dr["Id"], allowance, fullName);
                        }
                    }
                }
                else
                {
                    null_mentor = 1;
                }
                //var data_mentor = _repoDepartment.Get(a => a.Subject_TrainingCenter.Any(s => s.subject_id == value_subject) && a.headname.HasValue);
                //if (data_mentor.Any())
                //{
                //    null_mentor = 0;
                //    foreach (var item in data_mentor)
                //    {
                //        var idInstructor = item.Trainee;

                //        if (idInstructor != null)
                //        {
                //            var fullName = ReturnDisplayLanguage(idInstructor.FirstName, idInstructor.LastName);
                //            if (idInstructor.Id == monitor)
                //            {

                //                html_metor.AppendFormat("<option value='{0}' data-allowance='{1}' selected >{2}</option>", idInstructor.Id, idInstructor?.Monitor_Ability?.FirstOrDefault()?.Allowance ?? 0, fullName);
                //            }
                //            else
                //            {
                //                html_metor.AppendFormat("<option value='{0}' data-allowance='{1}' >{2}</option>", idInstructor.Id, idInstructor?.Monitor_Ability?.FirstOrDefault()?.Allowance ?? 0, fullName);
                //            }
                //        }

                //    }
                //}
                //else
                //{
                //    null_mentor = 1;
                //}

                //------------Get Examiner--------------
                DataTable db_exam = CMSUtils.GetDataSQL("", string.Format("Examiner_Ability ex INNER JOIN Trainee t ON t.Id = ex.ExaminerId INNER JOIN SubjectDetail sj ON sj.Id = ex.SubjectDetailId LEFT JOIN Course_Detail_Instructor cdi ON cdi.Id = (SELECT TOP 1 cdi2.Id FROM Course_Detail_Instructor AS cdi2 WHERE cdi2.Instructor_Id = t.Id AND cdi2.Type = 2 AND cdi2.Course_Detail_Id = {0})", coursedetailID), "sj.Duration as duration_exam, ex.Allowance as allowance_exam,t.Id as Id,T.FirstName as FirstName,t.LastName as LastName, cdi.Course_Detail_Id as detailid, cdi.Type as type", String.Format("ex.SubjectDetailId = {0} AND ISNULL(t.IsDeleted,0) <> 1 ", value_subject), "");
                if (db_exam.Rows.Count > 0)
                {
                    null_exam = 0;

                    if (coursedetailID == 0)
                    {
                        foreach (DataRow dr in db_exam.Rows)
                        {
                            var allowance_exam = !string.IsNullOrEmpty(dr["allowance_exam"] + "") ? dr["allowance_exam"] : 0;
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr["FirstName"] + "", dr["LastName"] + "");
                            var duration_exam = !string.IsNullOrEmpty(dr["duration_exam"] + "") ? dr["duration_exam"] : 0;

                            html_exam.AppendFormat(
                                "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                dr["Id"], fullName, duration_exam, allowance_exam);
                        }
                    }
                    else
                    {
                        foreach (DataRow dr in db_exam.Rows)
                        {
                            var allowance_exam = !string.IsNullOrEmpty(dr["allowance_exam"] + "") ? dr["allowance_exam"] : 0;
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr["FirstName"] + "", dr["LastName"] + "");
                            var duration_exam = !string.IsNullOrEmpty(dr["duration_exam"] + "") ? dr["duration_exam"] : 0;

                            if (!string.IsNullOrEmpty(dr["detailid"] + ""))
                            {
                                if (Convert.ToInt32(dr["detailid"]) == coursedetailID && Convert.ToInt32(dr["type"]) == (int)UtilConstants.TypeInstructor.Hannah)
                                {
                                    html_exam.AppendFormat(
                                        "<option data-duration={2} data-allowance={3} value='{0}' selected >{1}</option>",
                                        dr["Id"], fullName, duration_exam, allowance_exam);
                                }
                                else
                                {
                                    html_exam.AppendFormat(
                                        "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                         dr["Id"], fullName, duration_exam, allowance_exam);
                                }
                            }
                            else
                            {
                                html_exam.AppendFormat(
                                        "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                         dr["Id"], fullName, duration_exam, allowance_exam);
                            }
                        }
                    }
                }
                else
                {
                    null_exam = 1;
                }
                //------------Get Intructor--------------
                DataTable db_ins = CMSUtils.GetDataSQL("", string.Format("Instructor_Ability ia INNER JOIN Trainee t ON t.Id = ia.InstructorId INNER JOIN SubjectDetail sj ON sj.Id = ia.SubjectDetailId LEFT JOIN Course_Detail_Instructor cdi ON cdi.Id = (SELECT TOP 1 cdi2.Id FROM Course_Detail_Instructor AS cdi2 WHERE cdi2.Instructor_Id = t.Id AND cdi2.Type = 0 AND cdi2.Course_Detail_Id = {0})", coursedetailID), "sj.Duration as duration, ia.Allowance as allowance_ins,t.Id as Id,T.FirstName as FirstName,t.LastName as LastName, cdi.Course_Detail_Id as detailid, cdi.Type as type", String.Format("ia.SubjectDetailId = {0} AND ISNULL(t.IsDeleted,0) <> 1 ", value_subject), "");
                if (db_ins.Rows.Count > 0)
                {
                    null_instructor = 0;

                    if (coursedetailID == 0)
                    {
                        foreach (DataRow dr in db_ins.Rows)
                        {
                            var allowance_ins = !string.IsNullOrEmpty(dr["allowance_ins"] + "") ? dr["allowance_ins"] : 0;
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr["FirstName"] + "", dr["LastName"] + "");
                            var duration = !string.IsNullOrEmpty(dr["duration"] + "") ? dr["duration"] : 0;

                            html.AppendFormat(
                                "<option data-duration={2} data-allowance={3}  value='{0}' onclick='checkInstructor(this)'>{1}</option>",
                                dr["Id"], fullName, duration, allowance_ins);
                        }
                    }
                    else
                    {
                        foreach (DataRow dr in db_ins.Rows)
                        {
                            var allowance_ins = !string.IsNullOrEmpty(dr["allowance_ins"] + "") ? dr["allowance_ins"] : 0;
                            //var fullName = dr.FirstName + " " + dr.LastName;
                            var fullName = ReturnDisplayLanguage(dr["FirstName"] + "", dr["LastName"] + "");
                            var duration = !string.IsNullOrEmpty(dr["duration"] + "") ? dr["duration"] : 0;

                            if (!string.IsNullOrEmpty(dr["detailid"] + ""))
                            {
                                if (Convert.ToInt32(dr["detailid"]) == coursedetailID && Convert.ToInt32(dr["type"]) == (int)UtilConstants.TypeInstructor.Instructor)
                                {
                                    //html_exam.AppendFormat(
                                    //    "<option data-duration={2} data-allowance={3} value='{0}' selected >{1}</option>",
                                    //    dr["Id"], fullName, 0, allowance_ins);
                                    html.AppendFormat(
                                        "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}' selected >{1}</option>",
                                        dr["Id"], fullName, duration, allowance_ins);
                                }
                                else
                                {
                                    //html_exam.AppendFormat(
                                    //    "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                                    //     dr["Id"], fullName, 0, allowance_ins);
                                    html.AppendFormat(
                                    "<option data-duration={2} data-allowance={3}  value='{0}' onclick='checkInstructor(this)'>{1}</option>",
                                     dr["Id"], fullName, duration, allowance_ins);
                                }
                            }
                            else
                            {
                                html.AppendFormat(
                                   "<option data-duration={2} data-allowance={3}  value='{0}' onclick='checkInstructor(this)'>{1}</option>",
                                    dr["Id"], fullName, duration, allowance_ins);
                            }
                        }
                    }
                }
                else
                {
                    null_instructor = 1;
                }




                //var data_exam = EmployeeService.GetExaminerAbility(a => a.SubjectDetailId == value_subject && a.Trainee.IsDeleted != true);
                ////var dataFirt_exam = data_exam.FirstOrDefault();
                //var duration_exam = 0;//dataFirt_exam?.SubjectDetail.Duration ?? 0;

                //var data_instructor_exam = data_exam.Select(a => a.Trainee).Distinct();

                //if (data_instructor_exam.Any())
                //{
                //    null_exam = 0;
                //    if (coursedetailID == 0)
                //    {
                //        foreach (var dr in data_instructor_exam)
                //        {
                //            var allowance_exam = data_exam.FirstOrDefault(a => a.ExaminerId == dr.Id)?.Allowance ?? 0;
                //            //var fullName = dr.FirstName + " " + dr.LastName;
                //            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                //            html_exam.AppendFormat(
                //                "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                //                dr.Id, fullName, duration_exam, allowance_exam);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var dr in data_instructor_exam)
                //        {
                //            var allowance_exam = data_exam.FirstOrDefault(a => a.ExaminerId == dr.Id)?.Allowance ?? 0;
                //            //var fullName = dr.FirstName + " " + dr.LastName;
                //            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                //            if (dr.Course_Detail_Instructor.Any(a => a.Instructor_Id == dr.Id && a.Type == (int)UtilConstants.TypeInstructor.Hannah && a.Course_Detail_Id == coursedetailID))
                //            {
                //                html_exam.AppendFormat(
                //                    "<option data-duration={2} data-allowance={3} value='{0}' selected >{1}</option>",
                //                    dr.Id, fullName, duration_exam, allowance_exam);
                //            }
                //            else
                //            {
                //                html_exam.AppendFormat(
                //                    "<option data-duration={2} data-allowance={3} value='{0}'>{1}</option>",
                //                    dr.Id, fullName, duration_exam, allowance_exam);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    null_exam = 1;
                //}
                //------------Get Intructor--------------
                //var data = EmployeeService.GetAbility(a => a.SubjectDetailId == value_subject && a.Trainee.IsDeleted != true);
                //var data_instructor = data.Select(a => a.Trainee).Distinct();
                ////var dataFirt = data.FirstOrDefault();
                //var duration = 0;

                //if (data_instructor.Any())
                //{
                //    null_instructor = 0;
                //    if (coursedetailID == 0)
                //    {
                //        foreach (var dr in data_instructor)
                //        {
                //            var allowance = data.FirstOrDefault(a => a.InstructorId == dr.Id)?.Allowance ?? 0;
                //            //var fullName = dr.FirstName + " " + dr.LastName;
                //            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                //            html.AppendFormat(
                //                "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}'>{1}</option>",
                //                dr.Id, fullName, duration, allowance);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var dr in data_instructor)
                //        {
                //            var allowance = data.FirstOrDefault(a => a.InstructorId == dr.Id)?.Allowance ?? 0;
                //            //var fullName = dr.FirstName + " " + dr.LastName;
                //            var fullName = ReturnDisplayLanguage(dr.FirstName, dr.LastName);
                //            if (dr.Course_Detail_Instructor.Any(a => a.Instructor_Id == dr.Id && a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id == coursedetailID))
                //            {
                //                html.AppendFormat(
                //                    "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}' selected >{1}</option>",
                //                    dr.Id, fullName, duration, allowance);
                //            }
                //            else
                //            {
                //                html.AppendFormat(
                //                   "<option data-duration={2} data-allowance={3} onclick='checkInstructor(this)' value='{0}'>{1}</option>",
                //                   dr.Id, fullName, duration, allowance);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    null_instructor = 1;
                //}

                var jsonresult = Json(new
                {
                    //Get Intructor
                    result = true,
                    value_option = html.ToString(),
                    value_null = null_instructor,
                    //Get mentor
                    value_option_mentor = html_metor.ToString(),
                    value_null_mentor = null_mentor,
                    //Get exam
                    value_option_exam = html_exam.ToString(),
                    value_null_exam = null_exam,

                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/GetValueInstructor", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //không xử dụng
        /// <summary>
        /// The ChangeGroupSubjectReturnListSubject.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="idGroupsubject">The idGroupsubject<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult ChangeGroupSubjectReturnListSubject(jQueryDataTableParamModel param, string idGroupsubject)
        {
            try
            {
                int idgroupsubject = int.Parse(idGroupsubject == "" ? "0" : idGroupsubject);
                var data = _repoSubject.GetGroupSubjectItem(a => a.id_groupsubject == idgroupsubject).Select(a => a.id_subject);

                var db_ = _repoSubject.Get(a => data.Contains(a.Subject_Id) && a.int_Parent_Id != null).ToList();

                var nullline = new Subject { int_GroupSubject_id = -1, Subject_Id = -1 };
                if (!db_.Any())
                {
                    db_.Add(nullline);
                }
                IEnumerable<Subject> filtered = new List<Subject>();
                filtered = db_;

                Func<Subject, string> orderingFunction = (c => c.Subject_Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc


                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(orderingFunction)
                                : filtered.OrderBy(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let idElement = DateTime.Now.ToString("ddMMyyyyhhmmssfff")
                             select new object[] {
                                 Return_DDL_Subject(idElement,c.Subject_Id) + "<input type='hidden' id='form_addsubject_id' name='form_addsubject_id' value='-1'> <br/><br/>  " +
                                  Return_DDL_INSTRUCTOR(idElement, c.Subject_Id),
                                 "<label>Type</label><select  class='form-control' id='form_addsubject_type_Leaning' name='form_addsubject_type_Leaning'> <option value='1'>Online</option> <option selected value='0'>Classroom</option> </select>",
                                 "<label>Regisable</label><select  class='form-control' id='form_addsubject_Regisable' name='form_addsubject_Regisable'> <option  value='1'>Yes</option> <option selected value='0'>No</option> </select>",
                                 "<label>From Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_from'  class='form-control  date-picker date_from' />",
                                 "<label>To Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_to' class='form-control  date-picker date_to' />",
                                 "<label>From Time</label><input type='text' value='8:30' style='width:100px' name='form_addsubject_time_from' class='form-control  date' />",
                                 "<label>To Time</label><input type='text' value='16:30' style='width:100px' name='form_addsubject_time_to' class='form-control  date' />",
                                 Return_DDL_Room(),
                                 "<div style='font-size: 15pt; color: #df3333; cursor:pointer'><i onclick='DelRow(this)' class='fa fa-minus-square-o'></i></div>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ChangeGroupSubjectReturnListSubject", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //không xử dụng
        /// <summary>
        /// The ReturnListSubjectForEdit.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="idCourse">The idCourse<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult ReturnListSubjectForEdit(jQueryDataTableParamModel param, string idCourse)
        {
            try
            {
                int courseId = int.Parse(idCourse == "" ? "0" : idCourse);

                var data = CourseService.GetById(courseId).Course_Detail;

                var db_ = data;


                IEnumerable<Course_Detail> filtered = new List<Course_Detail>();
                filtered = db_;

                Func<Course_Detail, string> orderingFunction = (c => c.SubjectDetailId.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let idElement = DateTime.Now.ToString("ddMMyyyyhhmmssfff")
                             select new object[] {
                               Return_DDL_Subject(idElement,(int)c.SubjectDetailId) + "<input type='hidden' id='form_addsubject_id' name='form_addsubject_id' value='"+c.SubjectDetailId+"'> <br/><br/>  " +
                                  Return_DDL_INSTRUCTORForEdit(idElement, (int)c.SubjectDetailId, c.Course_Detail_Instructor),
                                 "<label>Type</label><select  class='form-control' id='form_addsubject_type_Leaning' name='form_addsubject_type_Leaning'> <option value='1'>Online</option> <option selected value='0'>Classroom</option> </select>",
                                 "<label>Regisable</label><select  class='form-control' id='form_addsubject_Regisable' name='form_addsubject_Regisable'><option "+ ((c.bit_Regisable.HasValue && c.bit_Regisable.Value) ? "selected" : "") +" value='0'>No</option>  <option "+((c.bit_Regisable.HasValue && c.bit_Regisable.Value) ? "selected" : "") +" value='1'>Yes</option> </select>",
                                 "<label>From Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_from' value='"+(c.dtm_time_from.HasValue ? c.dtm_time_from.Value.ToString("dd/MM/yyyy") : string.Empty)+"'  class='form-control  date-picker' />",
                                 "<label>To Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_to' value='"+(c.dtm_time_to.HasValue? c.dtm_time_to.Value.ToString("dd/MM/yyyy") : string.Empty)+"' class='form-control  date-picker' />",
                                 "<label>From Time</label><input type='text' value='"+c.time_from?.Substring(0,2) +':'+c?.time_from?.Substring(2,2) + " ' style='width:100px' name='form_addsubject_time_from' class='form-control  date' />",
                                 "<label>To Time</label><input type='text' value='"+c.time_to?.Substring(0,2) +':'+c?.time_to?.Substring(2,2) + "' style='width:100px' name='form_addsubject_time_to' class='form-control  date' />",
                                 Return_DDL_Room(c.RoomId),
                                 "<div style='font-size: 15pt; color: #df3333; cursor:pointer '><i onclick='DelRow(this)' class='fa fa-minus-square-o'></i></div>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ReturnListSubjectForEdit", ex.Message);

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //không xử dụng
        /// <summary>
        /// The AddRowListSubject.
        /// </summary>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult AddRowListSubject()
        {
            try
            {
                var nullline = new Subject { int_GroupSubject_id = -1, Subject_Id = -1 };
                List<Subject> filtered = new List<Subject>();
                filtered.Add(nullline);

                var result = from c in filtered
                             let idElement = DateTime.Now.ToString("ddMMyyyyhhmmssfff")
                             select new object[] {
                                 Return_DDL_Subject(idElement,c.Subject_Id) + "<input type='hidden' id='form_addsubject_id' name='form_addsubject_id' value='-1'> <br/><br/>  " +
                                  Return_DDL_INSTRUCTOR(idElement, c.Subject_Id),
                                 "<label>Type</label><select  class='form-control' id='form_addsubject_type_Leaning' name='form_addsubject_type_Leaning'> <option value='1'>Online</option> <option value='0'>Classroom</option> </select>",
                                 "<label>Regisable</label><input type='checkbox' style='margin:auto !important' name='checkRegisable' class='form-control center checkRegisable' />",
                                 "<label>From Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_from'  class='form-control  date-picker' />",
                                 "<label>To Date</label><input type='text' style='width:100px; z-index: 1151 !important;' name='form_addsubject_dtm_to' class='form-control  date-picker' />",
                                 "<label>From Time</label><input type='text' style='width:100px' name='form_addsubject_time_from' class='form-control  date' />",
                                 "<label>To Time</label><input type='text' style='width:100px' name='form_addsubject_time_to' class='form-control  date' />",
                                 Return_DDL_Room(),
                                 "<div style='font-size: 19pt;color: red; cursor:pointer'  ><i class='fa fa-minus-square-o'></i></div>"
                             };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The CreateEID.
        /// </summary>
        /// <param name="valuetype">The valuetype<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CreateEID(int valuetype = -1)
        {
            string str_start = "";
            var datapartner = _repoDepartment.GetById(valuetype);
            if (datapartner != null)
            {
                str_start = datapartner.Code;
            }
            string EID = "";
            string EndsWith = DateTime.Now.Year.ToString();
            var data = CourseService.GetCreatEID(a => a.Code.StartsWith(str_start) && a.Code.EndsWith(EndsWith));
            for (int i = data.Count() + 1; ; i++)
            {
                //if (i <= 9)
                //{
                //    EID = str_start + "000000000" + i.ToString();
                //}
                //else if (i > 9 && i <= 99)
                //{
                //    EID = str_start + "00000000" + i.ToString();

                //}
                //else if (i > 99 && i <= 999)
                //{
                //    EID = str_start + "0000000" + i.ToString();
                //}
                //else if (i > 999 && i <= 9999)
                //{
                //    EID = str_start + "000000" + i.ToString();
                //}
                //else if (i > 9999 && i <= 99999)
                //{
                //    EID = str_start + "00000" + i.ToString();
                //}
                //else if (i > 99999 && i <= 999999)
                //{
                //    EID = str_start + "0000" + i.ToString();
                //}
                //else if (i > 999999 && i <= 9999999)
                //{
                //    EID = str_start + "000" + i.ToString();
                //}
                //else if (i > 9999999 && i <= 99999999)
                //{
                //    EID = str_start + "00" + i.ToString();
                //}
                //if (i <= 9)
                //{
                //    EID = str_start+"-" + "000" + i.ToString() + "-" + DateTime.Now.Year;
                //}
                //else 

                if (i <= 9)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + "00" + i.ToString();

                }
                else if (i > 9 && i <= 99)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + "0" + i.ToString();
                }
                else if (i > 99 && i <= 999)
                {
                    EID = str_start + "-" + DateTime.Now.Year + "-" + i.ToString();
                }

                var data_ = CourseService.GetCreatEID(a => a.Code == EID);
                if (data_.Count() == 0)
                {
                    break;
                }
            }

            return Json(EID);
        }

        /// <summary>
        /// The CancelRequest.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="note">The note<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult CancelRequest(string id, string note)
        {
            var courseId = int.Parse(id);
            var course = CourseService.GetById(courseId);
            if (course == null)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/CancelRequest", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.ISVALID_DATA
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                //  var processStep = ProcessStep((int)UtilConstants.ApproveType.Course);

                Modify_TMS(false, course, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.CancelRequest, UtilConstants.ActionType.Request, note);

                //await Task.Run(() =>
                //{
                //    #region [//------- sent mail assign trainee-------//]
                //    var checkCancelRequest =
                //       ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.CourseCancelRequest);
                //    if (checkCancelRequest != null)
                //    {
                //        var user = _repoUser.GetById(CurrentUser.USER_ID);
                //        Sent_Email_TMS(null, null, user, course, null, null, (int)UtilConstants.ActionTypeSentmail.CancelRequest);                     
                //    }


                //    #endregion

                //    #region [--------CALL LMS (CRON PROGRAM)----------] [Mục đích để thông báo cho LMS biết khóa này đã bị Block để LMS đóng khóa này lại, chờ Approve lại mới mở ra]
                //    var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                //    if (!callLms)
                //    {
                //    }
                //    else
                //    {
                //        CallServices(UtilConstants.CRON_COURSE);
                //    }
                //    #endregion
                //});
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/CancelRequest", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The CheckInstructor.
        /// </summary>
        /// <param name="date_from">The date_from<see cref="string"/>.</param>
        /// <param name="date_to">The date_to<see cref="string"/>.</param>
        /// <param name="time_from">The time_from<see cref="string"/>.</param>
        /// <param name="time_to">The time_to<see cref="string"/>.</param>
        /// <param name="learningType">The learningType<see cref="int"/>.</param>
        /// <param name="instructorId">The instructorId<see cref="int?"/>.</param>
        /// <param name="courseDetailId">The courseDetailId<see cref="int?"/>.</param>
        /// <param name="listInstructor">The listInstructor<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [AllowAnonymous]
        public JsonResult CheckInstructor(string date_from, string date_to, string time_from, string time_to, int learningType, int? instructorId, int? courseDetailId, string listInstructor)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
            if (!instructorId.HasValue)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_COURSEDETAIL_INSTRUCTOR + "<br />",
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(date_from))
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_COURSEDETAIL_FROMDATE + "<br />",
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(date_to))
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_COURSEDETAIL_TODATE + "<br />",
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(time_from))
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_COURSEDETAIL_FROMTIME + "<br />",
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(time_to))
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_COURSEDETAIL_TOTIME + "<br />",
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            IList<ListInstructor> _listInstructor = new List<ListInstructor>();
            if (!string.IsNullOrEmpty(listInstructor))
            {

                string strCauHinh = string.Concat("[", listInstructor, "]");
                _listInstructor = new JavaScriptSerializer().Deserialize<IList<ListInstructor>>(strCauHinh);
            }
            if (true)//learningType == (int)UtilConstants.LearningTypes.Online)
            {
                var dateFrom = DateTime.ParseExact(date_from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var dateTo = DateTime.ParseExact(date_to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var timeFrom = DateTime.ParseExact(date_from + " " + time_from, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                var timeTo = DateTime.ParseExact(date_to + " " + time_to, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                var checklist = CourseDetailService.Get(a =>
                (!courseDetailId.HasValue || a.Id != courseDetailId) &&
                a.IsDeleted == false &&
                a.type_leaning == learningType &&
                a.Course_Detail_Instructor.Any(b => b.Instructor_Id == instructorId && b.Type == (int)UtilConstants.TypeInstructor.Instructor) &&
                ((dateFrom >= a.dtm_time_from && dateFrom <= a.dtm_time_to && dateTo >= a.dtm_time_from && dateTo <= a.dtm_time_to) || (dateFrom <= a.dtm_time_from && dateTo >= a.dtm_time_from && dateTo <= a.dtm_time_to) || (dateFrom >= a.dtm_time_from && dateFrom <= a.dtm_time_to && dateTo >= a.dtm_time_from && dateTo >= a.dtm_time_to)) &&
                !a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.CourseResult && b.int_id_status == (int)UtilConstants.EStatus.Approve));
                if (checklist.Any())
                {
                    foreach (var item in checklist)
                    {
                        if (!string.IsNullOrEmpty(item.time_from) || !string.IsNullOrEmpty(item.time_to))
                        {
                            var timeFromConvert = DateTime.ParseExact(string.Format("{0:dd/MM/yyyy}", item.dtm_time_from.Value.Date) + " " + item.time_from, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                            var timeToConvert = DateTime.ParseExact(string.Format("{0:dd/MM/yyyy}", item.dtm_time_to.Value.Date) + " " + item.time_to, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                            if ((timeFromConvert <= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert <= timeTo && timeTo <= timeToConvert))
                            {
                                var trainee = EmployeeService.GetById(instructorId);
                                var fullName = trainee.str_Staff_Id + " - " + ReturnDisplayLanguage(trainee.FirstName, trainee.LastName);
                                var dateCourseDetail = item.dtm_time_from.Value.ToString("dd/MM/yyyy") + " - " + item.dtm_time_to.Value.ToString("dd/MM/yyyy");
                                var timeCourseDetail = item.time_from + " - " + item.time_to;
                                var message = string.Format(Messege.CHECK_INSTRUCTOR_EXIST_CUSTOM, fullName,
                                    dateCourseDetail, timeCourseDetail, item.Course.Code);
                                return Json(new AjaxResponseViewModel()
                                {
                                    type = (int)UtilConstants.TypeCheck.Checked,
                                    message = message + "<br />",
                                    result = true
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in _listInstructor)
                    {
                        var ins = Convert.ToInt32(item.instructorId);
                        if (instructorId == ins)
                        {
                            if (!string.IsNullOrEmpty(item.time_from) || !string.IsNullOrEmpty(item.time_to))
                            {
                                var timeFromConvert = DateTime.ParseExact(item.dtm_time_from + " " + item.time_from, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                                var timeToConvert = DateTime.ParseExact(item.dtm_time_to + " " + item.time_to, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                                var dtm_time_from = DateTime.Parse(item.dtm_time_from, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                                var dtm_time_to = DateTime.Parse(item.dtm_time_to, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                                if ((timeFromConvert <= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert <= timeTo && timeTo <= timeToConvert))
                                {
                                    var trainee = EmployeeService.GetById(instructorId);
                                    var fullName = trainee.str_Staff_Id + " - " + ReturnDisplayLanguage(trainee.FirstName, trainee.LastName);
                                    var dateCourseDetail = dtm_time_from.ToString("dd/MM/yyyy") + " - " + dtm_time_to.ToString("dd/MM/yyyy");
                                    var timeCourseDetail = item.time_from + " - " + item.time_to;
                                    var message = string.Format(Messege.CHECK_INSTRUCTOR_EXIST, fullName,
                                        dateCourseDetail, timeCourseDetail);
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        type = (int)UtilConstants.TypeCheck.Checked,
                                        message = message + "<br />",
                                        result = true
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }
            }

            return Json(new AjaxResponseViewModel()
            {
                type = 2,
                result = true,  
                message = Messege.SUCCESS
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The Create.
        /// </summary>
        /// <param name="model">The model<see cref="CourseModifyModel"/>.</param>
        /// <param name="note">The note<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> Create(CourseModifyModel model, string note)
        {
            //var currentUser = GetUser();
            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (!string.IsNullOrEmpty(model.dtm_startdate))
                {
                    model.BeginDate = DateTime.Parse(model.dtm_startdate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }
                if (!string.IsNullOrEmpty(model.dtm_enddate))
                {
                    model.EndDate = DateTime.Parse(model.dtm_enddate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
                }


                var courseInsert = CourseService.ModifyReturnModel(model);

                // 0 : submit
                if (courseInsert != null && model.SubmitType == 0)
                {
                    //var processStep = ProcessStep((int)UtilConstants.ApproveType.Course);
                    Modify_TMS(false, courseInsert, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.Pending, UtilConstants.ActionType.Request, note);

                    //if (!processStep)
                    //{
                    //    UpdateMember(courseInsert);

                    //    await Task.Run(() =>
                    //    {
                    //        #region [-------------------------Sent mail Approve Course----------------------------]

                    //        var checkCourseCreate = ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.CourseCreate);

                    //        // var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);
                    //        var courseDetailcheck = _repoCourseDetail.GetByCourse(courseInsert.Id);
                    //        var aModel = _repoTmsApproves.Get(a => a.int_Course_id == courseInsert.Id, (int)UtilConstants.ApproveType.Course).FirstOrDefault();

                    //        if (checkCourseCreate != null)
                    //        {
                    //            if (courseDetailcheck.Any())
                    //            {
                    //                foreach (var item in courseDetailcheck)
                    //                {
                    //                    var instructor_coursedetail = _repoCourseDetail.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                    //                    if (instructor_coursedetail.Any())
                    //                    {
                    //                        foreach (var details in instructor_coursedetail)
                    //                        {
                    //                            var instructor = EmployeeService.GetById(details.Instructor_Id);
                    //                            {
                    //                                Sent_Email_TMS(instructor, null, null, courseInsert, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            Sent_Email_TMS(null, null, null, courseInsert, null, aModel.int_Requested_by, (int)UtilConstants.ActionTypeSentmail.ApprovedProgram);
                    //        }

                    //        #endregion
                    //        #region [--------CALL LMS (CRON PROGRAM)----------]
                    //        var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                    //        if (!callLms)
                    //        {
                    //            //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/Create", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate)));
                    //            //return Json(new AjaxResponseViewModel()
                    //            //{
                    //            //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), courseInsert.Name),
                    //            //    result = false
                    //            //}, JsonRequestBehavior.AllowGet);
                    //        }
                    //        else
                    //        {
                    //            CallServices(UtilConstants.CRON_COURSE);
                    //        }
                    //        #endregion
                    //    });
                    //}

                }
                var result = new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/Create", ex.Message);

                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The CheckCourse.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CheckCourse(int id)
        {
            var approveType = (int)UtilConstants.ApproveType.SubjectResult;
            var approveStatus = (int)UtilConstants.EStatus.Approve;

            var model =
                CourseService.GetCourseResult(
                    a => a.CourseDetailId == id && !string.IsNullOrEmpty(a.Result));
            var courseDetail = _repoCourseDetail.GetById(id);
            if (courseDetail.LmsStatus == StatusModify)
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = string.Format(Messege.WARNING_DELETE_COURSE_NOTSYNC, courseDetail.SubjectDetail.Name),
                }, JsonRequestBehavior.AllowGet);

            }
            if (model.Any())
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = string.Format(Messege.WARNING_DELETE_COURSE, courseDetail.SubjectDetail.Name),
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel()
            {
                result = false,
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> delete(int id = -1)
        {
            try
            {
                var course = CourseService.GetById(id);
                if (!CurrentUser.IsMaster)
                {
                    var checkDelete = course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee);
                    if (checkDelete)
                    {
                        return Json(new AjaxResponseViewModel
                        {
                            message = string.Format(Messege.WARNING_DELETE_PROGRAM, course.Name),
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (course.LMSStatus == StatusModify)
                    {
                        return Json(new AjaxResponseViewModel()
                        {
                            message = string.Format(Messege.DELETED_UNSUCCESS_SENDLMS, course.Name),
                            result = false
                        }, JsonRequestBehavior.AllowGet);

                    }
                }

                if (course != null)
                {
                    course.IsDeleted = true;
                    course.DeletedBy = CurrentUser.Username;
                    course.DeletedDate = DateTime.Now;
                    course.LMSStatus = StatusModify;
                    course.IsActive = false;
                    foreach (var item in course.Course_Detail)
                    {
                        item.LmsStatus = StatusModify;
                        item.IsDeleted = true;
                        item.IsActive = false;
                    }
                    CourseService.Update(course);
                    await Task.Run(() =>
                    {
                        var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                        if (!callLms)
                        {
                            //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/delete", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                            //return Json(new AjaxResponseViewModel()
                            //{
                            //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                            //    result = false
                            //}, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            CallServices(UtilConstants.CRON_COURSE);
                        }
                    });
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.UNSUCCESS,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The duplicate.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult duplicate(int id = -1)
        {
            try
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/duplicate", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.UNSUCCESS,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitSetParticipateCourse.
        /// </summary>
        /// <param name="isParticipate">The isParticipate<see cref="int"/>.</param>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateCourse(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idCourse = int.Parse(id);
                var course = CourseService.GetById(idCourse);
                if (isParticipate == 1)
                {
                    course.IsActive = false;
                }
                else
                {
                    course.IsActive = true;
                }
                course.LMSStatus = StatusModify;
                //var idLmsStatus = CourseService.GetCourseLms(idCourse, UtilConstants.LMSStatus.Course);
                // if (idLmsStatus != null)
                // {
                //    idLmsStatus.Status = (int)UtilConstants.ApiStatus.Modify;
                CourseService.Update(course);
                //     CourseService.UpdateCourseLmsStatus(idLmsStatus);
                // }
                await Task.Run(() =>
                {
                    var callLms = CallServices(UtilConstants.CRON_PROGRAM);
                    if (!callLms)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateCourse", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        CallServices(UtilConstants.CRON_COURSE);
                    }
                });
                return Json(new AjaxResponseViewModel { message = Messege.SUCCESS, result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateCourse", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The Note.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Note(int? id)
        {
            var courseResult = new CourseResultSearch();
            if (id.HasValue)
            {

                var course = CourseService.GetById(id);
                if (course != null)
                {
                    courseResult.Code = course.Code.HasValue() ? course.Code : "";
                    courseResult.Name = course.Name.HasValue() ? course.Name : "";
                }

            }
            return View(courseResult);
        }

        /// <summary>
        /// The DeleteNote.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult DeleteNote(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    var note = CourseDetailService.GetDetailSubjectNoteById(id.Value);
                    if (note == null)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/delete", Messege.ISVALID_DATA);
                        return Json(new AjaxResponseViewModel
                        {
                            message = Messege.ISVALID_DATA,
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }


                    note.IsDeleted = true;
                    CourseDetailService.Update(note);
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETE_SUCCESSFULLY, Resource.lblNote),
                        result = true
                    }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/DeleteNote", ex.Message);
                    return Json(new AjaxResponseViewModel
                    {
                        message = ex.Message,
                        result = false
                    }, JsonRequestBehavior.AllowGet);


                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.DELETE_UNSUCCESSFULLY, "note"),
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ModifyNote.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ModifyNote(string id)
        {
            var prefix = "New_";
            var model = new CourseDetailNoteViewModel();
            var course = 0;
            if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix))
            {
                course = Convert.ToInt32(id.Replace(prefix, ""));
                var courseInfo = CourseService.GetById(course);
                model.CourseId = courseInfo.Id;
                model.Code = courseInfo.Code;
                model.TrainingId = courseInfo.Course_TrainingCenter.FirstOrDefault()?.khoidaotao_id;
            }
            else
            {
                var noteId = 0;
                Int32.TryParse(id, out noteId);
                var note = CourseDetailService.GetDetailSubjectNoteById(noteId);
                course = note?.Course_Detail.CourseId ?? 0;
                model.CourseId = course;
                model.Subject = note?.course_detail_id;
                model.Id = note?.id;
                model.Note = note?.note;
                model.Code = note?.Course_Detail?.Course?.Code;
                model.TrainingId = note?.Course_Detail.Course?.Course_TrainingCenter.FirstOrDefault()?.khoidaotao_id;
            }

            model.Courses =
                new SelectList(CourseService.Get().OrderBy(m => m.Id).ToList(),
                    "Id", "Name", course);
            model.TrainingCenters =
                new SelectList(_repoDepartment.Get().OrderBy(a => a.Ancestor), "Id",
                    "Name");
            return View(model);
        }

        //Modify Note
        /// <summary>
        /// The Note.
        /// </summary>
        /// <param name="viewmodel">The viewmodel<see cref="CourseDetailNoteViewModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult Note(CourseDetailNoteViewModel viewmodel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var note = CourseDetailService.GetDetailSubjectNoteById(viewmodel.Id);
                    if (note == null)
                    {

                        note = new Course_Detail_Subject_Note
                        {
                            createdate = DateTime.Now,
                            IsDeleted = false,
                            note = viewmodel.Note,
                            //SubjectDetailId = viewmodel.
                            course_detail_id = viewmodel.Subject
                        };
                        CourseDetailService.Insert(note);
                    }
                    else if (note.IsDeleted.HasValue && note.IsDeleted.Value)
                    {
                        return Json(new AjaxResponseViewModel
                        {
                            message = Resource.LISTING_DATA_NOT_FOUND,
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        note.IsDeleted = false;
                        note.note = viewmodel.Note;
                        note.course_detail_id = viewmodel.Subject;
                        CourseDetailService.Update(note);
                    }

                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = Messege.SUCCESS,
                        result = true
                    };
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/Note", ex.Message);
                    return Json(new AjaxResponseViewModel
                    {
                        message = ex.Message,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new AjaxResponseViewModel
            {
                message = MessageInvalidData(ModelState),
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The AjaxHandlerListNote.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerListNote(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var code = string.IsNullOrEmpty(Request.QueryString["code"]) ? "" : Request.QueryString["code"].Trim();
                var course = string.IsNullOrEmpty(Request.QueryString["fCourseList"]) ? "" : Request.QueryString["fCourseList"].Trim();
                var subject = string.IsNullOrEmpty(Request.QueryString["subject"]) ? -1 : Convert.ToInt32(Request.QueryString["subject"]);
                var note = string.IsNullOrEmpty(Request.QueryString["fNote"]) ? "" : Request.QueryString["fNote"].Trim();
                var strFrom = string.IsNullOrEmpty(Request.QueryString["dateFrom"]) ? "" : Request.QueryString["dateFrom"].Trim();
                var strTo = string.IsNullOrEmpty(Request.QueryString["dateTo"]) ? "" : Request.QueryString["dateTo"].Trim();
                var status = string.IsNullOrEmpty(Request.QueryString["status"]) ? "" : Request.QueryString["status"].Trim();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(strFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
                DateTime.TryParse(strTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var totarow = 0;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var data = new List<Course_Detail_Subject_Note>();
                if (subject != -1 || !string.IsNullOrEmpty(note))
                {
                    data = _repoCourseDetail.GetDetailSubjectNote(a =>
                    (subject == -1 && string.IsNullOrEmpty(note) && string.IsNullOrEmpty(course) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.Course_Detail.Course.StartDate >= timenow : true) &&
                    (subject == -1 || a.course_detail_id == subject) &&
                    (string.IsNullOrEmpty(note) || a.note.Contains(note)) &&
                     (string.IsNullOrEmpty(course) || a.Course_Detail.Course.Name.Contains(course)) &&
                      (string.IsNullOrEmpty(code) || a.Course_Detail.Course.Code.Contains(code)) &&
                       (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.Course_Detail.Course.StartDate) >= 0) &&
                         (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.Course_Detail.Course.EndDate, dateTo) >= 0)
                    ).ToList();
                    totarow = data.Count();
                }
                else
                {

                    //var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.Approve);
                    var dataCourseResultProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted != true, (int)UtilConstants.ApproveType.CourseResult);
                    // danh sách khóa học đã duyệt (approved course)
                    var coursestemp = CourseService.Get(a =>
                    (string.IsNullOrEmpty(course) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                    (string.IsNullOrEmpty(course) || a.Name.Contains(course)) &&
                      (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                       (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                         (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) &&
                         !a.TMS_APPROVES.Any(x => x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.CourseResult), true
                     ).OrderByDescending(a => a.StartDate);
                    totarow = coursestemp.Count();
                    var courses = coursestemp.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                    foreach (var oCourse in courses)
                    {
                        if (oCourse.Course_Detail.Any() && oCourse.Course_Detail.Any(a => a.Course_Detail_Subject_Note.Any(x => (!x.IsDeleted.HasValue || !x.IsDeleted.Value))))
                        {
                            var subjectNotes = oCourse.Course_Detail.Where(a => a.IsDeleted != true &&
                             a.SubjectDetail.IsDelete != true).SelectMany(a => a.Course_Detail_Subject_Note).Where(x => (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                            if (subjectNotes.Count() > 0)
                            {
                                data.AddRange(subjectNotes.OrderByDescending(a => a.Course_Detail.Course.StartDate));
                            }
                            else
                            {
                                data.Add(new Course_Detail_Subject_Note()
                                {
                                    course_detail_id = -1,
                                    note = "",
                                    Course_Detail = new Course_Detail()
                                    {
                                        Course = oCourse,
                                        CourseId = oCourse.Id,
                                        SubjectDetail = new SubjectDetail()
                                    }
                                });
                            }
                        }
                        else
                        {
                            // Thêm khóa học không có note
                            data.Add(new Course_Detail_Subject_Note()
                            {
                                course_detail_id = -1,
                                note = "",
                                Course_Detail = new Course_Detail()
                                {
                                    CourseId = oCourse.Id,
                                    Course = oCourse,
                                    SubjectDetailId = -1,
                                    SubjectDetail = new SubjectDetail()
                                }
                            });
                        }
                    }
                }

                var statusId = 0;

                IEnumerable<Course_Detail_Subject_Note> notes = data;


                //if (!string.IsNullOrEmpty(status) && Int32.TryParse(status, out statusId) && UtilConstants.ReportCourseStatusDictionary.ContainsKey(statusId))
                //{

                //    var enumStatus = UtilConstants.ReportCourseStatusDictionary[statusId];
                //    notes = enumStatus == UtilConstants.status_report_training.complete
                //        ? notes.Where(a =>
                //            dataCourseResultProccess.LastOrDefault(x => a.Course_Detail.Course.Id == x.int_Course_id)?.int_id_status ==
                //            (int)UtilConstants.EStatus.Approve)
                //        : notes.Where(a =>
                //            dataCourseResultProccess.LastOrDefault(x => a.Course_Detail.Course.Id == x.int_Course_id)?.int_id_status !=
                //           (int)UtilConstants.EStatus.Approve);
                //}
                //data = notes.OrderBy(a => a.Course_Detail.Course.Name).ToList();
                //var displayed = data;
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in notes
                             select new object[] {
                                 c?.Course_Detail?.Course.Code,
                                   c?.Course_Detail?.Course?.Name,
                                   (c?.Course_Detail?.Course?.StartDate?.ToString("dd/MM/yyyy")) + ("-" +  c?.Course_Detail?.Course?.EndDate?.ToString("dd/MM/yyyy")),
                                   c?.Course_Detail?.SubjectDetail?.Name,
                                    c?.note,
                                    c?.id != 0 ? ("<a title='Edit' href='/Course/ModifyNote/"+ c?.id +"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>&nbsp;" + verticalBar +
                                    "<a title='Delete' href='javascript:void(0)' onClick='calldelete("+ c?.id+")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>") : "",
                                "<a title='Create' href='/Course/ModifyNote/New_"+c?.Course_Detail?.CourseId  +"' data-toggle='tooltip'><i class='far fa-plus-square btnIcon_blue font-byhoa' aria-hidden='true' style=''></i></a>",
                        };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totarow,
                    iTotalDisplayRecords = totarow,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerListNote", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ChangeTraningReturnCourse.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <param name="course">The course<see cref="string"/>.</param>
        /// <param name="selected">The selected<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult ChangeTraningReturnCourse(int? id, string course, int? selected)
        {
            var options = new StringBuilder("<option value='-1'> -- " + Resource.lblCourseList + " --</option>");
            var courses = CourseDetailService.Get(null);
            if (id.HasValue || !string.IsNullOrEmpty(course))
            {
                var lstCourseId = CourseService.GetTrainingCenters(a =>
                a.course_id.HasValue && a.Course.IsDeleted == false && (!id.HasValue || a.khoidaotao_id == id) &&
                (string.IsNullOrEmpty(course) || a.Course.Code.Contains(course))).Select(a => a.course_id).ToList();
                if (lstCourseId.Any())
                {
                    courses = courses.Where(
                        a =>
                            lstCourseId.Contains(a.CourseId)).OrderByDescending(a => a.Id);
                }
            }
            var lst = courses.Select(a => new { a.CourseId, a.Course.Name }).Distinct().ToList();
            if (lst.Any())
            {
                foreach (var item in lst)
                {
                    //options.AppendFormat("<option value='{0}' " + (selected.HasValue && selected.Value == item.CourseId ? "selected ='selected'" : "") + ">{1}</option>", item.CourseId, item.Code + "  -  " + item.Name);
                    options.AppendFormat("<option value='{0}' " + (selected.HasValue && selected.Value == item.CourseId ? "selected ='selected'" : "") + ">{1}</option>", item.CourseId, item.Name);
                }
            }
            return Json(new
            {
                result = true,
                value = options.ToString()
            });
        }

        /// <summary>
        /// The AjaxFilterCourseByDate.
        /// </summary>
        /// <param name="code">The code<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult AjaxFilterCourseByDate(string code)
        {
            var html = new StringBuilder("<option value=''>--" + Resource.lblCourseList + "--</option>");
            try
            {
                Expression<Func<Course, bool>> expression = a =>
                                                                 (String.IsNullOrEmpty(code) ||
                                                                  a.Code.Contains(code));

                var data = CourseService.Get(expression).OrderBy(a => a.Name);

                if (data.Any())
                {
                    foreach (var item in data)
                    {
                        html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Name);
                    }
                }
                return Json(new
                {
                    value = html.ToString(),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxFilterCourseByDate", ex.Message);
                return Json(new
                {
                    value = html,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The Cost.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Cost(int? id)
        {
            var courseResult = new CourseResultSearch();
            courseResult.GroupCosts = _courseServiceCost.GetGroupCost().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name);
            if (id.HasValue)
            {
                var course = CourseService.GetById(id);
                if (course != null)
                {
                    courseResult.Code = course.Code.HasValue() ? course.Code : "";
                    courseResult.Name = course.Name.HasValue() ? course.Name : "";

                }

            }
            return View(courseResult);
        }

        //
        /// <summary>
        /// The OnChangeCourseListInCost.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult OnChangeCourseListInCost(int id = -1)
        {
            string html = "";
            var db = CourseService.GetById(id);
            if (db != null)
            {
                html = (db.StartDate != null ? DateUtil.DateToString(db.StartDate, "dd/MM/yyyy") : "N/A") + (db.EndDate != null ? " - " + DateUtil.DateToString(db.EndDate, "dd/MM/yyyy") : "- N/A");
            }

            return Json(html);
        }

        /// <summary>
        /// The CreateCostphu.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CreateCostphu(int id)
        {
            var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.Approve);
            var dataCourse = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course);

            var courses = CourseService.Get(
                a => dataCourse.Any(x => x.Course == a), true)
                .Where(
                    a => dataCourseProccess.FirstOrDefault(x => x.Course == a) != null).OrderBy(a => a.Name);
            var catCourse =
                _courseServiceCost.Get()
                    .OrderBy(a => a.str_Name).ToDictionary(a => a.id, a => a.str_Code + " - " + a.str_Name);
            var selectedCourse = courses.FirstOrDefault(a => a.Id == id);
            var subjects = new SelectList(selectedCourse?.Course_Detail.ToDictionary(a => a.SubjectDetailId, a => a.SubjectDetail.Code + " - " + a.SubjectDetail.Name), "Key", "Value");
            ViewBag.ExpiredDate = selectedCourse?.StartDate?.ToString("dd/MM/yyyy") + "-" + selectedCourse?.EndDate?.ToString("dd/MM/yyyy");
            //ViewBag.CourseList = new SelectList(courses.Select(a=>new {a.Course_Id,a.str_Name}), "Course_Id", "str_Name",id);

            ViewBag.CourseList = selectedCourse.Id;
            ViewBag.CourseName = selectedCourse.Name;
            ViewBag.courseSubjects = subjects;
            ViewBag.CostList = catCourse;
            ViewBag.UnitList = new SelectList(_courseServiceCost.GetUnits().Where(a => a.module == "Cost").OrderBy(m => m.name).ToList(), "id", "name");
            return View();
        }

        /// <summary>
        /// The CreateCost.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CreateCost(int id)
        {
            // var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course, (int)UtilConstants.EStatus.Approve);
            //  var dataCourse = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course);

            var courses = CourseService.Get(a => a.Id == id, true).OrderBy(a => a.Name);

            var selectedCourse = courses.FirstOrDefault();
            var model = new CourseCodeModifyModel();
            if (selectedCourse != null)
            {
                model.CourseId = selectedCourse.Id;
                model.CourseName = selectedCourse.Name;
                model.DateFromTo = selectedCourse.StartDate?.ToString("dd/MM/yyyy") + "-" + selectedCourse.EndDate?.ToString("dd/MM/yyyy");
                model.Costes = new SelectList(_courseServiceCost.Get(a => a.IsActive == true && !a.str_Code.Equals("C001") && !a.str_Code.Equals("C002"))
                        .OrderBy(a => a.str_Name).ToDictionary(a => a.id, a => a.str_Code + " - " + a.str_Name), "Key", "Value");
                model.Subjects = new SelectList(selectedCourse?.Course_Detail.ToDictionary(a => a.Id, a => (a.SubjectDetail?.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + a.SubjectDetail?.Code + " - " + a.SubjectDetail?.Name), "Key", "Value");
            }
            return View(model);
        }

        /// <summary>
        /// The CreateCost.
        /// </summary>
        /// <param name="model">The model<see cref="CourseCodeModifyModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult CreateCost(CourseCodeModifyModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {


                    var newCourse = new Course_Cost();

                    newCourse.cost_id = model.Cost;
                    newCourse.course_id = model.CourseId;
                    newCourse.cost = model.CostValue;
                    newCourse.createdate = DateTime.Now;
                    newCourse.ExpectedCost = model.ExpectedCost;
                    newCourse.coursedetail_id = model.CourseDetailId;
                    var courseDetail = CourseDetailService.GetById(model.CourseDetailId);
                    if (courseDetail != null)
                    {
                        newCourse.subject_code = courseDetail.SubjectDetail.Code;
                        newCourse.subject_name = courseDetail.SubjectDetail.Name;
                    }
                    _courseServiceCost.Insert(newCourse, (int)UtilConstants.LogEvent.Insert);
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = Messege.SUCCESS,
                        result = true
                    };
                    return Json(new
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = MessageInvalidData(ModelState),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/CreateCost", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerListCost.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerListCost(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseName = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? "" : Request.QueryString["CourseList"].Trim();
                var costName = string.IsNullOrEmpty(Request.QueryString["CostList"]) ? "" : Request.QueryString["CostList"].Trim();
                var courseCode = string.IsNullOrEmpty(Request.QueryString["CourseCode"]) ? "" : Request.QueryString["CourseCode"].Trim();
                var costCode = string.IsNullOrEmpty(Request.QueryString["CostCode"]) ? "" : Request.QueryString["CostCode"].Trim();
                //var groupCost = string.IsNullOrEmpty(Request.QueryString["GroupCost"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupCost"].Trim());
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);

                var list = CourseService.Get(
                            a => (string.IsNullOrEmpty(costName) && string.IsNullOrEmpty(costCode) && string.IsNullOrEmpty(courseName) && string.IsNullOrEmpty(courseCode) ? a.StartDate >= timenow : true) &&
                                (string.IsNullOrEmpty(costName) || a.Course_Cost.Any(x => x.CAT_COSTS.str_Name.Contains(costName))) &&
                                (string.IsNullOrEmpty(costCode) || a.Course_Cost.Any(x => x.CAT_COSTS.str_Code.Contains(costCode))) &&
                                (string.IsNullOrEmpty(courseName) || a.Name.Contains(courseName)) &&
                                (string.IsNullOrEmpty(courseCode) || a.Code.Contains(courseCode)), true
                               ).OrderBy(a => a.Code);
                var courses = new Dictionary<Course, ICollection<Course_Cost>>();
                foreach (var item in list)
                {
                    if (item.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.Course)?
                                    .int_id_status == (int)UtilConstants.EStatus.Approve)
                    {
                        courses.Add(item, item.Course_Cost);
                    }
                }
                var dataSubject = _repoSubject.GetSubjectDetail(a => a.IsDelete != true);
                var data = new List<Course_Cost>();
                foreach (var course in courses)
                {
                    if (course.Value.Any())
                    {
                        var courseCost = course.Value.GroupBy(a => new { a.Course, a.CAT_COSTS, a.subject_code, a.subject_name })
                                .Select(a => new Course_Cost
                                {
                                    course_id = a.Key?.Course.Id,
                                    cost_id = a.Key?.CAT_COSTS?.id,

                                    subject_code = a.Key?.subject_code,
                                    subject_name = "<span " + (dataSubject.Where(b => b.Code == a.Key.subject_code).FirstOrDefault()?.IsActive != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" + a.Key?.subject_name + "</span>",
                                    Course = a.Key?.Course,
                                    CAT_COSTS = a.Key?.CAT_COSTS,
                                    cost = a.Sum(b => b.cost),
                                    id = a.FirstOrDefault().id,
                                })?.Where(a => a.cost > 0);
                        if (courseCost != null)
                        {
                            data.AddRange(courseCost);
                        }
                        else
                        {
                            data.Add(new Course_Cost()
                            {
                                course_id = course.Key.Id,
                                Course = course.Key,
                            });
                        }
                    }
                    else
                    {
                        data.Add(new Course_Cost()
                        {
                            course_id = course.Key.Id,
                            Course = course.Key,
                        });
                    }
                }
                IEnumerable<Course_Cost> filtered = data;
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed
                             select new object[] {
                                  c?.Course?.Code ,
                                  c?.Course?.Name,
                                  //c?.CAT_COSTS?.CAT_GROUPCOST?.Name,
                                  c?.subject_name,
                                  c?.CAT_COSTS?.str_Name,
                                  c?.cost?.ToString("#,###"),
                                  //c?.ExpectedCost?.ToString("#,###"),
                                  c?.id != 0
                                        ? ( "<a href='/Course/ModifyCost/"+ c?.id +"' data-toggle='tooltip' title='Edit'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>"+
                                           verticalBar +"<a href='javascript:void(0)' onClick='calldelete("+ c?.id+")' data-toggle='tooltip' title='Delete'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true'></i></a>"
                                           )
                                        : "",
                                    "<a title='Create' href='/Course/CreateCost/"+c?.course_id +"' data-toggle='tooltip' ><i class='far fa-plus-square btnIcon_blue font-byhoa' aria-hidden='true'></i></a>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerListCost", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new object[0]
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ModifyCost.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ModifyCost(int? id)
        {
            try
            {
                var courseCost = _courseServiceCost.GetCourseCostById(id);
                var dataSubject = _repoSubject.GetSubjectDetail(a => a.IsDelete != true);
                if (courseCost == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ModifyCost", Messege.ISVALID_DATA);
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Messege.ISVALID_DATA };
                    return RedirectToAction("Cost", "Course");
                }
                var model = new CourseCodeModifyModel()
                {
                    Courses = new SelectList(CourseService.Get(), "Id", "Name"),
                    Costes =
                        new SelectList(_courseServiceCost.Get(), "id",
                            "str_Name"),
                };
                var sumCost = string.IsNullOrEmpty(courseCost.subject_code)
                     ? _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == null).Sum(a => a.cost)
                     : _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == courseCost.subject_code).Sum(a => a.cost);
                var expectedCost = _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id)?.OrderByDescending(a => a.id)?.FirstOrDefault()?.ExpectedCost ?? 0;

                model.Id = courseCost.id;
                model.CourseId = courseCost.course_id;
                model.CourseName = courseCost.Course?.Name;
                model.DateFromTo = courseCost.Course?.StartDate?.ToString("dd/MM/yyyy") + "-" + courseCost.Course?.StartDate?.ToString("dd/MM/yyyy");
                model.Cost = courseCost.cost_id;
                model.CostValue = sumCost;
                model.CourseDetailId = courseCost.coursedetail_id ?? -1;
                model.SubjectName = (dataSubject.Where(b => b.Code == courseCost.subject_code).FirstOrDefault()?.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + courseCost.subject_name;
                model.ExpectedCost = expectedCost;

                return View(model);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ModifyCost", ex.Message);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = ex.Message };
                return RedirectToAction("Cost", "Course");
            }
        }

        /// <summary>
        /// The ModifyCost.
        /// </summary>
        /// <param name="model">The model<see cref="CourseCodeModifyModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult ModifyCost(CourseCodeModifyModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var courseCost = _courseServiceCost.GetCourseCostById(model.Id);
                    if (courseCost == null)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ModifyCost", Messege.ISVALID_DATA);
                        return Json(new AjaxResponseViewModel()
                        {
                            message = Messege.ISVALID_DATA,
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.CostValue <= 0)
                    {
                        return Json(new
                        {
                            message = Messege.UNSUCCESS,
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    var cost = string.IsNullOrEmpty(courseCost.subject_id.ToString())
                    ? _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == null).Sum(a => a.cost)
                    : _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == courseCost.subject_code).Sum(a => a.cost);

                    decimal? newCost = 0;
                    if (model.CostValue == 0)
                    {
                        newCost = cost * -1;
                    }
                    else
                    {
                        newCost = model.CostValue - cost;
                    }

                    var newCourse = new Course_Cost()
                    {
                        cost_id = courseCost.cost_id,
                        course_id = courseCost.course_id,
                        cost = newCost,
                        createdate = DateTime.Now,
                        subject_code = courseCost.subject_code,
                        subject_name = courseCost.subject_name,
                        subject_id = courseCost.subject_id,
                        coursedetail_id = courseCost.coursedetail_id,
                        ExpectedCost = model.ExpectedCost
                    };
                    _courseServiceCost.Insert(newCourse, (int)UtilConstants.LogEvent.Update);

                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = Messege.SUCCESS,
                        result = true
                    };
                    return Json(new
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = MessageInvalidData(ModelState),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ModifyCost", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The DeleteCost.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult DeleteCost(int? id)
        {
            try
            {
                var courseCost = _courseServiceCost.GetCourseCostById(id);


                if (courseCost == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/DeleteCost", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var cost = string.IsNullOrEmpty(courseCost.subject_id.ToString())
                   ? _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == null).Sum(a => a.cost)
                   : _courseServiceCost.GetCourseCost(a => a.course_id == courseCost.course_id && a.cost_id == courseCost.cost_id && a.Course.IsDeleted != true && a.subject_code == courseCost.subject_code).Sum(a => a.cost);
                var newCourse = new Course_Cost()
                {
                    cost_id = courseCost.cost_id,
                    course_id = courseCost.course_id,
                    cost = cost * (-1),
                    createdate = DateTime.Now,
                    subject_code = courseCost.subject_code,
                    subject_name = courseCost.subject_name,
                };
                _courseServiceCost.Insert(newCourse, (int)UtilConstants.LogEvent.Update);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/DeleteCost", ex.Message);
                return Json(new
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ListResultHasInsert.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ListResultHasInsert()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new CourseModifyModel();
            model.Courseses = CourseService.Get(a => a.StartDate >= timenow, true).OrderByDescending(m => m.Id).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            //model.ProcessStep = ProcessStep((int)UtilConstants.ApproveType.CourseResult);
            return View(model);
        }

        /// <summary>
        /// The AjaxHandleListResultHasInsert.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandleListResultHasInsert(jQueryDataTableParamModel param)
        {
            var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());

            try
            {
                //IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
                //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                //customCulture.NumberFormat.NumberDecimalSeparator = ".";
                //System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var data = CourseDetailService.Get(a => a.CourseId == courseList);

                IEnumerable<Course_Detail> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.SubjectDetail?.Code
                                                            : sortColumnIndex == 2 ? (object)c.SubjectDetail?.Name
                                                            : c.SubjectDetail.Name);



                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);



                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var result = from c in displayed.ToArray()
                             let duration = c.Course_Detail_Instructor.Sum(a => a.Duration)
                             select new object[] {
                        string.Empty,
                        c.SubjectDetail.Code,
                        c.SubjectDetail.Name,
                        TypeLearning(c.type_leaning.Value),
                        duration.HasValue ? (decimal)duration :0,
                        ((HttpContext.User.IsInRole("/Course/ListResultHasInsert")) ? "<a title='Edit' style='margin-right: 10px;' href='/Course/Result?idCouresDetails="+c.Id+"&courseId="+c.CourseId+"' class='' data-type='primary' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>":"")  + BtnApproval(c),
                          ReturnStatusText(c?.TMS_APPROVES)
                          };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandleListResultHasInsert", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The BtnApproval.
        /// </summary>
        /// <param name="courseDetail">The courseDetail<see cref="Course_Detail"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string BtnApproval(Course_Detail courseDetail)
        {
            string html = "";
            var verticalBal = GetByKey("VerticalBar");
            var checkApproval = courseDetail.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));
            var colorAppr = courseDetail.TMS_APPROVES?.FirstOrDefault()?.int_id_status;
            var checkResult = courseDetail.Course_Result.Any(a => a.IsDelete == false);
            if (checkResult)
            {
                if (checkApproval)
                {
                    html += "<a disabled='disabled' title='" + Resource.lbl_SENT + "' data-toggle='tooltip'><i class='fas fa-gavel "
                        + (colorAppr == (int)UtilConstants.EStatus.Pending ? "btnIcon_gray" : "btnIcon_green") +
                        "' aria-hidden='true'></i></a>&nbsp;";
                }
                else
                {
                    html += verticalBal + "<a href='javascript:void(0)' title='" + Resource.lbl_SENDREQUEST + "' onclick='callrequestSubject(" +
                           courseDetail.Id + "," + (int)UtilConstants.ApproveType.SubjectResult +
                           ")' data-toggle='tooltip'><i class='fas fa-gavel btnIcon_darkturquoise font-byhoa' aria-hidden='true'  ></i></a>&nbsp;";
                }
            }
            else
            {
                html += "";
            }


            return html;
        }

        /// <summary>
        /// The AjaxHandleListResultFinalApproval.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandleListResultFinalApproval(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());

                DataTable db = CMSUtils.GetDataSQL("", "TMS_Course_Member as cm inner join Course_Detail as cd on cm.Course_Details_Id = cd.Id inner join Course as c on c.Id = cd.CourseId", "Distinct cm.Member_Id", String.Format("c.Id = {0} and ISNULL(cm.IsDelete,0) <> 1 and ISNULL(cm.Status,0) <> 1	", courseList), "");
                var list = new List<string>();
                if (db.Rows.Count > 0)
                {
                    list = db.Rows.Cast<DataRow>().Select(row => row["Member_Id"].ToString()).ToList();
                }
                var data = CourseService.GetCourseResultFinal(a => list.Contains(a.traineeid + "") && a.courseid == courseList && a.IsDeleted != true, new int[] { (int)UtilConstants.ApproveType.CourseResult }).OrderByDescending(a => a.courseid);

                IEnumerable<Course_Result_Final> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                                                            : sortColumnIndex == 4 ? c?.Trainee?.Department?.Name
                                                            : sortColumnIndex == 5 ? c?.point
                                                            : sortColumnIndex == 6 ? c?.grade
                                                            : (object)c.point);



                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                 string.Empty,
                                                c?.Trainee?.str_Staff_Id,
                                                //c?.Trainee?.FirstName + " " + c?.Trainee?.LastName,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                                  c?.Trainee?.JobTitle?.Name,
                                                c?.Trainee?.Department?.Name,
                                               (c?.point.HasValue != null && (bool)c?.point.HasValue && c?.point != 0) ? string.Format("{0:0.#}", c?.point) : string.Empty,
                                                GetGrade(c?.grade),
                                                c?.remark ??"",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandleListResultFinalApproval", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandleListResultFinal_.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandleListResultFinal_(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var txtCoursepass = string.IsNullOrEmpty(Request.QueryString["txt_coursepass"]) ? -1 : Convert.ToInt32(Request.QueryString["txt_coursepass"].Trim());
                var txtCoursedistinction = string.IsNullOrEmpty(Request.QueryString["txt_coursedistinction"]) ? -1 : Convert.ToInt32(Request.QueryString["txt_coursedistinction"].Trim());
                var data_ = CourseDetailService.Get(a => a.CourseId == courseList && a.TMS_Course_Member.Any(x => x.IsActive == true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved))).Select(b => b.Id);
                var course_result = CourseService.GetCourseResult(a => data_.Contains((int)a.CourseDetailId));
                //var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList);
                var data =
                    CourseMemberService.Get(
                        a =>
                            data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(a => a.Id);
                var verticalBar = GetByKey("VerticalBar");
                IEnumerable<TMS_Course_Member> filtered = data.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault());
                //var filtered = data.AsEnumerable().Select(c => new AjaxFinalResultsModel()
                // {
                //     Id = (int)c.Member_Id,
                //     TraineeCode = c.Trainee.str_Staff_Id ?? "",
                //     ////FullName = c.Trainee.FirstName.Trim() + " " + c.Trainee.LastName.Trim(),
                //     FullName = ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName),
                //     JobTitleName = c.Trainee?.JobTitle?.Name ?? "",
                //     DepartmentName = c.Trainee?.Department?.Name ?? "",
                //     Point = GetResultFinal(UtilConstants.SwitchResult.Point,
                //       data_, course_result,
                //       c.traineeid, txtCoursepass,
                //       txtCoursedistinction),
                //     Grade = GetResultFinal(UtilConstants.SwitchResult.Grade,
                //       data_, course_result,
                //       c.traineeid,
                //       txtCoursepass,
                //       txtCoursedistinction),
                //     Action =
                //       "<input type='hidden' class='form-control' name='Course_Id' value='" +
                //       c.courseid + "'/>" + verticalBar +
                //       "<input type='hidden' class='form-control' name='Trainee_Id' value='" + c.traineeid + "'/>" + verticalBar +
                //       InsertFinalID_Custom(c),
                //     TraineeId = c?.traineeid,
                //     remark = c?.Course.Course_Result_Final.FirstOrDefault(m => m.traineeid == c.traineeid)?.remark,
                // });

                //var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList);
                // IEnumerable<Course_Result_Final> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                             : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName)
                                                             : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                             : c.Id.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             let remark = c?.Course_Detail.Course.Course_Result_Final.FirstOrDefault(m => m.traineeid == c.Member_Id)?.remark
                             select new object[] {
                            string.Empty,
                            c.Trainee.str_Staff_Id,
                            ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName),
                            c.Trainee?.JobTitle?.Name,
                            c.Trainee?.Department?.Name,
                            GetResultFinal(UtilConstants.SwitchResult.Point,data_, course_result,c.Member_Id, txtCoursepass,txtCoursedistinction),
                            GetResultFinal(UtilConstants.SwitchResult.Grade,data_, course_result,c.Member_Id, txtCoursepass,txtCoursedistinction),
                            "<textarea name='txt_Remark' id='txt_Remark' name='txt_Remark' >"+remark+"</textarea>",
                             "<input type='hidden' class='form-control' name='Course_Id' value='" + c.Course_Detail.CourseId + "'/>" + verticalBar + "<input type='hidden' class='form-control' name='Trainee_Id' value='" + c.Member_Id + "'/>" + verticalBar + InsertFinalID_Custom(c.Course_Detail.Course.Course_Result_Final.FirstOrDefault()) + "<a><i  class='fas fa-arrow-circle-up' onclick='updateResult(this)' data-courseid="+courseList+" data-traineeid='"+c?.Member_Id+"' title='Update Result'></i></a>",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandleListResultFinal", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandleListResultFinal.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandleListResultFinal(jQueryDataTableParamModel param)
        {
            try
            {
                IList<RemarkTrainee> LtsRemarkTrainee = new List<RemarkTrainee>();
                if (!string.IsNullOrEmpty(Request.QueryString["RemarkTrainee"]))
                {
                    string RemarkTrainee = Request.QueryString["RemarkTrainee"];
                    string strCauHinh = string.Concat("[", RemarkTrainee, "]");
                    LtsRemarkTrainee = new JavaScriptSerializer().Deserialize<IList<RemarkTrainee>>(strCauHinh);
                }

                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var txtCoursepass = string.IsNullOrEmpty(Request.QueryString["txt_coursepass"]) ? -1 : Convert.ToInt32(Request.QueryString["txt_coursepass"].Trim());
                var txtCoursedistinction = string.IsNullOrEmpty(Request.QueryString["txt_coursedistinction"]) ? -1 : Convert.ToInt32(Request.QueryString["txt_coursedistinction"].Trim());
                var data_ = CourseDetailService.Get(a => a.CourseId == courseList).Select(b => b.Id);
                var course_result = CourseService.GetCourseResult(a => data_.Contains((int)a.CourseDetailId));
                var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList && a.Trainee.TMS_Course_Member.Any(x => data_.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)));
                //var data =
                //    CourseMemberService.Get(
                //        a =>
                //            data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(a => a.Id);
                var verticalBar = GetByKey("VerticalBar");
                IEnumerable<Course_Result_Final> filtered = data;
                //var filtered = data.AsEnumerable().Select(c => new AjaxFinalResultsModel()
                // {
                //     Id = (int)c.Member_Id,
                //     TraineeCode = c.Trainee.str_Staff_Id ?? "",
                //     ////FullName = c.Trainee.FirstName.Trim() + " " + c.Trainee.LastName.Trim(),
                //     FullName = ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName),
                //     JobTitleName = c.Trainee?.JobTitle?.Name ?? "",
                //     DepartmentName = c.Trainee?.Department?.Name ?? "",
                //     Point = GetResultFinal(UtilConstants.SwitchResult.Point,
                //       data_, course_result,
                //       c.traineeid, txtCoursepass,
                //       txtCoursedistinction),
                //     Grade = GetResultFinal(UtilConstants.SwitchResult.Grade,
                //       data_, course_result,
                //       c.traineeid,
                //       txtCoursepass,
                //       txtCoursedistinction),
                //     Action =
                //       "<input type='hidden' class='form-control' name='Course_Id' value='" +
                //       c.courseid + "'/>" + verticalBar +
                //       "<input type='hidden' class='form-control' name='Trainee_Id' value='" + c.traineeid + "'/>" + verticalBar +
                //       InsertFinalID_Custom(c),
                //     TraineeId = c?.traineeid,
                //     remark = c?.Course.Course_Result_Final.FirstOrDefault(m => m.traineeid == c.traineeid)?.remark,
                // });

                //var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList);
                // IEnumerable<Course_Result_Final> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, string> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                             : sortColumnIndex == 2 ? ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                                                             : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                                                             : sortColumnIndex == 4 ? c?.Trainee?.Department?.Name
                                                             : c?.id.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                if (filtered != null)
                {
                    filtered = (sortDirection == "asc") ? filtered?.OrderBy(orderingFunction)
                    : filtered?.OrderByDescending(orderingFunction);
                }

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                            string.Empty,
                            c?.Trainee?.str_Staff_Id,
                            ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName),
                            c?.Trainee?.JobTitle?.Name,
                            c?.Trainee?.Department?.Name,
                            GetResultFinal(UtilConstants.SwitchResult.Point,data_, course_result,c?.traineeid, txtCoursepass,txtCoursedistinction),
                            GetResultFinal(UtilConstants.SwitchResult.Grade,data_, course_result,c?.traineeid, txtCoursepass,txtCoursedistinction),
                            "<textarea name='txt_Remark' data-traineeid='"+c.traineeid+"' id='txt_Remark' name='txt_Remark' >"+(string.IsNullOrEmpty(c?.remark) ? LtsRemarkTrainee.FirstOrDefault(p=>p.idTrainee == c?.traineeid)?.remarkTrainee ?? "" : c?.remark)+"</textarea>",
                             "<input type='hidden' class='form-control' name='Course_Id' value='" + c?.courseid + "'/>" + verticalBar + "<input type='hidden' class='form-control' name='Trainee_Id' value='" + c?.traineeid + "'/>" + verticalBar + InsertFinalID_Custom(c) + "<a><i  class='fas fa-arrow-circle-up' onclick='updateResult(this)' data-courseid="+courseList+" data-traineeid='"+c?.traineeid+"' title='Update Result'></i></a>",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandleListResultFinal", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //TODO:DELETE
        /// <summary>
        /// The UpdateResult.
        /// </summary>
        /// <param name="CourseId">The CourseId<see cref="int"/>.</param>
        /// <param name="TraineeId">The TraineeId<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult UpdateResult(int CourseId, int TraineeId)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            try
            {
                /*
                 * Xử lý điểm cho học viên dự thính(Học viên học khóa trước nhưng bị final môn, nên khóa hiện tại chỉ học những môn rớt)
                 * vấn đề : HV chỉ học những môn rớt từ khóa trước,nên những môn còn lại của khóa hiện tại không có điểm,dẫn đến final sai.
                 * giải pháp: lấy điễm của những môn đã final cũa những khóa trước mà khóa hiện tại fail để update.
                 */

                //lấy id môn học của khóa
                var getCourseDetails = CourseDetailService.Get(a => a.CourseId == CourseId);
                var getCourseDetails_ = getCourseDetails.Select(b => b.Id);
                //lấy môn học bị rớt
                var getCourseResultFail = CourseService.GetCourseResult(
                    a => getCourseDetails_.Contains(a.CourseDetailId.Value)
                    && a.TraineeId == TraineeId && (string.IsNullOrEmpty(a.Re_Check_Result) ? a.First_Check_Result == "F" : a.Re_Check_Result == "F"));


                var getCourseResultFail_ = getCourseResultFail.Select(b => b.CourseDetailId);

                //lộc courseDetailsLai lay subject id
                var subjectId = getCourseDetails.Where(a => getCourseResultFail_.Contains(a.Id)).Select(a => a.SubjectDetailId);
                //find mon hoc lai cua hoc vien (loai bo mon hoc khoa nay, thoi gian lon hon khoa hien tai) va môn học thuộc khóa học đã được approve final result
                var recourse_result = CourseService.GetCourseResult(a => subjectId.Contains(a.Course_Detail.SubjectDetailId)
                                                                 && !getCourseResultFail_.Contains(a.CourseDetailId) && a.TraineeId == TraineeId).AsEnumerable();
                // update getCourseResultFail
                foreach (var item in getCourseResultFail.ToList())
                {
                    var updateitem = recourse_result?.Where(a => a.Course_Detail.SubjectDetailId == item.Course_Detail.SubjectDetailId
                                                                && a.Course_Detail.Course.TMS_APPROVES.Where(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.CourseResult).Any()).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                    if (updateitem != null)
                    {
                        item.First_Check_Result = updateitem.First_Check_Result;
                        item.First_Check_Score = updateitem.First_Check_Score;
                        item.Re_Check_Result = updateitem.Re_Check_Result;
                        item.Re_Check_Score = updateitem.Re_Check_Score;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = CurrentUser.USER_ID.ToString();
                        item.LmsStatus = StatusIsSync;
                        item.Remark = updateitem.Remark;
                        CourseService.UpdateCourseResult(item);
                    }

                }
                return Json(CMSUtils.alert("success", Messege.SUCCESS));
            }
            catch (Exception ex)
            {
                return Json(CMSUtils.alert("danger", Messege.UNSUCCESS));

            }
        }

        /// <summary>
        /// The InsertFinalID.
        /// </summary>
        /// <param name="courseID">The courseID<see cref="int?"/>.</param>
        /// <param name="traineeid">The traineeid<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string InsertFinalID(int? courseID = -1, int? traineeid = -1)
        {
            string _return = "";

            var data = CourseService.GetCourseResultFinal(a => a.traineeid == traineeid && a.courseid == courseID).FirstOrDefault();

            if (data != null)
            {
                _return += "<input type='hidden' class='form-control' name='Course_Result_Final_Id' value='" + data.id + "'/>";
            }
            else
            {
                _return += "<input type='hidden' class='form-control' name='Course_Result_Final_Id' value='-1'/>";
            }

            return _return;
        }

        /// <summary>
        /// The InsertFinalID_Custom.
        /// </summary>
        /// <param name="final">The final<see cref="Course_Result_Final"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string InsertFinalID_Custom(Course_Result_Final final)
        {
            string _return = "";
            if (final != null)
            {
                _return += "<input type='hidden' class='form-control' name='Course_Result_Final_Id' value='" + final.id + "'/>";
            }
            else
            {
                _return += "<input type='hidden' class='form-control' name='Course_Result_Final_Id' value='-1'/>";
            }

            return _return;
        }

        /// <summary>
        /// The GetGrade.
        /// </summary>
        /// <param name="subjectDetailId">The subjectDetailId<see cref="int"/>.</param>
        /// <param name="score">The score<see cref="double"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetGrade(int subjectDetailId, double score = -1)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            var getsubjectDetails = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.Id == subjectDetailId).Select(a => a.Id);
            if (getsubjectDetails.Any())
            {
                var getsubjectScore = _repoSubject.GetScores(a => getsubjectDetails.Contains((int)a.subject_id)).OrderByDescending(a => a.point_from);
                if (getsubjectScore.Any())
                {
                    foreach (var item in getsubjectScore)
                    {
                        if (score >= item.point_from)
                        {
                            result = item.grade;
                            break;
                        }
                    }
                }
            }
            var _return = result.Contains(UtilConstants.Grade.Fail.ToString())
                ? "F"
                : "P";
            return _return;
        }

        /// <summary>
        /// The GetGradeSummary.
        /// </summary>
        /// <param name="subjectDetailId">The subjectDetailId<see cref="int"/>.</param>
        /// <param name="score">The score<see cref="double"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetGradeSummary(int subjectDetailId, double score = -1)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            var getsubjectDetails = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.Id == subjectDetailId).Select(a => a.Id);
            if (getsubjectDetails.Any())
            {
                var getsubjectScore = _repoSubject.GetScores(a => getsubjectDetails.Contains((int)a.subject_id)).OrderByDescending(a => a.point_from);
                if (getsubjectScore.Any())
                {
                    foreach (var item in getsubjectScore)
                    {
                        if (score >= item.point_from)
                        {
                            result = item.grade;
                            break;
                        }

                    }
                }
                else
                {
                    result = string.Empty;
                }
            }

            return result;
        }

        /// <summary>
        /// The GetResultFinal.
        /// </summary>
        /// <param name="type">The type<see cref="UtilConstants.SwitchResult"/>.</param>
        /// <param name="courseDetailIds">The courseDetailIds<see cref="IQueryable{int}"/>.</param>
        /// <param name="courseResult">The courseResult<see cref="IQueryable{Course_Result}"/>.</param>
        /// <param name="traineeid">The traineeid<see cref="int?"/>.</param>
        /// <param name="scorepass">The scorepass<see cref="int"/>.</param>
        /// <param name="scoredistinction">The scoredistinction<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetResultFinal(UtilConstants.SwitchResult type, IQueryable<int> courseDetailIds, IQueryable<Course_Result> courseResult, int? traineeid = -1, int scorepass = -1, int scoredistinction = -1)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var result = string.Empty;
            double? score = 0;
            var countscore = 0;
            bool checkRe = false;
            var checkfail = false;
            var grade = (int)UtilConstants.Grade.Pass;
            var strGrade = UtilConstants.Grade.Pass.ToString();
            //if (getcoursedetail.Any())
            //{
            //var courseDetailId = getcoursedetail.Select(a => a.Id);
            var courseSummary = courseResult.Where(a => a.TraineeId == traineeid);

            //var courseResult = CourseService.GetCourseResult(a => a.TraineeId == traineeid && courseDetailId.Contains((int)a.CourseDetailId)).ToList();
            var allpassfail = courseSummary.All(a => a.Course_Detail.SubjectDetail.IsAverageCalculate == false);
            if (courseSummary.Any())
            {

                foreach (var item in courseSummary)
                {
                    if ((bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Score != null)
                        {
                            if (item.Re_Check_Score == -1)
                            {
                                item.Re_Check_Score = 0;
                            }
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                            checkRe = true;
                            score = score + item.Re_Check_Score;
                        }
                        else
                        {
                            if (item?.First_Check_Score != null)
                            {
                                if (item.First_Check_Score == -1)
                                {
                                    item.First_Check_Score = 0;
                                }
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;
                                }
                                score = score + item.First_Check_Score;
                            }

                        }
                        countscore++;
                    }
                    else if (!(bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Result != null)
                        {
                            checkRe = true;
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                        }
                        else
                        {
                            if (item?.First_Check_Result != null)
                            {
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;

                                }
                            }
                            else
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;
                                break;
                            }

                        }

                    }

                }
                if (countscore != 0)
                {
                    var score_temp = score / countscore;
                    score = Math.Round((double)score_temp, 1);
                }


                // score = (float)80.83333333333333;

            }
            else
            {
                checkfail = true;
            }
            var point = (score == 0 || score == -1) ? ("<input type='hidden' class='form-control' name='score_final' value='" + score + "'/>") : (score + "<input type='hidden' class='form-control' name='score_final' value='" + score + "'/>");
            if (type == UtilConstants.SwitchResult.Point)
            {
                result = point;

            }
            if (type == UtilConstants.SwitchResult.Grade)
            {
                var countDetail = courseDetailIds.Count();
                var countResult = courseSummary.Count();
                if (countResult < countDetail)
                {
                    checkfail = true;
                }
                var checkFail = courseSummary.Any(
        a => (a.Type == true));

                if (checkFail || checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                else
                {
                    //true = checked
                    if (allpassfail == false)
                    {
                        if (score > 0)
                        {
                            if (score < scorepass)
                            {
                                grade = (int)UtilConstants.Grade.Fail;
                            }
                            else if (scorepass <= score && score < scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Pass;
                            }
                            else if (score >= scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Distinction;
                            }
                        }
                        else
                        {
                            grade = (int)UtilConstants.Grade.Fail;
                        }
                    }
                }
                if (grade == (int)UtilConstants.Grade.Distinction)
                {
                    if (checkRe)
                    {
                        grade = (int)UtilConstants.Grade.Pass;
                    }
                }
                if (checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                switch (grade)
                {
                    case (int)UtilConstants.Grade.Fail:
                        grade = (int)UtilConstants.Grade.Fail;
                        strGrade = UtilConstants.Grade.Fail.ToString();
                        break;
                    case (int)UtilConstants.Grade.Pass:
                        grade = (int)UtilConstants.Grade.Pass;
                        strGrade = UtilConstants.Grade.Pass.ToString();
                        break;
                    case (int)UtilConstants.Grade.Distinction:
                        grade = (int)UtilConstants.Grade.Distinction;
                        strGrade = UtilConstants.Grade.Distinction.ToString();
                        break;
                }

                result = strGrade + "<input type='hidden' class='form-control' name='result_final' value='" + grade + "'/>";
            }
            //}
            return result;
        }

        /// <summary>
        /// The GetResultFinal_Custom.
        /// </summary>
        /// <param name="type">The type<see cref="UtilConstants.SwitchResult"/>.</param>
        /// <param name="courseDetailIds">The courseDetailIds<see cref="IEnumerable{int}"/>.</param>
        /// <param name="course_result">The course_result<see cref="IQueryable{Course_Result}"/>.</param>
        /// <param name="traineeid">The traineeid<see cref="int?"/>.</param>
        /// <param name="scorepass">The scorepass<see cref="int"/>.</param>
        /// <param name="scoredistinction">The scoredistinction<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetResultFinal_Custom(UtilConstants.SwitchResult type, IEnumerable<int> courseDetailIds, IQueryable<Course_Result> course_result, int? traineeid = -1, int scorepass = -1, int scoredistinction = -1)
        {
            var result = string.Empty;
            double? score = 0;
            var countscore = 0;
            bool checkRe = false;
            var checkfail = false;
            var grade = (int)UtilConstants.Grade.Pass;
            var strGrade = "Pass";
            //if (getcoursedetail.Any())
            //{
            //var courseDetailId = getcoursedetail.Select(a => a.Id);
            var courseSummary = course_result.Where(a => a.TraineeId == traineeid);
            var allpassfail = courseSummary.All(a => a.Course_Detail.SubjectDetail.IsAverageCalculate == false);
            //var courseResult = CourseService.GetCourseResult(a => a.TraineeId == traineeid && courseDetailId.Contains((int)a.CourseDetailId)).ToList();
            if (courseSummary.Any())
            {
                foreach (var item in courseSummary)
                {
                    if ((bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Score != null)
                        {
                            if (item.Re_Check_Score == -1)
                            {
                                item.Re_Check_Score = 0;
                            }
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                            checkRe = true;
                            score = score + item.Re_Check_Score;
                        }
                        else
                        {
                            if (item?.First_Check_Score != null)
                            {
                                if (item.First_Check_Score == -1)
                                {
                                    item.First_Check_Score = 0;

                                }
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;
                                }
                                score = score + item.First_Check_Score;
                            }

                        }
                        countscore++;
                    }
                    else if (!(bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Result != null)
                        {
                            checkRe = true;
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                        }
                        else
                        {
                            if (item?.First_Check_Result != null)
                            {
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;

                                }
                            }
                            else
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;
                                break;
                            }

                        }

                    }
                }
                if (countscore != 0)
                {
                    var score_temp = score / countscore;
                    score = Math.Round((double)score_temp, 1);
                }
            }
            else
            {
                checkfail = true;
            }
            var point = score.ToString();
            if (type == UtilConstants.SwitchResult.Point)
            {
                result = point;
            }

            if (type == UtilConstants.SwitchResult.Grade)
            {
                var countDetail = courseDetailIds.Count();
                var countResult = courseSummary.Count();
                if (countResult < countDetail)
                {
                    checkfail = true;
                }
                var checkFail = courseSummary.Any(
        a => (a.Type == true));
                if (checkFail || checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                else
                {

                    //true = checked

                    if (allpassfail == false)
                    {
                        if (score > 0)
                        {
                            if (score < scorepass)
                            {
                                grade = (int)UtilConstants.Grade.Fail;
                            }
                            else if (scorepass <= score && score < scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Pass;
                            }
                            else if (score >= scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Distinction;
                            }
                        }
                        else
                        {
                            grade = (int)UtilConstants.Grade.Fail;
                        }
                    }
                }
                if (grade == (int)UtilConstants.Grade.Distinction)
                {
                    if (checkRe)
                    {
                        grade = (int)UtilConstants.Grade.Pass;
                    }
                }
                if (checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                switch (grade)
                {
                    case (int)UtilConstants.Grade.Fail:
                        grade = (int)UtilConstants.Grade.Fail;
                        strGrade = "Fail";
                        break;
                    case (int)UtilConstants.Grade.Pass:
                        grade = (int)UtilConstants.Grade.Pass;
                        strGrade = "Pass";
                        break;
                    case (int)UtilConstants.Grade.Distinction:
                        grade = (int)UtilConstants.Grade.Distinction;
                        strGrade = "Distinction";
                        break;
                }

                result = strGrade;
            }
            return result;
        }

        /// <summary>
        /// The GetSubjectListId.
        /// </summary>
        /// <returns>The <see cref="List{Course_Result}"/>.</returns>
        public List<Course_Result> GetSubjectListId()
        {
            return new List<Course_Result>();
        }

        /// <summary>
        /// The ResultGetButtom.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult ResultGetButtom(int? courseId = -1)
        {
            courseId = courseId ?? -1;
            try
            {

                //var processStep = ProcessStep((int)UtilConstants.ApproveType.CourseResult);

                if (courseId != -1)
                {
                    int showbutton = 0;
                    var showexport = false;
                    var dataTMS_APPROVES = ApproveService.Get(a => a.int_Course_id == courseId);
                    if (dataTMS_APPROVES.Count() > 0)
                    {
                        showbutton = dataTMS_APPROVES.Count(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve);
                        if (showbutton == dataTMS_APPROVES.Count(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult))
                        {
                            var checkapproval = dataTMS_APPROVES.Count(
                                        a => a.int_Type == (int)UtilConstants.ApproveType.CourseResult &&
                                        (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));

                            if (checkapproval != 1)
                            {
                                return Json(new AjaxResponseViewModel()
                                {
                                    result = true,
                                    data = "<a class='btn btn-primary href='javascript:void(0)' legitRipple' onclick='callrequestFinal()' >" + Resource.lblRequestApprove + "</a>"
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    data = "",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    data = ex.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ResultGetButtom_Export.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResultGetButtom_Export(int? courseId = -1)
        {
            courseId = courseId ?? -1;
            try
            {

                //var processStep = ProcessStep((int)UtilConstants.ApproveType.CourseResult);

                if (courseId != -1)
                {
                    int showbutton = 0;
                    var showexport = false;
                    var course = CourseService.GetById(courseId);
                    var check = course.TMS_APPROVES.Any(a => a.int_Course_id == courseId && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Pending);
                    if (check != true)
                    {
                        return Json(new AjaxResponseViewModel()
                        {
                            result = true,
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new AjaxResponseViewModel()
                {
                    result = false,

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    data = ex.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult RequestApprove(int courseDetailsId)
        //{
        //    try
        //    {
        //        //if (!_sLmsServices.CallLmsServices_Course_Send_Flag_Point(CourseDetailService.GetById(courseDetailsId), 1))
        //        //{
        //        //    return Json(CMSUtils.alert("danger", Messege.UNSUCCESS));
        //        //}
        //        var currentUser = CurrentUser;
        //        //_repoTmsApproves.CreateApproveSubjectResultRecore(currentUser, CourseDetailService.GetById(courseDetailsId), 1, (int)UtilConstants.EStatus.Pending);

        //        ///////////
        //        var data = CourseDetailService.GetById(courseDetailsId);
        //        if (data != null)
        //        {
        //            var hodRoleName = UtilConstants.HOD;
        //            var firstOrDefault = _repoUser.GetAll(a => a.UserRoles.Any(x => x.ROLE.NAME == hodRoleName), currentUser.PermissionIds).FirstOrDefault();
        //            int To = firstOrDefault?.ID ?? _repoUser.GetAll(a => a.UserRoles.Any(x => x.ROLE.NAME == hodRoleName), currentUser.PermissionIds).FirstOrDefault().ID;
        //            //// get id approval
        //            var data_ = _repoTmsApproves.Get(a => a.int_courseDetails_Id == courseDetailsId && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult).FirstOrDefault();
        //            /////
        //            SendNotification((int)UtilConstants.NotificationType.AutoProcess, 3, data_.id, To, DateTime.Now, UtilConstants.NotificationTemplate.Request_SubjectResult,
        //                string.Format(UtilConstants.NotificationContent.Request_SubjectResult, data?.SubjectDetail?.Name, "(" + data?.Course.Code + ") " + data?.Course.Name), UtilConstants.NotificationTemplate.Request_SubjectResult_VN,
        //                string.Format(UtilConstants.NotificationContent.Request_SubjectResult_VN, data?.SubjectDetail?.Name, "(" + data?.Course.Code + ") " + data?.Course.Name));

        //        }



        //        return Json(new AjaxResponseViewModel { message = Messege.SUCCESS, result = true }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
        //        throw;
        //    }
        //}
        /// <summary>
        /// The ChangeCourseReturnSubject.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int?"/>.</param>
        /// <param name="subject">The subject<see cref="int?"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpPost]
        public JsonResult ChangeCourseReturnSubject(int? courseId, int? subject = null)
        {
            try
            {
                var html = new StringBuilder("<option value=''>--" + Resource.lblSubjectList + "--</option>");
                var nullInstructor = 1;
                Expression<Func<Course_Detail, bool>> expression = a => a.SubjectDetail.IsDelete == false;
                if (courseId != 0)
                {
                    expression =
                        a => a.CourseId == courseId && a.SubjectDetail.IsDelete == false;
                }
                var data = CourseDetailService.Get(expression).Select(a => new { a.Id, a.SubjectDetail.Name }).Distinct();
                if (data.Any())
                {
                    nullInstructor = 0;
                    foreach (var item in data)
                    {
                        html.AppendFormat("<option value='{0}' " + (subject.HasValue && subject.Value == item.Id ? "selected ='selected'" : "") + "> {1}</option>", item.Id, item.Name);
                    }
                }
                else
                {
                    nullInstructor = 1;
                }

                return Json(new
                {
                    value_option = html.ToString(),
                    value_null = nullInstructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RequestApproveCourse_new(FormCollection form)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                var currentUser = GetUser();
                var submitType = string.IsNullOrEmpty(form["submitType"]) ? -1 : Convert.ToInt32(form["submitType"]);
                var courseResultFinalId = form.GetValues("Course_Result_Final_Id");
                var courseId = string.IsNullOrEmpty(form["hdcourseid"]) ? -1 : Convert.ToInt32(form["hdcourseid"]);
                IList<RemarkTrainee> LtsRemarkTrainee = new List<RemarkTrainee>();
                if (!string.IsNullOrEmpty(form["RemarkTrainee"]))
                {

                    string RemarkTrainee = form["RemarkTrainee"];
                    string strCauHinh = string.Concat("[", RemarkTrainee, "]");
                    LtsRemarkTrainee = new JavaScriptSerializer().Deserialize<IList<RemarkTrainee>>(strCauHinh);
                }

                var courseDetail = _repoCourseDetail.GetByCourse(courseId);
                var flag = true;
                foreach (var item in courseDetail)
                {
                    if (!item.TMS_APPROVES.Any())
                    {
                        flag = false;
                    }
                }
                if (!flag)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RequestApproveCourse", string.Format(Messege.WARNING_SUBJECT_COMPLETED, Resource.lblCourse, Resource.lblSubject));
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.WARNING_SUBJECT_COMPLETED, Resource.lblCourse, Resource.lblSubject),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var i = 0;
                foreach (var item in courseResultFinalId)
                {
                    if (!string.IsNullOrEmpty(item) && item != "-1")
                    {
                        var courseResultId = int.Parse(item);
                        var coursefinal = CourseService.GetCourseResultFinalById(courseResultId);

                        var score = form.GetValues("score_final");
                        if (!string.IsNullOrEmpty(score[i]) && score[i] != "-1")
                            coursefinal.point = double.Parse(score[i]);

                        var result = form.GetValues("result_final");
                        if (result?[i] != null)
                            coursefinal.grade = int.Parse(result[i]);

                        coursefinal.createday = DateTime.Now;
                        coursefinal.createby = currentUser.Username;
                        coursefinal.remark = LtsRemarkTrainee[i]?.remarkTrainee?.ToString() ?? "";
                        coursefinal.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                        if (!string.IsNullOrEmpty(courseResultFinalId[i]) && courseResultFinalId[i] != "-1")
                        {
                            CourseService.UpdateCourseResultFinal(coursefinal);
                        }
                    }
                    i++;
                }
                var course = CourseService.GetById(courseId);
                if (course != null)
                {
                    Modify_TMS(false, course, (int)UtilConstants.ApproveType.CourseResult, submitType, UtilConstants.ActionType.Request);

                    if (submitType == (int)UtilConstants.EStatus.Approve)
                    {

                        #region [-------------Modify Course Result Sumary--------------]
                        var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.IsDeleted == false).OrderByDescending(a => a.courseid).ToList();
                        var courseDetailIds = course.Course_Detail.Select(a => a.Id).ToList();
                        var courseResultSummary =
                            CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value)).ToList();
                        if (courseResultSummary.Any())
                        {
                            foreach (var summary in courseResultSummary)
                            {
                                summary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                                CourseService.Update(summary);
                            }
                        }
                        #endregion

                        #region [---------Sent mail trainee member course final -------]

                        var checkRequestApproveCourse =
                            ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.RequestApproveCourse);
                        //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);

                        if (checkRequestApproveCourse != null)
                        {
                            #region [--------sent mail GV & GV mantor---------]
                            var usercheck = _repoCourseDetail.GetByCourse(courseId);
                            if (usercheck.Any())
                            {
                                var UserHannal = _repoUser.GetAll(a => a.UserRoles.Any(c => c.RoleId == 9) && a.IsDeleted == false && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => a.FIRSTNAME + " " + a.LASTNAME);

                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = _repoCourseDetail.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                                    if (instructor_coursedetail.Any())
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = EmployeeService.GetById(details.Instructor_Id);
                                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region [--------sent mail trainee---------]
                            if (data.Any())
                            {
                                foreach (var trainees_final in data)
                                {
                                    Sent_Email_TMS(null, trainees_final.Trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                }
                            }
                            #endregion
                        }

                        #endregion

                        return Json(new
                        {
                            message = string.Format(Messege.SUBMIT_SUCCESS, Resource.lblApproved),
                            result = true
                        }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RequestApproveCourse", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The RequestApproveCourse.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RequestApproveCourse(FormCollection form)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"].Trim());
                var txtCoursepass = string.IsNullOrEmpty(form["txt_coursepass"]) ? -1 : Convert.ToInt32(form["txt_coursepass"].Trim());
                var txtCoursedistinction = string.IsNullOrEmpty(form["txt_coursedistinction"]) ? -1 : Convert.ToInt32(form["txt_coursedistinction"].Trim());

                var data_ = CourseDetailService.Get(a => a.CourseId == courseList).Select(b => b.Id);
                var course_result = CourseService.GetCourseResult(a => data_.Contains((int)a.CourseDetailId));
                var data_new = CourseService.GetCourseResultFinal(a => a.courseid == courseList && a.Trainee.TMS_Course_Member.Any(x => data_.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)));

                var filtered = data_new.AsEnumerable().Select(c => new AjaxFinalResultsModel()
                {
                    Point = GetResultFinal_Custom(UtilConstants.SwitchResult.Point,
                       data_, course_result,
                       c.traineeid, txtCoursepass,
                       txtCoursedistinction),
                    Grade = GetResultFinal_Custom(UtilConstants.SwitchResult.Grade,
                       data_, course_result,
                       c.traineeid,
                       txtCoursepass,
                       txtCoursedistinction),
                    Action = c != null ? c.id.ToString() : "-1",
                    TraineeId = c?.traineeid,
                }).ToList();

                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                var currentUser = GetUser();
                var submitType = string.IsNullOrEmpty(form["submitType"]) ? -1 : Convert.ToInt32(form["submitType"]);             
                var courseId = string.IsNullOrEmpty(form["hdcourseid"]) ? -1 : Convert.ToInt32(form["hdcourseid"]);
                IList<RemarkTrainee> LtsRemarkTrainee = new List<RemarkTrainee>();
                if (!string.IsNullOrEmpty(form["RemarkTrainee"]))
                {

                    string RemarkTrainee = form["RemarkTrainee"];
                    string strCauHinh = string.Concat("[", RemarkTrainee, "]");
                    LtsRemarkTrainee = new JavaScriptSerializer().Deserialize<IList<RemarkTrainee>>(strCauHinh);
                }

                var courseDetail = _repoCourseDetail.GetByCourse(courseId);
                var flag = true;
                foreach (var item in courseDetail)
                {
                    if (!item.TMS_APPROVES.Any())
                    {
                        flag = false;
                    }
                }
                if (!flag)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RequestApproveCourse", string.Format(Messege.WARNING_SUBJECT_COMPLETED, Resource.lblCourse, Resource.lblSubject));
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.WARNING_SUBJECT_COMPLETED, Resource.lblCourse, Resource.lblSubject),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var i = 0;
                foreach (var item in filtered)
                {
                    if (!string.IsNullOrEmpty(item.Action) && item.Action != "-1")
                    {
                        var courseResultId = int.Parse(item.Action);
                        var coursefinal = CourseService.GetCourseResultFinalById(courseResultId);

                        Dictionary<string, object> dic_final = new Dictionary<string, object>();
                        var score = item.Point;
                        if (!string.IsNullOrEmpty(score) && score != "-1")
                            dic_final.Add("point", double.Parse(score));

                        var result = item.Grade;
                        if (result != null)
                        {
                            switch (result.ToLower())
                            {
                                case "fail":
                                    //coursefinal.grade = (int)UtilConstants.Grade.Fail;
                                    dic_final.Add("grade", (int)UtilConstants.Grade.Fail);

                                    break;
                                case "pass":
                                    //coursefinal.grade = (int)UtilConstants.Grade.Pass;
                                    dic_final.Add("grade", (int)UtilConstants.Grade.Pass);
                                    break;
                                case "distinction":
                                    //coursefinal.grade = (int)UtilConstants.Grade.Distinction;
                                    dic_final.Add("grade", (int)UtilConstants.Grade.Distinction);
                                    break;
                            }
                        }
                        //coursefinal.createday = DateTime.Now;
                        //coursefinal.createby = currentUser.Username;
                        //coursefinal.remark = "";
                        //coursefinal.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                        dic_final.Add("createday", DateTime.Now);
                        dic_final.Add("createby", currentUser.Username);
                        dic_final.Add("remark", LtsRemarkTrainee.FirstOrDefault(p => p.idTrainee == item.TraineeId)?.remarkTrainee ?? "");
                        dic_final.Add("LmsStatus", (int)UtilConstants.ApiStatus.AddNewTMS);

                        if (!string.IsNullOrEmpty(item.Action) && item.Action != "-1")
                        {
                            //CourseService.UpdateCourseResultFinal(coursefinal);
                            if (CMSUtils.UpdateDataSQLNoLog("id", coursefinal.id + "", "Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                            {

                            }
                        }
                    }
                    i++;
                }
                var course = CourseService.GetById(courseId);
                if (course != null)
                {
                    Modify_TMS(false, course, (int)UtilConstants.ApproveType.CourseResult, submitType, UtilConstants.ActionType.Request);

                    if (submitType == (int)UtilConstants.EStatus.Approve)
                    {

                        var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.IsDeleted == false).OrderByDescending(a => a.courseid).ToList();
                        var courseDetailIds = course.Course_Detail.Select(a => a.Id);
                        var courseResultSummary =
                            CourseService.GetCourseResultSummaries(a => courseDetailIds.Contains(a.CourseDetailId.Value)).ToList();
                        if (courseResultSummary.Any())
                        {
                            foreach (var summary in courseResultSummary)
                            {
                                summary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                                CourseService.Update(summary);
                            }
                        }


                        var checkRequestApproveCourse =
                            ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.RequestApproveCourse);
                        //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);

                        if (checkRequestApproveCourse != null)
                        {
                            var usercheck = _repoCourseDetail.GetByCourse(courseId);
                            if (usercheck.Any())
                            {
                                var UserHannal = _repoUser.GetAll(a => a.UserRoles.Any(c => c.RoleId == 9) && a.IsDeleted == false && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => a.FIRSTNAME + " " + a.LASTNAME);

                                foreach (var item in usercheck)
                                {
                                    var instructor_coursedetail = _repoCourseDetail.GetDetailInstructors(a => a.Course_Detail_Id == item.Id);
                                    if (instructor_coursedetail.Any())
                                    {
                                        foreach (var details in instructor_coursedetail)
                                        {
                                            var instructor = EmployeeService.GetById(details.Instructor_Id);
                                            Sent_Email_TMS(instructor, null, null, course, details, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                        }
                                    }
                                }
                            }

                            if (data.Any())
                            {
                                foreach (var trainees_final in data)
                                {
                                    Sent_Email_TMS(null, trainees_final.Trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram);
                                }
                            }
                        }


                        return Json(new
                        {
                            message = string.Format(Messege.SUBMIT_SUCCESS, Resource.lblApproved),
                            result = true
                        }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RequestApproveCourse", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ReSync.
        /// </summary>
        /// <param name="idCouresDetails">The idCouresDetails<see cref="int?"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [AllowAnonymous]
        public async Task<ActionResult> ReSync(int? idCouresDetails)
        {
            try
            {
                if (idCouresDetails.HasValue && idCouresDetails > 0)
                {
                    var type = UtilConstants.CRON_POST_RESULT + idCouresDetails;
                    var server = GetByKey("API_LMS_SERVER");
                    var token = GetByKey("API_LMS_TOKEN");
                    var function = GetByKey("FUNCTION");
                    var moodlewsrestformat = GetByKey("moodlewsrestformat") ?? "";
                    var restClient = new RestClient(server);
                    var request = new RestRequest(Method.POST);
                    request.AddParameter("wstoken", token);

                    request.AddParameter("wsfunction", function);
                    request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                    request.AddParameter("type", type);
                    var response = restClient.Execute(request);
                    var responseContent = response.Content;
                    var model_ = new List<CourseResultLMS>();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {

                        return Json(new AjaxResponseViewModel { message = "Re-Sync Result Unsuccessfully !!!", result = false }, JsonRequestBehavior.AllowGet);
                    }
                    if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                    {
                        return Json(new AjaxResponseViewModel { message = "Re-Sync Result Successfully !!!", result = true }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(new AjaxResponseViewModel { message = "Re-Sync Result Unsuccessfully !!!", result = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel { message = ex.ToString(), result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Defines the <see cref="CourseResultLMS" />.
        /// </summary>
        public class CourseResultLMS
        {
            /// <summary>
            /// Gets or sets the TraineeCode.
            /// </summary>
            public string TraineeCode { get; set; }

            /// <summary>
            /// Gets or sets the Score.
            /// </summary>
            public string Score { get; set; }

            /// <summary>
            /// Gets or sets the Time.
            /// </summary>
            public string Time { get; set; }

            /// <summary>
            /// Gets or sets the CourseDetailId.
            /// </summary>
            public string CourseDetailId { get; set; }
        }

        /// <summary>
        /// The Result.
        /// </summary>
        /// <param name="idCouresDetails">The idCouresDetails<see cref="int"/>.</param>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Result(int idCouresDetails, int courseId)
        {

            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted != true && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending), (int)UtilConstants.ApproveType.SubjectResult);
            //var courses =
            //    CourseService.Get()
            //        .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            var model = new CourseResultViewModel();
            model.ProcessStep = ProcessStep((int)UtilConstants.ApproveType.SubjectResult);
            var courseDetail = CourseDetailService.GetById(idCouresDetails);
            model.TypeLearning = TypeLearningIcon(courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline);
            model.TypeLearningId = courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline;
            if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Online)
            {
                model.MarkType = (int)UtilConstants.MarkTypes.Auto;
            }
            else if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Offline || model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline)
            {
                if (courseDetail.mark_type.HasValue)
                {
                    if (courseDetail.mark_type == (int)UtilConstants.MarkTypes.Auto) // Auto không áp dụng cho khóa Pass/Fail
                    {
                        model.MarkType = courseDetail.SubjectDetail.IsAverageCalculate == true ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                    }
                    else
                    {
                        model.MarkType = (int)UtilConstants.MarkTypes.Manual;
                    }
                }
                else
                {
                    model.MarkType = model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                }
            }
            model.checkSuccessCron = false;
            if (model.MarkType == (int)UtilConstants.MarkTypes.Auto)
            {
                var type = UtilConstants.CRON_POST_RESULT + idCouresDetails;
                var server = GetByKey("API_LMS_SERVER");
                var token = GetByKey("API_LMS_TOKEN");
                var function = GetByKey("FUNCTION");
                var moodlewsrestformat = GetByKey("moodlewsrestformat") ?? "";
                var restClient = new RestClient(server);
                var request = new RestRequest(Method.POST);
                request.AddParameter("wstoken", token);

                request.AddParameter("wsfunction", function);
                request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                request.AddParameter("type", type);
                var response = restClient.Execute(request);
                var responseContent = response.Content;
                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    model.checkSuccessCron = true;
                }
            }
            //model.Courses = courses;
            model.CourseId = (int)courseDetail.CourseId;
            model.CourseDetailId = idCouresDetails;
            model.SubjectDetailId = (int)courseDetail.SubjectDetailId;
            model.CourseCode = courseDetail.Course.Code;
            model.CourseName = courseDetail.Course.Name;
            model.SubjectCode = courseDetail.SubjectDetail?.Code;
            model.SubjectName = courseDetail.SubjectDetail?.Name;
            model.RoomName = courseDetail.Room?.str_Name;
            model.Duration = courseDetail.SubjectDetail?.Duration?.ToString();
            model.DateFromTo = courseDetail.dtm_time_from?.ToString("dd/MM/yyy") + " - " +
            courseDetail.dtm_time_to?.ToString("dd/MM/yyyy");
            model.MaxGrade = courseDetail.Course?.MaxGrade == null ? "100" : courseDetail.Course?.MaxGrade?.ToString();
            var instructorAbility = courseDetail.Course_Detail_Instructor.ToList();
            var instructors = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor);
            var hannah = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah);
            var mentor = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor);
            model.Instructors = instructors.Select(b => ReturnDisplayLanguageCustom(b.Trainee.FirstName, b.Trainee.LastName, null, b.Duration.HasValue ? b.Duration.Value : 0)).Aggregate(model.Instructors, (current, fullName) => current + (fullName + "<br />"));
            model.Hannah = hannah.Select(b => ReturnDisplayLanguageCustom(b.Trainee.FirstName, b.Trainee.LastName, null, b.Duration.HasValue ? b.Duration.Value : 0)).Aggregate(model.Hannah, (current, fullName) => current + (fullName + "<br />"));
            model.Mentor = mentor.Select(b => ReturnDisplayLanguageCustom(b.Trainee.FirstName, b.Trainee.LastName, null, b.Duration.HasValue ? b.Duration.Value : 0)).Aggregate(model.Mentor, (current, fullName) => current + (fullName + "<br />"));
            //iscaculate
            model.IsCalculate = (bool)courseDetail.SubjectDetail.IsAverageCalculate;
            //check Approve Pending
            model.IsApproved =
                 dataCourseProccess.Any(
                        a =>
                            a.int_courseDetails_Id == idCouresDetails &&
                            (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));

            //model.TypeLearningId = courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline;

            model.Members = courseDetail.TMS_Course_Member?.Where(a => a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).Select(a => new CourseResultViewModel.Member()
            {
                Id = a.Id,

                TraineeId = a.Trainee?.Id,
                Code = a.Trainee?.str_Staff_Id,
                Name = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                DepartmentCode = a.Trainee?.Department?.Code,
                DepartmentName = a.Trainee?.Department?.Name,
                LearningTime = a.Course_Detail?.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) + " - " + a.Course_Detail?.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                Score = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Score?.ToString().Replace("-1", "0"),
                Result = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Result,
                Score_Re = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Score?.ToString().Replace(",", ".").Replace("-1", "0"),
                Result_Re = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Result,
                Score_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Score?.ToString().Replace(",", "."),
                Result_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Result,
                Score_Re_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Score?.ToString().Replace(",", "."),
                Result_Re_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Result,
                Course_Result_Id = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Id,
                RealReResult = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Score?.ToString(),
                Remark = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Remark?.Replace("!!!!!", Environment.NewLine),
                Type = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Type ?? false,
                CheckFail = "<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + a.Id + ",this)'><i class='fas fa-edit " + (a.Course_Detail?.Course_Result?.Any(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id && b.Type == true) == true ? "highlight" : "") + " ' aria-hidden='true' ></i></a>"
                 ,
                CheckBox = "<input onclick='CheckBoxClick(this)' type='CheckBox' id='" + a.Id + "' value='" + a.Id + "' />"
            });
            return View(model);
        }

        /// <summary>
        /// The ListNoteChangeCourseCodeReturnSubject.
        /// </summary>
        /// <param name="id_course">The id_course<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [AllowAnonymous]
        public JsonResult ListNoteChangeCourseCodeReturnSubject(string id_course)
        {
            try
            {
                StringBuilder html = new StringBuilder("<option value=''>--Subject List--</option>");
                int null_instructor = 1;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var data = CourseDetailService.Get(a => (string.IsNullOrEmpty(id_course) ? a.Course.StartDate >= timenow : true) && (string.IsNullOrEmpty(id_course) || a.Course.Name.Contains(id_course)) && a.SubjectDetail.IsDelete != true && a.SubjectDetail.CourseTypeId.HasValue &&
                           a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General).Select(a => new { a.Id, a.SubjectDetail.Name, a.SubjectDetail.IsActive }).OrderBy(x => x.Id);

                if (data.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data)
                    {
                        html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, (item.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + item.Name);
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ListNoteChangeCourseCodeReturnSubject", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerListResult.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerListResult(jQueryDataTableParamModel param)
        {
            try
            {
                var courseDetailId = string.IsNullOrEmpty(Request.QueryString["CourseDetailId"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseDetailId"].Trim());
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");



                var data = CourseMemberService.Get(a => a.Course_Details_Id == courseDetailId && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);
                //var model = data.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                //{
                //    TraineeCode = a.Trainee?.str_Staff_Id,
                //    //FullName = a.Trainee?.FirstName + " " + a.Trainee?.LastName,
                //    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                //    DepartmentName = a.Trainee?.Department?.Name,
                //    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),

                //    Grade = returnpointgrade_Custom(2, a.Course_Detail?.Course_Result, a?.Member_Id, a?.Course_Details_Id),
                //    Id = a.Id,
                //    TraineeId = a.Member_Id ?? -1,
                //    SubjectDetailId = a.Course_Detail?.SubjectDetailId ?? -1,
                //    CourseDetailId = a.Course_Details_Id ?? -1,
                //    Type = a.Course_Detail?.Course_Result?.Any(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id && b.Type == true) ?? false,
                //    FirstResult = GetMark_custom(1, UtilConstants.DetailResult.Score, a.Course_Detail?.Course_Result, a.Member_Id, a.Course_Details_Id, 1, a.Course_Detail?.type_leaning, a.Course_Detail?.SubjectDetail?.IsAverageCalculate),
                //    ReResult = GetMark_custom(2, UtilConstants.DetailResult.Score, a.Course_Detail?.Course_Result, a.Member_Id, a.Course_Details_Id, 1, a.Course_Detail?.type_leaning, a.Course_Detail?.SubjectDetail?.IsAverageCalculate),
                //    Remark = a.Course_Result?.FirstOrDefault()?.Remark.Replace("!!!!!", Environment.NewLine) ?? "",
                //});

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                            : sortColumnIndex == 2 ? c.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                            : sortColumnIndex == 4 ? DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy")
                                                            : sortColumnIndex == 5 ? GetMark_custom(1, UtilConstants.DetailResult.Score, c.Course_Detail?.Course_Result, c.Member_Id, c.Course_Details_Id, 1, c.Course_Detail?.type_leaning, c.Course_Detail?.SubjectDetail?.IsAverageCalculate)
                                                            : sortColumnIndex == 6 ? GetMark_custom(2, UtilConstants.DetailResult.Score, c.Course_Detail?.Course_Result, c.Member_Id, c.Course_Details_Id, 1, c.Course_Detail?.type_leaning, c.Course_Detail?.SubjectDetail?.IsAverageCalculate)
                                                            : c.Trainee?.str_Staff_Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
                                                c.Trainee?.str_Staff_Id,
                                                c.Trainee?.Department?.Name,
                                                DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                                "<input type='hidden' data-staffid='"+c.Trainee?.str_Staff_Id+"' data-='"+c.Course_Detail?.SubjectDetailId+"' class='form-control' name='Id' value='"+c?.Id+"'/>" +
                                                "<input type='hidden' data-staffid='"+c.Trainee?.str_Staff_Id+"' data-='"+c.Course_Detail?.SubjectDetailId+"' class='form-control' name='Course_Details_Id' value='"+c?.Course_Details_Id+"'/>" +
                                                "<input type='hidden' data-staffid='"+c.Trainee?.str_Staff_Id+"' data-='"+c.Course_Detail?.SubjectDetailId+"' class='form-control' name='Trainee_Id' value='"+c?.Member_Id+"'/>"+
                                                "<input type='hidden' class='form-control' name='isfail' value='-1'/>" +
                                                GetMark_custom(1, UtilConstants.DetailResult.Score, c.Course_Detail?.Course_Result, c.Member_Id, c.Course_Details_Id, 1, c.Course_Detail?.type_leaning, c.Course_Detail?.SubjectDetail?.IsAverageCalculate),
                                                GetMark_custom(2, UtilConstants.DetailResult.Score, c.Course_Detail?.Course_Result, c.Member_Id, c.Course_Details_Id, 1, c.Course_Detail?.type_leaning, c.Course_Detail?.SubjectDetail?.IsAverageCalculate),
                                               returnpointgrade_Custom(2, c.Course_Detail?.Course_Result, c?.Member_Id, c?.Course_Details_Id),
                                               c.Course_Result?.FirstOrDefault()?.Remark.Replace("!!!!!", Environment.NewLine)
                                               };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// The AjaxHandlerListResult_Custom.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandlerListResult_Custom(jQueryDataTableParamModel param)
        {
            try
            {
                var courseDetailId = string.IsNullOrEmpty(Request.QueryString["CourseDetailId"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseDetailId"].Trim());
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var data = CourseMemberService.Get(a => a.Course_Details_Id == courseDetailId && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);
                var model = data.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                {
                    TraineeCode = a.Trainee?.str_Staff_Id,
                    //FullName = a.Trainee?.FirstName + " " + a.Trainee?.LastName,
                    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    DepartmentName = a.Trainee?.Department?.Name,
                    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                    Grade = returnpointgrade_Custom(2, a.Course_Detail?.Course_Result, a?.Member_Id, a?.Course_Details_Id),
                    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                    Remark = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Type == true ? GetRemarkCheckFail(a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Id) : (a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Remark != null ? a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Remark?.Replace("!!!!!", "<br />") : null),
                    //a.Course_Detail?.Course_Result?.FirstOrDefault()?.Remark?.ToString().Replace("!!!!!", Environment.NewLine) ?? "",
                });

                IEnumerable<AjaxCourseResultFinalModel> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxCourseResultFinalModel, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.FullName
                                                            : sortColumnIndex == 2 ? c.TraineeCode
                                                            : sortColumnIndex == 3 ? c.DepartmentName
                                                            : sortColumnIndex == 4 ? c.DateFromTo
                                                            : sortColumnIndex == 5 ? c.FirstResult
                                                            : sortColumnIndex == 6 ? c.ReResult
                                                             : sortColumnIndex == 7 ? c.Grade
                                                             : sortColumnIndex == 8 ? c.Remark
                                                            : c.FirstResult);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(a => a.FirstResultCertificate).ThenByDescending(a => a.ReResultCertificate).ThenBy(a => a.TraineeCode).ThenBy(orderingFunction)
                                    : filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(a => a.FirstResultCertificate).ThenByDescending(a => a.ReResultCertificate).ThenBy(a => a.TraineeCode).ThenByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c.FullName,
                                                c.TraineeCode,
                                                c.DepartmentName,
                                                c.DateFromTo,
                                               c.FirstResultCertificate,
                                                c.ReResultCertificate,
                                               c.Grade,
                                               c.Remark
                                               };
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// The GetRemarkCheckFail.
        /// </summary>
        /// <param name="resultid">The resultid<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetRemarkCheckFail(int? resultid)
        {
            var remark = "";
            var remarkresult = CourseService.GetCourseResultCheckFail(a => a.CourseResultID == resultid);
            remark = remarkresult.FirstOrDefault()?.RemarkContent;
            return remark;
        }

        /// <summary>
        /// The InsertResult.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> InsertResult(FormCollection form)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                var user = GetUser();
                var memberId = form.GetValues("MemberId");
                var traineeIds = form.GetValues("TraineeId");
                var traineeCode = form.GetValues("TraineeCode");
                var traineeName = form.GetValues("TraineeName");
                var departmentCode = form.GetValues("DepartmentCode");
                var txtRemark = form.GetValues("txt_Remark");
                var countIngre = form.GetValues("CountIngre");
                var txtResult = form.GetValues("result");
                var txtFirstCheckResult = form.GetValues("First_Check_Result");
                var txtFirstCheckScore = form.GetValues("First_Check_Score");
                var txtReCheckResult = form.GetValues("Re_Check_Result");
                var txtReCheckScore = form.GetValues("Re_Check_Score");
                var courseId = string.IsNullOrEmpty(form["fCourseId"]) ? -1 : Convert.ToInt32(form["fCourseId"]);
                var courseDetailId = string.IsNullOrEmpty(form["fCourseDetailId"])
                    ? -1
                    : Convert.ToInt32(form["fCourseDetailId"]);
                var subjectDetailId = string.IsNullOrEmpty(form["fSubjectDetailId"])
                    ? -1
                    : Convert.ToInt32(form["fSubjectDetailId"]);
                var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
                //var note = string.IsNullOrEmpty(form["fNote"]) ? string.Empty : form["fNote"].Trim();
                var isCalculate = string.IsNullOrEmpty(form["fIsCalculate"])
                    ? -1
                    : Convert.ToInt32(form["fIsCalculate"]);
                var maxGade = string.IsNullOrEmpty(form["MaxGrade"])
                    ? -1
                    : double.Parse(form["MaxGrade"]);

                if (traineeIds != null)
                {
                    var i = 0;
                    foreach (var id in traineeIds)
                    {
                        var appSettings = form.AllKeys.Where(k => k.StartsWith("TraineeId_" + id + "_")).ToDictionary(k => k, k => form[k]);
                        foreach (var key in appSettings)
                        {
                            var separators = new[] { "_", "," };
                            var words = key.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                            //  var traineeId = words;
                            var traineeId = int.Parse(words.GetValue(1).ToString());
                            var score = words.GetValue(3).ToString();

                            var result = string.Empty;
                            var remark = string.Empty;
                            //var newScore = txtResult[i];
                            var newFirstCheckResult = txtFirstCheckResult[i];
                            var newFirstCheckScore = txtFirstCheckScore[i];
                            if (!string.IsNullOrEmpty(newFirstCheckScore))
                            {
                                newFirstCheckScore = Regex.Replace(newFirstCheckScore, "[.,]", separator);
                            }
                            var newReCheckResult = txtReCheckResult[i];
                            var newReCheckScore = txtReCheckScore[i];
                            if (!string.IsNullOrEmpty(newReCheckScore))
                            {
                                newReCheckScore = Regex.Replace(newReCheckScore, "[.,]", separator);
                            }
                            var entity = CourseService.GetCourseResult(traineeId, courseDetailId);
                            if (entity != null)
                            {
                                remark = txtRemark[i];
                                entity.Result = result;
                                if (!string.IsNullOrEmpty(newFirstCheckScore) && newFirstCheckScore != "-1")
                                {
                                    entity.First_Check_Score = !string.IsNullOrEmpty(newFirstCheckScore) ? Math.Round(double.Parse(newFirstCheckScore), 1) : -1;
                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newFirstCheckScore) ? double.Parse(newFirstCheckScore) : -1);
                                }
                                else
                                {
                                    entity.First_Check_Score = -1;
                                    entity.First_Check_Result = "F";
                                }
                                entity.Remark = remark;
                                if (!string.IsNullOrEmpty(newReCheckScore) && newReCheckScore != "-1")
                                {
                                    entity.Re_Check_Score = !string.IsNullOrEmpty(newReCheckScore) ? Math.Round(double.Parse(newReCheckScore), 1) : -1;
                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newReCheckScore) ? double.Parse(newReCheckScore) : -1);
                                }
                                else
                                {
                                    entity.Re_Check_Score = null;
                                    entity.Re_Check_Result = null;
                                }
                                entity.ModifiedAt = DateTime.Now;
                                if (!string.IsNullOrEmpty(newFirstCheckResult) && newFirstCheckResult != "-1")
                                {

                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : newFirstCheckResult;
                                }
                                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                                if (!string.IsNullOrEmpty(newReCheckResult) && newReCheckResult != "-1")
                                {

                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : newReCheckResult;
                                }
                                entity.inCourseMemberId = int.Parse(memberId[i]);
                                entity.IsDelete = false;
                                entity.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                                CourseService.UpdateCourseResult(entity);
                            }
                            else
                            {
                                remark = txtRemark[i];
                                entity = new Course_Result()
                                {
                                    CourseDetailId = courseDetailId,
                                    TraineeId = traineeId,
                                    Result = result,
                                    Remark = remark,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = CurrentUser.USER_ID.ToString(),
                                    IsDelete = false,
                                    LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS
                                };
                                if (!string.IsNullOrEmpty(newFirstCheckScore) && newFirstCheckScore != "-1")
                                {
                                    entity.First_Check_Score = !string.IsNullOrEmpty(newFirstCheckScore) ? Math.Round(double.Parse(newFirstCheckScore), 1) : -1;
                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newFirstCheckScore) ? double.Parse(newFirstCheckScore) : -1);
                                }
                                else
                                {
                                    entity.First_Check_Score = -1;
                                    entity.First_Check_Result = "F";
                                }

                                if (!string.IsNullOrEmpty(newReCheckScore) && newReCheckScore != "-1")
                                {
                                    entity.Re_Check_Score = !string.IsNullOrEmpty(newReCheckScore) ? Math.Round(double.Parse(newReCheckScore), 1) : -1;
                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newReCheckScore) ? double.Parse(newReCheckScore) : -1);
                                }
                                if (!string.IsNullOrEmpty(newFirstCheckResult) && newFirstCheckResult != "-1")
                                {

                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : newFirstCheckResult;
                                }
                                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                                if (!string.IsNullOrEmpty(newReCheckResult) && newReCheckResult != "-1")
                                {

                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : newReCheckResult;
                                }
                                entity.inCourseMemberId = int.Parse(memberId[i]);
                                CourseService.InsertCourseResult(entity);
                            }


                            var entitySummary = _repoCourseResultSummaryService.GetById(traineeId, courseDetailId);
                            var subject = _repoSubject.GetSubjectDetailById(subjectDetailId);
                            if (entitySummary != null)
                            {
                                if (entity != null && subject?.IsAverageCalculate == true)
                                {
                                    if (entity.Re_Check_Score.HasValue)
                                    {
                                        entitySummary.point = entity.Re_Check_Score;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, (int)entity.Re_Check_Score);

                                    }
                                    else
                                    {
                                        entitySummary.point = entity.First_Check_Score.HasValue ? entity.First_Check_Score : -1;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, entity.First_Check_Score.HasValue ? (int)entity.First_Check_Score : -1);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(entity.Re_Check_Result))
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.Re_Check_Result == "F" ? "Fail" : "Pass";

                                    }
                                    else
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.First_Check_Result == "P" ? "Pass" : "Fail";
                                    }
                                }

                                entitySummary.Remark = remark;
                                if (isCalculate == (int)UtilConstants.BoolEnum.Yes)
                                {
                                    entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;//submitType == (int)UtilConstants.EStatus.Approve ? StatusIsSync : StatusModify;
                                }
                                else
                                {
                                    entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;//StatusIsSync;
                                }
                                _repoCourseResultSummaryService.Update(entitySummary);
                            }
                            else
                            {
                                entitySummary = new Course_Result_Summary()
                                {
                                    TraineeId = traineeId,
                                    CourseDetailId = courseDetailId,
                                };
                                if (entity != null && subject?.IsAverageCalculate == true)
                                {
                                    if (entity.Re_Check_Score.HasValue)
                                    {
                                        entitySummary.point = entity.Re_Check_Score;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, (int)entity.Re_Check_Score);

                                    }
                                    else
                                    {
                                        entitySummary.point = entity.First_Check_Score.HasValue ? entity.First_Check_Score : -1;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, entity.First_Check_Score.HasValue ? (int)entity.First_Check_Score : -1);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(entity.Re_Check_Result))
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.Re_Check_Result == "F" ? "Fail" : "Pass";

                                    }
                                    else
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.First_Check_Result == "P" ? "Pass" : "Fail";
                                    }
                                }
                                entitySummary.Remark = remark;
                                entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;// StatusModify;//submitType == (int)UtilConstants.EStatus.Approve ? StatusIsSync : StatusModify;
                                _repoCourseResultSummaryService.Insert(entitySummary);
                            }
                            i++;

                        }

                    }

                    //if (submitType == (int)UtilConstants.EStatus.Approve)
                    //{
                    //    var processStep = ProcessStep((int)UtilConstants.ApproveType.SubjectResult);
                    //    if (!processStep)
                    //    {
                    //        var course = CourseService.GetById(courseId);
                    //        Modify_TMS(course, (int)UtilConstants.ApproveType.SubjectResult, submitType, UtilConstants.ActionType.Approve, "", courseDetailId);
                    //        await Task.Run(() =>
                    //        {
                    //            #region [--------CALL LMS (CronGet Course ResultSummary)----------]
                    //            var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                    //            if (!callLms)
                    //            {
                    //                //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertResult", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                    //                //return Json(new AjaxResponseViewModel()
                    //                //{
                    //                //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                    //                //    result = false
                    //                //}, JsonRequestBehavior.AllowGet);
                    //            }
                    //            #endregion
                    //        });
                    //    }
                    //}


                }


                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertResult", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The InsertResult222222222222.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> InsertResult222222222222(FormCollection form)
        {
            try
            {

                var user = GetUser();
                var time = form.GetValues("txt_time");
                var traineeIds = form.GetValues("Trainee_Id");
                var noCalculate = form.GetValues("NoCalculate");
                var txtRemark = form.GetValues("txt_Remark");


                var courseId = string.IsNullOrEmpty(form["fCourseId"]) ? -1 : Convert.ToInt32(form["fCourseId"]);
                var courseDetailId = string.IsNullOrEmpty(form["fCourseDetailId"]) ? -1 : Convert.ToInt32(form["fCourseDetailId"]);
                var subjectDetailId = string.IsNullOrEmpty(form["fSubjectDetailId"]) ? -1 : Convert.ToInt32(form["fSubjectDetailId"]);
                var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
                //var note = string.IsNullOrEmpty(form["fNote"]) ? string.Empty : form["fNote"].Trim();
                var isCalculate = string.IsNullOrEmpty(form["fIsCalculate"]) ? -1 : Convert.ToInt32(form["fIsCalculate"]);

                var i = 0;
                if (traineeIds != null)
                {
                    foreach (var item in traineeIds)
                    {
                        var insertOrNot = false;
                        var appSettings = form.AllKeys.Where(k => k.StartsWith("txt_trainee_" + item + "_")).ToDictionary(k => k, k => form[k]);
                        foreach (var key in appSettings)
                        {
                            var separators = new[] { "_", "," };
                            var words = key.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            var getResultId = words.GetValue(4).ToString();
                            var getTraineeId = words.GetValue(2).ToString();
                            var getScore = key.Value;
                            Course_Result entity;
                            var result = string.Empty;
                            var remark = string.Empty;

                            if (!string.IsNullOrEmpty(getResultId) && getResultId != "-1")
                            {
                                var intResultId = int.Parse(getResultId);
                                entity = CourseService.GetCourseResultById(intResultId);
                                entity.times = (!string.IsNullOrEmpty(time?[i])) ? int.Parse(time[i]) : 1;
                            }
                            else
                            {
                                entity = new Course_Result();
                                entity.times = (!string.IsNullOrEmpty(time?[i])) ? int.Parse(time[i]) : 1;
                            }

                            switch (isCalculate)
                            {
                                case (int)UtilConstants.BoolEnum.Yes:
                                    if (1 == 1)/*(!string.IsNullOrEmpty(getScore) && getScore != "-1") */
                                    {

                                        entity.Score = !string.IsNullOrEmpty(getScore) ? double.Parse(getScore) : -1;
                                        remark = txtRemark != null ? txtRemark[i] : string.Empty;
                                        //  var subjectDetailId = _repoCourseDetail.GetById(courseDetailsId).SubjectDetailId;
                                        result = entity.Type == true
                                            ? UtilConstants.Grade.Fail.ToString()
                                            : GetGrade(subjectDetailId, !string.IsNullOrEmpty(getScore) ? double.Parse(getScore) : -1);
                                        insertOrNot = true;
                                    }
                                    break;
                                case (int)UtilConstants.BoolEnum.No:
                                    if (noCalculate != null && noCalculate[i] != "-1")
                                    {
                                        // true = checked
                                        if (entity.Type == true)
                                        {
                                            result = UtilConstants.Grade.Fail.ToString();

                                            remark = (entity.Type == true) ? entity.Remark : txtRemark[i];
                                        }
                                        else
                                        {
                                            // true = checked
                                            remark = (entity.Type != true) ? txtRemark[i] : entity.Remark;
                                            var intAverageCaculate = int.Parse(noCalculate[i]);
                                            switch (intAverageCaculate)
                                            {
                                                case (int)UtilConstants.Grade.Fail:
                                                    result = UtilConstants.Grade.Fail.ToString();
                                                    break;
                                                case (int)UtilConstants.Grade.Pass:
                                                    result = UtilConstants.Grade.Pass.ToString();
                                                    break;

                                            }
                                        }
                                        insertOrNot = true;
                                    }
                                    break;
                            }
                            entity.CourseDetailId = courseDetailId;
                            var intTraineeId = int.Parse(getTraineeId);
                            entity.TraineeId = intTraineeId;
                            entity.Result = result;
                            entity.Remark = remark;
                            if (getResultId != "-1")
                            {
                                entity.ModifiedBy = user.USER_ID.ToString();
                                entity.ModifiedAt = DateTime.Now;
                                CourseService.UpdateCourseResult(entity);
                            }
                            else
                            {
                                if (!insertOrNot) continue;
                                entity.CreatedBy = user.USER_ID.ToString();
                                entity.CreatedAt = DateTime.Now;
                                CourseService.InsertCourseResult(entity);
                            }
                        }



                        var traineeId = int.Parse(item);
                        var entitySummary = _repoCourseResultSummaryService.GetById(traineeId, courseDetailId);
                        double sPoint = -1;
                        //var sResult = string.Empty;
                        //var sRemark = string.Empty;

                        var sumaryResult = GetSumaryResult(traineeId, courseDetailId, subjectDetailId);
                        var c = new[] { '_' };
                        var splitResult = sumaryResult.Split(c);
                        sPoint = (!string.IsNullOrEmpty(splitResult.GetValue(0).ToString()) /*&& splitResult.GetValue(0).ToString() != "0"*/) ? double.Parse(splitResult.GetValue(0).ToString()) : -1;
                        var sResult = splitResult.GetValue(1).ToString() ?? string.Empty;
                        var sRemark = splitResult.GetValue(2).ToString() ?? string.Empty;

                        if (entitySummary != null)
                        {
                            entitySummary.point = sPoint;
                            entitySummary.Result = sResult;
                            entitySummary.Remark = sRemark;
                            if (isCalculate == (int)UtilConstants.BoolEnum.Yes)
                            {
                                entitySummary.LmsStatus = submitType == (int)UtilConstants.EStatus.Approve ? StatusIsSync : StatusModify;
                            }
                            else
                            {
                                entitySummary.LmsStatus = StatusIsSync;
                            }
                            _repoCourseResultSummaryService.Update(entitySummary);
                        }
                        else
                        {
                            entitySummary = new Course_Result_Summary()
                            {
                                TraineeId = traineeId,
                                CourseDetailId = courseDetailId,
                                point = sPoint,
                                Result = sResult,
                                Remark = sRemark,
                            };
                            if (isCalculate == (int)UtilConstants.BoolEnum.Yes)
                            {
                                entitySummary.LmsStatus = submitType == (int)UtilConstants.EStatus.Approve
                                    ? StatusIsSync
                                    : StatusModify;
                            }
                            else
                            {
                                entitySummary.LmsStatus = StatusIsSync;
                            }
                            if (insertOrNot)
                            {
                                _repoCourseResultSummaryService.Insert(entitySummary);

                            }
                        }


                        i++;

                    }

                    //  InsertOrUpdateStatusApi(courseId, UtilConstants.ApiStatus.Modify, UtilConstants.LMSStatus.Result);

                    if (submitType == (int)UtilConstants.EStatus.Approve)
                    {
                        var processStep = ProcessStep((int)UtilConstants.ApproveType.SubjectResult);
                        if (!processStep)
                        {
                            var course = CourseService.GetById(courseId);
                            Modify_TMS(false, course, (int)UtilConstants.ApproveType.SubjectResult, submitType, UtilConstants.ActionType.Approve, "", courseDetailId);
                            await Task.Run(() =>
                            {
                                var callLms = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
                                if (!callLms)
                                {
                                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertResult", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                                    //return Json(new AjaxResponseViewModel()
                                    //{
                                    //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                                    //    result = false
                                    //}, JsonRequestBehavior.AllowGet);
                                }
                            });
                        }
                    }

                }

                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertResult", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The RemarkComment.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RemarkComment(string id)
        {
            var courseMemberId = int.Parse(id);
            var courseMember = CourseMemberService.GetById(courseMemberId);

            if (courseMember == null)
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.ISVALID_DATA
                }, JsonRequestBehavior.AllowGet);
            }

            var courseResult =
                CourseService.GetCourseResult(
                    a => a.CourseDetailId == courseMember.Course_Details_Id && a.TraineeId == courseMember.Member_Id)?.FirstOrDefault();

            //var fullName = courseMember.Trainee.FirstName.Trim() + " " + courseMember.Trainee.LastName.Trim();
            var fullName = ReturnDisplayLanguage(courseMember.Trainee.FirstName,
                courseMember.Trainee.LastName);
            var model = new RemarkComment
            {
                FullName = fullName,
                EId = courseMember.Trainee.str_Staff_Id,
                Id = courseMember.Id,
                CourseDetailId = courseMember.Course_Details_Id ?? -1,
                TraineeId = courseMember.Member_Id ?? -1,
                Remark = courseResult?.CourseRemarkCheckFails?.FirstOrDefault()?.RemarkContent ?? "Incomplete",
                Result = courseResult?.Result ?? string.Empty,
                Type = courseResult?.Type ?? true
            };
            return PartialView("_Partials/_ReMarkComment", model);
        }

        /// <summary>
        /// The RemarkCommentMany.
        /// </summary>
        /// <param name="listid">The listid<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RemarkCommentMany(string listid)
        {
            var list = listid.Split(new char[] { ',' }).Select(p => Convert.ToInt32(p)).ToList();
            var Model = new List<RemarkComment>();
            foreach (var item in list)
            {
                var courseMemberId = item;
                var courseMember = CourseMemberService.GetById(courseMemberId);
                if (courseMember == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.ISVALID_DATA
                    }, JsonRequestBehavior.AllowGet);
                }
                var courseResult =
               CourseService.GetCourseResult(
                   a => a.CourseDetailId == courseMember.Course_Details_Id && a.TraineeId == courseMember.Member_Id)?.FirstOrDefault();
                var fullName = ReturnDisplayLanguage(courseMember.Trainee.FirstName,
              courseMember.Trainee.LastName);
                var model = new RemarkComment
                {
                    FullName = fullName,
                    EId = courseMember.Trainee.str_Staff_Id,
                    Id = courseMember.Id,
                    CourseDetailId = courseMember.Course_Details_Id ?? -1,
                    TraineeId = courseMember.Member_Id ?? -1,
                    Remark = courseResult?.CourseRemarkCheckFails?.FirstOrDefault()?.RemarkContent ?? "Incomplete",
                    Result = courseResult?.Result ?? string.Empty,
                    Type = courseResult?.Type ?? true
                };
                Model.Add(model);
            }
            return PartialView("_Partials/_ReMarkCommentMany", Model);
        }

        /// <summary>
        /// The RemarkComment.
        /// </summary>
        /// <param name="model">The model<see cref="RemarkComment"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult RemarkComment(RemarkComment model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (model.Type && string.IsNullOrEmpty(model.Remark))
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.VALIDATION_REMARKCOMMENT_REMARK
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                var tmsMember = CourseMemberService.Get(a =>
                    a.Course_Details_Id == model.CourseDetailId && a.Member_Id == model.TraineeId &&
                    a.IsActive == true && a.IsDelete != true).FirstOrDefault();
                if (tmsMember != null)
                {
                    var courseResult =
                        CourseService.GetCourseResult(
                            a => a.CourseDetailId == model.CourseDetailId && a.TraineeId == model.TraineeId).ToList();
                    if (courseResult.Count() > 0)
                    {
                        foreach (var result in courseResult)
                        {
                            //result.Type = model.Type;
                            ////result.Remark = model.Remark;
                            //result.Result = model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty;
                            //result.ModifiedBy = CurrentUser.USER_ID.ToString();
                            //result.ModifiedAt = DateTime.Now;
                            //result.inCourseMemberId = tmsMember.Id;
                            //CourseService.UpdateCourseResult(result);

                            Dictionary<string, object> dic_result = new Dictionary<string, object>();
                            dic_result.Add("Type", model.Type ? 1 : 0);
                            dic_result.Add("Result", model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty);
                            dic_result.Add("ModifiedBy", CurrentUser.USER_ID + "");
                            dic_result.Add("ModifiedAt", DateTime.Now);
                            dic_result.Add("inCourseMemberId", tmsMember.Id);
                            if (CMSUtils.UpdateDataSQLNoLog("Id", result.Id + "", "Course_Result", dic_result.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_result.Values.ToArray())) > 0)
                            {

                            }

                            var remarkresult = CourseService.GetCourseResultCheckFail(a => a.CourseResultID == result.Id);
                            if (remarkresult.Any())
                            {
                                var recordremarkitem = remarkresult.FirstOrDefault();
                                //recordremarkitem.RemarkContent = model.Remark;
                                //recordremarkitem.CreatedAt = DateTime.Now;
                                //CourseService.UpdateCourseResultCheckFail(recordremarkitem);

                                Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                dic_remark.Add("RemarkContent", model.Remark);
                                dic_remark.Add("CreatedAt", DateTime.Now);
                                if (CMSUtils.UpdateDataSQLNoLog("Id", recordremarkitem.Id + "", "CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                {

                                }
                            }
                            else
                            {
                                //var remark = new CourseRemarkCheckFail();
                                //remark.CourseResultID = result.Id;
                                //remark.RemarkContent = model.Remark;
                                //remark.CreatedAt = DateTime.Now;
                                //CourseService.InsertCourseResultCheckFail(remark);

                                Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                dic_remark.Add("CourseResultID", result.Id);
                                dic_remark.Add("RemarkContent", model.Remark);
                                dic_remark.Add("CreatedAt", DateTime.Now);
                                if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                {

                                }
                            }


                        }
                    }
                    else
                    {
                        var entity = new Course_Result
                        {
                            TraineeId = model.TraineeId,
                            CourseDetailId = model.CourseDetailId,
                            Score = model.Score ?? 0,
                            CreatedAt = DateTime.Now,
                            CreatedBy = CurrentUser.USER_ID.ToString(),
                            Type = model.Type,
                            //Remark = model.Remark,
                            Result = model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty,
                            inCourseMemberId = tmsMember.Id
                        };
                        CourseService.InsertCourseResult(entity);


                        //var remark = new CourseRemarkCheckFail();
                        //remark.CourseResultID = entity.Id;
                        //remark.RemarkContent = model.Remark;
                        //remark.CreatedAt = DateTime.Now;
                        //CourseService.InsertCourseResultCheckFail(remark);

                        Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                        dic_remark.Add("CourseResultID", entity.Id);
                        dic_remark.Add("RemarkContent", model.Remark);
                        dic_remark.Add("CreatedAt", DateTime.Now);
                        if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                        {

                        }
                    }
                }
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                    data = model.Type
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RemarkComment", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The RemarkCommentMany.
        /// </summary>
        /// <param name="model">The model<see cref="List{RemarkComment}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult RemarkCommentMany(List<RemarkComment> model)
        {
            var listsuccess = new List<int>();
            var listfail = new List<int>();
            foreach (var item in model)
            {
                if (item.Type && string.IsNullOrEmpty(item.Remark))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.VALIDATION_REMARKCOMMENT_REMARK
                    }, JsonRequestBehavior.AllowGet);
                }


            }
            foreach (var item in model)
            {
                try
                {

                    var tmsMember = CourseMemberService.Get(a =>
                        a.Course_Details_Id == item.CourseDetailId && a.Member_Id == item.TraineeId &&
                        a.IsActive == true && a.IsDelete != true).FirstOrDefault();
                    if (tmsMember != null)
                    {
                        var courseResult =
                            CourseService.GetCourseResult(
                                a => a.CourseDetailId == item.CourseDetailId && a.TraineeId == item.TraineeId).ToList();
                        if (courseResult.Any())
                        {
                            foreach (var result in courseResult)
                            {
                                //result.Type = item.Type;
                                ////result.Remark = model.Remark;
                                //result.Result = item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty;
                                //result.ModifiedBy = CurrentUser.USER_ID.ToString();
                                //result.ModifiedAt = DateTime.Now;
                                //result.inCourseMemberId = tmsMember.Id;
                                //CourseService.UpdateCourseResult(result);
                                Dictionary<string, object> dic_result = new Dictionary<string, object>();
                                dic_result.Add("Type", item.Type ? 1 : 0);
                                dic_result.Add("Result", item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty);
                                dic_result.Add("ModifiedBy", CurrentUser.USER_ID + "");
                                dic_result.Add("ModifiedAt", DateTime.Now);
                                dic_result.Add("inCourseMemberId", tmsMember.Id);
                                if (CMSUtils.UpdateDataSQLNoLog("Id", result.Id + "", "Course_Result", dic_result.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_result.Values.ToArray())) > 0)
                                {

                                }
                                var remarkresult = CourseService.GetCourseResultCheckFail(a => a.CourseResultID == result.Id);
                                if (remarkresult.Any())
                                {
                                    var recordremarkitem = remarkresult.FirstOrDefault();
                                    //recordremarkitem.RemarkContent = item.Remark;
                                    //recordremarkitem.CreatedAt = DateTime.Now;
                                    //CourseService.UpdateCourseResultCheckFail(recordremarkitem);
                                    Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                    dic_remark.Add("RemarkContent", item.Remark);
                                    dic_remark.Add("CreatedAt", DateTime.Now);
                                    if (CMSUtils.UpdateDataSQLNoLog("Id", recordremarkitem.Id + "", "CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                                else
                                {
                                    //var remark = new CourseRemarkCheckFail();
                                    //remark.CourseResultID = result.Id;
                                    //remark.RemarkContent = item.Remark;
                                    //remark.CreatedAt = DateTime.Now;
                                    //CourseService.InsertCourseResultCheckFail(remark);

                                    Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                    dic_remark.Add("CourseResultID", result.Id);
                                    dic_remark.Add("RemarkContent", item.Remark);
                                    dic_remark.Add("CreatedAt", DateTime.Now);
                                    if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                    {

                                    }
                                }

                                if (item.Type == true)
                                {
                                    listsuccess.Add(item.Id);
                                }
                                else
                                {
                                    listfail.Add(item.Id);
                                }
                            }
                        }
                        else
                        {
                            var entity = new Course_Result
                            {
                                TraineeId = item.TraineeId,
                                CourseDetailId = item.CourseDetailId,
                                Score = item.Score ?? 0,
                                CreatedAt = DateTime.Now,
                                CreatedBy = CurrentUser.USER_ID.ToString(),
                                Type = item.Type,
                                //Remark = model.Remark,
                                Result = item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty,
                                inCourseMemberId = tmsMember.Id
                            };
                            CourseService.InsertCourseResult(entity);

                            //var remark = new CourseRemarkCheckFail();
                            //remark.CourseResultID = entity.Id;
                            //remark.RemarkContent = item.Remark;
                            //remark.CreatedAt = DateTime.Now;
                            //CourseService.InsertCourseResultCheckFail(remark);

                            Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                            dic_remark.Add("CourseResultID", entity.Id);
                            dic_remark.Add("RemarkContent", item.Remark);
                            dic_remark.Add("CreatedAt", DateTime.Now);
                            if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                            {

                            }

                            if (item.Type == true)
                            {
                                listsuccess.Add(item.Id);
                            }
                            else
                            {
                                listfail.Add(item.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/RemarkComment", ex.Message);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS + ": " + ex,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true,
                data = string.Join(",", listsuccess),
                data1 = string.Join(",", listfail),
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The CheckSubmitResult.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CheckSubmitResult(int courseId)
        {
            bool check = false;
            var course = CourseService.GetById(courseId);
            var statusCourse = course?.TMS_APPROVES?.FirstOrDefault(b => b.int_id_status == (int)UtilConstants.EStatus.Block && b.int_Type == (int)UtilConstants.ApproveType.Course);
            if (statusCourse != null)
            {
                check = true;
            }
            return check;
        }

        /// <summary>
        /// The ResultHasInsert.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ResultHasInsert(int? id = 0)
        {
            if (id != 0)
            {
                return View(CourseDetailService.GetById(id));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// The AjaxHandlResultHasInsert.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlResultHasInsert(jQueryDataTableParamModel param, int id = -1)
        {
            try
            {
                int SubjectList = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? id : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                var data = CourseMemberService.Get(a => a.Course_Details_Id == SubjectList && a.DeleteApprove == null && a.IsDelete == null && a.Approve_Id != null);

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Trainee?.Department?.Code
                                                            : c.Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";

                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                 string.Empty,
                                                c?.Trainee?.str_Staff_Id,
                                                //c?.Trainee?.FirstName + " "+ c?.Trainee?.LastName ,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                                //c?.Trainee?.JobTitle?.Name,
                                                c?.Trainee?.Department?.Code,
                                                DateUtil.DateToString(c?.Course_Detail.dtm_time_from,"dd/MM/yyyy")  +"-"+ DateUtil.DateToString(c?.Course_Detail.dtm_time_to,"dd/MM/yyyy"),
                                                GetDetailResultByCourseDetail(UtilConstants.DetailResult.Score,c?.Trainee?.Id,c?.Course_Details_Id),
                                                GetDetailResultByCourseDetail(UtilConstants.DetailResult.Grade,c?.Trainee?.Id,c?.Course_Details_Id),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlResultHasInsert", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ImportResult.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ImportResult()
        {
            if (!Is_View("/Course/ImportResult"))
            {
                return RedirectToAction("Index", "Redirect");
            }
            ViewBag.CourseList = CourseService.Get().OrderByDescending(m => m.Id);
            return View();
        }

        /// <summary>
        /// The InputCerNo.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult InputCerNo(int? id = 0)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new FinalCourseResultModel()
            {
                Courses = CourseService.Get(a => a.StartDate >= timenow && a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.CourseResult && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.IsDeleted != true).OrderByDescending(b => b.StartDate).ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name)),
                //_repoTmsApproves.Get(
                //    a =>
                //        a.int_Type == (int)UtilConstants.ApproveType.CourseResult &&
                //        a.int_id_status == (int)UtilConstants.EStatus.Approve && a.Course.IsDeleted != true)
                //    .OrderByDescending(b => b.Course.StartDate)
                //    .ToDictionary(c => c.Course.Id, c => string.Format("{0} - {1}", c.Course.Code, c.Course.Name)),
                //Certificates = CourseService.GetCatCertificates(a => a.IsActive == true).ToDictionary(b => b.ID, b => b.Name),
                //Departments = DepartmentService.Get(null, CurrentUser.PermissionIds).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name),
                //JobTitles = _repoJobTiltle.Get(null, true).ToDictionary(a => a.Id, a => a.Name),
                //GroupCertificates = CourseService.GetAllGroupCertificate(a => a.IsActive == true).ToDictionary(a => a.Id, a => a.Name),
                SubjectList = _repoSubject.GetSubjectDetail(a => !string.IsNullOrEmpty(a.CertificateCode) && a.Course_Detail.Any(x => x.Course_Result.Any())).ToDictionary(a => a.Id, a => (a.Code + "_" + a.Name))
            };
            return View(model);
        }

        /// <summary>
        /// The SaveCertifiCateForGroup.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SaveCertifiCateForGroup(FormCollection form)

        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseid = string.IsNullOrEmpty(form["courseid"]) ? 0 : Convert.ToInt32(form["courseid"]);
                var certificateForAll = string.IsNullOrEmpty(form["CertificateForAll"]) ? -1 : Convert.ToInt32(form["CertificateForAll"].Trim());
                var certificateByUser = form.GetValues("CertificateByUser");
                var formDateCompleted = form["CreateDate"];
                var cerNo = form.GetValues("CerNo");
                var ATO = form["ATO"];
                var id = form.GetValues("Id_");
                var checkFail = form.GetValues("is_fail");
                var i = 0;
                var course = _repoCourse.GetById(courseid);
                if (id == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCateForGroup", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                var listFresult = new List<Course_Result_Final>();
                int count = 0;

                DateTime dateCompleted;
                DateTime.TryParse(formDateCompleted, out dateCompleted);
                foreach (var item in id)
                {
                    if (!string.IsNullOrEmpty(cerNo?[i]))
                    {
                        var fId = int.Parse(item);
                        var intCertificateId = certificateByUser != null ? int.Parse(certificateByUser[i]) : -1;
                        var fResult = CourseService.GetCourseResultFinalById(fId);
                        if (fResult != null)
                        {
                            if (cerNo != null)
                            {
                                var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.IdTrainee == fResult.Trainee.Id && a.TypeCertificate == 1 && a.CoureResultFinalID.HasValue && a.CoureResultFinalID == fResult.id);
                                var entity =
                                  new TMS_CertificateApproved();
                                if (cerNo[i] != "nocertificate")
                                {
                                    if (fResult.statusCertificate != 0 || string.IsNullOrEmpty(fResult.certificatefinal))
                                    {
                                        var catCertificate = CourseService.GetCatCertificateById(certificateForAll != -1 ? certificateForAll : intCertificateId);
                                        var content = BodyCertificate(catCertificate, null, fResult.Trainee,
                                             fResult.Course, fResult);
                                        //TypeCertificate = 1 dành cho group, 0 là dành cho subject

                                        if (listcertificateapproved.Any())
                                        {
                                            entity = listcertificateapproved.FirstOrDefault();
                                            if (entity.status != (int)UtilConstants.StatusApiApprove.Approved)
                                            {
                                                entity.IdTrainee = fResult.Trainee.Id;
                                                entity.certificatefinal = cerNo[i];
                                                entity.IsActive = true;
                                                entity.IsDeleted = false;
                                                entity.Path = content;
                                                entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                                entity.CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;
                                                entity.ModifyDate = DateTime.Now;
                                                entity.ModifyBy = CurrentUser.USER_ID;
                                            }


                                        }
                                        else
                                        {
                                            entity = new TMS_CertificateApproved();
                                            entity.IdTrainee = fResult.Trainee.Id;
                                            entity.CreateDate = DateTime.Now;
                                            entity.CreateBy = CurrentUser.USER_ID;
                                            entity.certificatefinal = cerNo[i];
                                            entity.IsActive = true;
                                            entity.IsDeleted = false;
                                            entity.Path = content;
                                            entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                            entity.CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;
                                            entity.CoureResultFinalID = fResult.id;
                                            entity.TypeCertificate = 1; //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                                            entity.IsRevoked = false;
                                        }
                                        CourseService.ModifyTMSCertificateAppovedEntity(entity);
                                        //fResult..CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;SRNO = cerNo[i];
                                        //fResult.Path = content;
                                        //fResult.CreateCertificateDate = DateTime.Now; //DateTime.Parse(form["CreateDate"]);
                                        //fResult.CreateCertificateBy = GetUser().USER_ID;
                                        //fResult.ATO = ATO;
                                        //fResult.LmsStatus = StatusModify;
                                        //fResult.CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;
                                        //listFresult.Add(fResult);
                                        //CourseService.UpdateCourseResultFinalReturnEntity(fResult);
                                        count++;
                                    }
                                }
                                else
                                {
                                    if (checkFail[i] == "Fail")
                                    {
                                        if (listcertificateapproved.Any())
                                        {
                                            entity = listcertificateapproved.FirstOrDefault();
                                            if (entity.status != (int)UtilConstants.StatusApiApprove.Approved)
                                            {
                                                entity.IsRevoked = true;
                                                entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                                CourseService.ModifyTMSCertificateAppovedEntity(entity);
                                                count++;
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }
                    i++;
                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.CERTIFICATE_TRAINEE_NOT_ADDNEW,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
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
                var count_temp = 0;
                var list_temp = new List<int>(new int[count_temp]);
                var firstOrDefault = UserContext.Get(a => a.IsDeleted != true && a.ISACTIVE == 1 && a.UserRoles.Any(x => x.ROLE.NAME == "HOD" && x.ROLE.IsActive == true), list_temp).ToList();

                if(firstOrDefault.Count() > 0)
                {
                    foreach (var item in firstOrDefault)
                    {
                        var to = item.ID ;
                        var notification = new Notification { Message = "Request for certificate approval. ", MessageContent = string.Format("The certificate course '{0}' needs to be approved.", course.Code), MessageVN = "Yêu cầu phê duyệt chứng chỉ", MessageContentVN = string.Format("Chứng chỉ của khóa '{0}' cần được phê duyệt.", course.Code), URL = "/Approve/Certificate", Type = (int)UtilConstants.NotificationType.AutoProcess };
                        notification.Notification_Detail.Add(new Notification_Detail
                        {
                            idmessenge = notification.MessageID,
                            datesend = DateTime.Now,
                            iddata = to,
                            iduserfrom = CurrentUser?.USER_ID,
                            status = 0,
                            IsDeleted = false,
                            IsActive = true
                        });
                        NotificationService.Insert(notification);
                    }
                    
                }
                else
                {
                    var to = 7;
                    var notification = new Notification { Message = "Request for certificate approval. ", MessageContent = string.Format("The certificate course '{0}' needs to be approved.", course.Code), MessageVN = "Yêu cầu phê duyệt chứng chỉ", MessageContentVN = string.Format("Chứng chỉ của khóa '{0}' cần được phê duyệt.", course.Code), URL = "/Approve/Certificate", Type = (int)UtilConstants.NotificationType.AutoProcess };
                    notification.Notification_Detail.Add(new Notification_Detail
                    {
                        idmessenge = notification.MessageID,
                        datesend = DateTime.Now,
                        iddata = to,
                        iduserfrom = CurrentUser?.USER_ID,
                        status = 0,
                        IsDeleted = false,
                        IsActive = true
                    });
                    NotificationService.Insert(notification);
                }
                
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SaveCertifiCateForGroup", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The SaveFilePNG.
        /// </summary>
        /// <param name="fullName">The fullName<see cref="string"/>.</param>
        /// <param name="courseName">The courseName<see cref="string"/>.</param>
        /// <param name="dateComplete">The dateComplete<see cref="string"/>.</param>
        /// <param name="path">The path<see cref="string"/>.</param>
        /// <param name="jobTitle">The jobTitle<see cref="string"/>.</param>
        private void SaveFilePNG(string fullName, string courseName, string dateComplete, string path, string jobTitle)
        {
        }

        /// <summary>
        /// The GetGrade.
        /// </summary>
        /// <param name="grade">The grade<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
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

        /// <summary>
        /// The GetGradeInt.
        /// </summary>
        /// <param name="grade">The grade<see cref="string"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetGradeInt(string grade)
        {
            var result = (int)UtilConstants.Grade.Fail;
            switch (grade)
            {
                case "Fail":
                    result = (int)UtilConstants.Grade.Fail;
                    break;
                case "Pass":
                    result = (int)UtilConstants.Grade.Pass;
                    break;
                case "Distinction":
                    result = (int)UtilConstants.Grade.Distinction;
                    break;
            }
            return result;
        }

        /// <summary>
        /// The GetATO.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult GetATO(int id = -1)
        {
            var ATO = string.Empty;
            var courseId = string.Empty;
            if (id != -1)
            {
                var model = CourseService.GetCourseResultFinal(a => a.courseid == id, new int[] { (int)UtilConstants.ApproveType.CourseResult }, (int)UtilConstants.EStatus.Approve).FirstOrDefault();
                ATO = model?.ATO ?? "";
                courseId = model?.courseid.ToString() ?? "";
            }
            var result = new { ATO = ATO, courseId = courseId };
            return Json(result);
        }

        /// <summary>
        /// The AjaxHandleLisInputCetificate.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandleLisInputCetificate(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());

                string fTraineeEID = string.IsNullOrEmpty(Request.QueryString["fillter_eid"]) ? "" : Request.QueryString["fillter_eid"].Trim();
                string fName = string.IsNullOrEmpty(Request.QueryString["fillter_name"]) ? "" : Request.QueryString["fillter_name"].Trim();
                string fCertificate = string.IsNullOrEmpty(Request.QueryString["fillter_certificate"]) ? "" : Request.QueryString["fillter_certificate"].Trim();

                string fSearchDate_fromCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_FromCreate"]) ? "" : Request.QueryString["fSearchDate_FromCreate"].ToString();
                string fSearchDate_toCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_toCreate"]) ? "" : Request.QueryString["fSearchDate_toCreate"].ToString();

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.FirstName + " " + c?.Trainee?.LastName
                                                            : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                                                            : sortColumnIndex == 4 ? c?.Trainee.Department.Name
                                                            : (object)c?.point);
                DataTable db = CMSUtils.GetDataSQL("", "TMS_Course_Member as cm inner join Course_Detail as cd on cm.Course_Details_Id = cd.Id inner join Course as c on c.Id = cd.CourseId", "Distinct cm.Member_Id", String.Format("c.Id = {0} and ISNULL(cm.IsDelete,0) <> 1 and ISNULL(cm.Status,0) <> 1	", courseList), "");
                var list = new List<string>();
                if (db.Rows.Count > 0)
                {
                    list = db.Rows.Cast<DataRow>().Select(row => row["Member_Id"].ToString()).ToList();
                }
                var data__ = CourseService.GetCourseResultFinal(a => list.Contains(a.traineeid + "") && a.courseid == courseList && a.IsDeleted == false, new int[] { (int)UtilConstants.ApproveType.CourseResult }, (int)UtilConstants.EStatus.Approve);
                var sortDirection = Request["sSortDir_0"] ?? "desc"; // asc or desc
                var data = (sortDirection == "asc")
                    ? data__.OrderBy(orderingFunction)
                    : data__
                        .OrderByDescending(orderingFunction);
                var groupsubject = _repoSubject.GetGroupSubject(a => a.IsActive == true && a.IsDeleted != true && !string.IsNullOrEmpty(a.CertificateCode));
                var _groupID = new List<int>();
                if (groupsubject != null)
                {
                    _groupID = groupsubject.Select(a => a.id).ToList();
                }

                var displayed = data.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var domainName = Request.Url.Authority;
                var result = from c in displayed.ToArray()
                             select new object[]
                             {
                        string.Empty,
                        c?.Trainee?.str_Staff_Id,
                        //c?.Trainee?.FirstName + " " + c?.Trainee?.LastName,
                        ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                        c?.Trainee?.JobTitle?.Name,
                        c?.Trainee?.Department?.Name,
                       (c?.point.HasValue != null && (bool)c?.point.HasValue && c?.point != 0) ? string.Format("{0:0.#}", c?.point) : string.Empty,
                        GetGrade(c?.grade),
                        GetGrade(c?.grade) == "Fail" ?
                            "<input  type='hidden' class='form-control' name='is_fail' value='Fail'/>" +
                            "<input  type='hidden' class='form-control' name='Id_' value='"+c?.id+"'/>" +
                            "<input type='hidden' class='form-control' readonly name='CerNo' value='nocertificate'/>" :

                            (!_groupID.Contains(c.Course.GroupSubjectId.HasValue ? (int)c?.Course?.GroupSubjectId : -1 ) ?  "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>" +
                            "<input  type='hidden' class='form-control' name='Id_' value='"+c?.id+"'/>" +
                            "<input type='hidden' class='form-control' readonly name='CerNo' value='nocertificate'/>" : ( !string.IsNullOrEmpty(c?.certificatefinal) ?
                            "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>" +
                            "<input  type='hidden' class='form-control' name='Id_' value='"+c?.id+"'/>" +
                            "<input type='text' class='form-control' readonly name='CerNo' value='"+c?.certificatefinal+"'/>" :

                            "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>" +
                            "<input  type='hidden' class='form-control' name='Id_' value='"+c?.id+"'/>" +
                            "<input type='text' class='form-control' readonly name='CerNo' value='"+CreateCodeCertificate(c?.Course?.GroupSubjectId)+"'/>")) ,

                        GetCertificate(c?.grade,1,c?.CAT_CERTIFICATE_ID),
                        GetGrade(c?.grade) == "Fail" ? "" : (GetGrade(c?.grade) != "Fail" && string.IsNullOrEmpty(c?.certificatefinal) ? "" : ( string.IsNullOrEmpty(c?.statusCertificate.ToString()) ? "" : (c?.statusCertificate == 0 ? "<span class='label label-success'>Have a certificate</span>" : "<span class='label label-warning'>Have been revoked</span>"))),
                        //!string.IsNullOrEmpty(c?.SRNO)
                        //    ? ( (bool)c?.Path.StartsWith("<div") ? "<a onclick='Blank_Review("+c?.traineeid+")' title='View'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i> </a> "+
                        //      "<div id='c"+c?.traineeid+"' style='display:none;'><span id='a"+c?.traineeid+"' class='widget'>" + c?.Path+ "</span></div>" : "<a  href='//"+domainName + GetByKey("PathFileExtImage") +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>")
                        //    : ""
                             };

                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// The GetCertificate.
        /// </summary>
        /// <param name="grade">The grade<see cref="int?"/>.</param>
        /// <param name="typevalue">The typevalue<see cref="int?"/>.</param>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetCertificate(int? grade, int? typevalue, int? id = -1)
        {
            var html = new StringBuilder();
            var data = CourseService.GetCatCertificates(a => a.IsActive == true && a.Type == typevalue);
            html.Append("<select class='form-control Certificate select2'  id='CertificateByUser' name='CertificateByUser' style='" + ((grade > 0) ? string.Empty : "visibility:hidden;") + "' >");
            if (data.Any())
            {
                foreach (var item in data)
                {

                    html.Append("<option " + (item.ID == id ? "selected" : "") + "  value='" + item.ID + "'>" + item.Name + "</option>");

                }
            }
            else
            {
                html.Append("<option value='-1'>" + Resource.lblSelectAnOption + "</option> ");
            }
            html.Append("</select>");
            return html.ToString();
        }

        /// <summary>
        /// The AssignTrainee.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AssignTrainee()
        {
            var dataCourseResultProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.CourseResult, (int)UtilConstants.EStatus.Approve);
            //var checkApprove = _repoTmsApproves.Get(a => a.Course.IsDeleted == false , (int)UtilConstants.ApproveType.AssignedTrainee, (int)UtilConstants.EStatus.Approve);
            var model = new AssignTraineeModel()
            {
                //Departments = GetDepartmentModel().OrderBy(a => a.Ancestor).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.DepartmentName)),
                Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster),
                JobTitles = _repoJobTiltle.Get(a => a.IsDelete == false).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name),
                GroupTrainees = _repoEmployeeService.GetAllGroupTrainees(a => a.IsActived == true).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name)),
                //khoa hoan thanh
                Prerequisite = CourseService.Get(a => dataCourseResultProccess.Any(b => b.Course == a)).OrderBy(a => a.Id).ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name)),
                ProcessStep = ProcessStep((int)UtilConstants.ApproveType.AssignedTrainee)
            };
            return View(model);
        }

        /// <summary>
        /// The AjaxHandlerAvailableSubject.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerAvailableSubject(jQueryDataTableParamModel param)
        {
            try
            {
                var courseId = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var DepartmentList = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
                var JobtitleList = string.IsNullOrEmpty(Request.QueryString["JobtitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobtitleList"].Trim());
                var EID = string.IsNullOrEmpty(Request.QueryString["EID"]) ? string.Empty : Request.QueryString["EID"].Trim().ToLower();
                var FullName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].Trim().ToLower();
                var GroupTraineeId = string.IsNullOrEmpty(Request.QueryString["GroupTrainee"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupTrainee"].Trim());
                var PrerequisiteId = string.IsNullOrEmpty(Request.QueryString["Prerequisite"]) ? -1 : Convert.ToInt32(Request.QueryString["Prerequisite"].Trim());

                var subjects = CourseService.GetById(courseId)?.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => a.SubjectDetailId).Distinct();

                var dataOrientation = _repoOrientationService.Get(a => a.Orientation_Item.Any(b => subjects.Contains((int)b.SubjectId))).Select(a => a.TraineeId);

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.LastName
                                                            : sortColumnIndex == 3 ? c?.Department?.Ancestor
                                                             : sortColumnIndex == 4 ? c?.JobTitle?.Name
                                                          : c?.Department?.Ancestor);
                var lstDepartment = new List<int?>();
                var department = DepartmentService.GetById(DepartmentList);
                if (department != null)
                {
                    var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).OrderBy(b => b.Ancestor).Select(a => a.Id);
                    if (departmentIds.Any())
                    {
                        foreach (var id in departmentIds)
                        {
                            lstDepartment.Add(id);
                        }
                    }
                }
                var data = subjects != null ?
                    EmployeeService.Get(
                        //tru nhung nguoi da dang ky
                        a => !a.Course_Result_Final.Any(x => x.IsDeleted == false && x.courseid == courseId) &&
                             /////
                             //!a.TMS_Course_Member.Any(x => x.IsActive && x.IsDelete == false && subjects.Contains(x.Course_Detail.SubjectDetailId)) &&
                             //nhung nguoi co jobtitle phu hop
                             //a.JobTitle.Title_Standard.Any(x => subjects.Contains(x.Subject_Id)) &&
                             //nhung nguoi co trong danh sach orientation
                             dataOrientation.Contains(a.Id) &&

                             (DepartmentList == -1 || lstDepartment.Contains(a.Department_Id)) &&
                             (JobtitleList == -1 || a.Job_Title_id == JobtitleList) &&
                             (string.IsNullOrEmpty(EID) || a.str_Staff_Id.Contains(EID)) &&
                               (string.IsNullOrEmpty(FullName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(FullName.Trim())))


                        ) : new List<Trainee>().AsQueryable();
                var filtered = (Request["sSortDir_0"] == "asc")
                    ? data.OrderBy(orderingFunction)
                    : data.OrderByDescending(orderingFunction);
                // var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                    //c?.FirstName.Trim() + " " + c?.LastName.Trim(),
                                    ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                    //c?.Department?.Name,
                                    c?.JobTitle?.Name,
                                    c?.str_Email,
                                 "<input type='checkbox' name='id[]' value='"+c?.Id+"'>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAvailableSubject", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerAvailableSubject2.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerAvailableSubject2(jQueryDataTableParamModel param)
        {
            try
            {
                var courseId = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList2"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList2"].Trim());
                var jobtitleId = string.IsNullOrEmpty(Request.QueryString["JobtitleList2"]) ? -1 : Convert.ToInt32(Request.QueryString["JobtitleList2"].Trim());
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["EID2"]) ? string.Empty : Request.QueryString["EID2"].Trim().ToLower();
                var fullName = string.IsNullOrEmpty(Request.QueryString["FullName2"]) ? string.Empty : Request.QueryString["FullName2"].Trim().ToLower();
                var groupTraineeId = string.IsNullOrEmpty(Request.QueryString["GroupTrainee2"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupTrainee2"].Trim());
                var prerequisiteId = string.IsNullOrEmpty(Request.QueryString["Prerequisite2"]) ? -1 : Convert.ToInt32(Request.QueryString["Prerequisite2"].Trim());
                const int pass = (int)UtilConstants.Grade.Pass;
                const int distinction = (int)UtilConstants.Grade.Distinction;

                //var listBindSubject = courseId != -1 ? CourseService.GetById(courseId)?.Course_Subject_Item.Select(a => (int)a.SubjectId).ToList() : new List<int>();
                //var resultItem = CourseService.GetCourseResultSummaries(a => a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.Result == "Pass").ToList();

                var listItem = new List<Course_Result_Summary>();
                var listTrainee = new List<int>();
                var count = 1;

                //foreach (var item in resultItem.OrderBy(a => a.TraineeId))
                //{
                //    foreach (var _item in listBindSubject)
                //    {
                //        if (item.Course_Detail.SubjectDetailId == _item)
                //        {
                //            listItem.Add(item);
                //        }
                //    }
                //}
                if (listItem.Count() != 0)
                {
                    for (var i = 1; i < listItem.Count(); i++)
                    {
                        if (listItem.ElementAt(i).TraineeId == listItem.ElementAt(i - 1).TraineeId)
                        {
                            count++;
                        }
                        //if (count == listBindSubject.Count())
                        //{
                        //    listTrainee.Add((int)listItem.ElementAt(i).TraineeId);
                        //    count = 1;
                        //}
                    }
                }

                //////////////////////////////////////////////////////////////
                var lstDepartment = new List<int?>();
                var department = DepartmentService.GetById(departmentId);
                if (department != null)
                {
                    var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).Select(a => a.Id);
                    if (departmentIds.Any())
                    {
                        foreach (var id in departmentIds)
                        {
                            lstDepartment.Add(id);
                        }
                    }
                }
                //var courseDetailIds = _repoCourseDetail.GetByCourse(courseId).Select(a => a.Id);
                var members = CourseMemberService.Get(a => a.DeleteApprove != 1 && a.IsDelete != true &&  a.IsActive == true && a.Course_Detail.CourseId == courseId, true).Select(a => a.Member_Id);
                var EID_ = new List<string>();
                var FullName_ = new List<string>();
                if (!string.IsNullOrEmpty(traineeCode))
                {
                    EID_ = traineeCode.Split(',').ToList();
                }
                if (!string.IsNullOrEmpty(fullName))
                {
                    FullName_ = fullName.Split(',').ToList();
                }
                var data = EmployeeService.Get(a => /*(listBindSubject.Count() == 0 || listTrainee.Contains(a.Id)) &&*/a.IsActive == true && a.IsDeleted != true &&
                                 (courseId != -1 && departmentId != -1 || jobtitleId != -1 || !string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(traineeCode) || groupTraineeId != -1 || prerequisiteId != -1)
                                 && (departmentId == -1 || lstDepartment.Contains(a.Department_Id))
                                 && (jobtitleId == -1 || a.Job_Title_id == jobtitleId)
                                 && (string.IsNullOrEmpty(traineeCode) || EID_.Any(t => a.str_Staff_Id.Equals(t)))
                                 && (string.IsNullOrEmpty(fullName) || FullName_.Any(t => (string.IsNullOrEmpty(a.LastName) || (a.LastName == a.FirstName) ? a.FirstName : a.LastName + " " + a.FirstName).Contains(t)))
                                 && (groupTraineeId == -1 || a.GroupTrainee_Item.Any(b => b.GroupTraineeId == groupTraineeId))
                                 && (prerequisiteId == -1 || a.Course_Result_Final.Any(b => b.courseid == prerequisiteId && (b.grade == pass || b.grade == distinction)))
                                 && !members.Contains(a.Id), true);

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                             : sortColumnIndex == 2 ? c?.LastName
                                                             : sortColumnIndex == 3 ? c?.Department?.Ancestor
                                                              : sortColumnIndex == 4 ? c?.JobTitle?.Name
                                                           : c?.Department?.Ancestor);

                var filtered = (Request["sSortDir_0"] == "asc")
                   ? data.OrderBy(orderingFunction)
                   : data.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                   c?.str_Staff_Id,
                                   //c?.FirstName.Trim() + " " +c?.LastName.Trim(),
                                   ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                  // c?.Department?.Name,
                                   c?.JobTitle?.Name,
                                   c?.str_Email,
                                    "<input id='remark' type='text' name='remark[]' value='' />",
                                   "<input type='checkbox' name='id2[]' value='"+c?.Id+"'>",

                        };
                var jsonresult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAvailableSubject2", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerAssignTrainee.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerAssignTrainee(jQueryDataTableParamModel param)
        {
            try
            {

                var courseId = string.IsNullOrEmpty(Request.QueryString["courseId"]) ? -1 : Convert.ToInt32(Request.QueryString["courseId"].Trim());
                var CheckTrainee = string.IsNullOrEmpty(Request.QueryString["CheckTrainee"]) ? string.Empty : Request.QueryString["CheckTrainee"];

                var sss = CourseMemberService.Get(b => b.DeleteApprove != 1 && b.Course_Detail.CourseId == courseId
                                                  && b.Course_Details_Id != null
                                                  && b.IsDelete != true
                                                  && b.IsActive == true, true).Select(a => a.Member_Id);

                var model = EmployeeService.Get(a => a.IsDeleted != true && sss.Contains(a.Id), true);
                var VerticalBar = GetByKey("VerticalBar");
                //var model = data2.Select(a => new AjaxAssignTrainee()
                //{
                //    Id = a.Id,
                //    TraineeCode = a.str_Staff_Id,
                //    FullName = ReturnDisplayLanguage(a.FirstName, a.LastName),
                //    Option = "<input type = 'checkbox' name = 'idAssign[]' value = '" + a.Id + "' />" + VerticalBar + "<a href = 'javascript:void(0)' onclick = 'detail(" + a.Id + ")' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>",
                //    JobTitle = a?.JobTitle?.Name,
                //    Email = a.str_Email,
                //    Remark = a?.TMS_Course_Member_Remark?.FirstOrDefault(b => b.TraineeId == a.Id && b.CourseId == courseId)?.remark?.ToString() ?? ""

                //});
                IEnumerable<Trainee> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (o
                                                          => sortColumnIndex == 1 ? o.str_Staff_Id
                                                          //: sortColumnIndex == 2 ? o.LastName
                                                          : o.str_Staff_Id);
                var order = Request["sSortDir_0"];
                if (order == null)
                    order = "asc";
                filtered = order == "asc"
                    ? filtered
                        .OrderBy(orderingFunction)
                    : filtered
                        .OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                    ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                    c?.JobTitle?.Name,
                                    c?.str_Email,
                                    c?.TMS_Course_Member_Remark?.FirstOrDefault(b => b.TraineeId == c.Id && b.CourseId == courseId)?.remark ?? "",
                                   "<input type = 'checkbox' name = 'idAssign[]'"+(CheckTrainee.Trim().Contains(c.Id.ToString().Trim()) ? "checked": "")+" onclick='CheckTrainee(this,"+c.Id+")'  value = '" + c.Id + "' />" + VerticalBar + "<a href = 'javascript:void(0)' onclick = 'detail(" + c.Id + ")' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>"

                                    //c?.FullName,
                                    //c?.JobTitle,
                                    //c?.Email,
                                    //c?.Remark,
                                    //c?.Option
                        };
                var jsonresult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAssignTrainee", ex.Message);
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
        public ActionResult AjaxHandlerAssignTrainee_Approve(jQueryDataTableParamModel param)
        {
            try
            {

                var courseId = string.IsNullOrEmpty(Request.QueryString["courseId"]) ? -1 : Convert.ToInt32(Request.QueryString["courseId"].Trim());
                var CheckTrainee = string.IsNullOrEmpty(Request.QueryString["CheckTrainee"]) ? string.Empty : Request.QueryString["CheckTrainee"];

                var sss = CourseMemberService.Get(b => b.Course_Detail.CourseId == courseId
                                                  && b.Course_Details_Id != null
                                                  && (b.DeleteApprove != 1 || (b.DeleteApprove == 1 && b.LmsStatus != 99))
                                                  && b.IsDelete != true
                                                  && b.IsActive == true && (b.Status != 1), true).Select(a => a.Member_Id);

                var model = EmployeeService.Get(a => a.IsDeleted != true && sss.Contains(a.Id), true);
                var VerticalBar = GetByKey("VerticalBar");
                //var model = data2.Select(a => new AjaxAssignTrainee()
                //{
                //    Id = a.Id,
                //    TraineeCode = a.str_Staff_Id,
                //    FullName = ReturnDisplayLanguage(a.FirstName, a.LastName),
                //    Option = "<input type = 'checkbox' name = 'idAssign[]' value = '" + a.Id + "' />" + VerticalBar + "<a href = 'javascript:void(0)' onclick = 'detail(" + a.Id + ")' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>",
                //    JobTitle = a?.JobTitle?.Name,
                //    Email = a.str_Email,
                //    Remark = a?.TMS_Course_Member_Remark?.FirstOrDefault(b => b.TraineeId == a.Id && b.CourseId == courseId)?.remark?.ToString() ?? ""

                //});
                IEnumerable<Trainee> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (o
                                                          => sortColumnIndex == 1 ? o.str_Staff_Id
                                                          //: sortColumnIndex == 2 ? o.LastName
                                                          : o.str_Staff_Id);
                var order = Request["sSortDir_0"];
                if (order == null)
                    order = "asc";
                filtered = order == "asc"
                    ? filtered
                        .OrderBy(orderingFunction)
                    : filtered
                        .OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                    ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                    c?.JobTitle?.Name,
                                    c?.str_Email,
                                    c?.TMS_Course_Member_Remark?.FirstOrDefault(b => b.TraineeId == c.Id && b.CourseId == courseId)?.remark ?? "",
                                   "<input type = 'checkbox' name = 'idAssign[]'"+(CheckTrainee.Trim().Contains(c.Id.ToString().Trim()) ? "checked": "")+" onclick='CheckTrainee(this,"+c.Id+")'  value = '" + c.Id + "' />" + VerticalBar + "<a href = 'javascript:void(0)' onclick = 'detail(" + c.Id + ")' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>"

                                    //c?.FullName,
                                    //c?.JobTitle,
                                    //c?.Email,
                                    //c?.Remark,
                                    //c?.Option
                        };
                var jsonresult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAssignTrainee", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// The SubmitLmsAssign.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> SubmitLmsAssign(FormCollection form)
        {
            int courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = !string.IsNullOrEmpty(form["idLmsAssign[]"]) ? form["idLmsAssign[]"].Trim().Split(new char[] { ',' }) : null;
            idtrainee = CMSUtils.SetObjectNull(idtrainee);
            if (courseId == -1)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }
            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            try
            {
                var course = CourseService.GetById(courseId);
                if (course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.VALIDATION_STEP_ASSIGNTRAINEE_APPROVE,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                foreach (var idT in idtrainee)
                {
                    if (!CMSUtils.IsNull(idT))
                    {
                        int id = int.Parse(idT.ToString());
                        if (id != 0)
                        {
                            CourseService.UpdateLmsAssgin(id, courseId);
                        }

                    }

                }
                //InsertOrUpdateStatusApi(courseId, UtilConstants.ApiStatus.Modify, UtilConstants.LMSStatus.AssignTrainee);

                if (course != null)
                {
                    var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
                    var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                    var result = string.Empty;
                    var data = string.Empty;

                    var courseDetail = course.Course_Detail;
                    if (courseDetail.Any())
                    {

                        result += "<ul>";
                        foreach (var item in courseDetail.Where(a => a.IsDeleted == false))
                        {
                            var count = item.TMS_Course_Member.Count(a => a.IsDelete == false && a.Status == (int)UtilConstants.TypeAssign.Assigned);
                            result += "<li>" + item.SubjectDetail.Name + "&nbsp;:&nbsp;<font " + (count > maxtrainee ? "color='red'" : "") + ">" + count + "&nbsp;</font>/&nbsp;" + maxtrainee + "</li>";
                        }
                        result += "</ul>";
                        var countResult = course.Course_Result_Final.Count(a => a.IsDeleted == false);
                        data += "&nbsp;:&nbsp;<font " + (countResult > maxtrainee ? "color='red'" : "") + " >&nbsp;" + countResult + "&nbsp;</font>/&nbsp;" + maxtrainee;
                    }

                    await Task.Run(() =>
                    {
                        var checkLMSAssign = ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.LMSAssign);
                        //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);

                        if (checkLMSAssign != null)
                        {

                            if (idtrainee != null)
                            {
                                var y = 0;
                                foreach (var arr_c in idtrainee)
                                {
                                    if (!CMSUtils.IsNull(idtrainee[y]))
                                    {
                                        int id = int.Parse(idtrainee[y].ToString());
                                        var trainee = EmployeeService.GetById(id);

                                        Sent_Email_TMS(null, trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee, true);
                                    }
                                    y++;
                                }

                            }
                        }

                        var callLms = CallServices(UtilConstants.CRON_TRAINEE_HISTORY);
                        if (!callLms)
                        {
                            //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign", string.Format(Messege.ALERT_MAXTRAINEE, courseName, result) + " " + Messege.ERROR_CALL_LMS);
                            //return Json(new AjaxResponseViewModel()
                            //{
                            //    message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result) + "<br />" + Messege.ERROR_CALL_LMS,
                            //    data = data,
                            //    result = false
                            //}, JsonRequestBehavior.AllowGet);
                        }
                    });
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                        data = data,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerLMSAssign.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerLMSAssign(jQueryDataTableParamModel param)
        {
            try
            {
                var courseId = string.IsNullOrEmpty(Request.QueryString["courseId"]) ? -1 : Convert.ToInt32(Request.QueryString["courseId"].Trim());
                var eId = string.IsNullOrEmpty(Request.QueryString["EID3"]) ? string.Empty : Request.QueryString["EID3"].Trim();
                var name = string.IsNullOrEmpty(Request.QueryString["FullName3"]) ? string.Empty : Request.QueryString["FullName3"].Trim();

                var data = CourseMemberService.Get(a => (string.IsNullOrEmpty(eId) || a.Trainee.str_Staff_Id.ToLower().Trim() == eId.ToLower()) &&

                  (string.IsNullOrEmpty(name) || ((a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim()).Contains(name.Trim())))


                && a.Course_Detail.Course.Id == courseId && a.Status == (int)UtilConstants.APIAssign.Pending);


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.FirstName
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.Course?.Name
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 3 ? c?.AssignBy
                                                          : c.Trainee.LastName);


                var filtered = (Request["sSortDir_0"] == "asc") ? data.OrderBy(orderingFunction)
                                    : data.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[]
                             {
                        string.Empty,
                        //c?.Trainee?.FirstName?.Trim() + " " + c?.Trainee?.LastName?.Trim(),
                        ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                        c?.Course_Detail?.Course?.Name,
                        c?.Course_Detail?.SubjectDetail?.Name,
                        c?.AssignBy,
                        c?.Status == (int)UtilConstants.APIAssign.Approved ? "<input type='checkbox' name='idLmsAssign[]' value='"+c.Member_Id+",1' checked='checked' />" : "<input type='checkbox' name='idLmsAssign[]' value='"+c?.Member_Id+",0' />"
                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerLMSAssign", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerAssignTraineeSubject.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerAssignTraineeSubject(jQueryDataTableParamModel param)
        {
            try
            {
                var courseId = string.IsNullOrEmpty(Request.QueryString["courseId"]) ? -1 : Convert.ToInt32(Request.QueryString["courseId"].Trim());
                var employeeId = string.IsNullOrEmpty(Request.QueryString["traineeId"]) ? -1 : Convert.ToInt32(Request.QueryString["traineeId"].Trim());
                var data =
                    CourseMemberService.Get(
                        a =>
                            a.Member_Id == employeeId && a.Course_Detail.Course.Id == courseId &&
                             a.Course_Detail.IsDeleted == false, true);

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.Trainee.FirstName : sortColumnIndex == 2 ? c?.Course_Detail?.SubjectDetail?.Name
                                                          : c.Course_Detail.SubjectDetail.Name);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                   ReturnDisplayLanguage(c.Trainee.FirstName ,c?.Trainee.LastName),
                                   c.Course_Detail?.SubjectDetail?.Name,
                                   c.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Subject(0,"+c.Id+")' aria-hidden='true' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Subject(1,"+c.Id+")' aria-hidden='true' style='cursor: pointer;'></i>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAssignTraineeSubject", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The CheckAssignTrainee.
        /// </summary>
        /// <param name="date_form">The date_form<see cref="string"/>.</param>
        /// <param name="date_to">The date_to<see cref="string"/>.</param>
        /// <param name="time_from">The time_from<see cref="string"/>.</param>
        /// <param name="time_to">The time_to<see cref="string"/>.</param>
        /// <param name="traineeId">The traineeId<see cref="int"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckAssignTrainee(string date_form, string date_to, string time_from, string time_to, int traineeId)
        {
            var check = false;
            if (!string.IsNullOrEmpty(date_form) || !string.IsNullOrEmpty(date_to))
            {
                var dateFrom = Convert.ToDateTime(date_form);
                var dateTo = Convert.ToDateTime(date_to);
                var timeFrom = DateTime.ParseExact(time_from, "HH:mm", CultureInfo.InvariantCulture);
                var timeTo = DateTime.ParseExact(time_to, "HH:mm", CultureInfo.InvariantCulture);

                var checklist = CourseDetailService.Get(a =>
                ((a.dtm_time_from >= dateFrom && dateFrom <= a.dtm_time_to) || (a.dtm_time_from >= dateTo && dateTo <= a.dtm_time_to))
                && a.TMS_Course_Member.Any(b => b.Member_Id == traineeId && b.IsDelete == false && b.IsActive == true)

                );
                if (checklist.Any())
                {
                    foreach (var item in checklist)
                    {
                        if (!string.IsNullOrEmpty(item.time_from) || !string.IsNullOrEmpty(item.time_to))
                        {
                            var timeFromConvert = DateTime.ParseExact(item.time_from, "HH:mm", CultureInfo.InvariantCulture);
                            var timeToConvert = DateTime.ParseExact(item.time_to, "HH:mm", CultureInfo.InvariantCulture);
                            if ((timeFromConvert >= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert >= timeTo && timeTo <= timeToConvert))
                            {
                                check = true;
                                break;
                            }
                        }
                    }
                }
                // && ((((!string.IsNullOrEmpty(a.time_from) ? int.Parse(a.time_from) : 0 ) > timeFrom) || ((!string.IsNullOrEmpty(a.time_to) ? int.Parse(a.time_to) : 0) < timeFrom)) && (((!string.IsNullOrEmpty(a.time_from) ? int.Parse(a.time_from) : 0) > timeTo) || ((!string.IsNullOrEmpty(a.time_to) ? int.Parse(a.time_to) : 0) < timeTo)))

            }
            return check;
        }

        /// <summary>
        /// The WarningAssignTranee.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult WarningAssignTranee(FormCollection form)
        {
            var type = string.IsNullOrEmpty(form["type"]) ? -1 : Convert.ToInt32(form["type"]);
            var courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            var trainee = !string.IsNullOrEmpty(form["id[]"]) ? form["id[]"].Trim().Split(new char[] { ',' }) : null;
            var trainee2 = form["id2[]"] != null ? form.GetValues("id2[]") : null;
            var traineeLms = !string.IsNullOrEmpty(form["idLmsAssign[]"]) ? form["idLmsAssign[]"].Trim().Split(new char[] { ',' }) : null;
            var course = CourseService.GetById(courseId);

            if (course != null)
            {

                if (false)//course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.VALIDATION_STEP_ASSIGNTRAINEE_APPROVE,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
                    var mintrainee = course.MinTrainee.HasValue ? course.MinTrainee : 0;
                    var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                    var result = string.Empty;
                    var membersResult = CourseService.GetCourseResultFinal(a => a.courseid == courseId, new int[] { (int)UtilConstants.ApproveType.Course }, (int)UtilConstants.EStatus.Approve).Count();
                    var fullName = string.Empty;
                    var dateCourseDetail = string.Empty;
                    var timeCourseDetail = string.Empty;
                    switch (type)
                    {
                        case 1:

                            int itab = 0;
                            var check = false;
                            if (trainee == null || !trainee.Any())
                            {
                                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/WarningAssignTranee/Tab1", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                                return Json(new AjaxResponseViewModel()
                                {
                                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (var idform in trainee)
                                {

                                    var traineeId = int.Parse(idform);
                                    var courseDetails = course.Course_Detail;
                                    if (courseDetails.Any())
                                    {
                                        foreach (var courseDetail in courseDetails)
                                        {

                                            check = CheckAssignTrainee(courseDetail.dtm_time_from?.ToShortDateString(), courseDetail.dtm_time_to?.ToShortDateString(), courseDetail.time_from, courseDetail.time_to, traineeId);
                                            if (check)
                                            {
                                                var model = _repoEmployeeService.GetById(traineeId);
                                                if (model != null)
                                                    //fullName = model.str_Staff_Id + " - " + model.FirstName.Trim() + " " + model.LastName.Trim();
                                                    fullName = model.str_Staff_Id + " - " + ReturnDisplayLanguage(model.FirstName, model.LastName);

                                                dateCourseDetail = courseDetail.dtm_time_from?.ToShortDateString() + " - " +
                                                                    courseDetail.dtm_time_to?.ToShortDateString();
                                                timeCourseDetail = courseDetail.time_from + " - " + courseDetail.time_to;
                                            }
                                        }
                                    }
                                    itab++;
                                }
                            }

                            if (check)
                            {

                                return Json(new AjaxResponseViewModel()
                                {
                                    type = (int)UtilConstants.TypeCheck.Checked,
                                    message = string.Format(Messege.CHECK_ASSIGNTRAINEE, fullName, dateCourseDetail, timeCourseDetail, "<br />"),
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var traineeform = itab;
                                var math = traineeform + membersResult;
                                if (CheckSiteConfig(UtilConstants.KEY_MIN_TRAINEE))
                                {
                                    if (math < mintrainee)
                                    {
                                        result = "<br />&nbsp;:&nbsp;<font color='red'>" + math + "&nbsp;</font>/&nbsp;" + mintrainee + "<br />";
                                        return Json(new AjaxResponseViewModel()
                                        {
                                            message = string.Format(Messege.WARNING_MIN_ASSIGNTRAINEE, courseName, result, mintrainee, "<br>"),
                                            result = true
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (math > maxtrainee)
                                {
                                    result = "<br />&nbsp;:&nbsp;<font color='red'>" + math + "&nbsp;</font>/&nbsp;" + maxtrainee + "<br />";
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        message = string.Format(Messege.WARNING_MAX_ASSIGNTRAINEE, courseName, result, maxtrainee, "<br>"),
                                        result = true
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;

                        case 2:
                            var check2 = false;
                            if (trainee2 == null || !trainee2.Any())
                            {
                                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/WarningAssignTranee/Tab2", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                                return Json(new AjaxResponseViewModel()
                                {
                                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (var idform in trainee2)
                                {
                                    var traineeId = int.Parse(idform);
                                    var courseDetails = course.Course_Detail;
                                    if (courseDetails.Any())
                                    {
                                        foreach (var courseDetail in courseDetails)
                                        {

                                            check2 = CheckAssignTrainee(courseDetail.dtm_time_from?.ToShortDateString(), courseDetail.dtm_time_to?.ToShortDateString(), courseDetail.time_from, courseDetail.time_to, traineeId);
                                            if (check2)
                                            {
                                                var model = _repoEmployeeService.GetById(traineeId);
                                                if (model != null)
                                                    //fullName = model.str_Staff_Id + " - " + model.FirstName.Trim() + " " + model.LastName.Trim();
                                                    fullName = model.str_Staff_Id + " - " + ReturnDisplayLanguage(model.FirstName, model.LastName);
                                                dateCourseDetail = courseDetail.dtm_time_from?.ToShortDateString() + " - " +
                                                                    courseDetail.dtm_time_to?.ToShortDateString();
                                                timeCourseDetail = courseDetail.time_from + " - " + courseDetail.time_to;
                                            }
                                        }
                                    }

                                }
                            }
                            if (check2)
                            {
                                return Json(new AjaxResponseViewModel()
                                {
                                    type = (int)UtilConstants.TypeCheck.Checked,
                                    message = string.Format(Messege.CHECK_ASSIGNTRAINEE, fullName, dateCourseDetail, timeCourseDetail, "<br />"),
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var traineeform2 = trainee2?.Count();
                                var math2 = traineeform2 + membersResult;
                                if (CheckSiteConfig(UtilConstants.KEY_MIN_TRAINEE))
                                {
                                    if (math2 < mintrainee)
                                    {
                                        result = "<br />&nbsp;:&nbsp;<font color='red'>" + math2 + "&nbsp;</font>/&nbsp;" + mintrainee + "<br />";
                                        return Json(new AjaxResponseViewModel()
                                        {
                                            message = string.Format(Messege.WARNING_MIN_ASSIGNTRAINEE, courseName, result, mintrainee, "<br>"),
                                            result = true
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                if (math2 > maxtrainee)
                                {
                                    result = "<br />&nbsp;:&nbsp;<font color='red'>" + math2 + "&nbsp;</font>/&nbsp;" + maxtrainee + "<br />";
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        message = string.Format(Messege.WARNING_MAX_ASSIGNTRAINEE, courseName, result, maxtrainee, "<br>"),
                                        result = true
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;

                        case 3:
                            var check3 = false;
                            int itab3 = 0;
                            if (traineeLms == null || !traineeLms.Any())
                            {
                                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/WarningAssignTranee/Tab3", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                                return Json(new AjaxResponseViewModel()
                                {
                                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (var idform in traineeLms)
                                {
                                    if (idform != "0")
                                    {
                                        var traineeId = int.Parse(idform);
                                        var courseDetails = course.Course_Detail;
                                        if (courseDetails.Any())
                                        {
                                            foreach (var courseDetail in courseDetails)
                                            {
                                                check3 = CheckAssignTrainee(courseDetail.dtm_time_from?.ToShortDateString(), courseDetail.dtm_time_to?.ToShortDateString(), courseDetail.time_from, courseDetail.time_to, traineeId);
                                                if (check3)
                                                {
                                                    var model = _repoEmployeeService.GetById(traineeId);
                                                    if (model != null)
                                                        //fullName = model.str_Staff_Id + " - " + model.FirstName.Trim() + " " + model.LastName.Trim();
                                                        fullName = model.str_Staff_Id + " - " + ReturnDisplayLanguage(model.FirstName, model.LastName);
                                                    dateCourseDetail = courseDetail.dtm_time_from?.ToShortDateString() + " - " +
                                                                        courseDetail.dtm_time_to?.ToShortDateString();
                                                    timeCourseDetail = courseDetail.time_from + " - " + courseDetail.time_to;
                                                }
                                            }
                                        }
                                        itab3++;
                                    }
                                }
                            }
                            if (check3)
                            {
                                return Json(new AjaxResponseViewModel()
                                {
                                    type = (int)UtilConstants.TypeCheck.Checked,
                                    message = string.Format(Messege.CHECK_ASSIGNTRAINEE, fullName, dateCourseDetail, timeCourseDetail, "<br />"),
                                    result = false
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var traineeformLms = itab3;
                                var math3 = traineeformLms + membersResult;
                                if (CheckSiteConfig(UtilConstants.KEY_MIN_TRAINEE))
                                {
                                    if (math3 < mintrainee)
                                    {
                                        result = "<br />&nbsp;:&nbsp;<font color='red'>" + math3 + "&nbsp;</font>/&nbsp;" + mintrainee + "<br />";
                                        return Json(new AjaxResponseViewModel()
                                        {
                                            message = string.Format(Messege.WARNING_MIN_ASSIGNTRAINEE, courseName, result, mintrainee, "<br>"),
                                            result = true
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                if (math3 > maxtrainee)
                                {
                                    result = "<br />&nbsp;:&nbsp;<font color='red'>" + math3 + "&nbsp;</font>/&nbsp;" + maxtrainee + "<br />";
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        message = string.Format(Messege.WARNING_MAX_ASSIGNTRAINEE, courseName, result, maxtrainee, "<br>"),
                                        result = true
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;

                    }
                }


            }
            LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/WarningAssignTranee", Messege.ISVALID_DATA);
            return Json(new AjaxResponseViewModel()
            {
                message = Messege.ISVALID_DATA,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitAddAssignTrainee.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> SubmitAddAssignTrainee(FormCollection form)
        {

            var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
            var courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = !string.IsNullOrEmpty(form["id[]"]) ? form["id[]"].Split(new char[] { ',' }) : null;
            idtrainee = CMSUtils.SetObjectNull(idtrainee);

            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            if (courseId == -1)
            {

                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }
            var course = CourseService.GetById(courseId);
            if (course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve))
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.VALIDATION_STEP_ASSIGNTRAINEE_APPROVE,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var getSubjectInCourse = CourseDetailService.Get(a => a.CourseId == courseId)
                                .Select(p => p.Id)
                                .Distinct().ToArray();

                for (int i = 0; i < getSubjectInCourse.Length; i++)
                {
                    for (int y = 0; i < idtrainee.Length; i++)
                    {
                        if (!CMSUtils.IsNull(idtrainee[y]))
                        {
                            var id = int.Parse(idtrainee[y].ToString());
                            var courseDetail = CourseDetailService.GetById(int.Parse(getSubjectInCourse[i].ToString()));
                            var trainee = _repoEmployeeService.GetById(id);

                            var courseProgram = CourseService.GetById(courseDetail.CourseId);

                            var removeTrainee = new TMS_Course_Member() { Member_Id = id };
                            CourseService.Insert(removeTrainee, courseDetail.Id, courseId, UtilConstants.ApproveType.AssignedTrainee, submitType);

                            //var lstCourseMember = CourseService.GetListCourseMemberByMember(id);
                            //if (CheckUserEnrol_Tho(lstCourseMember))
                            //{
                            //    var lstSubjectID = CourseService.GetListSubjectDetailIdByCourseId((int)courseDetail.CourseId);
                            //    var lstApprove = new List<ApprovePortal>();
                            //    if (lstSubjectID.Count > 0)
                            //    {
                            //        foreach (var subId in lstSubjectID)
                            //        {
                            //            var subjectDetail = _repoSubject.GetSubjectDetailById(subId);
                            //            var approve = new ApprovePortal();
                            //            approve.Username = trainee.str_Staff_Id;
                            //            approve.CourseCode = courseProgram.Code;
                            //            approve.SubjectCode = subjectDetail.Code;

                            //            lstApprove.Add(approve);
                            //        }
                            //    }

                            //    ApprovedForPortal_Tho(lstApprove, Session["auth_token"].ToString(), false);

                            //}
                        }
                        y++;
                    }
                }

                if (course != null)
                {
                    var processStep = ProcessStep((int)UtilConstants.ApproveType.AssignedTrainee);
                    if (!processStep)
                    {
                        Modify_TMS(false, course, (int)UtilConstants.ApproveType.AssignedTrainee, (int)UtilConstants.EStatus.Approve, UtilConstants.ActionType.Approve);
                        await Task.Run(() =>
                        {

                            var checkCourseAssignTrainee =
                            ConfigService.GetScheduleByKey((int)UtilConstants.KeySend.CourseAssignTrainee);
                            //var checkSENT_EMAIL = GetByKey("SENT_EMAIL_PROCESS");//CheckSiteConfig(UtilConstants.KEY_SENT_EMAIL_PROCESS);
                            if (checkCourseAssignTrainee != null)
                            {
                                if (idtrainee != null)
                                {
                                    var y = 0;
                                    foreach (var arr_c in idtrainee)
                                    {
                                        if (!CMSUtils.IsNull(idtrainee[y]))
                                        {
                                            int id = int.Parse(idtrainee[y].ToString());
                                            var trainee = EmployeeService.GetById(id);
                                            Sent_Email_TMS(null, trainee, null, course, null, null, (int)UtilConstants.ActionTypeSentmail.AssignTrainee);
                                        }
                                        y++;
                                    }
                                }

                            }
                            var callLmsHistory = CallServices(UtilConstants.CRON_TRAINEE_HISTORY);
                            if (!callLmsHistory)
                            {
                                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign/Call_CRON_TRAINEE_HISTORY", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                                //return Json(new AjaxResponseViewModel()
                                //{
                                //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                                //    result = false
                                //}, JsonRequestBehavior.AllowGet);
                            }
                            var callLmsAssignTrainee = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                            if (!callLmsAssignTrainee)
                            {
                                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign/Call_CRON_ASSIGN_TRAINEE", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                                //return Json(new AjaxResponseViewModel()
                                //{
                                //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                                //    result = false
                                //}, JsonRequestBehavior.AllowGet);
                            }
                        });
                    }

                    var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
                    var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                    var result = string.Empty;
                    var data = string.Empty;

                    var courseDetail = course.Course_Detail;
                    if (courseDetail.Any())
                    {
                        result += "<ul>";
                        foreach (var item in courseDetail.Where(a => a.IsDeleted == false))
                        {
                            var count = item.TMS_Course_Member.Count(a => a.IsDelete == false && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
                            result += "<li>" + item.SubjectDetail.Name + "&nbsp;:&nbsp;<font " + (count > maxtrainee ? "color='red'" : "") + ">" + count + "&nbsp;</font>/&nbsp;" + maxtrainee + "</li>";
                        }
                        result += "</ul>";
                        var countResult = course.Course_Result_Final.Count(a => a.IsDeleted == false);
                        data += "&nbsp;:&nbsp;<font " + (countResult > maxtrainee ? "color='red'" : "") + " >&nbsp;" + countResult + "&nbsp;</font>/&nbsp;" + maxtrainee;
                    }



                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                        data = data,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitLmsAssign", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new AjaxResponseViewModel()
            {
                message = Messege.UNSUCCESS_ASSIGNTRAINEE,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitAddAssignTrainee2_.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SubmitAddAssignTrainee2_(FormCollection form)
        {

            //submit process
            var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
            int courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = form["id2[]"] != null ? form.GetValues("id2[]") : null;
            object[] remarkObject = form["remark[]"] != null ? form.GetValues("remark[]") : null;
            IList<RemarkTrainee> LtsRemarkTrainee = new List<RemarkTrainee>();
            if (!string.IsNullOrEmpty(form["RemarkTrainee"]))
            {

                string RemarkTrainee = form["RemarkTrainee"];
                string strCauHinh = string.Concat("[", RemarkTrainee, "]");
                LtsRemarkTrainee = new JavaScriptSerializer().Deserialize<IList<RemarkTrainee>>(strCauHinh);
            }

            //remarkObject = remarkObject.Where(c => c != "").ToArray();
            idtrainee = CMSUtils.SetObjectNull(idtrainee);
            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            if (courseId == -1)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }

            try
            {
                var course = CourseService.GetById(courseId);
                if (course != null)
                {
                    //var processStep = ProcessStep((int)UtilConstants.ApproveType.AssignedTrainee);
                    var courseDetail = course.Course_Detail.Where(a => a.IsDeleted != true).Select(p => p.Id).Distinct();
                    //var getSubjectInCourse = courseDetail.ToArray();

                    CourseService.Insert_Custom_new(courseId, courseDetail, idtrainee, LtsRemarkTrainee, 0);

                    var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                    var result = string.Empty;
                    var data = string.Empty;

                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                        data = data,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS_ASSIGNTRAINEE,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The SubmitAddAssignTrainee2.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SubmitAddAssignTrainee2(FormCollection form)
        {

            //submit process
            var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
            int courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = form["id2[]"] != null ? form.GetValues("id2[]") : null;
            object[] remarkObject = form["remark[]"] != null ? form.GetValues("remark[]") : null;
            IList<RemarkTrainee> LtsRemarkTrainee = new List<RemarkTrainee>();
            if (!string.IsNullOrEmpty(form["RemarkTrainee"]))
            {

                string RemarkTrainee = form["RemarkTrainee"];
                string strCauHinh = string.Concat("[", RemarkTrainee, "]");
                LtsRemarkTrainee = new JavaScriptSerializer().Deserialize<IList<RemarkTrainee>>(strCauHinh);
            }

            //remarkObject = remarkObject.Where(c => c != "").ToArray();
            idtrainee = CMSUtils.SetObjectNull(idtrainee);
            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            if (courseId == -1)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }

            try
            {
                var course = CourseService.GetById(courseId);
                if (course != null)
                {
                    var courseDetail = course.Course_Detail.Where(a => a.IsDeleted != true);

                    foreach (var item in courseDetail)
                    {
                        var entity_ = item.TMS_Course_Member;
                        var y = 0;
                        foreach (var arr_c in idtrainee)
                        {
                            var id = Convert.ToInt32(idtrainee[y]);
                            string remark = LtsRemarkTrainee?.FirstOrDefault(a => a.idTrainee == id)?.remarkTrainee;
                            var entity = entity_.FirstOrDefault(a => a.Member_Id == id && a.Course_Details_Id == item.Id);
                            if (entity == null)
                            {
                                Dictionary<string, object> dic = new Dictionary<string, object>();
                                dic.Add("Member_Id", id);
                                dic.Add("Course_Details_Id", item.Id);
                                dic.Add("DeleteApprove", 0);
                                dic.Add("IsDelete", 0);
                                dic.Add("IsActive", 1);
                                dic.Add("AssignBy", CurrentUser.USER_ID + "");
                                dic.Add("Status", 1); // chưa gửi request phê duyệt
                                dic.Add("LmsStatus", 99);// chưa approval
                                if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                {

                                }
                            }
                            else
                            {
                                Dictionary<string, object> dic = new Dictionary<string, object>();
                                dic.Add("DeleteApprove", 0);
                                dic.Add("IsDelete", 0);
                                dic.Add("IsActive", 1);
                                dic.Add("AssignBy", CurrentUser.USER_ID + "");
                                dic.Add("Status", 1); // chưa gửi request phê duyệt
                                dic.Add("LmsStatus", 99);// chưa approval
                                if (CMSUtils.UpdateDataSQLNoLog("Id", entity.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                {

                                }
                            }
                            if (!string.IsNullOrEmpty(remark))
                            {
                                TMS_Course_Member_Remark entity_remark = CourseMemberService.GetRemark(a => a.TraineeId == id && a.CourseId == course.Id).FirstOrDefault();
                                if (entity_remark == null)
                                {
                                    Dictionary<string, object> dic = new Dictionary<string, object>();
                                    dic.Add("TraineeId", id);
                                    dic.Add("CourseId", course.Id);
                                    dic.Add("remark", remark);
                                    if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member_Remark", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                                else
                                {
                                    Dictionary<string, object> dic = new Dictionary<string, object>();
                                    dic.Add("remark", remark);
                                    if (CMSUtils.UpdateDataSQLNoLog("Id", entity_remark.Id + "", "TMS_Course_Member_Remark", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                            }
                            y++;
                        }
                    }
                    var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                    var result = string.Empty;
                    var data = string.Empty;

                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                        data = data,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS_ASSIGNTRAINEE,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetQuantityAssignTrainee.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetQuantityAssignTrainee(int courseId)
        {
            var course = CourseService.GetById(courseId);

            if (course != null)
            {
                var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
                var courseName = course.Name;
                var result = string.Empty;
                var data = string.Empty;

                var courseDetail = course.Course_Detail;
                if (courseDetail.Any())
                {
                    result += "<ul>";
                    foreach (var item in courseDetail.Where(a => a.IsDeleted == false))
                    {
                        var count = item.TMS_Course_Member.Count(a => a.IsDelete == false);
                        result += "<li>" + item.SubjectDetail.Name + "&nbsp;:&nbsp;<font color='red'>" + count + "&nbsp;</font>/&nbsp;" + maxtrainee + "</li>";
                    }
                    result += "</ul>";
                    data += "&nbsp;:&nbsp;<font color='red'>" + courseDetail.Where(a => a.IsDeleted == false).FirstOrDefault().TMS_Course_Member.Count(a => a.IsDelete == false) + "</font>/" + maxtrainee;
                }
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                    data = data,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel()
            {
                message = string.Empty,
                data = string.Empty,
                result = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitRemoveAssignTrainee_.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> SubmitRemoveAssignTrainee_(FormCollection form)
        {
            int courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = !string.IsNullOrEmpty(form["idAssign[]"]) ? form["idAssign[]"].Split(new char[] { ',' }) : null;
            idtrainee = CMSUtils.SetObjectNull(idtrainee);
            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitRemoveAssignTrainee", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            if (courseId == -1)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }
            var course = CourseService.GetById(courseId);
            if (course == null)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitRemoveAssignTrainee", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.ISVALID_DATA,
                    result = false
                });
            }

            //if (course.TMS_APPROVES.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve))
            //{
            //    return Json(new AjaxResponseViewModel()
            //    {
            //        message = Messege.VALIDATION_STEP_ASSIGNTRAINEE_APPROVE,
            //        result = false
            //    }, JsonRequestBehavior.AllowGet);
            //}
            var courseDetails = course.Course_Detail;
            foreach (var detail in courseDetails)
            {
                var check = detail.Course_Result.Any(a =>
                    !string.IsNullOrEmpty(a.Result));
                if (check)
                {
                    var name = detail.SubjectDetail.Name;
                    return Json(new AjaxResponseViewModel { message = string.Format(Messege.WARNING_REMOVE_TRAINEE, "<font color='red'>" + name + "</font>"), result = false }, JsonRequestBehavior.AllowGet);
                }
            }
            var subjectInCourse = CourseDetailService.Get(a => a.CourseId == courseId)
                .Select(p => p.Id)
                .Distinct().ToArray();

            for (int i = 0; i < subjectInCourse.Length; i++)
            {

                var y = 0;
                foreach (var arr_c in idtrainee)
                {
                    if (!CMSUtils.IsNull(idtrainee[y]))
                    {
                        int id = int.Parse(idtrainee[y].ToString());
                        var removeTrainee = new TMS_Course_Member() { Member_Id = id };
                        //CourseMemberService.Get(a => a.Member_Id == id && a.Course_Details_Id == course).FirstOrDefault();
                        CourseService.Delete(removeTrainee, courseId);
                    }
                    y++;
                }

            }
            var countTrainee = course.Course_Result_Final.Count(a => a.IsDeleted == false && a.courseid == courseId);
            if (countTrainee == 0)
            {
                _repoTmsApproves.UpdateApproves(courseId);
            }
            var processStep = ProcessStep((int)UtilConstants.ApproveType.AssignedTrainee);
            if (!processStep)
            {
                Modify_TMS(false, course, (int)UtilConstants.ApproveType.AssignedTrainee, (int)UtilConstants.EStatus.Approve, UtilConstants.ActionType.Approve);
                await Task.Run(() =>
                {
                    var callLmsHistory = CallServices(UtilConstants.CRON_TRAINEE_HISTORY);
                    if (!callLmsHistory)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2/Call_CRON_TRAINEE_HISTORY", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    var callLmsAssignTrainee = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                    if (!callLmsAssignTrainee)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitAddAssignTrainee2/Call_CRON_ASSIGN_TRAINEE", Messege.SUCCESS + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = Messege.SUCCESS + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                });

            }

            var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
            var courseName = course.Code.Trim() + " - " + course.Name.Trim();
            var result = string.Empty;
            var data = string.Empty;

            var courseDetail = course.Course_Detail;
            if (courseDetail.Any())
            {
                result += "<ul>";
                foreach (var item in courseDetail.Where(a => a.IsDeleted == false))
                {
                    var count = item.TMS_Course_Member.Count(a => a.IsDelete == false && (a.Status == null || a.Status == (int)UtilConstants.ApiStatus.Synchronize));
                    result += "<li>" + item.SubjectDetail.Name + "&nbsp;:&nbsp;<font " +
                              (count > maxtrainee ? "color='red'" : "") + ">" + count +
                              "&nbsp;</font>/&nbsp;" + maxtrainee + "</li>";
                }
                result += "</ul>";
                var countResult = course.Course_Result_Final.Count(a => a.IsDeleted == false);
                data += "&nbsp;:&nbsp;<font " + (countResult > maxtrainee ? "color='red'" : "") + " >&nbsp;" +
                        countResult + "&nbsp;</font>/&nbsp;" + maxtrainee;
            }




            return Json(new AjaxResponseViewModel()
            {
                message = string.Format(Messege.REMOVE_TRAINEE, courseName, result),
                data = data,
                result = true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitRemoveAssignTrainee.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SubmitRemoveAssignTrainee(FormCollection form)
        {
            int courseId = string.IsNullOrEmpty(form["courseId"]) ? -1 : Convert.ToInt32(form["courseId"]);
            object[] idtrainee = !string.IsNullOrEmpty(form["idAssign[]"]) ? form["idAssign[]"].Split(new char[] { ',' }) : null;
            idtrainee = CMSUtils.SetObjectNull(idtrainee);
            if (idtrainee == null || !idtrainee.Any())
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitRemoveAssignTrainee", Messege.WARNING_CHECK_TRAINEE_ASSIGN);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_CHECK_TRAINEE_ASSIGN,
                    result = false
                });
            }
            if (courseId == -1)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });
            }
            var course = CourseService.GetById(courseId);
            if (course == null)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitRemoveAssignTrainee", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.ISVALID_DATA,
                    result = false
                });
            }


            var courseDetails = course.Course_Detail.Where(a => a.IsDeleted != true);

            foreach (var detail in courseDetails)
            {
                var check = detail.Course_Result.Any(a =>
                    !string.IsNullOrEmpty(a.Result));
                if (check)
                {
                    var name = detail.SubjectDetail.Name;
                    return Json(new AjaxResponseViewModel { message = string.Format(Messege.WARNING_REMOVE_TRAINEE, "<font color='red'>" + name + "</font>"), result = false }, JsonRequestBehavior.AllowGet);
                }
            }
            var list_final = course.Course_Result_Final;
            foreach (var item in courseDetails)
            {
                var entity_ = item.TMS_Course_Member;
                var y = 0;
                foreach (var arr_c in idtrainee)
                {
                    if (!CMSUtils.IsNull(idtrainee[y]))
                    {
                        var id = Convert.ToInt32(idtrainee[y]);
                        var entity = entity_.FirstOrDefault(a => a.Member_Id == id && a.Course_Details_Id == item.Id);
                        if (entity != null)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("DeleteApprove",1);
                            //dic.Add("IsDelete", 1);
                            //dic.Add("IsActive", 0);
                            //dic.Add("LmsStatus", 99);// chưa approval
                            if (CMSUtils.UpdateDataSQLNoLog("Id", entity.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                        }
                        //var final = list_final.FirstOrDefault(a => a.traineeid == id && a.courseid == courseId);
                        //if (final != null)
                        //{
                        //    Dictionary<string, object> dic_final = new Dictionary<string, object>();
                        //    dic_final.Add("IsDeleted", 1);
                        //    dic_final.Add("DeletedBy", CurrentUser.USER_ID + "");
                        //    dic_final.Add("DeletedDate", DateTime.Now);
                        //    dic_final.Add("LmsStatus", 99);
                        //    dic_final.Add("MemberStatus", (int)UtilConstants.CourseResultFinalStatus.Removed);
                        //    if (CMSUtils.UpdateDataSQLNoLog("id", final.id + "", "Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                        //    {

                        //    }
                        //}
                        //TMS_Course_Member_Remark entity_remark = CourseMemberService.GetRemark(a => a.TraineeId == id && a.CourseId == course.Id).FirstOrDefault();
                        //if (entity_remark != null)
                        //{
                        //    CMSUtils.DeleteDataSQLNoLog("Id", entity_remark.Id + "", "TMS_Course_Member_Remark");
                        //}
                    }
                    y++;
                }
            }
            var countTrainee = course.Course_Result_Final.Count(a => a.IsDeleted == false && a.courseid == courseId);
            if (countTrainee == 0)
            {
                var approves = course.TMS_APPROVES;
                if (approves.Count() > 0)
                {
                    var assignTraineeToEnd = approves.Where(a => a.int_Type != (int)UtilConstants.ApproveType.Course);
                    if (assignTraineeToEnd.Any())
                    {
                        foreach (var approve in assignTraineeToEnd)
                        {
                            approve.int_id_status = (int)UtilConstants.EStatus.EmptyTrainee;
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("int_id_status", (int)UtilConstants.EStatus.EmptyTrainee);
                            if (CMSUtils.UpdateDataSQLNoLog("id", approve.id + "", "TMS_APPROVES", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                        }
                    }
                }
            }
            var courseName = course.Code.Trim() + " - " + course.Name.Trim();
            var result = string.Empty;
            var data = string.Empty;


            return Json(new AjaxResponseViewModel()
            {
                message = string.Format(Messege.REMOVE_TRAINEE, courseName, result),
                data = data,
                result = true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The SubmitSetParticipateSubject.
        /// </summary>
        /// <param name="isParticipate">The isParticipate<see cref="int"/>.</param>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="Task{ActionResult}"/>.</returns>
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateSubject(int isParticipate, string id, FormCollection form)
        {
            try
            {
                var subjectId = int.Parse(id);
                var removeTrainee = CourseMemberService.GetById(subjectId);
                if (removeTrainee == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateSubject", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var type = (int)UtilConstants.StatusTraineeHistory.Missing;
                if (isParticipate == 1)
                {
                    removeTrainee.IsActive = false;
                }
                else
                {
                    removeTrainee.IsActive = true;
                    type = (int)UtilConstants.StatusTraineeHistory.Trainning;
                }
                removeTrainee.LmsStatus = StatusModify;
                CourseMemberService.Update(removeTrainee);

                UpdateStatusTraineeHistoryItem(removeTrainee.Member_Id.Value, removeTrainee.Course_Details_Id.Value, type);
                //var fullName = removeTrainee.Trainee.FirstName.Trim() + " " + removeTrainee.Trainee.LastName.Trim();
                var fullName = ReturnDisplayLanguage(removeTrainee.Trainee.FirstName, removeTrainee.Trainee.LastName);
                await Task.Run(() =>
                {



                    var callLms = CallServices(UtilConstants.CRON_TRAINEE_HISTORY);
                    if (!callLms)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateSubject/CRON_TRAINEE_HISTORY", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName));
                        if (isParticipate == 1)
                        {
                            //return Json(new AjaxResponseViewModel()
                            //{

                            //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName),
                            //    result = false
                            //});
                        }
                        else
                        {
                            //return Json(new AjaxResponseViewModel()
                            //{

                            //    message = string.Format(Messege.ADD_SUCCESSFULLY_BUT_ERROR_LMS, fullName),
                            //    result = true
                            //});
                        }

                    }
                    var callLmsAssign = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
                    if (!callLmsAssign)
                    {
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateSubject/CRON_ASSIGN_TRAINEE", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName),
                        //    result = false
                        //});
                    }
                });
                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, fullName), result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateSubject", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);


            }
        }

        /// <summary>
        /// The UpdateStatusTraineeHistoryItem.
        /// </summary>
        /// <param name="traineeId">The traineeId<see cref="int"/>.</param>
        /// <param name="courseDetaiId">The courseDetaiId<see cref="int"/>.</param>
        /// <param name="type">The type<see cref="int"/>.</param>
        private void UpdateStatusTraineeHistoryItem(int traineeId, int courseDetaiId, int type)
        {
            var subjectId = CourseDetailService.GetById(courseDetaiId).SubjectDetailId;
            var entity =
                CourseService.GetHistoryItems(a => a.SubjectId == subjectId && a.TraineeHistory.Trainee_Id == traineeId).OrderByDescending(a => a.Id).FirstOrDefault();
            if (entity == null) return;
            entity.Status = type;
            entity.TraineeHistory.LmsStatus = StatusModify;
            CourseService.Update(entity);
        }

        /// <summary>
        /// The SubmitRequest.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <param name="type">The type<see cref="int"/>.</param>
        /// <param name="note">The note<see cref="string"/>.</param>
        /// <param name="courseDetailId">The courseDetailId<see cref="int?"/>.</param>
        /// <param name="remarkassign">The remarkassign<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SubmitRequest(int courseId, int type, string note, int? courseDetailId = -1, string remarkassign = "")
        {
            var course = CourseService.GetById(courseId);
            if (course == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.VALIDATION_COURSE_FILTER, Resource.lblCourse),
                    result = false
                });


            try
            {

                switch (type)
                {
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        //if (!course.Course_Result_Final.Any(a => a.IsDeleted == false && a.MemberStatus == (int)UtilConstants.BoolEnum.Yes))
                        //    return Json(new AjaxResponseViewModel()
                        //    {
                        //        message = string.Format(Messege.VALIDATION_COURSE_HAS_TRAINEE, Resource.lblCourse, course.Code + " - " + course.Name),
                        //        result = false
                        //    });
                        var list_member = new List<int?>();
                        if (course.Course_Detail.Any())
                        {
                            foreach (var item in course.Course_Detail)
                            {

                                foreach (var item_ in item.TMS_Course_Member.Where(a=>a.IsDelete != true && a.Course_Details_Id == item.Id))
                                {
                                    Dictionary<string, object> dic = new Dictionary<string, object>();
                                    if (item_.Status == (int)UtilConstants.APIAssign.Pending)
                                    {
                                        dic.Add("Status", 2); // chưa gửi request phê duyệt
                                        dic.Add("LmsStatus", 99);// chưa approval
                                    }
                                    else
                                    {
                                        dic.Add("LmsStatus", 99);// chưa approval
                                    }
                                    if (CMSUtils.UpdateDataSQLNoLog("Id", item_.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                    {
                                        list_member.Add(item_.Member_Id);
                                    }

                                }
                                //if (item.TMS_Course_Member.Any(a => a.Status.HasValue && a.Status == (int)UtilConstants.APIAssign.Pending))
                                //{
                                //    foreach (var item_ in item.TMS_Course_Member.Where(a => a.IsDelete != true && a.Status.HasValue && a.Status == (int)UtilConstants.APIAssign.Pending && a.Course_Details_Id == item.Id))
                                //    {
                                //        Dictionary<string, object> dic = new Dictionary<string, object>();
                                //        dic.Add("Status", 0); // chưa gửi request phê duyệt
                                //        dic.Add("LmsStatus", 99);// chưa approval
                                //        if (CMSUtils.UpdateDataSQLNoLog("Id", item_.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                //        {
                                //            list_member.Add(item_.Member_Id);
                                //        }
                                //    }
                                //}
                                //if (item.TMS_Course_Member.Any(a => a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved) && a.Course_Details_Id == item.Id))
                                //{
                                //    foreach (var item_ in item.TMS_Course_Member.Where(a => a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved) && a.Course_Details_Id == item.Id))
                                //    {

                                //        Dictionary<string, object> dic = new Dictionary<string, object>();
                                //        dic.Add("LmsStatus", 99);// chưa approval
                                //        if (CMSUtils.UpdateDataSQLNoLog("Id", item_.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                //        {
                                //            list_member.Add(item_.Member_Id);
                                //        }

                                //    }
                                //}
                                //else if(item.TMS_Course_Member.Any(a=> a.Course_Details_Id == item.Id && a.IsDelete != true && a.DeleteApprove == 1))
                                //{
                                //    foreach (var item_ in item.TMS_Course_Member.Where(a => (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved) && a.Course_Details_Id == item.Id))
                                //    {

                                //        Dictionary<string, object> dic = new Dictionary<string, object>();
                                //        dic.Add("LmsStatus", 99);// chưa approval
                                //        if (CMSUtils.UpdateDataSQLNoLog("Id", item_.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                //        {
                                //            list_member.Add(item_.Member_Id);
                                //        }
                                //    }
                                //}
                            }
                            //RegistTraineeToCourse 
                            var list_final = course.Course_Result_Final;
                            list_member = list_member.Distinct().ToList();
                            foreach (var item in list_member)
                            {
                                var entity = list_final.Where(a => a.traineeid == item && a.courseid == courseId);
                                if (!entity.Any())
                                {
                                    Dictionary<string, object> dic_final = new Dictionary<string, object>();
                                    dic_final.Add("traineeid", item);
                                    dic_final.Add("courseid", courseId);
                                    dic_final.Add("createby", CurrentUser?.USER_ID + "");
                                    dic_final.Add("IsDeleted", 0);
                                    dic_final.Add("createday", DateTime.Now);
                                    dic_final.Add("LmsStatus", 99);
                                    dic_final.Add("MemberStatus", (int)UtilConstants.CourseResultFinalStatus.Pending);
                                    if (CMSUtils.InsertDataSQLNoLog("Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                                else
                                {
                                    Dictionary<string, object> dic_final = new Dictionary<string, object>();
                                    dic_final.Add("IsDeleted", 0);
                                    dic_final.Add("LmsStatus", 99);
                                    dic_final.Add("MemberStatus", (int)UtilConstants.CourseResultFinalStatus.Pending);
                                    if (CMSUtils.UpdateDataSQLNoLog("Id", entity.FirstOrDefault().id + "", "Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                            }
                        }



                        //var check = false;
                        //if (course.Course_Detail.Any())
                        //{
                        //    foreach (var item in course.Course_Detail)
                        //    {
                        //        if (!item.TMS_Course_Member.Any(a => a.IsDelete == false && a.IsActive))
                        //        {
                        //            check = true;
                        //        }
                        //    }
                        //}
                        //if (check)
                        //{
                        //    return Json(new AjaxResponseViewModel()
                        //    {
                        //        message = string.Format(Messege.VALIDATION_COURSE_HAS_TRAINEE, Resource.lblCourse, course.Code + " - " + course.Name),
                        //        result = false
                        //    });
                        //}
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        var courseDetail = _repoCourseDetail.GetById(courseDetailId);
                        if (!courseDetail.Course_Result.Any(a => a.IsDelete == false))
                        {
                            return Json(new AjaxResponseViewModel()
                            {
                                message = string.Format(Messege.VALIDATION_SUBJECT_HAS_RESULT, Resource.lblSubject, courseDetail.SubjectDetail.Code + " - " + courseDetail.SubjectDetail.Name),
                                result = false
                            });
                        }
                        break;

                }
                Modify_TMS(false, course, (int)type, (int)UtilConstants.EStatus.Pending, UtilConstants.ActionType.Request, note, courseDetailId, remarkassign);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.WARNING_SENT_REQUEST_SUCCESS,
                    result = true
                });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitRequest", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                });
            }
        }

        /// <summary>
        /// The SubmitTrinhKi.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SubmitTrinhKi(FormCollection form)
        {
            var courseId = string.IsNullOrEmpty(form["courseId"]) ? "-1" : form["courseId"].Trim(',');
            var idcourse = Int32.Parse(courseId.ToString());
            try
            {
                _repoTmsApproves.UpdateApprove(idcourse, null, UtilConstants.ApproveType.AssignedTrainee,
                UtilConstants.EStatus.Pending);
            }
            catch (Exception)
            {
                return Json(CMSUtils.alert("fail", Messege.FAIL));
            }
            var type = (int)UtilConstants.ApproveType.AssignedTrainee;
            var course_ = CourseService.GetById(idcourse);
            if (course_ != null)
            {
                //var hodRoleName = ConfigService.UtilConstants.HOD");
                int To = _repoUser.GetAll(null, CurrentUser.PermissionIds).FirstOrDefault().ID;//first user (administrator)
                var data = _repoTmsApproves.Get(a => a.int_Course_id == idcourse && a.int_Type == type).FirstOrDefault();
                var pattern = UtilConstants.NotificationContent.Request_AssignTrainee;
                var stringformat = "(" + course_?.Code + ") " + course_?.Name;
                SendNotification((int)UtilConstants.NotificationType.AutoProcess, type, data?.id, To, DateTime.Now, UtilConstants.NotificationTemplate.Request_AssignTrainee,
                    string.Format(pattern, stringformat),
                    UtilConstants.NotificationTemplate.Request_AssignTrainee_VN,
                    string.Format(pattern, stringformat));


            }
            var str_log = Log(type, course_);
            var TMS_APPROVES_LOG_ = new TMS_APPROVES_LOG();
            TMS_APPROVES_LOG_.str_content = str_log;
            TMS_APPROVES_LOG_.dtm_create = DateTime.Now;
            TMS_APPROVES_LOG_.int_course_id = idcourse;
            TMS_APPROVES_LOG_.int_type = type;
            _repoTmsApproves.InsertLog(TMS_APPROVES_LOG_);
            return Json(new AjaxResponseViewModel { message = Messege.SUCCESS_COURSE, result = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ChangeDepartmentReturnJobtitle.
        /// </summary>
        /// <param name="id_department">The id_department<see cref="string"/>.</param>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpPost]
        public JsonResult ChangeDepartmentReturnJobtitle(string id_department, string id)
        {
            try
            {
                if (!CMSUtils.IsNull(id_department))
                {
                    int id_department_ = Int32.Parse(id_department);
                    int id_ = -1;
                    if (id != null)
                    {
                        id_ = Int32.Parse(id);
                    }
                    StringBuilder html = new StringBuilder();
                    var data = _repoJobTiltle.Get();
                    var datauser = EmployeeService.GetById(id_);
                    int jobtitleid = -1;
                    if (datauser != null)
                    {
                        jobtitleid = (int)datauser.Job_Title_id;
                    }
                    if (data != null)
                    {
                        //html.Append("<select name='Job_Title_id' id='ddl_JOBTITLE' class='form-control' data-placeholder='-- JOB TITLE --'> <option></option>");
                        foreach (var item in data)
                        {
                            if (jobtitleid == item.Id)
                            {
                                html.AppendFormat("<option value='{0}' selected>{1}</option>", item.Id, item?.Name);
                            }
                            else
                            {
                                html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item?.Name);
                            }

                        }
                        //html.Append("</select>");
                    }
                    return Json(new
                    {
                        htmlout = html.ToString()
                    });
                }
                return Json(new
                {
                    htmlout = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);

            }
        }

        /// <summary>
        /// The FilterAssign.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult FilterAssign(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var html = new StringBuilder();
            var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();
            var courseId = string.IsNullOrEmpty(form["hdcourseid"]) ? -1 : Convert.ToInt32(form["hdcourseid"].Trim());

            DateTime fromDateFrom;
            DateTime toDateFrom;
            DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out fromDateFrom);
            DateTime.TryParse(fSearchDateTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out toDateFrom);
            //DateTime.TryParse(fSearchDateFrom, out fromDateFrom);
            //DateTime.TryParse(fSearchDateTo, out toDateFrom);
            fromDateFrom = fromDateFrom != DateTime.MinValue ? fromDateFrom.Date : fromDateFrom;
            toDateFrom = toDateFrom != DateTime.MinValue ? toDateFrom.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : toDateFrom;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var data = CourseService.Get(a =>
                        (string.IsNullOrEmpty(fCode) && fromDateFrom == DateTime.MinValue && toDateFrom == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)) &&
                        (fromDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", fromDateFrom, a.StartDate) >= 0) &&
                        (toDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, toDateFrom) >= 0), true)
                        .OrderByDescending(p => p.Id);
            if (!data.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in data)
            {
                if (item.Id == courseId)
                {
                    html.AppendFormat("<option   value='{0}' selected>{1} - {2}</option>", item.Id, item.Code, item.Name);
                }
                else
                {
                    html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
                }

            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The FilterFinal.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult FilterFinal(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var html = new StringBuilder();
            var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();

            DateTime fromDateFrom;
            DateTime toDateFrom;

            DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out fromDateFrom);
            DateTime.TryParse(fSearchDateTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out toDateFrom);
            fromDateFrom = fromDateFrom != DateTime.MinValue ? fromDateFrom.Date : fromDateFrom;
            toDateFrom = toDateFrom != DateTime.MinValue ? toDateFrom.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : toDateFrom;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var data = CourseService.Get(a =>
                        (string.IsNullOrEmpty(fCode) && fromDateFrom == DateTime.MinValue && toDateFrom == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)) &&
                        (fromDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", fromDateFrom, a.StartDate) >= 0) &&
                        (toDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, toDateFrom) >= 0), true)
                        .OrderByDescending(p => p.Id);
            if (!data.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in data)
            {
                if (CourseService.checkapproval(item, new[] { (int)UtilConstants.ApproveType.CourseResult }))
                    html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FilterFinal_new(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var html = new StringBuilder();
            var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();

            DateTime fromDateFrom;
            DateTime toDateFrom;

            DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out fromDateFrom);
            DateTime.TryParse(fSearchDateTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out toDateFrom);
            fromDateFrom = fromDateFrom != DateTime.MinValue ? fromDateFrom.Date : fromDateFrom;
            toDateFrom = toDateFrom != DateTime.MinValue ? toDateFrom.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : toDateFrom;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var data = CourseService.Get(a =>
                        (string.IsNullOrEmpty(fCode) && fromDateFrom == DateTime.MinValue && toDateFrom == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)) &&
                        (fromDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", fromDateFrom, a.StartDate) >= 0) &&
                        (toDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, toDateFrom) >= 0), true)
                        .OrderByDescending(p => p.Id);
            if (!data.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in data)
            {
                if (CourseService.checkapproval(item, new[] { (int)UtilConstants.ApproveType.Course }))
                    html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// The Filtercourse.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Filtercourse(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var html = new StringBuilder();
            var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();

            DateTime fromDateFrom;
            DateTime toDateFrom;

            DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out fromDateFrom);
            DateTime.TryParse(fSearchDateTo, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out toDateFrom);
            fromDateFrom = fromDateFrom != DateTime.MinValue ? fromDateFrom.Date : fromDateFrom;
            toDateFrom = toDateFrom != DateTime.MinValue ? toDateFrom.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : toDateFrom;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var data = CourseService.Get(a =>
                        (string.IsNullOrEmpty(fCode) && fromDateFrom == DateTime.MinValue && toDateFrom == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)) &&
                        (fromDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", fromDateFrom, a.StartDate) >= 0) &&
                        (toDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, toDateFrom) >= 0), true)
                        .OrderByDescending(p => p.Id);

            if (!data.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in data)
            {
                html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The CheckStatusAssign.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CheckStatusAssign(int? courseId)
        {
            courseId = courseId ?? -1;
            //var processStep = ProcessStep((int)UtilConstants.ApproveType.AssignedTrainee);

            //var tmsApprove = _repoTmsApproves.Get(a => a.int_Course_id == courseId && a.Course.IsDeleted != true, null, (int)UtilConstants.EStatus.Approve).OrderByDescending(a => a.id).FirstOrDefault().int_Type;


            if (courseId != -1)
            {
                //var tmsApprove = _repoTmsApproves.Get(a => a.int_Course_id == courseId && a.Course.IsDeleted == false, null, (int)UtilConstants.EStatus.Approve).OrderByDescending(a => a.id).FirstOrDefault().int_Type;

                //var lastStep = tmsApprove ?? (int)UtilConstants.ApproveType.Course;

                //var stepRequirement = ProcessStepRequirement(lastStep, (int)UtilConstants.ApproveType.AssignedTrainee);

                //var statusCourse = _repoTmsApproves.Get(b => (/*b.int_id_status == (int)UtilConstants.EStatus.Pending ||*/ b.int_id_status == (int)UtilConstants.EStatus.Approve) && b.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && b.int_Course_id == courseId);
                var course = CourseService.GetById(courseId);
                //var statusCourse = course.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.CourseResult);

                if (course != null)// statusCourse.Count() == 0 && !stepRequirement)//if (!statusCourse && !stepRequirement)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = true,
                        data = "<a onclick='SubmitRequest()' class='btn btn-danger'><i class='fa fa-paper-plane' aria-hidden='true'></i> " + Resource.lblSubmit + "</a>"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new AjaxResponseViewModel()
            {
                result = false,
                data = "",
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The MCost.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MCost()
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }
            ViewBag.CourseList = new SelectList(CourseService.Get().OrderBy(m => m.Name).ToList(), "Id", "Name");
            ViewBag.CostList = new SelectList(_courseServiceCost.Get().OrderBy(m => m.str_Name).ToList(), "id", "str_Name");
            ViewBag.UnitList = new SelectList(_courseServiceCost.GetUnits().Where(a => a.module == "Cost").OrderBy(m => m.name).ToList(), "id", "name");

            return View();
        }

        /// <summary>
        /// The AjaxHandlerMListCost.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerMListCost(jQueryDataTableParamModel param)
        {
            try
            {
                int CourseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());

                var datacost = _courseServiceCost.GetCourseCost(a => a.course_id == CourseList);

                //var data_instructor = CourseDetailService.Get(a => a.Course_Id == CourseList).Select(a=>a.duration);


                IEnumerable<Course_Cost> models = datacost;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Cost, string> orderingFunction = (c
                                                          => sortColumnIndex == 0 ? c?.Course?.Name
                                                            : sortColumnIndex == 1 ? c?.CAT_COSTS?.str_Name
                                                            : sortColumnIndex == 2 ? c?.cost.ToString()
                                                          : c.id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                var filtered = (sortDirection == "asc") ? models.OrderBy(orderingFunction)
                                   : models.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                   c?.CAT_COSTS?.str_Name,
                                    c?.cost,
                                    "<a href='javascript:void(0)'><i class='fas fa-trash' aria-hidden='true'></i></a>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerMListCost", ex.Message);


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The Register.
        /// </summary>
        /// <param name="courseDetailid">The courseDetailid<see cref="string"/>.</param>
        /// <param name="staffid">The staffid<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Register(string courseDetailid, string staffid)
        {
            var subjectdetail = new Course_Detail();
            if (!string.IsNullOrEmpty(courseDetailid))
            {
                int id = int.Parse(courseDetailid);
                subjectdetail = CourseDetailService.GetById(id);
            }
            string traineeCode = "";
            traineeCode = staffid;

            ViewBag.staff = traineeCode;
            return View(subjectdetail);
        }

        /// <summary>
        /// The Register.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            var CourseMember = new TMS_Course_Member();
            string tCode = "";
            if (!string.IsNullOrEmpty(form["CourseDetailID"]) && !string.IsNullOrEmpty(form["staffCode"]))
            {
                int cDid = int.Parse(form["CourseDetailID"]);
                tCode = form["staffCode"];
                var check = EmployeeService.Get(a => a.str_Staff_Id == tCode);
                if (check.Any())
                {
                    var trainee = EmployeeService.Get(a => a.str_Staff_Id == tCode).FirstOrDefault();

                    if (
                        !trainee.TMS_Course_Member.Any(
                            a => a.Course_Details_Id == cDid && a.Member_Id == trainee.Id))
                    {
                        CourseMember.Member_Id = trainee.Id;
                        CourseMember.Course_Details_Id = cDid;
                        CourseMemberService.Insert(CourseMember);

                        TempData["success"] = Messege.SUCCESS;

                    }
                    else
                    {
                        TempData["fail"] = Messege.REGISTERED_COURSE; // "Bạn đã đăng ký khóa học này rồi.";
                    }
                }
                else
                {
                    TempData["fail"] = Messege.TRAINEE_EXIST; //"Học viên không tồn tại.";
                }

            }
            var subjectdetail = new Course_Detail();
            ViewBag.staff = tCode;
            return RedirectToAction("Register", new { courseDetailid = form["CourseDetailID"], staffid = form["staffCode"] });
        }

        /// <summary>
        /// The Attendance.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult Attendance()
        {
            var model = new CourseViewAttendance();
            model.Courseses = CourseService.Get()
                .OrderByDescending(a => a.Id)
                .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            return View(model);
        }

        /// <summary>
        /// The AjaxHandlerListTraineeOfSubject.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerListTraineeOfSubject(jQueryDataTableParamModel param)
        {
            try
            {
                int coursedetailId = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
                var selectDate = string.IsNullOrEmpty(Request.QueryString["selectdate"]) ? string.Empty : Request.QueryString["selectdate"].Trim();
                int totalDay = string.IsNullOrEmpty(Request.QueryString["totalDay"]) ? -1 : Convert.ToInt32(Request.QueryString["totalDay"].Trim());



                DateTime date;
                DateTime.TryParse(selectDate, out date);
                date = date != DateTime.MinValue ? date.Date : date;


                var dataMembers2 = CourseMemberService.Get(a => (a.Course_Details_Id == -1 || a.Course_Details_Id == coursedetailId)
                 && a.IsActive == true && date != DateTime.MinValue).ToList().Select(a => new TraineeAttendanceModel
                 {
                     Id = a.Course_Detail?.Course_Attendance?.FirstOrDefault(c => c.TraineeId == a.Member_Id && c.AttendedDate == date)?.Id ?? -1,
                     TraineeId = a.Trainee.Id,
                     CourseDetailId = a.Course_Details_Id ?? -1,
                     Code = a.Trainee.str_Staff_Id,
                     //FullName = a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim(),
                     FullName = ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName),
                     Percent = a.Course_Detail?.Course_Attendance?.Count(b => b.Type == (int)UtilConstants.Attendance.Present) != null ?
                     ((a.Course_Detail?.Course_Attendance?.Count(b => (b.Type == (int)UtilConstants.Attendance.Present || b.Type == (int)UtilConstants.Attendance.Late) && b.CourseDetailId == coursedetailId && b.TraineeId == a.Member_Id) * 100) / totalDay).ToString() : "",

                     Type =
                            a.Course_Detail?.Course_Attendance?.FirstOrDefault(c => c.TraineeId == a.Member_Id && c.AttendedDate == date)?.Type ?? -1,
                     Note = a.Course_Detail?.Course_Attendance?.FirstOrDefault(c => c.TraineeId == a.Member_Id && c.AttendedDate == date)?.Note ?? string.Empty
                 });


                //var courseAttendance =
                //  CourseService.GetAllTraineeAttendance(
                //      a => a.CourseDetailId == coursedetailId && (date == DateTime.MinValue || date == a.AttendedDate) 


                //      ).ToList().Select(a => new TraineeAttendanceModel()
                //      {
                //          Id = a.Id,
                //          TraineeId = a.TraineeId ?? 0,
                //          CourseDetailId = a.CourseDetailId ?? -1,
                //          Code = a.Trainee.str_Staff_Id,
                //          FullName = a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim(),
                //          Note = a.Note ?? string.Empty,
                //          Type = a.Type ?? -1
                //      });
                //IEnumerable<TraineeAttendanceModel> filtered;
                //if (courseAttendance.Any())
                //{
                //    filtered = courseAttendance;
                //}
                //else
                //{
                //    var dataMembers = CourseMemberService.Get(a => a.Course_Details_Id == coursedetailId && a.IsActive).ToList().Select(a => new TraineeAttendanceModel
                //    {
                //        Id = -1,
                //        TraineeId = a.Trainee.Id,
                //        CourseDetailId = a.Course_Details_Id ?? -1,
                //        Code = a.Trainee.str_Staff_Id,
                //        FullName = a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim(),
                //        Type = -1,
                //        Note = string.Empty
                //    });
                //    filtered = dataMembers;
                //}

                IEnumerable<TraineeAttendanceModel> filtered = dataMembers2;
                //, a => a.OrderByDescending(p => p.Id)
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TraineeAttendanceModel, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.Percent
                                                            : c.TraineeId.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                //2
                                                c?.Code,
                                                //3
                                                c?.FullName,
                                                //4
                                                c?.Percent + "%",
                                                //5
                                                 date.ToString("dd/MM/yyyy")
                                                 + "<input type='hidden' class='frmFilter' id='attenddate' name='attenddate' value='"+date.ToString("dd/MM/yyyy")+"' > ",
                                                 //checkbox 6
                                               "<div class='col-md-12' style='margin-top: -30px;'><div class='col-md-4'> <input class='radio frmFilter'  id='Type_" + c?.TraineeId+"_"+(int) UtilConstants.Attendance.Present+"' name='Type_"+c?.TraineeId+"' type='radio' value='"+(int) UtilConstants.Attendance.Present+"' " + (c?.Type == (int) UtilConstants.Attendance.Present ? "checked" : "") + " /> <label for='Type_"+c?.TraineeId+"_"+(int) UtilConstants.Attendance.Present+"'>"+ Resource.lblPresent+"</label></div>"  +

                                               " <div class='col-md-4'><input class='radio frmFilter' id='Type_"+c?.TraineeId+"_"+(int) UtilConstants.Attendance.Absent+"' name='Type_"+c?.TraineeId+"' type='radio' value='"+(int) UtilConstants.Attendance.Absent+"' " + (c?.Type == (int) UtilConstants.Attendance.Absent ? "checked" : "") + "/>  <label for='Type_"+c?.TraineeId+"_"+(int) UtilConstants.Attendance.Absent+"'>"+ Resource.lblAbsent+"</label></div>"  +

                                               " <div class='col-md-4'><input class='radio frmFilter'  id='Type_"+c?.TraineeId+"_"+(int) UtilConstants.Attendance.Late+"' name='Type_"+c?.TraineeId+"' type='radio' value='"+(int) UtilConstants.Attendance.Late+"' " + (c?.Type == (int) UtilConstants.Attendance.Late ? "checked" : "") + "/>  <label for='Type_"+c?.TraineeId+"_"+(int) UtilConstants.Attendance.Late+"'>"+ Resource.lblLate+"</label></div></div>" +


                                               "<input type='hidden' data-staffid='"+c?.Code+"' data-='"+c?.CourseDetailId+"' class='form-control frmFilter' name='Trainee_Id' value='"+c?.TraineeId+"'/>" +
                                              "<input type='hidden' data-staffid='"+c?.Code+"' data-='"+c?.CourseDetailId+"' class='form-control frmFilter' name='Course_Details_Id' value='"+c?.CourseDetailId+"'/>" +
                                              "<input type='hidden' data-staffid='"+c?.Code+"' data-='"+c?.CourseDetailId+"' class='form-control frmFilter' name='Id' value='"+c?.Id+"'/>",
                                               //7
                                                "<textarea class='form-control frmFilter' id='txtNote' name='txtNote' rows='3' cols='3'>" + c?.Note + "</textarea>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerListTraineeOfSubject", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                   JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ChangeCourseDetailReturnDate.
        /// </summary>
        /// <param name="courseDetailId">The courseDetailId<see cref="int?"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpGet]
        public JsonResult ChangeCourseDetailReturnDate(int? courseDetailId)
        {

            try
            {
                var html = new StringBuilder("<option value=''>--" + Resource.lblDate + "--</option>");
                var nullInstructor = 1;
                var count = 0;
                Expression<Func<Course_Detail, bool>> expression = a => a.SubjectDetail.IsDelete == false;
                if (courseDetailId != 0)
                {
                    expression =
                        a => a.Id == courseDetailId && a.SubjectDetail.IsDelete == false;
                }
                var data = CourseDetailService.Get(expression).Select(a => new { a.dtm_time_from, a.dtm_time_to }).FirstOrDefault();
                if (data != null)
                {
                    nullInstructor = 0;
                    for (DateTime d = (DateTime)data.dtm_time_from; d <= data.dtm_time_to; d = d.AddDays(1))
                    {

                        html.AppendFormat("<option value='{0}' " + (d.Date.ToShortDateString()) + ">{0}</option>", d.Date.ToShortDateString());
                        count++;
                    }
                }
                else
                {
                    nullInstructor = 1;
                }

                return Json(new
                {
                    totalDay = count.ToString(),
                    value_option = html.ToString(),
                    value_null = nullInstructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ChangeCourseDetailReturnDate", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The InsertAttendance.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult InsertAttendance(FormCollection form)
        {
            try
            {
                var user = GetUser();

                var traineeId = form.GetValues("Trainee_Id");

                var txtNote = form.GetValues("txtNote");

                var attenDate = form.GetValues("attenddate");
                var selectDate = form["selectDate"];
                var courseDetailId = string.IsNullOrEmpty(form["ddl_subject"])
                    ? -1
                    : Convert.ToInt32(form["ddl_subject"]);
                var i = 0;
                if (traineeId != null)
                {
                    foreach (var item in traineeId)
                    {
                        var appSettings = form.AllKeys.Where(k => k.StartsWith("Type_" + item)).ToDictionary(k => k, k => form[k]);
                        foreach (var key in appSettings)
                        {
                            var separators = new[] { "_", "," };
                            var words = key.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            var getTraineeId = int.Parse(words.GetValue(1).ToString());
                            var getType = key.Value != null ? int.Parse(key.Value) : (int)UtilConstants.Attendance.Undefined;

                            var parseDate = DateTime.Parse(selectDate);
                            var model =
                                CourseService.GetTraineeAttendance(z => z.TraineeId == getTraineeId && z.AttendedDate == parseDate && z.CourseDetailId == courseDetailId);
                            if (model != null)
                            {
                                model.Type = getType;
                                model.Note = txtNote[i] ?? string.Empty;
                                model.ModifiedDate = DateTime.Now;
                                model.ModifiedBy = user.USER_ID;
                                CourseService.UpdateAttendance(model);
                            }
                            else
                            {
                                model = new Course_Attendance()
                                {
                                    CourseDetailId = courseDetailId,
                                    TraineeId = getTraineeId,
                                    Type = getType,
                                    Note = txtNote[i] ?? string.Empty,
                                    AttendedDate = DateTime.ParseExact(attenDate[i], "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    Attender = user.USER_ID,
                                    CreatedDate = DateTime.Now,
                                    CreaatedBy = user.USER_ID,
                                };
                                CourseService.InsertAttendance(model);
                            }
                        }

                        i++;

                    }

                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }



                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);



            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertAttendance", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerGroupCertificate.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandlerGroupCertificate(jQueryDataTableParamModel param)
        {
            try
            {

                var data = CourseService.GetAllGroupCertificate();


                IEnumerable<Group_Certificate> filtered = data;
                var sortColumnIdex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Group_Certificate, object> orderingFunction = (c =>
                                 sortColumnIdex == 1 ? c.Name
                                 : sortColumnIdex == 2 ? c.IsActive
                                : (object)c.CreateDate);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);


                var result = from c in displayed.ToArray()
                             select new object[]
                             {
                                  string.Empty,
                                     c?.Name,
                                    (c?.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_GroupCertificate(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_GroupCertificate(1,"+c?.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                                   "<a title='Edit' href='javascript:void(0);' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' onclick='loadContent("+c?.Id+")' ></i></a>"
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
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                   JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetSub.
        /// </summary>
        /// <param name="groupCertificate">The groupCertificate<see cref="Group_Certificate_Schedule"/>.</param>
        /// <returns>The <see cref="GetSubject"/>.</returns>
        private GetSubject GetSub(Group_Certificate_Schedule groupCertificate)
        {
            var result = new GetSubject();
            result.IsCompleted = false;
            result.Content += "&nbsp;-&nbsp;" + groupCertificate.Group_Certificate.Name + "<br/>";
            if (groupCertificate.Group_Certificate_Schedule_Detail.Any())
            {
                foreach (var item in groupCertificate.Group_Certificate_Schedule_Detail)
                {
                    if (item.Type == (int)UtilConstants.StatusTraineeHistory.Completed)
                    {
                        result.Content += "&nbsp;&nbsp;&nbsp;&nbsp;+&nbsp;" + "<label style='color: #4caf50;'>" + item.SubjectDetail.Code + " - " + item.SubjectDetail.Name + "</label> <i class='fas fa-check-circle font-byhoa' color: #4caf50; aria-hidden='true'></i><br/>";
                    }
                    else
                    {
                        result.Content += "&nbsp;&nbsp;&nbsp;&nbsp;+&nbsp;" + item.SubjectDetail.Code + " - " + item.SubjectDetail.Name + "<br/>";
                    }
                }
                if (
                    groupCertificate.Group_Certificate_Schedule_Detail.All(
                        a => a.Type == (int)UtilConstants.StatusTraineeHistory.Completed))
                {
                    result.IsCompleted = true;
                }
            }

            return result;
        }

        /// <summary>
        /// The AjaxHandlerEmployeeSubjectCertificate.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandlerEmployeeSubjectCertificate(jQueryDataTableParamModel param)
        {
            try
            {
                var depId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());

                var jobId = string.IsNullOrEmpty(Request.QueryString["JobTitle"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitle"].Trim());
                var groupCertificateId = string.IsNullOrEmpty(Request.QueryString["GroupCertificate"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupCertificate"].Trim());

                var fullName = string.IsNullOrEmpty(Request.QueryString["Fullname"]) ? string.Empty : Request.QueryString["Fullname"].Trim();
                var email = string.IsNullOrEmpty(Request.QueryString["Email"]) ? string.Empty : Request.QueryString["Email"].Trim();
                var eid = string.IsNullOrEmpty(Request.QueryString["Eid"]) ? string.Empty : Request.QueryString["Eid"].Trim();

                var data = CourseService.GetAllGroupCertificateSchedule(a =>
                    (depId == -1 || a.Trainee.Department_Id == depId) &&
                    (jobId == -1 || a.Trainee.Job_Title_id == jobId) &&
                    (groupCertificateId == -1 || a.IdGroupCertificate == groupCertificateId) &&
                    (string.IsNullOrEmpty(fullName) || (a.Trainee.FirstName + " " + a.Trainee.LastName).Contains(fullName)) &&
                    (string.IsNullOrEmpty(email) || a.Trainee.str_Email.Contains(email)) &&
                    (string.IsNullOrEmpty(eid) || a.Trainee.str_Staff_Id.Contains(eid)) &&
                    a.Group_Certificate.IsDeleted == false && a.Group_Certificate.IsActive == true
                );


                IEnumerable<Group_Certificate_Schedule> filtered = data;
                var sortColumnIdex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Group_Certificate_Schedule, object> orderingFunction = (c =>
                                 sortColumnIdex == 1 ? c?.Trainee?.str_Staff_Id
                                 : sortColumnIdex == 2 ? ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                                 : sortColumnIdex == 3 ? c?.Trainee?.Department?.Name
                                 : sortColumnIdex == 4 ? c?.Trainee?.JobTitle?.Name
                                 : sortColumnIdex == 5 ? c?.Group_Certificate_Schedule_Detail.Any(b => b.Type == (int)UtilConstants.StatusTraineeHistory.Completed)
                                : (object)c?.Group_Certificate_Schedule_Detail.Any(b => b.Type == (int)UtilConstants.StatusTraineeHistory.Completed));
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var domainName = Request.Url.Authority;
                var result = from c in displayed.ToArray()
                             let sub = GetSub(c)
                             select new object[]
                             {
                                 string.Empty,
                                 c?.Trainee?.str_Staff_Id,
                                    ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                    c?.Trainee?.Department?.Name,
                                     c?.Trainee?.JobTitle?.Name,
                                      sub.Content,
                            //          !sub.IsCompleted
                            //? ""
                            //: "<input  type='hidden' class='form-control' name='IdGroupCertificate' value='" + c?.Id +
                            //  "'/><input type='text' class='form-control' name='GroupCertificateNo' value='" + c?.SRNO + "'/>",
                               !string.IsNullOrEmpty(c?.SRNO) ?  c?.SRNO.Replace("||","")  : string.Empty,

                             !string.IsNullOrEmpty(c?.SRNO)
                            ? ( (bool)c?.Path.StartsWith("<div") ? "<a onclick='Blank_Review("+c?.IdTrainee+")' title='View'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i> </a> "+
                              "<div id='c"+c?.IdTrainee+"' style='display:none;'><span id='a"+c?.IdTrainee+"' class='widget'>" + c?.Path+ "</span></div>" : "<a  href='//"+domainName + GetByKey("PathFileExtImage") +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>")
                            : ""
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
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                   JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerSubjectCertificate.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectCertificate(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var ddl_subject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                //var datafromto = CourseDetailService.GetAllApi(a => a.CourseId == courseList && a.SubjectDetailId == ddl_subject).Select(a => (int?)a.Id);

                var data = CourseMemberService.Get(a => a.Course_Details_Id == ddl_subject && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved),true);
                //var model = data.Select(a => new AjaxCourseResultFinalModel
                //{
                //    TraineeCode = a.Trainee?.str_Staff_Id,
                //    //FullName = a.Trainee?.FirstName + " " + a.Trainee?.LastName,
                //    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),

                //    DepartmentName = a.Trainee?.Department?.Name,
                //    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),                  
                //    Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                //    Id = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Id ?? 0,
                //    TraineeId = a.Member_Id ?? -1,
                //    SubjectDetailId = a.Course_Detail?.SubjectDetailId ?? -1,
                //    CourseDetailId = a.Course_Details_Id ?? -1,
                //    Type = a.Course_Detail?.Course_Result?.Any(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id && b.Type == true) ?? false,
                //    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),                  
                //    codecertificate = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateSubject,
                //    checkcodecertificate = a?.Course_Detail?.SubjectDetail?.CertificateCode,
                //    checkstatus = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.StatusCertificate,
                //    CertificateID = GetCertificate(GetGradeInt(returnpointgrade_Custom(2, a?.Course_Detail?.Course_Result, a?.Member_Id, a?.Course_Details_Id)), 0, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateID),
                //});

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                            : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                            : sortColumnIndex == 4 ? DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy")
                                                            : sortColumnIndex == 5 ? ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id))
                                                            //: sortColumnIndex == 6 ? c.Remark
                                                            : c.Trainee?.str_Staff_Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var domainName = Request.Url.Authority;
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c.Trainee?.str_Staff_Id,
                                                ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
                                                c.Trainee?.Department?.Name,
                                                DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy") ,
                                               ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)),
                                                ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)),
                                                //c?.Remark,
                                              returnpointgrade_Custom(2,c?.Course_Detail?.Course_Result, c?.Member_Id, c?.Course_Details_Id) == "Fail" ? "<input  type='hidden' class='form-control' name='is_fail' value='Fail'/>" +
                                                  "<input  type='hidden' class='form-control' name='subject_Id' value='"+c?.Course_Detail.SubjectDetailId +"'/>" +
                                                 "<input  type='hidden' class='form-control' name='course_detail_id' value='"+c?.Course_Details_Id+"'/>" +
                                                "<input  type='hidden' class='form-control' name='trainee_id' value='"+c?.Member_Id+"'/>" +
                                                "<input  type='hidden' class='form-control' name='Id_' value='"+(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id ?? 0)+"'/>" +
                                                "<input type='hidden' class='form-control' readonly name='CerNo' value='nocertificate'/>" :

                                                 (string.IsNullOrEmpty(c?.Course_Detail?.SubjectDetail?.CertificateCode) ? "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>" +
                                                  "<input  type='hidden' class='form-control' name='subject_Id' value='"+c?.Course_Detail.SubjectDetailId+"'/>" +
                                                 "<input  type='hidden' class='form-control' name='course_detail_id' value='"+c?.Course_Details_Id+"'/>" +
                                                "<input  type='hidden' class='form-control' name='trainee_id' value='"+c?.Member_Id+"'/>" +
                                                 "<input  type='hidden' class='form-control' name='Id_' value='"+(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id ?? 0)+"'/>" +
                                                "<input type='hidden' class='form-control' readonly name='CerNo' value='nocertificate'/>" : (!string.IsNullOrEmpty(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.CertificateSubject) ?
                                                 "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>"+
                                                "<input  type='hidden' class='form-control' name='subject_Id' value='"+c?.Course_Detail.SubjectDetailId+"'/>" +
                                                "<input  type='hidden' class='form-control' name='course_detail_id' value='"+c?.Course_Details_Id+"'/>" +
                                                "<input  type='hidden' class='form-control' name='trainee_id' value='"+c?.Member_Id+"'/>" +
                                                 "<input  type='hidden' class='form-control' name='Id_' value='"+(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id ?? 0)+"'/>" +
                                                "<input type='text' class='form-control' readonly name='CerNo' value='"+c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.CertificateSubject+"'/>"
                                                :
                                                "<input  type='hidden' class='form-control' name='is_fail' value='NotFail'/>"+
                                                "<input  type='hidden' class='form-control' name='subject_Id' value='"+c?.Course_Detail.SubjectDetailId+"'/>" +
                                                "<input type='hidden' class='form-control' name='course_detail_id' value='"+c?.Course_Details_Id+"'/>" +
                                                "<input  type='hidden' class='form-control' name='trainee_id' value='"+c?.Member_Id+"'/>" +
                                                 "<input  type='hidden' class='form-control' name='Id_' value='"+(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id ?? 0)+"'/>" +
                                                "<input type='text' class='form-control' readonly name='CerNo' value='"+CreateCodeCertificateSubject(c?.Course_Detail.SubjectDetail)+"'/>")) ,
                                             GetCertificate(GetGradeInt(returnpointgrade_Custom(2, c?.Course_Detail?.Course_Result, c?.Member_Id, c?.Course_Details_Id)), 0, c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.CertificateID),
                                              returnpointgrade_Custom(2,c?.Course_Detail?.Course_Result, c?.Member_Id, c?.Course_Details_Id) == "Fail" ? "" : (returnpointgrade_Custom(2,c?.Course_Detail?.Course_Result, c?.Member_Id, c?.Course_Details_Id) != "Fail" && string.IsNullOrEmpty(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.CertificateSubject) ? "" : (string.IsNullOrEmpty(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.StatusCertificate.ToString()) ? "": (c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.StatusCertificate == 0 ? "<span class='label label-success'>Have a certificate</span>" : "<span class='label label-warning'>Have been revoked</span>")))
                            //                         string.IsNullOrEmpty(c?.codecertificate)
                            //? ( (bool)c?.Path.StartsWith("<div") ? "<a onclick='Blank_Review("+c?.TraineeId+")' title='View'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i> </a> "+
                            //  "<div id='c"+c?.TraineeId+"' style='display:none;'><span id='a"+c?.TraineeId+"' class='widget'>" + c?.Path+ "</span></div>" : "<a  href='//"+domainName + GetByKey("PathFileExtImage") +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>")
                            //: ""
                                                //"<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + c?.Id +")'><i class='fas fa-edit' aria-hidden='true' style='color: " + (c?.Type == true ? "red" : "black" ) +" ; '></i></a>"

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

        /// <summary>
        /// The ReturnTraineePoint.
        /// </summary>
        /// <param name="isFirstCheck">The isFirstCheck<see cref="bool"/>.</param>
        /// <param name="isAvarage">The isAvarage<see cref="bool?"/>.</param>
        /// <param name="rs">The rs<see cref="Course_Result"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected object ReturnTraineePoint(bool isFirstCheck, bool? isAvarage, Course_Result rs)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            object point = null;
            if (rs == null) return null;
            if (isFirstCheck)
            {
                if (isAvarage.HasValue && isAvarage.Value)
                {
                    point = rs.First_Check_Score != -1 ? rs.First_Check_Score : 0;
                }
                else
                {
                    switch (rs.First_Check_Result)
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
                    point = rs.Re_Check_Score != -1 ? rs.Re_Check_Score : 0;
                }
                else
                {
                    switch (rs.Re_Check_Result)
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

        /// <summary>
        /// The returnpointgrade.
        /// </summary>
        /// <param name="type">The type<see cref="int?"/>.</param>
        /// <param name="Trainee_Id">The Trainee_Id<see cref="int?"/>.</param>
        /// <param name="Course_Details_Id">The Course_Details_Id<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
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
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString().Replace("-1", "0");
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString().Replace("-1", "0");
                        }
                    }
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
                            var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderByDescending(a => a.point_from);
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
                                var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderBy(a => a.point_from);
                                foreach (var item in check_distintion)
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

        /// <summary>
        /// The returnpointgrade_Custom.
        /// </summary>
        /// <param name="type">The type<see cref="int?"/>.</param>
        /// <param name="data_">The data_<see cref="ICollection{Course_Result}"/>.</param>
        /// <param name="Trainee_Id">The Trainee_Id<see cref="int?"/>.</param>
        /// <param name="Course_Details_Id">The Course_Details_Id<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string returnpointgrade_Custom(int? type, ICollection<Course_Result> data_, int? Trainee_Id, int? Course_Details_Id)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            string _return = "";
            var data = data_.FirstOrDefault(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id);
            if (data != null)
            {
                if (type == 1)
                {
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString().Replace("-1", "0");
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString().Replace("-1", "0");
                        }
                    }
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
                            var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderByDescending(a => a.point_from);
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
                                var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderBy(a => a.point_from);
                                foreach (var item in check_distintion)
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

        /// <summary>
        /// The SetCertificate.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult SetCertificate()
        {
            var model = new SetCertificateModel();
            model.Programs = CourseService.Get().ToDictionary(a => a.Id, a => a.Name);
            return View(model);
        }

        /// <summary>
        /// The AddGroup.
        /// </summary>
        /// <param name="model">The model<see cref="GroupCertificateModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddGroup(GroupCertificateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = MessageInvalidData(ModelState)
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var result = CourseService.ModifyGroupCertificate(model);
                if (result != null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = true,
                        message = string.Format(Messege.CREATE_SUCCESSFULLY, result.Name)
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.SUCCESS
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The FilterImg.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FilterImg(int? id)
        {
            var data = CourseService.GetCatCertificateById(id);
            var model = new LoadImg();
            if (data != null)
            {
                model.Content = data.Content;
            }
            return PartialView("_Partials/_LoadImg", model);
        }

        /// <summary>
        /// The LoadContent.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult LoadContent(int? id)
        {
            var model = new GroupCertificateModel();
            var data = CourseService.GetGroupCertificateById(id);
            model.Subjects = _repoSubject.GetSubjectDetail(null).OrderByDescending(b => b.Id).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
            model.Certificates = CourseService.GetCatCertificates(null).ToDictionary(a => a.ID, a => a.Name);
            if (data != null)
            {
                model.Id = data.Id;
                model.Name = data.Name;
                model.IdSubjects = data.Group_Certificate_Subjects.Select(a => a.IdSubject).ToArray();
                model.CertificateId = data.IdCertificate;
            }
            return PartialView("_Partials/_ListCertificate", model);
        }

        /// <summary>
        /// The SaveCertificate.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SaveCertificate(FormCollection form)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseid = string.IsNullOrEmpty(form["courseid"]) ? 0 : Convert.ToInt32(form["courseid"]);
                var subjectid = string.IsNullOrEmpty(form["subjectid"]) ? 0 : Convert.ToInt32(form["subjectid"]);
                var certificateForAll = string.IsNullOrEmpty(form["CertificateForAll"]) ? -1 : Convert.ToInt32(form["CertificateForAll"].Trim());
                var certificateByUser = form.GetValues("CertificateByUser");
                var formDateCompleted = form["CreateDate"];
                var cerNo = form.GetValues("CerNo");
                var ATO = form["ATO"];
                var id = form.GetValues("Id_");
                var checkFail = form.GetValues("is_fail");
                var course = _repoCourse.GetById(courseid);
                var subject = course.Course_Detail.FirstOrDefault(p => p.Id == subjectid)?.SubjectDetail;
                var subjectCode = subject != null ? subject.Code : "";
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

                DateTime dateCompleted;
                DateTime.TryParse(formDateCompleted, out dateCompleted);
                foreach (var item in id)
                {
                    if (!string.IsNullOrEmpty(cerNo?[i]))
                    {
                        var fId = int.Parse(item);
                        var intCertificateId = certificateByUser != null ? int.Parse(certificateByUser[i]) : -1;
                        var fResult = CourseService.GetCourseResultById(fId);
                        if(fResult != null)
                        {
                            if (cerNo != null)
                            {
                                var listcertificateapproved = CourseService.GetAllGroupCertificateApprove(a => a.IdTrainee == fResult.Trainee.Id && a.TypeCertificate == 0 && a.CourseResultID.HasValue && a.CourseResultID == fResult.Id);
                                var entity =
                                         new TMS_CertificateApproved();
                                if (cerNo[i] != "nocertificate")
                                {
                                    if (fResult.StatusCertificate != 0 || string.IsNullOrEmpty(fResult.CertificateSubject))
                                    {
                                        var catCertificate = CourseService.GetCatCertificateById(certificateForAll != -1 ? certificateForAll : intCertificateId);
                                        var content = BodyCertificateSubject(catCertificate, null, fResult.Trainee,
                                             fResult.Course_Detail.Course, fResult);
                                        //TypeCertificate = 1 dành cho group, 0 là dành cho subject

                                        DateTime? ex_date = null;
                                        var dataCourseDetail = CourseService.GetTraineemember(a => a.Course_Detail.SubjectDetailId == fResult.Course_Detail.SubjectDetailId && a.Course_Detail.IsDeleted != true && a.IsDelete != true && a.Member_Id == fResult.TraineeId && (a.Status == 0 || a.Status == null)).Where(a =>
                                                  a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));
                                        if (dataCourseDetail.Any())
                                        {
                                            IEnumerable<TMS_Course_Member> filteredB = dataCourseDetail;
                                            filteredB = filteredB.OrderByDescending(a => a.Course_Detail.dtm_time_from);
                                            var displayedB = filteredB;
                                            var resultB = from c in displayedB.ToArray()
                                                          select new ProfileSubjectModel
                                                          {
                                                              bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                                              SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                                              dtm_from = c?.Course_Detail?.dtm_time_from,
                                                              subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                                              courseDetails = c?.Course_Detail,
                                                              memberId = c?.Member_Id,
                                                          };
                                            resultB = resultB.ToList();
                                            for (int y = 0; y < resultB.Count(); y++)
                                            {
                                                if (y != 0 && resultB.ElementAt(y).subjectName == resultB.ElementAt(y - 1).subjectName)
                                                {
                                                    resultB.ElementAt(y).ex_Date = ResturnExpiredate(resultB.ElementAt(y).courseDetails, resultB.ElementAt(y).memberId, resultB.ElementAt(y - 1).ex_Date);
                                                }
                                                else
                                                {
                                                    resultB.ElementAt(y).ex_Date = ResturnExpiredate(resultB.ElementAt(y).courseDetails, resultB.ElementAt(y).memberId, resultB.ElementAt(y).ex_Date);
                                                }
                                                if (fResult?.Course_Detail?.dtm_time_from == resultB.ElementAt(y).dtm_from)
                                                {
                                                    ex_date = resultB.ElementAt(y).ex_Date;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ex_date = ResturnExpiredate(fResult?.Course_Detail, fResult?.TraineeId, ex_date);
                                        }

                                        if (listcertificateapproved.Any())
                                        {
                                            entity = listcertificateapproved.FirstOrDefault();
                                            if (entity.status != (int)UtilConstants.StatusApiApprove.Approved)
                                            {
                                                entity.IdTrainee = fResult.Trainee.Id;
                                                entity.certificatefinal = cerNo[i];
                                                entity.IsActive = true;
                                                entity.IsDeleted = false;
                                                entity.Path = content;
                                                entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                                entity.CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;
                                                entity.Expiration_Date = ex_date;
                                            }
                                        }
                                        else
                                        {
                                            entity = new TMS_CertificateApproved();
                                            entity.IdTrainee = fResult.Trainee.Id;
                                            entity.CreateDate = DateTime.Now;
                                            entity.CreateBy = CurrentUser.USER_ID;
                                            entity.certificatefinal = cerNo[i];
                                            entity.IsActive = true;
                                            entity.IsDeleted = false;
                                            entity.Path = content;
                                            entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                            entity.CAT_CERTIFICATE_ID = certificateForAll != -1 ? certificateForAll : intCertificateId;
                                            entity.CourseResultID = fResult.Id;
                                            entity.TypeCertificate = 0; //TypeCertificate = 1 dành cho group, 0 là dành cho subject
                                            entity.Expiration_Date = ex_date;
                                            entity.IsRevoked = false;
                                        }
                                        CourseService.ModifyTMSCertificateAppovedEntity(entity);
                                        count++;
                                    }
                                }
                                else
                                {
                                    if (checkFail[i] == "Fail")
                                    {
                                        if (listcertificateapproved.Any())
                                        {
                                            entity = listcertificateapproved.FirstOrDefault();
                                            if (entity.status != (int)UtilConstants.StatusApiApprove.Approved)
                                            {
                                                entity.IsRevoked = true;
                                                entity.status = (int)UtilConstants.StatusApiApprove.Pending; //chưa duyệt
                                                CourseService.ModifyTMSCertificateAppovedEntity(entity);
                                                count++;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                       
                    }
                    i++;
                }
                if (count == 0)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.CERTIFICATE_TRAINEE_NOT_ADDNEW,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
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
                var count_temp = 0;
                var list_temp = new List<int>(new int[count_temp]);
                var firstOrDefault = UserContext.Get(a => a.IsDeleted != true && a.ISACTIVE == 1 && a.UserRoles.Any(x => x.ROLE.NAME == "HOD" && x.ROLE.IsActive == true), list_temp).ToList();
                if (firstOrDefault.Count() > 0)
                {
                    foreach (var item in firstOrDefault)
                    {
                        var to = item.ID;
                        var notification = new Notification { Message = "Request for certificate approval. ", MessageContent = string.Format("The certificate subject '{0}' needs to be approved.", course.Code + " " + subjectCode), MessageVN = "Yêu cầu phê duyệt chứng chỉ", MessageContentVN = string.Format("Chứng chỉ của môn '{0}' cần được phê duyệt.", course.Code + " " + subjectCode), URL = "/Approve/Certificate", Type = (int)UtilConstants.NotificationType.AutoProcess };
                        notification.Notification_Detail.Add(new Notification_Detail
                        {
                            idmessenge = notification.MessageID,
                            datesend = DateTime.Now,
                            iddata = to,
                            iduserfrom = CurrentUser?.USER_ID,
                            status = 0,
                            IsDeleted = false,
                            IsActive = true
                        });
                        NotificationService.Insert(notification);
                    }

                }
                else
                {
                    var to = 7;
                    var notification = new Notification { Message = "Request for certificate approval. ", MessageContent = string.Format("The certificate subject '{0}' needs to be approved.", course.Code + " " + subjectCode), MessageVN = "Yêu cầu phê duyệt chứng chỉ", MessageContentVN = string.Format("Chứng chỉ của môn '{0}' cần được phê duyệt.", course.Code + " " + subjectCode), URL = "/Approve/Certificate", Type = (int)UtilConstants.NotificationType.AutoProcess };
                    notification.Notification_Detail.Add(new Notification_Detail
                    {
                        idmessenge = notification.MessageID,
                        datesend = DateTime.Now,
                        iddata = to,
                        iduserfrom = CurrentUser?.USER_ID,
                        status = 0,
                        IsDeleted = false,
                        IsActive = true
                    });
                    NotificationService.Insert(notification);
                }




                

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

        /// <summary>
        /// The SubmitSetParticipateGroupCertificate.
        /// </summary>
        /// <param name="isParticipate">The isParticipate<see cref="int"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SubmitSetParticipateGroupCertificate(int isParticipate, int id)
        {
            try
            {
                var removeGroupCer = CourseService.GetGroupCertificateById(id);
                if (removeGroupCer == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/SubmitSetParticipateGroupCertificate", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false

                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeGroupCer.IsActive = false;
                }
                else
                {
                    removeGroupCer.IsActive = true;
                }
                CourseService.UpdateGroupCertificate(removeGroupCer);
                //var callLms = CallServices(UtilConstants.CRON_USER);
                //if (!callLms)
                //{
                //    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SubmitSetParticipateEmployee", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, fullName));
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, fullName),
                //        result = false
                //    });
                //}
                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeGroupCer.Name), result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SubmitSetParticipateEmployee", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ManagerCost.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ManagerCost()
        {

            return View();
        }

        /// <summary>
        /// The AjaxHandlerManagerCost.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerManagerCost(jQueryDataTableParamModel param)
        {
            try
            {
                var code = string.IsNullOrEmpty(Request.QueryString["Code"]) ? "" : Request.QueryString["Code"].Trim();
                var name = string.IsNullOrEmpty(Request.QueryString["Name"]) ? "" : Request.QueryString["Name"].Trim();

                var data = CourseService.Get(a =>
                                                  (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                                                  (string.IsNullOrEmpty(name) || a.Name.Contains(name)), false);
                IEnumerable<Course> models = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Code
                                                            : sortColumnIndex == 2 ? c?.Name
                                                          : c?.Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                var filtered = (sortDirection == "asc") ? models.OrderBy(orderingFunction)
                                   : models.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let sum = c?.Transaction_Course_Trainee.Sum(b => b.TransactionCost)
                             select new object[] {
                                 string.Empty,
                                 "<a href='/Course/DetailCourseCost/"+c?.Id+"'>"+c?.Code+"</a>",
                                 "<a href='/Course/DetailCourseCost/"+c?.Id+"'>"+c?.Name+"</a>",
                                 c?.TrainingProgam_Cost != null ?   c?.TrainingProgam_Cost?.LastOrDefault()?.Cost.Value.ToString("#,##.00") : "",
                                 sum > 0 ? sum.Value.ToString("#,##.00") : "",
                                    "<a href='/Course/DetailCourseCost/"+c?.Id+"'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerMListCost", ex.Message);


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The DetailCourseCost.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult DetailCourseCost(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("ManagerCost", "Course");
            }
            var course = CourseService.GetById(id);
            var model = new DetailCourseCostModel();
            model.Id = course.Id;
            model.Code = course.Code;
            model.Name = course.Name;
            model.Venue = course.Venue;
            model.Note = course.Note;
            model.Date = course.StartDate?.ToString("dd/MM/yyyy") + " -  " + course.EndDate?.ToString("dd/MM/yyyy");
            model.Cost = course.TrainingProgam_Cost?.LastOrDefault()?.Cost.Value.ToString("#,##.00");
            return View(model);
        }

        /// <summary>
        /// The AjaxHandlerCourseCost.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerCourseCost(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {

                var data = CourseService.GetById(id);
                IEnumerable<Course_Detail> models = data.Course_Detail;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, string> orderingFunction = (c
                                                          => sortColumnIndex == 0 ? c?.SubjectDetail?.Code
                                                            : sortColumnIndex == 1 ? c?.SubjectDetail?.Name
                                                          : c?.SubjectDetail?.Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                var filtered = (sortDirection == "asc") ? models.OrderBy(orderingFunction)
                                   : models.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let dbScorePass = c?.SubjectDetail?.Subject_Score?.Min(x => x.point_from)

                             let instructor = c?.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(b => b.Trainee.FirstName + " " + b.Trainee.LastName).Aggregate("", (current, fullName) => current + (fullName + "<br />"))
                             let hannah = c?.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah).Select(b => b.Trainee.FirstName + " " + b.Trainee.LastName).Aggregate("", (current, fullName) => current + (fullName + "<br />"))
                             let mentor = c?.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor).Select(b => b.Trainee.FirstName + " " + b.Trainee.LastName).Aggregate("", (current, fullName) => current + (fullName + "<br />"))



                             select new object[] {
                                 string.Empty,
                                 c?.SubjectDetail?.Code,
                                 c?.SubjectDetail?.Name,
                                 c?.SubjectDetail?.IsAverageCalculate == true ? "Yes" : "No",
                                 dbScorePass != null ? dbScorePass.ToString() : "",
                                 c?.SubjectDetail?.RefreshCycle,
                                 c?.SubjectDetail?.Duration?.ToString(CultureInfo.InvariantCulture),
                                 c?.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" +c?.time_from.Substring(0, 2) + "" + c?.time_from.Substring(2),
                                 c?.dtm_time_to?.ToString("dd/MM/yyyy") +"<br />"+c?.time_to.Substring(0, 2) + "" + c?.time_to.Substring(2),
                                 c?.type_leaning == (int)UtilConstants.LearningTypes.Online ? "<i class='fa fa-desktop' style='color:red;'></i>" : c?.type_leaning == (int)UtilConstants.LearningTypes.Offline ? "<i class='fas fa-chalkboard-teacher'  style='color:green;'></i>" : "<i class='fas fa-book-reader'  style='color:royalblue;'></i>",
                                 c?.Room == null ? "" : c?.Room.str_Name,
                                 instructor,
                                 hannah,
                                 mentor

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerMListCost", ex.Message);


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandlerTraineeCost.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AjaxHandlerTraineeCost(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {

                var data = CourseService.GetById(id);
                IEnumerable<Transaction_Course_Trainee> models = data.Transaction_Course_Trainee;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Transaction_Course_Trainee, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                    : sortColumnIndex == 2 ? c?.Trainee?.LastName
                    : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                    : sortColumnIndex == 4 ? c?.Trainee?.Department?.Ancestor
                    : c?.Trainee?.Department?.Ancestor);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                var filtered = (sortDirection == "asc") ? models.OrderBy(orderingFunction)
                                   : models.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let fullName = ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                             select new object[] {
                                 string.Empty,
                                 c?.Trainee?.str_Staff_Id,
                                 fullName,
                                 c?.Trainee?.str_Staff_Id,
                                 c?.Trainee?.Department?.Name,
                                 c?.CurrentPurchaseCost,
                                 c?.TransactionCost
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerMListCost", ex.Message);


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ModifyIngredient.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ModifyIngredient()
        {
            var model = new PartialIngredientsViewModify();
            model.DictionaryIngredients = CourseService.GetCourseIngredients(null).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name);
            return PartialView("_Partials/_IngredientModify", model);
        }

        /// <summary>
        /// The ModifyIngredient.
        /// </summary>
        /// <param name="model">The model<see cref="PartialIngredientsViewModify"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult ModifyIngredient(PartialIngredientsViewModify model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var random = Common.RandomCode();
                model.Code = random;
                CourseService.Modify(model);
                var result = new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/ModifyIngredient", ex.Message);

                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The filterIngredient.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult filterIngredient()
        {
            var html = new StringBuilder();
            var ingredients = CourseService.GetCourseIngredients(null).OrderBy(a => a.Name);
            if (!ingredients.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in ingredients)
            {
                html.AppendFormat("<option   value='{0}'>{1}</option>", item.Id, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GetRecordIngredient.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetRecordIngredient(int id)
        {
            try
            {
                var data = CourseService.GetIngredientsById(id);
                if (data != null)
                {
                    return Json(new
                    {
                        result = true,
                        name = data.Name,
                        description = data.Description,
                        id = data.Id

                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    result = false,
                    name = "",
                    description = "",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/GetRecordIngredient", ex.Message);
                return Json(new
                {
                    result = false,
                    name = "",
                    description = "",
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AddRowIngredient.
        /// </summary>
        /// <param name="row">The row<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AddRowIngredient(int row)
        {
            var html = new StringBuilder("");
            var DictionaryIngredients = CourseService.GetCourseIngredients(null).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name);
            html.Append(
                "<son class=' col-md-12 Ingredientfor' data-index='" + row + "' id='son_Detail_Ingredient_" + row +
                "'>" +
                "<div class='form-group col-md-1 margin-top-10'>" +
                "<a href='javascript:void(0)' data-toggle='tooltip' title='Remove Ingredient' data-index='" + row +
                "' onclick='removeIngredient(this)'>" +
                "<i class='fas fa-times'></i></a>&nbsp;" +
                "</div>" +
                "<div class='form-group col-md-5 margin-top-10'>" +
                "<select class='form-control input-sm default required DictionaryIngredients' data-index='" + row +
                "' data-toggle='tooltip' title='Ingredients' id='course-detail-Ingredient-select'>" +
                "<option value='-1' disabled selected>-- Ingredient --</option>");
            if (DictionaryIngredients.Any() || DictionaryIngredients != null)
            {
                foreach (var room in DictionaryIngredients)
                {
                    html.Append("<option value='" + room.Key + "'>" + room.Value + "</option>");
                }

            }
            html.Append("</select>" +
                "</div>" +
                "<div class='form-group col-md-2 margin-top-10'><input type='text' class='form-control input-sm percentIngredient' id='course-detail-Ingredient-percent' pattern='^[1-9][0-9]?$|^100$' /></div><div class='form-group col-md-4 margin-top-20-md'>&nbsp;</div></son>");



            return Json(new AjaxResponseViewModel()
            {
                result = true,
                data = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The AjaxHandlerListSelectSubject.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxHandlerListSelectSubject(string id, FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["FilterCodeOrName"])
                   ? string.Empty
                   : form["FilterCodeOrName"].Trim().ToLower();
            StringBuilder HTML = new StringBuilder();
            HTML.Append("<ul>");
            HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>&nbspAll</span>");
            HTML.Append("<ul>");
            var list_subject = _repoSubject.GetSubjectDetail(a => (string.IsNullOrEmpty(filterCodeOrName) || a.Code.Trim().ToLower().Contains(filterCodeOrName) || a.Name.Trim().ToLower().Contains(filterCodeOrName)));
            int CodeId = !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
            if (list_subject.Any())
            {
                var listTraineeGroup = CourseService.GetById(CodeId)?.Course_Subject_Item?.Select(a => a.SubjectId);
                foreach (var item in list_subject)
                {
                    HTML.Append("<li>");
                    HTML.AppendFormat("" +
                                      "<input data-id='-1' data-parentname='All'  multiple value='{0}' class='BindToSubject' name='BindToSubject' id='trainee_" + item.Id + "' type='checkbox' " + ((listTraineeGroup != null && listTraineeGroup.Contains(item.Id)) ? "Checked" : "") + " />" +
                                      "<input type='hidden' value='{1}' name='BindToSubject2' /><label for='trainee_" + item.Id + "'>&nbsp{2}</label>", item.Id, item.Id, string.Format("{0} - {1}", item?.Code, item?.Name));
                    HTML.Append("</li>");
                }
            }
            HTML.Append("</ul>");
            HTML.Append("</li>");
            HTML.Append("</ul>");
            return Json(HTML.ToString());
        }

        // Phan he quan ly Cac Instructor da khong hoat dong trong vong 5 nam
        /// <summary>
        /// The ExpiredInstructors.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ExpiredInstructors()
        {
            return View();
        }

        /// <summary>
        /// Defines the <see cref="ExpiredInstructor" />.
        /// </summary>
        public class ExpiredInstructor
        {
            /// <summary>
            /// Gets or sets the code.
            /// </summary>
            public string code { get; set; }

            /// <summary>
            /// Gets or sets the firstname.
            /// </summary>
            public string firstname { get; set; }

            /// <summary>
            /// Gets or sets the lastname.
            /// </summary>
            public string lastname { get; set; }

            /// <summary>
            /// Gets or sets the department.
            /// </summary>
            public string department { get; set; }

            /// <summary>
            /// Gets or sets the lastDay.
            /// </summary>
            public DateTime? lastDay { get; set; }

            /// <summary>
            /// Gets or sets the coursecode.
            /// </summary>
            public string coursecode { get; set; }

            /// <summary>
            /// Gets or sets the subject.
            /// </summary>
            public string subject { get; set; }

            /// <summary>
            /// Gets or sets the days.
            /// </summary>
            public int days { get; set; }
        }

        /// <summary>
        /// The AjaxExpiredInstructorsList.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxExpiredInstructorsList(jQueryDataTableParamModel param)
        {
            try
            {
                var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                DateTime dateNow = DateTime.Now;
                //DateTime oldestDate = DateTime.Now.Subtract(new TimeSpan(1825, 0, 0, 0, 0));
                var Namnamtrc = DateTime.Now.AddDays(-(365 * 5));
                var traine = EmployeeService.Get(a => a.Course_Detail_Instructor.Any(b => b.Course_Detail.dtm_time_to <= Namnamtrc)
                && (string.IsNullOrEmpty(fCode) || a.str_Staff_Id.Contains(fCode))
                && (string.IsNullOrEmpty(fName) || a.FirstName.Contains(fName) || a.LastName.Contains(fName))
                && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to) >= 0)

                && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateTo) >= 0), true);
                IQueryable<ExpiredInstructor> data = traine.Select(a => new ExpiredInstructor()
                {
                    code = a.str_Staff_Id,
                    firstname = a.FirstName,
                    lastname = a.LastName,
                    department = a.Department.Name,
                    lastDay = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to,
                    coursecode = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course.Code,
                    subject = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.SubjectDetail.Name,
                    days = DbFunctions.DiffDays(a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateNow) ?? 0,
                });
                IEnumerable<ExpiredInstructor> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ExpiredInstructor, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.code
                                                            : sortColumnIndex == 2 ? c.firstname + c.lastname
                                                            : sortColumnIndex == 3 ? c.department
                                                            : sortColumnIndex == 4 ? c.lastDay.ToString()
                                                            : sortColumnIndex == 5 ? c.coursecode
                                                            : sortColumnIndex == 6 ? c.subject
                                                            : sortColumnIndex == 7 ? c.days.ToString()
                                                            : "");

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//UtilConstants.VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                                    string.Empty,
                                    c.code,
                                    ReturnDisplayLanguage(c.firstname, c.lastname),
                                    c.department,
                                    string.Format("{0:dd/MM/yyyy}",c.lastDay),
                                    c.coursecode,
                                    c.subject,
                                    c.days,
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The ExportExpiredInstructors.
        /// </summary>
        /// <returns>The <see cref="FileContentResult"/>.</returns>
        [AllowAnonymous]
        public FileContentResult ExportExpiredInstructors()
        {
            var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            var filecontent = ExportExcelExpiredInstructors(fCode, fName, dateFrom, dateTo);

            return File(filecontent, ExportUtils.ExcelContentType, "ExpiredInstructor.xlsx");
        }

        /// <summary>
        /// The ExportExcelExpiredInstructors.
        /// </summary>
        /// <param name="fCode">The fCode<see cref="string"/>.</param>
        /// <param name="fName">The fName<see cref="string"/>.</param>
        /// <param name="dateFrom">The dateFrom<see cref="DateTime?"/>.</param>
        /// <param name="dateTo">The dateTo<see cref="DateTime?"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private byte[] ExportExcelExpiredInstructors(string fCode, string fName, DateTime? dateFrom, DateTime? dateTo)
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/ExpiredInstructor.xlsx");
            FileInfo template = new FileInfo(templateFilePath);

            DateTime dateNow = DateTime.Now;
            var Namnamtrc = DateTime.Now.AddDays(-(365 * 5));
            var traine = EmployeeService.Get(a => a.Course_Detail_Instructor.Any(b => b.Course_Detail.dtm_time_to <= Namnamtrc)
            && (string.IsNullOrEmpty(fCode) || a.str_Staff_Id.Contains(fCode))
            && (string.IsNullOrEmpty(fName) || a.FirstName.Contains(fName) || a.LastName.Contains(fName))
            && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to) >= 0)

            && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateTo) >= 0), true);
            IQueryable<ExpiredInstructor> data = traine.Select(a => new ExpiredInstructor()
            {
                code = a.str_Staff_Id,
                firstname = a.FirstName,
                lastname = a.LastName,
                department = a.Department.Name,
                lastDay = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to,
                coursecode = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course.Code,
                subject = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.SubjectDetail.Name,
                days = DbFunctions.DiffDays(a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateNow) ?? 0,
            });

            //Tao List Subject, de show tren cung, muc dich nhom Instructor theo Subject khi export Excel
            List<string> SubjectList = new List<string>();
            foreach (var item in data.OrderBy(a => a.days))
            {
                if (item != null)
                {
                    string newSubject = "";
                    if (item.subject.Contains("Initial") || item.subject.Contains("Recurrent") || item.subject.Contains("Re-Qualification") || item.subject.Contains("Upgrade") || item.subject.Contains("Bridge") || item.subject.Contains("General"))
                    {
                        string[] split = item.subject.Split(new[] { '-' }, 2);
                        newSubject = split[1] + " (" + split[0].TrimEnd() + ")";
                        newSubject = item.coursecode + " - " + newSubject;
                    }
                    else
                    {
                        newSubject = item.coursecode + " - " + item.subject;
                    }

                    SubjectList.Add(newSubject);
                }
            }
            SubjectList = SubjectList.Distinct().ToList();


            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                var startrow = 8;
                int count = 0;
                // string[] header = GetByKey("Report_TrainingPlan").Split(',');
                string[] header = GetByKey("Survey_Header").Split(new char[] { ',' });

                ExcelRange cellHeader = worksheet.Cells[1, 6];
                cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                cellHeader.Style.Font.Size = 11;

                foreach (var item in SubjectList)
                {
                    int totalInstructor = 0;
                    const int col = 1;

                    worksheet.Cells[
                        startrow, 1, startrow, 6]
                        .Merge = true;
                    ExcelRange cell = worksheet.Cells[startrow, 1];
                    cell.Value = item;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;

                    foreach (var item1 in data)
                    {
                        if (item1 != null && item.Contains(item1.coursecode)/* && item.Contains(item1.subject)*/)
                        {
                            count++;
                            totalInstructor++;
                            startrow++;
                            ExcelRange cellNo = worksheet.Cells[startrow, col, startrow, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellInstructorCode =
                            worksheet.Cells[startrow, col + 1, startrow, col + 1];
                            cellInstructorCode.Value = item1.code;
                            cellInstructorCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellInstructorCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellInstructorCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellInstructorName =
                           worksheet.Cells[startrow, col + 2, startrow, col + 2];
                            cellInstructorName.Value = ReturnDisplayLanguage(item1.firstname, item1.lastname);
                            cellInstructorName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellInstructorName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellInstructorName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellDepartment =
                                worksheet.Cells[startrow, col + 3, startrow, col + 3];
                            cellDepartment.Value = item1.department;
                            cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellLastDay =
                                worksheet.Cells[startrow, col + 4, startrow, col + 4];
                            cellLastDay.Value = string.Format("{0:dd/MM/yyyy}", item1.lastDay);
                            cellLastDay.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellLastDay.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellLastDay.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                            ExcelRange cellDaysAgo = worksheet.Cells[startrow, col + 5, startrow, col + 5];
                            cellDaysAgo.Value = item1.days;
                            cellDaysAgo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellDaysAgo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDaysAgo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        }

                    }
                    startrow++;

                    ExcelRange cellTotal = worksheet.Cells[startrow, 1, startrow, 2];
                    cellTotal.Style.Font.Bold = true;
                    cellTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTotal.Value = "INSTRUCTORS TOTAL:";
                    cellTotal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellTotal.Merge = true;

                    ExcelRange cellTotalHours = worksheet.Cells[startrow, 3];
                    cellTotalHours.Value = totalInstructor;
                    cellTotalHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTotalHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellTotalHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellMerge = worksheet.Cells[startrow, 4, startrow, 6];
                    cellMerge.Merge = true;
                    cellMerge.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    startrow++;

                }

                //int lastRow = startrow + row + groupHeader + grouptotal;
                //worksheet.Cells[lastRow + 1, 1, lastRow + 3, 12].Merge = true;
                //ExcelRange cell1 = worksheet.Cells[lastRow + 1, 1];
                //cell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //cell1.Style.Font.Bold = true;
                //cell1.Style.Font.Size = 11;
                //cell1.Value = "Notes: eL: e-Learning, cR: ClassRoom Learning, cRo: ClassRoom and Online Learning. The class will start from 08:30 to 12:00, and from 13:00 to 16:30 (From Mon-Sat). The training plan does not include the recheck, any failure must be reported to Vietjet Training Center for arrangement.  " + "\r\n" + "Please contact Vietjet Training Center for any change or further information. Contact No.: (+84) 83547 1853 Ext: 400 or Mail: vjaa.planningteam@vietjetair.com";
                //cell1.Style.WrapText = true;

                //worksheet.Cells[lastRow + 4, 1, lastRow + 4, 2].Merge = true;
                //ExcelRange cell3 = worksheet.Cells[lastRow + 4, 1];
                //worksheet.Cells[lastRow + 4, 1].Style.Font.Bold = true;
                //worksheet.Cells[lastRow + 4, 1].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell3.Value = "Approved by";

                //worksheet.Cells[lastRow + 5, 1, lastRow + 5, 2].Merge = true;
                //ExcelRange cell4 = worksheet.Cells[lastRow + 5, 1];
                //worksheet.Cells[lastRow + 5, 1].Style.Font.Bold = true;
                //worksheet.Cells[lastRow + 5, 1].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 5, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell4.Value = "HEAD OF TRAINING";

                //worksheet.Cells[lastRow + 6, 1, lastRow + 6, 2].Merge = true;
                //ExcelRange cell4_ = worksheet.Cells[lastRow + 6, 1];
                //worksheet.Cells[lastRow + 6, 1].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell4_.Value = "Date .....................................";

                //worksheet.Cells[lastRow + 4, 11, lastRow + 4, 12].Merge = true;
                //ExcelRange cell7 = worksheet.Cells[lastRow + 4, 11];
                //worksheet.Cells[lastRow + 4, 11].Style.Font.Bold = true;
                //worksheet.Cells[lastRow + 4, 11].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell7.Value = "Prepared by";

                //worksheet.Cells[lastRow + 5, 11, lastRow + 5, 12].Merge = true;
                //ExcelRange cell8 = worksheet.Cells[lastRow + 5, 11];
                //worksheet.Cells[lastRow + 5, 11].Style.Font.Bold = true;
                //worksheet.Cells[lastRow + 5, 11].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 5, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell8.Value = "PLANNING TEAM";

                //worksheet.Cells[lastRow + 6, 11, lastRow + 6, 12].Merge = true;
                //ExcelRange cell9 = worksheet.Cells[lastRow + 6, 11];
                //worksheet.Cells[lastRow + 6, 11].Style.Font.Size = 14;
                //worksheet.Cells[lastRow + 6, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell9.Value = "Date .....................................";

                Bytes = xlPackage.GetAsByteArray();

            }

            return Bytes;
        }

        /// <summary>
        /// The ExpiredInstructorsPrint.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult ExpiredInstructorsPrint()
        {
            // xử lý param gửi lên 
            var fCode = string.IsNullOrEmpty(Request.QueryString["fCode"]) ? "" : Request.QueryString["fCode"].Trim();
            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            DateTime dateNow = DateTime.Now;
            //DateTime oldestDate = DateTime.Now.Subtract(new TimeSpan(1825, 0, 0, 0, 0));
            var Namnamtrc = DateTime.Now.AddDays(-(365 * 5));
            var traine = EmployeeService.Get(a => a.Course_Detail_Instructor.Any(b => b.Course_Detail.dtm_time_to <= Namnamtrc)
            && (string.IsNullOrEmpty(fCode) || a.str_Staff_Id.Contains(fCode))
            && (string.IsNullOrEmpty(fName) || a.FirstName.Contains(fName) || a.LastName.Contains(fName))
            && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to) >= 0)

            && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateTo) >= 0), true);
            List<ExpiredInstructor> data = traine.Select(a => new ExpiredInstructor()
            {
                code = a.str_Staff_Id,
                firstname = a.FirstName,
                lastname = a.LastName,
                department = a.Department.Name,
                lastDay = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to,
                coursecode = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course.Code,
                subject = a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.SubjectDetail.Name,
                days = DbFunctions.DiffDays(a.Course_Detail_Instructor.OrderByDescending(b => b.Course_Detail.dtm_time_to).FirstOrDefault(b => b.Course_Detail.dtm_time_to <= Namnamtrc).Course_Detail.dtm_time_to, dateNow) ?? 0,
            }).ToList();

            return PartialView("_Partials/_ExpiredInstructorsPrint", data.OrderByDescending(a => a.days));
        }

        //Function check xem Course có phải là test đầu vào hay không
        //public bool CheckCourseEntranceTest(int? idCouresDetails)
        //{
        //    return idCouresDetails == Convert.ToInt32(ConfigurationManager.AppSettings["EntranceCourseID"]);

        //}
        //public List<int> ListCoursedetailTheoDiem(double result)
        //{

        //    if (result <= 10)
        //    {
        //        return new List<int>() { 7488, 7486, 7485 };
        //    }

        //    if (result <= 20)
        //    {
        //        return new List<int>() { 7487 };
        //    }

        //    if (result <= 30)
        //    {
        //        return new List<int>() { 7487 };
        //    }

        //    if (result <= 39)
        //    {
        //        return new List<int>() { 7487 };
        //    }
        //    return null;
        //}
        //public void xulydiemdauvao(int traineeID, double result, int courseId)
        //{
        //    var list = ListCoursedetailTheoDiem(result);
        //    if (list != null)
        //    {
        //        foreach (var courseDetailId in list)
        //        {
        //            _repoCourse.Insert(traineeID, courseDetailId, courseId, CurrentUser.USER_ID.ToString());
        //        }
        //    }
        //}
        //public bool isAssigned(int traineeID,int CoursedetailId)
        //{
        //    var courseMemberDetail = _repoCourse.GetListCourseMemberByMemberandCourseDetailId(traineeID, CoursedetailId);
        //    if(courseMemberDetail != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        /// <summary>
        /// Defines the CountNumber.
        /// </summary>
        private int CountNumber = 1;

        /// <summary>
        /// The CreateCodeCertificate.
        /// </summary>
        /// <param name="valuetype">The valuetype<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [AllowAnonymous]
        public string CreateCodeCertificate(int? valuetype = -1) //Create
        {
            string str_start = "";
            var datapartner = _repoSubject.GetGroupSubjectById(valuetype);
            if (datapartner != null)
            {
                str_start = datapartner?.CertificateCode?.ToString();
            }

            string EID = "";
            string EndsWith = DateTime.Now.Year.ToString();
            var data = _repoCourse.GetCourseResultFinal(a => a.certificatefinal.StartsWith(str_start) && a.certificatefinal.EndsWith(EndsWith));
            for (int i = data.Count() + 1; ; i++)
            {
                if (CountNumber > 1)
                {
                    i = CountNumber;
                }
                if (i <= 9)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "0000" + i.ToString();

                }
                else if (i > 9 && i <= 99)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "000" + i.ToString();

                }
                else if (i > 99 && i <= 999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "00" + i.ToString();

                }
                else if (i > 999 && i <= 9999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "0" + i.ToString();
                }
                else if (i > 9999 && i <= 99999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + i.ToString();
                }
                CountNumber++;

                var data_ = _repoCourse.GetCourseResultFinal(a => a.certificatefinal == EID);
                if (data_.Count() == 0)
                {
                    break;
                }
            }
            return EID;
        }

        /// <summary>
        /// The CreateCodeCertificateSubject.
        /// </summary>
        /// <param name="datapartner">The datapartner<see cref="SubjectDetail"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string CreateCodeCertificateSubject(SubjectDetail datapartner) //Create
        {
            string str_start = "";

            if (datapartner != null)
            {
                str_start = datapartner?.CertificateCode;
            }

            string EID = "";
            string EndsWith = DateTime.Now.Year.ToString();
            var data = CourseService.GetCourseResult(a => a.CertificateSubject.StartsWith(str_start) && a.CertificateSubject.EndsWith(EndsWith));

            for (int i = data.Count() + 1; ; i++)
            {
                if (CountNumber > 1)
                {
                    i = CountNumber;
                }
                if (i <= 9)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "0000" + i.ToString();

                }
                else if (i > 9 && i <= 99)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "000" + i.ToString();

                }
                else if (i > 99 && i <= 999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "00" + i.ToString();

                }
                else if (i > 999 && i <= 9999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + "0" + i.ToString();
                }
                else if (i > 9999 && i <= 99999)
                {
                    EID = str_start + "/" + DateTime.Now.Year + "-" + i.ToString();
                }
                CountNumber++;

                var data__ = CourseService.GetCourseResult(a => a.CertificateSubject == EID);
                if (data__.Count() == 0)
                {
                    break;
                }
            }
            return EID;
        }

        /// <summary>
        /// Cấp chứng chỉ cho học viên có approved và reject.
        /// </summary>
        /// <returns>.</returns>
        [AllowAnonymous]
        public ActionResult CreateCertificate()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new FinalCourseResultModel()
            {
                Courses = CourseService.Get(a => a.StartDate >= timenow && a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.Course && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.IsDeleted != true).OrderByDescending(b => b.StartDate).ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name)),
                //Courses =
                //     _repoTmsApproves.Get(
                //         a =>
                //             a.int_Type == (int)UtilConstants.ApproveType.Course &&
                //             a.int_id_status == (int)UtilConstants.EStatus.Approve && a.Course.IsDeleted == false)
                //         .OrderByDescending(b => b.Course.StartDate)
                //         .ToDictionary(c => c.Course.Id, c => string.Format("{0} - {1}", c.Course.Code, c.Course.Name)),
                //Certificates = CourseService.GetCatCertificates(a => a.IsActive == true).ToDictionary(b => b.ID, b => b.Name),
                //Departments = DepartmentService.Get(null, CurrentUser.PermissionIds).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name),
                //JobTitles = _repoJobTiltle.Get(null, true).ToDictionary(a => a.Id, a => a.Name),
                //GroupCertificates = CourseService.GetAllGroupCertificate(a => a.IsActive == true).ToDictionary(a => a.Id, a => a.Name)
            };
            return View(model);
        }

        /// <summary>
        /// Defines the AssignTraineeCols.
        /// </summary>
        private string[] AssignTraineeCols = new[] { "EID", "Remark", "Full Name" };

        /// <summary>
        /// Defines the <see cref="AssignImport" />.
        /// </summary>
        public class AssignImport
        {
            /// <summary>
            /// Gets or sets the Eid.
            /// </summary>
            public string Eid { get; set; }

            /// <summary>
            /// Gets or sets the Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Remark.
            /// </summary>
            public string Remark { get; set; }

            /// <summary>
            /// Gets or sets the Status.
            /// </summary>
            public string Status { get; set; }
        }

        /// <summary>
        /// The CheckFile.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult CheckFile()
        {
            try
            {
                HttpPostedFileBase postedFile = Request.Files[0];
                List<AssignImport> list = new List<AssignImport>();

                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => AssignTraineeCols.Contains(a)))
                    {
                        return Json(new AjaxResponseViewModel { message = Messege.VALIDATION_FILE, result = false }, JsonRequestBehavior.AllowGet);
                    }
                    var listDepartments = new Dictionary<string, AssignTraineeImportModel>();
                    var list_trainee = EmployeeService.Get(true);
                    for (var j = 2; j <= worksheet.Tables[0].Address.Rows; j++)
                    {
                        var code = worksheet.Cells[j, 1].Text.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            var traineeID = list_trainee.FirstOrDefault(a => a.str_Staff_Id == code);
                            if (traineeID != null)
                            {

                                AssignImport assignImport = new AssignImport();
                                assignImport.Eid = code;
                                assignImport.Name = traineeID.LastName + " " + traineeID.FirstName;
                                assignImport.Remark = string.IsNullOrEmpty(worksheet.Cells[j, 2].Text) ? "" : Regex.Replace(worksheet.Cells[j, 2].Text, "[\r\n]", "<br/>");
                                if(traineeID.IsActive == true)
                                {
                                    assignImport.Status = "<p class='text-success'>pass</p>";
                                }
                                else
                                {
                                    assignImport.Status = "<p class='text-warning'>DeActive</p>";
                                }
                               
                                list.Add(assignImport);
                            }
                            else
                            {
                                AssignImport assignImport = new AssignImport();
                                assignImport.Eid = code;
                                assignImport.Name = "";
                                assignImport.Remark = string.IsNullOrEmpty(worksheet.Cells[j, 2].Text) ? "" : Regex.Replace(worksheet.Cells[j, 2].Text, "[\r\n]", "<br/>");
                                assignImport.Status = "<p class='text-danger'>" + code + " " + Messege.VALIDATION_IMPORT_ASSIGN + "</p>";
                                list.Add(assignImport);
                            }
                        }
                    }
                }
                return Json(new AjaxResponseViewModel { result = true, data = JsonConvert.SerializeObject(list) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The AjaxHandleLisFinalCertificateCompleted.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandleLisFinalCertificateCompleted(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                string fTraineeEID = string.IsNullOrEmpty(Request.QueryString["fillter_eid"]) ? "" : Request.QueryString["fillter_eid"].Trim();
                string fName = string.IsNullOrEmpty(Request.QueryString["fillter_name"]) ? "" : Request.QueryString["fillter_name"].Trim();
                string fCertificate = string.IsNullOrEmpty(Request.QueryString["fillter_certificate"]) ? "" : Request.QueryString["fillter_certificate"].Trim();

                string fSearchDate_fromCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_FromCreate"]) ? "" : Request.QueryString["fSearchDate_FromCreate"].ToString();
                string fSearchDate_toCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_toCreate"]) ? "" : Request.QueryString["fSearchDate_toCreate"].ToString();
                int Status = string.IsNullOrEmpty(Request.QueryString["status_course"]) ? -1 : Convert.ToInt32(Request.QueryString["status_course"].Trim());
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_fromCreate, out dateFrom);
                DateTime.TryParse(fSearchDate_toCreate, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var data__temp = CourseService.GetCourseResultFinal(a =>
                (courseList == -1 && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue && string.IsNullOrEmpty(fTraineeEID) && string.IsNullOrEmpty(fName) && string.IsNullOrEmpty(fCertificate) ? a.Course.StartDate >= timenow : true) &&
                a.statusCertificate == Status && (courseList == -1 || a.courseid == courseList)
                && (dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateDate.Value) <= 0)
                && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateDate.Value, dateTo) <= 0)
                && (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID))
                && (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName))
                && (string.IsNullOrEmpty(fCertificate) || a.certificatefinal.Contains(fCertificate)), new int[] { (int)UtilConstants.ApproveType.CourseResult }, (int)UtilConstants.EStatus.Approve).GroupBy(v => new { v.traineeid, v.point, v.grade }).Select(c => c.FirstOrDefault());

                var data__ = data__temp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = data__temp.Count();


                //var model = data__.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                //{
                //    TraineeCode = a.Trainee?.str_Staff_Id,
                //    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                //    DepartmentName = a.Trainee?.Department?.Name,
                //    DateFromTo = DateUtil.DateToString(a.Course?.StartDate, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course?.EndDate, "dd/MM/yyyy"),
                //    TraineeId = a.Trainee?.Id ?? -1,
                //    Point = a.point.HasValue ? string.Format("{0:0.#}", a.point) : "",
                //    Grade = GetGrade(a.grade.HasValue ? a.grade : -1),
                //    codecertificate = a.certificatefinal,
                //    checkstatus = a.statusCertificate.HasValue ? a.statusCertificate : null,
                //    Path = a?.Path?.ToString() ?? ""
                //});

                IEnumerable<Course_Result_Final> filtered = data__;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                            : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                            : sortColumnIndex == 4 ? DateUtil.DateToString(c.Course?.StartDate, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course?.EndDate, "dd/MM/yyyy")
                                                            : sortColumnIndex == 5 ? (c.point.HasValue ? string.Format("{0:0.#}", c.point) : "")
                                                            //: sortColumnIndex == 6 ? c.Remark
                                                            : c.Trainee?.str_Staff_Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered;
                var domainName = Request.Url.Authority;
                var result = from c in displayed
                             let Grade = GetGrade(c.grade.HasValue ? c.grade : -1)
                             let codecertificate = c.certificatefinal
                             let checkstatus = c.statusCertificate.HasValue ? c.statusCertificate : null
                             let Path = c?.Path
                             select new object[] {
                                                string.Empty,
                                                c.Trainee?.str_Staff_Id,
                                                ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
                                                c.Trainee?.Department?.Name,
                                                DateUtil.DateToString(c.Course?.StartDate, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course?.EndDate, "dd/MM/yyyy"),
                                                c.point.HasValue ? string.Format("{0:0.#}", c.point) : "",
                                                Grade,
                                                Grade == "Fail" ? "" :  codecertificate,

                                                  !string.IsNullOrEmpty(codecertificate)
                            ? ( !string.IsNullOrEmpty(Path) && checkstatus == 0 ? "<a  href='"+ ConfigurationSettings.AppSettings["AWSLinkS3"] +c?.Path+"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>"  : "")
                            : "",
                                                   checkstatus.HasValue ? (checkstatus == 0 ? "<span class='label label-success'>Have a certificate</span>" : "<span class='label label-warning'>Have been revoked</span>")  : ""  ,


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

        /// <summary>
        /// The AjaxHandlerSubjectCertificateCompleted.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectCertificateCompleted(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                int SubjectListID = string.IsNullOrEmpty(Request.QueryString["SubjectListID"]) ? -1 : Convert.ToInt32(Request.QueryString["SubjectListID"].Trim());
                string fTraineeEID = string.IsNullOrEmpty(Request.QueryString["fillter_eid1"]) ? "" : Request.QueryString["fillter_eid1"].Trim();
                string fName = string.IsNullOrEmpty(Request.QueryString["fillter_name1"]) ? "" : Request.QueryString["fillter_name1"].Trim();
                string fCertificate = string.IsNullOrEmpty(Request.QueryString["fillter_certificate1"]) ? "" : Request.QueryString["fillter_certificate1"].Trim();
                int Status = string.IsNullOrEmpty(Request.QueryString["status_subject1"]) ? -1 : Convert.ToInt32(Request.QueryString["status_subject1"].Trim());
                string fSearchDate_fromCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_FromCreate1"]) ? "" : Request.QueryString["fSearchDate_FromCreate1"].ToString();
                string fSearchDate_toCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_toCreate1"]) ? "" : Request.QueryString["fSearchDate_toCreate1"].ToString();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_fromCreate, out dateFrom);
                DateTime.TryParse(fSearchDate_toCreate, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                //var courseDetail = CourseDetailService.Get(a => a.SubjectDetailId == SubjectListID).Select(a => (int?)a.Id);
                var modeltemp = CourseService.GetCourseResult(a =>
                (SubjectListID == -1 && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue && string.IsNullOrEmpty(fTraineeEID) && string.IsNullOrEmpty(fName) && string.IsNullOrEmpty(fCertificate) ? a.Course_Detail.Course.StartDate >= timenow : true) &&
                a.StatusCertificate == Status
                && (SubjectListID == -1 || a.Course_Detail.SubjectDetailId == SubjectListID)
                && (dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateAt.Value) <= 0)
                && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateAt.Value, dateTo) <= 0)
                 && (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID))
                && (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName))
                && (string.IsNullOrEmpty(fCertificate) || a.CertificateSubject.Contains(fCertificate))
                && a.Course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve
                && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult));


                //var modeltemp = CourseMemberService.Get(a => data.Contains(a.Id)
                //&& (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID))
                //&& (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName))
                //&& (string.IsNullOrEmpty(fCertificate) || a.Course_Result.Any(x => x.CertificateSubject.Contains(fCertificate)))
                //&& (/*!courseDetail.Any() || courseDetail.Contains((int)a.Course_Details_Id) && */ a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)));

                var model = modeltemp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = modeltemp.Count();
                IEnumerable<Course_Result> filtered = model;

                //filtered = model.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                //{
                //    TraineeCode = a.Trainee?.str_Staff_Id,
                //    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                //    DepartmentName = a.Trainee?.Department?.Name,
                //    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                //    Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                //    TraineeId = a.Trainee?.Id ?? -1,

                //    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    codecertificate = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateSubject,
                //    checkstatus = a.Course_Detail.Course_Result.FirstOrDefault(x => x.TraineeId == a.Member_Id).StatusCertificate.HasValue ? a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.StatusCertificate : null,
                //    Path = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Path ?? "",
                //});
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                            : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                            : sortColumnIndex == 4 ? DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy")
                                                            : sortColumnIndex == 5 ? ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c)
                                                            //: sortColumnIndex == 6 ? c.Remark
                                                            : sortColumnIndex == 6 ? ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c)
                                                            : sortColumnIndex == 8 ? c?.CertificateSubject
                                                            : c.Trainee?.str_Staff_Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered;
                var domainName = Request.Url.Authority;
                var result = from c in displayed
                             let codecertificate = c?.CertificateSubject
                             let checkstatus = c.StatusCertificate.HasValue ? c?.StatusCertificate : null
                             let Path = c?.Path
                             select new object[] {
                                            string.Empty,
                                            c?.Trainee?.str_Staff_Id,
                                            ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
                                            c.Trainee?.Department?.Name,
                                            DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),

                                            ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c),
                                            ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c),
                                            returnpointgrade(2, c?.TraineeId, c?.CourseDetailId),
                                            codecertificate,

                                            !string.IsNullOrEmpty(codecertificate)
                                                ? ( !string.IsNullOrEmpty(Path) && checkstatus == 0 ? "<a  href='"+ ConfigurationSettings.AppSettings["AWSLinkS3"] + Path +"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>"  : "")
                                                : "",
                                            checkstatus.HasValue ? (checkstatus == 0 ? "<span class='label label-success'>Have a certificate</span>" : "<span class='label label-warning'>Have been revoked</span>")  : "" ,
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerSubjectCertificateCompleted", ex.Message);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The filterSubject.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult filterSubject(FormCollection form)
        {
            StringBuilder HTML = new StringBuilder();

            string fCCode = string.IsNullOrEmpty(form["CourseCode"]) ? "" : form["CourseCode"].ToString();
            string fCName = string.IsNullOrEmpty(form["CourseName"]) ? "" : form["CourseName"].ToString();
            string fCode = string.IsNullOrEmpty(form["SubjectCode"]) ? "" : form["SubjectCode"].ToString();
            string fName = string.IsNullOrEmpty(form["SubjectName"]) ? "" : form["SubjectName"].ToString();

            var data = _repoSubject.GetSubjectDetail(a => a.Course_Detail.Any(x => x.Course_Result.Any()) && !String.IsNullOrEmpty(a.CertificateCode) && (String.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) &&
                        (String.IsNullOrEmpty(fName) || a.Name.Contains(fName)) && (string.IsNullOrEmpty(fCCode) || a.Course_Detail.Any(x => x.Course.Code.Contains(fCCode))) && (string.IsNullOrEmpty(fCName) || a.Course_Detail.Any(x => x.Course.Name.Contains(fCName)))).OrderByDescending(p => p.Id);

            if (data.Any())
            {
                HTML.Append("<option></option>");
                foreach (var item in data)
                {
                    HTML.AppendFormat("<option   value='{0}'>{1}</option>", item.Id, item.Code + "_" + item.Name);
                }
            }
            return Json(new
            {
                value_option = HTML.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        //LMS CALL
        /// <summary>
        /// The PluginInPutresult.
        /// </summary>
        /// <param name="idCouresDetails">The idCouresDetails<see cref="int"/>.</param>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult PluginInPutresult(int idCouresDetails, int courseId)
        {

            var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.SubjectResult, (int)UtilConstants.EStatus.Approve);
            var courses =
                CourseService.Get()
                    .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            var model = new CourseResultViewModel();
            model.ProcessStep = ProcessStep((int)UtilConstants.ApproveType.SubjectResult);
            model.Courses = courses;
            model.CourseId = courseId;
            model.CourseDetailId = idCouresDetails;
            //model.CourseDetailsByCourse =
            //    CourseDetailService.Get(
            //        a =>
            //            a.CourseId == courseId &&
            //            dataCourseProccess.Any(x => x.Course == a.Course))
            //        .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.SubjectDetail.Code, a.SubjectDetail.Name));
            var courseDetail = CourseDetailService.GetById(idCouresDetails);
            model.SubjectDetailId = (int)courseDetail.SubjectDetailId;
            model.CourseCode = courseDetail.Course.Code.Trim();
            model.CourseName = courseDetail.Course.Name.Trim();
            model.SubjectCode = courseDetail.SubjectDetail.Code.Trim();
            model.SubjectName = courseDetail.SubjectDetail.Name.Trim();
            model.RoomName = courseDetail.Room?.str_Name.Trim();
            model.Duration = courseDetail.SubjectDetail.Duration.ToString();
            model.DateFromTo = courseDetail.dtm_time_from?.ToString("dd/MM/yyy") + " - " +
            courseDetail.dtm_time_to?.ToString("dd/MM/yyyy");
            model.TypeLearning = TypeLearningIcon(courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline);
            model.MaxGrade = courseDetail.Course?.MaxGrade == null ? "100" : courseDetail.Course?.MaxGrade.ToString();

            var instructorAbility = courseDetail.Course_Detail_Instructor.ToList();
            var instructors = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor);
            var hannah = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah);
            var mentor = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor);
            model.Instructors = instructors.Select(b => ReturnDisplayLanguageCustom(b.Trainee.FirstName, b.Trainee.LastName, null, b.Duration.HasValue ? b.Duration.Value : 0)).Aggregate(model.Instructors, (current, fullName) => current + (fullName + "<br />"));
            model.Hannah = hannah.Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(model.Hannah, (current, fullName) => current + (fullName + "<br />"));
            model.Mentor = mentor.Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(model.Mentor, (current, fullName) => current + (fullName + "<br />"));
            //iscaculate
            model.IsCalculate = (bool)courseDetail.SubjectDetail.IsAverageCalculate;
            //check Approve Pending
            model.IsApproved =
                dataCourseProccess.Any(
                    a =>
                        a.int_courseDetails_Id == idCouresDetails &&
                        (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));


            //var ingredient = courseDetail.Course_Detail_Course_Ingredients.Where(a => a.IsActive == true && a.IsDeleted == false);
            //model.Ingredients =
            //    ingredient.ToDictionary(a => a.CourseIngredientId.Value, a => a.Course_Ingredients_Learning.Name);

            model.TypeLearningId = courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline;
            if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Online)
            {
                model.MarkType = (int)UtilConstants.MarkTypes.Auto;
            }
            else if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Offline || model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline)
            {
                if (courseDetail.mark_type.HasValue)
                {
                    if (courseDetail.mark_type == (int)UtilConstants.MarkTypes.Auto) // Auto không áp dụng cho khóa Pass/Fail
                    {
                        model.MarkType = courseDetail.SubjectDetail.IsAverageCalculate == true ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                    }
                    else
                    {
                        model.MarkType = (int)UtilConstants.MarkTypes.Manual;
                    }
                }
                else
                {
                    model.MarkType = model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                }
            }
            //model.PassScore = courseDetail.SubjectDetail?.Subject_Score?.OrderByDescending(a => a.point_from).FirstOrDefault().point_from;
            model.Members = courseDetail.TMS_Course_Member.Where(a => a.IsActive == true && a.IsDelete == false).Select(a => new CourseResultViewModel.Member()
            {
                Id = a.Id,

                TraineeId = a.Trainee?.Id,
                Code = a.Trainee?.str_Staff_Id,
                Name = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                DepartmentCode = a.Trainee?.Department?.Code,
                DepartmentName = a.Trainee?.Department?.Name,
                LearningTime = a.Course_Detail?.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) + " - " + a.Course_Detail?.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                Score = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Score?.ToString(),
                Result = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Result?.ToString(),
                Score_Re = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Score?.ToString(),
                Result_Re = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Result?.ToString(),
                Course_Result_Id = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Id,
                RealReResult = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Score?.ToString(),
                //Result = a.Course_Result.Any() ? "co" : "k",
                Remark = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Remark,
                Type = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Type ?? false,
                CheckFail = "<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + a.Id + ",this)'><i class='fas fa-edit " + (a.Course_Result?.Any(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id && b.Type == true) == true ? "highlight" : "") + " ' aria-hidden='true' ></i></a>"
                ,
                CheckBox = "<input onclick='CheckBoxClick(this)' type='CheckBox' id='" + a.Id + "' value='" + a.Id + "' />"
                //ResultIngredientses = a.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Course_Result_Course_Detail_Ingredient?.OrderBy(b => b.CourseDetailIngredient_Id).Select(c => new CourseResultViewModel.Member.ResultIngredient()
                //{
                //    IngredientCode = c.Course_Detail_Course_Ingredients?.Course_Ingredients_Learning?.Code,
                //    CourseDetailIngredient_Id = c.Course_Detail_Course_Ingredients.Id,
                //    CourseResult_Id = c.CourseResult_Id.Value,
                //    Score = c.Score.ToString()
                //})

            });
            //model.Ingredientses = courseDetail.Course_Detail_Course_Ingredients.Where(a => a.IsActive == true && a.IsDeleted == false).OrderBy(a => a.Id);
            return View(model);
        }

        /// <summary>
        /// The PluginModifyNote.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult PluginModifyNote(string id)
        {
            var prefix = "New_";
            var model = new CourseDetailNoteViewModel();
            var course = 0;
            if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix))
            {
                course = Convert.ToInt32(id.Replace(prefix, ""));
                var courseInfo = CourseService.GetById(course);
                model.CourseId = courseInfo.Id;
                model.Code = courseInfo.Code;
                model.TrainingId = courseInfo.Course_TrainingCenter.FirstOrDefault()?.khoidaotao_id;
            }
            else
            {
                var noteId = 0;
                Int32.TryParse(id, out noteId);
                var note = CourseDetailService.GetDetailSubjectNoteById(noteId);
                course = note?.Course_Detail.CourseId ?? 0;
                model.CourseId = course;
                model.Subject = note?.course_detail_id;
                model.Id = note?.id;
                model.Note = note?.note;
                model.Code = note?.Course_Detail?.Course?.Code;
                model.TrainingId = note?.Course_Detail.Course?.Course_TrainingCenter.FirstOrDefault()?.khoidaotao_id;
            }

            model.Courses =
                new SelectList(CourseService.Get().OrderBy(m => m.Id).ToList(),
                    "Id", "Name", course);
            model.TrainingCenters =
                new SelectList(_repoDepartment.Get().OrderBy(a => a.Ancestor), "Id",
                    "Name");
            return View(model);
        }

        /// <summary>
        /// The ShowRemark.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [AllowAnonymous]
        public string ShowRemark(int id)
        {
            var remark = "";
            var check = CourseDetailService.GetById(id)?.str_remark;
            remark = string.IsNullOrEmpty(check) ? "" : Regex.Replace(check, "[\r\n]", "<br/>");
            return remark;
        }

        /// <summary>
        /// The ResturnExpiredate.
        /// </summary>
        /// <param name="CourseDetails">The CourseDetails<see cref="Course_Detail"/>.</param>
        /// <param name="memberid">The memberid<see cref="int?"/>.</param>
        /// <param name="prevExdate">The prevExdate<see cref="DateTime?"/>.</param>
        /// <returns>The <see cref="DateTime?"/>.</returns>
        public DateTime? ResturnExpiredate(Course_Detail CourseDetails, int? memberid, DateTime? prevExdate)
        {

            DateTime? returnVal = null;
            if (CourseDetails == null) return null;
            //listCourseResult = listCourseResult.Where(a =>
            //    (a.Re_Check_Result == null ? a.First_Check_Result != "F" : a.Re_Check_Result != "F")
            //    &&
            //    a.Course_Detail.TMS_APPROVES.OrderByDescending(x=>x.id).FirstOrDefault(c => c.int_Type == (int)Constants.ApproveType.SubjectResult)
            //        .int_id_status == (int)Constants.EStatus.Approve).ToList();

            var subjectCode = CourseDetails.SubjectDetail.Code;
            var listresult = CourseService.GetCourseResult(a => a.TraineeId == memberid && a.Course_Detail.SubjectDetail.Code == subjectCode
                                                   && a.Course_Detail.dtm_time_from <= CourseDetails.dtm_time_from
                                                   && (a.Re_Check_Result == null ? (a.First_Check_Result != "F" && a.First_Check_Result != null) : a.Re_Check_Result != "F") && a.Course_Detail.TMS_APPROVES.OrderByDescending(x => x.id).FirstOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult).int_id_status == (int)UtilConstants.EStatus.Approve
                                                    );
            if (!listresult.Any()) return null;
            if (listresult.Count() > 1)
            {
                //lấy 2 phần tử cuối
                var courseResults = listresult.OrderByDescending(a => a.Course_Detail.dtm_time_from).Take(2);
                //DateTime? piriodDate = new DateTime();
                //DateTime? firstdate = new DateTime();
                //DateTime? lastdate = new DateTime();

                //-- debug
                //// Kq lần đầu
                if (listresult.Count() > 1)
                {
                    var lastOrDefault = courseResults.AsEnumerable().LastOrDefault();
                    if (lastOrDefault?.Course_Detail.dtm_time_to != null)
                    {
                        var fromdateLast = returnDateExpireTepm(lastOrDefault.Course_Detail.dtm_time_to.Value, lastOrDefault.Course_Detail.SubjectDetail.RefreshCycle.HasValue ? (int)lastOrDefault.Course_Detail.SubjectDetail.RefreshCycle : 0);
                        if (prevExdate == null)
                        {
                            prevExdate = fromdateLast;
                        }
                    }
                    var firstOrDefault = courseResults.FirstOrDefault();
                    if (firstOrDefault?.Course_Detail.dtm_time_from == null) return returnVal;
                    //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from.Value;
                    var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_to.Value;
                    var expiredate = prevExdate;
                    var expiredate3Month = expiredate?.AddMonths(-3);
                    returnVal = returnDateExpireTepm(fromdateFirst, firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle.HasValue ? (int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle : 0);

                    if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                    {
                        returnVal = expiredate?.AddMonths(firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle.HasValue ? (int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle : 0);
                    }
                }
                
            }
            else
            {
                var courseResult = listresult.FirstOrDefault();
                if (courseResult?.Course_Detail?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm(courseResult.Course_Detail.dtm_time_to.Value, courseResult.Course_Detail.SubjectDetail.RefreshCycle.HasValue ? (int)courseResult.Course_Detail.SubjectDetail.RefreshCycle : 0);
            }
            return returnVal;
        }

        /// <summary>
        /// The returnDateExpireTepm.
        /// </summary>
        /// <param name="fromdate">The fromdate<see cref="DateTime"/>.</param>
        /// <param name="cycle">The cycle<see cref="int"/>.</param>
        /// <returns>The <see cref="DateTime?"/>.</returns>
        private DateTime? returnDateExpireTepm(DateTime fromdate, int cycle)
        {
            if (cycle == 0) return null;
            return (new DateTime(fromdate.Year, fromdate.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(cycle);
        }

        /// <summary>
        /// The ShowRemarkAssign.
        /// </summary>
        /// <param name="courseID">The courseID<see cref="int?"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [AllowAnonymous]
        public string ShowRemarkAssign(int? courseID)
        {
            var item = _repoTmsApproves.Get(
                        a => a.int_Course_id == courseID && a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee).FirstOrDefault();
            var result = string.IsNullOrEmpty(item?.str_Remark) ? "" : item?.str_Remark;
            return result;
        }

        /// <summary>
        /// The ExportCertificate.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        [AllowAnonymous]
        public FileResult ExportCertificate(int id, string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate)
        {

            if (id != -1)
            {
                byte[] filecontent = ExportExcelCertificate(id, traineeEid, traineename, traineeCertificate, fSearchDateFromCreate, fSearchDatetoCreate);
                if (filecontent != null)
                {
                    return File(filecontent, ExportUtils.ExcelContentType, "CourseCertificate" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// The ExportExcelCertificate.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private byte[] ExportExcelCertificate(int id, string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate)
        {
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFromCreate, out dateFrom);
            DateTime.TryParse(fSearchDatetoCreate, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
            string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/CourseCertificate.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            var CourseRecord = _repoCourse.GetById(id);
            var courseApprove =

                   _repoTmsApproves.Get(
                       a =>
                           a.int_Course_id == id
                           && a.int_id_status == (int)UtilConstants.EStatus.Approve
                           && a.int_Type == (int)UtilConstants.ApproveType.CourseResult);
            var data = courseApprove.Any() ? CourseService.GetCourseResultFinal(a => a.statusCertificate == 0 && a.courseid == id && ((dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateDate.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateDate.Value, dateTo) <= 0)) && ((string.IsNullOrEmpty(traineeEid) || a.Trainee.str_Staff_Id.Contains(traineeEid)) && (string.IsNullOrEmpty(traineename) || a.Trainee.FirstName.Contains(traineename) || a.Trainee.LastName.Contains(traineename)) && (string.IsNullOrEmpty(traineeCertificate) || a.certificatefinal.Contains(traineeCertificate)))).GroupBy(v => new { v.traineeid, v.point, v.grade }).Select(c => c.FirstOrDefault()).ToList() : new List<Course_Result_Final>();

            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (CourseRecord != null)
            {
                ExcelPackage excelPackage;
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 6;

                    ExcelRange cellHeader = worksheet.Cells[startRow, 4];
                    cellHeader.Value = CourseRecord?.Name;
                    cellHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeader.Style.Font.Size = 12;

                    ExcelRange cellHeaderCourseCode = worksheet.Cells[startRow + 1, 4];
                    cellHeaderCourseCode.Value = CourseRecord?.Code;
                    cellHeaderCourseCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderCourseCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderCourseCode.Style.Font.Size = 12;

                    int count = 0;
                    startRow = 9;
                    int col = 2;
                    foreach (var item in data.OrderBy(a => a.Trainee.str_Staff_Id))
                    {
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);



                        ExcelRange cellStaffId = worksheet.Cells[startRow + 1, col + 1];
                        cellStaffId.Value = item?.Trainee?.str_Staff_Id;
                        cellStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellStaffId.Style.Font.Size = 13;




                        var fullName = ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee?.LastName);
                        var name = worksheet.Cells[startRow + 1, col + 1];
                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, col + 2];
                        cellFullName.Value = fullName;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellFullName.Style.Font.Size = 13;



                        ExcelRange cellJobtitle = worksheet.Cells[startRow + 1, col + 3];
                        cellJobtitle.Value = item?.Trainee?.JobTitle?.Name;
                        cellJobtitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellJobtitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellJobtitle.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellJobtitle.Style.Font.Size = 13;


                        ExcelRange cellDepartment = worksheet.Cells[startRow + 1, col + 4];
                        cellDepartment.Value = item?.Trainee?.Department?.Name;
                        cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellDepartment.Style.Font.Size = 13;
                        cellDepartment.AutoFitColumns();

                        ExcelRange cellPoint = worksheet.Cells[startRow + 1, col + 5];
                        cellPoint.Value = string.Format("{0:0.#}", item?.point);
                        cellPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellPoint.Style.Font.Size = 13;


                        ExcelRange cellGrade = worksheet.Cells[startRow + 1, col + 6];
                        cellGrade.Value = GetGrade(item.grade.HasValue ? item.grade : -1);
                        cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellGrade.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellGrade.Style.Font.Size = 13;


                        ExcelRange cellCertificate = worksheet.Cells[startRow + 1, col + 7];
                        cellCertificate.Value = item?.certificatefinal;
                        cellCertificate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCertificate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCertificate.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellCertificate.Style.Font.Size = 13;


                        startRow++;

                    }
                    var total = worksheet.Cells[startRow + 1, col + 6, startRow + 1, col + 7];
                    total.Merge = true;
                    ExcelRange cellcount = worksheet.Cells[startRow + 1, col + 6];
                    cellcount.Value = "Records:  " + count;
                    cellcount.Style.Font.Bold = true;
                    total.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    total.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    bytes = excelPackage.GetAsByteArray();

                }
            }
            return bytes;
        }

        /// <summary>
        /// The ExportCertificateSubject.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        [AllowAnonymous]
        public FileResult ExportCertificateSubject(int id, string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate)
        {

            if (id != -1)
            {
                byte[] filecontent = ExportExcelCertificateSubject(id, traineeEid, traineename, traineeCertificate, fSearchDateFromCreate, fSearchDatetoCreate);
                if (filecontent != null)
                {
                    return File(filecontent, ExportUtils.ExcelContentType, "SubjectCertificate" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// The ExportExcelCertificateSubject.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private byte[] ExportExcelCertificateSubject(int id, string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate)
        {
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFromCreate, out dateFrom);
            DateTime.TryParse(fSearchDatetoCreate, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
            var Subject = _repoSubject.GetById(id);
            var courseDetail = CourseDetailService.Get(a => a.SubjectDetailId == id).Select(a => a.Id);
            var data = CourseService.GetCourseResult(a => a.StatusCertificate == 0 && a.Subject_Id == id && ((dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateAt.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateAt.Value, dateTo) <= 0)) && a.Course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult))?.Select(a => a.inCourseMemberId);
            IEnumerable<AjaxCourseResultFinalModel> filtered = new List<AjaxCourseResultFinalModel>();
            if (data.Any() && courseDetail.Any())
            {
                var model = CourseService.GetTraineemember(a => data.Contains(a.Id) && ((string.IsNullOrEmpty(traineeEid) || a.Trainee.str_Staff_Id.Contains(traineeEid)) && (string.IsNullOrEmpty(traineename) || a.Trainee.FirstName.Contains(traineename) || a.Trainee.LastName.Contains(traineename)) && (string.IsNullOrEmpty(traineeCertificate) || a.Course_Result.Any(x => x.CertificateSubject.Contains(traineeCertificate)))) && courseDetail.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
                filtered = model.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                {
                    TraineeCode = a.Trainee?.str_Staff_Id,
                    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    DepartmentName = a.Trainee?.Department?.Code,
                    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                    Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                    TraineeId = a.Trainee?.Id ?? -1,
                    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                    codecertificate = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateSubject,
                    checkstatus = a.Course_Detail.Course_Result.FirstOrDefault(x => x.TraineeId == a.Member_Id).StatusCertificate.HasValue ? a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.StatusCertificate : null,
                    Path = a?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.Path?.ToString() ?? "",
                    CourseCode = a?.Course_Detail?.Course?.Code,
                });
            }
            DataTable dt = ExportUtils.ConvertToDatatable(filtered.ToArray());
            byte[] Bytes = null;
            int startrow = 7;
            string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/SubjectCertificate.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            MemoryStream MS = new MemoryStream();
            if (Subject != null)
            {
                ExcelPackage xlPackage;
                using (xlPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.First();
                    ExcelRange cellHeaderCourseName = worksheet.Cells[startrow, 3];
                    cellHeaderCourseName.Value = Subject.str_Name;

                    ExcelRange cellHeaderCourseCode = worksheet.Cells[startrow, 8];
                    cellHeaderCourseCode.Value = Subject.str_Code;

                    startrow = 10;
                    int row = 1;


                    int rowheader = 0;
                    int rowDetail = 0;
                    int count = 0;
                    if (filtered.Any())
                    {
                        foreach (var item1 in filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenBy(a => a.TraineeCode).ThenByDescending(a => a.FirstResultCertificate).ThenByDescending(a => a.ReResultCertificate).ThenBy(a => a.TraineeCode))
                        {
                            count++;
                            int col = 2;

                            var courseresult = CourseService.GetCourseResult(a => a.TraineeId == item1.TraineeId && a.CourseDetailId == item1.CourseDetailId).OrderByDescending(b => b.Id).FirstOrDefault();


                            ExcelRange cellNo = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //EID
                            ExcelRange cellEID = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellEID.Value = item1?.TraineeCode;
                            cellEID.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellEID.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //Full name
                            ExcelRange cellFullName = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellFullName.Value = item1?.FullName;

                            //dept
                            ExcelRange celdept = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celdept.Value = item1?.DepartmentName;
                            celdept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celdept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //course
                            ExcelRange cellCourse = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellCourse.Value = item1?.CourseCode;
                            cellCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //first check
                            ExcelRange celfirstcheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celfirstcheck.Value = item1?.FirstResultCertificate;
                            celfirstcheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celfirstcheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //re check
                            ExcelRange celrecheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celrecheck.Value = item1?.ReResultCertificate;
                            celrecheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celrecheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //grade
                            ExcelRange celgrade = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celgrade.Value = item1?.Grade;
                            celgrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celgrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //remark
                            ExcelRange celremark = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celremark.Value = (courseresult?.Remark != null ? courseresult?.Remark.Replace("!!!!!", Environment.NewLine) : "");
                            celgrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celremark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            celremark.Style.WrapText = true;

                            ExcelRange celCertificate = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celCertificate.Value = item1?.codecertificate;

                            rowDetail++;

                        }

                    }

                    //if (dt.Rows.Count > 0)
                    //{
                    using (ExcelRange r = worksheet.Cells[startrow - 1, 2, startrow + dt.Rows.Count, dt.Columns.Count - 11])
                    {
                        r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }
                    //}
                    Bytes = xlPackage.GetAsByteArray();
                }
            }
            return Bytes;
        }

        //private void buttom(string id)
        //{
        //    ViewBag.submitapproval = "<input type='button' class='btn btn-danger' id='submit1' value='Submit' onclick='test(1)' />";
        //    ViewBag.submit = "<input type='button' class='btn btn-danger' id='submit2' value='Save' onclick='test()' />";
        //    ViewBag.assigntrainee = "<input type='button'  onclick='nextStep()' id='assign' class='btn btn-danger' value='ASSIGN TRAINEE' />";
        //    if (id != null)
        //    {
        //        int courseid = Int32.Parse(id);
        //        var checkapproval = _repoTMS_APPROVES.Get(
        //                                a => a.int_Course_id == courseid &&
        //                                a.int_Type == (int)Constants.ApproveType.Course &&
        //                                (a.int_id_status == (int)Constants.EStatus.Approve || a.int_id_status == (int)Constants.EStatus.Pending)).Count();

        //        if (checkapproval != 1)
        //        {
        //            ViewBag.submitapproval = "<input type='button' class='btn btn-danger' id='submit1' value='Submit' onclick='save_edit(1)' />";
        //            ViewBag.submit = "<input type='button' class='btn btn-danger' id='submit2' value='Save' onclick='save_edit(0)' />";
        //            ViewBag.assigntrainee = "<input type='button'  onclick='nextStep()' id='assign' class='btn btn-danger' value='ASSIGN TRAINEE' />";
        //        }
        //        else
        //        {
        //            ViewBag.submitapproval = "";
        //            ViewBag.submit = "";
        //        }
        //    }
        //}
        /// <summary>
        /// The AjaxHandleListGlobalCetificate.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandleListGlobalCetificate(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                int Status = string.IsNullOrEmpty(Request.QueryString["status"]) ? -1 : Convert.ToInt32(Request.QueryString["status"].Trim());
                int Type = string.IsNullOrEmpty(Request.QueryString["type"]) ? -1 : Convert.ToInt32(Request.QueryString["type"].Trim());

                string fTraineeEID = string.IsNullOrEmpty(Request.QueryString["fillter_eid3"]) ? "" : Request.QueryString["fillter_eid3"].Trim();
                string fName = string.IsNullOrEmpty(Request.QueryString["fillter_name3"]) ? "" : Request.QueryString["fillter_name3"].Trim();
                string fCertificate = string.IsNullOrEmpty(Request.QueryString["fillter_certificate3"]) ? "" : Request.QueryString["fillter_certificate3"].Trim();

                string fSearchDate_fromCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_FromCreate3"]) ? "" : Request.QueryString["fSearchDate_FromCreate3"].ToString();
                string fSearchDate_toCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_toCreate3"]) ? "" : Request.QueryString["fSearchDate_toCreate3"].ToString();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_fromCreate, out dateFrom);
                DateTime.TryParse(fSearchDate_toCreate, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                IEnumerable<Course_Result_Final> filtered = new List<Course_Result_Final>();
                IEnumerable<Course_Result_Final> filteredtemp = new List<Course_Result_Final>();
                filteredtemp = CourseService.GetCourseResultFinal(a =>
                (dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue && string.IsNullOrEmpty(fTraineeEID) && string.IsNullOrEmpty(fName) && string.IsNullOrEmpty(fCertificate) ? a.Course.StartDate >= timenow : true) &&
                a.statusCertificate == Status && (dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateDate.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateDate.Value, dateTo) <= 0) && (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID)) && (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName)) && (string.IsNullOrEmpty(fCertificate) || a.certificatefinal.Contains(fCertificate)) && a.Course.TMS_APPROVES.Any(x => x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.CourseResult));

                filtered = filteredtemp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = filteredtemp.Count();

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Trainee?.Department?.Name
                                                            : c?.Trainee?.FirstName);

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered;

                var result = from c in displayed
                             select new object[] {
                                                 string.Empty,
                                                c?.Trainee?.str_Staff_Id,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                                c?.Trainee?.JobTitle?.Name,
                                                c?.Trainee?.Department?.Name,
                                                c?.Course?.Code,
                                                c?.point != 0 ? string.Format("{0:0.#}", c?.point) : "",
                                                GetGrade(c?.grade),
                                                c?.certificatefinal,
                                                c?.CreateCertificateDate?.ToString("dd/MM/yyyy") ?? "",
                                                //c?.statusCertificate == 0 ? "<span class='label label-success'>Have a certificate</span>" : "<span class='label label-warning'>Have been revoked</span>",
                        };


                var jsonResult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = tempcount,
                    iTotalDisplayRecords = tempcount,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
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

        /// <summary>
        /// The AjaxHandleListGlobalCetificate_.
        /// </summary>
        /// <param name="param">The param<see cref="jQueryDataTableParamModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult AjaxHandleListGlobalCetificate_(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                int Status = string.IsNullOrEmpty(Request.QueryString["status"]) ? -1 : Convert.ToInt32(Request.QueryString["status"].Trim());
                int Type = string.IsNullOrEmpty(Request.QueryString["type"]) ? -1 : Convert.ToInt32(Request.QueryString["type"].Trim());

                string fTraineeEID = string.IsNullOrEmpty(Request.QueryString["fillter_eid3"]) ? "" : Request.QueryString["fillter_eid3"].Trim();
                string fName = string.IsNullOrEmpty(Request.QueryString["fillter_name3"]) ? "" : Request.QueryString["fillter_name3"].Trim();
                string fCertificate = string.IsNullOrEmpty(Request.QueryString["fillter_certificate3"]) ? "" : Request.QueryString["fillter_certificate3"].Trim();

                string fSearchDate_fromCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_FromCreate3"]) ? "" : Request.QueryString["fSearchDate_FromCreate3"].ToString();
                string fSearchDate_toCreate = string.IsNullOrEmpty(Request.QueryString["fSearchDate_toCreate3"]) ? "" : Request.QueryString["fSearchDate_toCreate3"].ToString();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_fromCreate, out dateFrom);
                DateTime.TryParse(fSearchDate_toCreate, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                //var model = CourseService.GetTraineemember(a => a.Course_Result.Any(x => x.StatusCertificate == Status && (dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, x.CreateCertificateAt.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(x.CreateCertificateAt.Value, dateTo) <= 0) && x.Course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult)) &&
                // (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID)) && (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName)) && (string.IsNullOrEmpty(fCertificate) || a.Course_Result.Any(x => x.CertificateSubject.Contains(fCertificate))) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));

                var modeltemp = CourseService.GetCourseResult(a =>
               (dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue && string.IsNullOrEmpty(fTraineeEID) && string.IsNullOrEmpty(fName) && string.IsNullOrEmpty(fCertificate) ? a.Course_Detail.Course.StartDate >= timenow : true) &&
               a.StatusCertificate == Status
               && (dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateAt.Value) <= 0)
               && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateAt.Value, dateTo) <= 0)
                && (string.IsNullOrEmpty(fTraineeEID) || a.Trainee.str_Staff_Id.Contains(fTraineeEID))
               && (string.IsNullOrEmpty(fName) || a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName))
               && (string.IsNullOrEmpty(fCertificate) || a.CertificateSubject.Contains(fCertificate))
               && a.Course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve
               && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult));
                var model = modeltemp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = modeltemp.Count();
                IEnumerable<Course_Result> filtered = model;

                //IEnumerable<AjaxCourseResultFinalModel> filtered = NewMethod(model).Select(a => new AjaxCourseResultFinalModel
                //{
                //    TraineeCode = a.Trainee?.str_Staff_Id,
                //    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                //    DepartmentName = a.Trainee?.Department?.Code,
                //    DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                //    Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                //    FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                //    codecertificate = a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateSubject,
                //    CourseCode = a?.Course_Detail?.Course?.Code,
                //    createDate = a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CreateCertificateAt?.ToString("dd/MM/yyyy") ?? "",
                //    SubjectCode = a?.Course_Detail?.SubjectDetail?.Code,
                //});
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName)
                                                            : sortColumnIndex == 3 ? c.Trainee?.Department?.Name
                                                            : sortColumnIndex == 4 ? c?.Course_Detail?.Course?.Code
                                                            : sortColumnIndex == 5 ? c?.Course_Detail?.SubjectDetail?.Code
                                                            : sortColumnIndex == 6 ? DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy")
                                                            : sortColumnIndex == 7 ? ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c)
                                                            : sortColumnIndex == 8 ? ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c)
                                                            : sortColumnIndex == 9 ? returnpointgrade(2, c?.TraineeId, c?.CourseDetailId)
                                                             : sortColumnIndex == 10 ? c?.CertificateSubject
                                                            : c.Trainee?.str_Staff_Id);

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                //var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in filtered.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c?.Trainee?.str_Staff_Id,
                                                ReturnDisplayLanguage(c.Trainee?.FirstName, c.Trainee?.LastName),
                                                c?.Trainee?.Department?.Name,
                                                c?.Course_Detail?.Course?.Code,
                                                c?.Course_Detail?.SubjectDetail?.Code,
                                                DateUtil.DateToString(c.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                                ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c),
                                                ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c),
                                                returnpointgrade(2, c?.TraineeId, c?.CourseDetailId),
                                                c?.CertificateSubject,
                                                c?.CreateCertificateAt?.ToString("dd/MM/yyyy") ?? "",
                             };
                var jsonResult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = tempcount,
                    iTotalDisplayRecords = tempcount,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The NewMethod.
        /// </summary>
        /// <param name="model">The model<see cref="IQueryable{TMS_Course_Member}"/>.</param>
        /// <returns>The <see cref="IEnumerable{TMS_Course_Member}"/>.</returns>
        private static IEnumerable<TMS_Course_Member> NewMethod(IQueryable<TMS_Course_Member> model)
        {
            return model.AsEnumerable();
        }

        /// <summary>
        /// The ExportGlobalCertificate.
        /// </summary>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <param name="status">The status<see cref="int"/>.</param>
        /// <param name="type">The type<see cref="int"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        [AllowAnonymous]
        public FileResult ExportGlobalCertificate(string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate, int status, int type)
        {

            byte[] filecontent = ExportExcelGlobalCertificate(traineeEid, traineename, traineeCertificate, fSearchDateFromCreate, fSearchDatetoCreate, status, type);
            if (filecontent != null)
            {
                if (type == 1)
                {
                    return File(filecontent, ExportUtils.ExcelContentType, "CourseCertificate" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
                else if (type == 0)
                {
                    return File(filecontent, ExportUtils.ExcelContentType, "SubjectCertificate" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }

            }
            return null;
        }

        /// <summary>
        /// The ExportExcelGlobalCertificate.
        /// </summary>
        /// <param name="traineeEid">The traineeEid<see cref="string"/>.</param>
        /// <param name="traineename">The traineename<see cref="string"/>.</param>
        /// <param name="traineeCertificate">The traineeCertificate<see cref="string"/>.</param>
        /// <param name="fSearchDateFromCreate">The fSearchDateFromCreate<see cref="string"/>.</param>
        /// <param name="fSearchDatetoCreate">The fSearchDatetoCreate<see cref="string"/>.</param>
        /// <param name="status">The status<see cref="int"/>.</param>
        /// <param name="type">The type<see cref="int"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private byte[] ExportExcelGlobalCertificate(string traineeEid, string traineename, string traineeCertificate, string fSearchDateFromCreate, string fSearchDatetoCreate, int status, int type)
        {
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFromCreate, out dateFrom);
            DateTime.TryParse(fSearchDatetoCreate, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


            MemoryStream ms = new MemoryStream();
            ExcelPackage excelPackage;
            byte[] bytes = null;
            if (type == 1)
            {

                string templateFilePath = Server.MapPath("~/Template/ExcelFile/CourseCertificate.xlsx");
                FileInfo template = new FileInfo(templateFilePath);
                var data_ = CourseService.GetCourseResultFinal(a => a.statusCertificate == status && ((dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateDate.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateDate.Value, dateTo) <= 0)) && ((string.IsNullOrEmpty(traineeEid) || a.Trainee.str_Staff_Id.Contains(traineeEid)) && (string.IsNullOrEmpty(traineename) || a.Trainee.FirstName.Contains(traineename) || a.Trainee.LastName.Contains(traineename)) && (string.IsNullOrEmpty(traineeCertificate) || a.certificatefinal.Contains(traineeCertificate))) && a.Course.TMS_APPROVES.Any(x => x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.CourseResult));


                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 6;

                    int count = 0;
                    startRow = 9;
                    int col = 2;
                    if (data_.Any())
                    {
                        foreach (var item in data_.OrderBy(a => a.Trainee.str_Staff_Id))
                        {
                            count++;
                            ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);



                            ExcelRange cellStaffId = worksheet.Cells[startRow + 1, col + 1];
                            cellStaffId.Value = item?.Trainee?.str_Staff_Id;
                            cellStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellStaffId.Style.Font.Size = 13;




                            var fullName = ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee?.LastName);
                            var name = worksheet.Cells[startRow + 1, col + 1];
                            ExcelRange cellFullName = worksheet.Cells[startRow + 1, col + 2];
                            cellFullName.Value = fullName;
                            cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellFullName.Style.Font.Size = 13;



                            ExcelRange cellJobtitle = worksheet.Cells[startRow + 1, col + 3];
                            cellJobtitle.Value = item?.Trainee?.JobTitle?.Name;
                            cellJobtitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellJobtitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellJobtitle.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellJobtitle.Style.Font.Size = 13;


                            ExcelRange cellDepartment = worksheet.Cells[startRow + 1, col + 4];
                            cellDepartment.Value = item?.Trainee?.Department?.Name;
                            cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellDepartment.Style.Font.Size = 13;
                            cellDepartment.AutoFitColumns();

                            ExcelRange cellPoint = worksheet.Cells[startRow + 1, col + 5];
                            cellPoint.Value = string.Format("{0:0.#}", item?.point);
                            cellPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellPoint.Style.Font.Size = 13;


                            ExcelRange cellGrade = worksheet.Cells[startRow + 1, col + 6];
                            cellGrade.Value = GetGrade(item.grade.HasValue ? item.grade : -1);
                            cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellGrade.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellGrade.Style.Font.Size = 13;


                            ExcelRange cellCertificate = worksheet.Cells[startRow + 1, col + 7];
                            cellCertificate.Value = item?.certificatefinal;
                            cellCertificate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellCertificate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellCertificate.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellCertificate.Style.Font.Size = 13;
                            startRow++;
                        }
                    }

                    var total = worksheet.Cells[startRow + 1, col + 6, startRow + 1, col + 7];
                    total.Merge = true;
                    ExcelRange cellcount = worksheet.Cells[startRow + 1, col + 6];
                    cellcount.Value = "Records:  " + count;
                    cellcount.Style.Font.Bold = true;
                    total.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    total.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    bytes = excelPackage.GetAsByteArray();
                }
            }
            else if (type == 0)
            {
                var data = CourseService.GetCourseResult(a => a.StatusCertificate == status && ((dateFrom == DateTime.MinValue || DateTime.Compare(dateFrom, a.CreateCertificateAt.Value) <= 0) && (dateTo == DateTime.MinValue || DateTime.Compare(a.CreateCertificateAt.Value, dateTo) <= 0)) && a.Course_Detail.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve
                           && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult))?.Select(a => a.inCourseMemberId);
                IEnumerable<AjaxCourseResultFinalModel> filtered = new List<AjaxCourseResultFinalModel>();

                if (data.Any())
                {
                    var model = CourseService.GetTraineemember(
                    a =>
                        data.Contains(a.Id) &&
                        ((string.IsNullOrEmpty(traineeEid) || a.Trainee.str_Staff_Id.Contains(traineeEid)) &&
                         (string.IsNullOrEmpty(traineename) || a.Trainee.FirstName.Contains(traineename) || a.Trainee.LastName.Contains(traineename)) &&
                         (string.IsNullOrEmpty(traineeCertificate) || a.Course_Result.Any(x => x.CertificateSubject == traineeCertificate))) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
                    filtered = model.AsEnumerable().Select(a => new AjaxCourseResultFinalModel
                    {
                        TraineeCode = a.Trainee?.str_Staff_Id,
                        FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                        DepartmentName = a.Trainee?.Department?.Code,
                        DateFromTo = DateUtil.DateToString(a.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(a.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                        Grade = returnpointgrade(2, a?.Member_Id, a?.Course_Details_Id),
                        FirstResultCertificate = ReturnTraineePoint(true, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                        ReResultCertificate = ReturnTraineePoint(false, a?.Course_Detail?.SubjectDetail?.IsAverageCalculate, a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)),
                        codecertificate = a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CertificateSubject,
                        CourseCode = a?.Course_Detail?.Course?.Code,
                        createDate = a?.Course_Result?.FirstOrDefault(x => x.TraineeId == a.Member_Id)?.CreateCertificateAt?.ToString("dd/MM/yyyy") ?? "",
                        SubjectCode = a?.Course_Detail?.SubjectDetail?.Code,
                    }
                    );
                }

                DataTable dt = ExportUtils.ConvertToDatatable(filtered.ToArray());
                int startrow = 7;
                string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/SubjectCertificateGlobal.xlsx");
                FileInfo template = new FileInfo(templateFilePath);

                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                    startrow = 10;
                    int row = 1;


                    int rowheader = 0;
                    int rowDetail = 0;
                    int count = 0;
                    if (data.Any())
                    {
                        foreach (var item1 in filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenBy(a => a.TraineeCode).ThenByDescending(a => a.FirstResultCertificate).ThenByDescending(a => a.ReResultCertificate).ThenBy(a => a.TraineeCode))
                        {
                            count++;
                            int col = 2;
                            var courseresult = CourseService.GetCourseResult(a => a.TraineeId == item1.TraineeId && a.CourseDetailId == item1.CourseDetailId).OrderByDescending(b => b.Id).FirstOrDefault();


                            ExcelRange cellNo = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //EID
                            ExcelRange cellEID = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellEID.Value = item1?.TraineeCode;
                            cellEID.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellEID.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //Full name
                            ExcelRange cellFullName = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellFullName.Value = item1?.FullName;

                            //dept
                            ExcelRange celdept = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celdept.Value = item1?.DepartmentName;
                            celdept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celdept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //course
                            ExcelRange cellCourse = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellCourse.Value = item1?.CourseCode;
                            cellCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //course
                            ExcelRange cellSubject = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            cellSubject.Value = item1?.SubjectCode;
                            cellSubject.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellSubject.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //first check
                            ExcelRange celfirstcheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celfirstcheck.Value = item1?.FirstResultCertificate;
                            celfirstcheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celfirstcheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //re check
                            ExcelRange celrecheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celrecheck.Value = item1?.ReResultCertificate;
                            celrecheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celrecheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //grade
                            ExcelRange celgrade = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celgrade.Value = item1?.Grade;
                            celgrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celgrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //remark
                            ExcelRange celremark = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celremark.Value = (courseresult?.Remark != null
                                ? courseresult?.Remark.Replace("!!!!!", Environment.NewLine)
                                : "");
                            celgrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            celremark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            celremark.Style.WrapText = true;

                            ExcelRange celCertificate = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            celCertificate.Value = item1?.codecertificate;

                            rowDetail++;
                        }
                    }



                    //if (dt.Rows.Count > 0)
                    //{
                    using (
                        ExcelRange r =
                            worksheet.Cells[startrow - 1, 2, startrow + dt.Rows.Count, dt.Columns.Count - 11])
                    {
                        r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }
                    //}


                    bytes = excelPackage.GetAsByteArray();
                }
            }

            return bytes;
        }

        /// <summary>
        /// The Filtercourse_huy.
        /// </summary>
        /// <param name="form">The form<see cref="FormCollection"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [AllowAnonymous]
        public ActionResult Filtercourse_huy(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var html = new StringBuilder();
            var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();
            var fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
            var fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();

            DateTime fromDateFrom;
            DateTime toDateFrom;

            DateTime.TryParse(fSearchDateFrom, out fromDateFrom);
            DateTime.TryParse(fSearchDateTo, out toDateFrom);
            fromDateFrom = fromDateFrom != DateTime.MinValue ? fromDateFrom.Date : fromDateFrom;
            toDateFrom = toDateFrom != DateTime.MinValue ? toDateFrom.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : toDateFrom;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);

            var data = CourseService.Get(a =>
                        (string.IsNullOrEmpty(fCode) && fromDateFrom == DateTime.MinValue && toDateFrom == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                        (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)) &&
                        (fromDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", fromDateFrom, a.StartDate) >= 0) &&
                        (toDateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, toDateFrom) >= 0), true)
                        .OrderByDescending(p => p.Id);
            if (!data.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in data)
            {
                html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ExportFinalCourseResult.
        /// </summary>
        /// <param name="courseId">The courseId<see cref="int"/>.</param>
        /// <param name="txtCoursepass">The txtCoursepass<see cref="int"/>.</param>
        /// <param name="txtCoursedistinction">The txtCoursedistinction<see cref="int"/>.</param>
        /// <returns>The <see cref="FileContentResult"/>.</returns>
        [AllowAnonymous]
        public FileContentResult ExportFinalCourseResult(int courseId, int txtCoursepass, int txtCoursedistinction)
        {
            byte[] filecontent = ExportExcelFinalCourseResult(courseId, txtCoursepass, txtCoursedistinction);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "FinalCourseResult.xlsx");
            }
            return null;
        }

        //[AllowAnonymous]
        //private byte[] ExportExcelFinalCourseResult_(int courseList, int txtCoursepass, int txtCoursedistinction)
        //{
        //    CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        //    customCulture.NumberFormat.NumberDecimalSeparator = ".";
        //    System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        //    Course data_course = CourseService.GetById(courseList);
        //    var courseDetails = CourseDetailService.Get(a => a.CourseId == data_course.Id);
        //    var data_ = courseDetails.Select(b => b.Id).ToList();

        //    var data =
        //        CourseMemberService.Get(
        //            a =>
        //                data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved),true).OrderByDescending(a => a.Id);
        //    var verticalBar = GetByKey("VerticalBar");
        //    IEnumerable<AjaxFinalResultsModel> filtered = data.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault()).AsEnumerable().Select(c => new AjaxFinalResultsModel()
        //    {
        //        Id = (int)c.Member_Id,
        //        TraineeCode = c.Trainee.str_Staff_Id ?? "",
        //        ////FullName = c.Trainee.FirstName.Trim() + " " + c.Trainee.LastName.Trim(),
        //        FullName = ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName),
        //        JobTitleName = c.Trainee?.JobTitle?.Name ?? "",
        //        DepartmentName = c.Trainee?.Department?.Name ?? "",
        //        Point = GetResultFinal_Custom(UtilConstants.SwitchResult.Point,
        //          data_,
        //          c.Member_Id,
        //          txtCoursepass,
        //          txtCoursedistinction,
        //          c.Course_Detail.SubjectDetail.IsAverageCalculate),
        //        Grade = GetResultFinal_Custom(UtilConstants.SwitchResult.Grade,
        //          data_,
        //          c.Member_Id,
        //          txtCoursepass,
        //          txtCoursedistinction,
        //          c.Course_Detail.SubjectDetail.IsAverageCalculate),
        //        Action =
        //          "<input type='hidden' class='form-control' name='Course_Id' value='" +
        //          c.Course_Detail.CourseId + "'/>" + verticalBar +
        //          "<input type='hidden' class='form-control' name='Trainee_Id' value='" + c.Member_Id + "'/>" + verticalBar +
        //          InsertFinalID(c.Course_Detail.CourseId, c.Member_Id.Value),
        //        TraineeId = c?.Member_Id,
        //        remark = c?.Course_Detail.Course.Course_Result_Final.FirstOrDefault(m => m.traineeid == c.Member_Id)?.remark,
        //    });
        //    //.OrderByDescending(a => a.Grade == UtilConstants.Grade.Distinction.ToString()).ThenByDescending(a => a.Grade == UtilConstants.Grade.Pass.ToString()).ThenByDescending(a => a.Grade == UtilConstants.Grade.Fail.ToString()).ThenByDescending(a => a.Point);

        //    var subjectResult = CourseService.GetCourseResult(a => a.CourseDetailId.HasValue && data_.Contains(a.CourseDetailId.Value));
        //    byte[] Bytes = null;
        //    int startrow = 13;
        //    string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/FinalCourseResult.xlsx");//Server.MapPath(@"~/ExcelFile/FinalCourseResult.xlsx");
        //    FileInfo template = new FileInfo(templateFilePath);
        //    ExcelPackage xlPackage;
        //    MemoryStream MS = new MemoryStream();

        //    using (xlPackage = new ExcelPackage(template, false))
        //    {
        //        //int columnMerge1 = 5 + ((columnMerge <= 0 ? 1 : columnMerge) - 1);

        //        ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
        //        #region Write Header

        //        ExcelRange cellHeaderCourseName = worksheet.Cells[5, 3];
        //        cellHeaderCourseName.Value = data_course?.Name;
        //        cellHeaderCourseName.Style.Font.Bold = true;

        //        ExcelRange cellHeaderCourseCode = worksheet.Cells[6, 3];
        //        cellHeaderCourseCode.Value = data_course?.Code;
        //        cellHeaderCourseCode.Style.Font.Bold = true;

        //        ExcelRange cellHeaderSubjectName = worksheet.Cells[7, 3];
        //        cellHeaderSubjectName.Value = data_course?.Venue;
        //        cellHeaderSubjectName.Style.Font.Bold = true;

        //        ExcelRange cellHeaderDuration = worksheet.Cells[8, 3];
        //        cellHeaderDuration.Value = data_course?.StartDate.Value.ToString("dd/MM/yyyy") + " - " + data_course?.EndDate.Value.ToString("dd/MM/yyyy");
        //        cellHeaderDuration.Style.Font.Bold = true;
        //        #endregion

        //        #region Write Details

        //        #region Wirte Captions 

        //        int row_1 = 10;
        //        int row_1_point_5 = 11;
        //        int row_2 = 12;
        //        int col_default = 1;
        //        worksheet.Cells[row_1, col_default + 1, row_2, col_default + 1].Merge = true;
        //        ExcelRange cellNO = worksheet.Cells[row_1, col_default + 1];
        //        cellNO.Value = "No.";
        //        cellNO.Style.Font.Bold = true;
        //        cellNO.Style.Font.Size = 11;
        //        cellNO.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + 2, row_2, col_default + 2].Merge = true;
        //        ExcelRange cellName = worksheet.Cells[row_1, col_default + 2];
        //        cellName.Value = "Name";
        //        cellName.Style.Font.Bold = true;
        //        cellName.Style.Font.Size = 11;
        //        cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + 3, row_2, col_default + 3].Merge = true;
        //        ExcelRange cellStaff = worksheet.Cells[row_1, col_default + 3];
        //        cellStaff.Value = "EID";
        //        cellStaff.Style.Font.Bold = true;
        //        cellStaff.Style.Font.Size = 11;
        //        cellStaff.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + 4, row_2, col_default + 4].Merge = true;
        //        ExcelRange cellDept = worksheet.Cells[row_1, col_default + 4];
        //        cellDept.Value = "Dept";
        //        cellDept.Style.Font.Bold = true;
        //        cellDept.Style.Font.Size = 11;
        //        cellDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        //
        //        int genDynamicColumn = 5;

        //        foreach (var item in courseDetails)
        //        {
        //            worksheet.Cells[row_1_point_5, col_default + genDynamicColumn, row_1_point_5, col_default + genDynamicColumn + 1].Merge = true;
        //            ExcelRange CellName = worksheet.Cells[row_1_point_5, col_default + genDynamicColumn];
        //            CellName.Value = item?.SubjectDetail?.Name;
        //            CellName.Style.Font.Bold = true;
        //            CellName.Style.Font.Size = 11;
        //            CellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //            ExcelRange cellpointdynamic = worksheet.Cells[row_2, col_default + genDynamicColumn];
        //            cellpointdynamic.Value = "First Check";
        //            cellpointdynamic.Style.Font.Bold = true;
        //            cellpointdynamic.Style.Font.Size = 11;
        //            cellpointdynamic.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            genDynamicColumn++;
        //            ExcelRange cellpointdynamicRe = worksheet.Cells[row_2, col_default + genDynamicColumn];
        //            cellpointdynamicRe.Value = "ReCheck";
        //            cellpointdynamicRe.Style.Font.Bold = true;
        //            cellpointdynamicRe.Style.Font.Size = 11;
        //            cellpointdynamicRe.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            genDynamicColumn++;
        //        }

        //        genDynamicColumn--;


        //        worksheet.Cells[row_1, col_default + 5, row_1, col_default + genDynamicColumn].Merge = true;
        //        ExcelRange cellpoint = worksheet.Cells[row_1, col_default + 5];
        //        cellpoint.Value = "Points";
        //        cellpoint.Style.Font.Bold = true;
        //        cellpoint.Style.Font.Size = 11;
        //        cellpoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + genDynamicColumn + 1, row_2, col_default + genDynamicColumn + 1].Merge = true;
        //        ExcelRange cellAVG = worksheet.Cells[row_1, col_default + genDynamicColumn + 1];
        //        cellAVG.Value = "Average Point";
        //        cellAVG.Style.Font.Bold = true;
        //        cellAVG.Style.Font.Size = 11;
        //        cellAVG.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + genDynamicColumn + 2, row_2, col_default + genDynamicColumn + 2].Merge = true;
        //        ExcelRange cellGrade = worksheet.Cells[row_1, col_default + genDynamicColumn + 2];
        //        cellGrade.Value = "Grade";
        //        cellGrade.Style.Font.Bold = true;
        //        cellGrade.Style.Font.Size = 11;
        //        cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[row_1, col_default + genDynamicColumn + 3, row_2, col_default + genDynamicColumn + 3].Merge = true;
        //        ExcelRange cellRemark = worksheet.Cells[row_1, col_default + genDynamicColumn + 3];
        //        cellRemark.Value = "Remarks";
        //        cellRemark.Style.Font.Bold = true;
        //        cellRemark.Style.Font.Size = 11;
        //        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


        //        #endregion
        //        int row = 1;

        //        int x = 0;
        //        foreach (var item in filtered.OrderByDescending(a => a.Grade).ThenByDescending(p => p.Point))
        //        {
        //            row = startrow + x;
        //            int genCols = 1;
        //            ExcelRange cellRowNo = worksheet.Cells[row, col_default + genCols];
        //            cellRowNo.Value = x + 1;
        //            cellRowNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            cellRowNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;

        //            //Trainee Name
        //            ExcelRange cellTraineeName = worksheet.Cells[row, col_default + genCols];
        //            cellTraineeName.Value = item?.FullName;
        //            cellTraineeName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //            cellTraineeName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;


        //            //Trainee ID
        //            ExcelRange cellEid = worksheet.Cells[row, col_default + genCols];
        //            cellEid.Value = item?.TraineeCode;
        //            cellEid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            cellEid.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;


        //            //Trainee Department
        //            ExcelRange cellTraineeDept = worksheet.Cells[row, col_default + genCols];
        //            cellTraineeDept.Value = item?.DepartmentName;
        //            cellTraineeDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            cellTraineeDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;


        //            //Result
        //            int dynamicCol = 5;
        //            var traineeResult = subjectResult.Where(a => a.TraineeId == item.Id);
        //            foreach (var detail in courseDetails)
        //            {
        //                var resultbycourseDetail = traineeResult.FirstOrDefault(a => a.CourseDetailId == detail.Id);

        //                ExcelRange cellTraineeScore = worksheet.Cells[row, col_default + dynamicCol];
        //                cellTraineeScore.Value = detail.SubjectDetail.IsAverageCalculate ==false ? resultbycourseDetail?.First_Check_Result : resultbycourseDetail?.First_Check_Score?.ToString().Replace("-1","0");
        //                cellTraineeScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                cellTraineeScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                dynamicCol++;
        //                genCols++;

        //                ExcelRange cellTraineeRescore = worksheet.Cells[row, col_default + dynamicCol];
        //                cellTraineeRescore.Value = detail.SubjectDetail.IsAverageCalculate == false ? resultbycourseDetail?.Re_Check_Result : resultbycourseDetail?.Re_Check_Score?.ToString().Replace("-1", "0");
        //                cellTraineeRescore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                cellTraineeRescore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                dynamicCol++;
        //                genCols++;

        //            }

        //            //Trainee Average Point
        //            ExcelRange cellTraineeAvgPoint = worksheet.Cells[row, col_default + genCols];
        //            cellTraineeAvgPoint.Value = item.Point; //String.Format("{0:0.#}", item.point)
        //            cellTraineeAvgPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            cellTraineeAvgPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;

        //            //Trainee Grade
        //            ExcelRange cellTraineeGrade = worksheet.Cells[row, col_default + genCols];
        //            cellTraineeGrade.Value = item.Grade;
        //            cellTraineeGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            cellTraineeGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            genCols++;
        //            x++;
        //        }

        //        #endregion

        //        #region Write Footer
        //        ExcelRange cell = worksheet.Cells[startrow + x + 1, genDynamicColumn + 3];
        //        cell.Value = "Number of certificates issued: " + filtered.Count(a => a.Grade != UtilConstants.Grade.Fail.ToString());//count.ToString();
        //        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        //        worksheet.Cells[startrow + x + 3, 1 + 1, startrow + x + 3, 1 + 2].Merge = true;
        //        ExcelRange cellfooterleft1 = worksheet.Cells[startrow + x + 3, 1 + 1];
        //        cellfooterleft1.Value = "Approved by";
        //        worksheet.Cells[startrow + x + 3, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        worksheet.Cells[startrow + x + 3, 1 + 1].Style.Font.Size = 11;

        //        worksheet.Cells[startrow + x + 4, 1 + 1, startrow + x + 4, 1 + 2].Merge = true;
        //        ExcelRange cellfooterleft2 = worksheet.Cells[startrow + x + 4, 1 + 1];
        //        cellfooterleft2.Value = "HEAD OF TRAINING";
        //        worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Bold = true;
        //        worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Size = 12;
        //        worksheet.Cells[startrow + x + 4, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[startrow + x + 5, 1 + 1, startrow + x + 5, 1 + 2].Merge = true;
        //        ExcelRange cellfooterleft3 = worksheet.Cells[startrow + x + 5, 1 + 1];
        //        cellfooterleft3.Value = "Date .....................................";
        //        worksheet.Cells[startrow + x + 5, 1 + 1].Style.Font.Size = 12;
        //        worksheet.Cells[startrow + x + 5, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


        //        worksheet.Cells[startrow + x + 3, genDynamicColumn + 2, startrow + x + 3, genDynamicColumn + 3].Merge = true;
        //        ExcelRange cellfooterright1 = worksheet.Cells[startrow + x + 3, genDynamicColumn + 2];
        //        cellfooterright1.Value = "Prepared by";
        //        cellfooterright1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        cellfooterright1.Style.Font.Size = 11;

        //        worksheet.Cells[startrow + x + 4, genDynamicColumn + 2, startrow + x + 4, genDynamicColumn + 3].Merge = true;
        //        ExcelRange cellfooterright2 = worksheet.Cells[startrow + x + 4, genDynamicColumn + 2];
        //        cellfooterright2.Value = "VTC TRAINING MANAGER";
        //        cellfooterright2.Style.Font.Bold = true;
        //        cellfooterright2.Style.Font.Size = 12;
        //        cellfooterright2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        worksheet.Cells[startrow + x + 5, genDynamicColumn + 2, startrow + x + 5, genDynamicColumn + 3].Merge = true;
        //        ExcelRange cellfooterright3 = worksheet.Cells[startrow + x + 5, genDynamicColumn + 2];
        //        cellfooterright3.Value = " Date .....................................";
        //        cellfooterright3.Style.Font.Size = 12;
        //        cellfooterright3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;






        //        #endregion

        //        #region[header]
        //        //using (ExcelRange r = worksheet.Cells[2, 1 + 1, 4, dt.Columns.Count - 1 + 1])
        //        //{
        //        //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //        //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
        //        //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
        //        //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
        //        //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
        //        //}
        //        using (ExcelRange r = worksheet.Cells[row_1, 1 + 1, startrow + x - 1, col_default + genDynamicColumn + 3])
        //        {
        //            r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //            r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //            r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //            r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
        //            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
        //            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
        //            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
        //        }
        //        //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Merge = true;
        //        worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

        //        worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Merge = true;
        //        worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;


        //        worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Merge = true;
        //        worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //        //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //        ExcelRange celltitleheader = worksheet.Cells[2, 4];
        //        celltitleheader.Value = "FINAL COURSE RESULT";
        //        celltitleheader.Style.Font.Bold = true;
        //        celltitleheader.Style.Font.Size = 15;
        //        worksheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        worksheet.Cells[2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


        //        string[] header = GetByKey("Final_Course_Result_Header").Split(',');
        //        ExcelRange celltitleheader3 = worksheet.Cells[2, genDynamicColumn + 3];
        //        celltitleheader3.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
        //        celltitleheader3.Style.Font.Size = 10;
        //        celltitleheader3.Style.WrapText = true;
        //        celltitleheader3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        //        celltitleheader3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


        //        #endregion
        //        Bytes = xlPackage.GetAsByteArray();
        //    }

        //    return Bytes;
        //}
        /// <summary>
        /// The ExportExcelFinalCourseResult.
        /// </summary>
        /// <param name="courseList">The courseList<see cref="int"/>.</param>
        /// <param name="txtCoursepass">The txtCoursepass<see cref="int"/>.</param>
        /// <param name="txtCoursedistinction">The txtCoursedistinction<see cref="int"/>.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        [AllowAnonymous]
        private byte[] ExportExcelFinalCourseResult(int courseList, int txtCoursepass, int txtCoursedistinction)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Course data_course = CourseService.GetById(courseList);
            var courseDetails = CourseDetailService.Get(a => a.CourseId == courseList);
            var data_ = courseDetails.Select(b => b.Id);
            var course_result = CourseService.GetCourseResult(a => data_.Contains((int)a.CourseDetailId));
            var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList && a.Trainee.TMS_Course_Member.Any(x => data_.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete == false && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)));

            //var data =
            //    CourseMemberService.Get(
            //        a =>
            //            data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(a => a.Id);
            var verticalBar = GetByKey("VerticalBar");
            var filtered = data.AsEnumerable().Select(c => new
            {
                Id = (int)c.traineeid,
                TraineeCode = c.Trainee.str_Staff_Id ?? "",
                ////FullName = c.Trainee.FirstName.Trim() + " " + c.Trainee.LastName.Trim(),
                FullName = ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName),
                DepartmentName = c.Trainee?.Department?.Name ?? "",
                Point = GetResultFinal_Custom(UtilConstants.SwitchResult.Point,
                  data_, course_result,
                  c.traineeid,
                  txtCoursepass,
                  txtCoursedistinction),
                Grade = GetResultFinal_Custom(UtilConstants.SwitchResult.Grade,
                  data_, course_result,
                  c.traineeid,
                  txtCoursepass,
                  txtCoursedistinction),
                TraineeId = c?.traineeid,
                remark = c?.remark,
            }).OrderByDescending(a => a.Grade == "Distinction").ThenByDescending(a => a.Grade == "Pass").ThenByDescending(a => a.Grade == "Fail").ThenByDescending(a => a.Point);

            //var subjectResult = CourseService.GetCourseResult(a => a.CourseDetailId.HasValue && data_.Contains(a.CourseDetailId.Value));
            byte[] Bytes = null;
            int startrow = 13;
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/FinalCourseResult.xlsx");//Server.MapPath(@"~/ExcelFile/FinalCourseResult.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();

            using (xlPackage = new ExcelPackage(template, false))
            {
                //int columnMerge1 = 5 + ((columnMerge <= 0 ? 1 : columnMerge) - 1);

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];

                ExcelRange cellHeaderCourseName = worksheet.Cells[5, 3];
                cellHeaderCourseName.Value = data_course?.Name;
                cellHeaderCourseName.Style.Font.Bold = true;

                ExcelRange cellHeaderCourseCode = worksheet.Cells[6, 3];
                cellHeaderCourseCode.Value = data_course?.Code;
                cellHeaderCourseCode.Style.Font.Bold = true;

                ExcelRange cellHeaderSubjectName = worksheet.Cells[7, 3];
                cellHeaderSubjectName.Value = data_course?.Venue;
                cellHeaderSubjectName.Style.Font.Bold = true;

                ExcelRange cellHeaderDuration = worksheet.Cells[8, 3];
                cellHeaderDuration.Value = data_course?.StartDate.Value.ToString("dd/MM/yyyy") + " - " + data_course?.EndDate.Value.ToString("dd/MM/yyyy");
                cellHeaderDuration.Style.Font.Bold = true;



                int row_1 = 10;
                int row_1_point_5 = 11;
                int row_2 = 12;
                int col_default = 1;
                worksheet.Cells[row_1, col_default + 1, row_2, col_default + 1].Merge = true;
                ExcelRange cellNO = worksheet.Cells[row_1, col_default + 1];
                cellNO.Value = "No.";
                cellNO.Style.Font.Bold = true;
                cellNO.Style.Font.Size = 11;
                cellNO.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellNO.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 2, row_2, col_default + 2].Merge = true;
                ExcelRange cellName = worksheet.Cells[row_1, col_default + 2];
                cellName.Value = "Name";
                cellName.Style.Font.Bold = true;
                cellName.Style.Font.Size = 11;
                cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 3, row_2, col_default + 3].Merge = true;
                ExcelRange cellStaff = worksheet.Cells[row_1, col_default + 3];
                cellStaff.Value = "EID";
                cellStaff.Style.Font.Bold = true;
                cellStaff.Style.Font.Size = 11;
                cellStaff.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellStaff.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 4, row_2, col_default + 4].Merge = true;
                ExcelRange cellDept = worksheet.Cells[row_1, col_default + 4];
                cellDept.Value = "Dept.";
                cellDept.Style.Font.Bold = true;
                cellDept.Style.Font.Size = 11;
                cellDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //
                int genDynamicColumn = 5;

                foreach (var item in courseDetails)
                {
                    worksheet.Cells[row_1_point_5, col_default + genDynamicColumn, row_1_point_5, col_default + genDynamicColumn + 1].Merge = true;
                    ExcelRange CellName = worksheet.Cells[row_1_point_5, col_default + genDynamicColumn];
                    CellName.Value = item?.SubjectDetail?.Name;
                    CellName.Style.Font.Bold = true;
                    CellName.Style.Font.Size = 11;
                    CellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    CellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellpointdynamic = worksheet.Cells[row_2, col_default + genDynamicColumn];
                    cellpointdynamic.Value = "First Attempt";
                   // cellpointdynamic.Style.Font.Bold = true;
                    cellpointdynamic.Style.Font.Size = 11;
                    cellpointdynamic.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellpointdynamic.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    genDynamicColumn++;
                    ExcelRange cellpointdynamicRe = worksheet.Cells[row_2, col_default + genDynamicColumn];
                    cellpointdynamicRe.Value = "Second Attempt";
                    //cellpointdynamicRe.Style.Font.Bold = true;
                    cellpointdynamicRe.Style.Font.Size = 11;
                    cellpointdynamicRe.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellpointdynamicRe.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genDynamicColumn++;
                }

                genDynamicColumn--;


                worksheet.Cells[row_1, col_default + 5, row_1, col_default + genDynamicColumn].Merge = true;
                ExcelRange cellpoint = worksheet.Cells[row_1, col_default + 5];
                cellpoint.Value = "Result";
                cellpoint.Style.Font.Bold = true;
                cellpoint.Style.Font.Size = 11;
                cellpoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellpoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 1, row_2, col_default + genDynamicColumn + 1].Merge = true;
                ExcelRange cellAVG = worksheet.Cells[row_1, col_default + genDynamicColumn + 1];
                cellAVG.Value = "Average Score";
                cellAVG.Style.Font.Bold = true;
                cellAVG.Style.Font.Size = 11;
                cellAVG.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellAVG.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 2, row_2, col_default + genDynamicColumn + 2].Merge = true;
                ExcelRange cellGrade = worksheet.Cells[row_1, col_default + genDynamicColumn + 2];
                cellGrade.Value = "Grade";
                cellGrade.Style.Font.Bold = true;
                cellGrade.Style.Font.Size = 11;
                cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 3, row_2, col_default + genDynamicColumn + 3].Merge = true;
                ExcelRange cellRemark = worksheet.Cells[row_1, col_default + genDynamicColumn + 3];
                cellRemark.Value = "Remarks";
                cellRemark.Style.Font.Bold = true;
                cellRemark.Style.Font.Size = 11;
                cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                int row = 1;

                int x = 0;
                foreach (var item in filtered.OrderByDescending(a => a.Grade == "Distinction").ThenByDescending(a => a.Grade == "Pass").ThenByDescending(a => a.Grade == "Fail").ThenByDescending(a => a.Point))
                {
                    row = startrow + x;
                    int genCols = 1;
                    ExcelRange cellRowNo = worksheet.Cells[row, col_default + genCols];
                    cellRowNo.Value = x + 1;
                    cellRowNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellRowNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;

                    //Trainee Name
                    ExcelRange cellTraineeName = worksheet.Cells[row, col_default + genCols];
                    cellTraineeName.Value = item?.FullName;
                    cellTraineeName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellTraineeName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Trainee ID
                    ExcelRange cellEid = worksheet.Cells[row, col_default + genCols];
                    cellEid.Value = item?.TraineeCode;
                    cellEid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellEid.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Trainee Department
                    ExcelRange cellTraineeDept = worksheet.Cells[row, col_default + genCols];
                    cellTraineeDept.Value = item?.DepartmentName;
                    cellTraineeDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Result
                    int dynamicCol = 5;
                    //var traineeResult = subjectResult.Where(a => a.TraineeId == item.Id);
                    foreach (var detail in courseDetails)
                    {
                        var resultbycourseDetail = detail.Course_Result.FirstOrDefault(a => a.CourseDetailId == detail.Id && a.TraineeId == item.TraineeId);

                        ExcelRange cellTraineeScore = worksheet.Cells[row, col_default + dynamicCol];
                        cellTraineeScore.Value = detail.SubjectDetail.IsAverageCalculate == false ? resultbycourseDetail?.First_Check_Result : resultbycourseDetail?.First_Check_Score?.ToString().Replace("-1", "0");
                        cellTraineeScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        dynamicCol++;
                        genCols++;

                        ExcelRange cellTraineeRescore = worksheet.Cells[row, col_default + dynamicCol];
                        cellTraineeRescore.Value = detail.SubjectDetail.IsAverageCalculate == false ? resultbycourseDetail?.Re_Check_Result : resultbycourseDetail?.Re_Check_Score?.ToString().Replace("-1", "0");
                        cellTraineeRescore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeRescore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        dynamicCol++;
                        genCols++;

                    }

                    //Trainee Average Point
                    ExcelRange cellTraineeAvgPoint = worksheet.Cells[row, col_default + genCols];
                    cellTraineeAvgPoint.Value = item.Point; //String.Format("{0:0.#}", item.point)
                    cellTraineeAvgPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeAvgPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;

                    //Trainee Grade
                    ExcelRange cellTraineeGrade = worksheet.Cells[row, col_default + genCols];
                    cellTraineeGrade.Value = item.Grade;
                    cellTraineeGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;
                    x++;
                }

                ExcelRange cell = worksheet.Cells[startrow + x + 1, genDynamicColumn + 3];
                cell.Value = "Number of certificates to be issued: " + filtered.Count(a => a.Grade != "Fail");//count.ToString();
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[startrow + x + 3, 1 + 1, startrow + x + 3, 1 + 2].Merge = true;
                ExcelRange cellfooterleft1 = worksheet.Cells[startrow + x + 3, 1 + 1];
                cellfooterleft1.Value = "Approved by";
                worksheet.Cells[startrow + x + 3, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[startrow + x + 3, 1 + 1].Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, 1 + 1, startrow + x + 4, 1 + 2].Merge = true;
                ExcelRange cellfooterleft2 = worksheet.Cells[startrow + x + 4, 1 + 1];
                cellfooterleft2.Value = "HEAD OF TRAINING";
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Bold = true;
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Size = 12;
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, 1 + 1, startrow + x + 8, 1 + 2].Merge = true;
                ExcelRange cellfooterleft3 = worksheet.Cells[startrow + x + 8, 1 + 1];
                cellfooterleft3.Value = "Date .....................................";
                worksheet.Cells[startrow + x + 5, 1 + 1].Style.Font.Size = 12;
                worksheet.Cells[startrow + x + 5, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                // Verified by
                worksheet.Cells[startrow + x + 3, genDynamicColumn / 2 + 2, startrow + x + 3, genDynamicColumn / 2 + 3].Merge = true;
                ExcelRange cellfootercenter1 = worksheet.Cells[startrow + x + 3, genDynamicColumn / 2 + 2];
                cellfootercenter1.Value = "Verified by";
                cellfootercenter1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfootercenter1.Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, genDynamicColumn / 2 + 2, startrow + x + 4, genDynamicColumn / 2 + 3].Merge = true;
                ExcelRange cellfootercenter2 = worksheet.Cells[startrow + x + 4, genDynamicColumn / 2 + 2];
                cellfootercenter2.Value = "VTC TRAINING MANAGER";
                cellfootercenter2.Style.Font.Bold = true;
                cellfootercenter2.Style.Font.Size = 12;
                cellfootercenter2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, genDynamicColumn / 2 + 2, startrow + x + 8, genDynamicColumn / 2 + 3].Merge = true;
                ExcelRange cellfootercenter3 = worksheet.Cells[startrow + x + 8, genDynamicColumn / 2 + 2];
                cellfootercenter3.Value = " Date .....................................";
                cellfootercenter3.Style.Font.Size = 12;
                cellfootercenter3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;



                worksheet.Cells[startrow + x + 3, genDynamicColumn + 2, startrow + x + 3, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright1 = worksheet.Cells[startrow + x + 3, genDynamicColumn + 2];
                cellfooterright1.Value = "Prepared by";
                cellfooterright1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfooterright1.Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, genDynamicColumn + 2, startrow + x + 4, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright2 = worksheet.Cells[startrow + x + 4, genDynamicColumn + 2];
                cellfooterright2.Value = "ATM DEPARTMENT";
                cellfooterright2.Style.Font.Bold = true;
                cellfooterright2.Style.Font.Size = 12;
                cellfooterright2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, genDynamicColumn + 2, startrow + x + 8, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright3 = worksheet.Cells[startrow + x + 8, genDynamicColumn + 2];
                cellfooterright3.Value = " Date .....................................";
                cellfooterright3.Style.Font.Size = 12;
                cellfooterright3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //using (ExcelRange r = worksheet.Cells[2, 1 + 1, 4, dt.Columns.Count - 1 + 1])
                //{
                //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                //}
                using (ExcelRange r = worksheet.Cells[row_1, 1 + 1, startrow + x - 1, col_default + genDynamicColumn + 3])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }
                //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Merge = true;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Merge = true;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;


                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Merge = true;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ExcelRange celltitleheader = worksheet.Cells[2, 4];
                celltitleheader.Value = "FINAL COURSE RESULT";
                celltitleheader.Style.Font.Bold = true;
                celltitleheader.Style.Font.Size = 15;
                worksheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                string[] header = GetByKey("Final_Course_Result_Header").Split(',');
                ExcelRange celltitleheader3 = worksheet.Cells[2, genDynamicColumn + 3];
                celltitleheader3.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                celltitleheader3.Style.Font.Size = 10;
                celltitleheader3.Style.WrapText = true;
                celltitleheader3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                celltitleheader3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                Bytes = xlPackage.GetAsByteArray();
            }

            return Bytes;
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult ChangeCourseReturnSubjectResult(int courseId, int? reportsubjectresult)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                var data = CourseDetailService.Get(a => a.CourseId == courseId);//
                if (data.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data.Where(a => (reportsubjectresult == 1 ? true : true)))
                    {
                        if ((bool)CourseDetailService.checkapproval(item, new[] { (int)UtilConstants.ApproveType.AssignedTrainee }))

                            html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.SubjectDetail.Name);
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/ChangeCourseReturnSubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
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
                var data = CourseDetailService.Get(a=>a.CourseId == courseId && a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult));
                //var data =
                //    ApproveService.Get(
                //        a => a.int_Course_id == courseId && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.Course_Detail.IsDeleted != true)
                //        .ToList();
                var verticalBar = GetByKey("VerticalBar");
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                IEnumerable<Course_Detail> filtered = data;
                Func<Course_Detail, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Course.Code
                                              : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                              : c?.dtm_time_from.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                        : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from a in displayed
                             let approve = a.TMS_APPROVES.FirstOrDefault()
                             select new object[] {
                       string.Empty,                    
                       ( a?.SubjectDetail?.IsActive != true ? "<span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ a?.SubjectDetail?.Code + "</span> - <span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ a?.SubjectDetail?.Name + "</span>" : a?.SubjectDetail?.Code + " - " + a?.SubjectDetail?.Name) + ReturnStatus(approve?.int_id_status),
                       ReturnDisplayLanguage(approve?.USER1?.LASTNAME, approve?.USER1?.FIRSTNAME),
                       approve?.Date_Requested?.ToString("dd/MM/yyyy"),                     
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
    }
}
