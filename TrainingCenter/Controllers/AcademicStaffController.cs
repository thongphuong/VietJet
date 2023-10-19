using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.TraineeHis;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Employee;

namespace TrainingCenter.Controllers
{
    public class AcademicStaffController : BaseAdminController
    {
        private readonly ISubjectService _repoSubjectService;
        private readonly ICompanyService _repoCompanyService;
        private readonly IJobtitleService _repoJobtitleService;
        private readonly ITraineeHistoryService _repotraineeHistoryService;
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _repoEmployeeService;

        public AcademicStaffController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService, ISubjectService repoSubjectService, ICompanyService repoCompany, IJobtitleService jobtitleService, ITraineeHistoryService repotraineeHistoryService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoJobtitleService = jobtitleService;
            _repoCompanyService = repoCompany;
            _departmentService = departmentService;
            _repoSubjectService = repoSubjectService;
            _repotraineeHistoryService = repotraineeHistoryService;
            _repoEmployeeService = employeeService;
        }

        // GET: AcademicStaff
        [AllowAnonymous]
        public ActionResult Index()
        {
            var model = new EmployeeModelIndex();
            model.Departments = LoadDepartment();
            model.Genders = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value");
            model.JobTitles = new SelectList(_repoJobtitleService.Get().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name), "Key", "Value");

