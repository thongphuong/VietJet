using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIEmployeeProfile
{
    using DAL.Entities;
    using System.ComponentModel.DataAnnotations;
    using Utils;

    public class APIEmployeeProfile
    {
        public bool EmployeeType { get; set; }
        public string Avatar { get; set; }
        public string EID { get; set; }
        public string Passport { get; set; }
        public string PersonalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Birthdate { get; set; }
        public int? Gender { get; set; }
        public string Place_Of_Birth { get; set; }
        public string Email { get; set; }
        public string Nation { get; set; }
        public string Cell_Phone { get; set; }

        public bool IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public string Department_Code { get; set; }
        public string JobTitle_Code { get; set; }
        public string Company_Code { get; set; }
       
        public int Join_Date { get; set; }
        public double? Allowance { get; set; }
     
        public IEnumerable<Edu> Edu { get; set; }
        public IEnumerable<Contract> Contract { get; set; }
        public IEnumerable<Course_Detail_Instructor> InstructorAllowance { get; set; } 
        public APIEmployeeProfile(Trainee trainee)
        {
            this.EmployeeType = trainee.bit_Internal==true;
            this.Avatar = (trainee.avatar != null ? UtilConstants.PathImage + trainee.avatar : "" );
            this.EID = trainee.str_Staff_Id;
            this.Passport = trainee.Passport;
            this.PersonalId = trainee.PersonalId;
            this.FirstName = trainee.LastName;
            this.LastName = trainee.FirstName;
            this.IsDeleted = (bool)trainee.IsDeleted;
            this.IsActive = trainee.IsActive;
            this.Birthdate = (trainee.dtm_Birthdate != null ? (int)DateUtil.ConvertToUnixTime(trainee.dtm_Birthdate.Value) : 0);
            this.Gender = trainee.Gender;
            this.Place_Of_Birth = trainee.str_Place_Of_Birth;
            this.Email = trainee.str_Email;
            this.Nation = trainee.Nation;
            this.Cell_Phone = trainee.str_Cell_Phone ?? "";
            this.Join_Date = (trainee.dtm_Join_Date != null ? (int)DateUtil.ConvertToUnixTime(trainee.dtm_Join_Date.Value) : 0);
            this.Allowance = trainee.Allowance;
            this.Department_Code = trainee.Department.Code;
            this.JobTitle_Code = trainee.JobTitle.Code;
            this.Company_Code = trainee.Company != null ? trainee.Company.str_code : "";
            
            this.Edu = trainee.Trainee_Record.Where(b => b.bit_Deleted==false).Select(edu => new Edu()
            {
                time_from = (edu.dtm_time_from != null ? (int)DateUtil.ConvertToUnixTime(edu.dtm_time_from.Value) : 0),
                time_to = (edu.dtm_time_to != null ? (int)DateUtil.ConvertToUnixTime(edu.dtm_time_to.Value) : 0),
                Duration = edu.str_Duration,
                Location = edu.str_Location,
                note = edu.str_note,
                organization = edu.str_organization,
                Result = edu.str_Result,
                Subject = edu.str_Subject,
                Trainer = edu.str_Trainer,
                Id = edu.Trainee_Record_Id
            });
            this.Contract = trainee.Trainee_Contract.Select(ct => new Contract()
            {
                Id = ct.id,
                ExpireDate = (ct.expire_date != null ? (int)DateUtil.ConvertToUnixTime(ct.expire_date.Value) : 0),
                Description = ct.description,
                ContractNo = ct.contractno
            });
            this.InstructorAllowance = trainee.Course_Detail_Instructor.Select(a => new Course_Detail_Instructor()
            {

            });
        }
    }
    public class Edu
    {
        public int? Id { get; set; }
        public int? time_from { get; set; }
        public int? time_to { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public string note { get; set; }
        public string organization { get; set; }
        public string Result { get; set; }
        public string Subject { get; set; }
        public string Trainer { get; set; }

    }
  
    public class Contract
    {
        public int Id { get; set; }
        public int? ExpireDate { get; set; }
        public string Description { get; set; }
        public string ContractNo { get; set; }
    }




}
