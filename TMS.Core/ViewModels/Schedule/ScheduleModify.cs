using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Schedule
{
    public class ScheduleModify
    {
        public int? Id { get; set; }

        public string IsAll { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
                ErrorMessageResourceName = "VALIDATION_SCHEDULE_NAME")]
        public string Name { get; set; }
        public string Content { get; set; }
        public Dictionary<int, string> Methods { get; set; }

        public Dictionary<int, string> Types { get; set; }
        public Dictionary<int, string> DayOfWeek { get; set; }
        public Dictionary<int, string> UserTypes { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_SCHEDULE_SENDTO")]
        public int UserTypeId { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_SCHEDULE_METHOD")]
        public int MethodId { get; set; }

        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_SCHEDULE_TYPE")]
        public int TypeId { get; set; }

        public int TimeRepeat { get; set; } //tính bằng giây
        public int TimeRemind { get; set; }
        public string Catmail_code { get; set; }
        public DateTime? DateCalendar { get; set; }

        public string TimeCalendar { get; set; } //tính bằng giờ

        public string TimePeriodic { get; set; }  //tính bằng giờ
        public int[] ListDay { get; set; }
        public int[] DayValues { get; set; }
        //public Dictionary<int, string> Departments { get; set; }
        public string Departments { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public Dictionary<int, string> GroupTrainees { get; set; }
        public Dictionary<int, string> GroupUsers { get; set; }
        public Dictionary<int, string> CourseList { get; set; }
        public int? CourseListID { get; set; }
        public int? GroupUsersID { get; set; }
        public int? JobTitlesID { get; set; }
        public int? GroupTraineesID { get; set; }

        public Dictionary<int, string> Prerequisite { get; set; }
        public string CheckPrerequisite { get; set; } = "PRE-REQUISITE";
        public string CheckGroupTrainee { get; set; } = "GROUP_TRAINEE";
        public IEnumerable<GetUser> GetUsers { get; set; }

        public string NUserId { get; set; }
        public string NEmployeeId { get; set; }

        public Dictionary<int, string> TemplateMails { get; set; }
        public int? TemplateId { get; set; }
        public string TeamplateContent { get; set; }

        public Dictionary<int, string> TimeMark { get; set; }
        public int? TimeMarkId { get; set; }
        public class GetUser
        {
            public int? Id { get; set; }
            public string FullName { get; set; }
            public string FirstName { get; set; }
            public string Department { get; set; }

        }


    }
}
