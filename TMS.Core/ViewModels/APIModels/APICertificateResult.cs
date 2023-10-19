using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APICertificateResult //send certificate course result;
    {
        public string CertificateNo { get; set; }
        public int? CourseDetailId { get; set; }
        public string TraineeCode { get; set; }
        public string ExpireDate { get; set; }
        //public string CourseId { get; set; }
        public string Issue { get; set; }
        public int TypeCertificate { get; set; }
        public string Path { get; set; }
        public bool? IsNotShow { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public string Grade { get; set; }
        public int CreateDate { get; set; }
       
        public APICertificateResult(Course_Result courseResult)
        {
            this.CertificateNo = courseResult?.CertificateSubject ?? string.Empty;
            this.CourseDetailId = courseResult?.CourseDetailId ?? 0;
            this.TraineeCode = courseResult?.Trainee?.str_Staff_Id ?? string.Empty;
            this.ExpireDate = courseResult.TMS_CertificateApproved.Any(a=>a.Expiration_Date.HasValue) ? DateUtil.ConvertToUnixTime(courseResult.TMS_CertificateApproved.FirstOrDefault(a => a.Expiration_Date.HasValue).Expiration_Date.Value).ToString() : "";
            this.Issue = courseResult.TMS_CertificateApproved.Any(a => a.CreateBy.HasValue) ? courseResult.TMS_CertificateApproved.FirstOrDefault(a => a.CreateBy.HasValue).USER.LASTNAME + " " + courseResult.TMS_CertificateApproved.FirstOrDefault(a => a.CreateBy.HasValue).USER.FIRSTNAME : "";
            this.TypeCertificate = 0;
            //this.Path = (courseResultFinal.Path != null ? UtilConstants.PathFileExtImage + courseResultFinal.Path : "") ;
            this.Path = (string.IsNullOrEmpty(courseResult.Path) ? string.Empty : courseResult.Path);
            this.IsNotShow = courseResult.StatusCertificate == 1 ? true : false;
            this.StartDate = (int)DateUtil.ConvertToUnixTime(courseResult?.Course_Detail?.dtm_time_to ?? DateTime.Now);
            this.EndDate = (int)DateUtil.ConvertToUnixTime(courseResult?.Course_Detail?.dtm_time_from ?? DateTime.Now);
            this.CreateDate = (int)DateUtil.ConvertToUnixTime(courseResult.CreateCertificateAt ?? DateTime.Now);
            this.Grade = !string.IsNullOrEmpty(courseResult?.Re_Check_Result) ? GetGradeInt(courseResult.Re_Check_Result) : GetGradeInt(courseResult.First_Check_Result);
        }
        public string GetGradeInt(string grade)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            switch (grade)
            {
                case "F":
                    result = UtilConstants.Grade.Fail.ToString();
                    break;
                case "P":
                    result = UtilConstants.Grade.Pass.ToString();
                    break;
                case "D":
                    result = UtilConstants.Grade.Distinction.ToString();
                    break;
            }
            return result;
        }
    }
}
