using System.Configuration;

namespace TMS.Core.Utils
{
    using System;
    using System.Collections.Generic;
    using TMS.Core.App_GlobalResources;

    public static class UtilConstants
    {

        public static string PrivateLogo
        {
            get { return ConfigurationManager.AppSettings["PrivateLogo"]; }
        }
        public static string PrivateLogoLogin
        {
            get { return ConfigurationManager.AppSettings["PrivateLogoLogin"]; }
        }
        public static string PathImage
        {
            get { return ConfigurationManager.AppSettings["PathImage"]; }
        }
        public static string NotifyMessageName
        {
            get { return "notification"; }
        }
        public static string String_DeActive_Color
        {
            get { return "#9E9E9E"; }
        }
        public static string String_DeActive
        {
            get { return "DeActive"; }
        }
    


    #region [KEY string]SENDEMAILGV
    public static string KEY_TraineeTemplatePath { get { return "TraineeTemplatePath"; } } //TraineeTemplatePath
    public static string KEY_SENDEMAILGV { get { return "SENDEMAILGV"; } }
    public static string KEY_COSTPERPERSON { get { return "COSTPERPERSON"; } }
    public static string KEY_COMMITMENT { get { return "COMMITMENT"; } }
    public static string KEY_HANNAH { get { return "HANNAH"; } }
    public static string KEY_MENTOR { get { return "MENTOR"; } }
    public static string KEY_MIN_TRAINEE { get { return "MIN_TRAINEE"; } }
    public static string KEY_PREREQUISITE { get { return "REQUISITE"; } }
    public static string KEY_GROUP_TRAINEE { get { return "GROUP_TRAINEE"; } }
    public static string KEY_BLENDED { get { return "BLENDED"; } }
    public static string KEY_ATTENDANCE { get { return "ATTENDANCE"; } }
    public static string KEY_SENT_EMAIL_CHANGE_PASSWORD { get { return "SENT_EMAIL_CHANGE_PASSWORD"; } }
    public static string KEY_AUTO_ASSIGN_TRAINEE { get { return "AUTO_ASSIGN_TRAINEE"; } }
    public static string KEY_SENT_EMAIL_THAIVJ { get { return "SENT_EMAIL_THAIVJ"; } }
    public static string CERTIFICATE_COURSE_NAME { get { return "CERTIFICATE_COURSE_NAME"; } }
    public static string CERTIFICATE_FULLNAME { get { return "CERTIFICATE_FULLNAME"; } }
    public static string CERTIFICATE_DATE_COMPLELTED { get { return "CERTIFICATE_DATE_COMPLETED"; } }
    public static string CERTIFICATE_JOBTITLE_NAME { get { return "CERTIFICATE_JOBTITLE_NAME"; } }
    public static string CERTIFICATE_GRADE { get { return "CERTIFICATE_GRADE"; } }
    public static string CERTIFICATE_POINT { get { return "CERTIFICATE_POINT"; } }
    public static string CERTIFICATE_SR_NO { get { return "CERTIFICATE_SR_NO"; } }
    public static string CERTIFICATE_ATO { get { return "CERTIFICATE_ATO"; } }
    public static string CERTIFICATE_SUBJECT_NAME { get { return "CERTIFICATE_SUBJECT_NAME"; } }
    public static string CERTIFICATE_SUBJECT_DATEFROM { get { return "CERTIFICATE_SUBJECT_DATEFROM"; } }
    public static string CERTIFICATE_SUBJECT_DATETO { get { return "CERTIFICATE_SUBJECT_DATETO"; } }
    public static string CERTIFICATE_DATE_OF_BIRTH { get { return "CERTIFICATE_DATE_OF_BIRTH"; } }
    public static string CERTIFICATE_PLACE_OF_BIRTH { get { return "CERTIFICATE_PLACE_OF_BIRTH"; } }
    public static string CERTIFICATE_COURSE_DATE_FROM { get { return "CERTIFICATE_COURSE_DATE_FROM"; } }
    public static string CERTIFICATE_COURSE_DATE_TO { get { return "CERTIFICATE_COURSE_DATE_TO"; } }

