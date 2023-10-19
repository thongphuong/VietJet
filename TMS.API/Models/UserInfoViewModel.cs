using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.API.Models
{
    public class UserInfoViewModel
    {
        public string FullName { get; set; }
        public string Eid { get; set; }//staff_id
        public bool IsInternal { get; set; }
        public string PersonalNo { get; set; } //str_id
        public string Passport { get; set; }
        public string CellPhone { get; set; }
        public int? PayPerHour { get; set; }
        public string Email { get; set; }
        public int? suspended { get; set; }

        public string Nation { get; set; }
        public string Mail { get; set; }
        public string Station { get; set; }

        public bool IsInstructor { get; set; }

        public DateTime? BirthDate { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime? JoinedDate { get; set; }

        //reference keys
        public int? Gender { get; set; }
        public int? CompanyId { get; set; }
        public int? JobTitleId { get; set; }
        public int? DepartmentId { get; set; }
        public int? CourseId { get; set; }
        public ICollection<Education> Educations{ get; set; }
        public ICollection<Contract> Contracts{ get; set; }

    }

    public class Education
    {
        public string CourseName { get; set; }
        public string Organization { get; set; }
        public string Note { get; set; }
        public DateTime? From{ get; set; }
        public DateTime? To { get; set; }
    }

    public class Contract
    {
        public string ContractNo { get; set; }
        public string Description { get; set; }
        public DateTime? ExpiredDate { get; set; } 
    }
}