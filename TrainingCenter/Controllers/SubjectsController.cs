using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Jobtitle;
using TMS.Core.ViewModels.AjaxModels.AjaxSubject;

namespace TrainingCenter.Controllers
{
    using System.Text;
    using DAL.Entities;
    using TMS.Core.App_GlobalResources;
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
    using TMS.Core.ViewModels.Subjects;
    using TMS.Core.Services;
    using Utilities;
    using System.IO;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Configuration;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public class SubjectsController : BaseAdminController
    {
        // GET: Subjects
        private readonly ISubjectService _repoSubject;
        private readonly IJobtitleService _repoJobtitleService;
        private readonly IDepartmentService _repoDepartmentService;


        private const string TempSubjectScoreName = "SubjectScores";
        public SubjectsController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService repoSubject, IApproveService approveService, IJobtitleService repoJobtitleService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoSubject = repoSubject;
            _repoJobtitleService = repoJobtitleService;
            _repoDepartmentService = departmentService;
        }

        #region Export PDF
        // Export PDF
        public ActionResult GetDataPDF()
        {
            int ComOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
            int fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"]);
            int ddl_TYPE = string.IsNullOrEmpty(Request.QueryString["ddl_TYPE"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_TYPE"]);
            int fJobTitle = string.IsNullOrEmpty(Request.QueryString["fJobTitle"]) ? -1 : Convert.ToInt32(Request.QueryString["fJobTitle"].Trim());
            int fGender = string.IsNullOrEmpty(Request.QueryString["fGender"]) ? -1 : Convert.ToInt32(Request.QueryString["fGender"].Trim());
            //string url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().ToString();
            string fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].ToString();
            string fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? "" : Request.QueryString["fEmail"].ToString();
            string fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? "" : Request.QueryString["fPhone"].ToString();
            string fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].ToString();
            bool ss = true;
            if (ddl_TYPE == 0)
            {
                ss = false;
            }
            var data = new List<Trainee>();

            data = EmployeeService.Get(a => a.int_Role == (int)UtilConstants.ROLE.Instructor
          && (ddl_TYPE == -1 || a.bit_Internal == ss)
          && (ComOrDepId == -1 || a.Department_Id == ComOrDepId)
          && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName)))
          && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
          && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail))
          && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone))
          && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
          && (fGender == -1 || a.Gender == fGender)
          && (fStatus == -1 || a.Suspended == fStatus)).ToList();
            return View(data);
        }

        #endregion

        #region Export EXCEL
        public FileResult ExportEXCEL(FormCollection form)
        {
            var code = string.IsNullOrEmpty(Request.QueryString["fcode"]) ? string.Empty : Request.QueryString["fcode"].ToLower().Trim();
            var name = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].ToLower().Trim();
            var duration = string.IsNullOrEmpty(Request.QueryString["fDuration"]) ? -1 : Convert.ToDouble(Request.QueryString["fDuration"].Trim());
            var recurrent = string.IsNullOrEmpty(Request.QueryString["fRecurrent"]) ? -1 : Convert.ToDecimal(Request.QueryString["fRecurrent"].Trim());
            var isCaculate = string.IsNullOrEmpty(Request.QueryString["bit_ScoreOrResult"]) ? -1 : Convert.ToInt32(Request.QueryString["bit_ScoreOrResult"].Trim());
            var int_khoidaotao = string.IsNullOrEmpty(Request.QueryString["int_khoidaotao"]) ? new List<string>() : Request.QueryString["int_khoidaotao"].Trim().Split(',').ToList();
            var listCourseID = int_khoidaotao.Count > 0 ? int_khoidaotao.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            var filecontent = ExportEXCEL(code, name, duration, recurrent, isCaculate, listCourseID);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "SubjectsList.xlsx");
            }
            return null;
        }
        private byte[] ExportEXCEL(string code, string name, double duration, decimal recurrent, int isCaculate, List<int> listCourseID)
        {
            var data_ = _repoSubject.GetTrainingCenter(a => (!listCourseID.Any() || listCourseID.Contains((int)a.khoidaotao_id))
           && a.SubjectDetail.IsDelete != true && a.SubjectDetail.CourseTypeId.HasValue && a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General).Select(a => a.subject_id);

            var data = _repoSubject.GetSubjectDetail(a => data_.Contains(a.Id) &&
           (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
           (string.IsNullOrEmpty(name) || a.Name.Contains(name)) &&
           (duration == -1 || a.Duration == duration) &&
           (recurrent == -1 || a.RefreshCycle == recurrent) &&
            (isCaculate == -1 || a.IsAverageCalculate == (isCaculate == 1)) && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Id);

            var templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/SubjectsList.xlsx");
            var template = new FileInfo(templateFilePath);

            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (data != null)
            {
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 7;
                    //var Row = 0;
                    int count = 0;
                    foreach (var item in data)
                    {
                        int col = 1;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellCode = worksheet.Cells[startRow + 1, ++col];
                        cellCode.Value = item?.Code;
                        cellCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = item?.Name;
                        cellFullName.Style.WrapText = true;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellRecurrent = worksheet.Cells[startRow + 1, ++col];
                        cellRecurrent.Value = item?.RefreshCycle > 0 ? item?.RefreshCycle.ToString() : "";
                        cellRecurrent.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellRecurrent.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellRecurrent.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellCalculate1 = worksheet.Cells[startRow + 1, ++col]; //test
                        cellCalculate1.Value = (item.CourseTypeId.HasValue && item?.CourseTypeId != 6) ? item?.Course_Type.str_Name : string.Empty;
                        cellCalculate1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCalculate1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCalculate1.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellCalculate = worksheet.Cells[startRow + 1, ++col];
                        cellCalculate.Value = item?.IsAverageCalculate == true ? "Yes" : "No";
                        cellCalculate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCalculate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCalculate.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellPassScore = worksheet.Cells[startRow + 1, ++col];
                        cellPassScore.Value = item?.Subject_Score?.OrderBy(a => a.point_from)?.FirstOrDefault()?.point_from.ToString() ?? "";
                        cellPassScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellPassScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellPassScore.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellDuration = worksheet.Cells[startRow + 1, ++col];
                        cellDuration.Value = item?.Duration > 0 ? item.Duration.ToString() : "";
                        cellDuration.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellDuration.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellDuration.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellGroupCourse = worksheet.Cells[startRow + 1, ++col];
                        var x = item?.Subject_TrainingCenter.Where(p => p.Department.IsActive == true).Select(p => p.Department.Name).ToList();
                        cellGroupCourse.Value = x.Any() ? " - " + string.Join(" \n -", x).Replace("\n",
                                                         Environment.NewLine) : "";
                        cellGroupCourse.Style.WrapText = true;
                        cellGroupCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellGroupCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellGroupCourse.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        //ExcelRange cellGroupCourse = worksheet.Cells[startRow + 1, ++col];
                        //cellGroupCourse.Value = GroupSubjectItem1(item.Id);
                        //cellGroupCourse.Style.WrapText = true;
                        //cellGroupCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        //cellGroupCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //cellGroupCourse.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        //ExcelRange cellStatus = worksheet.Cells[startRow + 1, ++col];
                        //cellStatus.Value = item.IsActive==true ? "Active" : "DeActive";
                        //cellStatus.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //cellStatus.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //cellStatus.Style.Border.BorderAround(ExcelBorderStyle.Thin);



                        startRow++;

                    }
                    bytes = excelPackage.GetAsByteArray();
                }

            }
            return bytes;

        }
        #endregion
        public ActionResult Index()
        {
            var model = new SubjectDetailModifyModel();
            model.ListRelevalDeparment = _repoDepartmentService.Get(a => a.IsDeleted == false && a.is_training == true).ToList();
            return View(model);
        }
        public ActionResult AjaxHandlerSubject_Child(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                IEnumerable<SubjectDetail> model;
                //var id = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
                var data = _repoSubject.GetSubjectDetailById(id);
                model = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.Code == data.Code && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Id);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.Course_Type?.str_Name
                                                          : sortColumnIndex == 2 ? (c?.IsAverageCalculate == true ? UtilConstants.BoolEnum.Yes.ToString() : UtilConstants.BoolEnum.No.ToString())
                                                          : sortColumnIndex == 4 ? c?.Duration
                                                          : sortColumnIndex == 6 ? c?.Subject_TrainingCenter?.FirstOrDefault()?.Department?.Name
                                                          : sortColumnIndex == 7 ? c?.CAT_GROUPSUBJECT_ITEM?.FirstOrDefault()?.CAT_GROUPSUBJECT.Name
                                                          : sortColumnIndex == 8 ? c.IsActive
                                                          : (object)c?.Course_Type?.str_Name);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                var verticalBar = GetByKey("VerticalBar");
                var displayed = model.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                            string.Empty,
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Course_Type?.str_Name +"</span>",
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + (c?.IsAverageCalculate == true ? UtilConstants.BoolEnum.Yes.ToString() : UtilConstants.BoolEnum.No.ToString()) +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + return_score_pass(c.Id) +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + c?.Duration +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +  c?.RefreshCycle +"</span>",
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +  GroupRelevantTrainingItem(c?.Id) +"</span>",
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + GroupSubjectItem(c?.Id) +"</span>",
                              c?.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Subject(0,"+c?.Id+",2)' aria-hidden='true' title='Inactive' style='cursor:    pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Subject(1,"+c?.Id+",2)' aria-hidden='true' title='Active'     style='cursor: pointer;'></i>",
                              ((User.IsInRole("/Subjects/Modify")) ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = c?.Id,  levelsubject = 2}) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>&nbsp;" : "") +
                              ((User.IsInRole("/Subjects/Delete")) ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c?.Id+ ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")
                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = model.Count(),
                    iTotalDisplayRecords = model.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
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

        private string check_child(int id = -1)
        {
            var HTML = new StringBuilder();
            var data = _repoSubject.GetSubjectDetailById(id);
            var verticalBar = GetByKey("VerticalBar");
            var model = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.Code.Contains(data.Code));
            //HTML.Append((User.IsInRole("/Subjects/Modify") ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = data.Id, levelsubject = 1 }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>&nbsp;" : "") +
            //                 (User.IsInRole("/Subjects/Delete") ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + data.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : ""));
            if (model.Count() > 0)
            {
                HTML.AppendFormat("&nbsp;<span data-value='{0}' class='expand_child' style='cursor: pointer;'><i class='fa fa-plus-circle btnIcon_gray font-byhoa' aria-hidden='true'></i></span>", id);
            }

            return HTML.ToString();
        }
        private string check_child_(int id = -1)
        {
            var HTML = new StringBuilder();
            var data = _repoSubject.GetSubjectDetailById(id);
            var verticalBar = GetByKey("VerticalBar");
            var model = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.Code.Contains(data.Code));
            HTML.Append((User.IsInRole("/Subjects/Modify") ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = data.Id, levelsubject = 1 }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>&nbsp;" : "") +
                             (User.IsInRole("/Subjects/Delete") ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + data.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : ""));
            if (model.Count() > 0)
            {
                HTML.AppendFormat("&nbsp;<span data-value='{0}' class='expand_child' style='cursor: pointer;'><i class='fa fa-plus-circle btnIcon_gray font-byhoa' aria-hidden='true'></i></span>", id);
            }

            return HTML.ToString();
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                //fDuration bit_ScoreOrResult fRecurrent
                var code = string.IsNullOrEmpty(Request.QueryString["fcode"]) ? string.Empty : Request.QueryString["fcode"].ToLower().Trim();
                var name = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].ToLower().Trim();
                var duration = string.IsNullOrEmpty(Request.QueryString["fDuration"]) ? -1 : Convert.ToDouble(Request.QueryString["fDuration"].Trim());
                var recurrent = string.IsNullOrEmpty(Request.QueryString["fRecurrent"]) ? -1 : Convert.ToDecimal(Request.QueryString["fRecurrent"].Trim());
                var isCaculate = string.IsNullOrEmpty(Request.QueryString["bit_ScoreOrResult"]) ? -1 : Convert.ToInt32(Request.QueryString["bit_ScoreOrResult"].Trim());
                var int_khoidaotao = string.IsNullOrEmpty(Request.QueryString["int_khoidaotao[]"]) ? string.Empty : Request.QueryString["int_khoidaotao[]"].ToLower().Trim();

                var model = _repoSubject.GetSubjectDetail(a =>
                (string.IsNullOrEmpty(code) || a.Code.ToLower().Trim().Contains(code)) &&
                (string.IsNullOrEmpty(name) || a.Name.ToLower().Trim().Contains(name)) &&
                (duration == -1 || a.Duration == duration) &&
                (recurrent == -1 || a.RefreshCycle == recurrent) &&
                 (isCaculate == -1 || a.IsAverageCalculate == (isCaculate == 1))
                 && a.CourseTypeId == (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Name).GroupBy(a => a.Code).Select(b => b.FirstOrDefault());

                if (!string.IsNullOrEmpty(int_khoidaotao))
                {
                    List<int> relevantList = int_khoidaotao.Split(',').Select(Int32.Parse).ToList();
                    List<SubjectDetail> modelData = model.ToList();
                    List<SubjectDetail> subjectRemove = new List<SubjectDetail>();
                    foreach (var item in modelData)
                    {
                        var data = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.Code == item.Code && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General && a.Subject_TrainingCenter.Any(b => relevantList.Contains((int)b.khoidaotao_id)));
                        if (!data.Any())
                        {
                            subjectRemove.Add(item);
                        }
                    }
                    if (subjectRemove.Count > 0) { modelData.RemoveAll(a => subjectRemove.Any(b => b.Id == a.Id)); }
                    model = modelData.AsQueryable();
                }


                IEnumerable<SubjectDetail> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.Code
                                                          : sortColumnIndex == 2 ? c?.Name
                                                          : c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed
                             select new object[] {
                             string.Empty,
                             //"<span data-value='"+c?.Id +"' class='expand' style='cursor: pointer;'><a><span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Code +"</span></a></span>",
                             //"<span data-value='"+c?.Id +"' class='expand' style='cursor: pointer;'><a><span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Name +"</span></a></span>",
                              "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Code +"</span>",
                                "<span data-value='"+c.Id+"' class='expand_child' "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+"><a "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+" onclick='javascript:void(0)'>" +c?.Name+"</a></span>",
                              c?.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Subject(0,"+c?.Id+",1)' aria-hidden='true' title='Inactive' style='cursor:    pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Subject(1,"+c?.Id+",1)' aria-hidden='true' title='Active'     style='cursor: pointer;'></i>",
                              check_child_(c.Id),
                             };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = model.Count(),
                    iTotalDisplayRecords = model.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public string GroupSubjectItem1(int id)
        {
            StringBuilder HTML = new StringBuilder();
            var groupSubjectItem = _repoSubject.GetGroupSubjectItem(a => a.id_subject == id);
            if (groupSubjectItem.Any())
            {
                foreach (var item in groupSubjectItem)
                {
                    if (item.id_groupsubject == 43)
                    {
                        var groupSubject = _repoSubject.GetGroupSubjectById(item.id_groupsubject);
                        HTML.Append("-" + groupSubject.Name + "\n");
                    }
                    else
                    {
                        var groupSubject = _repoSubject.GetGroupSubjectById(item.id_groupsubject);
                        HTML.Append("-" + groupSubject.Name + "\n");
                    }

                }
            }
            return HTML.ToString();
        }
        private string GroupSubjectItem(int? id)
        {
            StringBuilder HTML = new StringBuilder();
            var groupSubjectItem = _repoSubject.GetGroupSubjectItem(a => a.id_subject == id).ToList();
            if (!groupSubjectItem.Any()) return HTML.ToString();
            var data = groupSubjectItem.Select(item => item.CAT_GROUPSUBJECT);
            foreach (var groupSubject in data)
            {
                HTML.Append("<p> - " + groupSubject.Name + "<br/></p>");
            }
            return HTML.ToString();
        }
        private string GroupRelevantTrainingItem(int? id)
        {
            StringBuilder HTML = new StringBuilder();
            var SubjectItem = _repoSubject.GetSubjectDetailById(id);
            if (SubjectItem == null) return HTML.ToString();
            var data = SubjectItem.Subject_TrainingCenter.Select(item => item.Department);
            foreach (var groupSubject in data)
            {
                HTML.Append("<p> - " + groupSubject.Name + "<br/></p>");
            }
            return HTML.ToString();
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Delete", Resource.INVALIDDATA);
                return Json(new AjaxResponseViewModel() { result = false, message = string.Format(Resource.INVALIDDATA) });
            }
            try
            {
                var model = _repoSubject.GetSubjectDetailById(id.Value);
                if (model == null)
                    return Json(new AjaxResponseViewModel() { result = false, message = Messege.UNSUCCESS }, JsonRequestBehavior.AllowGet);
                if (!CurrentUser.IsMaster)
                {
                    if (model.Instructor_Ability.Any())
                    {
                        return Json(new AjaxResponseViewModel() { result = false, message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name) }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.LmsStatus == StatusModify)
                    {
                        return Json(new AjaxResponseViewModel()
                        {
                            message = string.Format(Messege.DELETED_UNSUCCESS_SENDLMS, model.Name),
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                model.IsDelete = true;
                model.IsActive = false;
                model.DeletedDate = DateTime.Now;
                model.DeletedBy = CurrentUser.USER_ID + "";
                // Cập nhật Status để gửi LMS
                model.LmsStatus = StatusModify;
               
                if (model.CourseTypeId == (int)UtilConstants.CourseTypes.General)
                {
                    var childs = _repoSubject.GetSubjectDetailByCode(model.Code).Where(x=>x.Id != model.Id);
                    foreach (var child in childs.ToList())
                    {
                        child.IsDelete = true;
                        child.IsActive = false;
                        child.DeletedDate = DateTime.Now;
                        child.DeletedBy = CurrentUser.USER_ID + "";
                        child.LmsStatus = StatusModify;
                      
                        _repoSubject.Update(child);
                    }
                }
                _repoSubject.Update(model);

                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON SUBJECT)----------]
                    var callLms = CallServices(UtilConstants.CRON_SUBJECT);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, model.Code),
                        //    result = false
                        //});
                    }
                    #endregion
                });

                return Json(new AjaxResponseViewModel() { result = true, message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Delete", ex.Message);
                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Modify(int? id)
        {
            var model = new SubjectDetailModifyModel();
            model.levelsubject = !string.IsNullOrEmpty(Request["levelsubject"]) ? Convert.ToInt32(Request["levelsubject"]) : 1;
            model.IsEdit = true;
            var entity = _repoSubject.GetSubjectDetailById(id);
            if (entity != null)
            {
                model.Id = entity.Id;
                model.Name = entity.Name;
                model.Code = entity.Code;
                model.IsAverageCaculate = entity.IsAverageCalculate == true ? 1 : 0;
                model.InstructorAbility = entity.Instructor_Ability.Select(x => (int)x.InstructorId).ToArray();
                model.ListGroupCourses = entity.CAT_GROUPSUBJECT_ITEM.Select(x => x.id_groupsubject).ToArray();
                model.Recurrent = entity.RefreshCycle.HasValue ? (int)entity.RefreshCycle : 0;
                model.Duration = entity.Duration.HasValue ? (double)entity.Duration : 0;
                model.PassScore = entity.PassScore;
                model.SubjectScoreModels = entity.Subject_Score.Select(x => new SubjectScoreViewModel()
                {
                    Grade = x.grade,
                    Point = x.point_from,
                });
                model.ListInstructors =
                    entity.Instructor_Ability.Where(z => z.Trainee.IsDeleted == false).Select(x => new SubjectDetailInfoViewModel
                    {
                        Allowance = x.Allowance ?? 0,
                        Id = x.id,
                        Name = ReturnDisplayLanguage(x.Trainee.FirstName, x.Trainee.LastName),
                        SubjectId = (int)x.SubjectDetailId,
                        InstructorId = (int)x.InstructorId,
                        CreateDate = x.CreateDate,
                        InstructorEID = x.Trainee.str_Staff_Id,
                    });


                model.IsUsed = (bool)entity.IsActive;
                model.SubjectScores = entity.Subject_Score;
                model.ListSubjectChild = _repoSubject.GetSubjectDetail(a => ((a.int_Parent_Id == entity.Id) || (a.Code == entity.Code)) && a.CourseTypeId.HasValue && a.CourseTypeId != 6 && a.IsDelete == false).OrderBy(a => a.Id).Select(a => new SubjectDetailChildInfoViewModel()
                {
                    Recurrent = a.RefreshCycle.HasValue ? a.RefreshCycle.Value : 0,
                    CoursetypeName = a.CourseTypeId.HasValue ? a.Course_Type.str_Name : string.Empty,
                    CertificateName = a.CertificateName,
                    CertificateCode = a.CertificateCode,
                    Duration = a.Duration.HasValue ? a.Duration.Value : 0,
                    CoursetypeID = a.CourseTypeId.HasValue ? a.CourseTypeId : null,
                }).ToList();
                model.IsEdit = !entity.Course_Detail.Any() ? true : false;
            }
            model.ListRelevalDeparment = _repoDepartmentService.Get(a => a.IsDeleted == false && a.is_training == true).ToList();
            model.Instructors = EmployeeService.GetInstructors()?
                    .OrderBy(a => a.LastName)
                    .ToDictionary(a => a.Id,
                        a => string.Format("{0} - {1}", a.str_Staff_Id, ReturnDisplayLanguage(a.FirstName, a.LastName)));
            model.GroupCourses = _repoSubject.GetGroupSubject()?.OrderBy(a => a.Name).ToDictionary(a => a.id, a => a.Name);
            model.AverageStatus = UtilConstants.YesNoDictionary();
            model.Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster);
            model.JobTitles = new SelectList(_repoJobtitleService.Get().OrderBy(m => m.Name), "Id", "Name");
            model.ListSubjectType = CourseService.GetCourseTypes().Select(a => new Subject_Types()
            {
                ID = a.Course_Type_Id,
                Name = a.str_Name,
            }).ToList();
            string listtype = string.Empty;
            foreach (var item in model.ListSubjectType)
            {
                listtype += "<option value='" + item.ID + "' " + (item.ID == model.CourseTypeID ? "selected"


                                : "") + ">" + item.Name + "</option>";
            }
            model.HtmlSubjectType = listtype;
            string listsubjecttype = string.Empty;
            foreach (var item in model.GroupCourses)
            {
                listsubjecttype += "<option value='" + item.Key + "' " + (item.Key == model.CourseTypeID ? "selected"


                                : "") + ">" + item.Value + "</option>";
            }
            model.HtmlGroupSubject = listsubjecttype;
            model.SubjectTrainingCenter = entity != null ? (entity.Subject_TrainingCenter.Any() ? entity.Subject_TrainingCenter.Select(a => a.khoidaotao_id).ToList() : new List<int?>()) : new List<int?>();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Modify(SubjectDetailModifyModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = MessageInvalidData(ModelState),
                    result = false
                });
            }
            try
            {
                _repoSubject.Modify(model, form);
                if (model.ListInstructors != null)
                {
                    foreach (var item in model.ListInstructors)
                    {
                        EmployeeService.ModifyEmployee(item.InstructorId, form);
                    }
                }
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON_SUBJECT)----------]
                    var callLms = CallServices(UtilConstants.CRON_SUBJECT);
                    if (!callLms)
                    {

                    }
                    #endregion
                });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Modify", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false

                });

            }

            var result = new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true
            };
            TempData[UtilConstants.NotifyMessageName] = result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(int? id)
        {
            var model = new SubjectDetailModifyModel();
            model.ListRelevalDeparment = _repoDepartmentService.Get(a => a.IsDeleted == false && a.is_training == true).ToList();
            var entity = _repoSubject.GetSubjectDetailById(id);
            if (entity != null)
            {
                model.SubjectIdList = _repoSubject.GetSubjectDetail(a => a.int_Parent_Id == entity.Id).Select(a => a.Id).ToList();
                model.Name = entity.Name;
                model.Code = entity.Code;
                model.Id = entity.Id;
            }
            var record = _repoSubject.GetSubjectDetail(a => a.CourseTypeId == (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Name);
            var recordtemp = record.GroupBy(a => a.Name).Select(b => b.FirstOrDefault());
            model.SubjectDetails = recordtemp.ToList();
            model.SubjectTrainingCenter = entity != null ? (entity.Subject_TrainingCenter.Any() ? entity.Subject_TrainingCenter.Select(a => a.khoidaotao_id).ToList() : new List<int?>()) : new List<int?>();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(SubjectDetailModifyModel model, FormCollection form)
        {
            try
            {
                _repoSubject.CreateGroup(model, form);
                var result = new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Create", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                });
            }
        }

        [HttpPost]
        public ActionResult GetSubjectScores(int? id)
        {
            try
            {
                IEnumerable<SubjectScoreViewModel> scores;
                var temps = TempData[TempSubjectScoreName];
                if (temps != null)
                {
                    scores = (List<SubjectScoreViewModel>)temps;
                }
                else
                {
                    scores = id.HasValue
                        ? _repoSubject.GetScores(a => a.subject_id == id).Select(a => new SubjectScoreViewModel() { Grade = a.grade, Id = a.id, Point = a.point_from }).ToList()
                        : new List<SubjectScoreViewModel>();
                }
                return Json(new AjaxResponseViewModel() { data = scores, result = true });

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/GetSubjectScores", ex.Message);
                return Json(new AjaxResponseViewModel() { message = ex.Message, result = false });

            }
        }
        public ActionResult AddScore(int? id)
        {
            var model = new SubjectScoreViewModel();
            if (id.HasValue)
            {
                var entity = _repoSubject.GetScoreById(id.Value);
                if (entity != null)
                {
                    model.Id = entity.id;
                    model.Grade = entity.grade;
                    model.Point = entity.point_from;
                }
            }
            return PartialView("_Partials/_SubjectScore", model);
        }
        // Check valid score only
        [HttpPost]
        public ActionResult AddScore(SubjectScoreViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Json(new AjaxResponseViewModel() { data = model, message = Messege.SUCCESS, result = true });
                }
                return Json(new AjaxResponseViewModel() { message = MessageInvalidData(ModelState), result = false });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/AddScore", ex.Message);
                return Json(new AjaxResponseViewModel() { message = ex.Message, result = false });

            }
        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateSubject(int isParticipate, string id, int type)
        {
            int idsubject = int.Parse(id);
            var subjectDetail = _repoSubject.GetSubjectDetailById(idsubject);
            if (subjectDetail == null)
                return Json(new AjaxResponseViewModel() { result = false, message = Messege.UNSUCCESS }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                subjectDetail.IsActive = false;
            }
            else
            {
                subjectDetail.IsActive = true;
            }
            subjectDetail.ModifyDate = DateTime.Now;
            subjectDetail.ModifiedBy = CurrentUser.USER_ID + "";
            //subjectDetail.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
            if (type == 1)
            {
                var list_child = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.Code == subjectDetail.Code && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General).ToList();
                foreach (var item in list_child)
                {
                    item.IsActive = subjectDetail.IsActive;
                    item.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                    item.ModifyDate = DateTime.Now;
                    item.ModifiedBy = CurrentUser.USER_ID + "";
                    _repoSubject.Update(item);
                }
            }

            _repoSubject.Update(subjectDetail);

            await Task.Run(() =>
            {
                #region [--------CALL LMS (CRON SUBJECT)----------]
                var callLms = CallServices(UtilConstants.CRON_SUBJECT);
                if (!callLms)
                {
                    //return Json(new AjaxResponseViewModel()
                    //{
                    //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, subjectDetail.Code.Trim() + " - " + subjectDetail.Name.Trim()),
                    //    result = false
                    //});
                }
                #endregion
            });
            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, subjectDetail.Code.Trim() + " - " + subjectDetail.Name.Trim()), result = true }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterInstructor(FormCollection form)
        {
            var filterInstructor = string.IsNullOrEmpty(form["Filter_Instructor"])
                ? string.Empty
                : form["Filter_Instructor"].Trim().ToLower();
            var departmentId = string.IsNullOrEmpty(form["DepartmentId"]) ? -1 : Convert.ToInt32(form["DepartmentId"].Trim());
            var jobTitleId = string.IsNullOrEmpty(form["JobTitleId"]) ? -1 : Convert.ToInt32(form["JobTitleId"].Trim());
            var subjectId = !string.IsNullOrEmpty(form["Instructors[]"]) ? form["Instructors[]"].Split(new char[] { ',' }) : null;

            var html = new StringBuilder();
            var lstDepartment = new List<int?>();
            var department = DepartmentService.GetById(departmentId);
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
            var data =
                 EmployeeService.Get(a => a.IsActive == true &&
                 (jobTitleId == -1 || a.Job_Title_id == jobTitleId) &&
                 (departmentId == -1 || lstDepartment.Contains(a.Department_Id)) &&
                 (string.IsNullOrEmpty(filterInstructor) ||
                 (a.str_Staff_Id.ToLower().Contains(filterInstructor.ToLower()) ||
                 a.FirstName.ToLower().Contains(filterInstructor.ToLower()) ||
                 a.LastName.ToLower().Contains(filterInstructor.ToLower())))).ToList();

            if (!data.Any())
            {
                return Json(new
                {
                    value = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in data)
            {
                if (subjectId != null && subjectId.Contains(item.Id.ToString()))
                {
                    html.AppendFormat("<option selected value='{0}'>{1} - {2}</option>", item.Id, item.str_Staff_Id, ReturnDisplayLanguage(item.FirstName.Trim(), item.LastName.Trim()));
                }
                else
                {
                    html.AppendFormat("<option value='{0}'>{1} - {2}</option>", item.Id, item.str_Staff_Id, ReturnDisplayLanguage(item.FirstName.Trim(), item.LastName.Trim()));
                }
            }
            return Json(new
            {
                value = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxHandlerListSelectInstructor(string id, int pageIndex, int pageSize, FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["form[Filter_Instructor]"])
                ? string.Empty
                : form["form[Filter_Instructor]"].Trim().ToLower();
            var departmentId = string.IsNullOrEmpty(form["form[DepartmentId]"]) ? -1 : Convert.ToInt32(form["form[DepartmentId]"].Trim());
            var jobTitleId = string.IsNullOrEmpty(form["form[JobTitleId]"]) ? -1 : Convert.ToInt32(form["form[JobTitleId]"].Trim());
            var lstDepartment = new List<int?>();
            var department = DepartmentService.GetById(departmentId);
            if (department != null)
            {
                var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).OrderBy(b => b.Ancestor).Select(a => a.Id);
                if (departmentIds.Any())
                {
                    foreach (var iditem in departmentIds)
                    {
                        lstDepartment.Add(iditem);
                    }
                }
            }
            StringBuilder HTML = new StringBuilder();
            HTML.Append("<ul>");
            HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>&nbspOther</span>");
            HTML.Append("<ul>");
            var list_instructor = EmployeeService.GetInstructors(a => (jobTitleId == -1 || a.Job_Title_id == jobTitleId) &&
                 (departmentId == -1 || lstDepartment.Contains(a.Department_Id)) &&
                 (string.IsNullOrEmpty(filterCodeOrName) || a.str_Staff_Id.Trim().ToLower().Contains(filterCodeOrName) || (a.LastName.Trim().ToLower() + " " + a.FirstName.Trim().ToLower()).Contains(filterCodeOrName)), true).OrderBy(n => n.Id);
            int SubjectId = !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
            if (list_instructor.Any())
            {
                var listinstructor = _repoSubject.GetSubjectDetailById(SubjectId)?.Instructor_Ability?.Select(a => a.InstructorId);
                foreach (var item in list_instructor)
                {
                    HTML.Append("<li>");
                    HTML.AppendFormat(" <input data-id='{0}' data-parentname='Other' data-name='{2}'   multiple value='{0}' class='InstructorAbility' name='InstructorAbility' id='InstructorAbility_" + item.Id + "' type='checkbox' " + (listinstructor != null && listinstructor.Contains(item.Id) ? "Checked" : "") + " /><input type='hidden' value='{1}' name='InstructorAbility2' /><label for='InstructorAbility_" + item.Id + "'>&nbsp{2}</label>", item.Id, item.Id, string.Format("{0} - {1}", item?.str_Staff_Id, ReturnDisplayLanguage(item?.FirstName, item?.LastName)));
                    HTML.Append("</li>");
                }
            }

            HTML.Append("</ul>");
            HTML.Append("</li>");
            HTML.Append("</ul>");

            return Json(HTML.ToString());
        }

        [AllowAnonymous]
        public ActionResult Group()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerGroup(jQueryDataTableParamModel param)
        {
            try
            {
                string fcode = string.IsNullOrEmpty(Request.QueryString["fcode"]) ? string.Empty : Request.QueryString["fcode"].ToLower().ToString();
                string fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].ToLower().ToString();
                int fDuration = string.IsNullOrEmpty(Request.QueryString["fDuration"]) ? -1 : Convert.ToInt32(Request.QueryString["fDuration"].Trim());
                int fRecurrent = string.IsNullOrEmpty(Request.QueryString["fRecurrent"]) ? -1 : Convert.ToInt32(Request.QueryString["fRecurrent"].Trim());
                string bit_ScoreOrResult = string.IsNullOrEmpty(Request.QueryString["bit_ScoreOrResult"]) ? string.Empty : Request.QueryString["bit_ScoreOrResult"].ToLower().ToString();

                List<int> listCourseID = new List<int>();
                var lst = string.IsNullOrEmpty(Request.QueryString["int_khoidaotao[]"]) ? new List<string>() : Request.QueryString["int_khoidaotao[]"].Trim().Split(',').ToList();
                listCourseID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                var model = _repoSubject.GetSubjectDetail(a => a.Subject_TrainingCenter.Any(x => listCourseID.Count() == 0 || listCourseID.Contains((int)x.khoidaotao_id)) && !a.int_Parent_Id.HasValue && !a.CourseTypeId.HasValue
                && (String.IsNullOrEmpty(fcode) || a.Code.Contains(fcode)) &&
                (String.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
                (fDuration == -1 || a.Duration == fDuration) &&
                (fRecurrent == -1 || a.RefreshCycle == fRecurrent) &&
                (String.IsNullOrEmpty(bit_ScoreOrResult) || a.IsAverageCalculate == (bit_ScoreOrResult == "True" ? true : false))).OrderByDescending(p => p.Name);
                IEnumerable<SubjectDetail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.Name
                                                          : c.Name);
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
                                "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Code +"</span>",
                                "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Name+"</span>",
                                   //c?.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Deactive(0,"+c.Id+")' aria-hidden='true' title='Deactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Deactive(1,"+c.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                                check_child_subject_parent(c.Id)
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
            catch (Exception)
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
        private string check_child_subject_parent(int Subject_Id = -1)
        {
            var data = _repoSubject.GetSubjectDetailById(Subject_Id);
            StringBuilder HTML = new StringBuilder();

            var model = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.int_Parent_Id == Subject_Id);
            HTML.Append(
                ((User.IsInRole("/Subjects/Modify") && data?.IsDelete != true) ? "<a   title='Edit' href='" + @Url.Action("Create", new { id = Subject_Id }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>&nbsp;" : "") +
                (User.IsInRole("/Subjects/Delete") ? "<a title='Delete' href='javascript:void(0)' onclick='calldeletegroup(" + Subject_Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : ""));
            if (model.Count() > 0)
            {
                HTML.AppendFormat("&nbsp;<span data-value='{0}' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle font-byhoa' aria-hidden='true' style='color: red;'></i></span>", Subject_Id);
            }
            return HTML.ToString();
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerGroupSubject_Child(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var data = _repoSubject.GetSubjectDetailById(id);
                var model = _repoSubject.GetSubjectDetail(a => a.int_Parent_Id == id && a.CourseTypeId == (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Name);
                IEnumerable<SubjectDetail> filtered = model.GroupBy(a => a.Name).Select(b => b.FirstOrDefault());

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.Code
                                                          : sortColumnIndex == 2 ? c?.Name
                                                          : c.Name);
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
                               "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Code +"</span>",
                               "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Name+"</span>",
                               //c?.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Subject(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor:    pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Subject(1,"+c?.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                               check_child(c.Id),
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
            catch (Exception)
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
        [AllowAnonymous]
        public ActionResult AjaxHandlerGroupSubject_Child_Subject(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {

                var data = _repoSubject.GetSubjectDetailById(id);

                var model = _repoSubject.GetSubjectDetail(a => a.Code == data.Code && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General).OrderByDescending(p => p.Id);
                //&& a.bit_Active == data.bit_Active
                IEnumerable<SubjectDetail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.Course_Type?.str_Name
                                                          : sortColumnIndex == 2 ? (c?.IsAverageCalculate == true ? UtilConstants.BoolEnum.Yes.ToString() : UtilConstants.BoolEnum.No.ToString())
                                                          : sortColumnIndex == 4 ? c?.Duration
                                                          : sortColumnIndex == 6 ? c?.Subject_TrainingCenter?.FirstOrDefault()?.Department?.Name
                                                          : sortColumnIndex == 7 ? c?.CAT_GROUPSUBJECT_ITEM?.FirstOrDefault()?.CAT_GROUPSUBJECT.Name
                                                          : sortColumnIndex == 8 ? c.IsActive
                                                          : (object)c?.Course_Type?.str_Name);
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
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.Course_Type?.str_Name +"</span>",
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + (c?.IsAverageCalculate == true ? UtilConstants.BoolEnum.Yes.ToString() : UtilConstants.BoolEnum.No.ToString()) +"</span>",
                           "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +return_score_pass(c.Id)+"</span>",
                           "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + c?.Duration +"</span>",
                           "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +  c?.RefreshCycle +"</span>",
                           "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +  GroupRelevantTrainingItem(c?.Id) +"</span>",
                            "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" + GroupSubjectItem(c?.Id) +"</span>",
                          //      c?.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Subject(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor:    pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Subject(1,"+c?.Id+")' aria-hidden='true' title='Active'     style='cursor: pointer;'></i>",
                          //((User.IsInRole("/Subjects/Modify")) ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = c?.Id,  levelsubject = 2}) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>&nbsp;" : "") +
                          //    ((User.IsInRole("/Subjects/Delete")) ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c?.Id+ ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")
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
            catch (Exception)
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
        private string return_score_pass(int idsubject = -1)
        {
            string return_ = "";
            var data = _repoSubject.GetScores(a => a.subject_id == idsubject && a.grade == "Pass").FirstOrDefault();
            if (data != null)
            {
                return_ = data.point_from.ToString();
            }
            return return_;
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult DeleteGroup(int? id)
        {
            if (!id.HasValue)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Delete", Resource.INVALIDDATA);
                return Json(new AjaxResponseViewModel() { result = false, message = string.Format(Resource.INVALIDDATA) });
            }
            try
            {
                var model = _repoSubject.GetSubjectDetailById(id.Value);
                if (model == null)
                    return Json(new AjaxResponseViewModel() { result = false, message = Messege.UNSUCCESS }, JsonRequestBehavior.AllowGet);
                model.IsDelete = true;
                model.IsActive = false;
                // Cập nhật Status để gửi LMS
                model.LmsStatus = StatusModify;
                _repoSubject.Update(model);
                return Json(new AjaxResponseViewModel() { result = true, message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Subjects/Delete", ex.Message);
                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}