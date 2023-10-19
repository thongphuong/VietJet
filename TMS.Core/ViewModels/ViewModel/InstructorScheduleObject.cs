using System.Collections.Generic;
namespace TMS.Core.ViewModels.ViewModel
{
    public class InstructorScheduleObject
    {
        public int Instructor_Id { get; set; }
        public string Instructor_Name { get; set; }
        public List<ScheduleObject> InstructorScheduleAM { get; set; }
        public List<ScheduleObject> InstructorSchedulePM { get; set; }
    }  

    public class InstructorDailyObject
    {
        public int Instructor_Id { get; set; }
        public string CourseName { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }
   
}