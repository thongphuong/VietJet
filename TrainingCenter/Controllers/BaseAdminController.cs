﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Mvc;
using DAL.Entities;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.APIModels;
using Utilities;
namespace TrainingCenter.Controllers
{
    using System.IO;
    using System.Reflection;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Companies;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Departments;
    using TMS.Core.ViewModels.UserModels;
    using TMS.Core.ViewModels.Company;
    using TrainingCenter.CustomAuthorizes;
    using System.Web;
    using TMS.Core.Services;
    using System.Web.Helpers;

    using Newtonsoft.Json.Linq;
    using Amazon.S3;
    using Amazon.S3.Model;
    using TrainingCenter.Utilities;
    using System.Threading.Tasks;
    using System.Globalization;

    //[OutputCache(CacheProfile = "Cache_BaseAdmin")]
    [CustomAuthorize]
    public class BaseAdminController : Controller
    {
        protected readonly IConfigService ConfigService;
        protected readonly IUserContext UserContext;
        protected readonly INotificationService NotificationService;
        protected readonly ICourseMemberService CourseMemberService;
        protected readonly ICourseDetailService CourseDetailService;
        protected readonly IEmployeeService EmployeeService;
        protected readonly IDepartmentService DepartmentService;
        protected readonly ICourseService CourseService;
        protected readonly IApproveService ApproveService;

        protected readonly RestRequest RequestWsLms = new RestRequest(Method.POST);

        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }


