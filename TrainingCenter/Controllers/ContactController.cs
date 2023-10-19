using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Contact;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Contact;
using Utilities;

namespace TrainingCenter.Controllers
{
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.IO;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TrainingCenter.Utilities;

    public class ContactController : BaseAdminController
    {
        private readonly IContactService _repoContact;
        public ContactController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService repoApproveService, IContactService repoContact) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, repoApproveService)
        {
            _repoContact = repoContact;
        }

        // GET: Contact
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Modify(int? id)
        {
            var entity = _repoContact.GetById(id);
            var contactor = new ContactDetails();

            if (entity != null)
            {
                //contactor.isDeleted = false;
                contactor.Id = entity.Id;
                contactor.Phone = entity.Phone;                
                contactor.FullName = entity.FullName;
                contactor.Email = entity.Email;
                contactor.Company = entity.Company;
            }

            return View(contactor);

        }

        [HttpPost]
        public ActionResult Modify(ContactDetails model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Email);
                if (model.Email.Contains(" "))
                {
                    return Json(new AjaxResponseViewModel { result = false, message = codeHasSpaceMessage }, JsonRequestBehavior.AllowGet);
                }
                var entity = _repoContact.GetById(model.Id);
                if (entity == null)
                {
                    //var test = _repoContact.GetAll(a => a.str_Code.ToLower() == model.Code.ToLower());
                    //var xx = new List<string>();
                    //foreach (var item in test)
                    //{
                    //    xx.Add(item.str_Code);
                    //}
                    //if (_repoContact.GetContractor(a => a.str_Code.ToLower() == model.Code.ToLower()).Any())
                    //{

                    //    return Json(new AjaxResponseViewModel { result = false, message = string.Format(Messege.EXISTING_CODE, model.Code) }, JsonRequestBehavior.AllowGet);

                    //}

                    _repoContact.InsertContact(model);
                }
                else
                {
                    //if (_repoContact.GetContractor(a => a.str_Code.ToLower() == model.Code.ToLower() && a.id != model.Id).Any())
                    //{
                    //    return Json(new AjaxResponseViewModel { result = false, message = string.Format(Messege.EXISTING_CODE, model.Code) }, JsonRequestBehavior.AllowGet);
                    //}
                    _repoContact.UpdateContact(model);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contractor/Modify", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            var result = new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true
            };
            TempData[UtilConstants.NotifyMessageName] = result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                //var comOrDepId = string.IsNullOrEmpty(Request.QueryString["Fullname"]) ? -1 : Convert.ToInt32(Request.QueryString["Fullname"].Trim());
                var fullName = string.IsNullOrEmpty(Request.QueryString["Fullname"]) ? string.Empty : Request.QueryString["Fullname"].Trim();
                var email = string.IsNullOrEmpty(Request.QueryString["Email"]) ? string.Empty : Request.QueryString["Email"].Trim();
                var phone = string.IsNullOrEmpty(Request.QueryString["Phone"]) ? string.Empty : Request.QueryString["Phone"].Trim();
                var company = string.IsNullOrEmpty(Request.QueryString["Company"]) ? string.Empty : Request.QueryString["Company"].Trim();
                var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();


                var data = _repoContact.GetContact(a =>
                    (string.IsNullOrEmpty(fullName) || a.FullName.Contains(fullName)) &&
                    (string.IsNullOrEmpty(email) || a.Email.Contains(email)) &&
                    (string.IsNullOrEmpty(phone) || a.Phone.Contains(phone)) &&
                    (string.IsNullOrEmpty(company) || a.Company.Contains(company))
                    );

                IEnumerable<INFO_CONTACT> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<INFO_CONTACT, object> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c.FullName
                                                             : sortColumnIndex == 2 ? c.Email
                                                             : sortColumnIndex == 3 ? c.Company
                                                             : sortColumnIndex == 4 ? c.Phone:""
                                                             );
                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(orderingFunction)
                                  : filtered.OrderBy(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                    let any = c?.INFO_REPLY_CONTACT?.Any()
                    select new object[]
                    {
                        string.Empty,
                        c?.FullName,
                        c?.Email,
                        c?.Company,
                        c?.Phone,
                        c.IsActive == false ?
                          "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Contact(0,"+c.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" :
                          "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Contact(1,"+c.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                        ((Is_Edit(url)) ?
                        "<a   title='Edit'  href='"+@Url.Action("Modify",new{id = c.Id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "") +
                        ((Is_Delete(url)) ?
                        verticalBar +"<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")                        

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                     JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Details(int id)
        {
            var model = _repoContact.GetById(id);
            if (model == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                return RedirectToAction("Index", "Contact");
            }
            var entity = new ContactDetails();
            entity.Id = model.Id;
            entity.FullName = model.FullName;
            entity.Email = model.Email;
            entity.Phone = model.Phone;
            entity.Address = model.Address;
            entity.Company = model.Company;
            entity.Subject = model.Subject;
            entity.Description = model.Content;
            entity.Email = model.Email;
            if (model.INFO_REPLY_CONTACT.Any())
            {
                entity.MyReply = model.INFO_REPLY_CONTACT.Select(a => new ContactDetails.ReplyContact()
                {
                    Id = a.Id,
                    ReplyContent = a.Content,
                    ReplyEmail = a.Email,
                    ReplySubject = a.Subject
                }).FirstOrDefault();
            }
           
            return View(entity);
        }

        public ActionResult Print(int? id)
        {
            var model = _repoContact.GetById(id);
            if (model == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                return PartialView("_partials/_PartialContactDetail");
            }
            var entity = new ContactDetails();
            entity.Id = model.Id;
            entity.FullName = model.FullName;
            entity.Email = model.Email;
            entity.Phone = model.Phone;
            entity.Address = model.Address;
            entity.Company = model.Company;
            entity.Subject = model.Subject;
            entity.Description = model.Content;
            entity.Email = model.Email;
            if (model.INFO_REPLY_CONTACT.Any())
            {
                entity.MyReply = model.INFO_REPLY_CONTACT.Select(a => new ContactDetails.ReplyContact()
                {
                    Id = a.Id,
                    ReplyContent = a.Content,
                    ReplyEmail = a.Email,
                    ReplySubject = a.Subject
                }).FirstOrDefault();
            }

            return PartialView("_partials/_PartialContactDetail", entity);
        }
        [HttpPost]
        public ActionResult ReplyContact(ContactDetails model)
        {
            #region [VALIDATION]
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
                var result = _repoContact.Insert(model);
                if (result)
                {
                    //TODO:SMS
                  var boolSendMail=   MailUtil.SendMail(model.Email, model.FSubject, model.FContent);
                    var temp = new AjaxResponseViewModel();
                    if (boolSendMail)
                    {
                        temp.result = true;
                        temp.message = Messege.SEND_SUCCESS_EMAIL;
                    }
                    else
                    {
                       
                        temp.result = false;
                        temp.message = Messege.UNABLE_TO_SEND_MAIL;
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/ReplyContact", Messege.UNABLE_TO_SEND_MAIL);
                    }
                    TempData[UtilConstants.NotifyMessageName] = temp;
                    return Json(temp, JsonRequestBehavior.AllowGet);
                }
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/ReplyContact", Messege.UNABLE_TO_SEND_MAIL);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.UNABLE_TO_SEND_MAIL
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception  ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/ReplyContact", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
        }

        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = _repoContact.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }


                model.IsDeleted = true;
                model.IsActive = false;                
                _repoContact.Update(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.FullName),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contractor/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
       
        [HttpPost]
        public ActionResult SubmitSetParticipateContact(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idContact = int.Parse(id);
                var removeContact = _repoContact.GetById(idContact);
                if (removeContact == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/SubmitSetParticipateContact", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeContact.IsActive = false;
                }
                else
                {
                    removeContact.IsActive = true;
                }
                _repoContact.Update(removeContact);


                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeContact.FullName), result = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/SubmitSetParticipateContact", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }


        }

        #region[Export Excel]
        [AllowAnonymous]
        public FileResult ExportEXCEL(FormCollection form)
        {
            var filecontent = ExportEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "ContactList.xlsx");
            }
            return null;
        }

        private byte[] ExportEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/ContactList.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);           
            var fullname = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : (Request.QueryString["FullName"].Trim());
            var email = string.IsNullOrEmpty(Request.QueryString["Email"]) ? string.Empty : (Request.QueryString["Email"]);
            var company = string.IsNullOrEmpty(Request.QueryString["Company"]) ? string.Empty : (Request.QueryString["Company"]);
            var phone = string.IsNullOrEmpty(Request.QueryString["Phone"]) ? string.Empty : (Request.QueryString["Phone"]);

            var data = _repoContact.GetContact(a =>
            (string.IsNullOrEmpty(fullname) || a.FullName == fullname) &&
            (string.IsNullOrEmpty(email) || a.Email == email) &&
            (string.IsNullOrEmpty(company) || a.Company == company) &&
            (string.IsNullOrEmpty(phone) ||a.Phone.Contains(phone)));

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


                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = item?.FullName;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellEmail = worksheet.Cells[startRow + 1, ++col];
                        cellEmail.Value = item?.Email;
                        cellEmail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellEmail.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellEmail.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellCompany = worksheet.Cells[startRow + 1, ++col];
                        cellCompany.Value = item?.Company;
                        cellCompany.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCompany.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellCompany.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellPhone = worksheet.Cells[startRow + 1, ++col];
                        cellPhone.Value = item?.Phone;
                        cellPhone.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellPhone.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellPhone.Style.Border.BorderAround(ExcelBorderStyle.Thin);                        

                        ExcelRange cellActive = worksheet.Cells[startRow + 1, ++col];
                        cellActive.Value = item?.bit_Is_active == true ? "Active" : "Deactivated";
                        cellActive.Style.WrapText = true;
                        cellActive.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellActive.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellActive.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        startRow++;

                    }
                    bytes = excelPackage.GetAsByteArray();
                }

            }
            return bytes;

        }
        #endregion
    }
}