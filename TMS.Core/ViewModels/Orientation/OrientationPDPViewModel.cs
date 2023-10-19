using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.Orientation
{
   public class OrientationPDPViewModel
    {

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ExpectedDate { get; set; }
        [Required]
        public string Remark { get; set; }
        public int? IdKindOfSuccessor { get; set; }
        public int TraineeId { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string TraineeCode { get; set; }
        public string Passport { get; set; }
        public string PersonId { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string Phone { get; set; }
        public string DateOfJoin { get; set; }


        public int JobTitleId { get; set; }
        public string JobTitleDefaultName { get; set; }
        public string Type { get; set; }
        public string DepartmentName { get; set; }
        public string Company { get; set; }
        public int JobFutureId { get; set; }
        public string JobTitleFutureName { get; set; }
        //Level
        public string Position { get; set; }
        public Dictionary<int, string> OrientationKindOfSuccessor { get; set; }

    }

  
}
