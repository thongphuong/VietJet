using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.ReportModels
{
    public class InstructorSubjectModel
    {
        public IEnumerable<InstructorSubjectModelRp> InstructorSubjectModelRps { get; set; }
        
        public class InstructorSubjectModelRp
        {
             public string Eid { get; set; }
            public string Name { get; set; }
            public string Job { get; set; }
            public string Department { get; set; }
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public string Type { get; set; }
            public List<Trainee_TrainingCenter> Relevant_Department { get; set; }
        }
    }
}