        protected int StatusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
        protected int StatusModify = (int)UtilConstants.ApiStatus.Modify;
        protected int StatusUnSuccessfully = (int)UtilConstants.ApiStatus.UnSuccessfully;
        protected string keyCodeStart = UtilConstants.KEY_CORE_START;
        protected string keyCodeEnd = UtilConstants.KEY_CORE_END;
        public int count = 0;
        public BaseAdminController(IConfigService configService, IUserContext userContext, INotificationService notificationService,
            ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService)
        {

            ConfigService = configService;
            UserContext = userContext;
            CourseService = courseService;
            NotificationService = notificationService;
            CourseMemberService = courseMemberService;
            EmployeeService = employeeService;
            CourseDetailService = courseDetailService;
            DepartmentService = departmentService;
            ApproveService = approveService;
            
            //ViewBag.MenuConfig = GenerateMenu();
           // ViewBag.User = GetUser();
            ViewBag.PrivateLogo = UtilConstants.PrivateLogo;




            var currentUser = GetUser();

            if (currentUser != null)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                ViewBag.CountNotification = NotificationService.GetDetails(a => a.status == 0 && a.datesend >= timenow).Count();
                //ViewBag.CurrentUser = currentUser.FirstName + " " + currentUser.LastName;
                ViewBag.CurrentUser = ReturnDisplayLanguage(currentUser.FirstName, currentUser.LastName);
                ViewBag.Avatar = currentUser.Avatar;
                //EmployeeService.UserPermissions = currentUser.PermissionIds;
                var pathimage = "morning.png";
                var time = DateTime.Now.TimeOfDay.Hours;
                if (4 < time && time < 12)
                {
                    pathimage = "morning.png";
                }
                else if (12 < time && time < 18)
                {
                    pathimage = "afternoon.png";
                }
                else if (18 < time && time < 22)
                {
                    pathimage = "evening.png";
                }
                else if (22 < time && time < 5)
                {
                    pathimage = "night.png";
                }
                ViewBag.pathimage = pathimage;

            }
        }


        private void CheckFileDefault(string name)
        {
            var path = Server.MapPath(UtilConstants.PathImage);
            var isExists = System.IO.Directory.Exists(path);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var pathFile = path + name;
            var imgDefaultIsExists = System.IO.File.Exists(pathFile);
            if (!imgDefaultIsExists)
            {
                System.IO.File.Copy(Server.MapPath("/Content/img/") + name, pathFile);
            }
        }

        protected AjaxResponseViewModel SaveImage(HttpPostedFileBase fileupload, string EID, UtilConstants.Upload type)
        {
            var myFile = fileupload;
            var fsize = 2 * 1024 * 1024;
            var defaultNameImg = type == UtilConstants.Upload.Trainee ? "NoAvata.png" : "";

            var path = type == UtilConstants.Upload.Trainee ? UtilConstants.PathImage : GetByKey("PathEducation");
            if (type == UtilConstants.Upload.Trainee)
            {
                CheckFileDefault(defaultNameImg);
            }


            if (myFile != null && myFile.ContentLength != 0 && myFile.ContentLength < fsize)
            {
                var extensionFile = Path.GetExtension(myFile.FileName);
                if (Common.CheckImgFile(extensionFile))
                {
                    var nameFile = string.Empty;
                    if (type == UtilConstants.Upload.Trainee)
                    {
                        if (!string.IsNullOrEmpty(EID))
                        {
                            nameFile = EID + ".jpg";
                        }
                        else
                        {
                            var code = Common.RandomCode();
                            nameFile = code + ".jpg";
                        }

                    }
                    else
                    {
                        var random = new Random();
                        nameFile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
                        nameFile = EID + "_" + nameFile + random.Next(1, 100) + extensionFile;
                    }
                    var useAws = AppUtils.getAppSetting("UseAws");
                    //upload tam

                    if (useAws == "1" /*&& type == UtilConstants.Upload.Trainee*/)
                    {
                        path = path.Substring(1);
                        if (!AWSUtils.AWS_CheckFolderExists(path))
                        {
                            AWSUtils.AWS_RunFolderCreationDemo(path);
                        }
                        AWSUtils.AWS_PutObject(path + nameFile, myFile.InputStream);
                    }
                    else
                    {
                        CreateFolderIfExists(string.Empty, path);

                        var pathForSaving = Server.MapPath(path);
                        // var nameFile = string.Empty;
                        var random = new Random();

                        var pf = Path.Combine(pathForSaving, nameFile);
                        myFile.SaveAs(pf);
                    }
                    return new AjaxResponseViewModel
                    {
                        result = true,
                        data = nameFile,
                    };

                }
                else
                {
                    return new AjaxResponseViewModel
                    {
                        result = false,
                        message = string.Format(myFile.FileName + " is " + Resource.INVALIDDATA),
                    };
                }
            }
            else
            {
                //var defaultAvatar = new FileInfo(Server.MapPath("/Content/img/") + "NoAvata.png");
                //var extensionFile = defaultAvatar.Extension;
                //var pathForSaving = Server.MapPath(UtilConstants.PathImage);
                //var namefile = "";
                //namefile = DateTime.Now.ToString(UtilConstants.SuffixesDateFormat);
                //namefile = namefile + extensionFile;
                //System.IO.File.Copy(Server.MapPath("/Content/img/") + "NoAvata.png", pathForSaving + namefile);
                return new AjaxResponseViewModel
                {
                    result = true,
                    data = defaultNameImg,
                };
            }
        }

        private static string LocDau(string str)
        {
            //Thay thế và lọc dấu từng char      
            for (var i = 1; i < UtilConstants.VietNamChar.Length; i++)
            {
                for (var j = 0; j < UtilConstants.VietNamChar[i].Length; j++)
                    str = str.Replace(UtilConstants.VietNamChar[i][j], UtilConstants.VietNamChar[0][i - 1]);
            }
            return str;
        }
        protected AjaxResponseViewModel SaveImagePost(HttpPostedFileBase fileupload, int type = -1)
        {
            var myFile = fileupload;
            const int fsize = 2 * 1024 * 1024;
            var nameFolder = "PostNews";
            WebImage img = null;
            bool is_check;
            if (type != 2 && myFile != null && myFile.ContentLength != 0 && myFile.ContentLength < fsize)
            {
                var getKey = GetByKey("ReSizeImage");
                is_check = !string.IsNullOrEmpty(getKey) && int.Parse(getKey) == 1;
                if (is_check)
                {
                    img = new WebImage(myFile.InputStream);
                    img.Resize(541, 285, false, false);
                }
            }
            if (img != null)
            {
                var extensionFile = Path.GetExtension(myFile.FileName);
                if (!Common.CheckImgFile(extensionFile))
                    return new AjaxResponseViewModel
                    {
                        result = false,
                        message = string.Format(myFile.FileName + " is " + Resource.INVALIDDATA),
                    };

                CreateFolderIfExists(nameFolder, UtilConstants.PathImage);

                var pathForSaving = Server.MapPath(UtilConstants.PathImage);
                var nameFile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
                nameFile = nameFile + extensionFile;
                img.Save(Path.Combine(pathForSaving, nameFolder + "/" + nameFile));
                return new AjaxResponseViewModel
                {
                    result = true,
                    data = nameFolder + "/" + nameFile,
                };
            }
            else if (myFile != null && myFile.ContentLength != 0 && myFile.ContentLength < fsize)
            {
                var extensionFile = Path.GetExtension(myFile.FileName);
                if (!Common.CheckImgFile(extensionFile))
                    return new AjaxResponseViewModel
                    {
                        result = false,
                        message = string.Format(img.FileName + " is " + Resource.INVALIDDATA),
                    };
                CreateFolderIfExists(nameFolder, UtilConstants.PathImage);

                var pathForSaving = Server.MapPath(UtilConstants.PathImage);
                var nameFile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
                nameFile = nameFile + extensionFile;
                myFile.SaveAs(Path.Combine(pathForSaving, nameFolder + "/" + nameFile));
                return new AjaxResponseViewModel
                {
                    result = true,
                    data = nameFolder + "/" + nameFile,
                };
            }
            else
            {
                var defaultAvatar = new FileInfo(Server.MapPath("/Uploads/avatar/") + "empty-image.png");
                var extensionFile = defaultAvatar.Extension;
                var pathForSaving = Server.MapPath(UtilConstants.PathImage);
                CreateFolderIfExists(nameFolder, UtilConstants.PathImage);

                var namefile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
                namefile = namefile + extensionFile;
                System.IO.File.Copy(Server.MapPath("/Uploads/avatar/") + "empty-image.png", pathForSaving + nameFolder + "/" + namefile);
                return new AjaxResponseViewModel
                {
                    result = true,
                    data = nameFolder + "/" + namefile,
                };
            }
        }

       
        private string GenerateMenu()
        {
            //HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies["language"];

            var user = GetUser();
            if (user == null) return null;
            var menus = ConfigService.GetMenu();
            var menuHtml = new StringBuilder();
            string code = null;
            var lvl = 1;
            var currentlvl = 1;

            foreach (var menu in menus)
            {
                var menuItems = new StringBuilder();
                if (menu.GroupFunction != null && menu.GroupFunction.IsActive == true && !user.FunctionIds.ContainsKey(menu.GroupFunction.Id))
                { continue; }

                var span = "<span class='zmdi zmdi-plus'></span>";
                #region

                var checkremovemenu = menus.Where(a => a.Ancestor.StartsWith(menu.Ancestor + "_"));
                if (checkremovemenu.Any())
                {
                    var checkreturn = true; // gan fail mac dinh
                    foreach (var item in checkremovemenu)
                    {

                        if (item.GroupFunction != null && item.GroupFunction.IsActive == true &&
                            !user.FunctionIds.ContainsKey(item.GroupFunction.Id))
                        {
                            // true
                        }
                        else
                        {
                            checkreturn = false;
                        }


                    }
                    if (checkreturn) // true thi dung
                    {
                        continue;
                    }
                }
                else
                {
                    span = "";
                }
                #endregion


                if (string.IsNullOrEmpty(code) || !menu.Ancestor.StartsWith(code))
                {
                    code = menu.Code;
                    for (var i = 1; i < currentlvl; i++)
                    {
                        menuItems.Append("</ul>");
                    }
                    currentlvl = lvl = 1;
                }

                lvl = menu.Ancestor.Split(new char[] { '_' }).Count();

                if (lvl > currentlvl)
                {
                    menuItems.AppendFormat("<ul class='nav' style='margin-left:{0}px;'>", lvl * 5);
                }
                else if (lvl < currentlvl)
                {
                    menuItems.Append("</ul>");
                }
                currentlvl = lvl;

                //var menuname = ConfigService.GetMenuNameById(menu.ID);
                menuItems.Append("<li class='line_" + menu.ID + "'>");
                menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                //if (menuname == null)
                //{
                //    menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                //}
                //else
                //{
                //    if (true)//cookie.Value != menuname.Type)
                //    {
                //        menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                //    }
                //    else
                //    {
                //        menuItems.Append("<a title ='" + menuname.Name + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menuname.Name + span + "</a>");
                //    }
                //}


                menuHtml.Append(menuItems);
            }
            for (var i = 1; i < currentlvl; i++)
            {
                menuHtml.Append("</ul>");
            }

            return menuHtml.ToString();
        }
        private string ReturnColumnCheck(GroupFunction menu, int? idRole, int? type, int[] allPermission)
        {
            if (type == (int)UtilConstants.ROLE_FUNCTION.FullOption && menu.GroupPermissionFunctions.Any())
            {
                var rolePermission = menu.ROLEMENUs.Where(a => a.ROLE_ID == idRole && a.IsActive == true).Select(a => a.MenuType);
                var icon = allPermission.All(a => rolePermission.Any(x => x == a)) ? "fa fa-check-square-o" : "fa fa-square-o";
                var isCheck = allPermission.All(a => rolePermission.Any(x => x == a)) ? 0 : 1;

                var _return = "<i class='" + icon + "' aria-hidden='true' onclick='role.updateRole(" + idRole + "," + menu.Id + "," + type + "," + isCheck + ",this)'></i>";
                return _return;
            }
            if (menu.GroupPermissionFunctions.Any(a => a.Function.ActionType == type))
            {
                var permission = menu.ROLEMENUs.FirstOrDefault(a => a.ROLE_ID == idRole && a.MenuType == type);
                var icon = permission?.IsActive == true ? "fa fa-check-square-o" : "fa fa-square-o";
                var isCheck = permission?.IsActive == true ? 0 : 1;
                var _return = "<i class='" + icon + "' aria-hidden='true' onclick='role.updateRole(" + idRole + "," + menu.Id + "," + type + "," + isCheck + ",this)'></i>";
                return _return;
            }
            return "";
        }

        protected Func<Course, bool> FilterCourseStatus(List<int> approveType, int approveStatus)
        {
            if (approveStatus == -1 && approveType.Count() == 0)
            {
                return a => true;
            }
            else if (approveStatus != -1 && approveType.Count() > 0)
            {
                return a => a.TMS_APPROVES.FirstOrDefault(x => x.int_Type.HasValue && approveType.Contains(x.int_Type.Value))?.int_id_status == approveStatus;
            }
            else if (approveStatus == -1)
            {
                return
                  a =>
                       approveType.Contains(a.TMS_APPROVES.Any() ? a.TMS_APPROVES.FirstOrDefault().int_Type.Value : 0);
            }
            else
            {
                return a => a.TMS_APPROVES.Any(x => x.int_id_status == approveStatus);
            }
        }

        protected object ReturnTraineePoint(bool isFirstCheck, bool? isAvarage, Course_Result rs)
        {
            object point = null;
            try
            {
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                if (rs == null) return null;
                if (isFirstCheck)
                {
                    if (isAvarage.HasValue && isAvarage == true)
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
                    if (isAvarage.HasValue && isAvarage == true)
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
            catch (Exception ex)
            {

                return point;
            }
            
        }


        //============================ Check Role Account TMS============================================================//
        protected string Log(int? typelog, Course course = null, int? course_detail_id = -1)
        {
            StringBuilder Html_ = new StringBuilder();
            #region[log course]
            if (typelog == 1 && course != null)
            {
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var data = course;
                if (data != null)
                {
                    Html_.AppendFormat("<b>Course name:</b> {0}<br />", data?.Name);
                    Html_.AppendFormat("<b>Code course:</b> {0}<br />", data?.Code);
                    Html_.AppendFormat("<b>Venue:</b> {0}<br />", data?.Venue);
                    Html_.AppendFormat("<b>No-trainee:</b> {0}<br />", data?.NumberOfTrainee);
                    //var datatrainingcenter = _repoCourse_TrainingCenter.Get(a => a.course_id == data.Course_Id);
                    var datatrainingcenter = course.Course_TrainingCenter;
                    if (datatrainingcenter.Any())
                    {
                        Html_.Append("<b>Training center:</b> ");
                        foreach (var item in datatrainingcenter)
                        {
                            Html_.AppendFormat("- {0}.<br />", item?.Department?.Name);
                        }
                    }
                    Html_.AppendFormat("<b>Partner:</b> {0}<br />", data.Company?.str_Name);
                    Html_.AppendFormat("<b>Survey:</b> {0}<br />", data.Survey);
                    Html_.AppendFormat("<b>From-To:</b> {0} - {1}<br />", DateUtil.DateToString(data?.StartDate, "dd/MM/yyyy"), DateUtil.DateToString(data?.EndDate, "dd/MM/yyyy"));
                    Html_.AppendFormat("<b>Type:</b> {0}<br />", data.CourseTypeId);
                    Html_.AppendFormat("<b>Note:</b> {0}<br />", data.Note);
                    var datacoursedetail = course.Course_Detail;
                    if (datacoursedetail.Any())
                    {
                        Html_.Append("--------------------------------------------------------------------<br />");
                        foreach (var item in datacoursedetail)
                        {
                            var trainee = item?.Course_Detail_Instructor?.FirstOrDefault()?.Trainee;
                            //var fullName = trainee?.FirstName + " " + trainee?.LastName;
                            var fullName = ReturnDisplayLanguage(trainee?.FirstName, trainee?.LastName);

                            Html_.AppendFormat("<b>Subject:</b> {0}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Type learning: {1}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Instructor: {2}<br />", item?.SubjectDetail?.Name, item?.type_leaning, fullName);
                            Html_.AppendFormat("<b>Date From-To:</b> {0} - {1}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Time From-To: {2} - {3}<br />", DateUtil.DateToString(item?.dtm_time_from, "dd/MM/yyyy"), DateUtil.DateToString(item?.dtm_time_to, "dd/MM/yyyy"), (item?.time_from != null ? (item?.time_from?.Substring(0, 2) + ":" + item?.time_from?.Substring(2)) : ""), (item?.time_to != null ? (item?.time_to?.Substring(0, 2) + ":" + item?.time_to?.Substring(2)) : ""));
                            Html_.AppendFormat("<b>Room:</b> {0}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Duration: {1}<br />", item?.Room?.str_Name, item?.Duration);
                            Html_.Append("--------------------------------------------------------------------<br />");
                        }
                    }

                }
            }
            #endregion

            #region[log assigntrainee]
            else if (typelog == 2 && course != null)
            {
                var get1 = course.Course_Detail.Select(p => p.Id).ToList();
                var get2 = CourseMemberService.Get(a => get1.Contains((int)a.Course_Details_Id) && a.DeleteApprove == null).Select(a => a.Member_Id);
                var get3 = EmployeeService.Get(a => get2.Contains(a.Id));

                // get nhug user hoc full
                if (get3.Any())
                {
                    Html_.Append("<b>Learning full:</b><br />");
                    int count = 0;
                    foreach (var item in get3)
                    {
                        count++;
                        var data_learningfull = CourseMemberService.Get(a => a.Member_Id == item.Id && get1.Contains(a.Course_Detail.Id) && (a.IsDelete == null || a.IsDelete != true)).OrderByDescending(b => b.Id);//.GroupBy(a => a.Course_Details_Id).Select(a => a.FirstOrDefault())
                        if (data_learningfull.Any())
                        {
                            if (data_learningfull.Count() == get1.Count())
                            {
                                var trainee = item?.Course_Detail_Instructor?.FirstOrDefault()?.Trainee;
                                //var fullName = trainee?.FirstName + " " + trainee?.LastName;
                                var fullName = ReturnDisplayLanguage(trainee?.FirstName, trainee?.LastName);

                                Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item?.str_Staff_Id, fullName);
                            }
                        }
                    }
                }


                //var data_learningfull = _repoTMS_Course_Member.Get(a => get2.Contains(a.Member_Id) && get1.Contains(a.Course_Detail.Course_Detail_Id) && (a.IsDelete == null || a.IsDelete != 1), a => a.OrderByDescending(b => b.Id));//.GroupBy(a => a.Course_Details_Id).Select(a => a.FirstOrDefault())
                //if (data_learningfull.Any())
                //{
                //    Html_.Append("<b>Learning full:</b><br />");
                //    int count = 0;
                //    foreach (var item in data_learningfull)
                //    {
                //        count++;
                //        Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item?.Trainee?.str_Staff_Id, item?.Trainee?.str_Fullname);
                //    }
                //}
                // get user theo mon
                if (get1.Any())
                {
                    foreach (var item in get1)
                    {
                        var data_learningsubject = CourseMemberService.Get(a => get2.Contains(a.Member_Id) && a.Course_Detail.Id == item && (a.IsDelete == null || a.IsDelete != true)).OrderByDescending(b => b.Id);
                        if (data_learningsubject.Any())
                        {
                            var subjectname = CourseDetailService.Get(a => a.Id == item).Select(a => a.SubjectDetail.Name).FirstOrDefault();
                            if (subjectname != null)
                            {
                                Html_.AppendFormat("<b>{0}</b><br />", subjectname);
                            }
                            int count = 0;
                            foreach (var item_ in data_learningsubject)
                            {
                                var trainee = item_.Trainee;
                                //   var fullName = trainee?.FirstName + " " + trainee?.LastName;
                                var fullName = ReturnDisplayLanguage(trainee?.FirstName, trainee?.LastName);

                                count++;
                                Html_.AppendFormat("{0}&nbsp; &nbsp; &nbsp;<b>EID:</b> {1}&nbsp; &nbsp; &nbsp;<b>Name:</b> {2}<br />", count, item_?.Trainee?.str_Staff_Id, fullName);
                            }
                        }
                    }
                }


            }
            #endregion

            #region[log subject result]
            else if (typelog == 3 && course_detail_id != -1)
            {
                //TODO: implement store sp_GetSubjectResultGrid
                //var data = ctx.sp_GetSubjectResultGrid(course_detail_id.ToString()).OrderBy(a => a.Grade).ToList<sp_GetSubjectResultGrid_Result>();
                //if (data.Any())
                //{
                //    var count = 0;
                //    foreach (var item in data)
                //    {
                //        count++;
                //        Html_.AppendFormat("<b>" + count + "&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", item?.FullName, item?.Score, item?.Grade);
                //    }
                //}
            }
            #endregion

            #region[log course result]
            else if (typelog == 4 && course != null)
            {
                var data = course.Course_Result_Final.OrderByDescending(p => p.id);

                if (data.Any())
                {
                    var count = 0;
                    foreach (var item in data)
                    {

                        var trainee = item.Trainee;
                        //var fullName = trainee?.FirstName + " " + trainee?.LastName;
                        var fullName = ReturnDisplayLanguage(trainee?.FirstName, trainee?.LastName);
                        count++;
                        Html_.AppendFormat("<b>{3}&nbsp; &nbsp;&nbsp; &nbsp;Name:</b> {0}&nbsp; &nbsp;<br /> &nbsp; &nbsp;&nbsp; &nbsp;<b>Score:</b> {1}&nbsp; &nbsp; <b>Grade:</b> {2}&nbsp; &nbsp; <br />", fullName, item?.point, item?.grade, count);
                    }
                }
            }
            #endregion
            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            //Html_.AppendFormat("<b>by: <em>{0}</em></b>", currUser?.FirstName + " " + currUser?.LastName);
            Html_.AppendFormat("<b>by: <em>{0}</em></b>", ReturnDisplayLanguage(currUser?.FirstName, currUser?.LastName));
            return Html_.ToString();
        }

        protected int PageID(string url = null)
        {
            var url_ = "";
            if (url == null)
            {
                var controllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                var actionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                url_ = "/" + controllerName + "/" + actionName;
            }
            else
            {
                url_ = url;
            }


            int return_ = -1;
            //var data = _repoMENU.Get(a => a.URL.Contains(url_)).FirstOrDefault();
            var data = ConfigService.GetMenu(a => a.URL.Contains(url_)).FirstOrDefault();
            if (data != null)
            {
                return_ = data.ID;
            }
            return return_;
        }
        #region [Check ]

        protected void Checkdead()
        {
            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            if (currUser == null)
                System.Web.HttpContext.Current.Response.Redirect("/Authenticate");
        }
        #endregion
        #region [protected bool IsGlobalAdmin()]
        protected bool Is_GlobalAdmin()
        {
            if (CMSContext.CurrentAccount.GroupAccountID == 1)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region [protected bool IsView(object trangID)]
        protected bool Is_View(object trangID)
        {
            // get ROLE_ID
            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            return currUser != null;
        }
        #endregion
        #region [protected bool IsView()]
        protected bool Is_View(string url = null)
        {
            // get ROLE_ID
            //UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            //if (currUser == null)
            //    return false;
            //var roleId = Int32.Parse(currUser.RoleId.ToString());
            //var pageid = PageID(url);
            //var checkdata = ConfigService.GetRoleFuntions(a => a.int_Role_id == roleId && a.int_Page_id == pageid && a.bit_view == true);
            //if (checkdata.Count() > 0)
            //{
            return true;
            //}
            //return false;
        }
        #endregion
        #region [protected bool IsAdd()]
        protected bool Is_Add(string url = null)
        {
            // get ROLE_ID

            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            return currUser != null;
        }
        #endregion
        #region [protected bool IsEdit()]
        protected bool Is_Edit(string url = null)
        {
            // get ROLE_ID

            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            return currUser != null;
        }
        #endregion
        #region [protected bool IsDelete()]
        protected bool Is_Delete(string url = null)
        {
            // get ROLE_ID

            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            return currUser != null;
        }
        #endregion

        protected UserModel GetUser()
        {
            var userModel = System.Web.HttpContext.Current.Session?["UserA"];

            return (UserModel)userModel;
        }
        //public int Admin(int id = 7)
        //{
        //    var idadmin = UserService.GetById(id);
        //    return idadmin.ID;
        //}

        //Xài cho View Department Tree
        protected IEnumerable<DepartmentModel> GetDepartmentModel(bool isMaster = false)
        {
            if (isMaster)
            {
                var departments = DepartmentService.Get().OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });

                return departments;
            }
            else
            {
                var departments = DepartmentService.Get(a => CurrentUser.PermissionIds.Contains(a.Id) /*&& a.IsActive == true*/).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                return departments;
            }
        }

        //TODO:Cách mạng Department
        protected string GetDepartmentAcestorModel(bool isMaster = false, List<int?> id = null)
        {
            var result = string.Empty;
            var lvl = 1;
            if (isMaster)
            {
                var departments = DepartmentService.Get(a => a.IsActive == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                foreach (var item in departments)
                {
                    lvl = item.Ancestor.Count(x => x.Equals('!'));
                    var khoangtrang = "";
                    for (var i = 0; i < lvl; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;";
                    }
                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + ((id != null && id.Contains(item.Id)) ? "Selected" : "") + ">" + khoangtrang + "+ " + item.Code + " - " + item.DepartmentName;
                    result += "</option>";
                }
                return result;
            }
            else
            {
                var departments = DepartmentService.Get(a => CurrentUser.PermissionIds.Contains(a.Id) && a.IsActive == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                foreach (var item in departments)
                {
                    lvl = item.Ancestor.Count(x => x.Equals('!'));
                    var khoangtrang = "";
                    for (var i = 0; i < lvl; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;";
                    }
                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + ((id != null && id.Contains(item.Id)) ? "Selected" : "") + ">" + khoangtrang + (lvl == 0 ? "- " : "+ ") + item.Code + " - " + item.DepartmentName;
                    result += "</option>";
                }
                return result;
            }
        }

        protected string GetDepartmentAcestorModelCustom(bool isMaster = false, List<int?> id = null)
        {
            var result = string.Empty;
            var lvl = 1;
            if (isMaster)
            {
                var departments = DepartmentService.Get(a => a.is_training == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                foreach (var item in departments)
                {
                    lvl = item.Ancestor.Count(x => x.Equals('!'));
                    var khoangtrang = "";
                    for (var i = 0; i < lvl; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;";
                    }
                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + ((id != null && id.Contains(item.Id)) ? "Selected" : "") + ">" + khoangtrang + "+ " + item.Code + " - " + item.DepartmentName;
                    result += "</option>";
                }
                return result;
            }
            else
            {
                var departments = DepartmentService.Get(a => CurrentUser.PermissionIds.Contains(a.Id) && a.IsActive == true && a.is_training == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                foreach (var item in departments)
                {
                    lvl = item.Ancestor.Count(x => x.Equals('!'));
                    var khoangtrang = "";
                    for (var i = 0; i < lvl; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;";
                    }
                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + ((id != null && id.Contains(item.Id)) ? "Selected" : "") + ">" + khoangtrang + (lvl == 0 ? "- " : "+ ") + item.Code + " - " + item.DepartmentName;
                    result += "</option>";
                }
                return result;
            }
        }

        protected IEnumerable<DepartmentModel> GetParentDepartmentModel(bool isMaster = false)
        {
            if (isMaster)
            {
                var departments = DepartmentService.Get(a => a.ParentId == null).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel()
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                return departments;
            }
            else
            {
                var departments = DepartmentService.Get(a => CurrentUser.PermissionIds.Contains(a.Id) && a.ParentId == null).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel()
                {
                    Id = a.Id,
                    IsActive = (bool)a.IsActive,
                    DepartmentName = a.Name,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Code = a.Code
                });
                return departments;
            }
        }
        protected string MessageInvalidData(ModelStateDictionary modelState)
        {
            var errors = modelState.Where(a => a.Value.Errors.Any());

            var msg = new StringBuilder("<ul>");

            foreach (var error in errors)
            {

                msg.AppendLine(string.Format("<li class='text-danger'>{0}:<ul>", error.Key));
                foreach (var msgError in error.Value.Errors)
                {
                    msg.AppendLine(string.Format("<li>{0}</li><hr>", msgError.ErrorMessage));
                }
                msg.AppendLine("</ul></li>");
            }
            msg.AppendLine("</ul>");
            return msg.ToString();
        }

        //protected List<string> KeyInvalidData(ModelStateDictionary modelState)
        //{
        //    var errors = modelState.Where(a => a.Value.Errors.Any());

        //    return errors.Select(error => error.Key).ToList();
        //}

        protected int MarkInTms()
        {
            return (int)UtilConstants.MarkInTMS.Yes;
        }
        protected string GetDetailResultByCourseDetail(UtilConstants.DetailResult type, int? traineeId, int? courseDetailsId, int? input = null)
        {
            var _return = "";
            //var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).FirstOrDefault();
            //switch (type)
            //{
            //    case UtilConstants.DetailResult.Score:
            //        _return = data?.Score?.ToString() ?? _return;
            //        _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? (input == 1 ? "<input type='number'  class='form-control' name='txt_Score' value='" + _return + "'/>" : _return) : _return;
            //        _return += data?.Id != null ? "<input type='hidden' class='form-control' name='ResultId' value='" + data.Id + "'/>" : "<input type='hidden' class='form-control' name='ResultId' value='-1'/>";
            //        break;
            //    case UtilConstants.DetailResult.Grade:
            //        _return = data?.Score != null ? CourseDetailService.GetGradebyscore(courseDetailsId, data.Score) : _return;
            //        _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? (input == 1 ? "<input type='text'  class='form-control' name='txt_Grade' value='" + _return + "'/>" : _return) : _return;
            //        break;
            //    case UtilConstants.DetailResult.Remark:
            //        _return = data?.Remark ?? _return;
            //        _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? (input == 1 ? "<textarea class='form-control' name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>" : _return) : _return;
            //        break;
            //}

            var data = CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).FirstOrDefault();
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    _return = data?.point?.ToString() ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? (input == 1 ? "<input type='number'  class='form-control' name='txt_Score' value='" + _return + "'/>" : _return) : _return;
                    _return += data?.Id != null ? "<input type='hidden' class='form-control' name='ResultId' value='" + data.Id + "'/>" : "<input type='hidden' class='form-control' name='ResultId' value='-1'/>";
                    break;
                case UtilConstants.DetailResult.Grade:
                    _return = data?.point != null ? CourseDetailService.GetGradebyscore(courseDetailsId, data.point) : _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? (input == 1 ? "<input type='text'  class='form-control' name='txt_Grade' value='" + _return + "'/>" : _return) : _return;
                    break;

            }

            return _return;
        }


        protected string GetSumaryResult(int? traineeId, int? courseDetailsId, int? subjectDetailId)
        {
            var _return = string.Empty;
            var cd = CourseDetailService.GetById(courseDetailsId);
            if (cd.GradingMethod != null)
            {
                //lay diem cao nhat
                if (cd.GradingMethod == (int)UtilConstants.GradingMethod.HighestGrade)
                {
                    var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).OrderByDescending(a => a.Score).FirstOrDefault();
                    _return = (!string.IsNullOrEmpty(data?.Score.ToString()) ? data.Score : -1) + "_" + (!string.IsNullOrEmpty(data?.Result) ? data?.Result : UtilConstants.Grade.Fail.ToString()) + "_" + (!string.IsNullOrEmpty(data?.Remark) ? data?.Remark : string.Empty);
                }
                //lay diem trung binh
                else if (cd.GradingMethod == (int)UtilConstants.GradingMethod.AverageGrade)
                {
                    var data = CourseService
                        .GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).ToList();
                    var grade = UtilConstants.Grade.Fail.ToString();
                    var average = data.Average(a => a.Score) ?? -1;
                    var remark = data.Any(a => a.ModifiedAt != null) ? data.OrderByDescending(a => a.ModifiedAt).FirstOrDefault()?.Remark : data.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.Remark;
                    var typeFail = data.Any(a => a.Type == true);
                    var isAverageCalculate = data.FirstOrDefault()?.Course_Detail.SubjectDetail?.IsAverageCalculate;
                    if (typeFail)
                    {
                        grade = UtilConstants.Grade.Fail.ToString();
                    }
                    else
                    {
                        //true = checked
                        if (isAverageCalculate == false)
                        {
                            var strGrade =
                                data.OrderByDescending(a => a.Id).FirstOrDefault(
                                    a => a.CourseDetailId == courseDetailsId && a.TraineeId == traineeId)?.Result ?? string.Empty;

                            var isAverageCalculated = strGrade.Equals(UtilConstants.Grade.Fail.ToString()) ? UtilConstants.Grade.Fail.ToString() : UtilConstants.Grade.Pass.ToString();
                            grade = isAverageCalculated;
                        }
                        else
                        {
                            var subjectDetails =
                          CourseService.GetScores(a => a.subject_id == subjectDetailId).OrderByDescending(a => a.point_from);
                            foreach (var item in subjectDetails)
                            {
                                if (average >= item.point_from)
                                {
                                    grade = item.grade;
                                    break;
                                }
                            }

                        }

                    }
                    _return = average + "_" + (!string.IsNullOrEmpty(grade) ? grade : string.Empty) + "_" + (!string.IsNullOrEmpty(remark) ? remark : string.Empty);
                }
                //lay diem dau tien
                else if (cd.GradingMethod == (int)UtilConstants.GradingMethod.FirstAttempt)
                {
                    var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).OrderBy(a => a.Id).FirstOrDefault();
                    _return = (!string.IsNullOrEmpty(data?.Score.ToString()) ? data.Score : -1) + "_" + (!string.IsNullOrEmpty(data?.Result) ? data?.Result : UtilConstants.Grade.Fail.ToString()) + "_" + (!string.IsNullOrEmpty(data?.Remark) ? data?.Remark : string.Empty);
                }
                //lay diem cuoi cung
                else if (cd.GradingMethod == (int)UtilConstants.GradingMethod.LastAttempt)
                {
                    var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).OrderByDescending(a => a.Id).FirstOrDefault();
                    _return = (!string.IsNullOrEmpty(data?.Score.ToString()) ? data.Score : -1) + "_" + (!string.IsNullOrEmpty(data?.Result) ? data?.Result : UtilConstants.Grade.Fail.ToString()) + "_" + (!string.IsNullOrEmpty(data?.Remark) ? data?.Remark : string.Empty);
                }
            }
            return _return;
        }

        protected int NumberOfExams(int? courseDetailsId)
        {
            var num = 1;
            var data = CourseDetailService.GetById(courseDetailsId);
            if (data.AttemptsAllowed != null)
                num = data.AttemptsAllowed.Value;
            return num;
        }
        protected string GetMark(int? times, UtilConstants.DetailResult type, int? traineeId, int? courseDetailsId, int? input, int? isOnline, bool? isAverageCalculate = false)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId);
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    if (data.Any())
                    {
                        if (isAverageCalculate == false)
                        {
                            var item = data.OrderByDescending(a => a.Id).FirstOrDefault();
                            
                            _return +=
                               "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? item?.First_Check_Result : item?.Re_Check_Result) + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionaryVJ();
                            _return = averageCaculate.Aggregate(_return,
                                (current, i) =>
                                    current + ("<option value=" + i.Key + " " + (i.Value.Equals(times == 1 ? item?.First_Check_Result : item?.Re_Check_Result) ? "selected" : "") + " >" + (i.Value == "P" ? "Pass" : "Fail") + "</option>"));
                            _return += "</select>";

                        }
                        else
                        {
                            var i = 0;
                            foreach (var item in data)
                            {
                                if(item.First_Check_Score.HasValue)
                                {
                                    item.First_Check_Score = Math.Round(item.First_Check_Score.Value,1);
                                }
                                if (item.Re_Check_Score.HasValue)
                                {
                                    item.Re_Check_Score = Math.Round(item.Re_Check_Score.Value, 1);
                                }
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? (item?.First_Check_Score != -1 ? item?.First_Check_Score.ToString().Replace(",",".") : "") : (item?.Re_Check_Score != -1 ? item?.Re_Check_Score.ToString().Replace(",", ".") : "")) + "' onkeypress='return isNumber(event)' />" : _return);
                                i++;
                            }
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='" + i + "' /> ";

                            //if (NumberOfExams(courseDetailsId) > i)
                            //{
                            //    _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + "  class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            //}

                        }

                    }
                    else
                    {
                        if (isAverageCalculate == false)
                        {
                            _return +=
                                "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, item) =>
                                    current + ("<option value=" + item.Key + ">" + item.Value + "</option>"));
                            _return += "</select>";
                        }
                        else
                        {
                            _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='0' /> ";
                        }

                    }
                    break;
                case UtilConstants.DetailResult.Remark:
                    var firstOrDefault = data.OrderByDescending(a => a.Id).FirstOrDefault();
                    if (firstOrDefault != null) _return = firstOrDefault?.Remark ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? ("<textarea class='form-control Remark' name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>") : _return;
                    break;


            }
            return _return;
        }

        protected string GetMark_custom(int? times, UtilConstants.DetailResult type, ICollection<Course_Result> data_, int? traineeId, int? courseDetailsId, int? input, int? isOnline, bool? isAverageCalculate = false)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var _return = "";
            var data = data_.Where(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId);
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    if (data.Any())
                    {
                        if (isAverageCalculate == false)
                        {
                            var item = data.OrderByDescending(a => a.Id).FirstOrDefault();

                            _return +=
                               "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? item?.First_Check_Result : item?.Re_Check_Result) + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionaryVJ();
                            _return = averageCaculate.Aggregate(_return,
                                (current, i) =>
                                    current + ("<option value=" + i.Key + " " + (i.Value.Equals(times == 1 ? item?.First_Check_Result : item?.Re_Check_Result) ? "selected" : "") + " >" + (i.Value == "P" ? "Pass" : "Fail") + "</option>"));
                            _return += "</select>";

                        }
                        else
                        {
                            var i = 0;
                            foreach (var item in data)
                            {
                                if (item.First_Check_Score.HasValue)
                                {
                                    item.First_Check_Score = Math.Round(item.First_Check_Score.Value, 1);
                                }
                                if (item.Re_Check_Score.HasValue)
                                {
                                    item.Re_Check_Score = Math.Round(item.Re_Check_Score.Value, 1);
                                }
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? (item?.First_Check_Score != -1 ? item?.First_Check_Score.ToString().Replace(",", ".").Replace("-1","0") : "0") : (item?.Re_Check_Score != -1 ? item?.Re_Check_Score.ToString().Replace(",", ".") : "0")) + "' onkeypress='return isNumber(event)' />" : _return);
                                i++;
                            }
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='" + i + "' /> ";

                            //if (NumberOfExams(courseDetailsId) > i)
                            //{
                            //    _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + "  class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            //}

                        }

                    }
                    else
                    {
                        if (isAverageCalculate == false)
                        {
                            _return +=
                                "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, item) =>
                                    current + ("<option value=" + item.Key + ">" + item.Value + "</option>"));
                            _return += "</select>";
                        }
                        else
                        {
                            _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='0' /> ";
                        }

                    }
                    break;
                case UtilConstants.DetailResult.Remark:
                    var firstOrDefault = data.OrderByDescending(a => a.Id).FirstOrDefault();
                    if (firstOrDefault != null) _return = firstOrDefault?.Remark ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? ("<textarea class='form-control Remark' name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>") : _return;
                    break;


            }
            return _return;
        }

        protected string GetlblResult(UtilConstants.DetailResult type, int? traineeId, int? courseDetailsId, int? input, int? isOnline, bool? isAverageCalculate = false, string grade = "")
        {
            var _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).ToList();
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    if (data.Any())
                    {
                        if (isAverageCalculate == false)
                        {

                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, i) =>
                                    current + ("<option value=" + i.Key + " " + (i.Value.Equals(grade) ? "selected" : "") + " >" + i.Value + "</option>"));
                            _return += "</select>";

                        }
                        else
                        {
                            var i = 0;
                            foreach (var item in data)
                            {
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               item?.Score + "' onkeypress='return isNumber(event)' />" : _return);
                                i++;
                            }
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='" + i + "' /> ";

                            if (NumberOfExams(courseDetailsId) > i)
                            {
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + "  class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            }

                        }

                    }
                    else
                    {
                        if (isAverageCalculate == false)
                        {
                            _return +=
                                "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><select id='IsAverageCaculate' onchange='CalculatedClick(this)' name='IsAverageCaculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, item) =>
                                    current + ("<option value=" + item.Key + ">" + item.Value + "</option>"));
                            _return += "</select>";
                        }
                        else
                        {
                            _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='0' /> ";
                        }

                    }
                    break;
                case UtilConstants.DetailResult.Remark:
                    var firstOrDefault = data.OrderByDescending(a => a.Id).FirstOrDefault();
                    if (firstOrDefault != null) _return = firstOrDefault?.Remark ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? ("<textarea class='form-control Remark' name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>") : _return;
                    break;


            }
            return _return;
        }

        protected string GetMarkCustom(int? times, UtilConstants.DetailResult type, int? traineeId, int? courseDetailsId, int? input, int? isOnline, bool? isAverageCalculate = false, string grade = "")
        {
            var _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).ToList();
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    if (data.Any())
                    {
                        if (isAverageCalculate == false)
                        {
                            var item = data.OrderByDescending(a => a.Id).FirstOrDefault();

                            _return +=
                               "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? item?.First_Check_Result : item?.Re_Check_Result) + "' onkeypress='return isNumber(event)' /> ";

                            if (times == 1)
                            {
                                if (!string.IsNullOrEmpty(item?.First_Check_Result))
                                {
                                    _return += "<input type='text' readonly='readonly' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                             (item?.First_Check_Result == "P" ? "Pass" : "Fail") + "' onkeypress='return isNumber(event)' /> ";
                                }



                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(item?.Re_Check_Result))
                                {
                                    _return += "<input type='text' class='form-control' readonly='readonly' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                             (item?.Re_Check_Result == "P" ? "Pass" : "Fail") + "' onkeypress='return isNumber(event)' /> ";
                                }

                            }

                        }
                        else
                        {
                            var i = 0;
                            foreach (var item in data)
                            {
                                _return += (input == 1 ? "<input type='text' readonly='readonly' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (times == 1 ? (item?.First_Check_Score != -1 ? item?.First_Check_Score.ToString() : "") : (item?.Re_Check_Score != -1 ? item?.Re_Check_Score.ToString() : "")) + "' onkeypress='return isNumber(event)' />" : _return);
                                i++;
                            }
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='" + i + "' /> ";

                            //if (NumberOfExams(courseDetailsId) > i)
                            //{
                            //    _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + "  class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            //}

                        }

                    }
                    else
                    {
                        if (isAverageCalculate == false)
                        {
                            _return +=
                                "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><input type='text' class='form-control' readonly='readonly' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, item) =>
                                    current + ("<option value=" + item.Key + ">" + item.Value + "</option>"));
                            _return += "</select>";
                        }
                        else
                        {
                            _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='0' /> ";
                        }

                    }
                    break;
                case UtilConstants.DetailResult.Remark:
                    var firstOrDefault = data.OrderByDescending(a => a.Id).FirstOrDefault();
                    if (firstOrDefault != null) _return = firstOrDefault?.Remark ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? ("<textarea class='form-control Remark' readonly name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>") : _return;
                    break;


            }
            return _return;
        }
        protected void InsertSentMail(string mail_receiver, int typeSentMail, string bodySentMail, int? courseId, string subjectName = "")
        {
            try
            {

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("mail_receiver", mail_receiver);
                dic.Add("type_sent", typeSentMail);
                dic.Add("content_body", bodySentMail);
                dic.Add("flag_sent", 0);
                dic.Add("cat_mail_ID", ConfigService.GetMail(a => a.Type == typeSentMail)?.FirstOrDefault()?.ID + "");
                dic.Add("id_course", courseId); 
                dic.Add("Is_Deleted", 0);
                dic.Add("Is_Active", 1);
                dic.Add("subjectname", subjectName);
                if (CMSUtils.InsertDataSQLNoLog("TMS_SentEmail", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                {

                }
            }
            catch (Exception ex)
            {
                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogSourse.SendMail, UtilConstants.LogEvent.Insert, ex.Message);
            }

        }

        protected void InsertSentMail_Custom(int? cat_mail_ID,string mail_receiver, int typeSentMail, string bodySentMail, int? courseId, string subjectName = "")
        {
            try
            {

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("mail_receiver", mail_receiver);
                dic.Add("type_sent", typeSentMail);
                dic.Add("content_body", bodySentMail);
                dic.Add("flag_sent", 0);
                dic.Add("cat_mail_ID", cat_mail_ID + "");
                if(courseId.HasValue)
                {
                    dic.Add("id_course", courseId);
                }
                dic.Add("Is_Deleted", 0);
                dic.Add("Is_Active", 1);
                dic.Add("subjectname", subjectName);
                if (CMSUtils.InsertDataSQLNoLog("TMS_SentEmail", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                {

                }
            }
            catch (Exception ex)
            {
                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogSourse.SendMail, UtilConstants.LogEvent.Insert, ex.Message);
            }

        }


        protected string TypeLearningIcon(int? type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='Class'><i class='fas fa-chalkboard-teacher'  style='color:green;'></i></a>";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='E-Learning'><i class='fa fa-desktop' style='color:red;'></i></a>";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='cRo'><i class='fas fa-book-reader' style='color:royalblue;'></i></a>";
                    break;
            }
            return str_type;
        }
        protected string TypeLearning(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "cR";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "eL";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "cRo";
                    break;
            }
            return str_type;
        }
        protected string TypeLearningName(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "cR";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "eL";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "cRo";
                    break;
            }
            return str_type;
        }
        protected string GetCourseType(int? type)
        {
            var strType = "";
            switch (type)
            {
                case (int)UtilConstants.CourseTypes.Initial:
                    strType = "Initial";
                    break;
                case (int)UtilConstants.CourseTypes.Recurrent:
                    strType = "Recurrent";
                    break;
                case (int)UtilConstants.CourseTypes.ReQualification:
                    strType = "Re - Qualification";
                    break;
                case (int)UtilConstants.CourseTypes.Upgrade:
                    strType = "Upgrade";
                    break;
                case (int)UtilConstants.CourseTypes.Bridge:
                    strType = "Bridge";
                    break;
            }
            return strType;
        }

        protected string GetGrace(int? grade)
        {
            var strGrade = "";
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    strGrade = "Fail";
                    break;
                case (int)UtilConstants.Grade.Pass:
                    strGrade = "Pass";
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    strGrade = "Distinction";
                    break;
            }
            return strGrade;
        }



        protected void SendNotification(UtilConstants.NotificationType type, int? typelog, int? idApproval, int? to, DateTime? datesend, string messenge, string messengeContent, string messengeVn, string messengeContentVn)
        {
            NotificationService.Notification_Insert((int)type, typelog, idApproval, to ?? -1, datesend, messenge, messengeContent, messengeVn, messengeContentVn);
        }




        protected UserModel GetSession()
        {
            UserModel currUser = (UserModel)System.Web.HttpContext.Current.Session["UserA"];
            if (currUser == null)
                System.Web.HttpContext.Current.Response.Redirect("/Authenticate");
            return currUser;
        }


        protected bool CheckSiteConfig(string key)
        {
            var currentUser = GetUser();
            return currentUser.ConfigSite.Contains(key);
        }

        protected string GetByKey(string key)
        {
            return ConfigService.GetByKey(key);
        }


        protected string BodySendMail(UtilConstants.TypeSentEmail cAT_MAIL, USER uSER = null, Trainee trainee = null, Course Program = null, TMS_APPROVES tmsApprove = null, int? type = -1)
        {
            var body = ConfigService.GetMail(a => a.Type == (int)cAT_MAIL).FirstOrDefault()?.Content;
            if (body != null)
            {
                if (uSER != null)
                {
                    var pass = uSER.PASSWORD != null ? Common.DecryptString(uSER.PASSWORD) : "";
                    body = body.Replace(UtilConstants.MAIL_USER_USERNAME, uSER.USERNAME)
                        //.Replace(UtilConstants.MAIL_USER_FULLNAME, uSER.FIRSTNAME.Trim() + " " + uSER.LASTNAME.Trim())
                        .Replace(UtilConstants.MAIL_USER_FULLNAME, ReturnDisplayLanguage(uSER.FIRSTNAME.Trim(), uSER.LASTNAME.Trim()))

                        .Replace(UtilConstants.MAIL_USER_PASSWORD, pass)
                        .Replace(UtilConstants.MAIL_USER_EMAIL, uSER.EMAIL)
                        .Replace(UtilConstants.MAIL_USER_PHONE, uSER.PHONENO);
                }
                if (trainee != null)
                {
                    int? grade;
                    if (Program != null)
                    {
                        grade = trainee.Course_Result_Final?.Where(a => a.courseid == Program.Id).FirstOrDefault()?.grade;
                    }
                    else
                    {
                        grade = 0;
                    }
                    var pass = trainee.Password != null ? Common.DecryptString(trainee?.Password) : "";
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_PASSWORD, pass)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, ReturnDisplayLanguage(trainee.FirstName,trainee.LastName))
                       //.Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, (cAT_MAIL == UtilConstants.TypeSentEmail.SendMailApproveToMail ? ReturnDisplayLanguage(uSER.FIRSTNAME, uSER.LASTNAME) : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName)))
                       .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                       .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone)
                       .Replace(UtilConstants.MAIL_TRAINEE_GRADE, GetGrace(grade));
                }
                if (Program != null)
                {
                    var strKey = "";
                    if (tmsApprove != null)
                    {
                        strKey = "id=" + tmsApprove.id + "&type=" + tmsApprove.int_Type;

                    }
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, Program.Name)
                       .Replace(UtilConstants.MAIL_PROGRAM_CODE, Program.Code)
                       .Replace(UtilConstants.MAIL_PROGRAM_STARTDATE, Program.StartDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, Program.EndDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_VENUE, Program.Venue)
                       .Replace(UtilConstants.MAIL_PROGRAM_MAXTRAINEE, Program.NumberOfTrainee.ToString())
                       .Replace(UtilConstants.MAIL_PROGRAM_NOTE, Program.Note)
                       .Replace(UtilConstants.MAIL_CODE, EncryptKeyEmail(strKey));

                    var itemcourse = "";
                    var listCourse = Program.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse.Count() > 0)
                    {
                        var count = 0;
                        itemcourse = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                        itemcourse += "<tbody>";
                        itemcourse += "<tr>";
                        itemcourse += "<th>" + @Resource.lblStt + "</th>";
                        itemcourse += "<th>" + @Resource.lblCode + "</th>";
                        itemcourse += "<th>" + @Resource.lblName + "</th>";
                        itemcourse += "<th>" + @Resource.lblStartDate + "</th>";
                        itemcourse += "<th>" + @Resource.lblEndDate + "</th>";
                        itemcourse += "<th>" + @Resource.lblMethod + "</th>";
                        itemcourse += "<th>" + @Resource.lblRoom + "</th>";
                        itemcourse += "<th>" + @Resource.lblInstructor + "</th>";
                        itemcourse += "<th>" + @Resource.lblRemark + "</th>";
                        if (type == 2)
                        {
                            itemcourse += "<th> Remark Assign </th>";
                        }
                        itemcourse += "</tr>";
                        var cos_span = listCourse.Count(x => x.TMS_Course_Member.Any(a => a.IsActive == true && a.Status != (int)UtilConstants.APIAssign.Pending && a.Member_Id == trainee.Id && a.Course_Details_Id == x.Id));
                        var count_row = 1;
                        foreach (var item in listCourse)
                        {
                            if(type == 1) // giáo viên
                            {
                                if(item.Course_Detail_Instructor.Any(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Instructor_Id == trainee.Id && a.Course_Detail_Id == item.Id))
                                {
                                    var instructor = "";
                                    var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id == item.Id).Select(a => a.Instructor_Id).ToList();
                                    if (InstructorIds.Any())
                                    {
                                        foreach (var teacherId in InstructorIds)
                                        {
                                            var teacher = EmployeeService.GetById(teacherId);
                                            if (teacher == null) continue;
                                            var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                            instructor += fullName;
                                        }
                                    }
                                    count++;
                                    itemcourse += "<tr>";
                                    itemcourse += "<td>" + count + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Code + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Name + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                                    itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                                    itemcourse += "<td>" + instructor + "</td>";
                                    itemcourse += "<td>" + (string.IsNullOrEmpty(item.str_remark) ? "" : Regex.Replace(item.str_remark, "[\r\n]", "<br/>")) + "</td>";
                                    itemcourse += "</tr>";
                                }
                                
                            } 
                            else if (type == 2)// học viên
                            {
                                if(item.TMS_Course_Member.Any(a=> a.IsActive == true && a.Status != (int)UtilConstants.APIAssign.Pending && a.Member_Id == trainee.Id && a.Course_Details_Id == item.Id))
                                {
                                    var instructor = "";
                                    var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => a.Instructor_Id).ToList();
                                    if (InstructorIds.Any())
                                    {
                                        foreach (var teacherId in InstructorIds)
                                        {
                                            var teacher = EmployeeService.GetById(teacherId);
                                            if (teacher == null) continue;
                                            var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                            instructor += fullName;
                                        }
                                    }
                                    count++;
                                    itemcourse += "<tr>";
                                    itemcourse += "<td>" + count + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Code + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Name + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                                    itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                                    itemcourse += "<td>" + instructor + "</td>";
                                    itemcourse += "<td>" + (string.IsNullOrEmpty(item.str_remark) ? "": Regex.Replace(item.str_remark, "[\r\n]", "<br/>")) + "</td>";
                                    if(count_row == 1)
                                    {
                                        itemcourse += "<td rowspan=" + cos_span + ">" + (string.IsNullOrEmpty(trainee.TMS_Course_Member_Remark.FirstOrDefault(b => b.TraineeId == trainee.Id && b.CourseId == Program.Id)?.remark) ? "" : Regex.Replace(trainee.TMS_Course_Member_Remark.FirstOrDefault(b => b.TraineeId == trainee.Id && b.CourseId == Program.Id)?.remark, "[\r\n]", "<br/>")) + "</td>";
                                    }
                                    
                                    itemcourse += "</tr>";
                                    count_row++;
                                }
                            }
                        }
                        itemcourse += "</tbody>";
                        itemcourse += "</table>";
                    }
                    body = body.Replace(UtilConstants.MAIL_LIST_COURSE, itemcourse);
                }

            }
            return body;
        }
        
		protected string BodySendMail_Custom(string body, USER uSER = null, Trainee trainee = null, Course Program = null, TMS_APPROVES tmsApprove = null, int? type = -1)
        {
            if (!string.IsNullOrEmpty(body))
            {
                if (uSER != null)
                {
                    var pass = uSER.PASSWORD != null ? Common.DecryptString(uSER.PASSWORD) : "";
                    body = body.Replace(UtilConstants.MAIL_USER_USERNAME, uSER.USERNAME)
                        //.Replace(UtilConstants.MAIL_USER_FULLNAME, uSER.FIRSTNAME.Trim() + " " + uSER.LASTNAME.Trim())
                        .Replace(UtilConstants.MAIL_USER_FULLNAME, ReturnDisplayLanguage(uSER.FIRSTNAME.Trim(), uSER.LASTNAME.Trim()))

                        .Replace(UtilConstants.MAIL_USER_PASSWORD, pass)
                        .Replace(UtilConstants.MAIL_USER_EMAIL, uSER.EMAIL)
                        .Replace(UtilConstants.MAIL_USER_PHONE, uSER.PHONENO);
                }
                if (trainee != null)
                {
                    int? grade;
                    if (Program != null)
                    {
                        grade = trainee.Course_Result_Final?.FirstOrDefault(a => a.courseid == Program.Id)?.grade;
                    }
                    else
                    {
                        grade = 0;
                    }
                    var pass = trainee.Password != null ? Common.DecryptString(trainee?.Password) : "";
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_PASSWORD, pass)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, ReturnDisplayLanguage(trainee.FirstName, trainee.LastName))
                       //.Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, (cAT_MAIL == UtilConstants.TypeSentEmail.SendMailApproveToMail ? ReturnDisplayLanguage(uSER.FIRSTNAME, uSER.LASTNAME) : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName)))
                       .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                       .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone)
                       .Replace(UtilConstants.MAIL_TRAINEE_GRADE, GetGrace(grade));
                }
                if (Program != null)
                {
                    var strKey = "";
                    if (tmsApprove != null)
                    {
                        strKey = "id=" + tmsApprove.id + "&type=" + tmsApprove.int_Type;

                    }
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, Program.Name)
                       .Replace(UtilConstants.MAIL_PROGRAM_CODE, Program.Code)
                       .Replace(UtilConstants.MAIL_PROGRAM_STARTDATE, Program.StartDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, Program.EndDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_VENUE, Program.Venue)
                       .Replace(UtilConstants.MAIL_PROGRAM_MAXTRAINEE, Program.NumberOfTrainee.ToString())
                       .Replace(UtilConstants.MAIL_PROGRAM_NOTE, Program.Note)
                       .Replace(UtilConstants.MAIL_CODE, EncryptKeyEmail(strKey));

                    var itemcourse = "";
                    var listCourse = Program.Course_Detail.Where(a => a.IsDeleted != true  && a.IsActive == true);
                    if (listCourse.Count() > 0)
                    {
                        var count = 0;
                        itemcourse = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                        itemcourse += "<tbody>";
                        itemcourse += "<tr>";
                        itemcourse += "<th>" + @Resource.lblStt + "</th>";
                        itemcourse += "<th>" + @Resource.lblCode + "</th>";
                        itemcourse += "<th>" + @Resource.lblName + "</th>";
                        itemcourse += "<th>" + @Resource.lblMethod + "</th>";
                        if (type == 2)
                        {
                            itemcourse += "<th> Status </th>";
                        }
                        itemcourse += "<th>" + @Resource.lblStartDate + "</th>";
                        itemcourse += "<th>" + @Resource.lblEndDate + "</th>";                      
                        itemcourse += "<th>" + @Resource.lblInstructor + "</th>";
                        itemcourse += "<th>" + @Resource.lblRoom + "</th>";
                        itemcourse += "<th>" + @Resource.lblRemark + "</th>";
                        //var cos_span = 0;
                        //if (type == 2)
                        //{
                        //    itemcourse += "<th> Remark Assign </th>";
                            
                        //    cos_span = listCourse.Count(x => x.TMS_Course_Member.Any(a => a.IsActive == true && a.Status != (int)UtilConstants.APIAssign.Pending && a.Member_Id == trainee.Id && a.Course_Details_Id == x.Id));
                        //}                      
                        itemcourse += "</tr>";
                        var count_row = 1;
                        foreach (var item in listCourse)
                        {
                            if (type == 1) // giáo viên
                            {
                                if (item.Course_Detail_Instructor.Any(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Instructor_Id == trainee.Id && a.Course_Detail_Id == item.Id))
                                {
                                    var instructor = "";
                                    var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id == item.Id);
                                    if (InstructorIds.Any())
                                    {
                                        foreach (var teacherId in InstructorIds)
                                        {
                                            var teacher = teacherId.Trainee;
                                            if (teacher == null) continue;
                                            var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                            instructor += fullName;
                                        }
                                    }
                                    count++;
                                    itemcourse += "<tr>";
                                    itemcourse += "<td>" + count + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Code + "</td>";
                                    itemcourse += "<td>" + item.SubjectDetail.Name + "</td>";
                                    itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + instructor + "</td>";
                                    itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";                                   
                                    itemcourse += "<td>" + (string.IsNullOrEmpty(item.str_remark) ? "" : Regex.Replace(item.str_remark, "[\r\n]", "<br/>")) + "</td>";
                                    itemcourse += "</tr>";
                                }

                            }
                            else if (type == 2)// học viên
                            {
                                if (item.TMS_Course_Member.Any(a => a.IsActive == true && a.Status != (int)UtilConstants.APIAssign.Pending && a.Member_Id == trainee.Id && a.Course_Details_Id == item.Id))
                                {
                                    var instructor = "";
                                    var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor);
                                    if (InstructorIds.Any())
                                    {
                                        foreach (var teacherId in InstructorIds)
                                        {
                                            var teacher = teacherId.Trainee;
                                            if (teacher == null) continue;
                                            var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                            instructor += fullName;
                                        }
                                    }
                                    count++;
                                    itemcourse += "<tr>";
                                    itemcourse += "<td>" + count + "</td>";
                                    itemcourse += "<td>" + item.Course.Code + " </td>";
                                    itemcourse += "<td style='word-break: break-word;'>" + item.SubjectDetail.Name + "</td>";
                                    itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                                    if (item.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline)
                                    {
                                        var blend = item.Course_Blended_Learning.Where(a => a.IsActive == true && a.IsDeleted != true && a.Course_Detail_Id == item.Id);
                                        var text = "";
                                        foreach (var item_blend in blend)
                                        {
                                            text += "-" + item_blend.LearningType + ": " + ((item_blend.DateFrom.HasValue ? item_blend.DateFrom.Value.ToString("dd/MM/yyyy") : "") + (item_blend.DateTo.HasValue ? "-" + item_blend.DateTo.Value.ToString("dd/MM/yyyy") : ""))+ (item_blend.Room == null ? "" : " => Room: "+item_blend.Room.str_Name) + "<br/>";

                                        }
                                        itemcourse += "<td>" + (!string.IsNullOrEmpty(text) ? text.Remove(text.Length - 5) : "") + "</td>";
                                    }
                                    else
                                    {
                                        itemcourse += "<td>" + UtilConstants.LearningTypesDictionaryMail()[(int)item.type_leaning]  + "</td>";
                                    }

                                    itemcourse += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                                    itemcourse += "<td>" + instructor + "</td>";

                                    //if (item.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline)
                                    //{
                                    //    var blend = item.Course_Blended_Learning.Where(a => a.IsActive == true && a.IsDeleted != true && a.Course_Detail_Id == item.Id);
                                    //    var text = "";
                                    //    foreach (var item_blend in blend)
                                    //    {
                                    //        text += "-" + item_blend.LearningType + ": " + (item_blend.Room == null ? "" : item_blend.Room.str_Name) + "<br/>";

                                    //    }
                                    //    itemcourse += "<td>" + (!string.IsNullOrEmpty(text) ? text.Remove(text.Length - 5) : "") + "</td>";
                                    //}
                                    //else
                                    //{
                                    //    itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                                    //}
                                    itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";


                                    itemcourse += "<td>" + (string.IsNullOrEmpty(item.str_remark) ? "" : Regex.Replace(item.str_remark, "[\r\n]", "<br/>")) + "</td>";
                                    //if (count_row == 1)
                                    //{
                                    //    itemcourse += "<td rowspan=" + cos_span + ">" + (string.IsNullOrEmpty(trainee.TMS_Course_Member_Remark.FirstOrDefault(b => b.TraineeId == trainee.Id && b.CourseId == Program.Id)?.remark) ? "" : Regex.Replace(trainee.TMS_Course_Member_Remark.FirstOrDefault(b => b.TraineeId == trainee.Id && b.CourseId == Program.Id)?.remark, "[\r\n]", "<br/>")) + "</td>";
                                    //}                                   
                                    itemcourse += "</tr>";
                                    count_row++;
                                }
                            }
                        }
                        itemcourse += "</tbody>";
                        itemcourse += "</table>";
                    }
                    body = body.Replace(UtilConstants.MAIL_LIST_COURSE, itemcourse);
                }

            }
            return body;
        }

        protected string GetMarkShow(UtilConstants.DetailResult type, int? traineeId, int? courseDetailsId, int? input, int? isOnline, bool? isAverageCalculate = false, string grade = "")
        {
            var _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).ToList();
            switch (type)
            {
                case UtilConstants.DetailResult.Score:
                    if (data.Any())
                    {
                        if (isAverageCalculate == false)
                        {
                            var item = data.OrderByDescending(a => a.Id).FirstOrDefault();

                            _return +=
                               "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               item?.Score + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, i) =>
                                    current + ("<option value=" + i.Key + " " + (i.Value.Equals(grade) ? "selected" : "") + " >" + i.Value + "</option>"));
                            _return += "</select>";

                        }
                        else
                        {
                            var i = 0;
                            foreach (var item in data)
                            {
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + item?.Id + "' value='" +
                                               (item?.Score != -1 ? item?.Score.ToString() : "") + "' onkeypress='return isNumber(event)' />" : _return);
                                i++;
                            }
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='" + i + "' /> ";

                            if (NumberOfExams(courseDetailsId) > i)
                            {
                                _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + "  class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            }
                        }
                    }
                    else
                    {
                        if (isAverageCalculate == false)
                        {
                            _return +=
                                "<input type='hidden' class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' /><select id='NoCalculate' onchange='CalculatedClick(this)' name='NoCalculate' class='form-control' ><option value='-1'>-- Select an option--</option>";
                            var averageCaculate = UtilConstants.ResultDictionary();
                            _return = averageCaculate.Aggregate(_return,
                                (current, item) =>
                                    current + ("<option value=" + item.Key + ">" + item.Value + "</option>"));
                            _return += "</select>";
                        }
                        else
                        {
                            _return += (input == 1 ? "<input type='text' " + (isOnline == (int)UtilConstants.LearningTypes.Online || isOnline == (int)UtilConstants.LearningTypes.OfflineOnline ? "readonly" : "") + " class='form-control' name='txt_trainee_" + traineeId + "_Score_" + -1 + "' onkeypress='return isNumber(event)' />" : _return);
                            _return += "  <input class='form-control' type='hidden' name='txt_time' value='0' /> ";
                        }

                    }
                    break;
                case UtilConstants.DetailResult.Remark:
                    var firstOrDefault = data.OrderByDescending(a => a.Id).FirstOrDefault();
                    if (firstOrDefault != null) _return = firstOrDefault?.Remark ?? _return;
                    _return = (MarkInTms() == (int)UtilConstants.MarkInTMS.Yes) ? ("<textarea class='form-control Remark' name='txt_Remark' rows='2' cols='3'>" + _return + "</textarea>") : _return;
                    break;


            }
            return _return;
        }
        public string bodyContent(int type, Course course)
        {
            var result = "";
            switch (type)
            {
                case (int)UtilConstants.ApproveType.Course:
                    var listCourse = course.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse.Any())
                    {
                        var count = 0;
                        result = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                        result += "<tbody>";
                        result += "<tr>";
                        result += "<th>" + @Resource.lblStt + "</th>";
                        result += "<th>" + @Resource.lblCode + "</th>";
                        result += "<th>" + @Resource.lblName + "</th>";
                        result += "<th>" + @Resource.lblStartDate + "</th>";
                        result += "<th>" + @Resource.lblEndDate + "</th>";
                        result += "<th>" + @Resource.lblMethod + "</th>";
                        result += "<th>" + @Resource.lblRoom + "</th>";
                        result += "<th>" + @Resource.lblInstructor + "</th>";
                        result += "</tr>";
                        foreach (var item in listCourse)
                        {
                            var instructor = "";
                            var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => a.Instructor_Id).ToList();
                            if (InstructorIds.Any())
                            {
                                foreach (var teacherId in InstructorIds)
                                {
                                    var teacher = EmployeeService.GetById(teacherId);
                                    if (teacher == null) continue;
                                    var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                    instructor += fullName;
                                }
                            }
                            count++;
                            result += "<tr>";
                            result += "<td>" + count + "</td>";
                            result += "<td>" + item.SubjectDetail.Code + "</td>";
                            result += "<td>" + item.SubjectDetail.Name + "</td>";
                            result += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                            result += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                            result += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                            result += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                            result += "<td>" + instructor + "</td>";
                            result += "</tr>";
                        }
                        result += "</tbody>";
                        result += "</table>";

                    }
                    break;
                case (int)UtilConstants.ApproveType.AssignedTrainee:
                    var listCourse2 = course.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse2.Any())
                    {

                        foreach (var item in listCourse2)
                        {
                            result += "<div style='color: #ffffff;background-color: #0882A5;border-color: #ecf0f1;'>";
                            result += "'" + item.SubjectDetail?.Code + "'" + "(" + item.SubjectDetail?.Name + ")" + "</div>";
                            result += "<br />";
                            var tmsMembers = item.TMS_Course_Member.Where(a => a.IsDelete == false && a.IsActive == true);
                            if (tmsMembers.Any())
                            {

                                result = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                                result += "<tbody>";
                                result += "<tr>";
                                result += "<th>" + @Resource.lblStt + "</th>";
                                result += "<th>" + @Resource.TRAINEE_EID + "</th>";
                                result += "<th>" + @Resource.lblFullName + "</th>";
                                result += "</tr>";
                                var count = 0;
                                foreach (var member in tmsMembers)
                                {
                                    var fullName = ReturnDisplayLanguage(member.Trainee?.FirstName, member.Trainee?.LastName);
                                    count++;
                                    result += "<tr>";
                                    result += "<td>" + count + "</td>";
                                    result += "<td>" + member.Trainee?.str_Staff_Id + "</td>";
                                    result += "<td>" + fullName + "</td>";
                                    result += "</tr>";
                                }
                            }
                        }
                        result += "</tbody>";
                        result += "</table>";


                    }
                    break;
                case (int)UtilConstants.ApproveType.SubjectResult:
                    var listCourse3 = course.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse3.Any())
                    {

                        foreach (var item in listCourse3)
                        {
                            result += "<div style='color: #ffffff;background-color: #0882A5;border-color: #ecf0f1;'>";
                            result += "'" + item.SubjectDetail?.Code + "'" + "(" + item.SubjectDetail?.Name + ")" + "</div>";
                            result += "<br />";
                            var tmsMembers = item.TMS_Course_Member.Where(a => a.IsDelete == false && a.IsActive == true);
                            if (tmsMembers.Any())
                            {

                                result = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                                result += "<tbody>";
                                result += "<tr>";
                                result += "<th>" + @Resource.lblStt + "</th>";
                                result += "<th>" + @Resource.TRAINEE_EID + "</th>";
                                result += "<th>" + @Resource.lblFullName + "</th>";
                                result += "</tr>";
                                var count = 0;
                                foreach (var member in tmsMembers)
                                {
                                    var fullName = ReturnDisplayLanguage(member.Trainee?.FirstName, member.Trainee?.LastName);
                                    count++;
                                    result += "<tr>";
                                    result += "<td>" + count + "</td>";
                                    result += "<td>" + member.Trainee?.str_Staff_Id + "</td>";
                                    result += "<td>" + fullName + "</td>";
                                    result += "</tr>";
                                }
                            }
                        }
                        result += "</tbody>";
                        result += "</table>";


                    }
                    break;
                case (int)UtilConstants.ApproveType.CourseResult:
                    break;

            }
            return result;
        }

        protected string BodyCertificate(CAT_CERTIFICATE catCertificate, USER user = null, Trainee trainee = null, Course program = null, Course_Result_Final final = null)
        {
            var body = catCertificate.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(program.Name) ? string.Empty : program.Name.ToUpper()))
                   //.Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : string.Format("{0} {1}", trainee.FirstName, trainee.LastName)))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName).ToUpper()))
                  .Replace(UtilConstants.CERTIFICATE_DATE_COMPLELTED, (final.CreateCertificateDate.HasValue ? DateUtil.DateToString(final.CreateCertificateDate, "dd/MM/yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(trainee.JobTitle?.Name) ? string.Empty : trainee.JobTitle.Name))
                   .Replace(UtilConstants.CERTIFICATE_GRADE, (final.grade.HasValue ? UtilConstants.GradeDictionary()[(int)final.grade] : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_POINT, (final.point?.ToString() ?? string.Empty))
                   //.Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(result?.CertificateSubject) ? string.Empty : result?.CertificateSubject))
                   //.Replace(UtilConstants.CERTIFICATE_ATO, (string.IsNullOrEmpty(final?.certificatefinal) ? string.Empty : final?.certificatefinal))                  
                   .Replace(UtilConstants.CERTIFICATE_DATE_OF_BIRTH, trainee.dtm_Birthdate.HasValue ? DateUtil.DateToString(trainee.dtm_Birthdate, "dd/MM/yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_PLACE_OF_BIRTH, !string.IsNullOrEmpty(trainee.str_Place_Of_Birth) ? trainee.str_Place_Of_Birth.ToUpper() : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_FROM, program.StartDate.HasValue ? DateUtil.DateToStringCertificate(program.StartDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_TO, program.EndDate.HasValue ? DateUtil.DateToStringCertificate(program.EndDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_TRAINEE_AVATAR, !string.IsNullOrEmpty(trainee?.avatar) ? "<img src='"+ ConfigurationSettings.AppSettings["AWSLinkS3"] + "Uploads/avatar/" + trainee?.avatar + "' width='175' height='240'>" : string.Empty)
                   ;
            return body;
        }
        protected string BodyCertificate_Custom(string point, string grade, DateTime? Enddate, CAT_CERTIFICATE catCertificate, USER user = null, Trainee trainee = null, Course program = null, Course_Result_Final final = null)
        {
            var body = catCertificate.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(program.Name) ? string.Empty : program.Name.ToUpper()))
                   //.Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : string.Format("{0} {1}", trainee.FirstName, trainee.LastName)))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName).ToUpper()))
                  .Replace(UtilConstants.CERTIFICATE_DATE_COMPLELTED, (final.CreateCertificateDate.HasValue ? DateUtil.DateToString(final.CreateCertificateDate, "dd/MM/yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(trainee.JobTitle?.Name) ? string.Empty : trainee.JobTitle.Name))
                   .Replace(UtilConstants.CERTIFICATE_GRADE, (!string.IsNullOrEmpty(grade) ? UtilConstants.GradeDictionary()[Convert.ToInt32(grade)] : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_POINT, point)
                   //.Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(result?.CertificateSubject) ? string.Empty : result?.CertificateSubject))
                   //.Replace(UtilConstants.CERTIFICATE_ATO, (string.IsNullOrEmpty(final?.certificatefinal) ? string.Empty : final?.certificatefinal))                  
                   .Replace(UtilConstants.CERTIFICATE_DATE_OF_BIRTH, trainee.dtm_Birthdate.HasValue ? DateUtil.DateToString(trainee.dtm_Birthdate, "dd/MM/yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_PLACE_OF_BIRTH, !string.IsNullOrEmpty(trainee.str_Place_Of_Birth) ? trainee.str_Place_Of_Birth.ToUpper() : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_FROM, program.StartDate.HasValue ? DateUtil.DateToStringCertificate(program.StartDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_TO, program.EndDate.HasValue ? DateUtil.DateToStringCertificate(Enddate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_TRAINEE_AVATAR, !string.IsNullOrEmpty(trainee?.avatar) ? "<img src='" + ConfigurationSettings.AppSettings["AWSLinkS3"] + "Uploads/avatar/" + trainee?.avatar + "' width='175' height='240'>" : string.Empty)
                   ;
            return body;
        }
        protected string BodyCertificateSubject(CAT_CERTIFICATE catCertificate, USER user = null, Trainee trainee = null, Course program = null, Course_Result result = null)
        {
            var body = catCertificate.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(program.Name) ? string.Empty : program.Name.ToUpper()))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName).ToUpper()))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(trainee.JobTitle?.Name) ? string.Empty : trainee.JobTitle.Name))
                   //.Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(result?.CertificateSubject) ? string.Empty : result?.CertificateSubject))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_NAME, (string.IsNullOrEmpty(result?.Course_Detail?.SubjectDetail?.Name) ? string.Empty : result?.Course_Detail?.SubjectDetail?.Name.ToUpper()))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_DATEFROM, (result.Course_Detail.dtm_time_from.HasValue ? DateUtil.DateToStringCertificate(result.Course_Detail.dtm_time_from, "dd MMMM yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_DATETO, (result.Course_Detail.dtm_time_to.HasValue ? DateUtil.DateToStringCertificate(result.Course_Detail.dtm_time_to, "dd MMMM yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_DATE_OF_BIRTH, trainee.dtm_Birthdate.HasValue ? DateUtil.DateToString(trainee.dtm_Birthdate, "dd/MM/yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_PLACE_OF_BIRTH, !string.IsNullOrEmpty(trainee.str_Place_Of_Birth) ? trainee.str_Place_Of_Birth.ToUpper() : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_FROM, program.StartDate.HasValue ? DateUtil.DateToStringCertificate(program.StartDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_TO, program.EndDate.HasValue ? DateUtil.DateToStringCertificate(program.EndDate, "dd MMMM yyyy") : string.Empty)
                   ;
            return body;
        }

        protected string BodyGroupCertificate(Group_Certificate_Schedule groupCertificateSchedule, string SRNO)
        {
            var body = groupCertificateSchedule.Group_Certificate.CAT_CERTIFICATE.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(groupCertificateSchedule.Group_Certificate.Name) ? string.Empty : groupCertificateSchedule.Group_Certificate.Name))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(groupCertificateSchedule.Trainee.LastName) ? string.Empty : ReturnDisplayLanguage(groupCertificateSchedule.Trainee.FirstName, groupCertificateSchedule.Trainee.LastName)))
                   .Replace(UtilConstants.CERTIFICATE_DATE_COMPLELTED, (groupCertificateSchedule?.Date_Of_Issue != null ? string.Format(GetByKey("CERTIFICATE_DATE_FORMAT"), groupCertificateSchedule.Date_Of_Issue) : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(groupCertificateSchedule.Trainee.JobTitle.Name) ? string.Empty : groupCertificateSchedule.Trainee.JobTitle.Name))
                   .Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(SRNO) ? string.Empty : SRNO.Replace("||", "")))
                   ;
            return body;
        }

        protected string ReplaceCertificateNo(string content, string cerno, int type)
        {
            if (type == 1)
            {
                content = content?.Replace(UtilConstants.CERTIFICATE_ATO, cerno);
            }
            else
            {
                content = content?.Replace(UtilConstants.CERTIFICATE_SR_NO, cerno);
            }

            return content;
        }
        protected MemoryStream LoadImage(string contentBody)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)

            var regex = new[] { "&quot;" };
            var splitContent = contentBody.Split(regex, StringSplitOptions.None);

            var content = splitContent[1];
            var replaceContent = content.Replace("data:image/jpeg;base64,", string.Empty);

            byte[] bytes = Convert.FromBase64String(replaceContent);


            var image = new MemoryStream(bytes, false);

            //using (MemoryStream ms = new MemoryStream(bytes))
            //{
            //    image = Image.FromStream(ms);
            //}
            return image;
        }

        protected bool CreateFolderIfExists(string courseCodeFolder, string parentFolder)
        {
            var path = Server.MapPath(parentFolder + courseCodeFolder);
            bool isExists = System.IO.Directory.Exists(path);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        protected bool CheckFileIfExists(string path)
        {
            var isExists = System.IO.File.Exists(path);
            return isExists;
        }

        protected string GetPointRemark(UtilConstants.DetailResult type, int? traineeId, int? courseDetailId)
        {
            var result = "";
            try
            {
                
                switch (type)
                {
                    case (int)UtilConstants.DetailResult.Score:
                        var data = CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).FirstOrDefault();
                        result = (data?.point != null && data?.point != -1) ? data.point?.ToString() : result;
                        break;
                    case UtilConstants.DetailResult.Remark:
                        var datar =
                   CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId)
                       .OrderByDescending(a => a.Id)
                       .FirstOrDefault();
                        result = datar?.Remark ?? result;
                        break;


                }
                return result;
            }
            catch (Exception ex)
            {

                return result;
            }
           

            
           
        }
        protected string GetResultGrade(int? subjectDetailId, int? traineeId, int? courseDetailsId)
        {

            float _return = -1;
            var data = CourseService.GetCourseResultSummary(traineeId, courseDetailsId);
           

            var grade = "Fail";
            var coursedetail = CourseDetailService.GetById(courseDetailsId);
            if (coursedetail.SubjectDetail.IsAverageCalculate == true)
            {
                _return = (data?.point != null) ? (float)data.point : _return;
                var subjectDetails = CourseService.GetScores(a => a.subject_id == subjectDetailId).OrderByDescending(a => a.point_from);
                foreach (var item in subjectDetails)
                {
                    if (_return >= item.point_from)
                    {
                        grade = item.grade;
                        break;
                    }
                }
            }
            else
            {
                grade = data.Result == "F" ? "Fail" : "Pass";

            }
          
            return grade;
        }

        protected string ReturnResult(IEnumerable<Subject_Score> subjectScores, double? point, string result)
        {
            var _return = UtilConstants.Grade.Fail.ToString();
            if (point != -1)
            {
                foreach (var item in subjectScores.Where(item => point >= item.point_from))
                {
                    _return = item.grade;
                    break;
                }
            }
            else
            {
                _return = result;
            }


            return _return;
        }
        protected bool UpdateStatus(Course course)
        {
            if (course == null) return false;           
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("LMSStatus", (int)UtilConstants.ApiStatus.Modify);
            if (CMSUtils.UpdateDataSQLNoLog("Id", course.Id + "", "Course", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
            {

            }
            var courseDetails = course.Course_Detail;
            if (courseDetails.Count() == 0) return false;
            foreach (var item in courseDetails)
            {
                Dictionary<string, object> dic_courseDetail = new Dictionary<string, object>();
                dic_courseDetail.Add("LmsStatus", (int)UtilConstants.ApiStatus.Modify);
                if (CMSUtils.UpdateDataSQLNoLog("Id", item.Id + "", "Course_Detail", dic_courseDetail.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_courseDetail.Values.ToArray())) > 0)
                {

                }
            }
            return true;
        }

        protected void UpdateMember(Course course,int UserID)
        {

            var courseDetailIds = course.Course_Detail.Select(a => a.Id);
            var tmsMember = CourseService.Lam_GetTraineemember_New(a => courseDetailIds.Contains((int)a.Course_Details_Id) && a.IsDelete != true);
            if (tmsMember.Any())
            {
                var tmsMemberIds = tmsMember.Select(a => a.Member_Id).Distinct();
                var list_final = course.Course_Result_Final;
                foreach (var courseDetailId in courseDetailIds)
                {
                    var tmsMemberAvailable = tmsMember.Where(a => a.Course_Details_Id == courseDetailId);
                    if (!tmsMemberAvailable.Any())
                    {
                       
                        foreach (var memberId in tmsMemberIds)
                        {
                            
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("Member_Id", memberId);
                            dic.Add("Course_Details_Id", courseDetailId);
                            dic.Add("IsDelete", 0);
                            dic.Add("IsActive", 1);
                            dic.Add("AssignBy", UserID + "");
                            dic.Add("Status", 0); // gửi request phê duyệt
                            dic.Add("LmsStatus", 1);// approval
                            if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {
                               
                            }
                        }
                    }
                    else if(tmsMemberAvailable.Any())
                    {
                        if(course.TMS_APPROVES.Any(a=> a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve))
                        {
                            foreach (var memberId in tmsMemberAvailable.Where(a => a.Status != 1))
                            {
                                Dictionary<string, object> dic = new Dictionary<string, object>();

                                dic.Add("LmsStatus", 1);// đã approval
                                if (CMSUtils.UpdateDataSQLNoLog("Id", memberId.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                {

                                }
                            }
                        }
                       
                    }
                }
                foreach (var item in tmsMemberIds)
                {
                    //RegistTraineeToCourse 
                    var entity = list_final.Where(a => a.traineeid == item && a.courseid == course.Id);
                    if (!entity.Any())
                    {
                        Dictionary<string, object> dic_final = new Dictionary<string, object>();
                        dic_final.Add("traineeid", item);
                        dic_final.Add("courseid", course.Id);
                        dic_final.Add("createby", UserID + "");
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
        }
        protected void UpdateMember_New(Course course)
        {

            var courseDetailIds = course.Course_Detail.Where(a=>a.IsDeleted != true).Select(a => a.Id);
            var tmsMember = CourseService.Lam_GetTraineemember_New(a => courseDetailIds.Contains((int)a.Course_Details_Id) && a.Status != 1); // status = 1 là chưa gửi request approve
            var list_not_remove_final = new List<int?>();
            if (tmsMember.Any())
            {
                var tmsMemberIds = tmsMember.Select(a => a.Member_Id).Distinct();
                var list_final = course.Course_Result_Final;
                foreach (var courseDetailId in courseDetailIds)
                {
                    var tmsMemberAvailable = tmsMember.Where(a => a.Course_Details_Id == courseDetailId);
                    if (!tmsMemberAvailable.Any())
                    {

                        foreach (var memberId in tmsMemberIds)
                        {

                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("Member_Id", memberId);
                            dic.Add("Course_Details_Id", courseDetailId);
                            dic.Add("IsDelete", 0);
                            dic.Add("IsActive", 1);
                            dic.Add("AssignBy", CurrentUser.USER_ID + "");
                            dic.Add("Status", 0); // gửi request phê duyệt
                            dic.Add("LmsStatus", 1);// approval
                            if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                            list_not_remove_final.Add(memberId);
                        }
                    }
                    else
                    {
                        foreach (var memberId in tmsMemberAvailable.Where(a=>a.IsDelete != true))
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            if (memberId.DeleteApprove == 1)
                            {
                                dic.Add("IsDelete", 1);
                                dic.Add("IsActive", 0);
                            }
                            dic.Add("Status", 0); // 
                            dic.Add("LmsStatus", 1);// đã approval
                            if (CMSUtils.UpdateDataSQLNoLog("Id", memberId.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                            if (memberId.DeleteApprove == 1)
                            {
                                var check_remove = list_not_remove_final.Contains(memberId.Member_Id);
                                if (check_remove != true)
                                {
                                    var final = list_final.FirstOrDefault(a => a.traineeid == memberId.Id && a.courseid == course.Id);
                                    if (final != null)
                                    {
                                        Dictionary<string, object> dic_final = new Dictionary<string, object>();
                                        dic_final.Add("IsDeleted", 1);
                                        dic_final.Add("DeletedBy", CurrentUser.USER_ID + "");
                                        dic_final.Add("DeletedDate", DateTime.Now);
                                        dic_final.Add("LmsStatus", 99);
                                        dic_final.Add("MemberStatus", (int)UtilConstants.CourseResultFinalStatus.Removed);
                                        if (CMSUtils.UpdateDataSQLNoLog("id", final.id + "", "Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                                        {

                                        }
                                    }
                                    TMS_Course_Member_Remark entity_remark = CourseMemberService.GetRemark(a => a.TraineeId == memberId.Id && a.CourseId == course.Id).FirstOrDefault();
                                    if (entity_remark != null)
                                    {
                                        CMSUtils.DeleteDataSQLNoLog("Id", entity_remark.Id + "", "TMS_Course_Member_Remark");
                                    }
                                }
                            }
                           

                        }
                    }
                }
            }
        }

        #region [------------------PROCESS STEP--------------------]
        protected bool ProcessStep(int presentStep)
        {
            return ApproveService.ProcessStep(presentStep);
        }
        protected bool ProcessStepRequirement(int lastStep, int stepNotEdit)
        {
            return ApproveService.ProcessStepRequirement(lastStep, stepNotEdit);
        }
        protected void Modify_TMS(bool? isApprove, Course course, int type, int status, UtilConstants.ActionType actionType, string note = "", int? courseDetailId = -1,string remarkassign = null)
        {
            courseDetailId = courseDetailId ?? -1;
            var approveType = UtilConstants.ApproveType.Course;
            var eStatus = UtilConstants.EStatus.Pending;
            var notiTemplate = string.Empty;
            var notiTemplateVn = string.Empty;
            var notiContent = string.Empty;
            var notiContentVn = string.Empty;
            #region [Request]
            if (actionType == UtilConstants.ActionType.Request)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Request_Course;
                        notiContentVn = UtilConstants.NotificationContent.Request_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Request_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Request_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_CourseResult_VN;
                        break;
                }
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    eStatus = UtilConstants.EStatus.Approve;
                }

                if (status == (int)UtilConstants.EStatus.CancelRequest)
                {
                    eStatus = UtilConstants.EStatus.CancelRequest;
                    notiTemplate = UtilConstants.NotificationTemplate.CancelRequest;
                    notiTemplateVn = UtilConstants.NotificationTemplate.CancelRequestVN;
                    notiContent = UtilConstants.NotificationContent.CancelRequest;
                    notiContentVn = UtilConstants.NotificationContent.CancelRequestVN;
                }
            }

            #endregion
            #region [Approve]

            if (actionType == UtilConstants.ActionType.Approve)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_Course;
                        notiContentVn = UtilConstants.NotificationContent.Approval_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Approval_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_CourseResult_VN;
                        break;
                }

                #region [status]
                switch (status)
                {
                    case (int)UtilConstants.EStatus.Pending:
                        eStatus = UtilConstants.EStatus.Pending;
                        break;
                    case (int)UtilConstants.EStatus.Approve:
                        eStatus = UtilConstants.EStatus.Approve;
                        break;
                    case (int)UtilConstants.EStatus.Reject:
                        eStatus = UtilConstants.EStatus.Reject;
                        break;
                    case (int)UtilConstants.EStatus.Block:
                        eStatus = UtilConstants.EStatus.Block;
                        break;
                }
                #endregion

                #region [Course]
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockCourse;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockCourseVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockCourse;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockCourseVn;
                }
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_Course;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_Course_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_Course;
                    notiContentVn = UtilConstants.NotificationContent.Reject_Course_VN;
                }
                #endregion
                #region [Assign]
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockAssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockAssignTraineeVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockAssign;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockAssignVn;
                }
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_AssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_AssignTrainee_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_AssignTrainee;
                    notiContentVn = UtilConstants.NotificationContent.Reject_AssignTrainee_VN;
                }
                #endregion
                #region [Subject]
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockSubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockSubjectResultVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockSubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockSubjectResultVn;
                }
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_SubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_SubjectResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_SubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_SubjectResult_VN;
                }
                #endregion
                #region [Final]
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockFinal;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockFinalVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockFinal;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockFinalVn;
                }
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_CourseResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_CourseResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_CourseResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_CourseResult_VN;
                }
                #endregion


            }
            #endregion

            var approve = ApproveService.Modify(isApprove,course, approveType, eStatus, actionType, courseDetailId, note, remarkassign);
            if (approve == null) throw new Exception(Messege.WARNING_SENT_REQUEST_ERROR);
            var list_temp = new List<int>(new int[count]);
           

            //var approver = ApproveService.GetApprover(approveType);
            //var approverId = approver?.UserId ?? 7; // admin = 7
           
            if(actionType == UtilConstants.ActionType.Request)
            {
                var hod = UserContext.Get(a => a.IsDeleted != true && a.ISACTIVE == 1 && a.UserRoles.Any(x => x.ROLE.NAME == "HOD" && x.ROLE.IsActive == true), list_temp).ToList();
                if(hod.Count() > 0)
                {
                    foreach (var item in hod)
                    {
                        var toUser = item.ID;
                        SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)approveType, approve.id, toUser, DateTime.Now, notiTemplate,
                       (
                       approveType == UtilConstants.ApproveType.SubjectResult
                       ? string.Format(notiContent, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
                       : string.Format(notiContent, approve.Course.Code + " - " + approve.Course.Name, note)),
                       notiTemplateVn,
                       (
                       approveType == UtilConstants.ApproveType.SubjectResult
                       ? string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
                       : string.Format(notiContentVn, approve.Course.Code + " - " + approve.Course.Name, note)));
                    }
                  
                }
                else
                {
                    var toUser = 7;
                    SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)approveType, approve.id, toUser, DateTime.Now, notiTemplate,
                   (
                   approveType == UtilConstants.ApproveType.SubjectResult
                   ? string.Format(notiContent, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
                   : string.Format(notiContent, approve.Course.Code + " - " + approve.Course.Name, note)),
                   notiTemplateVn,
                   (
                   approveType == UtilConstants.ApproveType.SubjectResult
                   ? string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
                   : string.Format(notiContentVn, approve.Course.Code + " - " + approve.Course.Name, note)));
                }
                
               
            }
            else
            {
                var toUser = approve.int_Requested_by;
                SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)approveType, approve.id, toUser, DateTime.Now, notiTemplate,
               (
               approveType == UtilConstants.ApproveType.SubjectResult
               ? string.Format(notiContent, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
               : string.Format(notiContent, approve.Course.Code + " - " + approve.Course.Name, note)),
               notiTemplateVn,
               (
               approveType == UtilConstants.ApproveType.SubjectResult
               ? string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Name, approve.Course.Code + " - " + approve.Course.Name, "")
               : string.Format(notiContentVn, approve.Course.Code + " - " + approve.Course.Name, note)));
            }
            

            #region [Send email GV]

            //var checkcsendEmail = GetByKey("SENDEMAILGV");
            //if (checkcsendEmail.Equals("1") && actionType == UtilConstants.ActionType.Request)
            //{
            //    var currentUser = CurrentUser;
            //    var tmsApprove = course.TMS_APPROVES?.FirstOrDefault(a => a.int_Type == type);

            //    Sent_Email_TMS(null, null, approver?.USER, course, null, currentUser.USER_ID, type, false, tmsApprove, (int)UtilConstants.ActionTypeSentmail.SendMailApprove);
            //}
            #endregion



            /// var abc = Modify_TMS_SendNotificationAndMail(course, approveType, actionType, approve, notiTemplate, notiTemplateVn, notiContent, notiContentVn, note, type);

        }
        private async Task<int> Modify_TMS_SendNotificationAndMail(Course course, UtilConstants.ApproveType approveType, UtilConstants.ActionType actionType, TMS_APPROVES approve, string notiTemplate, string notiTemplateVn, string notiContent, string notiContentVn, string note, int type)
        {
            await Task.Run(() =>
            {
               
            });
            return 0;
        }
        #endregion

        #region [------------------SENT EMAIL------------------]
        protected void Sent_Email_TMS(Trainee instructor, Trainee trainee, USER user, Course course, Course_Detail_Instructor details, int? int_Requested_by, int? actionType = -1, bool? LMSAssign = false, TMS_APPROVES tmsApprove = null, int? sendApproveMail = -1)
        {//, UtilConstants.EStatus status
            #region [CodeNEw]
            var mail_title = "";
            var body_Ins = string.Empty;
            var mail_receiver = string.Empty;
            var TypeSentEmail = -1;
         
            #region [------Approve---------]
            #region [ApproveCourse]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor--------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedGV, null, instructor, course,null,1);
                            mail_receiver = instructor.str_Email;
                            mail_title = "Thư mời dạy học/ Assignment letter";
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedGV;
                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant, null, instructor, course);
                            //mail_receiver = instructor.str_Email;
                            //TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            //if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            //{
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal, null, instructor, course);
                            //    mail_receiver = instructor.str_Email;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal;


                            //}
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = UserContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse;

                }
            }
            #endregion

            #region [Assign Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.AssignTrainee)
            {

                if (trainee != null && course != null)
                {
                    if (LMSAssign == false)
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTrainee, null, trainee, course, null, 2);
                        mail_receiver = trainee.str_Email;
                        mail_title = "Thư mời học/ Enrollment letter";
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee;

                    }
                    else
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS, null, trainee, course, null, 2);
                        mail_receiver = trainee.str_Email;
                        mail_title = "Thư mời học/ Enrollment letter";
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS;

                    }

                }
            }
            #endregion

            #region [Approved Final Program]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor-------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalGV, null, instructor, course);
                            mail_title = "";
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV;

                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalMantor, null, instructor, course);
                            //mail_receiver = instructor.str_Email;
                            //TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalMantor;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            //if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            //{
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah, null, instructor, course);
                            //    mail_receiver = instructor.str_Email;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah;

                            //}
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (trainee != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalCourse, null, trainee, course);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse;


                }
            }

            #endregion
            #endregion

            #region [Create Password]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser)
            {
                if (user != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordUser, user, null, null);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordUser;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion

            #region [Create New Instructor_Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreateInstructor_Trainee)
            {
                if (instructor != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailCreateInstructor, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailCreateInstructor;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailCreateTrainee, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailCreateTrainee;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion

            #region [------Reject---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.Reject)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectGV;


                            break;
                        case (int)UtilConstants.TypeInstructor.Mentor:
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectMantor, null, instructor, course);
                            //mail_receiver = instructor.str_Email;
                            //TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectMantor;


                            break;
                        case (int)UtilConstants.TypeInstructor.Hannah:
                            //if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            //{
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah, null, instructor, course);
                            //    mail_receiver = instructor.str_Email;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah;
                            //}
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah_User;
                            //}
                            break;
                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = UserContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectCourse;

                }
            }
            #endregion

            #region [------Cancel Request---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CancelRequest)
            {
                if (user != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailCancelRequest, user, null, course);
                    mail_receiver = user.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailCancelRequest;

                }
            }
            #endregion
            #region [Send Email Approve]
            if (sendApproveMail == (int)UtilConstants.ActionTypeSentmail.SendMailApprove)
            {
                if (user != null && course != null)
                {
                    // type lấy khi nào config bật , ko thì lấy UtilConstants.ActionTypeSentmail
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailApproveToMail, user, null, course, tmsApprove, actionType);
                    mail_receiver = user.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailApproveToMail;
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(mail_receiver) && TypeSentEmail != -1 && !string.IsNullOrEmpty(body_Ins))
            {
                InsertSentMail(mail_receiver, TypeSentEmail, body_Ins, course?.Id, mail_title);
            }
            #endregion
            #region [Code Old]
            ////var currentModel = GetUser();
            //var cAT_MAIL_INS = new CAT_MAIL();
            //var body_Ins = string.Empty;
            //var checkHANNAH = CheckSiteConfig(UtilConstants.KEY_HANNAH);

            //var UserHannal = checkHANNAH ? UserContext.Get(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => UtilConstants.ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME)) : null;

            //#region [------Approve---------]
            //if (status == UtilConstants.EStatus.Approve)
            //{
            //    #region [ApproveCourse]
            //    if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedProgram)
            //    {
            //        if (instructor != null && course != null)
            //        {
            //            switch (details.Type)
            //            {
            //                case (int)UtilConstants.TypeInstructor.Instructor:

            //                    #region [--------------------------Instructor--------------------------------]
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedGV).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                    InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApprovedGV, body_Ins, course.Id);
            //                    break;
            //                #endregion

            //                case (int)UtilConstants.TypeInstructor.Mentor:

            //                    #region [--------------------------Mentor------------------------------------]
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                    InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant, body_Ins, course.Id);
            //                    break;
            //                #endregion

            //                case (int)UtilConstants.TypeInstructor.Hannah:

            //                    #region [--------------------------Hannah------------------------------------]
            //                    if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
            //                    {
            //                        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal).FirstOrDefault();
            //                        body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                        InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal, body_Ins, course.Id);
            //                    }
            //                    else
            //                    {
            //                        var userId = UserContext.GetById((int)details.Instructor_Id);
            //                        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal_User).FirstOrDefault();
            //                        body_Ins = BodySendMail(cAT_MAIL_INS, userId, null, course);
            //                        InsertSentMail(userId.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal_User, body_Ins, course.Id);
            //                    }
            //                    break;
            //                    #endregion

            //            }
            //        }

            //        if (course != null && int_Requested_by.HasValue)
            //        {
            //            var user_create = UserContext.GetById((int)int_Requested_by);
            //            cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse).FirstOrDefault();
            //            body_Ins = BodySendMail(cAT_MAIL_INS, user_create, null, course);
            //            InsertSentMail(user_create.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse, body_Ins, course.Id);
            //        }
            //    }
            //    #endregion

            //    #region [Assign Trainee]
            //    if (actionType == (int)UtilConstants.ActionTypeSentmail.AssignTrainee)
            //    {

            //        if (trainee != null && course != null)
            //        {
            //            if (LMSAssign == false)
            //            {
            //                cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee).FirstOrDefault();
            //                body_Ins = BodySendMail(cAT_MAIL_INS, null, trainee, course);
            //                InsertSentMail(trainee.str_Email, (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee, body_Ins, course.Id);
            //            }
            //            else
            //            {
            //                cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS).FirstOrDefault();
            //                body_Ins = BodySendMail(cAT_MAIL_INS, null, trainee, course);
            //                InsertSentMail(trainee.str_Email, (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS, body_Ins, course.Id);
            //            }

            //        }
            //    }
            //    #endregion

            //    #region [Approved Final Program]
            //    if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram)
            //    {
            //        if (instructor != null && course != null)
            //        {
            //            switch (details.Type)
            //            {
            //                case (int)UtilConstants.TypeInstructor.Instructor:

            //                    #region [--------------------------Instructor-------------------------------]
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                    InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV, body_Ins, course.Id);
            //                    break;
            //                #endregion

            //                case (int)UtilConstants.TypeInstructor.Mentor:

            //                    #region [--------------------------Mentor------------------------------------]
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApproveFinalMantor).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                    InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApproveFinalMantor, body_Ins, course.Id);
            //                    break;
            //                #endregion

            //                case (int)UtilConstants.TypeInstructor.Hannah:

            //                    #region [--------------------------Hannah------------------------------------]
            //                    if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
            //                    {
            //                        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah).FirstOrDefault();
            //                        body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                        InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah, body_Ins, course.Id);
            //                    }
            //                    else
            //                    {
            //                        var userId = UserContext.GetById((int)details.Instructor_Id);
            //                        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User).FirstOrDefault();
            //                        body_Ins = BodySendMail(cAT_MAIL_INS, userId, null, course);
            //                        InsertSentMail(userId.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User, body_Ins, course.Id);
            //                    }
            //                    break;
            //                    #endregion

            //            }
            //        }

            //        if (trainee != null && course != null)
            //        {
            //            cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse).FirstOrDefault();
            //            body_Ins = BodySendMail(cAT_MAIL_INS, null, trainee, course);
            //            InsertSentMail(trainee.str_Email, (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse, body_Ins, course.Id);
            //        }
            //    }

            //    #endregion

            //    #region [Create Password]
            //    if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser)
            //    {
            //        if (user != null)
            //        {
            //            cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailPasswordUser).FirstOrDefault();
            //            body_Ins = BodySendMail(cAT_MAIL_INS, user, null, null);
            //            InsertSentMail(user.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailPasswordUser, body_Ins, -1);
            //        }
            //    }
            //    if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            //    {
            //        if (instructor != null)
            //        {
            //            cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp).FirstOrDefault();
            //            body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, null);
            //            InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp, body_Ins, -1);
            //        }
            //        if (trainee != null)
            //        {
            //            cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp).FirstOrDefault();
            //            body_Ins = BodySendMail(cAT_MAIL_INS, null, trainee, null);
            //            InsertSentMail(trainee.str_Email, (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp, body_Ins, -1);
            //        }
            //    }
            //    #endregion

            //}
            //#endregion

            //#region [------Reject---------]
            //if (status == UtilConstants.EStatus.Reject)
            //{
            //    if (instructor != null && course != null)
            //    {
            //        switch (details.Type)
            //        {
            //            case (int)UtilConstants.TypeInstructor.Instructor:
            //                cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectGV).FirstOrDefault();
            //                body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailRejectGV, body_Ins, course.Id);
            //                break;
            //            case (int)UtilConstants.TypeInstructor.Mentor:
            //                cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectMantor).FirstOrDefault();
            //                body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailRejectMantor, body_Ins, course.Id);
            //                break;
            //            case (int)UtilConstants.TypeInstructor.Hannah:
            //                if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
            //                {
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectHannah).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, null, instructor, course);
            //                    InsertSentMail(instructor.str_Email, (int)UtilConstants.TypeSentEmail.SentMailRejectHannah, body_Ins, course.Id);
            //                }
            //                else
            //                {
            //                    var userId = UserContext.GetById((int)details.Instructor_Id);
            //                    cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectHannah_User).FirstOrDefault();
            //                    body_Ins = BodySendMail(cAT_MAIL_INS, userId, null, course);
            //                    InsertSentMail(userId.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailRejectHannah_User, body_Ins, course.Id);
            //                }
            //                break;
            //        }
            //    }

            //    if (course != null && int_Requested_by.HasValue)
            //    {
            //        var user_create = UserContext.GetById((int)int_Requested_by);
            //        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailRejectCourse).FirstOrDefault();
            //        body_Ins = BodySendMail(cAT_MAIL_INS, user_create, null, course);
            //        InsertSentMail(user_create.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailRejectCourse, body_Ins, course.Id);
            //    }
            //}
            //#endregion

            //#region [------Cancel Request---------]
            //if (status == UtilConstants.EStatus.CancelRequest)
            //{
            //    if (user != null && course != null)
            //    {
            //        cAT_MAIL_INS = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailCancelRequest)
            //           .FirstOrDefault();
            //        body_Ins = BodySendMail(cAT_MAIL_INS, user, null, course);
            //        InsertSentMail(user.EMAIL, (int)UtilConstants.TypeSentEmail.SentMailCancelRequest, body_Ins, course.Id);
            //    }

            //}
            //#endregion
            #endregion
        }

        protected void Sent_Email_TMS_Custom(int? cat_mail_ID ,string body_Ins, Trainee instructor, Trainee trainee, USER user, Course course, Course_Detail_Instructor details, int? int_Requested_by, int? actionType = -1, bool? LMSAssign = false, TMS_APPROVES tmsApprove = null, int? sendApproveMail = -1)
        {//, UtilConstants.EStatus status
            #region [CodeNEw]
            var mail_title = "";
          
            var mail_receiver = string.Empty;
            var TypeSentEmail = -1;

            #region [------Approve---------]
            #region [ApproveCourse]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor--------------------------------]
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedGV, null, instructor, course, null, 1);
                            mail_receiver = instructor.str_Email;
                            mail_title = "Thư mời dạy học/ Assignment letter";
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedGV;
                            break;
                            #endregion

                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    //var user_create = UserContext.GetById((int)int_Requested_by);
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedCourse, user_create, null, course);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse;

                }
            }
            #endregion

            #region [Assign Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.AssignTrainee)
            {

                if (trainee != null && course != null)
                {
                    if (LMSAssign == false)
                    {
                        //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTrainee, null, trainee, course, null, 2);
                        mail_receiver = trainee.str_Email;
                        mail_title = "Thư mời học/ Enrollment letter";
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee;

                    }
                    else
                    {
                       //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS, null, trainee, course, null, 2);
                        mail_receiver = trainee.str_Email;
                        mail_title = "Thư mời học/ Enrollment letter";
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS;

                    }

                }
            }
            #endregion

            #region [Approved Final Program]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor-------------------------------]
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalGV, null, instructor, course);
                            mail_title = "";
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV;

                            break;
                        #endregion
                    }
                }

                if (trainee != null && course != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalCourse, null, trainee, course);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse;


                }
            }

            #endregion
            #endregion

            #region [Create Password]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser)
            {
                if (user != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordUser, user, null, null);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordUser;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion

            #region [Create New Instructor_Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreateInstructor_Trainee)
            {
                if (instructor != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailCreateInstructor, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailCreateInstructor;


                }
                if (trainee != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailCreateTrainee, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailCreateTrainee;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion

            #region [------Reject---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.Reject)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:
                            //body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectGV;
                            break;                      
                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = UserContext.GetById((int)int_Requested_by);
                   // body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectCourse;

                }
            }
            #endregion

            #region [------Cancel Request---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CancelRequest)
            {
                if (user != null && course != null)
                {
                   // body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailCancelRequest, user, null, course);
                    mail_receiver = user.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailCancelRequest;

                }
            }
            #endregion
            #region [Send Email Approve]
            if (sendApproveMail == (int)UtilConstants.ActionTypeSentmail.SendMailApprove)
            {
                if (user != null && course != null)
                {
                    // type lấy khi nào config bật , ko thì lấy UtilConstants.ActionTypeSentmail
                   // body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailApproveToMail, user, null, course, tmsApprove, actionType);
                    mail_receiver = user.EMAIL;
                    mail_title = "";
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailApproveToMail;
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(mail_receiver) && TypeSentEmail != -1 && !string.IsNullOrEmpty(body_Ins))
            {
                InsertSentMail_Custom(cat_mail_ID,mail_receiver, TypeSentEmail, body_Ins, course?.Id, mail_title);
            }
            #endregion
        }


        #endregion

        protected string OnOffTextOption(string controller, string view, int id, string reSourse, string fontAwesome, string href = "", string onClick = "")
        {
            var html = string.Empty;
            var onorOff = int.Parse(GetByKey("DisplayTextOption"));
            html += ((HttpContext.User.IsInRole("/" + controller + "/" + view + ""))
                ? "<a title='" + reSourse + "' " + onClick + "'" + href + "')'><i class='" + fontAwesome + "' aria-hidden='true' style='cursor: pointer;font-size: 16px;'>&nbsp;" + (onorOff == (int)UtilConstants.BoolEnum.Yes ? reSourse : string.Empty) + "</i></a>&nbsp;"
                : "");
            return html;
        }


        #region [---------CALL LMS------------]
        protected bool CallServices(string type)
        {
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
            if (response.StatusCode != HttpStatusCode.OK)
            {

                return false;
            }

            var responseContent = response.Content;
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                return true;
            }
            return false;
        }
        protected APIResponse CallServicesReturnJson(string type)
        {
            var server = GetByKey("API_LMS_SERVER");
            var token = GetByKey("API_LMS_TOKEN");
            var function = GetByKey("FUNCTION");
            var moodlewsrestformat = GetByKey("moodlewsrestformat");
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);

            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("type", type);
            var response = restClient.Execute(request);
            var result = new APIResponse()
            {
                StatusResponse = string.Empty,
                Message = string.Empty

            };
            var responseContent = response.Content;
            result.StatusResponse = (int)response.StatusCode + " - " + response.StatusCode;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                result.Result = false;

            }
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                result.Result = true;

            }
            else
            {
                result.Result = false;
                var json = JsonConvert.DeserializeObject<APIResponseOJB>(responseContent);
                if (json != null )
                {
                    
                    result.Message = json.warnings?.FirstOrDefault()?.message;
                }else
                {
                    result.Message = "Unsuccess!!!";
                }
               
                // result.StatusResponse = response.ResponseStatus.ToString();

            }
            return result;
        }
        #endregion

        public string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            //culture = "en";
            //HttpCookie cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            //if (cultureCookie != null)
            //{
            //    culture = cultureCookie.Value;
            //}
            //switch (culture)
            //{
            //    case "vi":
            //        fullName = firstName + " " + lastName;
            //        break;
            //    default:
            //        fullName = lastName + " " + firstName;
            //        break;
            //}
            if(string.IsNullOrEmpty(lastName) || lastName.Equals(firstName))
            {
                fullName = firstName;
            }
            else
            {
                fullName = lastName + " " + firstName;
            }
            
            return fullName;
        }
        protected string ReturnDisplayLanguageCustom(string firstName, string lastName, string culture = null, double duration = 0)
        {
            string fullName;
            //culture = "vi";
            //HttpCookie cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            //if (cultureCookie != null)
            //{
            //    //culture = GetByKey("DisplayLanguage");
            //    culture = cultureCookie.Value;
            //}
            //switch (culture)
            //{
            //    case "vi":
            //        fullName = firstName + " " + lastName + " - Duration (Hours) : " + duration;
            //        break;
            //    default:
            //        fullName = lastName + " " + firstName + " - Duration (Hours) : " + duration;
            //        break;
            //}
            if (string.IsNullOrEmpty(lastName) || lastName.Equals(firstName))
            {
                fullName = firstName + " - Duration (Hours) : " + duration;
            }
            else
            {
                fullName = lastName + " " + firstName + " - Duration (Hours) : " + duration;
            }
            return fullName;
        }
        protected string GetCulture()
        {
            var culture = "vi";
            var cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            if (cultureCookie != null)
            {
                culture = cultureCookie.Value;
            }

            return culture;
        }


        protected void LogEvent(UtilConstants.LogType logType,
           UtilConstants.LogEvent logEvent, string logSourse, object messageEx)
        {
            var log = new MessagesModel
            {
                Message = messageEx,
                MessageDate = DateTime.Now,
                UserId = CurrentUser?.USER_ID ?? 7,
            };
            ConfigService.LogEvent(logType, logEvent, logSourse, log);
        }

        #region[LMS Webservice]
        protected JObject CallServicesReturnJson(string function, RestRequest RequestWsLms = null)
        {
            var server = GetByKey("SERVER");
            var token = GetByKey("TOKEN");
            var moodlewsrestformat = GetByKey("moodlewsrestformat");
            var restClient = new RestClient(server);
            RequestWsLms.AddParameter("wstoken", token);
            RequestWsLms.AddParameter("wsfunction", function);
            RequestWsLms.AddParameter("moodlewsrestformat", moodlewsrestformat);

            var response = restClient.Execute(RequestWsLms);
            JObject rss = JObject.Parse(response.Content);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            var responseContent = response.Content;
            if (!responseContent.Contains("moodle_exception"))
            {
                return rss;
            }
            return null;
        }
        #endregion

        #region [EncryptKeyEmail]
        private string EncryptKeyEmail(string input)
        {
            var _return = "";
            foreach (char c in input)
            {
                switch (c.ToString())
                {
                    case "q": _return += "="; break;
                    case "w": _return += ";"; break;
                    case "e": _return += "0"; break;
                    case "r": _return += "1"; break;
                    case "t": _return += "2"; break;
                    case "y": _return += "3"; break;
                    case "u": _return += "4"; break;
                    case "i": _return += "9"; break;
                    case "o": _return += "8"; break;
                    case "p": _return += "7"; break;
                    case "a": _return += "6"; break;
                    case "s": _return += "5"; break;
                    case "d": _return += "M"; break;
                    case "f": _return += "n"; break;
                    case "g": _return += "B"; break;
                    case "h": _return += "v"; break;
                    case "j": _return += "c"; break;
                    case "k": _return += "x"; break;
                    case "l": _return += "Z"; break;
                    case "z": _return += "l"; break;
                    case "x": _return += "k"; break;
                    case "c": _return += "j"; break;
                    case "v": _return += "h"; break;
                    case "b": _return += "g"; break;
                    case "n": _return += "F"; break;
                    case "m": _return += "d"; break;
                    case "1": _return += "s"; break;
                    case "2": _return += "a"; break;
                    case "3": _return += "q"; break;
                    case "4": _return += "W"; break;
                    case "5": _return += "e"; break;
                    case "6": _return += "r"; break;
                    case "7": _return += "T"; break;
                    case "8": _return += "y"; break;
                    case "9": _return += "u"; break;
                    case "0": _return += "i"; break;
                    case "=": _return += "O"; break;
                    case ";": _return += "P"; break;
                    default: _return += c.ToString(); break;
                }
            }
            return keyCodeStart + _return.Trim() + keyCodeEnd;
        }
        #endregion

        protected bool IsPasswordAllowed(string password)
        {

            return Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$");
        }
        protected bool IsNumberAllowed(string text)
        {

            return Regex.IsMatch(text, @"^[0-9]+$");
        }    
        protected bool PostToApi<T>(List<T> item, string domain, string action)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(domain + action)
            };
            var json = JsonConvert.SerializeObject(item);
            var response = client.PostAsync(action,
                    new StringContent(json,
                        Encoding.UTF8, "application/json")).Result;
            return response.IsSuccessStatusCode;
        }
        public List<int> String2Array(string source)
        {
            var LtsValue = new List<int>();
            if (string.IsNullOrEmpty(source)) return LtsValue;
            if (source.EndsWith(","))
                source = source.Substring(0, source.LastIndexOf(","));
            if (source.Contains(','))
            {
                LtsValue = source.Split(',').Select(o => Convert.ToInt32(o)).ToList();
            }
            else
                LtsValue.Add(Convert.ToInt32(source));
            return LtsValue;
        }
    }
}