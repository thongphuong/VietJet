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
    
    public partial class Orientation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Orientation()
        {
            this.Orientation_Item = new HashSet<Orientation_Item>();
        }
    
        public int Id { get; set; }
        public Nullable<int> TraineeId { get; set; }
        public Nullable<int> JobHistoryId { get; set; }
        public Nullable<int> JobFutureId { get; set; }
        public Nullable<int> IdKindOfSuccessor { get; set; }
        public Nullable<System.DateTime> CreateDay { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> ReadyDate { get; set; }
        public Nullable<System.DateTime> ExpectedDate { get; set; }
        public string Remark { get; set; }
    
        public virtual JobTitle JobTitle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Orientation_Item> Orientation_Item { get; set; }
        public virtual Orientation_Kind_Of_Successor Orientation_Kind_Of_Successor { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual USER USER { get; set; }
    }
}
