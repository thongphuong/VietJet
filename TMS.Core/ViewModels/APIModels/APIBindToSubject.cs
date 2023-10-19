using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIBindToSubject
    {
        public string Course_id { get; set; } //code khóa học hiện tại
        public IEnumerable<APISubjectItem> Required_course_id { get; set; } //code của khóa học điều kiện cho khóa học hiện tại
        public string Complete_status_course_id { get; set; } //trạng thái hoàn thành của khóa học điều kiện 
        public class APISubjectItem
        {
            public string codeSubject { get; set; }
            public string nameSubject { get; set; }
            public APISubjectItem(string code,string name)
            {
                this.codeSubject = code;
                this.nameSubject = name;
            }
        }
        public APIBindToSubject(string subjectcode, List<Course_Subject_Item> listsubjectrequired, string status)
        {
            this.Course_id = !string.IsNullOrEmpty(subjectcode) ? subjectcode : string.Empty;
            this.Required_course_id = listsubjectrequired.Select(a => new APISubjectItem(a?.SubjectDetail?.Code, a?.SubjectDetail?.Name));
            this.Complete_status_course_id = status != "" ? status : string.Empty;
        }
    }
}
