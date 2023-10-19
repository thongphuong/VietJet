using OfficeOpenXml;
using OfficeOpenXml.Style;
using TMS.Core.App_GlobalResources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;

namespace TrainingCenter.Controllers
{
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Subjects;

    public class SubjectController : BaseAdminController
    {
        #region[UnitOfWork]

        private readonly ISubjectService _repoSubject;


        #endregion

        #region Index
        // return view

        public SubjectController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService repoSubject) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService)
        {
            _repoSubject = repoSubject;
        }

        public ActionResult Index()
        {
            return View();
        }

        // fill data to datatable by ajax 
        
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
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
                var data = _repoSubject.Get(a =>
                (String.IsNullOrEmpty(fcode) || a.str_Code.Contains(fcode)) &&
                (String.IsNullOrEmpty(fName) || a.str_Name.Contains(fName)) );


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Subject, object> orderingFunction = (c => sortColumnIndex == 1 ? c.str_Code
                                                          : sortColumnIndex == 2 ? c.str_Name
                                                          //: sortColumnIndex == 5 ? c.Pass_Score
                                                          //: sortColumnIndex == 6 ? c.CAT_GROUPSUBJECT?.str_Name
                                                          : c.str_Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                
                    var dataResult = (sortDirection == "asc") ? data.OrderBy(orderingFunction)
                                    : data.OrderByDescending(orderingFunction);

                var displayed = dataResult.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {

                             string.Empty,
                         c.str_Code ?? "",
                         c.str_Name ?? "",
                          ((User.IsInRole("/Subject/Modify")) ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = c.Subject_Id}) + "'><i class='fa fa-pencil-square-o' style=' font-size: 16px;  '></i></a>" : "") + TMS.Core.Util.UtilConstants.VerticalBar +
                    ((User.IsInRole("/Subject/Delete")) ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.Subject_Id+ ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px;  '></i></a>" : "")


            };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = dataResult.Count(),
                    iTotalDisplayRecords = dataResult.Count(),
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
            StringBuilder HTML = new StringBuilder();

            var model = _repoSubject.Get(a => !a.bit_Deleted && a.int_Parent_Id == Subject_Id).OrderByDescending(p => p.Subject_Id);
            HTML.Append(
                ((Is_Edit("/Subject/Index")) ? "<a   title='Edit' href='" + Url.Action("Modify", new { id = Subject_Id, editparent = 0 }) + "'><i class='fa fa-pencil-square-o' style=' font-size: 16px;  '></i></a>" : "") + UtilCon
                ((Is_Delete("/Subject/Index")) ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + Subject_Id + ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px;  '></i></a>" : ""));
            if (model.Any())
            {
                HTML.AppendFormat("&nbsp;<span data-value='{0}' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; color: red; '></i></span>", Subject_Id);
            }
            return HTML.ToString();
        }
        private string check_child(int Subject_Id = -1)
        {
            StringBuilder HTML = new StringBuilder();
            var data = _repoSubject.GetById(Subject_Id);

            var model = _repoSubject.Get(a => !a.bit_Deleted && a.str_Name.Contains(data.str_Name)).OrderByDescending(p => p.Subject_Id);
            HTML.Append(((User.IsInRole("/Subjects/Modify")) ? "<a href='" + @Url.Action("Modify", new { id = data.Subject_Id, editparent = 1 }) + "'><i class='fa fa-pencil-square-o' style=' font-size: 16px;  '></i></a>&nbsp;" : "") + ((User.IsInRole("/Subject/Delete")) ? "<a href='javascript:void(0)' onclick='calldelete(" + Subject_Id + ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px;  '></i></a>" : ""));
            if (model.Any())
            {
                HTML.AppendFormat("&nbsp;<span data-value='{0}' class='expand_child' style='cursor: pointer;'><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; color: blue; '></i></span>", Subject_Id);
            }

            return HTML.ToString();
        }
        private string return_score_pass(int idsubject = -1)
        {
            string return_ = "";
            var data = _repoSubject.GetScore(idsubject);
            if (data != null)
            {
                return_ = data.point_from.ToString();
            }
            return return_;
        }
        private string return_training_center(int idsubject = -1)
        {
            var HTML_ = new StringBuilder();
            //var data = _repoSubject.GetTrainingCenter(a => a.SubjectId== idsubject);
            //if (data.Any())
            //{
            //    foreach (var item in data)
            //    {
            //        HTML_.AppendFormat("{0} \n", item?.Department?.str_Name);
            //    }

            //}
            return HTML_.ToString();
        }

        
        public ActionResult AjaxHandlerSubject_Child(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var model = _repoSubject.GetSubjectDetail();

                IEnumerable<SubjectDetail> filtered = model.GroupBy(a => a.Name).Select(b => b.First());

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.Name
                                                          : sortColumnIndex == 3 ? c.Duration
                                                          : sortColumnIndex == 4 ? (object)c.RefreshCycle
                                                          //: sortColumnIndex == 5 ? c.Pass_Score.ToString(CultureInfo.InvariantCulture)
                                                          //: sortColumnIndex == 6 ? c.CAT_GROUPSUBJECT?.str_Name
                                                          : c.Id);


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
                         c?.Code != null ?c?.Code : "",
                         c?.Name != null ? c?.Name  : "",
                           c?.RefreshCycle,
                         check_child(c.Id)
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

        
        public ActionResult AjaxHandlerSubject_(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 3 ? c.Duration
                                                          : sortColumnIndex == 4 ? (object)c.RefreshCycle
                                                          //: sortColumnIndex == 5 ? c.Pass_Score.ToString(CultureInfo.InvariantCulture)
                                                          //: sortColumnIndex == 6 ? c.CAT_GROUPSUBJECT?.str_Name
                                                          : c.Id);
                var data = _repoSubject.GetById(id);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                var model = (sortDirection == "asc")
                    ? _repoSubject.GetSubjectDetailByName(data.str_Name).OrderBy(orderingFunction)
                                   : _repoSubject.GetSubjectDetailByName(data.str_Name).OrderByDescending(orderingFunction);



                var displayed = model.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                             string.Empty,
                          //c.Course_Type?.str_Name,
                           c.IsAverageCalculate == true ? "Yes" : "No",
                           return_score_pass(c.Id),
                         c.Duration,
                          return_training_center(c.Id),
                          ((Is_Edit("/Subject/Index")) ? "<a href='"+Url.Action("Modify",new{id = c.Id,  editparent =2})+"'><i class='fa fa-pencil-square-o' style=' font-size: 16px;  '></i></a>&nbsp;":"") + UtilConstants.VerticalBar +
                          ((Is_Delete("/Subject/Index")) ? "<a href='javascript:void(0)' onclick='calldelete(" + c.Id  + ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px;  '></i></a>":"")
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

        
        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                Subject model;
                model = _repoSubject.GetById(id);
                model.bit_Deleted = true;
                _repoSubject.Update(model);
                return Json(CMSUtils.alert("success", Messege.SUCCESS));
            }
            catch (Exception ex)
            {
                return Json(CMSUtils.alert("success", Messege.UNSUCCESS));
            }

        }

        public ActionResult AjaxHandlerSubject(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var getinstructor = EmployeeService.GetAbility(a => a.SubjectDetailId == id).Select(a => a.InstructorId);

                var model = EmployeeService.Get(a => getinstructor.Contains(a.Id));

                IEnumerable<Trainee> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? c?.FirstName
                                                          : sortColumnIndex == 3 ? c?.JobTitle?.Name
                                                          : sortColumnIndex == 4 ? c?.Department?.Name
                                                          : c.FirstName);


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
                         c.str_Staff_Id,
                         c.FirstName + " " +c.LastName,
                         c.JobTitle?.Name,
                         c.Department?.Name};
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
        #endregion


        public ActionResult Modify(int? id)
        {
            if (!id.HasValue)
            {
                TempData[UtilConstants.NotifyMessageName] = new {result = false, message = "Invalid data"};
                return RedirectToAction("Index");
            }
            var entity = _repoSubject.GetById(id.Value);
            if (entity == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new { result = false, message = "Data is not found" };
                return RedirectToAction("Index");
            }
            var model = new SubjectModifyViewModel()
            {
                Id = entity.Subject_Id,
                Code = entity.str_Code,
                Name = entity.str_Name,
                ParentId = entity.int_Parent_Id,
                Subjects =
                    _repoSubject.Get(
                        a => a.Subject_Id != entity.Subject_Id && (a.int_Parent_Id == null && !a.Ancesstor.StartsWith(entity.Ancesstor + "_")))
                        .ToDictionary(a => a.Subject_Id, a => string.Format("{0} - {1}", a.str_Code, a.str_Name)),
                SubjectDetailModel = entity.SubjectDetails.Where(a=>!a.IsDelete).Select(a=> new SubjectDetailModifyModel()
                {
                    Id = a.Id,
                    Name = a.Name,
                    Code = a.Code,
                    IsAverageCaculate = a.IsAverageCalculate ? 1 : 0,
                    InstructorAbility = a.Instructor_Ability.Select(x=>x.InstructorId).ToArray(),
                    Recurrent = a.RefreshCycle,
                    Duration = a.Duration,
                    instructor = new {key = a.Instructor_Ability.Select(x => x.InstructorId).ToArray(),value = a.Instructor_Ability.Select(x => string.Format("{0} - {1} {2}",x.Trainee.str_Staff_Id, x.Trainee.FirstName ,x.Trainee.LastName)).ToArray() },
                    SubjectScoreModels = a.Subject_Score.Select(x=> new SubjectScoreModel()
                    {
                        Grade = x.grade,PointFrom = x.point_from,PointTo = x.point_from
                    })
                })
            };
            
            return View(model);
        }

        public ActionResult Create()
        {
            var subject = new SubjectModifyViewModel()
            {
                Subjects = _repoSubject.Get().OrderBy(a => a.str_Code).ToDictionary(a => a.Subject_Id, a => a.str_Code + " - " + a.str_Name),
                //AverageStatus = UtilConstants.YesNoDictionary(),
                //Instructors = EmployeeService.Get(a=>a.int_Role == (int)UtilConstants.ROLE.Instructor).ToDictionary(a=>a.Id,a=>string.Format("{0} {1}",a.FirstName ,a.LastName))
            };

            return View(subject);
        }


        [HttpPost]
        public ActionResult Create(SubjectModifyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _repoSubject.Modify(model, CurrentUser.Username);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[UtilConstants.NotifyMessageName] = new { result = false, message = ex.Message };
                    model.Subjects = _repoSubject.Get()
                        .OrderBy(a => a.str_Code)
                        .ToDictionary(a => a.Subject_Id, a => a.str_Code + " - " + a.str_Name);
                    return View(model);
                }
            }
            var message = MessageInvalidData(ModelState);
            TempData[UtilConstants.NotifyMessageName] = new { result = false, message = message };
            model.Subjects = _repoSubject.Get()
                .OrderBy(a => a.str_Code)
                .ToDictionary(a => a.Subject_Id, a => a.str_Code + " - " + a.str_Name);
            foreach (var m in model.SubjectDetailModel)
            {
                m.instructor = new
                {
                     key = m.InstructorAbility,value = m.Teacher.Split('|')
                };
            }
            return View(model);
        }


        #region Export Excel

        //controller Action
        
        public ActionResult Excel_(string fcode, string fName, string fDuration, string fRecurrent, string bit_ScoreOrResult, string int_khoidaotao)
        {
            string fcode_ = string.IsNullOrEmpty(fcode) ? string.Empty : fcode.ToLower().ToString();
            string fName_ = string.IsNullOrEmpty(fName) ? string.Empty : fName.ToLower().ToString();
            int fDuration_ = string.IsNullOrEmpty(fDuration) ? -1 : Convert.ToInt32(fDuration.Trim());
            int fRecurrent_ = string.IsNullOrEmpty(fRecurrent) ? -1 : Convert.ToInt32(fRecurrent.Trim());
            string bit_ScoreOrResult_ = string.IsNullOrEmpty(bit_ScoreOrResult) ? string.Empty : bit_ScoreOrResult.ToLower().ToString();



            //return View();
            List<int> listCourseID = new List<int>();
            var lst = string.IsNullOrEmpty(int_khoidaotao) ? new List<string>() : int_khoidaotao.Trim().Split(',').ToList();
            var data = _repoSubject.Get().Select(a => a.Subject_Id);

            var model = _repoSubject.GetSubjectDetail(a => data.Contains(a.Id) &&
            (String.IsNullOrEmpty(fcode) || a.Code.Contains(fcode)) &&
            (String.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
            (fDuration_ == -1 || a.Duration == fDuration_) &&
            (fRecurrent_ == -1 || a.RefreshCycle == fRecurrent_) &&
            (String.IsNullOrEmpty(bit_ScoreOrResult) || a.IsAverageCalculate == (bit_ScoreOrResult == "True" ? true : false))
                ).OrderByDescending(p => p.Id);
            var table = new System.Data.DataTable();

            table.Columns.Add("Subject Code", typeof(string));
            table.Columns.Add("Subject name", typeof(string));
            table.Columns.Add("Recurrent", typeof(string));
            table.Columns.Add("Subject Type", typeof(string));
            table.Columns.Add("Average Calculate", typeof(string));
            table.Columns.Add("Pass Score", typeof(string));
            table.Columns.Add("Duration", typeof(string));
            table.Columns.Add("Relevant Training Department", typeof(string));

            foreach (var item in model)
            {
                table.Rows.Add(
                                item.Code,
                                item.Name,
                                item.RefreshCycle,
                                //item.Course_Type?.str_Name,
                                item.IsAverageCalculate == true ? "Yes" : "No",
                                return_score_pass(item.Id),
                                item?.Duration,
                                 return_training_center(item.Id)
                          );
            }

            Export_ export = new Export_();
            export.ToExcel(Response, table);
            return View();
        }

        //helper class
        public class Export_
        {
            public void ToExcel(HttpResponseBase Response, object clientsList)
            {
                var grid = new System.Web.UI.WebControls.GridView();
                grid.DataSource = clientsList;
                grid.DataBind();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=SubjectList" + DateUtil.DateToString(DateTime.Now, "ddMMyyyy_hhmm") + ".xls");
                Response.ContentType = "application/excel";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                htw.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
                htw.Write("<table><tr><td colspan='8'></td></tr></table>");
                htw.Write("<table><tr  style='border:1px solid #000;'><td></td><td colspan='6'><h1 style='text-align:center'>SUBJECT</h1></td><td></td></tr></table>");
                htw.Write("<table><tr><td colspan='8'></td></tr></table>");

                grid.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.End();
            }
        }

        ///----------///
        ///
        public FileContentResult Export(string fcode, string fName, string fDuration, string fRecurrent, string bit_ScoreOrResult, string int_khoidaotao)
        {
            string fcode_ = string.IsNullOrEmpty(fcode) ? string.Empty : fcode.ToLower().ToString();
            string fName_ = string.IsNullOrEmpty(fName) ? string.Empty : fName.ToLower().ToString();
            int fDuration_ = string.IsNullOrEmpty(fDuration) ? -1 : Convert.ToInt32(fDuration.Trim());
            int fRecurrent_ = string.IsNullOrEmpty(fRecurrent) ? -1 : Convert.ToInt32(fRecurrent.Trim());
            string bit_ScoreOrResult_ = string.IsNullOrEmpty(bit_ScoreOrResult) ? string.Empty : bit_ScoreOrResult.ToLower().ToString();



            //return View();
            List<int> listCourseID = new List<int>();
            var lst = string.IsNullOrEmpty(int_khoidaotao) ? new List<string>() : int_khoidaotao.Trim().Split(',').ToList();
            listCourseID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            var data = _repoSubject.Get().Select(a => a.Subject_Id);
            var model = _repoSubject.GetSubjectDetail(a => data.Contains(a.Id) &&
            (String.IsNullOrEmpty(fcode) || a.Code.Contains(fcode)) &&
            (String.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
            (fDuration_ == -1 || a.Duration == fDuration_) &&
            (fRecurrent_ == -1 || a.RefreshCycle == fRecurrent_) &&
            (String.IsNullOrEmpty(bit_ScoreOrResult) || a.IsAverageCalculate == (bit_ScoreOrResult == "True" ? true : false))
                ).OrderByDescending(p => p.Id);


            byte[] filecontent = ExportExcelCourse(model);
            return File(filecontent, ExportUtils.ExcelContentType, "SubjectList.xlsx");
        }
        private byte[] ExportExcelCourse(IEnumerable<SubjectDetail> _subject)
        {
            string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/SubjectList.xlsx");
            FileInfo template = new FileInfo(templateFilePath);


            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                int startrow = 7;
                int groupHeader = 0;
                int grouptotal = 0;
                int row = 0;
                int col = 1;
                int count = 0;
                foreach (var item1 in _subject)
                {
                    count++;
                    ExcelRange cellNo = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col];
                    cellNo.Value = count;
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellCourse = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 1];
                    cellCourse.Value = item1?.Code;
                    cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellMethod = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 2];
                    cellMethod.Value = item1?.Name;
                    cellMethod.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellMethod.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    ExcelRange cellHours = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 3];
                    cellHours.Value = item1?.RefreshCycle;
                    cellHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //ExcelRange cellDays = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 4];
                    //cellDays.Value = item1?.Course_Type?.str_Name;
                    //cellDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //cellDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellDate = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 5];
                    cellDate.Value = item1?.IsAverageCalculate == true ? "Yes" : "No";
                    cellDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellInstructor = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 6];
                    cellInstructor.Value = return_score_pass(item1.Id);// item1?.Trainee?.str_Fullname;
                    cellInstructor.Style.WrapText = true;
                    cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellNum = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 7];
                    cellNum.Value = item1?.Duration;
                    cellNum.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNum.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellVenue = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 8];
                    cellVenue.Value = return_training_center(item1.Id);
                    cellVenue.Style.WrapText = true;
                    row++;
                }
                using (ExcelRange r = worksheet.Cells[startrow, 1, startrow + row + groupHeader + grouptotal, 9])
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


                Bytes = xlPackage.GetAsByteArray();

            }
            return Bytes;
        }
        #endregion

        [HttpPost]
        public ActionResult RemoveResult(string id)
        {
            if (CMSUtils.DeleteDataSQLNoLog("id", id, "Subject_Score") >= 1)
            {
                return Json(CMSUtils.alert("success", Messege.SUCCESS_REMOVE_RESULT));
            }
            return Json(CMSUtils.alert("danger", Messege.UNSUCCESS_REMOVE_RESULT));
        }

        public ActionResult SubjectDetail(int? id)
        {
            var entity = id.HasValue ? _repoSubject.GetSubjectDetailById(id.Value) : null;
            var model = new SubjectDetailModifyModel
            {
                Instructors = EmployeeService.Get(a => a.int_Role == (int)UtilConstants.ROLE.Instructor).ToDictionary(a => a.Id, a => string.Format("{0} - {1} {2}", a.str_Staff_Id, a.FirstName, a.LastName)),
                AverageStatus = UtilConstants.YesNoDictionary(),
                InstructorAbility = new int[0],
                SubjectScoreModels =new List<SubjectScoreModel>()
            };
            if (entity != null)
            {
                model.Id = entity.Id;
                model.Name = entity.Name;
                model.Recurrent = entity.RefreshCycle;
                model.IsAverageCaculate = entity.IsAverageCalculate ? 1 : 0;
                model.InstructorAbility = entity.Instructor_Ability.Select(a => a.InstructorId).ToArray();
                model.Code = entity.Code;
                model.Duration = entity.Duration;
                model.SubjectScoreModels = entity.Subject_Score.Select(a => new SubjectScoreModel()
                {
                    Grade = a.grade,Id = a.id,
                    PointFrom = a.point_from,//PointTo = a.point_to
                });
                var teachers = entity.Instructor_Ability.Select(a => a.id).ToArray();
                model.Teacher = string.Join("<br/>", teachers);
            }
            return PartialView("_Partials/_SubjectDetailModify", model);
        }
        
    }
}
