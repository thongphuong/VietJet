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
    
    public partial class TMS_Course_History_Member
    {
        public int Id { get; set; }
        public Nullable<int> Member_Id { get; set; }
        public Nullable<int> Course_Id { get; set; }
    
        public virtual TMS_Course_History TMS_Course_History { get; set; }
        public virtual Trainee Trainee { get; set; }
    }
}
