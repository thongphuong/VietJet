using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using TMS.Core.Services;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels;

    public class GroupSubjectController : BaseAdminController
    {
        #region MyRegion

        private readonly ISubjectService _repoSubject ;

        public GroupSubjectController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService repoSubject, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
            _repoSubject = repoSubject;
        }

        #endregion
        //
        // GET: /Admin/User/
        #region Index
        // return view
        public ActionResult Index(int id = 0)
        {
            var groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;
            return View(groupsubject);
        }
        
        public ActionResult Modify(int id = 0,string type = "")
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            string pageshow = "";
            if(type == "view")
            {
                pageshow = "Detailpage";
            }
            else if (type == "edit")
            {
                pageshow = "Editpage";
            }
            else if (type == "create")
            {
                pageshow = "Createpage";
            }

            return PartialView(pageshow, groupsubject);
        }

        public ActionResult Editpage(int id = 0)
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            return View(groupsubject);
        }
        public ActionResult Detailpage(int id = 0)
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            return View(groupsubject);
        }
        public ActionResult Createpage(int id = 0)
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            return View(groupsubject);
        }

        
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString();
                string strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().ToString();
                string url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().ToString();

                var data = _repoSubject.GetGroupSubject(a =>
                (String.IsNullOrEmpty(strCode) || a.Code.Contains(strCode)) &&
                (String.IsNullOrEmpty(strName) || a.Name.Contains(strName))).OrderByDescending(a => a.id);
                

                IEnumerable<CAT_GROUPSUBJECT> filtered = data;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CAT_GROUPSUBJECT, object> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.Name
                                                          : sortColumnIndex == 3 ? c.CreatedDate
                                                          : sortColumnIndex == 4 ? (object)c.UpdatedDate
                                                          : c.CreatedDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if(sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         c.Code,
                         "<a href='javascript:void(0)' onclick='active("+c.id+",\"view\")'>"+c.Name+"</a>",
                         c.CreatedDate.HasValue ?  c.CreatedDate.Value.ToString("dd/MM/yyyy") : "",
                         c.UpdatedDate?.ToString("dd/MM/yyyy"),
                         ((Is_View(url)) ? "<a href='javascript:void(0)' onclick='active("+c.id+",\"view\")'><i class='fa fa-search font-byhoa' aria-hidden='true' ></i></a>" : "") +
                         ((Is_Edit(url)) ? verticalBar +"<a href='javascript:void(0)' onclick='active("+c.id+",\"edit\")'><i class='fas fa-edit font-byhoa' aria-hidden='true' ></i></a>" : "")+
                         ((Is_Delete(url)) ? verticalBar +"<a href='javascript:void(0)' onclick='calldelete(" + c.id  + ")'><i class='fas fa-trash font-byhoa' aria-hidden='true' ></i></a>" : "")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupSubject/AjaxHandler", ex.Message);
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

        #region User-Modify

        
        [HttpPost]
        public ActionResult Modify(FormCollection form)
        {
            var update = false;
            CAT_GROUPSUBJECT model;
            var id = Int32.Parse(form["id"].Trim());
            var strName = form["str_Name"].Trim();
            var strCode = form["str_Code"].Trim();
            var strDescription = form["str_Description"].Trim();


            Boolean validate = true;
            string messenge = "";
            if (CMSUtils.IsNull(strCode))
            {
                messenge += Messege.VALIDATION_CODE+ "<br/>";
                validate = false;
            }
            if (CMSUtils.IsNull(strName))
            {
                messenge += Messege.VALIDATION_NAME + "<br/>";
                validate = false;
            }
            if (validate == false)
                return Json(CMSUtils.alert("warning", messenge));
            if (id == 0)
            {
                DataTable db = CMSUtils.GetDataSQL("", "CAT_GROUPSUBJECT", "str_Code", string.Format("str_Code='{0}'", strCode), "");
                if (db.Rows.Count > 0)
                {
                    return Json(CMSUtils.alert("danger", Messege.EXISTING_CODE));
                }
            }

           

            // user_id = 0 create new else update.
            if (id == 0)
            {
                model = new CAT_GROUPSUBJECT { CreatedDate = DateTime.Now, CreatedBy= CurrentUser.Username };
            }
            else
            {
                update = true;
                model = _repoSubject.GetGroupSubjectById(id);
                model.UpdatedDate= DateTime.Now;
                model.UpdatedBy = CurrentUser.Username;
            }
            model.Name = strName;
            model.Code= strCode;
            model.Description = strDescription;
            model.IsActive= true;
            //model.dtm_Created_date = DateTime.Now;
            //model.int_Created_by = Int32.Parse(GetUser().USER_ID.ToString(CultureInfo.CurrentCulture));
            if (update)
            {
                _repoSubject.UpdateGroupSubject(model);
            }
            else
            {
                _repoSubject.InsertGroupSubject(model);
               
            }
            return Json(CMSUtils.alert("success", Messege.SUCCESS));
        }
        #endregion

        
        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = _repoSubject.GetGroupSubjectById(id);
                model.IsActive = false;
                _repoSubject.UpdateGroupSubject(model);
                return Json(CMSUtils.alert("success", Messege.SUCCESS));
            }
            catch (Exception ex)
            {
                return Json(CMSUtils.alert("success", Messege.UNSUCCESS));
            }

        }

    }
}
