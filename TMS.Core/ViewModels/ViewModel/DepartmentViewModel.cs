using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Core.ViewModels.ViewModel
{
    public class DepartmentViewModel
    {
        public DepartmentViewModel()
        {
            //TMS_Approve_Department = new HashSet<TMS_Approve_Department>();
        }
        public Nullable<int> Department_Id { get; set; }
        [Required(ErrorMessageResourceName = "DEPARTMENTCODE_REQUIRED", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string str_Code { get; set; }
        [Required(ErrorMessageResourceName = "DEPARTMENTNAME_REQUIRED", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string str_Name { get; set; }
        public bool bit_Deleted { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public string str_Created_By { get; set; }
        public Nullable<System.DateTime> dtm_Last_Modified_At { get; set; }
        public string str_Last_Modified_By { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public string str_Deleted_By { get; set; }
        public Nullable<int> int_Parent_Id { get; set; }
        //public ICollection<TMS_Approve_Department> TMS_Approve_Department { get; set; }
        public Nullable<int> int_User_Id { get; set; }

        public string description { get; set; }
    }
}