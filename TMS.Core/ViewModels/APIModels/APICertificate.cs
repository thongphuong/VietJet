using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
   public class APICertificate
    {
        public string CertificateNo { get; set; }
        //public int? CourseDetailId { get; set; }
        public string TraineeCode { get; set; }
        public string ExpireDate { get; set; }
        public int CourseId { get; set; }
        public string Issue { get; set; }
        public int TypeCertificate { get; set; }
        public string Path { get; set; }
        public bool? IsNotShow { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public string Grade { get; set; }
        public int CreateDate { get; set; }
        public string CourseCode { get; set; }

        public APICertificate(Course_Result_Final courseResultFinal)
       {
            this.CertificateNo = courseResultFinal?.certificatefinal ?? string.Empty;
            this.CourseId = courseResultFinal?.courseid ?? 0;
            this.CourseCode = courseResultFinal?.Course?.Code;
            this.TraineeCode = courseResultFinal?.Trainee?.str_Staff_Id ?? string.Empty;
            this.ExpireDate = courseResultFinal.TMS_CertificateApproved.Any(a => a.Expiration_Date.HasValue) ? DateUtil.ConvertToUnixTime(courseResultFinal.TMS_CertificateApproved.FirstOrDefault(a => a.Expiration_Date.HasValue).Expiration_Date.Value).ToString() : "";
            this.Issue = courseResultFinal.TMS_CertificateApproved.Any(a => a.CreateBy.HasValue) ? courseResultFinal.TMS_CertificateApproved.FirstOrDefault(a => a.CreateBy.HasValue).USER.LASTNAME +" "+ courseResultFinal.TMS_CertificateApproved.FirstOrDefault(a => a.CreateBy.HasValue).USER.FIRSTNAME : "";
            this.TypeCertificate = 1;
            //this.Path = (courseResultFinal.Path != null ? UtilConstants.PathFileExtImage + courseResultFinal.Path : "") ;
            this.Path = (string.IsNullOrEmpty(courseResultFinal.Path) ? string.Empty : courseResultFinal.Path);
            this.IsNotShow = courseResultFinal.statusCertificate == 1 ? true : false;
            this.StartDate = (int)DateUtil.ConvertToUnixTime(courseResultFinal?.Course?.StartDate ?? DateTime.Now);
            this.EndDate = (int)DateUtil.ConvertToUnixTime(courseResultFinal?.Course?.EndDate  ?? DateTime.Now);
            this.CreateDate = (int)DateUtil.ConvertToUnixTime(courseResultFinal.CreateCertificateDate ?? DateTime.Now);
            this.Grade = courseResultFinal.grade.HasValue ? GetGrade(courseResultFinal?.grade) : "";
        }
        private string GetGrade(int? grade)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    result = UtilConstants.Grade.Fail.ToString();
                    break;
                case (int)UtilConstants.Grade.Pass:
                    result = UtilConstants.Grade.Pass.ToString();
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    result = UtilConstants.Grade.Distinction.ToString();
                    break;
            }
            return result;
        }
    }
}


