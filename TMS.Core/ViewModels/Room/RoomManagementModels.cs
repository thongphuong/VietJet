using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.Room
{
  public  class RoomManagementModels
    {
        public List<DateTime> Period { get; set; }
        public Dictionary<int, string> Background { get; set; }
        public IEnumerable<RoomModel> Room { get; set; }
        public class RoomModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<CourseDetailModel> CourseDetail { get; set; }
            public class CourseDetailModel
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public int Type { get; set; }
                public Dictionary<DateTime,int> StarCol { get; set; }
                public Management_Room_Item ManagementRoomItem { get; set; }
            }
        }
    }

}
