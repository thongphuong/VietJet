using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Room
{
  public  class RoomModels
    {
       public int? Id { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_CODE")]
        public string Code { get; set; }
        [Required(ErrorMessageResourceType = typeof(App_GlobalResources.Messege),
               ErrorMessageResourceName = "VALIDATION_NAME")]
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Equipment { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public int? Is_Meeting { get; set; }
        public string check_Meeting { get; set; }
        public string[] strEquipment { get; set; }
        public Dictionary<int,string> room_type { get; set; }

    }
}
