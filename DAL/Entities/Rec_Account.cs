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
    
    public partial class Rec_Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Rec_Account()
        {
            this.Rec_Company_Account = new HashSet<Rec_Company_Account>();
        }
    
        public int Id { get; set; }
        public string UserName { get; set; }
        public Nullable<int> int_Created_By { get; set; }
        public Nullable<int> int_Deleted_By { get; set; }
        public Nullable<int> int_Modified_By { get; set; }
        public Nullable<System.DateTime> dtm_Created_At { get; set; }
        public Nullable<System.DateTime> dtm_Deleted_At { get; set; }
        public Nullable<System.DateTime> dtm_Modified_At { get; set; }
        public Nullable<bool> Is_Active { get; set; }
        public Nullable<bool> Is_Delete { get; set; }
        public string str_Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rec_Company_Account> Rec_Company_Account { get; set; }
    }
}