    public static string CRON_USER { get { return "CRON_USER"; } }
    public static string CRON_SUBJECT { get { return "CRON_SUBJECT"; } }
    public static string CRON_JOBTITLE { get { return "CRON_JOBTITLE"; } }
    public static string CRON_TRAINEE_HISTORY { get { return "CRON_TRAINEE_HISTORY"; } }
    public static string CRON_PROGRAM { get { return "CRON_PROGRAM"; } }
    public static string CRON_COURSE { get { return "CRON_COURSE"; } }
    public static string CRON_ASSIGN_TRAINEE { get { return "CRON_ASSIGN_TRAINEE"; } }
    public static string CRON_GET_LIST_CATEGORY { get { return "CRON_GET_LIST_CATEGORY"; } }
    public static string CRON_GET_COURSE_RESULT_SUMMARY { get { return "CRON_GET_COURSE_RESULT_SUMMARY"; } }
    public static string CRON_GET_CERTIFICATE { get { return "CRON_GET_CERTIFICATE"; } }
    public static string CRON_GET_CERTIFICATE_COURSE { get { return "CRON_GET_CERTIFICATE_COURSE"; } }
    public static string CRON_GET_CERTIFICATE_CATEGORY { get { return "CRON_GET_CERTIFICATE_CATEGORY"; } }
    public static string CRON_GET_POSTNEWS { get { return "CRON_GET_POSTNEWS"; } }
    public static string CRON_GET_CATEGORY_POSTNEWS { get { return "CRON_GET_CATEGORY_POSTNEWS"; } }
    public static string CRON_DEPARTMENT { get { return "CRON_DEPARTMENT"; } }
    public static string CRON_SURVEY { get { return "CRON_GET_SURVEY_GLOBAL"; } }
    public static string CRON_POST_SURVEY { get { return "CRON_POST_SURVEY"; } }
    //CODE MAIL
    public static string KEY_CORE_START { get { return "pKmV"; } }
    public static string KEY_CORE_END { get { return "XlIz"; } }
    public static string CRON_POST_RESULT { get { return "CRON_POST_RESULT_"; } }
    public static string CERTIFICATE_TRAINEE_AVATAR { get { return "CERTIFICATE_TRAINEE_AVATAR"; } }

    #endregion

    #region[Key webservice]
    public static string core_user_get_users { get { return "core_user_get_users"; } }
    #endregion

    #region domain tms

    public static string TMS_Cookie { get { return ConfigurationManager.AppSettings["TMS_Cookie"]; } }

    #endregion

    public enum ScheduleType
    {
        Repeat = 0,//lập lại số giây
        SetCalendar,//đặt lịch
        Periodic,//định kỳ
    }


    public enum UserType
    {
        UserSystem = 0,
        Employee
    }

