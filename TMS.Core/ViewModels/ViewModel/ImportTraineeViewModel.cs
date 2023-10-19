using System;

namespace TMS.Core.ViewModels.ViewModel
{
    public class ImportTraineeViewModel
    {
        public string str_Fullname { get; set; }
        public string str_Email { get; set; }
        public string str_Cell_Phone { get; set; }
        public Nullable<System.DateTime> dtm_Birthdate { get; set; }
        public string str_Station { get; set; }
        public bool bit_Internal { get; set; }
        public string Department { get; set; }
        public string Company { get; set; }
        public string Job_Title { get; set; }        
        public string str_Staff_Id { get; set; }
        public string str_BirthPlace { get; set; }
        public string status { get; set; }
    }
}