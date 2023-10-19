using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Validation;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Courses;
using TMS.Core.ViewModels.Employee;
using TMS.Core.ViewModels.Room;

namespace TMS.Core.ViewModels.Survey
{
    public class SurveyModels
    {
        public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_CODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_NAME")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_STARTDATE")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
              ErrorMessageResourceName = "VALIDATION_ENDDATE")]
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public int ModifyBy { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public Dictionary<string,string> CourseList { get; set; }
        public List<RoomModels> RoomList { get; set; }
        public List<EmployeeModelModify> InstructorList { get; set; }
        public List<TypeCourseModel> Typecourse { get; set; }

    }

    public class TypeCourseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
