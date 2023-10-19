using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.API.Utilities
{
    public class Constants
    {
        public enum Currency
        {
            VND =0,
            USD
        }
        protected static Dictionary<int, string> CurrencyDictionary = new Dictionary<int, string>
        {
            {(int)Currency.VND,"VND"},
            {(int)Currency.USD,"USD"},
        };
        public enum Bool
        {
            True = 1,
            False = 0
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

        protected static Dictionary<int, status_report_training> ReportCourseStatusDictionary => new Dictionary<int, status_report_training>()
        {
            { (int)status_report_training.inprocess,status_report_training.inprocess},
            {(int)status_report_training.complete,status_report_training.complete},
        };

        public enum UserStateConstant
        {
            Offline = 0,
            Online = 1,
            OnlineAndOffline = 2
        }
        public enum Language
        {
            Vietnamese = 0,
            English = 1
        }
        public enum EStatus
        {
            Draft = 1,
            Approve = 2,
            Complete = 3,
            Pending = 4,
            Reject = 5,
            Block = 6,
            Processing = 7,
            UnProcess = 8
        }
        protected static Dictionary<int, string> EStatusDictionary = new Dictionary<int, string>
        {
            {(int)EStatus.Draft        , "Draft" },
            {(int)EStatus.Approve      , "Approve" },
            {(int)EStatus.Complete     , "Complete" },
            {(int)EStatus.Pending      , "Pending" },
            {(int)EStatus.Reject       , "Reject" },
            {(int)EStatus.Block        , "Block" },
            {(int)EStatus.Processing   , "Processing" },
        };
        public class NotificationTemplate
        {
            public const string
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
                Reject_CourseResult = "Final course result rejected.";
        }
        public class NotificationContent
        {
            public const string
            // gửi lên HOD
            Request_Course = "The course {0} needs to be approved.",
            Request_AssignTrainee = "The trainee assignment for the course {0} needs to be approved.",
            Request_SubjectResult = "The subject result {0} for the course {1} needs to be approved.",
            Request_CourseResult = "The final course result for the course {0} needs to be approved.",
            // HOD gửi về
            Approval_Course = "Your request to update the course {0} has been approved.",
            Approval_AssignTrainee = "Your request to update the trainee list for the course {0} has been approved.",
            Approval_SubjectResult = "Your request to update subject result {0}   for the course {1} has been approved.",
            Approval_CourseResult = "Your request to update the final course result {0} has been approved.",
            Reject_Course = "Your request to update the course {0} has been reject.",
            Reject_AssignTrainee = "Your request to update the trainee list for the course {0} has been reject.",
            Reject_SubjectResult = "Your request to update subject result {0}   for the course {1} has been reject.",
            Reject_CourseResult = "Your request to update the final course result {0} has been reject.";

        }
        public enum ApproveType
        {
            Course = 1,
            AssignedTrainee = 2,
            SubjectResult = 3,
            CourseResult = 4,
        }

        public enum Type
        {
            Class = 0,
            ELearn = 1,
            ClassElearn = 2,
        }
        public enum ScheduleStatus
        {
            InProcess = 0,
            Upcoming = 1
        }

        public enum StaffRole
        {
            Instructor = 1,
            Trainee = 2,
        }
        public enum Rate
        {
            Excellent = 1,
            Good = 2,
            Fair = 3,
            Need = 4
        }
        public enum ROLE
        {
            Instructor = 1,
            Trainee = 2
        }
        public enum ROLE_FUNCTION
        {
            FullOption = 1,
            View = 2,
            Create = 3,
            Edit = 4,
            Delete = 5,
            Active_Deactive = 6
        }


        public enum CourseType
        {
            Initial = 1,
            Recurrent = 2,
            ReQualification = 3,
            Upgrade = 4,
            Bridge = 5,
            General = 6
        }
    }
}