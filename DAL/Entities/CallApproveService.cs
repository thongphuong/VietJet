//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class CallApproveService
    {
        public int id { get; set; }
        public Nullable<int> step { get; set; }
        public Nullable<int> courseid { get; set; }
        public Nullable<int> approveid { get; set; }
        public Nullable<int> approvestatus { get; set; }
        public Nullable<int> LMSStatus { get; set; }
        public Nullable<int> LMSStatus2 { get; set; }
        public Nullable<bool> flag_sentmail { get; set; }
        public string template_mail { get; set; }
        public string Name_Issue { get; set; }
        public string courrseDetail_list { get; set; }
        public string traineeId_list { get; set; }
        public string cerNo_list { get; set; }
        public string checkFail { get; set; }
        public string course_ressult_id { get; set; }
        public Nullable<int> approve_by_id { get; set; }
    }
}
