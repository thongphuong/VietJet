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
    
    public partial class Subject_Score
    {
        public int id { get; set; }
        public Nullable<int> subject_id { get; set; }
        public Nullable<double> point_from { get; set; }
        public string grade { get; set; }
        public Nullable<double> point_to { get; set; }
    
        public virtual SubjectDetail SubjectDetail { get; set; }
    }
}
