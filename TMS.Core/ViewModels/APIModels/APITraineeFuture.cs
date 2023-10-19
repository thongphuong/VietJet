using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.APIModels
{
    public class APITraineeFuture
    {
        public string TraineeCode { get; set; }
        public string JobTitleCode { get; set; }
        public string JobTitleName { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }

        public class Subject
        {
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }

            public Subject(TraineeFuture_Item traineeFutureItem)
            {
                this.SubjectCode = traineeFutureItem.SubjectDetail.Code;
                this.SubjectName = traineeFutureItem.SubjectDetail.Name;
            }
        }

        public APITraineeFuture(TraineeFuture traineeFuture)
        {
            this.TraineeCode = traineeFuture.Trainee.str_Staff_Id;
            this.JobTitleCode = traineeFuture.JobTitle.Code;
            this.JobTitleName = traineeFuture.JobTitle.Name;
            this.Subjects = traineeFuture.TraineeFuture_Item.Select(a => new Subject(a));
        }
    }
}
