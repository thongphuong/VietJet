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
    
    public partial class GroupFunction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GroupFunction()
        {
            this.GroupPermissionFunctions = new HashSet<GroupPermissionFunction>();
            this.GroupUserMenus = new HashSet<GroupUserMenu>();
            this.MENUs = new HashSet<MENU>();
            this.ROLEMENUs = new HashSet<ROLEMENU>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> MenuId { get; set; }
        public string Alias { get; set; }
        public Nullable<int> GroupIndex { get; set; }
        public string DefaultUrl { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GroupPermissionFunction> GroupPermissionFunctions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GroupUserMenu> GroupUserMenus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MENU> MENUs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ROLEMENU> ROLEMENUs { get; set; }
    }
}
