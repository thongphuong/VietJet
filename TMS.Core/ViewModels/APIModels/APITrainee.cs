using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APITrainee
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
        public string Department_Name { get; set; }
        public string Department_Code_HOD { get; set; }
        public string JobTitle_Code { get; set; }
        public string JobTitle_Name { get; set; }
        public string Company_Code { get; set; }
        public string Company_Name { get; set; }

        public int Role { get; set; }
        public bool? Role_Examiner { get; set; }

        public string Password { get; set; }

        public bool Role_HOD { get; set; }

        public int Join_Date { get; set; }
        public int Join_Party_Date { get; set; }

        public IEnumerable<APIEdu> Edu { get; set; }
        public IEnumerable<APIContract> Contract { get; set; }

        public class APIContract
        {
            public int ExpireDate { get; set; }
            public string Description { get; set; }
            public string ContractNo { get; set; }

            public APIContract(Trainee_Contract traineeContract)
            {
                ExpireDate = (traineeContract.expire_date != null
                    ? (int)DateUtil.ConvertToUnixTime(traineeContract.expire_date.Value)
                    : 0);
                Description = traineeContract.description ?? "";
                ContractNo = traineeContract.contractno ?? "";
            }
        }

        public class APIEdu
        {
            public int time_from { get; set; }
            public int time_to { get; set; }
            public string Duration { get; set; }
            public string Location { get; set; }
            public string note { get; set; }
            public string organization { get; set; }
            public string Result { get; set; }
            public string Subject { get; set; }
            public string Trainer { get; set; }

            public IEnumerable<ListCertificate> Certificates { get; set; }

            
            public APIEdu(Trainee_Record traineeRecord)
            {
                time_from = (traineeRecord.dtm_time_from != null
                    ? (int)DateUtil.ConvertToUnixTime(traineeRecord.dtm_time_from.Value)
                    : 0);
                time_to = (traineeRecord.dtm_time_to != null
                    ? (int)DateUtil.ConvertToUnixTime(traineeRecord.dtm_time_to.Value)
                    : 0);
                Duration = traineeRecord.str_Duration ?? "";
                Location = traineeRecord.str_Location ?? "";
                note = traineeRecord.str_note ?? "";
                ;
                organization = traineeRecord.str_organization ?? "";
                Result = traineeRecord.str_Result ?? "";
                Subject = traineeRecord.str_Subject ?? "";
                Trainer = traineeRecord.str_Trainer ?? "";
                Certificates = traineeRecord.Trainee_Upload_Files.Select(a => new ListCertificate(a));

            }
            public class ListCertificate
            {
                public string Certificate { get; set; }

                public ListCertificate(Trainee_Upload_Files traineeUploadFiles)
                {
                    this.Certificate = string.IsNullOrEmpty(traineeUploadFiles.Name) ? UtilConstants.PathImage + traineeUploadFiles.Name : "";
                }
            }
        }

        public APITrainee(Trainee trainee)
        {
            this.EmployeeType = (bool)trainee.bit_Internal;
            this.Avatar =
             (string.IsNullOrEmpty(trainee.avatar) || trainee.avatar.StartsWith("NoAvata")
                 ? ""
                 : UtilConstants.PathImage + trainee.avatar);
            //this.Avatar = (string.IsNullOrEmpty(trainee.avatar) || trainee.avatar.StartsWith("NoAvata")) ? "/Content/img/NoAvata.png" : UtilConstants.PathImage + trainee.avatar  + "" ;
            this.EID = trainee.str_Staff_Id;
            this.Passport = trainee.Passport;
            this.PersonalId = trainee.PersonalId ?? "";
            this.FirstName = trainee.FirstName;
            this.LastName = trainee.LastName;
            this.IsDeleted = (bool)trainee.IsDeleted;
            this.IsActive = trainee.IsActive;
            this.Birthdate = (trainee.dtm_Birthdate != null ? (int)DateUtil.ConvertToUnixTime(trainee.dtm_Birthdate.Value) : 0);
            this.Gender = trainee.Gender ?? (int)UtilConstants.Gender.Others;
            this.Place_Of_Birth = trainee.str_Place_Of_Birth ?? "";
            this.Email = trainee.str_Email;
            this.Nation = trainee.Nation ?? "";
            this.Cell_Phone = trainee.str_Cell_Phone ?? "";
            this.Join_Date = (trainee.dtm_Join_Date != null ? (int)DateUtil.ConvertToUnixTime(trainee.dtm_Join_Date.Value) : 0);
            this.Join_Party_Date = (trainee.Join_Party_Date != null ? (int)DateUtil.ConvertToUnixTime(trainee.Join_Party_Date.Value) : 0);
            this.Department_Code = trainee.Department?.Code ?? "";
            this.Department_Name = trainee.Department?.Name ?? "";
            this.Department_Code_HOD = trainee.Departments.FirstOrDefault() != null ? trainee.Departments.FirstOrDefault().Code : "";
            this.Role_HOD = trainee.Departments.Count() > 0 ? true : false;
            this.JobTitle_Code = trainee.JobTitle?.Code ?? "";
            this.JobTitle_Name = trainee.JobTitle?.Name ?? "";
            this.Company_Code = trainee.Company?.str_code ?? "";
            this.Company_Name = trainee.Company?.str_Name ?? "";
            this.Role = trainee.int_Role ?? (int)UtilConstants.ROLE.Trainee;
            this.Password = string.IsNullOrEmpty(trainee.Password) ? "" : Utils.Common.DecryptString(trainee.Password);
            this.Role_Examiner = trainee.IsExaminer;
            this.Edu = trainee.Trainee_Record?.Where(a => a.bit_Deleted==false).Select(a => new APIEdu(a));
            this.Contract = trainee.Trainee_Contract?.Select(a => new APIContract(a));

        }
    }
}
