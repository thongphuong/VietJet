using System;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;

namespace TrainingCenter.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DAL.Entities;
    using OfficeOpenXml;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ViewModel.RoleMenus;
    using TMS.Core.Services;

    public class MenuController : BaseAdminController
    {
        //
        // GET: /Menu/
        private string[] functionCols = new[] { "Action", "Controller", "Type", "Method" };
        private string[] ValidValue = new[] { "1", "true" };

        #region init


        #endregion

        public MenuController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult AjaxHandlerListGroupFunctions(jQueryDataTableParamModel param)
        {
            var entities = ConfigService.GetGroupFunctions().ToList().OrderByDescending(a => a.IsActive).ThenBy(a => a.Name);
            var displayed = entities.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var verticalBar = GetByKey("VerticalBar");
            var result = from c in displayed.ToArray()
                select new object[]
                {
                    string.Empty,
                    c.Name,
                    c.IsActive,
                    "<a href='"+@Url.Action("Modify",new{id = c.Id})+"')' class='btn btn-danger' title='Modify'><i class='fa fa-pencil'></i> </a>"
                +verticalBar +"<button class='btn btn-danger expand' title='List children' data-id='"+ c.Id +"'><i class='fa fa-plus-circle'></i> </button>"
                };
           
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = entities.Count(),
                iTotalDisplayRecords = entities.Count(),
                aaData = result
            },
          JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerChildMenu(jQueryDataTableParamModel param, int? id)
        {
            if (!id.HasValue) return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = 0,
                iTotalDisplayRecords = 0,
                aaData = new List<Function>()
            },
           JsonRequestBehavior.AllowGet);

            var entities = ConfigService.GetGroupFunctions(a => a.Id == id).SelectMany(a => a.GroupPermissionFunctions.Select(x => x.Function)).ToList();
            var data = entities;

           
            var result = data.Select(a => new object[]
            {
                a.Id,a.Name,a.UrlAddress
            }).ToList();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = entities.Count(),
                iTotalDisplayRecords = entities.Count(),
                aaData = result
            },
          JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        public ActionResult Modify(int? id)
        {
            var model = new GroupFunctionViewModel()
            {
                ListFunctions = ConfigService.GetMenuFunctions(a => a.ActionType != 0).ToDictionary(a => a.Id, a => a.Name),
            };
            if (!id.HasValue)
            {
                return View(model);
            }
            var entity = ConfigService.GetGroupFunctions(a => a.Id == id).FirstOrDefault();
            if (entity == null)
                return RedirectToAction("Index", "Menu");
            model.Id = entity.Id;
            model.Name = entity.Name;
            model.DefaultUrl = entity.DefaultUrl;
            model.IsActive = (bool)entity.IsActive;
            //model.Code = entity.Code;
            model.CurrentFunctions = entity.GroupPermissionFunctions.Select(a => (int)a.FunctionId).ToList();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Modify(GroupFunctionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ConfigService.UpdateGroupFunction(model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

            }
            model.ListFunctions = ConfigService.GetMenuFunctions().ToDictionary(a => a.Id, a => a.Name);
            return View(model);
        }


        [AllowAnonymous]
        public void GetImportExcel()
        {
            var asm = Assembly.GetExecutingAssembly();

            var actions = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type)) //filter controllers
                .SelectMany(type => type.GetMethods())
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)));
            var products = new System.Data.DataTable("sheet1");
            products.Columns.Add("STT", typeof(int));
            products.Columns.Add("Action", typeof(string));
            products.Columns.Add("Controller", typeof(string));
            var i = 1;
            foreach (var action in actions)
            {
                var controller = action.DeclaringType.Name.Replace("Controller", "");
                if (action.ReturnType == typeof(ActionResult))
                {
                    products.Rows.Add(i++, action.Name, controller);
                }
            }

            var grid = new GridView();
            grid.DataSource = products;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=MyExcelFile.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            var sw = new StringWriter();
            var htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportFunctions(HttpPostedFileWrapper postedFile)
        {
            try
            {
                //var file = SaveFile(postedFile);
                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => functionCols.Contains(a)))
                        return null;
                    var listDepartments = new Dictionary<string, FunctionViewModel>();
                    for (var i = 2; i <= worksheet.Tables[0].Address.Rows; i++)
                    {
                        var code = worksheet.Cells[i, 2].Text.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            var url = string.Format("/{0}/{1}", code, worksheet.Cells[i, 1].Text.Trim());
                            if (!listDepartments.ContainsKey(url))
                                listDepartments.Add(url, new FunctionViewModel()
                                {
                                    Action = worksheet.Cells[i, 1].Text.Trim(),
                                    Controller = worksheet.Cells[i, 2].Text.Trim(),
                                    Type = Convert.ToInt32(worksheet.Cells[i, 3].Text.Trim()),
                                    Method = Convert.ToInt32(worksheet.Cells[i, 4].Text.Trim()),
                                });
                        }
                    }
                    if (listDepartments.Any())
                    {
                        var newFuncs = new List<Function>();
                        var list = ConfigService.GetMenuFunctions().Select(a => a.UrlAddress).ToList();
                        foreach (var address in listDepartments)
                        {
                            var newAddress = list.FirstOrDefault(a => String.CompareOrdinal(address.Key, a) == 0);
                            if (newAddress == null)
                            {
                                var item = address.Value;
                                newFuncs.Add(new Function()
                                {
                                    Name = item.Controller + " " + item.Action,
                                    UrlAddress = address.Key,
                                    ActionMethod = item.Method,
                                    ActionType = item.Type
                                });
                            }
                        }
                        ConfigService.InsertFunctions(newFuncs);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [AllowAnonymous]
        public ActionResult Schedule()
        {
            var menu = ConfigService.GetMenu().Select(a => new MenuViewModel()
            {
                Id = a.ID,
                ParentId = a.PARENT_ID,
                IsMenu = a.ISMENU > 0,
                MenuIndex = a.SHOWORDER,
                MenuTitle = a.TITLE,
                Icon = a.ICON,
                Url = a.URL,
                Function = a.GroupFunctionId
            }).ToArray();
            ViewBag.functions = ConfigService.GetGroupFunctions().ToDictionary(a=>a.Id,a=>a.Name); 
            return View(menu);
        }
          [AllowAnonymous]
        public ActionResult MenuListView()
        {
            var menu = ConfigService.GetMenu().Select(a => new MenuViewModel()
            {
                Id = a.ID,
                ParentId = a.PARENT_ID,
                IsMenu = a.ISMENU > 0,
                MenuIndex = a.SHOWORDER,
                MenuTitle = a.TITLE,
                Icon = a.ICON,
                Url = a.URL,
                Function = a.GroupFunctionId,
                Ancestor = a.Ancestor,
                Functionlist = a.GroupFunction
            });
            return PartialView("_Partials/_MenuListView", menu);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Schedule(MenuViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ConfigService.UpdateMenu(model);
                    return Json(new { result = true, message = "Update success" });
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, message = ex.Message });
                }
            }
            return Json(new { result = false, message = "Invalid data" });
        }
        [AllowAnonymous]
        public ActionResult ModifySchedule(int? id)
        {

            var model = new MenuViewModel()
            {
                ListFunctions = ConfigService.GetGroupFunctions().ToDictionary(a => a.Id, a => a.Name),
            };
            var menulist = ConfigService.GetMenu();
            if (id.HasValue)
            {

                var entity = ConfigService.GetMenuById(id);
                if (entity == null)
                {
                    // model.Notify = "Department is not found";
                }
                else
                {
                    menulist = menulist.Where(a => !a.Ancestor.StartsWith(entity.Ancestor));
                    model.Id = entity.ID;
                    model.MenuTitle = entity.NAME;
                    model.MenuIndex = entity.SHOWORDER;
                    model.Icon = entity.ICON;
                    model.ParentId = entity.PARENT_ID;
                    // model.IsActive = entity.ISACTIVE;
                    //model.Code = entity.Code;
                    model.Function = entity.GroupFunctionId;
                }
            }
            model.Menu = menulist.OrderBy(a => a.Ancestor).ToDictionary(a => a.ID, a => a.NAME);
            return PartialView("_partials/_MenuModify", model);
        }
        [AllowAnonymous]
        public ActionResult CreateSchedule(int? id)
        {
            var user = GetUser();
            if (user == null) return null;
            // var code = "";
            var menu = ConfigService.GetMenu().OrderBy(a => a.Ancestor);
            var entity = menu.ToDictionary(a => a.ID, a => a.NAME);

            var model = new MenuViewModel()
            {
                Menu = entity,
                ParentId = id,
                ListFunctions = ConfigService.GetGroupFunctions().ToDictionary(a => a.Id, a => a.Name),
            };

            return PartialView("_Partials/_MenuModify", model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddNode(string title)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = false, message = "Invalid data" });
            }
            try
            {
               var model = new MenuViewModel()
               {
                   MenuIndex = 0,MenuTitle = title
               };
                ConfigService.InsertMenu(model , UtilConstants.MenuType.TraningCenter);
               
                return Json(new { result = true, data = model });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, data = title, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult DeleteItem(int? id)
        {

            try
            {
                ConfigService.DeleteMenu(id);
                return Json(new { result = true, data = id });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, data = id, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxhandlerListMenuFunctions(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    var functions = ConfigService.GetGroupFunctions(a => a.MenuId == id).Select(a => new
                    {
                        id = a.Id,
                        alias = a.Alias,
                        index = a.GroupIndex ?? 0
                    });
                    return Json(new { result = true, data = functions });
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, data = id, message = ex.Message });
                }
            }
            else
            {
                return Json(new { result = false, message = "menu is not found" });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxhandlerListFunctions()
        {
            try
            {
                var functions = ConfigService.GetGroupFunctions().Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    selectedMenu = a.MenuId
                });
                return Json(new { result = true, data = functions });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message });
            }
        }

    }
}
