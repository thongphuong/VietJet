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
    
    public partial class SurveyLM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SurveyLM()
        {
            this.SurveyLMS_Item = new HashSet<SurveyLMS_Item>();
            this.SurveyLMS_Question = new HashSet<SurveyLMS_Question>();
        }
    
        public int id { get; set; }
        public Nullable<int> course_detail_id { get; set; }
        public string name { get; set; }
        public string str_intro { get; set; }
        public Nullable<int> idLMS { get; set; }
    
        public virtual Course_Detail Course_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyLMS_Item> SurveyLMS_Item { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyLMS_Question> SurveyLMS_Question { get; set; }
    }
}