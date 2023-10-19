using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.ViewModels.Common;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using RestSharp.Extensions;
    using System.Data.Entity.SqlServer;
    using System.Globalization;
    using System.Text;
    using System.Web.Script.Serialization;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.AjaxModels.AjaxRoom;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.Room;

    public class MeetingController : BaseAdminController
    {
        #region MyRegion
        private readonly IJobtitleService _repoJobTiltle;
        private readonly IDepartmentService _repoDepartment;
        private readonly IApproveService _repoTmsApproves;
        public MeetingController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, ICourseService courseService, IDepartmentService departmentService, IApproveService approveService, IJobtitleService repoJobTiltle) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoJobTiltle = repoJobTiltle;
            _repoDepartment = departmentService;
            _repoTmsApproves = approveService;
        }

        #endregion
        // GET: Meeting
        [AllowAnonymous]
        public ActionResult Index()
        {
            var model = new MeetingModels();
            var list_Room = CourseService.GetRoom(a => a.is_Meeting == 2).OrderByDescending(a => a.Room_Id).ToList();
            if (list_Room.Count() != 0)
            {
                model.ListRoom = list_Room.ToDictionary(a => a.Room_Id, a => a.str_Name);
            }
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                var room_Meeting = string.IsNullOrEmpty(Request.QueryString["Room_Meeting"]) ? -1 : Convert.ToInt32(Request.QueryString["Room_Meeting"].Trim());

                string fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? string.Empty : Request.QueryString["fSearchDate_from"].Trim();
                string fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? string.Empty : Request.QueryString["fSearchDate_to"].Trim();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                var data = CourseService.GetMeeting(a => (room_Meeting == -1 || a.RoomID == room_Meeting) && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0)).OrderByDescending(a => a.id).ToList();
                IEnumerable<Meeting> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Meeting, string> orderingFunction_ = (c => sortColumnIndex == 1 ? c.Name
                                                            : sortColumnIndex == 1 ? c.Room?.str_Name
                                                            : sortColumnIndex == 4 ? c.StartDate?.ToString("dd/MM/yyyy")
                                                            : sortColumnIndex == 5 ? c.EndDate?.ToString("dd/MM/yyyy")
                                                            : c.Name);
                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction_)
                                  : filtered.OrderByDescending(orderingFunction_);


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let Participants = GroupPartianItem(c?.id)
                             select new object[] {
                               string.Empty,
                                 c?.Name,
                                 c?.Room?.str_Name,
                                 c?.StartDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                                 c?.EndDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                                 c?.Description,
                                 c?.Room?.str_Location ?? string.Empty,
                                 Participants,
                                c?.IsActive == false? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Meeting(0," + c.id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;' 'data-toggle='tooltip'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Meeting(1," + c.id + ")' aria-hidden='true' title='Active' style='cursor: pointer;' 'data-toggle='tooltip'></i>",
                                
                                ( true/*User.IsInRole("/Meeting/Modify")*/ ? "<a title='Edit' href='" + @Url.Action("Modify", new { id = c.id }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : string.Empty) +

                                (true/*User.IsInRole("/Meeting/Delete")*/ ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty)
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/AjaxHandler", ex.Message);
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
        private string GroupPartianItem(int? id)
        {
            StringBuilder HTML = new StringBuilder();
            var list_participant = CourseService.GetMeetingParticipants(a => a.Meeting_Id == id && a.IsActive == true).ToList();
            if (list_participant == null) return HTML.ToString();

            foreach (var groupParticipant in list_participant.Select(item => EmployeeService.GetById(item.Participant_Id)))
            {
                HTML.Append("<p> - " + ReturnDisplayLanguage(groupParticipant?.FirstName, groupParticipant?.LastName) + "<br/></p>");
            }
            return HTML.ToString();
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerAvailableParticipants(jQueryDataTableParamModel param)
        {
            try
            {
                var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList2"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList2"].Trim());
                var jobtitleId = string.IsNullOrEmpty(Request.QueryString["JobtitleList2"]) ? -1 : Convert.ToInt32(Request.QueryString["JobtitleList2"].Trim());
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["EID2"]) ? string.Empty : Request.QueryString["EID2"].Trim().ToLower();
                var fullName = string.IsNullOrEmpty(Request.QueryString["FullName2"]) ? string.Empty : Request.QueryString["FullName2"].Trim().ToLower();
                var meetingId = string.IsNullOrEmpty(Request.QueryString["meeting_ID"]) ? -1 : Convert.ToInt32(Request.QueryString["meeting_ID"].Trim());
                List<int?> participants = new List<int?>();
                var listMeeting = CourseService.GetMeetingParticipants(a => a.Meeting_Id == meetingId);
                if (listMeeting.Count() != 0)
                {
                    participants = listMeeting.Select(a => a.Participant_Id).ToList();
                }
                var data = EmployeeService.Get(a => !participants.Contains(a.Id) &&
                                 (departmentId != -1 || jobtitleId != -1 || !string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(traineeCode))
                                 && (departmentId == -1 || a.Department_Id == departmentId)
                                 && (jobtitleId == -1 || a.Job_Title_id == jobtitleId)
                                 && (string.IsNullOrEmpty(traineeCode) || a.str_Staff_Id.Contains(traineeCode))
                                 && (string.IsNullOrEmpty(fullName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fullName.Trim()))));

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Department?.Name
                                                             : sortColumnIndex == 4 ? c?.JobTitle?.Name
                                                          : c.str_Staff_Id);

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
                                   c?.Department?.Name,
                                   c?.JobTitle?.Name,
                                   "<input type='checkbox' name='id2[]' value='"+c?.Id+"'>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/AjaxHandlerAvailableParticipants", ex.Message);
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
        public ActionResult AjaxHandlerSelectParticipants(jQueryDataTableParamModel param)
        {
            try
            {
                var meetingId = string.IsNullOrEmpty(Request.QueryString["meeting_ID"]) ? -1 : Convert.ToInt32(Request.QueryString["meeting_ID"].Trim());

                List<int?> participants = new List<int?>();
                var list_meeting = CourseService.GetMeetingParticipants(a => a.Meeting_Id == meetingId);
                if (list_meeting.Count() != 0)
                {
                    participants = list_meeting.Select(a => a.Participant_Id).ToList();
                }
                var data = EmployeeService.Get(a => participants.Contains(a.Id));
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Department?.Name
                                                             : sortColumnIndex == 4 ? c?.JobTitle?.Name
                                                          : c.LastName);

                var filtered = (Request["sSortDir_0"] == "asc")
                   ? data.OrderBy(orderingFunction)
                   : data.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                   c?.str_Staff_Id,
                                   ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                   c?.Department?.Name,
                                   c?.JobTitle?.Name,
                                   (checkActiveParticipant(c?.Id,meetingId) == false? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Item(0," + c.Id + ","+ meetingId +")' aria-hidden='true' title='Inactive' style='cursor: pointer;' 'data-toggle='tooltip'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Item(1," + c.Id + ","+ meetingId +")' aria-hidden='true' title='Active' style='cursor: pointer;' 'data-toggle='tooltip'></i>")
                                   //"<input type='checkbox' name='id3[]' value='"+c?.Id+"'>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/AjaxHandlerSelectParticipants", ex.Message);
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
        private bool? checkActiveParticipant(int? id_participant, int? id_meeting)
        {
            var Check_Participant = CourseService.GetMeetingParticipants(a => a.Meeting_Id == id_meeting && a.Participant_Id == id_participant);
            bool? check = false;
            if (Check_Participant.Any())
            {
                var item_participant = Check_Participant.FirstOrDefault();
                check = item_participant.IsActive;
            }
            return check;
        }
        [AllowAnonymous]
        public ActionResult Modify(int? id)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var meeting = new MeetingModels();
            var list_Room = CourseService.GetRoom(a => a.is_Meeting == 2).OrderByDescending(a => a.Room_Id).ToList();
            if (list_Room.Count() != 0)
            {
                meeting.ListRoom = list_Room.ToDictionary(a => a.Room_Id, a => a.str_Name);
            }
            //meeting.Departments =
            //        GetDepartmentModel().OrderBy(a => a.Ancestor)
            //            .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.DepartmentName));
            meeting.Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster);
            meeting.JobTitles = _repoJobTiltle.Get(a => a.IsDelete == false).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name);
            if (id.HasValue)
            {
                var entity = CourseService.GetMeetingById(id);
                meeting.Id = entity.id;
                meeting.Name = entity.Name;
                meeting.RoomID = entity.RoomID;
                meeting.StartDate = entity.StartDate;
                meeting.EndDate = entity.EndDate;
                meeting.TimeFrom = entity.TimeFrom;
                meeting.TimeTo = entity.TimeTo;
                meeting.Description = entity.Description;
            }
            return View(meeting);

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Modify(MeetingModels model, FormCollection form)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var meetingId = string.IsNullOrEmpty(form["meeting_ID"]) ? -1 : Convert.ToInt32(form["meeting_ID"].Trim());
                model.Id = meetingId;
                object[] idtrainee = form["id2[]"] != null ? form.GetValues("id2[]") : null;
                idtrainee = CMSUtils.SetObjectNull(idtrainee);
                if (idtrainee == null || !idtrainee.Any())
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/Modify", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    });
                }
                model.ListParticipant = idtrainee;
                if (!ModelState.IsValid)
                    return Json(new { result = false, message = MessageInvalidData(ModelState) });

                var entity = CourseService.GetMeetingById(model.Id);
                var currentUser = GetUser().Username.ToString(CultureInfo.CurrentCulture);
                model.EndDate = model.EndDate?.AddHours(23).AddMinutes(59).AddSeconds(59);
                var timeFrom = DateTime.ParseExact(model.TimeFrom, "HH:mm", CultureInfo.InvariantCulture);
                var timeTo = DateTime.ParseExact(model.TimeTo, "HH:mm", CultureInfo.InvariantCulture);
                Meeting record = new Meeting();
                if (entity == null)
                {
                    if (CourseService.GetMeeting(a => a.Name.Trim().ToLower().CompareTo(model.Name.Trim().ToLower()) == 0).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE });
                    }

                    if (model.StartDate > model.EndDate)
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.VALIDATION_COURSE_FROM_THAN_TO });
                    }
                    var checkList =
                        CourseService.GetMeeting(
                            a => a.RoomID == model.RoomID &&
                                ((a.StartDate <= model.StartDate && model.StartDate <= a.EndDate) ||
                                (a.EndDate >= model.EndDate && model.EndDate <= a.EndDate)));
                    if (checkList.Any())
                    {
                        foreach (var item in checkList)
                        {
                            if (!string.IsNullOrEmpty(item.TimeFrom) || !string.IsNullOrEmpty(item.TimeTo))
                            {
                                var timeFromConvert = DateTime.ParseExact(item.TimeFrom, "HH:mm", CultureInfo.InvariantCulture);
                                var timeToConvert = DateTime.ParseExact(item.TimeTo, "HH:mm", CultureInfo.InvariantCulture);
                                if ((timeFromConvert >= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert >= timeTo && timeTo <= timeToConvert))
                                {
                                    string message = Messege.WARNING_CREATEMEETING_ROOM;
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        type = (int)UtilConstants.TypeCheck.Checked,
                                        message = message + "<br />",
                                        result = false
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    record = CourseService.InsertMeeting(model);
                }
                else
                {
                    if (CourseService.GetMeeting(a => a.Name.Trim().ToLower().CompareTo(model.Name.Trim().ToLower()) == 0 && a.id != model.Id).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE });
                    }

                    var checkList =
                        CourseService.GetMeeting(
                            a => a.RoomID == model.RoomID && a.id != model.Id && (
                                (a.StartDate <= model.StartDate && model.StartDate <= a.EndDate) ||
                                (a.EndDate >= model.EndDate && model.EndDate <= a.EndDate)));
                    if (checkList.Any())
                    {
                        foreach (var item in checkList)
                        {
                            if (!string.IsNullOrEmpty(item.TimeFrom) || !string.IsNullOrEmpty(item.TimeTo))
                            {
                                var timeFromConvert = DateTime.ParseExact(item.TimeFrom, "HH:mm", CultureInfo.InvariantCulture);
                                var timeToConvert = DateTime.ParseExact(item.TimeTo, "HH:mm", CultureInfo.InvariantCulture);
                                if ((timeFromConvert >= timeFrom && timeFrom <= timeToConvert) || (timeFromConvert >= timeTo && timeTo <= timeToConvert))
                                {
                                    const string message = "bị trùng";
                                    return Json(new AjaxResponseViewModel()
                                    {
                                        type = (int)UtilConstants.TypeCheck.Checked,
                                        message = message + "<br />",
                                        result = false
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    record = CourseService.UpdateMeeting(model);
                }
                if (record != null)
                {
                    var list_participant = CourseService.GetMeetingParticipants(a => a.Meeting_Id == record.id && a.IsActive == true).ToList();
                    if (list_participant.Count() != 0)
                    {
                        var id_list = list_participant.Select(a => a.Participant_Id).ToList();
                        if (model.check_Meeting != null)
                        {
                            const int typeSendmail = (int)UtilConstants.TypeSentEmail.SentMailMeeting;
                            foreach (var item in id_list)
                            {
                                var employee = EmployeeService.GetById(item);
                                var body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailMeeting, null, employee, null);
                                InsertSentMail(employee.str_Email, typeSendmail, body_Ins, -1);
                            }
                        }

                        const string notiTemplate = "Meeting";
                        if (record.StartDate != null)
                        {
                            if (record.EndDate != null)
                            {
                                var notiContent = string.Format("You have a meeting at {0} from {1} to {2} in room {3} - {4}", record.TimeFrom, record.StartDate.Value.ToString("dd/MM/yyyy"), record.EndDate.Value.ToString("dd/MM/yyyy"), record.Room?.str_Name, record.Room?.str_Location);
                                foreach (var item in id_list)
                                {
                                    SendNotification((int)UtilConstants.NotificationType.AutoProcess, null, null, item, DateTime.Now, notiTemplate, notiContent, notiTemplate, notiContent);
                                }
                            }
                        }
                    }
                }
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = CourseService.GetMeetingById(id);
                if (model != null)
                {
                    if (model.StartDate < DateTime.Now && DateTime.Now < model.EndDate)
                    {
                        return Json(new AjaxResponseViewModel
                        {
                            message = String.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name),
                            result = false
                        }, JsonRequestBehavior.AllowGet);
                    }
                    model.IsDeleted = true;
                    model.IsActive = false;
                    model.DeletedDate = DateTime.Now;
                    model.DeletedBy = GetUser().USER_ID;
                    CourseService.UpdateMeeting(model);

                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.DELETED_UNSUCCESS_AVAILABLE,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //model.bit_Deleted = true;

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Meeting/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SubmitSetParticipateMeeting(int isParticipate, string id)
        {
            int idMeeting = int.Parse(id);
            var removeMeeting = CourseService.GetMeetingById(idMeeting);
            if (removeMeeting == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                removeMeeting.IsActive = false;
            }
            else
            {
                removeMeeting.IsActive = true;
            }
            CourseService.UpdateMeeting(removeMeeting);

            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, removeMeeting.Name),
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SubmitSetParticipate(int isParticipate, string id, string meeting_id)
        {
            int idPartiant = int.Parse(id);
            int idMeeting = int.Parse(meeting_id);
            var Check_Participant = CourseService.GetMeetingParticipants(a => a.Meeting_Id == idMeeting && a.Participant_Id == idPartiant);
            if (!Check_Participant.Any())
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            var item_participant = Check_Participant.FirstOrDefault();
            var employee = EmployeeService.GetById(idPartiant);
            if (isParticipate == 1)
            {
                item_participant.IsActive = false;
            }
            else
            {
                item_participant.IsActive = true;
            }

            CourseService.UpdateMeetingParticipant(item_participant);

            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, ReturnDisplayLanguage(employee?.FirstName, employee?.LastName)),
                result = true,
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult ListRoom()
        {
            ViewBag.DepartmentList = _repoDepartment.Get().OrderBy(m => m.Name).ToList();
            ViewBag.JobTitleList = new SelectList(_repoJobTiltle.Get().OrderBy(m => m.Name).ToList(), "Id", "Name");
            ViewBag.CurrentMonthOfRoomView = DateTime.Today;
            #region[Status]
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
            #endregion
            ViewBag.Status = Status;
            #region[ApproveType]
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
            #endregion
            ViewBag.CourseTypeList = ApproveType;// new SelectList(ApproveType, "id", "Name");
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult CreateRoom(int? id)
        {
            var model = new CourseModifyModel();
            var stringDetail = "[]";
           
            model.DictionaryRooms = CourseService.GetRoom().OrderBy(a => a.str_Name).ToDictionary(a => a.Room_Id, a => a.str_Name);
            string listroom = string.Empty;
            foreach (var item in model.DictionaryRooms)
            {
                listroom += "<option value='" + item.Key + "'>" + item.Value + "</option>";
            }
            model.HtmlRoom = listroom;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var detail = CourseDetailService.GetById(id);
            if(detail != null)
            {
                var selectlist = CourseService.GetCourseDetailRooms(a => a.CourseDetailID == id);
                if(selectlist.Count() > 0)
                {
                    var model_list = selectlist.AsEnumerable().Select(x => new ListDetailRoomViewModel
                     {
                         DateTime = x.LearningDate.HasValue ? x.LearningDate.Value.ToString("yyyy-MM-dd") : "",
                         RoomID = x.RoomIdOther == -1 ? -1 : x.RoomId,
                         RoomOther = x.RoomOther,
                     });
                    stringDetail = new JavaScriptSerializer().Serialize(model_list);
                }
                model.strCourseDetailRoom = stringDetail;
                model.CourseId = (int)detail.CourseId;
                model.Code = detail.Course.Code;
                model.Name = detail.Course.Name;
                model.BeginDate = detail.Course.StartDate;
                model.EndDate = detail.Course.EndDate;
                model.CourseDetailitems = new CourseDetailModel()
                {
                    Id = detail.Id,
                    TimeTo = detail.time_to,
                    TimeFrom = detail.time_from,
                    DateFrom = detail.dtm_time_from,
                    DateTo = detail.dtm_time_to,
                    Room = detail.RoomId ?? 0,
                    SubjectName = detail.SubjectDetail.Name,
                    SubjectCode = detail.SubjectDetail.Code,
                    SubjectType = detail.SubjectDetail.CourseTypeId.HasValue ? detail.SubjectDetail.Course_Type.str_Name : "",
                    IdGlobal = detail?.Course_Detail_Room_Global.FirstOrDefault()?.ID ?? 0,
                    RoomIdGlobal = detail?.Course_Detail_Room_Global.FirstOrDefault()?.RoomID ?? 0,
                    Remark = detail?.Course_Detail_Room_Global.FirstOrDefault()?.Remark,
                };
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateRoom(CourseRoomViewModel model)
        {
            try
            {
                var detail_global = CourseService.GetCourseDetailGlobalById((int)model.IdGlobal);
                if(detail_global != null)
                {
                    detail_global.Remark = model.Remark;
                    detail_global.RoomID = model.RoomIDGlobal;
                    CourseService.UpdateCourseDetailRoomGlobal(detail_global);
                }
                else
                {
                    var record = new Course_Detail_Room_Global();
                    record.Remark = model.Remark;
                    record.RoomID = model.RoomIDGlobal;
                    record.CourseDetailID = model.CourseDetailId;
                    CourseService.InsertCourseDetailRoomGlobal(record);
                }

                var detail = CourseService.GetCourseDetailRooms(a => a.CourseDetailID == model.CourseDetailId).ToList();
                if(detail.Count() > 0)
                {
                    CourseService.Delete(detail);
                }
                
                IList<ListDetailRoomViewModel> lstRoom = new List<ListDetailRoomViewModel>();
                string strlstRoom = string.Concat("[", model.ListDetailRoom.Trim(','), "]");
                lstRoom = new JavaScriptSerializer().Deserialize<IList<ListDetailRoomViewModel>>(strlstRoom);
                foreach (var item in lstRoom)
                {
                    var data = new Course_Detail_Room();
                    data.CourseDetailID = model.CourseDetailId;
                    if(item.RoomID != -1)
                    {
                        data.RoomId = item.RoomID;
                    }
                    else
                    {
                        data.RoomIdOther = item.RoomID;
                    }
                    
                    data.LearningDate = DateUtil.StringToDate2(item.DateTime, DateUtil.DATE_FORMAT_OUTPUT);
                    data.RoomOther = item.RoomOther;
                    CourseService.ModifyCourseDetailRoom(data);
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
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerCourseRoom(jQueryDataTableParamModel param)
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
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                var xxx = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                var yyy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                var data = CourseService.Get(a => /*a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)) &&*/
                            (string.IsNullOrEmpty(fCode) || a.Code.Contains(fCode)) &&
                            (string.IsNullOrEmpty(fName) || a.Name.Contains(fName)) &&
                            (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                            (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) &&
                            (fCourseType == -1 || a.TMS_APPROVES.Any(m => m.int_Type == fCourseType)) &&
                            (fStatus == -1 || a.TMS_APPROVES.Any(m => m.int_id_status == fStatus)) && a.Course_Detail.Count(x => x.type_leaning != (int)UtilConstants.LearningTypes.Online) > 0
                            , true);
                IEnumerable<Course> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                            : sortColumnIndex == 3 ? (object)c.StartDate
                                                            : sortColumnIndex == 4 ? (object)c.IsActive
                                                            : c.Id);


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

                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+ c.Name+"</a></span><br />"+  (c.StartDate.HasValue ? c.StartDate.Value.ToString("dd/MM/yyyy") : "") +" - "+(c.EndDate.HasValue ?  c.EndDate.Value.ToString("dd/MM/yyyy") : ""),           "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle btnIcon_gray font-byhoa' aria-hidden='true'></i></span>"

                             };


                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
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
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectRoom(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var model = CourseDetailService.GetByCourse(id).OrderBy(a => a.SubjectDetail.Name);

                IEnumerable<Course_Detail> filtered = model.Where(a=> a.type_leaning != (int)UtilConstants.LearningTypes.Online);

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
                var result = from c in displayed.ToArray()
                             select new object[] {
                            string.Empty,
                            c.SubjectDetail?.Code,
                            c.SubjectDetail?.Name,
                            c.SubjectDetail.CourseTypeId.HasValue ? courseTypes[c.SubjectDetail.CourseTypeId.Value] : "",
                            c.SubjectDetail?.IsAverageCalculate == true ? "Yes" : "No",
                            TypeLearningName(c.type_leaning.Value),
                            return_score_pass(c.SubjectDetail),
                            c.SubjectDetail?.RefreshCycle,
                            c.SubjectDetail?.Duration,
                            (HttpContext.User.IsInRole("/Course/Create") ?
                             "<a title='Edit' href='"+@Url.Action("CreateRoom",new{id = c.Id})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" :"") 
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
        [AllowAnonymous]
        [HttpGet]
        public string GetValueRoom(int? roomId)
        {
            var DictionaryRooms = CourseService.GetRoom(a=>a.isActive == true).OrderBy(a => a.str_Name).ToDictionary(a => a.Room_Id, a => a.str_Name);
            string listroom = string.Empty;
            foreach (var item in DictionaryRooms)
            {
                if(item.Key == roomId)
                {
                    listroom += "<option value='" + item.Key + "' selected >" + item.Value + "</option>";
                }else
                {
                    listroom += "<option value='" + item.Key + "' >" + item.Value + "</option>";
                }
                
            }
            listroom += "<option value='-1'> Other </option>";
            return listroom;
        }
    }
}