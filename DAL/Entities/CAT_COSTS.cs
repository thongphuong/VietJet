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
    
    public partial class CAT_COSTS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAT_COSTS()
        {
            this.Course_Cost = new HashSet<Course_Cost>();
        }
    
        public int id { get; set; }
        public string str_Code { get; set; }
        public string str_Name { get; set; }
        public string str_Name1 { get; set; }
        public string str_Description { get; set; }
        public Nullable<System.DateTime> dtm_Dreated_date { get; set; }
        public Nullable<int> int_Created_by { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> dtm_Updated_date { get; set; }
        public Nullable<int> int_Updated_by { get; set; }
        public Nullable<int> GroupCostId { get; set; }
    
        public virtual CAT_GROUPCOST CAT_GROUPCOST { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_Cost> Course_Cost { get; set; }
    }
}
