
using DAL.Entities;
using DAL.Repositories;
using DAL.UnitOfWork;
using Quartz;
using System.Linq;
using System.Collections.Generic;
using TMS.Core.Utils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Helpers;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Notifications;
using TMS.Core.ViewModels.Common;
using System.Threading.Tasks;
using TMS.Core.ViewModels;
using Newtonsoft.Json;
using System.Web;
using RestSharp;
using TMS.API.Utilities;
using static TMS.API.Utilities.Command;

namespace TMS.API.Scheduler
{
    public static class ScheduleMethod
    {
        private static readonly INotificationService NotificationService;
        public static void Execute(Schedules_Type type)
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    var repoCatmail = uow.Repository<CAT_MAIL>();
                    var repoCourse = uow.Repository<Course>();
                    var repoCourse_detail = uow.Repository<Course_Detail>();
                    var repoSchedulesMethod = uow.Repository<TMS_SentEmail>();
                    #region Thu gon

                    var schedule = type.Schedule;
                    foreach (var item in schedule.Schedules_Method)
                    {
                        if (item.IdMethod is (int)UtilConstants.ScheduleMethod.Mail)
                        {
                            var destination = schedule.Schedules_destination;
                            var index = 0;
                            while (index < destination.Count + 10)
                            {
                                foreach (var itemMail in destination.Skip(index).Take(10))
                                {

                                    if (schedule.IdTemplateMail == null) // nếu không có template là dạng gửi content manual
                                    {
                                        var entity = new TMS_SentEmail()
                                        {
                                            mail_receiver =
                                                itemMail.IsUser == true ? itemMail.USER.EMAIL : itemMail.Trainee.str_Email,
                                            content_body = schedule.Content,
                                            subjectname = schedule.Name,
                                            flag_sent = 0,
                                            Is_Deleted = false,
                                            Is_Active = true
                                        };
                                        repoSchedulesMethod.Insert(entity);
                                        uow.SaveChanges();
                                    }
                                    else
                                    {

                                        if (schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailScheduleCourseMissing.ToString())
                                        {
                                            #region[Gửi mail các khóa còn thiếu]

                                            var repotraineeHistoryService = uow.Repository<TraineeHistory>();
                                            var traineeHistories =
                                                repotraineeHistoryService.GetAll(a => a.Trainee_Id == itemMail.IdEmp).OrderByDescending(b => b.Id).FirstOrDefault()?.TraineeHistory_Item.OrderBy(a => a.SubjectDetail.Code);
                                            var missing = traineeHistories.Where(a =>
                                                a.Status == (int)UtilConstants.StatusTraineeHistory.Missing);
                                            if (missing.Any())
                                            {
                                                var entitySchedulesMethod = new TMS_SentEmail()
                                                {
                                                    mail_receiver =
                                                        itemMail.IsUser == true ? itemMail.USER.EMAIL : itemMail.Trainee.str_Email,
                                                    content_body = BodySendMail(repoCatmail, UtilConstants.TypeSentEmail.SendMailScheduleCourseMissing, itemMail.Trainee, missing),
                                                    subjectname = schedule.Name,
                                                    flag_sent = 0,
                                                    Is_Deleted = false,
                                                    Is_Active = true
                                                };
                                                repoSchedulesMethod.Insert(entitySchedulesMethod);
                                                uow.SaveChanges();
                                            }
                                            #endregion
                                        }
                                        else if (schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailScheduleReportTrainingPlan.ToString())
                                        {
                                            #region[Gửi mail báo cáo trainingplan]
                                            var entitySchedulesMethod = new TMS_SentEmail()
                                            {
                                                mail_receiver =
                                                    itemMail.IsUser == true ? itemMail.USER.EMAIL : itemMail.Trainee.str_Email,
                                                content_body = BodySendMail(repoCatmail, UtilConstants.TypeSentEmail.SendMailScheduleReportTrainingPlan, itemMail.Trainee, null),
                                                subjectname = schedule.Name,
                                                flag_sent = 0,
                                                Is_Deleted = false,
                                                Is_Active = true,
                                                AttachFileType = (int)UtilConstants.TypeSentEmailAttachFileType.ReportTrainingPlan,
                                                time_sent = DateTime.Now,
                                            };
                                            repoSchedulesMethod.Insert(entitySchedulesMethod);
                                            uow.SaveChanges();
                                            #endregion
                                        }
                                        else if (schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailReminderLogin.ToString())
                                        {

                                        }
                                    }
                                }
                                index += 10;
                            }
                            if (schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailReminderCourse.ToString())
                            {
                                double timeremind = Convert.ToDouble(type.TimeRemind);
                                var condition = DateTime.Now.Date.AddDays(timeremind).Date;
                                var courde_detail = repoCourse_detail.GetAll(a => a.dtm_time_to == condition && a.IsDeleted != true).ToList();
                                var mail_title = "Thư nhắc học / Completion Reminder";
                                var body = repoCatmail.Get(a => a.Code == UtilConstants.TypeSentEmail.SendMailReminderCourse.ToString());
                                var body_ = body?.Content;
                                var cat_mail_ID = body?.ID ?? -1;
                                foreach (var item_member in courde_detail)
                                {
                                    var trainee_assing = item_member.TMS_Course_Member.Where(a => a.Course_Details_Id == item_member.Id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
                                    foreach (var item_trainee in trainee_assing)
                                    {
                                        if (item_trainee?.Course_Detail.Course_Result.Any(a => a.CourseDetailId == item_member.Id &&  (string.IsNullOrEmpty(a.Re_Check_Result) ? (a.First_Check_Result != "F" && a.First_Check_Result != null) : a.Re_Check_Result != "F")) == true)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            var mail_receiver = item_trainee.Trainee.str_Email;
                                            var bodysendmail = BodySendMail_Custom_UAT(body_, null, item_member);
                                            if (!string.IsNullOrEmpty(mail_receiver) && cat_mail_ID != -1 && !string.IsNullOrEmpty(bodysendmail))
                                            {
                                                var entitySchedulesMethod = new TMS_SentEmail()
                                                {
                                                    mail_receiver = mail_receiver,
                                                    content_body = bodysendmail,
                                                    subjectname = mail_title,
                                                    flag_sent = 0,
                                                    Is_Deleted = false,
                                                    Is_Active = true,
                                                    cat_mail_ID = cat_mail_ID,
                                                };
                                                repoSchedulesMethod.Insert(entitySchedulesMethod);
                                            }
                                        }

                                    }
                                }
                                uow.SaveChanges();
                            }
                            if (schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailReminderFinalCourse.ToString())
                            {
                                double timeremind = Convert.ToDouble(type.TimeRemind);
                                var limit = DateTime.Now.Date.AddDays(-timeremind);
                                var courselist = repoCourse.GetAll(p => p.TMS_APPROVES.Any(a => a.Course.EndDate.Value.Date == limit && a.int_Type == (int)UtilConstants.ApproveType.CourseResult && a.int_id_status == (int)UtilConstants.EStatus.Approve) && p.IsDeleted == false && p.IsActive == true);
                                var body = repoCatmail.GetAll(a => a.Type == (int)UtilConstants.TypeSentEmail.SendMailReminderFinalCourse);
                                var body_ = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SendMailReminderFinalCourse)?.Content;
                                var cat_mail_ID = body.FirstOrDefault(a => a.Type == (int)UtilConstants.TypeSentEmail.SendMailReminderFinalCourse)?.ID;
                                foreach (var item_ in courselist)
                                {
                                    var list_detail = item_.Course_Detail.Where(a => a.IsDeleted != true && a.CourseId == item_.Id);
                                    foreach (var item__ in list_detail)
                                    {
                                        var list_ins = item__.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id == item__.Id);
                                        foreach (var item_ins in list_ins)
                                        {
                                            var bodysendmail = BodySendMail_Custom(body_, item_ins.Trainee, item_);
                                            var entitySchedulesMethod = new TMS_SentEmail()
                                            {
                                                mail_receiver = item_ins.Trainee.str_Email,
                                                content_body = bodysendmail,
                                                subjectname = schedule.Name,
                                                type_sent = (int)UtilConstants.TypeSentEmail.SendMailReminderFinalCourse,
                                                cat_mail_ID = cat_mail_ID,
                                                id_course = item_.Id,
                                                flag_sent = 0,
                                                Is_Deleted = false,
                                                Is_Active = true,
                                            };
                                            repoSchedulesMethod.Insert(entitySchedulesMethod);
                                            uow.SaveChanges();
                                        }
                                    }
                                }

                            }
                        }
                        else if (item.IdMethod is (int)UtilConstants.ScheduleMethod.Notification)
                        {
                            var destination = schedule.Schedules_destination;
                            foreach (var itemNotification in destination)
                            {
                                NotificationService.Notification_Insert((int)UtilConstants.NotificationType.Trainee, 0, 0,
                                    (itemNotification.IsUser == true
                                        ? itemNotification.USER.ID
                                        : itemNotification.Trainee.Id), DateTime.Now, schedule.Name, schedule.Content);
                            }
                        }
                        else if (item.IdMethod is (int)UtilConstants.ScheduleMethod.Sms)
                        {
                        }
                    }
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
               
            }

        }
        #region [ALL private function convert]
        private static string BodySendMail(IRepository<CAT_MAIL> CATMAIL, UtilConstants.TypeSentEmail cAT_MAIL, Trainee trainee = null, IEnumerable<TraineeHistory_Item> traineeHistoryItems = null, List<Course> ListCourse = null)
        {
            var body = CATMAIL.GetAll(a => a.Type == (int)cAT_MAIL).FirstOrDefault()?.Content;
            if (body != null)
            {
                if (trainee != null)
                {
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                        .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, trainee.FirstName + " " + trainee.LastName)
                        .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                        .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone);
                }
                if (cAT_MAIL != UtilConstants.TypeSentEmail.SendMailReminderCourse)
                {
                    if (traineeHistoryItems != null && traineeHistoryItems.Any())
                    {
                        var itemcourse = "";
                        foreach (var VARIABLE in traineeHistoryItems)
                        {
                            itemcourse += "-" + VARIABLE.SubjectDetail.Name + ". <br />";
                        }
                        body = body.Replace(UtilConstants.MAIL_LIST_COURSE, itemcourse);
                    }
                }
                else
                {
                    if (trainee != null && (ListCourse != null))
                    {
                        var html = "<ul>";
                        foreach (var course in ListCourse)
                        {
                            html += "<li>";
                            html += "<b>" + course.Name + "</b> <br>";
                            html += string.Join(",", course.Course_Detail.Select(p => p.SubjectDetail.Name).ToList());
                            html += "</li>";
                        }
                        html += "</ul>";
                        body = body.Replace(UtilConstants.MAIL_LIST_COURSE, html);
                    }
                }
            }
            return body;
        }
        private static string BodySendMail_Custom(string body, Trainee trainee = null, Course course = null)
        {
            if (!string.IsNullOrEmpty(body))
            {
               
                if (trainee != null)
                {
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, ReturnDisplayLanguage(trainee.FirstName, trainee.LastName))
                       .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                       .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone);                
                }
                if (course != null)
                {
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, course.Name)
                       .Replace(UtilConstants.MAIL_PROGRAM_CODE, course.Code)
                       .Replace(UtilConstants.MAIL_PROGRAM_STARTDATE, course.StartDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, course.EndDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_VENUE, course.Venue)
                       .Replace(UtilConstants.MAIL_PROGRAM_MAXTRAINEE, course.NumberOfTrainee.ToString())
                       .Replace(UtilConstants.MAIL_PROGRAM_NOTE, course.Note);

                    var html = "<ul>";
                    html += "<li>";
                    html += "<b>" + course.Name + "</b> <br>";
                    html += string.Join(",", course.Course_Detail.Select(p => p.SubjectDetail.Name).ToList());
                    html += "</li>";
                    html += "</ul>";
                    body = body.Replace(UtilConstants.MAIL_LIST_COURSE, html);
                }

            }
            return body;
        }
        private static string BodySendMail_Custom_UAT(string body, Trainee trainee = null, Course_Detail course = null)
        {
            if (!string.IsNullOrEmpty(body))
            {

                if (trainee != null)
                {
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, ReturnDisplayLanguage(trainee.FirstName, trainee.LastName));
                    
                }
                if (course != null)
                {
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, course.SubjectDetail.Name)
                     
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, course.dtm_time_to?.ToString("dd/MM/yyyy")).Replace("MAIL_COURSE_NAME", course.SubjectDetail.Name).Replace("MAIL_COURSE_ENDDATE", course.dtm_time_to?.ToString("dd/MM/yyyy"));
                }

            }
            return body;
        }
        public static string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            
            if (string.IsNullOrEmpty(lastName) || lastName.Equals(firstName))
            {
                fullName = firstName;
            }
            else
            {
                fullName = lastName + " " + firstName;
            }

            return fullName;
        }
        //public static byte[] ExportExcelCourse()
        //{
        //    byte[] Bytes = null;
        //    using (var dbContext = new EFDbContext())
        //    {
        //        var uow = new UnitOfWork(dbContext);
        //        var courseService = uow.Repository<Course>();
        //        var repoCourseServiceDetail = uow.Repository<Course_Detail_Instructor>();
        //        String templateFilePath = HostingEnvironment.MapPath(@"/Template/ExcelFile/TraingPlan.xlsx");
        //        var template = new FileInfo(templateFilePath);


        //        var data = courseService.GetAll(a => a.IsDeleted == false);

        //        ExcelPackage xlPackage;
        //        var MS = new MemoryStream();

        //        using (xlPackage = new ExcelPackage(template, false))
        //        {
        //            var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
        //            const int startrow = 8;
        //            var groupHeader = 0;
        //            var grouptotal = 0;
        //            var row = 0;
        //            var header = "FORM:VJC-VTC-F-001,ISS01/REV,01/01/2018".Split(new char[] { ',' });

        //            var cellHeader = worksheet.Cells[1, 10];
        //            cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
        //            cellHeader.Style.Font.Size = 11;

        //            foreach (var item in data)
        //            {
        //                int totalHours = 0;
        //                double totalDays = 0;
        //                const int col = 1;
        //                int count = 0;
        //                worksheet.Cells[
        //                    startrow + row + groupHeader + grouptotal, 1, startrow + row + groupHeader + grouptotal, 10]
        //                    .Merge = true;
        //                ExcelRange cell = worksheet.Cells[startrow + row + groupHeader + grouptotal, 1];
        //                cell.Value = item.Code.ToUpper() + " - " + item.Name.ToUpper();
        //                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
        //                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        //                cell.Style.Font.Bold = true;
        //                cell.Style.Font.Size = 14;
        //                foreach (
        //                    var item1 in
        //                        item.Course_Detail.Where(a => a.IsActive == true && a.IsDeleted == false)
        //                            .OrderBy(a => a.dtm_time_from))
        //                {
        //                    if (item1 != null)
        //                    {
        //                        count++;
        //                        string nameinstructor = "";
        //                        totalHours = totalHours + ((item1.Duration != null) ? (int)item1.Duration : 0);
        //                        totalDays = totalDays + ((item1.Duration != null) ? ((float)item1.Duration) / (float)8 : 0);
        //                        string date = item1.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "" + " - " +
        //                                      item1.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? "";

        //                        var instructor =
        //                            repoCourseServiceDetail.GetAll(a => a.Course_Detail_Id == item1.Id && a.Type == (int)UtilConstants.TypeInstructor.Instructor);
        //                        if (instructor.Any())
        //                        {
        //                            foreach (var item2 in instructor)
        //                            {
        //                                // nameinstructor += item2?.Trainee?.FirstName + " " + item2?.Trainee?.LastName + Environment.NewLine;
        //                                nameinstructor += item2?.Trainee?.FirstName + " " + item2?.Trainee?.LastName + Environment.NewLine;
        //                            }
        //                        }

        //                        ExcelRange cellNo = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col];
        //                        cellNo.Value = count;
        //                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellCourse =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 1];
        //                        cellCourse.Value = item1?.SubjectDetail?.Name;
        //                        cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellCourse.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellMethod =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 2];
        //                        cellMethod.Value = TypeLearningName(item1.type_leaning.Value);
        //                        cellMethod.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellMethod.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellMethod.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellHours =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 3];
        //                        cellHours.Value = item1.SubjectDetail?.Duration;
        //                        cellHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellDays =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 4];
        //                        cellDays.Value = ((float)item1.SubjectDetail?.Duration / (float)8);
        //                        cellDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellDays.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellDate =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 5];
        //                        cellDate.Value = date;
        //                        cellDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellInstructor =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 6];
        //                        cellInstructor.Value = nameinstructor; // item1?.Trainee?.str_Fullname;
        //                        cellInstructor.Style.WrapText = true;
        //                        cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellInstructor.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellNum = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 7];
        //                        cellNum.Value = item1?.Course?.NumberOfTrainee;
        //                        cellNum.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellNum.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellNum.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellVenue =
        //                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 8];
        //                        cellVenue.Value = item1.Course?.Venue ?? string.Empty;
        //                        cellVenue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellVenue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellVenue.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                        ExcelRange cellRemark =
        //                             worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 9];
        //                        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                    }
        //                    groupHeader++;
        //                }
        //                row++;
        //                ExcelRange cellTotal = worksheet.Cells[startrow + row + groupHeader + grouptotal, 1, startrow + row + groupHeader + grouptotal, 3];
        //                cellTotal.Style.Font.Bold = true;
        //                cellTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                cellTotal.Value = "TOTAL:";
        //                cellTotal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        //                cellTotal.Merge = true;

        //                ExcelRange cellTotalHours = worksheet.Cells[startrow + row + groupHeader + grouptotal, 4];
        //                cellTotalHours.Value = totalHours;
        //                cellTotalHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                cellTotalHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                cellTotalHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                ExcelRange cellTotalDays = worksheet.Cells[startrow + row + groupHeader + grouptotal, 5];
        //                cellTotalDays.Value = totalDays;
        //                cellTotalDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                cellTotalDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                cellTotalDays.Style.Border.BorderAround(ExcelBorderStyle.Thin);


        //                ExcelRange cellMerge = worksheet.Cells[startrow + row + groupHeader + grouptotal, 6, startrow + row + groupHeader + grouptotal, 10];
        //                cellMerge.Merge = true;
        //                cellMerge.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        //                grouptotal++;

        //            }
        //            Bytes = xlPackage.GetAsByteArray();

        //        }
        //    }
        //    return Bytes;
        //}
        private static string TypeLearningName(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "Classroom";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "Online";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "Blended";
                    break;
            }
            return str_type;
        }

        public static string ReplaceChar(string result)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result.Replace(" ", string.Empty);
                result.Replace(Environment.NewLine, string.Empty);
                result.Replace("\\t", string.Empty);
                result.Replace(";", string.Empty);
                result.Replace(",", ".");
                return result;
            }
            return string.Empty;
        }
        #endregion
    }
    [DisallowConcurrentExecution]
    public class Schedules : IJob
    {
        //private static readonly RestRequest RequestWsLms = new RestRequest(Method.POST);
        public void Execute(IJobExecutionContext context)
        {
            //schedule
            var thTypeRepeat = new Thread(TypeRepeat);
            //var thTypeSetCalendar = new Thread(TypeSetCalendar);
            //var thTypePeriodic = new Thread(TypePeriodic);
            //
            //var thTraineeHistoryInsert = new Thread(TraineeHistoryInsert);
            //var thTraineeHistoryUpdate = new Thread(TraineeHistoryUpdate);
            //var thTraineeFuture = new Thread(TraineeFuture);
            //var thTraineeImport = new Thread(TraineeImport);
            // var thSendMailReminder = new Thread(SendMailReminderLogin);
            // var thGroupCertificate = new Thread(GroupCertificate);
            //thGroupCertificate.Start();

            //schedule
            thTypeRepeat.Start();
            //thTypeSetCalendar.Start();
            //thTypePeriodic.Start();
            //thTraineeImport.Start();
            //
            //thTraineeHistoryInsert.Start(); // khong xai
            //thTraineeHistoryUpdate.Start();//
            //thTraineeFuture.Start();//
            //thSendMailReminder.Start();
        }
        private static void TypeRepeat()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    #region Thu gon
                    var repoSchedulesMethod = uow.Repository<Schedules_Type>();
                    var entity = repoSchedulesMethod.GetAll(a => a.Schedule.IsDelete == false && a.Schedule.IsActive == true && a.IdType == (int)UtilConstants.ScheduleType.Repeat).ToList();
                    if (entity.Count() == 0)
                    {
                        uow.Dispose();
                        return;
                    }
                    foreach (var item in entity)
                    {
                        if (item.LastAccess == null || item.Value == null) continue;
                        var date = item.LastAccess.Value.AddSeconds(int.Parse(item.Value));
                        if (date >= DateTime.Now) continue;

                        #region [Thao tác khi đủ điều kiện gửi]
                        ScheduleMethod.Execute(item);
                        #endregion
                        item.LastAccess = DateTime.Now;
                        repoSchedulesMethod.Update(item);
                    }
                    uow.SaveChanges();
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }               
            }
        }
        private static void TypeSetCalendar()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                #region Thu gon
                var repoSchedulesMethod = uow.Repository<Schedules_Type>();
                var entity = repoSchedulesMethod.GetAll(a => a.Schedule.IsDelete == false && a.Schedule.IsActive == true && a.IdType == (int)UtilConstants.ScheduleType.SetCalendar);
                if (!entity.Any())
                {
                    uow.Dispose();
                    return;
                }
                foreach (var item in entity)
                {
                    if (item.IsDone == true) continue;
                    if (item.Value == null) continue;

                    var valueNow = DateTime.Now;

                    var valueDB = DateTime.Parse(item.Value);

                    var valueBefor =
                        valueDB.AddSeconds(-int.Parse(ConfigurationManager.AppSettings["ScheduleSecond"]));

                    var valueAfter =
                        valueDB.AddSeconds(int.Parse(ConfigurationManager.AppSettings["ScheduleSecond"]));

                    if (valueBefor < valueNow && valueNow < valueAfter)
                    {
                        #region [Thao tác khi đủ điều kiện gửi]
                        ScheduleMethod.Execute(item);
                        #endregion
                        item.IsDone = true;
                        repoSchedulesMethod.Update(item);
                    }

                }
                uow.SaveChanges();
                #endregion
                uow.Dispose();
            }
        }
        private static void TypePeriodic()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                #region Thu gon
                var repoSchedulesMethod = uow.Repository<Schedules_Type>();
                var entity = repoSchedulesMethod.GetAll(a => a.Schedule.IsDelete == false && a.Schedule.IsActive == true && a.IdType == (int)UtilConstants.ScheduleType.Periodic);
                if (!entity.Any())
                {
                    uow.Dispose();
                    return;
                }
                foreach (var item in entity)
                {
                    if (item.Value == null) continue;

                    var value = item.Value.Split(new char[] { '|' });
                    var dayOfWeek = value[0];
                    var timeOfDay = value[1];
                    if (dayOfWeek == null || !dayOfWeek.Split(new char[] { ',' }).Any()) continue;
                    if (timeOfDay == null || !timeOfDay.Split(new char[] { ':' }).Any()) continue;
                    foreach (var itemDayOfWeek in dayOfWeek.Split(new char[] { ',' }))
                    {
                        if ((int)DateTime.Now.DayOfWeek != int.Parse(itemDayOfWeek)) continue;
                        var gio = int.Parse(timeOfDay.Split(new char[] { ':' })[0]);
                        var phut = int.Parse(timeOfDay.Split(new char[] { ':' })[1]);
                        var time = new TimeSpan(gio, phut, 0);

                        var valueNow = DateTime.Now;

                        var valueDB = DateTime.Now.Date + time;

                        var valueBefor =
                            valueDB.AddSeconds(-int.Parse(ConfigurationManager.AppSettings["ScheduleSecond"]));

                        var valueAfter =
                            valueDB.AddSeconds(int.Parse(ConfigurationManager.AppSettings["ScheduleSecond"]));

                        if (valueBefor < valueNow && valueNow < valueAfter)
                        {
                            #region [Thao tác khi đủ điều kiện gửi]
                            ScheduleMethod.Execute(item);
                            #endregion
                        }

                    }

                }
                #endregion
                uow.Dispose();
            }
        }
        private static void TraineeHistoryInsert()
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork uow = new UnitOfWork(DbContext);
                #region [ /* --------------------Schedule TraineeHistory --------------------- */]


                var repoCourseResultFinal = uow.Repository<Course_Result_Final>();
                var repoCourseDetail = uow.Repository<Course_Detail>();

                var repoTraineeHistory = uow.Repository<TraineeHistory>();
                var traineeHistories = repoTraineeHistory.GetAll(a => (a.Status == null || a.Status == (int)UtilConstants.StatusScheduler.Modify)).OrderByDescending(a => a.Id).Take(5).ToList();
                if (!traineeHistories.Any())
                {
                    uow.Dispose();
                    return;
                }
                {
                    foreach (var traineeHistory in traineeHistories)
                    {
                        var titleStandard = traineeHistory.JobTitle.Title_Standard.ToArray();

                        if (titleStandard.Any())
                        {
                            var subjectIds = titleStandard.Select(a => a.Subject_Id).ToArray();
                            var getCourseid = repoCourseResultFinal.GetAll(a => a.traineeid == traineeHistory.Trainee_Id && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid).ToArray();
                            var courseidAssign = repoCourseDetail.GetAll(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct().ToList();

                            foreach (var subjectId in subjectIds)
                            {
                                traineeHistory.TraineeHistory_Item.Add(new TraineeHistory_Item()
                                {
                                    SubjectId = subjectId,
                                    Status = courseidAssign.Contains(subjectId) ? (int)UtilConstants.StatusTraineeHistory.Trainning : (int)UtilConstants.StatusTraineeHistory.Missing
                                });
                            }
                        }
                        traineeHistory.Status = (int)UtilConstants.StatusScheduler.Synchronize;
                        repoTraineeHistory.Update(traineeHistory);
                    }
                    uow.SaveChanges();
                }

                #endregion
                uow.Dispose();
            }
        }
        private static void TraineeHistoryUpdate()
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork _uow = new UnitOfWork(DbContext);
                #region [ /* --------------------Schedule TraineeHistory --------------------- */]
                var repoCourseResultFinal = _uow.Repository<Course_Result_Final>();
                var repoCourseDetail = _uow.Repository<Course_Detail>();
                var repoTmsApprove = _uow.Repository<TMS_APPROVES>();

                var repoTraineeHistory = _uow.Repository<TraineeHistory>();
                var traineeHistories = repoTraineeHistory.GetAll(a => (a.Status == null || a.Status == (int)UtilConstants.StatusScheduler.Modify)).OrderByDescending(a => a.Id).Take(5);
                if (traineeHistories.Any())
                {
                    foreach (var traineeHistory in traineeHistories)
                    {
                        var titleStandard = traineeHistory.JobTitle.Title_Standard;

                        if (titleStandard.Any())
                        {
                            var subjectIds = titleStandard.Select(a => a.Subject_Id);
                            var getCourseid = repoCourseResultFinal.GetAll(a => a.traineeid == traineeHistory.Trainee_Id && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid);
                            var courseidAssign = repoCourseDetail.GetAll(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct();
                            var courseDetailCompleted = repoTmsApprove.GetAll(a => getCourseid.Contains(a.int_Course_id) && a.int_Type == (int)UtilConstants.ApproveType.CourseResult && a.int_id_status == (int)UtilConstants.EStatus.Approve).Select(a => a.Course.Course_Detail);

                            var subjectIdCompleted = new int[] { };

                            if (courseDetailCompleted.Any())
                            {
                                foreach (var item in courseDetailCompleted)
                                {
                                    subjectIdCompleted = item.Select(a => (int)a.SubjectDetailId).ToArray();
                                }
                            }
                            foreach (var subjectId in subjectIds)
                            {
                                var traineeHistoryItem = new TraineeHistory_Item();
                                traineeHistoryItem.SubjectId = subjectId;
                                traineeHistoryItem.TraineeHistoryId = traineeHistory.Id;

                                if (subjectIdCompleted.Length != 0)
                                {
                                    traineeHistoryItem.Status = (int)UtilConstants.StatusTraineeHistory.Completed;
                                }
                                else
                                {
                                    traineeHistoryItem.Status = courseidAssign.Contains(subjectId)
                                        ? (int)UtilConstants.StatusTraineeHistory.Trainning
                                        : (int)UtilConstants.StatusTraineeHistory.Missing;
                                }
                                traineeHistory.TraineeHistory_Item.Add(traineeHistoryItem);

                            }
                            traineeHistory.Status = (int)UtilConstants.StatusScheduler.Synchronize;
                            traineeHistory.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                            repoTraineeHistory.Update(traineeHistory);
                        }

                    }
                    _uow.SaveChanges();
                }
                #endregion
                _uow.Dispose();
            }
        }
        private static void TraineeFuture()
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork _uow = new UnitOfWork(DbContext);
                #region [ /* --------------------Schedule TraineeFuture --------------------- */]

                var repoTraineeFuture = _uow.Repository<TraineeFuture>();
                var traineeFutures = repoTraineeFuture.GetAll(a => (a.Schedule == (int)UtilConstants.StatusScheduler.Modify)).Take(5).ToList();
                if (traineeFutures.Any())
                {
                    foreach (var traineeFuture in traineeFutures)
                    {
                        var jobStandard = traineeFuture.JobTitle.Title_Standard;

                        if (jobStandard.Any())
                        {
                            var subjectIds = jobStandard.Select(a => a.Subject_Id).ToArray();

                            foreach (var subjectId in subjectIds)
                            {
                                traineeFuture.TraineeFuture_Item.Add(new TraineeFuture_Item()
                                {
                                    SubjectId = subjectId,
                                });
                            }
                        }
                        traineeFuture.Schedule = (int)UtilConstants.StatusScheduler.Synchronize;
                        traineeFuture.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                        repoTraineeFuture.Update(traineeFuture);
                    }
                    _uow.SaveChanges();
                }
                #endregion
                _uow.Dispose();


            }
        }
        private static void SendMailReminderLogin()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                #region Thu gon
                var repoconfig = uow.Repository<CONFIG>();
                var repoTmsSendmail = uow.Repository<TMS_SentEmail>();
                var entitySendmail =
                    repoTmsSendmail.GetAll(a => a.type_sent == (int)UtilConstants.TypeSentEmail.SendMailReminderLogin).Select(a => a.mail_receiver);
                var entity = repoconfig.GetAll(a => a.KEY.Equals("DataLmsUser")).FirstOrDefault();
                if (entity == null)
                {
                    uow.Dispose();
                    return;
                }
                var data = JObject.Parse(entity.VALUE);
                var date = (int)(DateTime.UtcNow.AddDays(-3).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var postTitles =
                    from p in data["users"]
                    where (int)p["lastaccess"] < date && !entitySendmail.Contains(p["email"].ToString())
                    select (string)p["email"];
                var insertTmsSentEmail = new TMS_SentEmail();
                foreach (var item in postTitles)
                {
                    insertTmsSentEmail.mail_receiver = item;
                    insertTmsSentEmail.type_sent = (int)UtilConstants.TypeSentEmail.SendMailReminderLogin;
                    insertTmsSentEmail.flag_sent = 0;
                    insertTmsSentEmail.subjectname = "Reminder Login";
                    insertTmsSentEmail.content_body = "Reminder Login";
                    insertTmsSentEmail.Is_Active = true;
                    insertTmsSentEmail.Is_Deleted = false;
                    repoTmsSendmail.Insert(insertTmsSentEmail);
                }
                uow.SaveChanges();
                #endregion
                uow.Dispose();
            }
        }

        private static void GroupCertificate()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                #region Thu gon
                var repoGroupCertificate = uow.Repository<Group_Certificate>();
                var repoGroupCertificateSchedule = uow.Repository<Group_Certificate_Schedule>();
                var repoGroup_Certificate_Schedule_Detail = uow.Repository<Group_Certificate_Schedule_Detail>();
                var repoTrainee = uow.Repository<Trainee>();

                var groupCertificates =
                    repoGroupCertificate.GetAll(a =>
                       a.IsDeleted == false &&
                       a.IsActive == true &&
                       (a.Status == null || a.Status == (int)UtilConstants.StatusScheduler.Modify));

                if (groupCertificates.Any())
                {
                    var dataTrainee = repoTrainee.GetAll(a => a.IsDeleted == false);
                    foreach (var group in groupCertificates)
                    {
                        if (group.Group_Certificate_Subjects.Any())
                        {
                            if (dataTrainee.Any())
                            {
                                foreach (var trainee in dataTrainee)
                                {

                                    var idSubjectCompleted =
                                        trainee.Course_Result.Where(
                                            a =>
                                                a.Course_Detail.TMS_APPROVES.Any(
                                                    b =>
                                                        b.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                        b.int_id_status == (int)UtilConstants.EStatus.Approve))
                                            .Select(c => c.Course_Detail.SubjectDetailId);

                                    var entity =
                                        group.Group_Certificate_Schedule.FirstOrDefault(a => a.IdTrainee == trainee.Id);

                                    if (entity != null)
                                    {
                                        entity.ModifiedDate = DateTime.Now;
                                        entity.ModifiedBy = group.CreateBy;
                                        repoGroup_Certificate_Schedule_Detail.Delete(entity.Group_Certificate_Schedule_Detail);
                                        entity.Group_Certificate_Schedule_Detail.Clear();
                                        repoGroupCertificateSchedule.Update(entity);
                                    }
                                    else
                                    {
                                        entity = new Group_Certificate_Schedule();
                                        entity.IdGroupCertificate = group.Id;
                                        entity.IdTrainee = trainee.Id;
                                        entity.CreateDate = DateTime.Now;
                                        entity.CreateBy = group.CreateBy;
                                        entity.IsActive = true;
                                        entity.IsDeleted = false;
                                        repoGroupCertificateSchedule.Insert(entity);
                                    }
                                    foreach (var subject in group.Group_Certificate_Subjects)
                                    {
                                        if (idSubjectCompleted.Contains(subject.IdSubject.Value))
                                        {
                                            entity.Group_Certificate_Schedule_Detail.Add(new Group_Certificate_Schedule_Detail
                                                ()
                                            {
                                                IdSubject = subject.IdSubject.Value,
                                                Type = (int)UtilConstants.StatusTraineeHistory.Completed
                                            });
                                        }
                                        else
                                        {
                                            entity.Group_Certificate_Schedule_Detail.Add(new Group_Certificate_Schedule_Detail()
                                            {
                                                IdSubject = subject.IdSubject.Value,
                                                Type = (int)UtilConstants.StatusTraineeHistory.Missing
                                            });
                                        }

                                    }

                                }

                            }
                            group.Status = (int)UtilConstants.StatusScheduler.Synchronize;
                            repoGroupCertificate.Update(group);
                        }

                    }
                    uow.SaveChanges();
                }
                #endregion
                uow.Dispose();
            }



        }
        private static void TraineeImport()
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork uow = new UnitOfWork(DbContext);
                #region [ /* --------------------Schedule TraineeHistory --------------------- */]


                var repoTraineeImport = uow.Repository<Trainee>();

                var traineeImport = repoTraineeImport.GetAll(a => (a.LmsStatus == null || a.LmsStatus == (int)UtilConstants.StatusScheduler.Modify)).Take(10).ToList();
                if (!traineeImport.Any())
                {
                    uow.Dispose();
                    return;
                }
                {
                    #region [--------CALL LMS (CRON USER)----------]
                    var callLms = CallServices(UtilConstants.CRON_USER);
                    #endregion
                }
                #endregion
                uow.Dispose();
            }
        }
        #region [All Function]
        private static bool CallServices(string type)
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork uow = new UnitOfWork(DbContext);
                #region Thu gon
                var repoConfig = uow.Repository<CONFIG>();
                var server = repoConfig.Get(a => a.KEY == "SERVER")?.VALUE;
                var token = repoConfig.Get(a => a.KEY == "TOKEN")?.VALUE;
                var function = repoConfig.Get(a => a.KEY == "FUNCTION")?.VALUE;
                var moodlewsrestformat = repoConfig.Get(a => a.KEY == "moodlewsrestformat")?.VALUE;
                var restClient = new RestClient(server);
                var request = new RestRequest(Method.POST);
                request.AddParameter("wstoken", token);
                request.AddParameter("wsfunction", function);
                request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                request.AddParameter("type", type);
                var response = restClient.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    uow.Dispose();
                    return false;
                }

                var responseContent = response.Content;
                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    uow.Dispose();
                    return true;
                }
                uow.Dispose();
                return false;
                #endregion
            }

        }

        #endregion

    }
    public class Schedules_FinalCourse : IJob
    {
        //private static readonly RestRequest RequestWsLms = new RestRequest(Method.POST);
        public void Execute(IJobExecutionContext context)
        {
            //schedule
            var thTypeRepeat = new Thread(TypeRepeat);          
            thTypeRepeat.Start();
        }
        private static void TypeRepeat()
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    #region Thu gon
                    var repoSchedulesMethod = uow.Repository<Schedules_Type>();
                    var entity = repoSchedulesMethod.GetAll(a => a.Schedule.IsDelete == false && a.Schedule.IsActive == true && a.IdType == (int)UtilConstants.ScheduleType.Repeat && a.Schedule.CAT_MAIL.Code == UtilConstants.TypeSentEmail.SendMailScheduleCourseMissing.ToString());
                    if (!entity.Any())
                    {
                        uow.Dispose();
                        return;
                    }
                    foreach (var item in entity)
                    {
                        if (item.LastAccess == null || item.Value == null) continue;
                        var date = item.LastAccess.Value.AddSeconds(int.Parse(item.Value));
                        if (date >= DateTime.Now) continue;

                        #region [Thao tác khi đủ điều kiện gửi]
                        ScheduleMethod.Execute(item);
                        #endregion
                        item.LastAccess = DateTime.Now;
                        repoSchedulesMethod.Update(item);
                    }
                    uow.SaveChanges();
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
              
            }
        }
    }
    [DisallowConcurrentExecution]
    public class IDGJob : IJob
    {
        public IDGJob()
        {
        }
        public void Execute(IJobExecutionContext context)
        {
           
            using (var dbContext = new EFDbContext())
            {
                var mail_receiver = "";
                var id_mail = 0;
                var uow = new UnitOfWork(dbContext);
                #region Thu gon
                var repoTmsSentEmail = uow.Repository<TMS_SentEmail>();
                try
                {
                    var entitySentMail = repoTmsSentEmail.GetAll(a => a.flag_sent == 0 && a.Is_Deleted != true && a.Is_Active == true).Take(20).ToArray();
                    if (entitySentMail.Length > 0)
                    {
                        foreach (var item1 in entitySentMail)
                        {
                            item1.flag_sent = 3;
                            repoTmsSentEmail.Update(item1);
                        }
                        uow.SaveChanges();

                        foreach (var item in entitySentMail)
                        {
                            Task.Delay(1000);
                            mail_receiver = item.mail_receiver;
                            id_mail = item.Id;
                            var mailTo = ScheduleMethod.ReplaceChar(item.mail_receiver);
                            if (MailUtil.SendMail(mailTo, item.subjectname ?? item.CAT_MAIL?.SubjectMail, item.content_body))
                            {
                                item.flag_sent = 1;
                            }
                            else
                            {
                                item.flag_sent = 2;
                            }
                            item.time_sent = DateTime.Now;
                            repoTmsSentEmail.Update(item);
                        }
                        uow.SaveChanges();
                    }
                    #endregion
                    uow.Dispose();
                }
                catch (Exception ex)
                {
                    if (id_mail > 0)
                    {
                        var mail_error = repoTmsSentEmail.Get(a => a.Id == id_mail);
                        if (mail_error != null)
                        {
                            mail_error.LogError = ex.ToString();
                            uow.SaveChanges();
                        }
                    }
                    uow.Dispose();
                    #region[testfilelog]
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine("Time: " + DateTime.Now);
                    sb.AppendLine("Mail: " + mail_receiver);
                    sb.AppendLine("ErrorDetail: " + ex.ToString());
                    Console.WriteLine(sb.ToString());
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "CheckLogSendMail.txt"),
                        sb.ToString());
                    #endregion
                }
            }
        }
    }
    [DisallowConcurrentExecution]
    public class IDGJob2 : IJob
    {
        public IDGJob2()
        {

        }
        public void Execute (IJobExecutionContext context)
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    #region Thu gon

                    var repoconfig = uow.Repository<CONFIG>();
                    var repoTmsSendmail = uow.Repository<TMS_SentEmail>();
                    var entitySendmail =
                        repoTmsSendmail.GetAll(a => a.type_sent == (int)UtilConstants.TypeSentEmail.SendMailReminderLogin).Select(a => a.mail_receiver);
                    var entity = repoconfig.GetAll(a => a.KEY.Equals("DataLmsUser")).FirstOrDefault();
                    if (entity == null)
                    {
                        uow.Dispose();
                        return;
                    }
                    var data = JObject.Parse(entity.VALUE);
                    var date = (int)(DateTime.UtcNow.AddDays(-3).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    var postTitles =
                        from p in data["users"]
                        where (int)p["lastaccess"] < date && !entitySendmail.Contains(p["email"].ToString())
                        select (string)p["email"];

                    foreach (var item in postTitles)
                    {
                        var insertTmsSentEmail = new TMS_SentEmail
                        {
                            mail_receiver = item,
                            type_sent = (int)UtilConstants.TypeSentEmail.SendMailReminderLogin,
                            flag_sent = 0,
                            subjectname = "Reminder Login",
                            content_body = "Reminder Login",
                            Is_Active = true,
                            Is_Deleted = false
                        };
                        repoTmsSendmail.Insert(insertTmsSentEmail);
                    }
                    uow.SaveChanges();
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
               
            }
        }

    }
    [DisallowConcurrentExecution]
    public class Cronservices : IJob
    {
        public Cronservices()
        {

        }
        public void Execute(IJobExecutionContext context)
        {
            if (CallProgram())
            {
                if (CallCourse())
                {
                    CallAssign();
                }
            }
        }

        //protected bool CallCron()
        //{

        //    if (CallProgram())
        //    {
        //        if (CallCourse())
        //        {
        //            CallAssign();
        //        }
        //    }           
        //    return true;
        //}
        protected bool CallProgram()
        {
            var result = CallServices(UtilConstants.CRON_PROGRAM);
            if (result != null)
            {
                if (!result.result)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Call CRON_PROGRAM", result.message);
                }

            }
            return result?.result ?? false;
        }
        protected bool CallCourse()
        {
            var result = CallServices(UtilConstants.CRON_COURSE);
            if (result != null)
            {
                if (!result.result)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Call CRON_COURSE", result.message);
                }

            }
            return result?.result ?? false;
        }
        protected bool CallAssign()
        {

            var result = CallServices(UtilConstants.CRON_ASSIGN_TRAINEE);
            if (result != null)
            {
                if (!result.result)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Call CRON_ASSIGN_TRAINEE", result.message);
                }
            }
            return result?.result ?? false;
        }

        private static AjaxResponseViewModel CallServices(string type)
        {
            var model = new AjaxResponseViewModel();
            try
            {
                var server = ConfigurationManager.AppSettings["API_LMS_SERVER"] ?? "";
                var token = ConfigurationManager.AppSettings["API_LMS_TOKEN"] ?? "";
                var function = ConfigurationManager.AppSettings["FUNCTION"] ?? "";
                var moodlewsrestformat = ConfigurationManager.AppSettings["moodlewsrestformat"] ?? "";
                var restClient = new RestClient(server);
                var request = new RestRequest(Method.POST);
                request.AddParameter("wstoken", token);

                request.AddParameter("wsfunction", function);
                request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                request.AddParameter("type", type);
                var response = restClient.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    model.message = "Error response !!!";
                    model.result = false;
                    return model;
                }

                var responseContent = response.Content;
                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    model.message = "";
                    model.result = true;
                    return model;
                }
                model.message = (response.ErrorException == null ? "" : response.ErrorException.ToString()) + (response.ErrorMessage == null ? "" : "---" + response.ErrorMessage.ToString() + "-----") + response.Content;
                model.result = false;
                return model;
            }
            catch (Exception ex)
            {
                model.message = ex.Message.ToString();
                model.result = false;
                return model;

            }


        }
        protected static void LogEvent(UtilConstants.LogType logType,
           UtilConstants.LogEvent logEvent, string logSourse, object messageEx)
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    #region Thu gon
                    var _repoSYS_LogEvent = uow.Repository<SYS_LogEvent>();
                    var _content = JsonConvert.SerializeObject(messageEx);

                    //var _content = "đang test";
                    var entity = new SYS_LogEvent();
                    entity.LogType = (int)logType;
                    entity.LogEvent = (int)logEvent;
                    entity.Source = logSourse;
                    entity.UserName = 7;
                    entity.Content = _content;
                    //entity.PageID
                    entity.CreateDay = DateTime.Now;
                    entity.IsDeleted = false;
                    _repoSYS_LogEvent.Insert(entity);
                    uow.SaveChanges();
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
             
            }

        }
    }
    [DisallowConcurrentExecution]
    public class Cronservices_Assign : IJob
    {
        public Cronservices_Assign()
        {

        }
        public async void Execute(IJobExecutionContext context)
        {
            await CallUser();
            await CallResult();
        }        
        protected async Task<bool> CallResult()
        {
            await Task.Delay(100);
            var result = CallServices(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY);
            if (result != null)
            {
                if (!result.result)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Call CRON_ASSIGN_TRAINEE", result.message);
                }
            }
            return result?.result ?? false;
        }
        protected async Task<bool> CallUser()
        {
            await Task.Delay(100);
            var result = CallServices(UtilConstants.CRON_USER);
            if (result != null)
            {
                if (!result.result)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Call CRON_USER", result.message);
                }
            }
            return result?.result ?? false;
        }
        private static AjaxResponseViewModel CallServices(string type)
        {
            var model = new AjaxResponseViewModel();
            try
            {
                var server = ConfigurationManager.AppSettings["API_LMS_SERVER"] ?? "";
                var token = ConfigurationManager.AppSettings["API_LMS_TOKEN"] ?? "";
                var function = ConfigurationManager.AppSettings["FUNCTION"] ?? "";
                var moodlewsrestformat = ConfigurationManager.AppSettings["moodlewsrestformat"] ?? "";
                var restClient = new RestClient(server);
                var request = new RestRequest(Method.POST);
                request.AddParameter("wstoken", token);

                request.AddParameter("wsfunction", function);
                request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                request.AddParameter("type", type);
                var response = restClient.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    model.message = "Error response !!!";
                    model.result = false;
                    return model;
                }

                var responseContent = response.Content;
                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    model.message = "";
                    model.result = true;
                    return model;
                }
                model.message = (response.ErrorException == null ? "" : response.ErrorException.ToString()) + (response.ErrorMessage == null ? "" : "---" + response.ErrorMessage.ToString() + "-----") + response.Content;
                model.result = false;
                return model;
            }
            catch (Exception ex)
            {
                model.message = ex.Message.ToString();
                model.result = false;
                return model;

            }


        }
        protected static void LogEvent(UtilConstants.LogType logType,
           UtilConstants.LogEvent logEvent, string logSourse, object messageEx)
        {
            using (var dbContext = new EFDbContext())
            {
                var uow = new UnitOfWork(dbContext);
                try
                {
                    #region Thu gon
                    var _repoSYS_LogEvent = uow.Repository<SYS_LogEvent>();
                    var _content = JsonConvert.SerializeObject(messageEx);

                    //var _content = "đang test";
                    var entity = new SYS_LogEvent();
                    entity.LogType = (int)logType;
                    entity.LogEvent = (int)logEvent;
                    entity.Source = logSourse;
                    entity.UserName = 7;
                    entity.Content = _content;
                    //entity.PageID
                    entity.CreateDay = DateTime.Now;
                    entity.IsDeleted = false;
                    _repoSYS_LogEvent.Insert(entity);
                    uow.SaveChanges();
                    #endregion
                    uow.Dispose();
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
              
            }

        }
    }
}