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
    
    public partial class Course_Result_Course_Detail_Ingredient
    {
        public int Id { get; set; }
        public Nullable<int> CourseDetailIngredient_Id { get; set; }
        public Nullable<int> CourseResult_Id { get; set; }
        public Nullable<int> Wiegth { get; set; }
        public Nullable<double> Score { get; set; }
    
        public virtual Course_Detail_Course_Ingredients Course_Detail_Course_Ingredients { get; set; }
        public virtual Course_Result Course_Result { get; set; }
    }
}
