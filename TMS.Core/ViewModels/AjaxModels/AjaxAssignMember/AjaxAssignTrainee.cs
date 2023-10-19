using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.AjaxModels.AjaxAssignMember
{
    public class AjaxAssignTrainee
    {
        public int Id { get; set; }
        public string TraineeCode { get; set; }
        public string FullName { get; set; }
        public string Option { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }

        public AjaxAssignTrainee()
        {
            this.Id = Id;
            this.TraineeCode = TraineeCode;
            this.FullName = FullName;
            this.Option = Option;
            this.JobTitle = JobTitle;
            this.Email = Email;
            this.Remark = Remark;
        }
        public AjaxAssignTrainee(Trainee member,string FullName,string VerticalBar)
        {
            this.Id = member.Id;
            this.TraineeCode = member.str_Staff_Id;
            //this.FullName =  member.FirstName.Trim() + " "+ member.LastName.Trim();
            this.FullName = FullName;
            this.Option ="<input type = 'checkbox' name = 'idAssign[]' value = '"+member.Id+"' />"+ VerticalBar /*+ "<a href = 'javascript:void(0)' onclick = 'detail(" + member.Id + ")' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue' aria-hidden='true' style='font-size: 16px;'></i></a>"*/;
            //if(LtsRemarkTrainee.Count() > 0)
            //{
            //    var x = LtsRemarkTrainee.LastOrDefault(a => a.idTrainee == member.Id) != null ? LtsRemarkTrainee.LastOrDefault(a => a.idTrainee == member.Id).remarkTrainee : string.Empty;
            //    this.Remark = "<input class='TraineeRemark' type = 'text' id = 'TraineeRemark' value='" + x + "' />";
            //}
            //else
            //{

            //    this.Remark = "<input class='TraineeRemark' name='TraineeRemark' type = 'text' id = 'TraineeRemark' value='"+ member?.TMS_Course_Member?.FirstOrDefault()?.Remark + "' />";
            //}
            //this.Remark = "<input class='TraineeRemark' name='TraineeRemark' type = 'text' id = 'TraineeRemark' value='" + member?.TMS_Course_Member?.FirstOrDefault()?.Remark + "' />";
            this.Remark = member?.TMS_Course_Member?.FirstOrDefault()?.Remark;
            this.JobTitle = member?.JobTitle?.Name;
            this.Email = member.str_Email;
        }
    }
    public class RemarkTrainee
    {
        public int idTrainee { get; set; }
        public string remarkTrainee { get; set; }
    }
    public class ListInstructor
    {
        public string instructorId { get; set; }
        public string dtm_time_from { get; set; }
        public string dtm_time_to { get; set; }
        public string time_from { get; set; }
        public string time_to { get; set; }
    }

}
