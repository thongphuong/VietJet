using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;
using TMS.Core.ViewModels.AjaxModels.AjaxRoom;
using TMS.Core.ViewModels.Common;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Room;

    public class RoomController : BaseAdminController
    {

        #region MyRegion


        public RoomController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, ICourseService courseService, IDepartmentService departmentService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
        }

        #endregion
        //
        // GET: /Room/
        public ActionResult Index(int id = 0)
        {
            var room = new RoomModels();
            room.room_type = CourseService.GetRoomType().ToDictionary(a => a.Id, a => a.Name);
            return View(room);
        }

        [AllowAnonymous]
        public ActionResult Management(int id = 0)
        {
            var datestart = DateTime.Now;
            var dateend = DateTime.Now.AddMonths(1);

            var entity = CourseService.GetManagement_Room_Item().OrderByDescending(a => a.IdManagementRoom);
            var room = new RoomManagementModels
            {
                Period = GetDateRange(datestart, dateend),
                Background = ReturnBackground(entity),
                Room = CourseService.GetRoom().AsEnumerable().Select(a => new RoomManagementModels.RoomModel()
                {
                    Id = a.Room_Id,
                    Name = a.str_Name,
                    CourseDetail = a.Management_Room_Item.Where(b =>

                    ((b.dtm_time_from < datestart && datestart < b.dtm_time_to) ||
                    (b.dtm_time_from < dateend && dateend < b.dtm_time_to) ||
                    (datestart < b.dtm_time_from && b.dtm_time_to < dateend))

                    ).OrderBy(b => b.time_to).Select(b => new RoomManagementModels.RoomModel.CourseDetailModel()
                    {
                        Id = b.Id,
                        Name = b.IdManagementRoom == null ? b.Name : b.Management_Room.Course_Detail.SubjectDetail.Name,
                        StarCol = ReturnStarCol(b.dtm_time_from.Value.Date, b.dtm_time_to.Value.Date, datestart.Date, dateend.Date),
                        ManagementRoomItem = b,
                        Type = TypeBooking(b.Management_Room)
                    })
                })
            };

            return View(room);
        }
        private static int TypeBooking(Management_Room managementRoom)
        {
            var int_ = 0;
            if (managementRoom?.IdCourse != null)
            {
                int_ = 1;
            }
            else if (managementRoom?.IdMeeting != null)
            {
                int_ = 2;
            }
            return int_;
        }
        private static Dictionary<int, string> ReturnBackground(IQueryable<Management_Room_Item> managementRoomItem)
        {
            var rnd = new Random();
            var dic = new Dictionary<int, string>();
            if (!managementRoomItem.Any()) return dic;
            var temp = 0;
            var tempcolor = "";
            foreach (var item in managementRoomItem)
            {

                var randomColor = Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)).Name;
                if (temp != item.IdManagementRoom)
                {
                    dic.Add(item.Id, randomColor);
                    temp = item.IdManagementRoom ?? 0;
                    tempcolor = randomColor;
                }
                else
                {
                    dic.Add(item.Id, tempcolor);
                }
            }
            return dic;
        }
        private static Dictionary<DateTime, int> ReturnStarCol(DateTime courseStart, DateTime courseEnd, DateTime inputStart, DateTime inputend)
        {
            var dic = new Dictionary<DateTime, int>();
            /*TH1: ---|----******----|--- */
            if (inputStart <= courseStart && courseEnd <= inputend)
            {
                var day = (int)(courseEnd - courseStart).TotalDays;
                dic.Add(courseStart, day);
            }
            /*TH2: ***|******--------|--- */
            if ((courseStart < inputStart) && courseEnd < inputend)
            {
                var day = (int)(courseEnd - inputStart).TotalDays;
                dic.Add(inputStart, day);
            }
            /*TH3: ---|--------******|*** */
            if (inputStart < courseStart && (inputend < courseEnd))
            {
                var day = (int)(inputend - courseStart).TotalDays;
                dic.Add(courseStart, day);
            }
            /*TH4: ***|**************|*** */
            if ((courseStart < inputStart) && (inputend < courseEnd))
            {
                var day = (int)(inputend - inputStart).TotalDays;
                dic.Add(inputStart, day);
            }
            return dic;
        }
        private static List<DateTime> GetDateRange(DateTime StartingDate, DateTime EndingDate)
        {
            if (StartingDate > EndingDate)
            {
                return null;
            }
            List<DateTime> rv = new List<DateTime>();
            DateTime tmpDate = StartingDate;
            do
            {
                rv.Add(tmpDate);
                tmpDate = tmpDate.AddDays(1);
            } while (tmpDate <= EndingDate);
            return rv;
        }
        public ActionResult Modify(int? id)
        {
            var room = new RoomModels();
            room.room_type = CourseService.GetRoomType().ToDictionary(a => a.Id, a => a.Name);
            if (!id.HasValue)
            {

                return View(room);
            }

            var entity = CourseService.GetRoomById(id);
            room.Id = entity.Room_Id;
            room.Code = entity.str_code;
            room.Name = entity.str_Name;
            room.Capacity = (int)entity.int_Capacity;
            room.Equipment = entity.str_Equipment;
            room.Area = entity.int_Area;
            room.Location = entity.str_Location;
            room.Is_Meeting = entity.is_Meeting;
            ViewBag.roomId = entity.RoomTypeId;
            room.room_type = CourseService.GetRoomType().ToDictionary(a => a.Id, a => a.Name);
            return View(room);

        }

        [HttpPost]
        public ActionResult Modify(RoomModels model)
        {
            try
            {
                //var check = form["is_meeting"];
                //model.Is_Meeting = check == null ? false : true;
                if (model.strEquipment != null)
                {
                    model.Equipment = string.Join(", ", model.strEquipment);
                }

                if (!ModelState.IsValid)
                    return Json(new { result = false, message = MessageInvalidData(ModelState) });

                var md_checkbox = !string.IsNullOrEmpty(model.check_Meeting);
                // model.Is_Meeting = md_checkbox;
                var entity = CourseService.GetRoomById(model.Id);
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    return Json(new AjaxResponseViewModel { result = false, message = codeHasSpaceMessage }, JsonRequestBehavior.AllowGet);
                }
                if (entity == null)
                {
                    if (CourseService.GetRoom(a => a.str_code.ToLower() == model.Code.ToLower()).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }

                    CourseService.InsertRoom(model);
                }
                else
                {
                    if (CourseService.GetRoom(a => a.str_code.ToLower() == model.Code.ToLower() && a.Room_Id != model.Id).Any())
                    {
                        return Json(new { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }

                    CourseService.Update(model);
                }

                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Room/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].Trim().ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].Trim().ToLower();
                var noSeats = string.IsNullOrEmpty(Request.QueryString["NoSeats"]) ? -1 : Convert.ToInt32(Request.QueryString["NoSeats"].Trim());
                var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].Trim().ToLower();
                var RoomTypeId = string.IsNullOrEmpty(Request.QueryString["RoomTypeId"]) ? -1 : Convert.ToInt32(Request.QueryString["RoomTypeId"].Trim());

                var data = CourseService.GetRoom(a =>
                                                (string.IsNullOrEmpty(strCode) || a.str_code.Trim().ToLower().Contains(strCode)) &&
                                                (string.IsNullOrEmpty(strCode) || a.str_code.Trim().ToLower().Contains(strCode)) &&
                                                (noSeats == -1 || a.int_Capacity == noSeats) &&
                                                (RoomTypeId == -1 || a.RoomTypeId == RoomTypeId) &&
                                                (string.IsNullOrEmpty(strName) || a.str_Name.Trim().ToLower().Contains(strName))).ToArray().OrderByDescending(x=>x.Room_Id);
                var verticalBar = GetByKey("VerticalBar");

                var filtered = data.Select(a => new AjaxRoom
                {
                    Code = a.str_code ?? string.Empty,
                    Name = "<a href='" + @Url.Action("Modify", new { id = a.Room_Id }) + "'>" + a.str_Name + "</a>",
                    Capacity = a.int_Capacity.ToString(),
                    Equipment = a.str_Equipment ?? string.Empty,
                    Area = a.int_Area ?? string.Empty,
                    Location = a.str_Location ?? string.Empty,
                    Status = (a.isActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Room(0," + a.Room_Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Room(1," + a.Room_Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                     ((User.IsInRole("/Room/Modify")) ? "<a title='Edit' href='" + @Url.Action("Modify", new { id = a.Room_Id }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : string.Empty) +

                        ((User.IsInRole("/Room/Delete")) ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + a.Room_Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty)
                    ),

                    RoomType = a?.Room_Type?.Name?.ToString() ?? ""
                });
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxRoom, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.RoomType
                                                          : sortColumnIndex == 3 ? c.Name
                                                          : sortColumnIndex == 4 ? c.Capacity
                                                          : sortColumnIndex == 6 ? c.Area
                                                          : null);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var ajaxRooms = filtered.ToArray();

                var displayed = ajaxRooms.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         c?.Code,
                         c?.RoomType,
                         c?.Name,
                         c?.Capacity,
                         c?.Equipment,
                         c?.Area,
                         c?.Location,
                         c?.Status,
                         c?.Option

                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxRooms.Count(),
                    iTotalDisplayRecords = ajaxRooms.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Room/AjaxHandler", ex.Message);
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
                var model = CourseService.GetRoomById(id);
                //model.bit_Deleted = true;
                if (model.Course_Detail.Any(a => a.IsDeleted == false))
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = String.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.str_code),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.isDeleted = true;
                model.isActive = false;
                model.bit_Deleted = true;
                model.dtm_Deleted_At = DateTime.Now;
                var currentUser = GetUser().USER_ID.ToString();
                model.str_Deleted_By = currentUser;
                CourseService.UpdateRoom(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.str_Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Room/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult SubmitSetParticipateRoom(int isParticipate, string id, FormCollection form)
        {
            int idroom = int.Parse(id);
            var removeRoom = CourseService.GetRoomById(idroom);
            if (removeRoom == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                removeRoom.isActive = false;
            }
            else
            {
                removeRoom.isActive = true;
            }
            CourseService.UpdateRoom(removeRoom);
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, removeRoom.str_Name),
                result = true
            }, JsonRequestBehavior.AllowGet);
        }

        private string[] InsertRoomCols = new[] { "EID", "CODE", "NAME", "CAPACITY", "AREA", "LOCATION", "EQUIPMENT", "ROOM_TYPE" };

        public class RoomImport
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Capacity { get; set; }
            public string Area { get; set; }
            public string Location { get; set; }
            public string Equipment { get; set; }
            public string Room_type { get; set; }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportRoom(HttpPostedFileBase postedFile)
        {
            try
            {
                postedFile = Request.Files["postedFile"];
                //int hd_courseID = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);

                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => InsertRoomCols.Contains(a)))
                    {//todo: valid template
                        return Json(new AjaxResponseViewModel { message = Messege.VALIDATION_FILE, result = false }, JsonRequestBehavior.AllowGet);
                    }
                    var model = new List<RoomModels>();
                    var code = "";
                    for (var j = 2; j <= worksheet.Tables[0].Address.Rows; j++)
                    {
                        code = worksheet.Cells[j, 1].Text.Trim();
                        var room = CourseService.GetRoom(a => a.str_code.ToLower() == code.ToLower())?.FirstOrDefault();
                        var type = worksheet.Cells[j, 7].Text;
                        var value = CourseService.GetRoomType(a => a.Name.ToLower() == type.ToLower())?.FirstOrDefault();
                        if (room != null)
                        {
                            model.Add(new RoomModels()
                            {
                                Id = room.Room_Id,
                                Code = room.str_code,
                                Name = worksheet.Cells[j, 2].Text,
                                Capacity = Convert.ToInt32(worksheet.Cells[j, 3].Text),
                                Area = worksheet.Cells[j, 4].Text,
                                Location = worksheet.Cells[j, 5].Text,
                                Equipment = worksheet.Cells[j, 6].Text,
                                Is_Meeting = value?.Id ?? 1,
                            });
                        }
                        else
                        {
                            model.Add(new RoomModels()
                            {
                                Code = worksheet.Cells[j, 1].Text,
                                Name = worksheet.Cells[j, 2].Text,
                                Capacity = Convert.ToInt32(worksheet.Cells[j, 3].Text),
                                Area = worksheet.Cells[j, 4].Text,
                                Location = worksheet.Cells[j, 5].Text,
                                Equipment = worksheet.Cells[j, 6].Text,
                                Is_Meeting = value?.Id ?? 1,
                            });
                        }
                    }
                    if (model.Any())
                    {
                        CourseService.ModifyRoom(model);
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                        {
                            result = true,
                            message = Messege.SUCCESS
                        };
                        return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
                    }
                }

                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Room/Modify", Messege.UNSUCCESS);
                return Json(new AjaxResponseViewModel { result = false, message = Messege.UNSUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Room/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}