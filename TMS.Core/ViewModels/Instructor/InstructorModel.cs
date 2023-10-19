using System;
using System.Collections.Generic;

using System.Web.Mvc;
using DAL.Entities;


namespace TMS.Core.ViewModels.Instructor
{
    public class InstructorModel
    {
        public SelectList Departments { get; set; } 
        public SelectList JobTitles { get; set; }
        public SelectList Genders { get; set; }

        public SelectList Status { get; set; }

        public SelectList Type { get; set; }
        public SelectList Mentor { get; set; }
        public string DepartmentList { get; set; }
    }
}
