using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APISurvey
    {
        public int Id { get; set; }
        public string Code { get; set; }     
        public string Name { get; set; }
        public int? OpenDate { get; set; }
        public int? CloseDate { get; set; }
        public int? CreatedAt { get; set; }
        public int? Resp_View { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public APISurvey(DAL.Entities.Survey survey)
        {
            this.Id = survey.Id;
            this.Code = survey.Code;
            this.Name = survey.Name; 
            this.OpenDate = (survey.OpenDate != null ? (int)DateUtil.ConvertToUnixTime(survey.OpenDate.Value) : 0);
            this.CloseDate = (survey.CloseDate != null ? (int)DateUtil.ConvertToUnixTime(survey.CloseDate.Value) : 0);
            this.CreatedAt = (survey.Created_At != null ? (int)DateUtil.ConvertToUnixTime(survey.Created_At.Value) : 0);
            this.Description = survey.Description;
            this.Resp_View = survey.Resp_View;
            this.Status = survey.Is_Active == true ? 1 : 0;
            this.IsActive = survey.Is_Active;
            this.IsDeleted = survey.Is_Deleted;            
        }
    }
}
