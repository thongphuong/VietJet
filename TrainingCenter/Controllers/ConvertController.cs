using System;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.Services.Configs;
namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using DAL.Repositories;
    using DAL.UnitOfWork;
    using global::Utilities;
    using NReco.ImageGenerator;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using TMS.API.Utilities;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.ReportModels;
    using TrainingCenter.Utilities;

    public class ConvertController : Controller
    {
        private readonly ICourseService _CourseService;
        private readonly IEmployeeService _EmployeeService;
        private readonly IConfigService _ConfigService;
        public ConvertController(ICourseService CourseService, IEmployeeService EmployeeService, IConfigService ConfigService)
        {
            _EmployeeService = EmployeeService;
            _CourseService = CourseService;
            _ConfigService = ConfigService;
        }
        // GET: Convert
        public ActionResult Index(string mes = null)
        {
            ViewBag.Message = mes;
            return View();
        }
        public ActionResult ConvertSubjectIndex()
        {
            var record = _CourseService.GetCourseResult(p => !string.IsNullOrEmpty(p.Backgroundcertificate) && p.Backgroundcertificate != "0" && string.IsNullOrEmpty(p.Path));
            ViewBag.Count = record.Count();
            return View();
        }
        [AllowAnonymous]
        public ActionResult ConvertPassword()
        {
            using (var DbContext = new EFDbContext())
            {
                UnitOfWork _uow = new UnitOfWork(DbContext);
                IRepository<USER> repoUser = _uow.Repository<USER>();
                IRepository<Trainee> repoTrainee = _uow.Repository<Trainee>();

                var user = repoUser.GetAll(p => !string.IsNullOrEmpty(p.PASSWORD));
                var trainee = repoTrainee.GetAll(p => !string.IsNullOrEmpty(p.Password));
                foreach (var item_user in user)
                {
                    try
                    {
                        var decryptTriple = DecryptString_Triple(item_user.PASSWORD);
                        var encrypt = EncryptString(decryptTriple);
                        item_user.PASSWORD = encrypt;
                        repoUser.Update(item_user);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                _uow.SaveChanges();
                foreach (var item_trainee in trainee)
                {
                    try
                    {
                        var decryptTriple = DecryptString_Triple(item_trainee.Password);
                        var encrypt = EncryptString(decryptTriple);
                        item_trainee.Password = encrypt;
                        repoTrainee.Update(item_trainee);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                _uow.SaveChanges();
            }
            var message = "Thành công";
            return Redirect("Index?mes=" + message);
        }

        public static string EncryptString(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            AesCryptoServiceProvider tdesAlgorithm = new AesCryptoServiceProvider();

            // Step 3. Setup the encoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = UTF8.GetBytes(message);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        public static string DecryptString(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            AesCryptoServiceProvider tdesAlgorithm = new AesCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Step 3. Setup the decoder

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(message);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(results);
        }

        public static string EncryptString_Triple(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = UTF8.GetBytes(message);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        public static string DecryptString_Triple(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Step 3. Setup the decoder

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(message);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(results);
        }

        #region [---------------------------Convert Certificate-------------------------------]
        [HttpPost]
        public ActionResult ConvertCertificateSubjet()
        {
            var record = _CourseService.GetCourseResult(p => !string.IsNullOrEmpty(p.Backgroundcertificate) && p.Backgroundcertificate != "0" && string.IsNullOrEmpty(p.Path)).Take(500);
            foreach (var item_record in record)
            {
                try
                {
                    var catCertificate = _CourseService.GetCatCertificates(a => a.Type == 0).FirstOrDefault();
                    if (catCertificate != null)
                    {
                        var content = BodyCertificateSubject(catCertificate, null, item_record.Trainee, item_record.Course_Detail.Course, item_record);
                        var path_temp = ReplaceCertificateNo(content, item_record.CertificateSubject, 0);
                        var stream = HTMLToImage(path_temp);
                        Stream filestream = new MemoryStream(stream);
                        var path = SaveImage(filestream, item_record.Trainee.str_Staff_Id);
                        if (!string.IsNullOrEmpty(path))
                        {
                            item_record.Path = path;
                            _CourseService.UpdateCourseResult(item_record);
                        }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ConvertCertificateFinal()
        {
            var record = _CourseService.GetCourseResultFinal(p => !string.IsNullOrEmpty(p.backgroundcertificate) && p.backgroundcertificate != "0" && string.IsNullOrEmpty(p.Path));
            foreach (var item_record in record)
            {
                try
                {
                    var catCertificate = _CourseService.GetCatCertificates(a => a.Type == 1).FirstOrDefault();
                    if (catCertificate != null)
                    {
                        var content = BodyCertificate(catCertificate, null, item_record.Trainee,
                                        item_record.Course, item_record);
                        var path_temp = ReplaceCertificateNo(content, item_record.certificatefinal, 1);
                        var stream = HTMLToImage(path_temp);
                        Stream filestream = new MemoryStream(stream);
                        var path = SaveImage(filestream, item_record.Trainee.str_Staff_Id);
                        if (!string.IsNullOrEmpty(path))
                        {
                            item_record.Path = path;
                            _CourseService.UpdateCourseResultFinalReturnEntity(item_record);
                        }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        protected string BodyCertificateSubject(CAT_CERTIFICATE catCertificate, USER user = null, Trainee trainee = null, Course program = null, Course_Result result = null)
        {
            var body = catCertificate.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(program.Name) ? string.Empty : program.Name))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName)))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(trainee.JobTitle?.Name) ? string.Empty : trainee.JobTitle.Name))
                   //.Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(result?.CertificateSubject) ? string.Empty : result?.CertificateSubject))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_NAME, (string.IsNullOrEmpty(result?.Course_Detail?.SubjectDetail?.Name) ? string.Empty : result?.Course_Detail?.SubjectDetail?.Name))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_DATEFROM, (result.Course_Detail.dtm_time_from.HasValue ? DateUtil.DateToStringCertificate(result.Course_Detail.dtm_time_from, "dd MMMM yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_SUBJECT_DATETO, (result.Course_Detail.dtm_time_to.HasValue ? DateUtil.DateToStringCertificate(result.Course_Detail.dtm_time_to, "dd MMMM yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_DATE_OF_BIRTH, trainee.dtm_Birthdate.HasValue ? DateUtil.DateToString(trainee.dtm_Birthdate, "dd/MM/yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_PLACE_OF_BIRTH, !string.IsNullOrEmpty(trainee.str_Place_Of_Birth) ? trainee.str_Place_Of_Birth : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_FROM, program.StartDate.HasValue ? DateUtil.DateToStringCertificate(program.StartDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_TO, program.EndDate.HasValue ? DateUtil.DateToStringCertificate(program.EndDate, "dd MMMM yyyy") : string.Empty)
                   ;
            return body;
        }
        public string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            fullName = lastName + " " + firstName;
            return fullName;
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
        public byte[] HTMLToImage(string html)
        {
            string read = @"<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset =UTF-8""/></head><body>" + html + "</body></html>";
            var htmlToImageConv = new NReco.ImageGenerator.HtmlToImageConverter();
            var image = htmlToImageConv.GenerateImage(read, ImageFormat.Png);
            return image;
        }
        protected string SaveImage(Stream filestream, string eid)
        {
            var path = "/Uploads/Certificate/";
            var random = new Random();
            var nameFile = DateTime.Now.ToString(GetByKey("SuffixesDateFormat"));
            nameFile = eid + "Cert" + "_" + nameFile + random.Next(1, 100) + ".png";

            var useAws = AppUtils.getAppSetting("UseAws");
            if (useAws == "1")
            {
                path = path.Substring(1);
                if (!AWSUtils.AWS_CheckFolderExists(path))
                {
                    AWSUtils.AWS_RunFolderCreationDemo(path);
                }
                AWSUtils.AWS_PutObject(path + nameFile, filestream);
            }
            return path + nameFile;
        }
        protected string GetByKey(string key)
        {
            return _ConfigService.GetByKey(key);
        }
        protected string BodyCertificate(CAT_CERTIFICATE catCertificate, USER user = null, Trainee trainee = null, Course program = null, Course_Result_Final final = null)
        {
            var body = catCertificate.Content;
            body = body?
                   .Replace(UtilConstants.CERTIFICATE_COURSE_NAME, (string.IsNullOrEmpty(program.Name) ? string.Empty : program.Name))
                   //.Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : string.Format("{0} {1}", trainee.FirstName, trainee.LastName)))
                   .Replace(UtilConstants.CERTIFICATE_FULLNAME, (string.IsNullOrEmpty(trainee.LastName) ? string.Empty : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName)))
                  .Replace(UtilConstants.CERTIFICATE_DATE_COMPLELTED, (final.CreateCertificateDate.HasValue ? DateUtil.DateToString(final.CreateCertificateDate, "dd/MM/yyyy") : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_JOBTITLE_NAME, (string.IsNullOrEmpty(trainee.JobTitle?.Name) ? string.Empty : trainee.JobTitle.Name))
                   .Replace(UtilConstants.CERTIFICATE_GRADE, (final.grade.HasValue ? UtilConstants.GradeDictionary()[(int)final.grade] : string.Empty))
                   .Replace(UtilConstants.CERTIFICATE_POINT, (final.point?.ToString() ?? string.Empty))
                   //.Replace(UtilConstants.CERTIFICATE_SR_NO, (string.IsNullOrEmpty(result?.CertificateSubject) ? string.Empty : result?.CertificateSubject))
                   //.Replace(UtilConstants.CERTIFICATE_ATO, (string.IsNullOrEmpty(final?.certificatefinal) ? string.Empty : final?.certificatefinal))                  
                   .Replace(UtilConstants.CERTIFICATE_DATE_OF_BIRTH, trainee.dtm_Birthdate.HasValue ? DateUtil.DateToString(trainee.dtm_Birthdate, "dd/MM/yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_PLACE_OF_BIRTH, !string.IsNullOrEmpty(trainee.str_Place_Of_Birth) ? trainee.str_Place_Of_Birth : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_FROM, program.StartDate.HasValue ? DateUtil.DateToStringCertificate(program.StartDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_COURSE_DATE_TO, program.EndDate.HasValue ? DateUtil.DateToStringCertificate(program.EndDate, "dd MMMM yyyy") : string.Empty)
                   .Replace(UtilConstants.CERTIFICATE_TRAINEE_AVATAR, !string.IsNullOrEmpty(trainee?.avatar) ? "<img src='" + ConfigurationSettings.AppSettings["AWSLinkS3"] + "Uploads/avatar/" + trainee?.avatar + "' width='175' height='240'>" : string.Empty);
            return body;
        }
        #endregion
        #region [---------------------------Convert Final Result-------------------------------]
        public ActionResult UpdateFinalResult(string mes = null, string username = "", string password = "")
        {
            using (var DbContext = new EFDbContext())
            {
                if (username == "admin" && password == "VJAAVietjet@123")
                {
                    UnitOfWork _uow = new UnitOfWork(DbContext);
                    IRepository<Course> repoCourse = _uow.Repository<Course>();
                    var list = repoCourse.GetAll(p => p.IsDeleted != true && p.TMS_APPROVES.Any(t => t.int_Type == (int)Constants.ApproveType.CourseResult && t.int_id_status == (int)Constants.EStatus.Pending)).ToDictionary(a => a.Id, a => a.Code + " " + a.Name);
                    ViewBag.ListCourse = list;
                    ViewBag.Message = mes;
                }
                else
                {
                    return Redirect("~/Redirect/ErrorPage");
                }
            }
            return View();
        }

        public ActionResult UpdateResult(int courseid = 0, int pass = 80, int distinction = 90)
        {
            using (var DbContext = new EFDbContext())
            {

                try
                {

                    UnitOfWork _uow = new UnitOfWork(DbContext);
                    IRepository<Course> repoCourse = _uow.Repository<Course>();
                    IRepository<Course_Detail> CourseDetailService = _uow.Repository<Course_Detail>();
                    IRepository<Course_Result> CourseResultService = _uow.Repository<Course_Result>();
                    IRepository<Course_Result_Final> CourseResultFinalService = _uow.Repository<Course_Result_Final>();

                    var course = repoCourse.Get(p => p.Id == courseid);

                    var courseList = courseid;
                    var txtCoursepass = pass;
                    var txtCoursedistinction = distinction;

                    var data_ = CourseDetailService.GetAll(a => a.CourseId == courseList).Select(a => a.Id);
                    var course_result = CourseResultService.GetAll(a => data_.Contains((int)a.CourseDetailId));
                    var data_new = CourseResultFinalService.GetAll(a => (a.point == null || a.grade == null) && a.IsDeleted != true && a.courseid == courseList && a.Trainee.TMS_Course_Member.Any(x => data_.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)));
                    var filtered = data_new.AsEnumerable().Select(c => new AjaxFinalResultsModel()
                    {
                        Point = GetResultFinal_Custom(UtilConstants.SwitchResult.Point,
                           data_, course_result,
                           c.traineeid, txtCoursepass,
                           txtCoursedistinction),
                        Grade = GetResultFinal_Custom(UtilConstants.SwitchResult.Grade,
                           data_, course_result,
                           c.traineeid,
                           txtCoursepass,
                           txtCoursedistinction),
                        Action = c != null ? c.id.ToString() : "-1",
                        TraineeId = c?.traineeid,
                    }).ToList();

                    var i = 0;
                    foreach (var item in filtered)
                    {
                        if (!string.IsNullOrEmpty(item.Action) && item.Action != "-1")
                        {
                            var courseResultId = int.Parse(item.Action);
                            var coursefinal = CourseResultFinalService.Get(courseResultId);

                            Dictionary<string, object> dic_final = new Dictionary<string, object>();

                            var score = item.Point;
                            if (!string.IsNullOrEmpty(score) && score != "-1")
                                dic_final.Add("point", double.Parse(score));
                            var result = item.Grade;
                            if (result != null)
                            {
                                switch (result.ToLower())
                                {
                                    case "fail":
                                        dic_final.Add("grade", (int)UtilConstants.Grade.Fail);
                                        break;
                                    case "pass":
                                        dic_final.Add("grade", (int)UtilConstants.Grade.Pass);
                                        break;
                                    case "distinction":
                                        dic_final.Add("grade", (int)UtilConstants.Grade.Distinction);
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(item.Action) && item.Action != "-1")
                            {
                                if (CMSUtils.UpdateDataSQLNoLog("id", coursefinal.id + "", "Course_Result_Final", dic_final.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_final.Values.ToArray())) > 0)
                                {

                                }
                            }
                        }
                        i++;
                    }
                    var message = "Thành công";
                    return Redirect("UpdateFinalResult?mes=" + message + "&username=admin&password=VJAAVietjet@123");
                }
                catch (Exception ex)
                {
                    var message = "Thất bại " + ex.Message.ToString();

                    return Redirect("UpdateFinalResult?mes=" + message + "&username=admin&password=VJAAVietjet@123");

                }
            }
        }


        private string GetResultFinal_Custom(UtilConstants.SwitchResult type, IEnumerable<int> courseDetailIds, IQueryable<Course_Result> course_result, int? traineeid = -1, int scorepass = -1, int scoredistinction = -1)
        {
            var result = string.Empty;
            double? score = 0;
            var countscore = 0;
            bool checkRe = false;
            var checkfail = false;
            var grade = (int)UtilConstants.Grade.Pass;
            var strGrade = "Pass";
            //if (getcoursedetail.Any())
            //{
            //var courseDetailId = getcoursedetail.Select(a => a.Id);
            var courseSummary = course_result.Where(a => a.TraineeId == traineeid);
            var allpassfail = courseSummary.All(a => a.Course_Detail.SubjectDetail.IsAverageCalculate == false);
            //var courseResult = CourseService.GetCourseResult(a => a.TraineeId == traineeid && courseDetailId.Contains((int)a.CourseDetailId)).ToList();
            if (courseSummary.Any())
            {
                #region [get point] 
                foreach (var item in courseSummary)
                {
                    if ((bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Score != null)
                        {
                            if (item.Re_Check_Score == -1)
                            {
                                item.Re_Check_Score = 0;
                            }
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                            checkRe = true;
                            score = score + item.Re_Check_Score;
                        }
                        else
                        {
                            if (item?.First_Check_Score != null)
                            {
                                if (item.First_Check_Score == -1)
                                {
                                    item.First_Check_Score = 0;

                                }
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;
                                }
                                score = score + item.First_Check_Score;
                            }

                        }
                        countscore++;
                    }
                    else if (!(bool)item.Course_Detail.SubjectDetail.IsAverageCalculate)
                    {
                        if (item?.Re_Check_Result != null)
                        {
                            checkRe = true;
                            if (item?.Re_Check_Result == "F")
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;

                            }
                        }
                        else
                        {
                            if (item?.First_Check_Result != null)
                            {
                                if (item?.First_Check_Result == "F")
                                {
                                    checkfail = true;
                                    grade = (int)UtilConstants.Grade.Fail;

                                }
                            }
                            else
                            {
                                checkfail = true;
                                grade = (int)UtilConstants.Grade.Fail;
                                break;
                            }

                        }

                    }
                }
                if (countscore != 0)
                {
                    var score_temp = score / countscore;
                    score = Math.Round((double)score_temp, 1);
                }
            }
            else
            {
                checkfail = true;
            }
            var point = score.ToString();
            if (type == UtilConstants.SwitchResult.Point)
            {
                result = point;
            }

            #endregion
            if (type == UtilConstants.SwitchResult.Grade)
            {
                var countDetail = courseDetailIds.Count();
                var countResult = courseSummary.Count();
                if (countResult < countDetail)
                {
                    checkfail = true;
                }
                #region [get grade] 
                var checkFail = courseSummary.Any(
                        a => (a.Type == true));
                if (checkFail || checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                else
                {

                    //true = checked

                    if (allpassfail == false)
                    {
                        if (score > 0)
                        {
                            if (score < scorepass)
                            {
                                grade = (int)UtilConstants.Grade.Fail;
                            }
                            else if (scorepass <= score && score < scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Pass;
                            }
                            else if (score >= scoredistinction)
                            {
                                grade = (int)UtilConstants.Grade.Distinction;
                            }
                        }
                        else
                        {
                            grade = (int)UtilConstants.Grade.Fail;
                        }
                    }
                }
                if (grade == (int)UtilConstants.Grade.Distinction)
                {
                    if (checkRe)
                    {
                        grade = (int)UtilConstants.Grade.Pass;
                    }
                }
                if (checkfail)
                {
                    grade = (int)UtilConstants.Grade.Fail;
                }
                switch (grade)
                {
                    case (int)UtilConstants.Grade.Fail:
                        grade = (int)UtilConstants.Grade.Fail;
                        strGrade = "Fail";
                        break;
                    case (int)UtilConstants.Grade.Pass:
                        grade = (int)UtilConstants.Grade.Pass;
                        strGrade = "Pass";
                        break;
                    case (int)UtilConstants.Grade.Distinction:
                        grade = (int)UtilConstants.Grade.Distinction;
                        strGrade = "Distinction";
                        break;
                }

                result = strGrade;
                #endregion
            }
            return result;
        }
        #endregion

        #region [---------------------------Update assign trainee-------------------------------]
        public ActionResult UpdateAssignTrainee(string mes = null, string username = "", string password = "")
        {
            using (var DbContext = new EFDbContext())
            {
                if (username == "admin" && password == "VJAAVietjet@123")
                {
                    UnitOfWork _uow = new UnitOfWork(DbContext);
                    IRepository<Course> repoCourse = _uow.Repository<Course>();
                    var model = new SubjectResultModel
                    {
                        Courseses =
                   repoCourse.GetAll(a => a.IsDeleted != true && a.IsActive == true && a.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && b.int_id_status == (int)UtilConstants.EStatus.Approve))
                       .OrderByDescending(b => b.Id)
                       .ToDictionary(a => a.Id, a => a.Name)
                    };
                    ViewBag.Message = mes;
                    return View(model);
                }
                else
                {
                    return Redirect("~/Redirect/ErrorPage");
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Filtercourse(FormCollection form)
        {
            using (var DbContext = new EFDbContext())
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var html = new StringBuilder();
                var fCode = string.IsNullOrEmpty(form["coursecode"]) ? "" : form["coursecode"].ToLower().Trim();

                UnitOfWork _uow = new UnitOfWork(DbContext);
                IRepository<Course> repoCourse = _uow.Repository<Course>();

                var data = repoCourse.GetAll(a => a.IsPublic != true && a.IsActive == true && a.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && b.int_id_status == (int)UtilConstants.EStatus.Approve) &&
                            (string.IsNullOrEmpty(fCode) || a.Code.ToLower().Contains(fCode)))
                            .OrderByDescending(p => p.Id);
                if (!data.Any())
                    return Json(new
                    {
                        value_option = html.ToString()
                    }, JsonRequestBehavior.AllowGet);
                html.Append("<option></option>");
                foreach (var item in data)
                {
                    html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Name);
                }
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ChangeCourseReturnSubjectResult(int courseId, int? reportsubjectresult)
        {
            using (var DbContext = new EFDbContext())
            {
                try
                {
                    UnitOfWork _uow = new UnitOfWork(DbContext);
                    IRepository<Course_Detail> repoCourseDetail = _uow.Repository<Course_Detail>();

                    StringBuilder html = new StringBuilder();
                    int null_instructor = 1;
                    var data = repoCourseDetail.GetAll(a => a.CourseId == courseId && a.IsDeleted != true && a.IsActive == true);//
                    if (data.Any())
                    {
                        null_instructor = 0;
                        foreach (var item in data.Where(a => (reportsubjectresult == 1 ? true : true)))
                        {
                            html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, (item?.SubjectDetail?.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + item.SubjectDetail.Name);
                        }
                    }
                    else
                    {
                        null_instructor = 1;
                    }

                    var objectReturn = Json(new
                    {
                        value_option = html.ToString(),
                        value_null = null_instructor
                    }, JsonRequestBehavior.AllowGet);

                    return objectReturn;
                }
                catch (Exception ex)
                {
                    return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
                }
            }
        }


        public ActionResult UpdateAssign(int courseid = 0, int detail = 0)
        {
            using (var DbContext = new EFDbContext())
            {

                try
                {

                    UnitOfWork _uow = new UnitOfWork(DbContext);
                    IRepository<Course> Course = _uow.Repository<Course>();
                    IRepository<Course_Detail> CourseDetail = _uow.Repository<Course_Detail>();
                    IRepository<TMS_Course_Member> CourseMember = _uow.Repository<TMS_Course_Member>();
                    var message = "";
                    if (detail != 0)
                    {
                        var course = Course.Get(p => p.Id == courseid);
                        var course_detail = CourseDetail.Get(a => a.CourseId == courseid && a.Id == detail);
                        var course_Member = CourseMember.GetAll(a => a.Course_Details_Id == detail && a.Status != 1).ToList();
                        course.LMSStatus = 1;
                        course_detail.LmsStatus = 1;
                        if (course_Member.All(a => a.LmsStatus == 1))
                        {
                            message = "Chưa đồng bộ";
                            return Redirect("UpdateAssignTrainee?mes=" + message + "&username=admin&password=VJAAVietjet@123");
                        }
                        course_Member.ForEach(a => a.LmsStatus = 1);
                        _uow.SaveChanges();
                        message = "Thành công";
                        return Redirect("UpdateAssignTrainee?mes=" + message + "&username=admin&password=VJAAVietjet@123");
                    }
                    else
                    {
                        message = "Chưa chọn môn học";
                        return Redirect("UpdateAssignTrainee?mes=" + message + "&username=admin&password=VJAAVietjet@123");
                    }
                }
                catch (Exception ex)
                {
                    var message = "Thất bại " + ex.Message.ToString();

                    return Redirect("UpdateAssignTrainee?mes=" + message + "&username=admin&password=VJAAVietjet@123");

                }
            }
        }

        #endregion

        [AllowAnonymous]
        //TODO : co thay doi iduser = iddata
        public ActionResult checksession()
        {
            DataTable db_exam = CMSUtils.GetDataSQL("", "TMS_Approval_Type", "TOP 1 id", "", "");
            return Json(new
            {
                sEcho = ""
            },
           JsonRequestBehavior.AllowGet); ;
        }
    }
}