            //model.Type = new SelectList(UtilConstants.ActiveStatusDictionary(), "Key", "Value");
            model.Type = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value");
            return View(model);
        }
        private string LoadDepartment(int? id = null)
        {
            var result = string.Empty;
            var data = DepartmentService.Get(a => CurrentUser.PermissionIds.Any(x => x == a.Id)).Select(x => new { x.Id, x.Ancestor, x.Name, x.Code });
            var lvl = 1;
            foreach (var item in data)
            {
                lvl = item.Ancestor.Count(x => x.Equals('!'));
                var khoangtrang = "";
                for (var i = 0; i < lvl; i++)
                {
                    khoangtrang += "&nbsp;&nbsp;&nbsp;";
                }
                result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + (id == item.Id ? "Selected" : "") + ">" + khoangtrang + "+ " + item.Code + " - " + item.Name;
                result += "</option>";
            }
            return result;
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerExaminer(jQueryDataTableParamModel param)
        {
            try
            {
                // xử lý param gửi lên
                var comOrDepId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
                var fJobTitle = string.IsNullOrEmpty(Request.QueryString["JobTitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitleList"].Trim());
                var fGender = string.IsNullOrEmpty(Request.QueryString["Genders"]) ? -1 : Convert.ToInt32(Request.QueryString["Genders"].Trim());
                var ddlType = string.IsNullOrEmpty(Request.QueryString["Type"]) ? -1 : Convert.ToInt32(Request.QueryString["Type"].Trim());
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? string.Empty : Request.QueryString["fEmail"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? string.Empty : Request.QueryString["fPhone"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? string.Empty : Request.QueryString["fStaffId"].Trim();
                var bitInternal = (ddlType == 1);
                var isActive = fStatus != 0;
                var lstDepartment = new List<int?>();

                if (comOrDepId != -1)
                {
                    var department = DepartmentService.GetById(comOrDepId);
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
                }



                var data = EmployeeService.Get(a => a.IsDeleted == false && a.IsExaminer == true
                && (comOrDepId == -1 || lstDepartment.Contains(a.Department_Id))
                  && (ddlType == -1 || a.bit_Internal == bitInternal)
                && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())) || ((a.LastName.Trim() + " " + a.FirstName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.IsActive == isActive), true);
                IEnumerable<Trainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.LastName
                                                            : sortColumnIndex == 3 ? c?.JobTitle?.Name
                                                            : sortColumnIndex == 4 ? c?.Department?.Ancestor
                                                            : sortColumnIndex == 5 ? c?.bit_Internal
                                                            : sortColumnIndex == 6 ? (object)c?.IsActive
                                                            : c?.Department?.Ancestor);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             let gender = c?.Gender ?? (int)UtilConstants.Gender.Others
                             let fullName = ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                "<a href='"+@Url.Action("Details",new{id = c?.Id, type = c?.int_Role})+"'>"+ fullName +"</a>",
                                    c?.JobTitle?.Name,
                                    c?.Department?.Code,
                                    c?.bit_Internal == true ? UtilConstants.CourseAreas.Internal.ToString(): UtilConstants.CourseAreas.External.ToString(),                                
                                    (c?.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Employee(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Employee(1,"+c?.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                                   ((User.IsInRole("/Employee/Details")) ? "<a title='View' href='"+Url.Action("Details",new{id = c?.Id, type = c?.int_Role})+"' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>" : "" ) +
                                    ((User.IsInRole("/Employee/Modify")) ? verticalBar +"<a title='Edit' href='"+@Url.Action("Modify",new{id = c?.Id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>":"") 
                                    //+((User.IsInRole("/Employee/Delete")) ? verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c?.Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" :"")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandler", ex.Message);
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
        public ActionResult AjaxHandlerMonitor(jQueryDataTableParamModel param)
        {
            try
            {
                var userPermission = CurrentUser.PermissionIds;
                // xử lý param gửi lên
                var comOrDepId = string.IsNullOrEmpty(Request.QueryString["DepartmentList1"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList1"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus1"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus1"].Trim());
                var fJobTitle = string.IsNullOrEmpty(Request.QueryString["JobTitleList1"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitleList1"].Trim());
                var fGender = string.IsNullOrEmpty(Request.QueryString["Genders1"]) ? -1 : Convert.ToInt32(Request.QueryString["Genders1"].Trim());
                var ddlType = string.IsNullOrEmpty(Request.QueryString["Type1"]) ? -1 : Convert.ToInt32(Request.QueryString["Type1"].Trim());
                var fName = string.IsNullOrEmpty(Request.QueryString["fName1"]) ? string.Empty : Request.QueryString["fName1"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail1"]) ? string.Empty : Request.QueryString["fEmail1"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone1"]) ? string.Empty : Request.QueryString["fPhone1"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId1"]) ? string.Empty : Request.QueryString["fStaffId1"].Trim();
              
                var bitInternal = (ddlType == 1);
                var isActive = fStatus != 0;
                var lstDepartment = new List<int?>();

                if (comOrDepId != -1)
                {
                    var department = DepartmentService.GetById(comOrDepId);
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
                }



                var data = EmployeeService.Get(a => a.IsDeleted == false && a.Departments.Any(x=> x.headname.HasValue && x.headname == a.Id)
                //&& userPermission.Any(x => a.Department_Id == x)
                && (comOrDepId == -1 || lstDepartment.Contains(a.Department_Id))
                  && (ddlType == -1 || a.bit_Internal == bitInternal)
                && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())) || ((a.LastName.Trim() + " " + a.FirstName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.IsActive == isActive), true);

                IEnumerable<Trainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.LastName
                                                            : sortColumnIndex == 3 ? c?.JobTitle?.Name
                                                            : sortColumnIndex == 4 ? c?.Department?.Ancestor
                                                            : sortColumnIndex == 5 ? c?.bit_Internal
                                                            : sortColumnIndex == 6 ? c?.Trainee_Type?.Any()
                                                            : sortColumnIndex == 7 ? (object)c?.IsActive
                                                            : c?.Department?.Ancestor);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             let gender = c?.Gender ?? (int)UtilConstants.Gender.Others
                             let fullName = ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                "<a href='"+@Url.Action("Details",new{id = c?.Id, type = c?.int_Role})+"'>"+ fullName +"</a>",
                                    c?.JobTitle?.Name,
                                    c?.Department?.Code,
                                    c?.bit_Internal == true ? UtilConstants.CourseAreas.Internal.ToString(): UtilConstants.CourseAreas.External.ToString(),
                                    (c?.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Employee(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Employee(1,"+c?.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                                   ((User.IsInRole("/Employee/Details")) ? "<a title='View' href='"+@Url.Action("Details",new{id = c?.Id, type = c?.int_Role})+"' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>" : "" ) +
                                    ((User.IsInRole("/Employee/Modify")) ? verticalBar +"<a title='Edit' onclick='ModifyAllowance("+c?.Id+")' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>":"") 
                                    //+((User.IsInRole("/Employee/Delete")) ? verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c?.Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" :"")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandler", ex.Message);
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
        public ActionResult Modify(int? id, int? type)
        {
            var model = new EmployeeModelModify();

            model.Subjects = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true)
                      .OrderBy(a => a.Code)
                      .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            var entity = EmployeeService.GetById(id);
            if (entity != null)
            {
                model.Id = entity.Id;
                model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                model.Eid = entity.str_Staff_Id;
                model.InstructorSubjects =
                    entity.Examiner_Ability.Select(b => new EmployeeModelModify.EmployeeSubject()
                    {
                        Allowance = b.Allowance ?? 0,
                        Id = b.id,
                        Name = b.SubjectDetail.Name,
                        Code = b.SubjectDetail.Code,
                        SubjectId = b.SubjectDetailId ?? 0,
                        InstructorId = b.ExaminerId ?? 0,
                        CreateDate = b.CreateDate
                    }) ;
                model.RelevantDepartmentId = entity.Trainee_TrainingCenter.Select(a => a.khoidaotao_id).ToList();
            }         
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Modify(EmployeeModelModify model)
        {
            try
            {
                var entity = EmployeeService.ModifyExaminerAbility(model);          
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "AcademicStaff/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxHandlerListSelectSubject(string id, int pageIndex, int pageSize, FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["form[FilterCodeOrName]"])
                ? string.Empty
                : form["form[FilterCodeOrName]"].Trim().ToLower();
            var checkselectvalue_ = !String.IsNullOrEmpty(form["form[int_khoidaotao][]"]) ? form["form[int_khoidaotao][]"].Split(new char[] { ',' }).Select(Int32.Parse).ToList() : new List<int>();
            StringBuilder HTML = new StringBuilder();
            if (pageIndex == 0)
            {
                HTML.Append("<ul>");
                HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>&nbspOther</span>");
                HTML.Append("<ul>");
            }

            int traineeId = !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
            var datakhoidaotao = _repoSubjectService.GetTrainingCenter(a => checkselectvalue_.Any(x => x == a.khoidaotao_id)).Select(a => a.subject_id);
            var data = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && (!datakhoidaotao.Any() || datakhoidaotao.Contains(a.Id)) && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General && (string.IsNullOrEmpty(filterCodeOrName) || a.Code.Trim().ToLower().Contains(filterCodeOrName) || a.Name.Trim().ToLower().Contains(filterCodeOrName))).OrderBy(n => n.Id);
            if (data.Any())
            {
                var listsubject = EmployeeService.GetById(traineeId)?.Examiner_Ability?.Select(a => a.SubjectDetailId);
                var data_ = data.Skip(pageIndex * pageSize).Take(pageSize);
                foreach (var item in data_)
                {
                    HTML.Append("<li>");
                    HTML.AppendFormat(" <input data-id='{0}' data-parentname='Other' data-name='{2}'   multiple value='{0}' class='InstructorAbility' name='InstructorAbility' id='InstructorAbility_" + item.Id + "' type='checkbox' " + ((listsubject != null && listsubject.Contains(item.Id)) ? "Checked" : "") + " /><input type='hidden' value='{1}' name='InstructorAbility2' /><label for='InstructorAbility_" + item.Id + "'>&nbsp{2}</label>", item.Id, item.Id, item.Name);
                    HTML.Append("</li>");
                }
            }


            HTML.Append("</ul>");
            HTML.Append("</li>");
            HTML.Append("</ul>");

            return Json(HTML.ToString());
        }
        [AllowAnonymous]
        public ActionResult ReloadAjax(int id)
        {
            var entity = EmployeeService.GetById(id);
            if(entity != null)
            {
                return Json(new 
                {
                    name = ReturnDisplayLanguage(entity?.FirstName, entity?.LastName),
                    code = entity.str_Staff_Id,
                    cost = entity.Monitor_Ability?.FirstOrDefault()?.Allowance ?? 0,
                    result = true,
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                result = false
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SaveAllowance(int id, decimal? allowance )
        {
            var entity = EmployeeService.GetById(id);
            if (entity != null)
            {
                EmployeeService.ModifyAllowance(id, allowance??0);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SUCCESS),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.ISVALID_DATA),
                result = false
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult Details(int? id, int? type)
        {
            if (
                 (type == (int)UtilConstants.ROLE.Trainee ||
                  type == (int)UtilConstants.ROLE.Instructor
                     ))
            {
                var entity = EmployeeService.GetById(id);
                if (entity != null)
                {
                    var model = new EmployeeModelDetails();
                    //if (type != entity.int_Role)
                    //{
                    //    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                    //    return RedirectToAction("Index", "Home");
                    //}

                    #region [Hannah Mentor]
                    var keyHannahMentor = GetByKey("MENTOR");
                    model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                    if (model.CheckHannahMentor)
                    {
                        var hannahMentor = string.Empty;
                        if (entity.Trainee_Type.Any())
                        {
                            foreach (var item in entity.Trainee_Type)
                            {
                                hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                            }
                        }
                        model.HannahMentor = hannahMentor;

                    }
                    #endregion


                    model.Control = type.Value;
                    model.Id = entity.Id;
                    model.Avatar = entity.avatar;
                    model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                    model.Eid = entity.str_Staff_Id;
                    model.PersonId = entity.PersonalId;
                    model.Passport = entity.Passport;
                    model.Email = entity.str_Email;
                    model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Type = entity.bit_Internal == true
                        ? UtilConstants.CourseAreas.Internal.ToString()
                        : UtilConstants.CourseAreas.External.ToString();
                    model.PlaceOfBirth = entity.str_Place_Of_Birth;
                    model.Department = entity.Department?.Code + " " + entity.Department?.Name;
                    model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                    model.Jobtitle = entity.JobTitle?.Name;
                    model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                    model.Company = entity.Company?.str_Name;
                    model.Phone = entity.str_Cell_Phone;
                    model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                    model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Nation = entity.Nation;
                    return View(model);
                }

            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return RedirectToAction("Index", "Home");
        }
    }
}