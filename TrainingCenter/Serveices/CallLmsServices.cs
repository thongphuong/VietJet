using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using TrainingCenter.Utilities;
using System.Net;
using DAL.Entities;
using TMS.Core.Utils;

namespace TrainingCenter.Serveices
{
    using DAL.UnitOfWork;
    using TMS.Core.ViewModels;

    public partial class CallLmsServices
    {
        #region Init
        readonly UnitOfWork _uow;
        private readonly DAL.Repositories.IRepository<LMS_REQUEST> _repoLMS_REQUEST;
        private readonly DAL.Repositories.IRepository<Subject_Score> _repoSubject_Score;
        private readonly DAL.Repositories.IRepository<Trainee_Record> _repoTrainee_Record;
        private readonly DAL.Repositories.IRepository<Trainee_Contract> _repoTrainee_Contract;
        private readonly DAL.Repositories.IRepository<Trainee_TrainingCenter> _repoTrainee_TrainingCenter;
        private readonly DAL.Repositories.IRepository<Nation> _repoNation;
        private readonly DAL.Repositories.IRepository<TMS_Course_Member> _repoTMS_Course_Member;
        private readonly DAL.Repositories.IRepository<CONFIG> _repoConfig;
        public CallLmsServices()
        {
            using (var dbcontext = new EFDbContext())
            {
                _uow = new UnitOfWork(dbcontext);
            }
            this._repoLMS_REQUEST = _uow.Repository<LMS_REQUEST>();
            this._repoSubject_Score = _uow.Repository<Subject_Score>();
            this._repoTrainee_Record = _uow.Repository<Trainee_Record>();
            this._repoTrainee_Contract = _uow.Repository<Trainee_Contract>();
            this._repoTrainee_TrainingCenter = _uow.Repository<Trainee_TrainingCenter>();
            this._repoNation = _uow.Repository<Nation>();
            this._repoTMS_Course_Member = _uow.Repository<TMS_Course_Member>();
            this._repoConfig = _uow.Repository<CONFIG>();
        }
        #endregion

        public bool checkserver(IRestResponse response, string type_request, int Course_Id)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var str_response = response.Content.Split(new char[] { ':' })[0];

