using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using TMS.Core.Services;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.Contracts;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Contractors;

    public class ContractorController : BaseAdminController
    {
        #region MyRegion

        private readonly IContractService _repoContractor;


        #endregion
        //
        // GET: /Admin/User/
        #region Index
        // return view
        public ContractorController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IContractService repoContractor, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoContractor = repoContractor;
        }

        #region Export EXCEL
        public FileResult ExportEXCEL(FormCollection form)
        {
            var filecontent = ExportEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "Contractor_List.xlsx");
            }
            return null;
        }
        private byte[] ExportEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/Contractor_List.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);

            var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().Trim();
            var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().Trim();
            var sortname = string.IsNullOrEmpty(Request.QueryString["sortname"]) ? string.Empty : Request.QueryString["sortname"].ToLower().Trim();
            var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();

            var data = _repoContractor.GetContractor(a =>
           (string.IsNullOrEmpty(strCode) || a.str_Code.Contains(strCode)) &&
           (string.IsNullOrEmpty(sortname) || a.str_Sortname.Contains(sortname)) &&
           (string.IsNullOrEmpty(strName) || a.str_Fullname.Contains(strName))).OrderByDescending(a => a.dtm_Created_date);


            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (data != null)
            {
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 9;
                    //var Row = 0;
                    int count = 0;
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
                        cellCode.Value = item?.str_Code;
                        cellCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellSortName = worksheet.Cells[startRow + 1, ++col];
                        cellSortName.Value = item?.str_Sortname;
                        cellSortName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellSortName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellSortName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = item?.str_Fullname;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellActive = worksheet.Cells[startRow + 1, ++col];
                        cellActive.Value = item?.bit_Isactive == true ? "Active" : "DeActive";
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
        public ActionResult Index(int id = 0)
        {
            CAT_CONTRACTOR catContractor = new CAT_CONTRACTOR();
            if (id != 0)
            {
                catContractor = _repoContractor.GetContractorById(id);
            }
            ViewBag.id = id;
            ViewBag.mModel = catContractor;
            return View(catContractor);
        }
        public ActionResult Modify(int? id)
        {
            var contractor = new ContractorModels();

            if (id.HasValue)
            {
                var entity = _repoContractor.GetContractorById(id);
                contractor.Id = entity.id;
                contractor.Code = entity.str_Code;
                contractor.SortName = entity.str_Sortname;
                contractor.FullName = entity.str_Fullname;
                contractor.Address = entity.str_address;
                contractor.Description = entity.str_Description;
                contractor.SerialNumberTax = entity.str_masothue;
            }

            return View(contractor);

        }

        [HttpPost]
        public ActionResult Modify(ContractorModels model)
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
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    return Json(new AjaxResponseViewModel { result = false, message = codeHasSpaceMessage }, JsonRequestBehavior.AllowGet);
                }
                var entity = _repoContractor.GetContractorById(model.Id);
                if (entity == null)
                {
                    var test = _repoContractor.GetContractor(a => a.str_Code.ToLower() == model.Code.ToLower());
                    var xx = new List<string>();
                    foreach (var item in test)
                    {
                        xx.Add(item.str_Code);
                    }
                    if (_repoContractor.GetContractor(a => a.str_Code.ToLower() == model.Code.ToLower()).Any())
                    {

                        return Json(new AjaxResponseViewModel { result = false, message = string.Format(Messege.EXISTING_CODE, model.Code) }, JsonRequestBehavior.AllowGet);

                    }
                    _repoContractor.InsertContractor(model);
                }
                else
                {
                    if (_repoContractor.GetContractor(a => a.str_Code.ToLower() == model.Code.ToLower() && a.id != model.Id).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = string.Format(Messege.EXISTING_CODE, model.Code) }, JsonRequestBehavior.AllowGet);
                    }
                    _repoContractor.UpdateContractor(model);
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

        public ActionResult Editpage(int id = 0)
        {
            CAT_CONTRACTOR catContractor = new CAT_CONTRACTOR();
            if (id != 0)
            {
                catContractor = _repoContractor.GetContractorById(id);
            }
            ViewBag.id = id;
            ViewBag.mModel = catContractor;
            return View(catContractor);
        }
        public ActionResult Detailpage(int id = 0)
        {
            CAT_CONTRACTOR catContractor = new CAT_CONTRACTOR();
            if (id != 0)
            {
                catContractor = _repoContractor.GetContractorById(id);
            }
            ViewBag.id = id;
            ViewBag.mModel = catContractor;
            return View(catContractor);
        }
        public ActionResult Createpage(int id = 0)
        {
            CAT_CONTRACTOR catContractor = new CAT_CONTRACTOR();
            if (id != 0)
            {
                catContractor = _repoContractor.GetContractorById(id);
            }
            ViewBag.id = id;
            ViewBag.mModel = catContractor;
            return View(catContractor);
        }

        // fill data to datatable by ajax 

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().Trim();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().Trim();
                var sortname = string.IsNullOrEmpty(Request.QueryString["sortname"]) ? string.Empty : Request.QueryString["sortname"].ToLower().Trim();
                var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();

                var data = _repoContractor.GetContractor(a =>
               (string.IsNullOrEmpty(strCode) || a.str_Code.Contains(strCode)) &&
               (string.IsNullOrEmpty(sortname) || a.str_Sortname.Contains(sortname)) &&
               (string.IsNullOrEmpty(strName) || a.str_Fullname.Contains(strName))).OrderByDescending(a => a.dtm_Created_date);

                IEnumerable<CAT_CONTRACTOR> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CAT_CONTRACTOR, object> orderingFunction = (c => sortColumnIndex == 1 ? c.str_Code
                                                          : sortColumnIndex == 2 ? c.str_Sortname
                                                          : sortColumnIndex == 3 ? c.str_Fullname
                                                          : sortColumnIndex == 4 ? (object)c.dtm_Created_date
                                                          : c.str_Fullname);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,
                          c.str_Code,
                          c.str_Sortname,
                          c?.str_Fullname,
                          c?.str_address,
                          c?.str_masothue,
                          c?.str_Description,
                         //c?.dtm_Created_date.ToString(),
                         //c?.dtm_Updated_date.ToString(),
                          c.bit_Isactive == false ?
                          "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Contractor(0,"+c.id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" :
                          "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Contractor(1,"+c.id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                          //((Is_View(url)) ? "<a title='View' href='javascript:void(0)' onclick='active("+c.id+",\"view\")'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i></a>&nbsp;" : "") +
                        ((Is_Edit(url)) ?
                        "<a   title='Edit'  href='"+@Url.Action("Modify",new{id = c.id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "") +
                        ((Is_Delete(url)) ?
                        verticalBar +"<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contractor/AjaxHandler", ex.Message);
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

        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = _repoContractor.GetContractorById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }


                model.isDelete = true;
                model.bit_Isactive = false;
                model.dtm_Updated_date = DateTime.Now;
                model.int_Updated_by = CurrentUser.USER_ID;
                _repoContractor.UpdateContractor(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.str_Fullname),
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
        public ActionResult SubmitSetParticipateContractor(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idContractor = int.Parse(id);
                var removeContractor = _repoContractor.GetContractorById(idContractor);
                if (removeContractor == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contact/SubmitSetParticipateContractor", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeContractor.bit_Isactive = false;
                }
                else
                {
                    removeContractor.bit_Isactive = true;
                }
                _repoContractor.UpdateContractor(removeContractor);


                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeContractor.str_Fullname), result = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Contractor/SubmitSetParticipateContractor", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }


        }
        [AllowAnonymous]
        public ActionResult CreateCode(string valuetype = "")
        {
            #region[]
            string str_start = valuetype;
            #endregion
            string EID = "";
            var data = _repoContractor.GetContractorAll(a => a.str_Code.StartsWith(str_start));
            for (int i = data.Count() + 1; ; i++)
            {
                if(i > 0 && i <= 9)
                {
                    EID = str_start + "- 0" + i.ToString();
                }
                if (i > 9)
                {
                    EID = str_start + "-" + i.ToString();
                }
                

                var data_ = _repoContractor.GetContractorAll(a => a.str_Code == EID);
                if (data_.Count() == 0)
                {
                    break;
                }
            }

            return Json(EID);
        }

    }
}
