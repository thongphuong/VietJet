using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Core.ViewModels.Subjects;

namespace TMS.Core.ViewModels.Employee
{
   public class EmployeeModelPrint
    {
        public bool CheckHannahMentor { get; set; }
        public string HannahMentor { get; set; }
        public int Control { get; set; }
        public int? Id { get; set; }

        public string FullName { get; set; }


        public string Eid { get; set; }

        public string PersonalId { get; set; }


        public string Passport { get; set; }

        public string Birthdate { get; set; }

        public string Gender { get; set; }

        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }

        public string Email { get; set; }

        public string Nation { get; set; }
        public string Phone { get; set; }

        public string DateOfJoin { get; set; }
        public int? EmployeeType { get; set; }

        public string JobTitle { get; set; }

        public string Department { get; set; }

        public string Company { get; set; }

        public string Avatar { get; set; } 
        public string ResignationDate { get; set; }

        public string Type { get; set; }
        public IEnumerable<Education> Educations { get; set; }
        public IEnumerable<Contract> Contracts { get; set; }
        public IEnumerable<TrainningCourse> TrainningCourses { get; set; }

        public IEnumerable<ConductedCourse> ConductedCourses { get; set; }
        public IEnumerable<TrainingCompetency> TrainingCompetencies { get; set; }
        public IEnumerable<ProfileSubjectModel> TrainingCourseCustom { get; set; }
        public class Education
        {
             public string Time { get; set; }
             public string Course { get; set; }
                public string Organization { get; set; }
            public string Note { get; set; }
        }
        public class Contract
        {
            public string ContractNo { get; set; }
            public string ExpiryDate { get; set; }
            public string Description { get; set; }
        }

        public class TrainningCourse
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Time { get; set; }
            public string Certificate { get; set; }

            public IEnumerable<SubjectOfTrainningCourse> SubjectOfTrainningCourses { get; set; }

            public class SubjectOfTrainningCourse
            {
                public string Time { get; set; }
                public string Subject { get; set; }
                public string TypeLearning { get; set; }
                public string Point { get; set; }
                public string Grade { get; set; }
                public string Remark { get; set; }
                public string RefreshCycle { get; set; }
                public string ExpiryDate { get; set; }
            }

        }
        public class TrainingCompetency
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
        //Subject
        public class ConductedCourse
        {
            public string Code { get; set; }
            public string Name { get; set; }


            public class CourseOfConducted
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public string From { get; set; }
                public string To { get; set; }
                public string Room { get; set; }
            }
        }


    }
}