                if (str_response.Contains("exception"))
                {
                    LMS_REQUEST LMS_REQUEST = new LMS_REQUEST();
                    LMS_REQUEST.id_item = Course_Id;
                    LMS_REQUEST.type_request = type_request;
                    LMS_REQUEST.messege = response.Content;
                    LMS_REQUEST.result = "Fail";
                    LMS_REQUEST.createdate = DateTime.Now;
                    _repoLMS_REQUEST.Insert(LMS_REQUEST);
                    _uow.SaveChanges();
                    return false;
                }
                return true;
            }
            else
            {
                LMS_REQUEST LMS_REQUEST = new LMS_REQUEST();
                LMS_REQUEST.id_item = Course_Id;
                LMS_REQUEST.type_request = type_request;
                LMS_REQUEST.messege = response.ErrorMessage.ToString();
                LMS_REQUEST.result = "Fail";
                LMS_REQUEST.createdate = DateTime.Now;
                _repoLMS_REQUEST.Insert(LMS_REQUEST);
                _uow.SaveChanges();
                return false;
            }
        }

        private bool checkServer(IRestResponse response, string type_request, int? Course_Id)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var str_response = response.Content.Split(new char[] { ':' })[0];
                if (str_response.Contains("exception"))
                {
                    LMS_REQUEST LMS_REQUEST = new LMS_REQUEST();
                    LMS_REQUEST.id_item = Course_Id;
                    LMS_REQUEST.type_request = type_request;
                    LMS_REQUEST.messege = response.Content;
                    LMS_REQUEST.result = "Fail";
                    LMS_REQUEST.createdate = DateTime.Now;
                    _repoLMS_REQUEST.Insert(LMS_REQUEST);
                    _uow.SaveChanges();
                    return false;
                }
                return true;
            }
            else
            {
                LMS_REQUEST LMS_REQUEST = new LMS_REQUEST();
                LMS_REQUEST.id_item = Course_Id;
                LMS_REQUEST.type_request = type_request;
                LMS_REQUEST.messege = response.ErrorMessage.ToString();
                LMS_REQUEST.result = "Fail";
                LMS_REQUEST.createdate = DateTime.Now;
                _repoLMS_REQUEST.Insert(LMS_REQUEST);
                _uow.SaveChanges();
                return false;
            }
        }
        #region Course
        //create course

        public bool CallLmsServices_CreateCourse(Course mModel)
        {
            //var server = _repoConfig.Get(a=>a.KEY =="API_LMS_SERVER");
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_COURSE_CREATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("categories[0][name]", mModel.Name);
            request.AddParameter("categories[0][code]", mModel.Code);
            request.AddParameter("categories[0][idnumber]", mModel.Id);
            request.AddParameter("categories[0][description]", mModel.Note);
            request.AddParameter("categories[0][venue]", mModel.Venue);
            IRestResponse response = restClient.Execute(request);


            return checkserver(response, "API_COURSE_CREATE", mModel.Id);
        }
        public bool CallLmsServices_BlockCourse(Course mModel)
        {

            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_COURSE_UPDATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);

            request.AddParameter("categories[0][id]", 0);// 0 is default to avoid bug from LMS
            request.AddParameter("categories[0][name]", mModel.Name);
            request.AddParameter("categories[0][idnumber]", mModel.Id);
            request.AddParameter("categories[0][description]", mModel.Note);
            request.AddParameter("categories[0][visible]", 0);

            IRestResponse response = restClient.Execute(request);
            return checkserver(response, "API_COURSE_UPDATE", mModel.Id);

        }
        //update course
        public bool CallLmsServices_UpdateCourse(Course mModel)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_COURSE_UPDATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);

            request.AddParameter("categories[0][id]", 0);// 0 is default to avoid bug from LMS
            request.AddParameter("categories[0][name]", mModel.Name);
            request.AddParameter("categories[0][idnumber]", mModel.Id);
            request.AddParameter("categories[0][description]", mModel.Note);
            request.AddParameter("categories[0][venue]", mModel.Venue);
            request.AddParameter("categories[0][visible]", 1);
            IRestResponse<ServicesResultModel> response = restClient.Execute<ServicesResultModel>(request);
            return checkserver(response, "API_COURSE_UPDATE", mModel.Id);
        }

        //Add subject
        public bool CallLmsServices_AddSubject(IEnumerable<Course_Detail> cCourseDetails, string currentUserCode)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_SUBJECT_CREATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            int no = 0;
            foreach (var item in cCourseDetails)//.Where(a=>a.type_leaning != (int)Constants.UserStateConstant.Offline)
            {
                request.AddParameter("courses[" + no + "][shortname]", "c" + item.CourseId + "s_" + item.Id);
                request.AddParameter("courses[" + no + "][fullname]", item.SubjectDetail.Name);
                request.AddParameter("courses[" + no + "][categoryid]", item.CourseId);
                int passfail = 1;
                if (item.SubjectDetail.IsAverageCalculate == false)
                {
                    passfail = 0;
                }
                int regisable = 0;
                if (item.bit_Regisable == true)
                {
                    regisable = 1;
                }
                var data = _repoSubject_Score.GetAll(a => a.subject_id == item.SubjectDetailId).OrderBy(b => b.point_from);
                if (data.Any())
                {
                    var datapass = data.FirstOrDefault().point_from != null ? data.FirstOrDefault().point_from : int.Parse(_repoConfig.Get(a => a.KEY == "KEY_SCORE_PASS").VALUE);
                    string grade = "";
                    foreach (var item_ in data)
                    {
                        //grade += "\"" + item_.point_from + "_" + item_.point_to + "\": \"" + item_.grade + "\",";
                    }

                    request.AddParameter("courses[" + no + "][score]", "{\"registrable\":" + regisable + " , \"passfail\": " + passfail + ",\"pass\": " + datapass + ", \"grade\": { " + grade.Trim(',') + " }}");
                }
                else
                {
                    request.AddParameter("courses[" + no + "][score]", "{\"registrable\":" + regisable + " , \"passfail\": " + passfail + ",\"pass\": " + _repoConfig.Get(a => a.KEY == "KEY_SCORE_PASS").VALUE + "}");
                }
                if (item.dtm_time_from != null)
                {
                    request.AddParameter("courses[" + no + "][startdate]", DateUtil.ConvertToUnixTime((DateTime)item.dtm_time_from));
                }
                else
                {
                    request.AddParameter("courses[" + no + "][startdate]", DateUtil.ConvertToUnixTime(DateTime.Now));
                }
                if (item.dtm_time_to != null)
                {
                    request.AddParameter("courses[" + no + "][enddate]", DateUtil.ConvertToUnixTime((DateTime)item.dtm_time_to));
                }
                else
                {
                    request.AddParameter("courses[" + no + "][enddate]", DateUtil.ConvertToUnixTime(DateTime.Now.AddDays(10)));
                }

                request.AddParameter("courses[" + no + "][visible]", 1);
                if (item.type_leaning != null)
                {
                    request.AddParameter("courses[" + no + "][type]", item.type_leaning);
                }
                else
                {
                    request.AddParameter("courses[" + no + "][type]", 0);
                }
                //request.AddParameter("courses[" + no + "][issurvey]", item.Course.survey);

                if (item.Course_Detail_Instructor.Any())
                {
                    request.AddParameter("courses[" + no + "][instructors][0][username]", currentUserCode);
                    request.AddParameter("courses[" + no + "][instructors][0][role]", "editinginstructor");

                    int noInstructor = 1;
                    foreach (var instructor in item.Course_Detail_Instructor)
                    {
                        request.AddParameter("courses[" + no + "][instructors][" + noInstructor + "][username]", instructor?.Trainee?.str_Staff_Id);
                        request.AddParameter("courses[" + no + "][instructors][" + noInstructor + "][role]", "instructor");
                        noInstructor++;
                    }
                }

                if (item.time_from != null && item.time_from != null)
                {

                    request.AddParameter("courses[" + no + "][schedules][0][starttime]", item?.time_from);
                    request.AddParameter("courses[" + no + "][schedules][0][endtime]", item?.time_to);
                }
                if (item.RoomId != null)
                {
                    request.AddParameter("courses[" + no + "][schedules][0][classroom]", item?.Room?.str_Name);
                }


                no++;
            }
            no = 0;
            IRestResponse<List<ServicesResultModel>> response = restClient.Execute<List<ServicesResultModel>>(request);

            int counterr = 0;
            foreach (var item in cCourseDetails)
            {
                if (!checkserver(response, "API_SUBJECT_CREATE", (int)item.CourseId))
                {
                    counterr++;
                }
            }
            return (counterr == 0);


        }

        //Update subject
        public bool CallLmsServices_UpdateSubject(IEnumerable<Course_Detail> cCourseDetails, string currentUserCode)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_SUBJECT_UPDATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            int no = 0;
            foreach (var item in cCourseDetails)//.Where(a => a.type_leaning != (int)Constants.UserStateConstant.Offline)
            {
                request.AddParameter("courses[" + no + "][id]", 0);
                request.AddParameter("courses[" + no + "][shortname]", "c" + item.CourseId + "s_" + item.Id);
                request.AddParameter("courses[" + no + "][fullname]", item.SubjectDetail.Name);
                request.AddParameter("courses[" + no + "][categoryid]", item.CourseId);

                int passfail = 1;
                if (item.SubjectDetail.IsAverageCalculate == false)
                {
                    passfail = 0;
                }
                int regisable = 0;
                if (item.bit_Regisable == true)
                {
                    regisable = 1;
                }
                var data = _repoSubject_Score.GetAll(a => a.subject_id == item.SubjectDetailId).OrderBy(b => b.point_from);
                if (data.Any())
                {
                    var datapass = data.FirstOrDefault().point_from != null ? data.FirstOrDefault().point_from : int.Parse(_repoConfig.Get(a => a.KEY == "KEY_SCORE_PASS").VALUE);
                    string grade = "";
                    foreach (var item_ in data)
                    {
                        // grade += "\"" + item_.point_from + "_" + item_.point_to + "\": \"" + item_.grade + "\",";
                    }

                    request.AddParameter("courses[" + no + "][score]", "{\"registrable\":" + regisable + " , \"passfail\": " + passfail + ",\"pass\": " + datapass + ", \"grade\": { " + grade.Trim(',') + " }}");
                }
                else
                {
                    request.AddParameter("courses[" + no + "][score]", "{\"registrable\":" + regisable + " , \"passfail\": " + passfail + ",\"pass\": " + _repoConfig.Get(a => a.KEY == "KEY_SCORE_PASS").VALUE + "}");
                }
                if (item.dtm_time_from != null)
                {
                    request.AddParameter("courses[" + no + "][startdate]", DateUtil.ConvertToUnixTime((DateTime)item.dtm_time_from));
                }
                else
                {
                    request.AddParameter("courses[" + no + "][startdate]", DateUtil.ConvertToUnixTime(DateTime.Now));
                }
                if (item.dtm_time_to != null)
                {
                    request.AddParameter("courses[" + no + "][enddate]", DateUtil.ConvertToUnixTime((DateTime)item.dtm_time_to));
                }
                else
                {
                    request.AddParameter("courses[" + no + "][enddate]", DateUtil.ConvertToUnixTime(DateTime.Now.AddDays(10)));
                }

                request.AddParameter("courses[" + no + "][visible]", 1);
                if (item.type_leaning != null)
                {
                    request.AddParameter("courses[" + no + "][type]", item.type_leaning);
                }
                else
                {
                    request.AddParameter("courses[" + no + "][type]", 0);
                }
                //request.AddParameter("courses[" + no + "][issurvey]", item.Course.survey);

                if (item.Course_Detail_Instructor.Any())
                {
                    request.AddParameter("courses[" + no + "][instructors][0][username]", currentUserCode);
                    request.AddParameter("courses[" + no + "][instructors][0][role]", "editinginstructor");

                    int noInstructor = 1;
                    foreach (var instructor in item.Course_Detail_Instructor)
                    {
                        request.AddParameter("courses[" + no + "][instructors][" + noInstructor + "][username]", instructor?.Trainee?.str_Staff_Id);
                        request.AddParameter("courses[" + no + "][instructors][" + noInstructor + "][role]", "instructor");
                        noInstructor++;
                    }
                }

                if (item.time_from != null && item.time_from != null)
                {

                    request.AddParameter("courses[" + no + "][schedules][0][starttime]", item?.time_from);
                    request.AddParameter("courses[" + no + "][schedules][0][endtime]", item?.time_to);
                }
                if (item.RoomId != null)
                {
                    request.AddParameter("courses[" + no + "][schedules][0][classroom]", item?.Room?.str_Name);
                }


                no++;
            }
            no = 0;
            IRestResponse<List<ServicesResultModel>> response = restClient.Execute<List<ServicesResultModel>>(request);

            int counterr = 0;
            foreach (var item in cCourseDetails)
            {
                if (!checkserver(response, "API_SUBJECT_UPDATE", (int)item.CourseId))
                {
                    counterr++;
                }
            }
            return (counterr == 0);

        }


        #endregion

        #region Employeee
        private string return_nation(string nation_code = "")
        {
            string OUT = "";
            var data = _repoNation.GetAll(a => a.Nation_Code == nation_code).Select(a => a.Nation_Name);
            if (data.Any())
            {
                OUT = data.FirstOrDefault();
            }
            return OUT;
        }
        public bool CallLmsServices_CreateUser(IEnumerable<Trainee> tUserforLms, string randompass)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_USER_CREATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            var no = 0;
            foreach (var item in tUserforLms)
            {
                request.AddParameter("users[" + no + "][username]", item.str_Staff_Id.ToLower());
                request.AddParameter("users[" + no + "][idnumber]", item.str_Staff_Id);
                request.AddParameter("users[" + no + "][password]", randompass);
                request.AddParameter("users[" + no + "][firstname]", item.FirstName);
                request.AddParameter("users[" + no + "][lastname]", item.LastName);
                request.AddParameter("users[" + no + "][email]", (item.str_Email ?? item.Id + "@eid.external"));
                request.AddParameter("users[" + no + "][suspended]", item.Suspended);


                request.AddParameter("users[" + no + "][customfields][0][type]", "birthday");
                request.AddParameter("users[" + no + "][customfields][0][value]", (item.dtm_Birthdate != null ? DateUtil.DateToString(item.dtm_Birthdate.Value, "dd/MM/yyyy") : ""));

                request.AddParameter("users[" + no + "][customfields][1][type]", "ID");
                request.AddParameter("users[" + no + "][customfields][1][value]", (item.PersonalId != null ? item.PersonalId : ""));

                request.AddParameter("users[" + no + "][customfields][2][type]", "placeofbirth");
                request.AddParameter("users[" + no + "][customfields][2][value]", (item.str_Place_Of_Birth != null ? item.str_Place_Of_Birth : ""));

                request.AddParameter("users[" + no + "][customfields][3][type]", "cellphone");
                request.AddParameter("users[" + no + "][customfields][3][value]", (item.str_Cell_Phone != null ? item.str_Cell_Phone : ""));

                request.AddParameter("users[" + no + "][customfields][4][type]", "gender");
                request.AddParameter("users[" + no + "][customfields][4][value]", (item?.Gender != null ? item?.Gender.ToString() : ""));

                request.AddParameter("users[" + no + "][customfields][5][type]", "nation");
                request.AddParameter("users[" + no + "][customfields][5][value]", (item?.Nation != null ? return_nation(item?.Nation) : ""));

                request.AddParameter("users[" + no + "][customfields][6][type]", "joindate");
                request.AddParameter("users[" + no + "][customfields][6][value]", (item?.dtm_Join_Date != null ? DateUtil.DateToString(item.dtm_Join_Date.Value, "dd/MM/yyyy") : ""));

                request.AddParameter("users[" + no + "][customfields][7][type]", "jobtitle");
                request.AddParameter("users[" + no + "][customfields][7][value]", (item?.Job_Title_id != null ? item?.JobTitle?.Name : ""));

                request.AddParameter("users[" + no + "][customfields][8][type]", "partner");
                request.AddParameter("users[" + no + "][customfields][8][value]", (item?.Company_Id != null ? item?.Company?.str_Name : ""));

                request.AddParameter("users[" + no + "][customfields][9][type]", "department");
                request.AddParameter("users[" + no + "][customfields][9][value]", (item?.Department_Id != null ? item?.Department?.Name : ""));

                request.AddParameter("users[" + no + "][customfields][10][type]", "passport");
                request.AddParameter("users[" + no + "][customfields][10][value]", (item?.Passport != null ? item?.Passport : ""));

                var dataedu = _repoTrainee_Record.GetAll(a => a.Trainee_Id == item.Id);
                if (dataedu.Any())
                {
                    var noEDU = 0;
                    foreach (var itemedu in dataedu)
                    {
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][dated]", itemedu?.dtm_Created_At != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_Created_At.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][course]", itemedu?.str_Subject != null ? itemedu?.str_Subject : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][organization]", itemedu?.str_organization != null ? itemedu?.str_organization : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][note]", itemedu?.str_note != null ? itemedu?.str_note : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][fromdate]", itemedu?.dtm_time_from != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_time_from.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][todate]", itemedu?.dtm_time_to != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_time_to.Value).ToString() : "");
                        noEDU++;
                    }
                    noEDU = 0;
                }
                var datacontrac = _repoTrainee_Contract.GetAll(a => a.Trainee_Id == item.Id);
                if (datacontrac.Any())
                {
                    var noCONTRAC = 0;
                    foreach (var itemcontrac in datacontrac)
                    {
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][expire_date]", itemcontrac?.expire_date != null ? DateUtil.ConvertToUnixTime(itemcontrac.expire_date.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][contract_no]", itemcontrac?.contractno != null ? itemcontrac?.contractno : "");
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][description]", itemcontrac?.description != null ? itemcontrac?.description : "");
                        noCONTRAC++;
                    }
                    noCONTRAC = 0;
                }

                no++;
            }
            no = 0;
            var response = restClient.Execute<List<ServicesResultModel>>(request);

            int counterr = 0;
            foreach (var item in tUserforLms)
            {
                if (!checkserver(response, "API_USER_CREATE", item.Id))
                {
                    counterr++;
                }
            }
            return (counterr == 0);


        }
        public bool CallLmsServices_UpdateUser(IEnumerable<Trainee> tUserforLms)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_USER_UPDATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            var no = 0;

            //request.AddParameter("users[0][username]", "hung13");
            //request.AddParameter("users[0][password]", "hung13");
            //request.AddParameter("users[0][firstname]", "hung13");
            //request.AddParameter("users[0][lastname]", "hung13");
            //request.AddParameter("users[0][id]", 0);
            //request.AddParameter("users[0][email]", "hung13@sda.com");
            //request.AddParameter("users[0][suspended]", 0);

            foreach (var item in tUserforLms)
            {
                request.AddParameter("users[" + no + "][id]", 0);// set 0 is default to avoid bug from LMS
                request.AddParameter("users[" + no + "][username]", item.str_Staff_Id.ToLower());
                request.AddParameter("users[" + no + "][idnumber]", item.str_Staff_Id);
                request.AddParameter("users[" + no + "][password]", "123");
                request.AddParameter("users[" + no + "][firstname]", item.FirstName);
                request.AddParameter("users[" + no + "][lastname]", item.LastName);
                request.AddParameter("users[" + no + "][email]", (item.str_Email != null ? item.str_Email : item.Id + "@eid.external"));
                request.AddParameter("users[" + no + "][suspended]", item.Suspended);


                request.AddParameter("users[" + no + "][customfields][0][type]", "birthday");
                request.AddParameter("users[" + no + "][customfields][0][value]", (item.dtm_Birthdate != null ? DateUtil.DateToString(item.dtm_Birthdate.Value, "dd/MM/yyyy") : ""));

                request.AddParameter("users[" + no + "][customfields][1][type]", "ID");
                request.AddParameter("users[" + no + "][customfields][1][value]", (item.PersonalId != null ? item.PersonalId : ""));

                request.AddParameter("users[" + no + "][customfields][2][type]", "placeofbirth");
                request.AddParameter("users[" + no + "][customfields][2][value]", (item.str_Place_Of_Birth != null ? item.str_Place_Of_Birth : ""));

                request.AddParameter("users[" + no + "][customfields][3][type]", "cellphone");
                request.AddParameter("users[" + no + "][customfields][3][value]", (item.str_Cell_Phone != null ? item.str_Cell_Phone : ""));

                request.AddParameter("users[" + no + "][customfields][4][type]", "gender");
                request.AddParameter("users[" + no + "][customfields][4][value]", (item?.Gender != null ? item?.Gender.ToString() : ""));

                request.AddParameter("users[" + no + "][customfields][5][type]", "nation");
                request.AddParameter("users[" + no + "][customfields][5][value]", (item?.Nation != null ? return_nation(item?.Nation) : ""));

                request.AddParameter("users[" + no + "][customfields][6][type]", "joindate");
                request.AddParameter("users[" + no + "][customfields][6][value]", (item?.dtm_Join_Date != null ? DateUtil.DateToString(item.dtm_Join_Date.Value, "dd/MM/yyyy") : ""));

                request.AddParameter("users[" + no + "][customfields][7][type]", "jobtitle");
                request.AddParameter("users[" + no + "][customfields][7][value]", (item?.Job_Title_id != null ? item?.JobTitle?.Name : ""));

                request.AddParameter("users[" + no + "][customfields][8][type]", "partner");
                request.AddParameter("users[" + no + "][customfields][8][value]", (item?.Company_Id != null ? item?.Company?.str_Name : ""));

                request.AddParameter("users[" + no + "][customfields][9][type]", "department");
                request.AddParameter("users[" + no + "][customfields][9][value]", (item?.Department_Id != null ? item?.Department?.Name : ""));

                request.AddParameter("users[" + no + "][customfields][10][type]", "passport");
                request.AddParameter("users[" + no + "][customfields][10][value]", (item?.Passport != null ? item?.Passport : ""));

                var dataedu = _repoTrainee_Record.GetAll(a => a.Trainee_Id == item.Id);
                if (dataedu.Any())
                {
                    var noEDU = 0;
                    foreach (var itemedu in dataedu)
                    {
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][dated]", itemedu?.dtm_Created_At != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_Created_At.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][course]", itemedu?.str_Subject != null ? itemedu?.str_Subject : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][organization]", itemedu?.str_organization != null ? itemedu?.str_organization : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][note]", itemedu?.str_note != null ? itemedu?.str_note : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][fromdate]", itemedu?.dtm_time_from != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_time_from.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][educations][" + noEDU + "][todate]", itemedu?.dtm_time_to != null ? DateUtil.ConvertToUnixTime(itemedu.dtm_time_to.Value).ToString() : "");
                        noEDU++;
                    }
                    noEDU = 0;
                }
                var datacontrac = _repoTrainee_Contract.GetAll(a => a.Trainee_Id == item.Id);
                if (datacontrac.Any())
                {
                    var noCONTRAC = 0;
                    foreach (var itemcontrac in datacontrac)
                    {
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][expire_date]", itemcontrac?.expire_date != null ? DateUtil.ConvertToUnixTime(itemcontrac.expire_date.Value).ToString() : "");
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][contract_no]", itemcontrac?.contractno != null ? itemcontrac?.contractno : "");
                        request.AddParameter("users[" + no + "][contracts][" + noCONTRAC + "][description]", itemcontrac?.description != null ? itemcontrac?.description : "");
                        noCONTRAC++;
                    }
                    noCONTRAC = 0;
                }
                no++;
            }
            no = 0;
            var response = restClient.Execute<List<ServicesResultModel>>(request);

            int counterr = 0;
            foreach (var item in tUserforLms)
            {
                if (!checkserver(response, "API_USER_UPDATE", item.Id))
                {
                    counterr++;
                }
            }
            return (counterr == 0);

        }
        public bool CallLmsServices_DeleteUser(IEnumerable<Trainee> tUserforLms)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_USER_DELETE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            var no = 0;
            foreach (var item in tUserforLms)
            {
                request.AddParameter("users[" + no + "][username]", item.str_Staff_Id.ToLower());
                no++;
            }
            no = 0;
            var response = restClient.Execute<List<ServicesResultModel>>(request);


            int counterr = 0;
            foreach (var item in tUserforLms)
            {
                if (!checkserver(response, "API_USER_DELETE", item.Id))
                {
                    counterr++;
                }
            }
            return (counterr == 0);
        }
        #endregion

        #region Enroll 
        public bool CallLmsServices_EnrollUser_ONE(Course_Detail courseDetail, int approveId, TMS_Course_Member member)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_ENROLL").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);

            int no = 0;
            request.AddParameter("enrolments[" + no + "][roleid]", "trainee");
            request.AddParameter("enrolments[" + no + "][userid]", member.Trainee.str_Staff_Id.ToLower());//
            request.AddParameter("enrolments[" + no + "][courseid]", "c" + courseDetail.CourseId + "s_" + courseDetail.Id);

            int counterr = 0;
            var response = restClient.Execute<List<ServicesResultModel>>(request);
            if (!checkserver(response, "API_ENROLL", (int)courseDetail.CourseId))
            {
                counterr++;
            }




            return (counterr == 0);


        }
        public bool CallLmsServices_EnrollUser(List<Course_Detail> courseDetail, int approveId)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_ENROLL").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            var no = 0;
            TMS_Course_Member dbTMS_Course_Member;
            foreach (var item in courseDetail)
            {
                foreach (var member in item.TMS_Course_Member)
                {
                    if (member.Approve_Id == null)
                    {
                        if ((member.IsDelete == null || member.IsDelete == false))
                        {
                            no++;
                            request.AddParameter("enrolments[" + no + "][roleid]", "trainee");
                            request.AddParameter("enrolments[" + no + "][userid]", member.Trainee.str_Staff_Id.ToLower());//
                            request.AddParameter("enrolments[" + no + "][courseid]", "c" + item.CourseId + "s_" + item.Id);

                        }
                    }
                    else
                    {
                        dbTMS_Course_Member = _repoTMS_Course_Member.Get(member.Id);
                        if (member.DeleteApprove != null)
                        {
                            if (CallLmsServices_UnEnrollUser(dbTMS_Course_Member, (int)item.CourseId, item.Id))
                            {
                                _repoTMS_Course_Member.Delete(dbTMS_Course_Member);
                                _uow.SaveChanges();
                            }
                        }
                    }
                }
            }
            //no = 0;
            int counterr = 0;
            if (no != 0)
            {
                var response = restClient.Execute<List<ServicesResultModel>>(request);

                foreach (var item in courseDetail)
                {
                    if (!checkserver(response, "API_ENROLL", (int)item.CourseId))
                    {
                        counterr++;
                    }
                }
            }
            else
            {
                counterr = 0;
            }




            return (counterr == 0);


        }
        public bool CallLmsServices_UnEnrollUser(TMS_Course_Member menber, int Course_Id, int Course_Detail_Id)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_UNENROLL").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("enrolments[0][userid]", menber.Trainee.str_Staff_Id.ToLower());
            request.AddParameter("enrolments[0][courseid]", "c" + Course_Id + "s_" + Course_Detail_Id);
            var response = restClient.Execute<List<ServicesResultModel>>(request);



            return checkserver(response, "API_UNENROLL", Course_Id);



        }
        #endregion

        #region Certificate

        public bool CallLMSServices_CreateCertificate(IEnumerable<Course_Result_Final> models)
        {
            var server = _repoConfig.Get(a => a.KEY == "API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_CREATE_CERTIFICATE").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            var frist = models?.FirstOrDefault();

            request.AddParameter("certificates[0][idnumber]", frist.courseid);
            request.AddParameter("certificates[0][fullname]", frist.Course.Name ?? "");
            try
            {
                request.AddParameter("certificates[0][startdate]", DateUtil.ConvertToUnixTime((DateTime)frist.Course.StartDate));
            }
            catch (Exception)
            {

                request.AddParameter("certificates[0][startdate]", DateUtil.ConvertToUnixTime(DateTime.Now));
            }
            try
            {
                request.AddParameter("certificates[0][enddate]", DateUtil.ConvertToUnixTime((DateTime)frist.Course.EndDate));
            }
            catch (Exception)
            {

                request.AddParameter("certificates[0][enddate]", DateUtil.ConvertToUnixTime(DateTime.Now));
            }
            foreach (var item in models)
            {
                request.AddParameter("certificates[0][users][0][username]", item?.Trainee?.str_Staff_Id ?? "");
                try
                {
                    request.AddParameter("certificates[0][users][0][dateissued]", DateUtil.ConvertToUnixTime((DateTime)frist.Course.EndDate));
                }
                catch (Exception)
                {

                    request.AddParameter("certificates[0][users][0][dateissued]", DateUtil.ConvertToUnixTime(DateTime.Now));
                }
                //request.AddParameter("certificates[0][users][0][dateissued]", "1234567888");
                request.AddParameter("certificates[0][users][0][dateexpire]", "1234567888");
                request.AddParameter("certificates[0][users][0][cer_type]", "1");
                request.AddParameter("certificates[0][users][0][book_no]", item?.SRNO ?? "");
                request.AddParameter("certificates[0][users][0][ato_no]", item?.ATO ?? "");
                request.AddParameter("certificates[0][users][0][grade]", item?.grade);
            }
            IRestResponse response = restClient.Execute(request);
            return checkServer(response, "API_COURSE_CREATE", models.FirstOrDefault()?.courseid);
        }

        #endregion

        //API_COURSE_SEND_FLAG_POINT
        #region[]
        public bool CallLmsServices_Course_Send_Flag_Point(Course_Detail courseDetail, int type_flag)
        {
            /*
             Type
             - 0: chưa kết điểm
             - 1: bock để gửi approval
             - 2: Đã approval
             */
            var server = _repoConfig.Get("API_LMS_SERVER").VALUE;
            var token = _repoConfig.Get(a => a.KEY == "API_LMS_TOKEN").VALUE;
            var function = _repoConfig.Get(a => a.KEY == "API_COURSE_SEND_FLAG_POINT").VALUE;
            var moodlewsrestformat = _repoConfig.Get(a => a.KEY == "API_LMS_FORMAT").VALUE;
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);
            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);

            int no = 0;
            request.AddParameter("flag[" + no + "][flag]", type_flag);
            request.AddParameter("flag[" + no + "][courseid]", "c" + courseDetail.CourseId + "s_" + courseDetail.Id);

            int counterr = 0;
            var response = restClient.Execute<List<ServicesResultModel>>(request);
            if (!checkserver(response, "API_COURSE_SEND_FLAG_POINT", (int)courseDetail.CourseId))
            {
                counterr++;
            }




            return (counterr == 0);


        }
        #endregion


    }
}