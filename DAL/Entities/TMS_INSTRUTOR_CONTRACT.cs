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
    
    public partial class TMS_INSTRUTOR_CONTRACT
    {
        public int id { get; set; }
        public Nullable<int> id_user { get; set; }
        public Nullable<int> id_contract { get; set; }
    
        public virtual TMS_CONTRACTS TMS_CONTRACTS { get; set; }
        public virtual TMS_EMPLOYEE TMS_EMPLOYEE { get; set; }
    }
}
