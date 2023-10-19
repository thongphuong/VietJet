using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Room
{
    public class MeetingModels
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }       
        public string Description { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        [Required]
        public int? RoomID { get; set; }
        public string Participants { get; set; }
        [Required]
        public string TimeFrom { get; set; }
        [Required]
        public string TimeTo { get; set; }
        public string check_Meeting { get; set; }
        public object[] ListParticipant { get; set; }
        public Dictionary<int,string> ListRoom { get; set; }
        //public Dictionary<int, string> Departments { get; set; }
        public string Departments { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public List<int?> ListSelect { get; set; }
    }
}
