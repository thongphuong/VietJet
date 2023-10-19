using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.Employee
{
    public class EmployeeModelDetails
    {
        public int Control { get; set; }
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string Eid { get; set; }
        public string PersonId { get; set; }
        public string Passport { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string Type { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string Jobtitle { get; set; }
        public string Nation { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string DateOfJoin { get; set; }
        public string ResignationDate { get; set; }
        public bool CheckHannahMentor { get; set; }
        public string HannahMentor { get; set; }
    }
    public class EmployeeModelPoint
    {
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string Eid { get; set; }
        public string PersonId { get; set; }
        public string Passport { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string Type { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string Jobtitle { get; set; }
        public string Nation { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string DateOfJoin { get; set; }
        public string ResignationDate { get; set; }
        public bool CheckHannahMentor { get; set; }
        public string HannahMentor { get; set; }
        public IEnumerable<ProfileSubjectModel1> trainee_point { get; set; }
    }
    public class ProfileSubjectModel1
    {
        public bool? bit_Active { get; set; }
        public string SubjectCode { get; set; }
        public DateTime? dtm_from { get; set; }
        public string dtm_from_to { get; set; }
        public string subjectName { get; set; }
        public object point { get; set; }
        public string remark { get; set; }
        public string grade { get; set; }
        public string recurrent { get; set; }
        public int? memberId { get; set; }
        public Course_Detail courseDetails { get; set; }
        public DateTime? ex_Date { get; set; }
    }
}
