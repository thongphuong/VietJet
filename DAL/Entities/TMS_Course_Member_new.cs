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
    
    public partial class TMS_Course_Member_new
    {
        public int Id { get; set; }
        public Nullable<int> Member_Id { get; set; }
        public Nullable<int> Course_Details_Id { get; set; }
        public Nullable<int> Approve_Id { get; set; }
        public Nullable<int> IsDelete { get; set; }
        public Nullable<int> DeleteApprove { get; set; }
    }
}
