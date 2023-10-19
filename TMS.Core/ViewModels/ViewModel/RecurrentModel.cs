using System;

namespace TMS.Core.ViewModels.ViewModel
{
    public class RecurrentModel
    {
        public string str_Staff_Id { get; set; }
        public string str_Fullname { get; set; }
        public string str_firstname { get; set; }
        public string str_lastname { get; set; }
        public string dept_Name { get; set; }
        public string job_Name { get; set; }
        public string subj_Code { get; set; }
        public string subj_Name { get; set; }
        public DateTime? ex_Date { get; set; }
        public DateTime? start_Date { get; set; }
        public string Validity { get; set; }
    }
}