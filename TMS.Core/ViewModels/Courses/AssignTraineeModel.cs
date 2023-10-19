using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.Courses
{
    public class AssignTraineeModel
    {
        //public Dictionary<int, string> Departments { get; set; }

        public string Departments { get; set; }
        public Dictionary<int, string> JobTitles { get; set; }
        public Dictionary<int,string> GroupTrainees { get; set; } 
        public Dictionary<int,string> Prerequisite { get; set; }
        public bool ProcessStep { get; set; }
        
        public string CheckPrerequisite { get; set; } = "PRE-REQUISITE";
        public string CheckGroupTrainee { get; set; } = "GROUP_TRAINEE";
        public bool CheckApprove { get; set; }
    }
}
