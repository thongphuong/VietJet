using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
   public class APIChangePasswordTrainee
    {
        public string TraineeCode { get; set; }

        public string Password { get; set; }
        public string Passport { get; set; }
        public string Phone { get; set; }
        public int? JoinDate { get; set; }
        public int? JoinPartyDate { get; set; }
        public int? BirthDay { get; set; }
        public string UrlAvatar { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }

        
    }
}