    public static Dictionary<int, string> UserTypeDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)UserType.UserSystem,UserType.UserSystem.ToString() },
               {(int)UserType.Employee,UserType.Employee.ToString() },
           };
    }

    public static Dictionary<int, string> ScheduleTypeDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)ScheduleType.Repeat,ScheduleType.Repeat.ToString() },
               {(int)ScheduleType.SetCalendar,ScheduleType.SetCalendar.ToString() },
                {(int) ScheduleType.Periodic,ScheduleType.Periodic.ToString()}
           };
    }

    public static Dictionary<int, string> DateOfWeekDictionary()
    {

        return new Dictionary<int, string>()
           {
               {(int)DayOfWeek.Monday,DayOfWeek.Monday.ToString() },
               {(int)DayOfWeek.Tuesday,DayOfWeek.Tuesday.ToString() },
               {(int)DayOfWeek.Wednesday,DayOfWeek.Wednesday.ToString() },
               {(int)DayOfWeek.Thursday,DayOfWeek.Thursday.ToString() },
               {(int)DayOfWeek.Friday,DayOfWeek.Friday.ToString() },
               {(int)DayOfWeek.Saturday,DayOfWeek.Saturday.ToString() },
               {(int)DayOfWeek.Sunday,DayOfWeek.Sunday.ToString() },
           };
    }


    public static Dictionary<int, string> ScheduleMethodDictionary()
    {
        return new Dictionary<int, string>()
           {
               //{(int)ScheduleMethod.System,ScheduleMethod.System.ToString() },
               {(int)ScheduleMethod.Notification,ScheduleMethod.Notification.ToString() },
                {(int) ScheduleMethod.Mail,ScheduleMethod.Mail.ToString()},
                 {(int) ScheduleMethod.Sms,ScheduleMethod.Sms.ToString()}
           };
    }
    public static Dictionary<int, string> ScheduleTimeMarkDictionary()
    {
        return new Dictionary<int, string>()
           {
               //{(int)ScheduleMethod.System,ScheduleMethod.System.ToString() },
               //{(int)ScheduleTimeMark.Second,ScheduleTimeMark.Second.ToString() },
                {(int) ScheduleTimeMark.Hour,ScheduleTimeMark.Hour.ToString()},
                 {(int) ScheduleTimeMark.Day,ScheduleTimeMark.Day.ToString()},
                 {(int) ScheduleTimeMark.Month,ScheduleTimeMark.Month.ToString()}
           };
    }
    public enum ScheduleTimeMark
    {
        //Second = 0,//giây
        Hour,//giờ
        Day,//ngày
        Month,//tháng
    }
    public enum ScheduleMethod
    {
        System = 0,//lập lại
        Notification,//đặt lịch
        Mail,//ngay bây giờ
        Sms,//định kỳ
    }

    public static readonly string[] VietNamChar = {
        "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ"
        };

    public static readonly string[] ImageExtensions = new[]
    {
           ".jpg",".jpeg",".exif",".tiff",".gif",".bmp",".png",".ppm",".pgm",".pbm",".pnm",
        };

    public enum Upload
    {
        Trainee,
        Education
    }


    public enum Switch
    {
        Horizontal = 0,
        Vertical
    }
    public enum TypePostNew
    {
        Notification = 0,
        News,
        Welcome
    }


    public enum ProcessStep
    {
        Empty = 0,
        Course = 1,
        AssignedTrainee = 2,
        SubjectResult = 3,
        CourseResult = 4,
    }
    public enum ProcessStepNotEdit
    {
        Course = 1,
        AssignedTrainee = 2,
        SubjectResult = 3,
        CourseResult = 4,
    }

    public enum TypeSentEmail
    {
        //user,program,list course
        SentMailApprovedCourse = 0,         //0
        SentMailApprovedGV,                 //1
        SentMailApprovedTeachingAssistant,  //2
                                            //user,program,list course          //
        SentMailRejectCourse,               //3
        SentMailRejectGV,                   //4
        SentMailRejectMantor,               //5
        SentMailRejectHannah,               //6
                                            //user,program,trainee              //
        SentMailApproveFinalCourse,         //7
        SentMailApproveFinalGV,             //8
        SentMailApproveFinalMantor,         //9
        SentMailApproveFinalHannah,         //10
                                            //user,trainee,program,list course  //
        SentMailAssignTrainee,              //11
        SentMailAssignTraineeLMS,           //12
        SentMailCancelRequest,              //13
                                            //user,                             //
        SentMailPasswordUser,               //14
                                            //trainee                           //
        SentMailPasswordEmp,                //15
        SentMailApprovedHannal,             //16
        SentMailApproveFinalHannah_User,    //17
        SentMailApprovedHannal_User,        //18
        SentMailRejectHannah_User,          //19
        SentMailMeeting,                    //20
                                            //Schedule course missing           //
        SendMailScheduleCourseMissing,      //21
                                            //Schedule report trainingplan      //
        SendMailScheduleReportTrainingPlan, //22
        SendMailReminderLogin,              //23
        SendMailApproveToMail,           //24
        SendMailReminderCourse,         //25   
        SendMailCreateTrainee,  //26
        SendMailCreateInstructor,
        SendMailReminderFinalCourse,//27
        }
    public enum TypeSentEmailAttachFileType
    {
        ReportTrainingPlan
    }
    public enum ActionTypeSentmail
    {
        ApprovedProgram,
        AssignTrainee,
        ApprovedFinalProgram,
        CreatePasswordUser,
        CreatePasswordEmployee,
        Reject,
        CancelRequest,
        SendMailApprove,
        CreateInstructor_Trainee
    }
    public enum StatusApiApprove
    {
        Pending = 0,
        Approved,
        Reject
    }

    public enum UserStateConstant
    {
        Offline = 0,
        Online = 1,
        OnlineAndOffline = 2
    }
    public enum StatusScheduler
    {
        Synchronize = 0,
        Modify,
    }

    public enum StatusTraineeHistory
    {
        Missing = 0,
        Trainning,
        List,
        Completed,
    }

    public enum TypeInstructor
    {
        Instructor = 0,
        Mentor, //code mới VJ Upgrade tương đương monitor
        Hannah, // code mới VJ Upgrade tưởng đương examiner
    }
    public enum StatusApiDHDaNang
    {
        Null = 0,
        Undefined,
        Insert,
        Modify,
        UsernameIsNull,
        PasswordIsNull,

    }

    public enum TypeAssign
    {
        Assigned = 0,
        Unassigned
    }

    public enum Attendance
    {
        Present = 0,
        Absent,
        Late,
        Undefined
    }




    public static Dictionary<int, string> AttendanceDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)Attendance.Present,"Present" },
               {(int)Attendance.Absent,"Absent" },
                {(int) Attendance.Late,"Late"}
           };
    }

    public enum TypeCheck
    {
        Unchecked = 0,
        Checked
    }
    public enum NotificationType
    {
        AutoProcess = 0,
        Department = 1,
        Trainee = 2
    }
    public enum MenuType
    {
        System = 0,
        TraningCenter = 1,
        Recruitment = 2
    }

    //User
    public static string MAIL_USER_USERNAME { get { return "MAIL_USER_USERNAME"; } }
    public static string MAIL_USER_FULLNAME { get { return "MAIL_USER_FULLNAME"; } }
    public static string MAIL_USER_PASSWORD { get { return "MAIL_USER_PASSWORD"; } }
    public static string MAIL_USER_EMAIL { get { return "MAIL_USER_EMAIL"; } }
    public static string MAIL_USER_PHONE { get { return "MAIL_USER_PHONE"; } }
    //Trainee
    public static string MAIL_TRAINEE_USERNAME { get { return "MAIL_TRAINEE_USERNAME"; } }
    public static string MAIL_TRAINEE_PASSWORD { get { return "MAIL_TRAINEE_PASSWORD"; } }
    public static string MAIL_TRAINEE_FULLNAME { get { return "MAIL_TRAINEE_FULLNAME"; } }
    public static string MAIL_TRAINEE_EMAIL { get { return "MAIL_TRAINEE_EMAIL"; } }
    public static string MAIL_TRAINEE_PHONE { get { return "MAIL_TRAINEE_PHONE"; } }
    public static string MAIL_TRAINEE_GRADE { get { return "MAIL_TRAINEE_GRADE"; } }
    //Program
    public static string MAIL_PROGRAM_NAME { get { return "MAIL_PROGRAM_NAME"; } }
    public static string MAIL_PROGRAM_CODE { get { return "MAIL_PROGRAM_CODE"; } }
    public static string MAIL_PROGRAM_STARTDATE { get { return "MAIL_PROGRAM_STARTDATE"; } }
    public static string MAIL_PROGRAM_ENDDATE { get { return "MAIL_PROGRAM_ENDDATE"; } }
    public static string MAIL_PROGRAM_VENUE { get { return "MAIL_PROGRAM_VENUE"; } }
    public static string MAIL_PROGRAM_MAXTRAINEE { get { return "MAIL_PROGRAM_MAXTRAINEE"; } }
    public static string MAIL_PROGRAM_NOTE { get { return "MAIL_PROGRAM_NOTE"; } }
    public static string MAIL_CODE { get { return "MAIL_CODE"; } }
    public static string MAIL_LIST { get { return "MAIL_LIST"; } }
    //Course
    public static string MAIL_LIST_COURSE { get { return "MAIL_LIST_COURSE"; } }



    public enum NotificationStatus
    {
        Show = 0,
        Hidden = 1
    }
    public enum ROLE
    {
        Instructor = 1,
        Trainee = 2,
    }
    public enum CourseAreas
    {
        External = 0,
        Internal = 1,
        
    }
    public enum CourseResultFinalStatus
    {
        Removed = 0,
        Pending = 1,
        Marked
    }
    public static Dictionary<int, string> CourseCourseAreasDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)CourseAreas.Internal, Resource.lblInternal },
               {(int)CourseAreas.External,Resource.lblExternal },
           };
    }
    public static Dictionary<int, string> SearchEmployee()
    {
        return new Dictionary<int, string>()
            {
                {
                    (int) TypeInstructor.Mentor,
                    TypeInstructor.Mentor.ToString()
                },
                {
                    (int) TypeInstructor.Hannah,
                    TypeInstructor.Hannah.ToString()
                }
            };
    }

    public enum ScheduleStatus
    {
        InProcess = 0,
        Upcoming = 1

    }

    public enum LearningTypes
    {

        Offline = 0,
        Online = 1,
        OfflineOnline = 2,
        Off
    }
    public static Dictionary<int, string> LearningTypesDictionary()
    {
        return new Dictionary<int, string>()
            {
               {(int)LearningTypes.Offline,"Classroom" },
               {(int)LearningTypes.Online,"Online" },
               {(int)LearningTypes.OfflineOnline,"cRo" },
            };
    }
    public static Dictionary<int, string> LearningTypesDictionaryMail()
    {
        return new Dictionary<int, string>()
        {
            {(int)LearningTypes.Offline,"Class Room Learning" },
            {(int)LearningTypes.Online,"e-Learning" },
            {(int)LearningTypes.OfflineOnline,"Class Room And Online" },
        };
    }
    public enum MarkTypes
    {
        Manual = 0,
        Auto = 1,

    }
    public static Dictionary<int, string> MarkTypesDictionary()
    {
        return new Dictionary<int, string>()
            {
               {(int)MarkTypes.Manual,"Manual" },
               {(int)MarkTypes.Auto,"Auto" },
            };
    }

    public enum AttemptsAllowed
    {
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten
    }
    public enum ConfigSite
    {
        UsingPermissionData = 1
    }
    public static Dictionary<int, string> AttemptsAllowedDictionary()
    {
        return new Dictionary<int, string>()
            {
                {(int)AttemptsAllowed.One,"1" },
                {(int)AttemptsAllowed.Two,"2" },
                {(int)AttemptsAllowed.Three,"3" },
                {(int)AttemptsAllowed.Four,"4" },
                {(int)AttemptsAllowed.Five,"5" },
                {(int)AttemptsAllowed.Six,"6" },
                {(int)AttemptsAllowed.Seven,"7" },
                {(int)AttemptsAllowed.Eight,"8" },
                {(int)AttemptsAllowed.Nine,"9" },
                {(int)AttemptsAllowed.Ten,"10" },
            };
    }
    public enum GradingMethod
    {
        HighestGrade = 1,
        AverageGrade,
        FirstAttempt,
        LastAttempt
    }
    public static Dictionary<int, string> GradingMethodDictionary()
    {
        return new Dictionary<int, string>()
            {
                {(int)GradingMethod.HighestGrade,Resource.lblHighestGrade},
                {(int)GradingMethod.AverageGrade,Resource.lblAverageGrade },
                {(int)GradingMethod.FirstAttempt,Resource.lblFirstAttempt},
                {(int)GradingMethod.LastAttempt,Resource.lblLastAttempt}
            };
    }
    public enum CourseTypes
    {
        Initial = 1,
        Recurrent,
        ReQualification,
        Upgrade,
        Bridge,
        General,

    }
    public static Dictionary<int, string> CourseTypesDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)CourseTypes.Initial,"Initial" },
               {(int)CourseTypes.Recurrent,"Recurrent" },
               {(int)CourseTypes.ReQualification,"Re - Qualification" },
               {(int)CourseTypes.Upgrade,"Upgrade" },
               {(int)CourseTypes.Bridge,"Bridge" },
               {(int)CourseTypes.General,"General" },
           };
    }
    public enum Gender
    {
        Male = 1,
        Female,
        Others
    }
    public static Dictionary<int, string> GenderDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)Gender.Male, Resource.lblMale },
               {(int)Gender.Female,Resource.lblFemale },
               {(int)Gender.Others,Resource.lblOthers },
           };
    }

    /*
    public enum MenuType
    {
        List = 1,
        Modify,
        Delete,
        Export
    }
    */
    public static string Limitdate()
    {
        var Limitdate = DateTime.Now.AddMinutes(10);
        return Limitdate.ToString();
    }
    public enum ROLE_FUNCTION
    {
        FullOption = 0,
        View,
        CreateEdit,
        Delete,
        Export,
    }
    public static List<int> ActionRoles
    {
        get
        {
            return new List<int>()
        {
            (int)ROLE_FUNCTION.View,
            (int)ROLE_FUNCTION.CreateEdit,
            (int)ROLE_FUNCTION.Delete,
            (int)ROLE_FUNCTION.Export,
        };
        }
    }
    public enum BoolEnum
    {
        Yes = 1,
        No = 0
    }

    public static Dictionary<int, string> YesNoDictionary()
    {
        return new Dictionary<int, string>()
            {{(int) BoolEnum.No, BoolEnum.No.ToString()},{(int) BoolEnum.Yes, BoolEnum.Yes.ToString()}};
    }
    public static Dictionary<BoolEnum, string> ActiveStatusDictionary()
    {
        return new Dictionary<BoolEnum, string>()
            {{BoolEnum.No, Resource.lblDeActive}, {BoolEnum.Yes, Resource.lblActive}};
    }
    public enum ApproveType
    {
        Course = 1,
        AssignedTrainee = 2,
        SubjectResult = 3,
        CourseResult = 4,
    }
    public static Dictionary<int, string> ApproveTypeDictionary()
    {
        return new Dictionary<int, string>()
            {
                {(int)ApproveType.Course      ,ApproveType.Course.ToString()     },
                {(int)ApproveType.AssignedTrainee    ,"AssignTrainee"  },
                {(int)ApproveType.SubjectResult   ,ApproveType.SubjectResult.ToString()     },
                {(int)ApproveType.CourseResult    ,ApproveType.CourseResult.ToString()     },
            };
    }
    public enum EStatus
    {
        Draft = 1,
        Approve = 2,// sử dụng
        Complete = 3,
        Pending = 4,// sử dụng
        Reject = 5,// sử dụng
        Block = 6,// sử dụng
        Processing = 7,
        UnProcess = 8,
        CancelRequest = 9,// sử dụng
        EmptyTrainee,
        Empty
    }
    public static Dictionary<int, string> EStatusTypeDictionary()
    {
        return new Dictionary<int, string>()
            {
                //{(int)EStatus.Draft      ,EStatus.Draft.ToString()     },
                {(int)EStatus.Approve    ,EStatus.Approve.ToString()  },
                //{(int)EStatus.Complete   ,EStatus.Complete.ToString()     },
                {(int)EStatus.Pending    ,EStatus.Pending.ToString()     },
                 {(int)EStatus.Reject    ,EStatus.Reject.ToString()     },
                  {(int)EStatus.Block    ,EStatus.Block.ToString()     },
                   {(int)EStatus.CancelRequest    ,EStatus.CancelRequest.ToString()     },
            };
    }

    public enum APIAssign
    {
        Approved = 0,
        Pending = 1
    }

    public enum ApiStatus
    {
        Synchronize = 0,
        Modify = 1,
        UnSuccessfully = 2,
        NoResponse = 3,
        AddNewTMS = 99,

    }
    public enum LMSStatus
    {
        Synchronize = 0,
        Course = 1,
        AssignTrainee = 2,
        Result = 3,
        Final = 4,
        Finish = 5
    }
    public enum LogType
    {
        EVENT_TYPE_ERROR = 0,
        EVENT_TYPE_INFORMATION = 1,
        EVENT_TYPE_WARNING = 2
    }
    public enum LogSourse
    {
        Role = 0,
        USER = 1,
        Course = 2,
        CourseCost = 3,
        Approval = 4,
        CourseResult = 5,
        AssignTrainee,
        SendMail,
        Certificate,
        Contract,
        Contractor,
        Cost,
        Note


    }
    public enum LogEvent
    {
        Insert = 0,
        Update = 1,
        Delete = 2,
        Active = 3,
        View = 4,
        Error,
        CancelRequest
    }
    public static Dictionary<int, string> LogEventDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)LogEvent.Insert,"Insert" },
               {(int)LogEvent.Update,"Update" },
               {(int)LogEvent.Delete,"Delete" },
               {(int)LogEvent.Active,"Active" },
                {(int) LogEvent.View,"View"}
           };
    }
    public enum TotalCount
    {
        Paticipant = 1,
        Distinction = 2,
        Pass = 3,
        Certificate = 4
    }

    public enum SwitchResult
    {
        Point = 0,
        Grade
    }

    public enum Grade
    {
        Fail = 0,
        Pass = 1,
        Distinction = 3,
        Certificate = 4,
        Paticipant = 2
    }
    public static Dictionary<int, string> GradeDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)Grade.Fail,Grade.Fail.ToString() },
               {(int)Grade.Pass,Grade.Pass.ToString() },
                {(int)Grade.Distinction,Grade.Distinction.ToString() }
           };
    }
    public static Dictionary<int, string> ResultDictionary()
    {
        return new Dictionary<int, string>()
           {
               {(int)Grade.Fail,Grade.Fail.ToString() },
               {(int)Grade.Pass,Grade.Pass.ToString() }
           };
    }
    public static Dictionary<int, string> ResultDictionaryVJ()
    {
        return new Dictionary<int, string>()
           {
               {(int)Grade.Fail,"F" },
               {(int)Grade.Pass,"P" }
           };
    }
    public enum MarkInTMS
    {
        No = 0,
        Yes = 1
    }
    public enum DetailResult
    {
        Score = 0,
        Grade = 1,
        Remark = 2,
        IsAverageCalculate
    }


    public static Dictionary<int, string> StatusDictionary()
    {
        return new Dictionary<int, string>()
            {
                {(int)EStatus.Draft      ,EStatus.Draft.ToString()     },
                {(int)EStatus.Approve    ,EStatus.Approve.ToString()     },
                {(int)EStatus.Complete   ,EStatus.Complete.ToString()     },
                {(int)EStatus.Pending    ,EStatus.Pending.ToString()     },
                {(int)EStatus.Reject     ,EStatus.Reject.ToString()     },
                {(int)EStatus.Block      ,EStatus.Block.ToString()     },
                {(int)EStatus.Processing ,EStatus.Processing.ToString()     },
                { (int)EStatus.UnProcess  ,EStatus.UnProcess.ToString()     },
                 { (int)EStatus.CancelRequest  ,EStatus.CancelRequest.ToString()     },
            };
    }


    public enum ActionType
    {
        Request = 0,
        Approve
    }


    public enum UserActive
    {
        On = 1,
        Off = 0
    }
    public enum status_report_training
    {
        complete = 1,
        Draft = 2,
        inprocess = 0
    }


    public static Dictionary<int, status_report_training> ReportCourseStatusDictionary => new Dictionary<int, status_report_training>()
        {
            { (int)status_report_training.inprocess,status_report_training.inprocess},
            {(int)status_report_training.complete,status_report_training.complete},
        };



    public class NotificationTemplate
    {
        public const string
            CancelRequest = "Request reject the course.",
            Request_Course = "Request for course approval.",
            Request_AssignTrainee = "Request for trainee assignment approval.",
            Request_SubjectResult = "Request for subject result approval.",
            Request_CourseResult = "Request for final course result approval.",
            Approval_Course = "Course approved.",
            Approval_AssignTrainee = "Trainee assignment approved.",
            Approval_SubjectResult = "Subject Result approved.",
            Approval_CourseResult = "Final course result approved.",
            Reject_Course = "Course rejected.",
            Reject_AssignTrainee = "Trainee assignment rejected.",
            Reject_SubjectResult = "Subject Result rejected.",
            Reject_CourseResult = "Final course result rejected.",
            UnBlockCourse = "Course is unblocked.",
            UnBlockAssignTrainee = "Trainee assignment is unblocked.",
            UnBlockSubjectResult = "Subject result is unblocked.",
            UnBlockFinal = "Final course is unblocked.",

            CancelRequestVN = "Yêu cầu hủy khóa học",
            Request_Course_VN = "Yêu cầu phê duyệt khóa học.",
            Request_AssignTrainee_VN = "Yêu cầu phê duyệt chấp nhận học viên.",
            Request_SubjectResult_VN = "Yêu cầu phê duyệt kết quả môn học",
            Request_CourseResult_VN = "Yêu cầu phê duyệt kết quả tổng kết khóa học.",
            Approval_Course_VN = "Khóa học đã được phê duyệt.",
            Approval_AssignTrainee_VN = "Học viên đã được phê duyệt.",
            Approval_SubjectResult_VN = "Kết quả môn học đã được phê duyệt.",
            Approval_CourseResult_VN = "Kết quả tổng kết khóa học đã được phê duyệt.",
            Reject_Course_VN = "Khóa học đã bị từ chối.",
            Reject_AssignTrainee_VN = "Học viên đã bị từ chối.",
            Reject_SubjectResult_VN = "Kết quả môn học đã bị từ chối.",
            Reject_CourseResult_VN = "Kết quả tổng kết khóa học đã bị từ chối.",
            UnBlockCourseVn = "Khóa học đã mở khóa.",
            UnBlockAssignTraineeVn = "Gán học viên đã mở khóa.",
            UnBlockSubjectResultVn = "Điểm Môn học đã mở khóa.",
            UnBlockFinalVn = "Điểm Khóa học đã mở khóa.";
    }
    public class NotificationContent
    {
        public const string
            // gửi lên HOD
            CancelRequest = "The course '{0}' needs to be rejected. Note: '{1}' .",
            Request_Course = "The course '{0}' needs to be approved. Note: '{1}' .",
            Request_AssignTrainee = "The trainee assignment for the course '{0}' needs to be approved. Note: '{1}' .",
            Request_SubjectResult = "The subject result {0} for the course '{1}' needs to be approved. Note: '{2}' .",
            Request_CourseResult = "The final course result for the course '{0}' needs to be approved. Note: '{1}' .",
            // HOD gửi về
            Approval_Course = "Your request to update the course '{0}' has been approved. Note: '{1}' .",
            Approval_AssignTrainee = "Your request to update the trainee list for the course '{0}' has been approved. Note: '{1}' .",
            Approval_SubjectResult =
                "Your request to update subject result '{0}' for the course '{1}' has been approved. Note: '{1}' .",
            Approval_CourseResult = "Your request to update the final course result '{0}' has been approved. Note: '{1}' .",
            Reject_Course = "Your request to update the course '{0}' has been reject. Note: '{1}' .",
            Reject_AssignTrainee = "Your request to update the trainee list for the course '{0}' has been reject. Note: '{1}' .",
            Reject_SubjectResult = "Your request to update subject result '{0}' for the course '{1}' has been reject. Note: '{2}' .",
            Reject_CourseResult = "Your request to update the final course result '{0}' has been reject. Note: '{1}' .",

            UnBlockCourse = "The course '{0}' is unblocked, you can modify. Note: '{1}' .",
            UnBlockAssign = "The trainee assignment for course '{0}' is unblocked, you can modify. Note: '{1}' .",
            UnBlockSubjectResult = "The Subject Result '{0}' for course '{1}' is unblocked, you can modify. Note: '{2}' .",
            UnBlockFinal = "The final course result for course '{0}' is unblocked, you can modify. Note: '{1}' .",


            CancelRequestVN = "Khóa học '{0}' cần được hủy. Ghi chú: '{1}' .",
            Request_Course_VN = "Khóa học '{0}' cần được phê duyệt. Ghi chú: '{1}' .",
            Request_AssignTrainee_VN = "Học viên gán cho khóa học '{0}' cần được phê duyệt. Ghi chú: '{1}' .",
            Request_SubjectResult_VN = "Kết quả môn học '{0}' cho khóa học '{1}' cần được phê duyệt. Ghi chú: '{2}' .",
            Request_CourseResult_VN = "Kết quả tổng kết khóa học của khóa học '{0}' cần được phê duyệt. Ghi chú: '{1}' .",
            // HOD gửi về
            Approval_Course_VN = "Yêu cầu cập nhật khóa học '{0}' đã được phê duyệt. Ghi chú: '{1}' .",
            Approval_AssignTrainee_VN = "Yêu cầu nhật danh sách học viên của khóa học '{0}' đã được phê duyệt. Ghi chú: '{1}' .",
            Approval_SubjectResult_VN = "Yêu cầu cập nhật kết quả môn học '{0}' cho khóa học '{1}' đã được phê duyệt. Ghi chú: '{2}' .",
            Approval_CourseResult_VN = "Yêu cầu cập nhật kết quả tổng kết khóa học '{0}' đã được phê duyệt. Ghi chú: '{1}' .",
            Reject_Course_VN = "Yêu cầu cập nhật khóa học '{0}' đã bị từ chối. Ghi chú: '{1}' .",
            Reject_AssignTrainee_VN = "Yêu cầu nhật danh sách học viên của khóa học '{0}' đã bị từ chối. Ghi chú: '{1}' .",
            Reject_SubjectResult_VN = "Yêu cầu cập nhật kết quả môn học '{0}' for the course '{1}' đã bị từ chối. Ghi chú: '{2}' .",
            Reject_CourseResult_VN = "Yêu cầu cập nhật kết quả tổng kết khóa học '{0}' đã bị từ chối. Ghi chú: '{1}' .",

            UnBlockCourseVn = "Khóa học '{0}' đã mở khóa , bạn có thể chỉnh sửa. Ghi chú: '{1}' . ",
            UnBlockAssignVn = "Gán học viên của khóa '{0}' đã mở khóa , bạn có thể chỉnh sửa. Ghi chú: '{1}' .",
            UnBlockSubjectResultVn = "Kết quả môn '{0}' của khóa '{1}' đã mở khóa, bạn có thể chỉnh sửa. Ghi chú: '{2}' .",
            UnBlockFinalVn = "Kết quả tổng của khóa '{0}',bạn có thể chỉnh sửa. Ghi chú: '{1}' .";

    }




    public enum Currency
    {
        VND = 0,
        USD,
        GBP
        //THB
    }
    public static Dictionary<int, string> CurrencyDictionary()
    {
        return new Dictionary<int, string>(){
            {(int)Currency.VND,"VND"},
            {(int)Currency.USD,"USD"},
             {(int)Currency.GBP,"GBP"},
            //{(int)Currency.THB,"THB"}
            };
    }

    public enum KeySend
    {
        ApproveCourse = 0,
        RejectApproveCourse,
        ApproveAssignTrainee,
        ApproveCourseResult,
        CourseCancelRequest,
        CourseCreate,
        RequestApproveCourse,
        LMSAssign,
        CourseAssignTrainee
    }

    public enum PaymentStatus
    {
        NoPayment = 1,
        Paid = 2,
        Pending = 3
    }
    public enum RequestOrApproval
    {
        Request,
        Approval
    }

}
}
