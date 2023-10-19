using System.Collections.Generic;

namespace TMS.Core.ViewModels.ViewModel
{
    public class RoomScheduleObject
    {
        public int Room_Id { get; set; }
        public string Room_Name { get; set; }
        public List<ScheduleObject> RoomScheduleAM { get; set; }
        public List<ScheduleObject> RoomSchedulePM { get; set; }
    }

    public class ScheduleObject
    {
        public int numOfDate { get; set; }
        public int colorType { get; set; }
        public string Text { get; set; }
        public int BookingId { get; set; }
    }


    public class RoomDailyObject
    {
        public int Room_Id { get; set; }
        public string CourseName { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }
   
}