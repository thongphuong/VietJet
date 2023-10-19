using System;
using System.Collections.Generic;
using System.Security.Permissions;
using DAL.Entities;
using TMS.Core.ViewModels.Common;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Orientation
{
    public class OrientationViewModel
    {
        public int? Id { get; set; }
        public int TraineeId { get; set; }
        public int? IdKindOfSuccessor { get; set; }
        public DateTime CreateDay { get; set; }
        public int CreateBy { get; set; }
        public DateTime ReadyDate { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ExpectedDate { get; set; }
        [Required]
        public string Remark { get; set; }
        public Dictionary<int, string> EmpList { get; set; }
        public IEnumerable<JobTitle> JobList { get; set; }
        public IEnumerable<Title_Standard> TitleStandard { get; set; }
        public Trainee Employee { get; set; }
        public IEnumerable<int> Abilities { get; set; }
        public IEnumerable<Course_Result_Final> CourseResultFinals { get; set; }
        public IEnumerable<CustomDataListModel> OrientationKindOfSuccessorList { get; set; }
        public List<int> ListSubjectAssign { get; set; }
        public List<int> ListSubjectFinal { get; set; }
        public Dictionary<int, string> GroupTrainees { get; set; }
        public string EmpListCustom { get; set; }
        public string Departments { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public IEnumerable<OrientationModify> OrientationModify { get; set; }
        public int? JobFuture { get; set; }
    }

    public class OrientationModifyViewModel : OrientationViewModel
    {
        public Trainee Trainee { get; set; }
        public int JobFutureId { get; set; }
        public JobTitle JobHistoryId { get; set; }
        public JobTitle JobFuture { get; set; }
        public IEnumerable<Orientation_Kind_Of_Successor> OrientationKindOfSuccessor { get; set; }
        public IEnumerable<CustomDataListModel> Subjects { get; set; }
        public IEnumerable<OrientationJobCoincideTrainee> TrainedEmployee { get; set; }
    }

    public class OrientationJobViewModel : OrientationListJobTitle
    {
        public IEnumerable<JobTitle> JobList { get; set; }
        public Dictionary<int, string> JobLevels { get; set; }
        public IEnumerable<CustomDataListModel> JobHeaders { get; set; }
        public IEnumerable<CustomDataListModel> JobPositions { get; set; }
    }

    public class OrientationJobRouteEmployeeViewModel 
    {
        public IEnumerable<CustomDataListModel> Subjects { get; set; }
        public IEnumerable<OrientationJobCoincideTrainee> TrainedEmployee { get; set; }
    }

    public class OrientationJobCoincideTrainee 
    {
        public int? Id { get; set; }
        public string FullName { get; set; }
        public int AmountSubjects { get; set; }
        public Trainee Employee { get; set; }
    }
    public class OrientationJobAvalibleSubject
    {
        public int SubjectId { get; set; }
        public bool Status { get; set; }
    }


    public class OrientationJobAvalibleSubjectTest
    {
        public int SubjectId { get; set; }
        public bool Status { get; set; }
    }
    public class OrientationListJobTitle
    {
        public IEnumerable<JobTitle> JobList { get; set; }
    }
    public class OrientationModify
    {
        public int EmployeeID { get; set; }
        public int JobTitleID { get; set; }
        public int FuturePositionID { get; set; }
        public bool SelectedValue { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEID { get; set; }
        public string JobTitleName { get; set; }
        public string JobTitleFutureName { get; set; }
        public int? Status { get; set; }
    }
}
