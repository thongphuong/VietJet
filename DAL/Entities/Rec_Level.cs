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
    
    public partial class Rec_Level
    {
        public int Id { get; set; }
        public string str_Name { get; set; }
        public string str_Description { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Delete { get; set; }
        public Nullable<int> int_Create_by { get; set; }
        public Nullable<int> int_Delete_by { get; set; }
        public Nullable<int> int_Modify_by { get; set; }
        public Nullable<System.DateTime> dtm_Create_at { get; set; }
        public Nullable<System.DateTime> dtm_Delete_at { get; set; }
        public Nullable<System.DateTime> dtm_Modify_at { get; set; }
    }
}
