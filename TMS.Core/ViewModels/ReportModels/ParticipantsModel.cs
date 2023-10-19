using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.ReportModels
{
    public class ParticipantsModel
    {
        public string[] header { get; set; }
        public string CourseCode { get; set; }
        public IEnumerable<CourseView> courses { get; set; }
    }
    public class CourseView
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<EmployeeView> trainees { get; set; }
    }
    public class EmployeeView
    {
        public string FullName { get; set; }
        public string str_Staff_Id { get; set; }
        public string DepartmentName { get; set; }
    }
}
