using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APISubjectDetail
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public int? CourseTypeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAverageCalculate { get; set; }
        public double Recurrent { get; set; }
        public IEnumerable<Score> Scores { get; set; }
        public int Id { get; set; }
        public class Score
        {
            public double Point { get; set; }
            public string Grade { get; set; }

            public Score(Subject_Score subjectScore)
            {
                this.Point = subjectScore.point_from ?? -1;
                this.Grade = subjectScore.grade ?? string.Empty;
            }
        }


        public APISubjectDetail(SubjectDetail subjectDetail)
        {
            this.Code = subjectDetail.Code;
            this.Name = subjectDetail.Name;
            this.Duration = (double)subjectDetail.Duration;
            //var courseTypes = UtilConstants.CourseTypesDictionary();
            //this.CourseType = subjectDetail.CourseTypeId.HasValue ? courseTypes[subjectDetail.CourseTypeId.Value] : string.Empty;
            this.IsActive = (bool)subjectDetail.IsActive;
            this.IsDeleted = (bool)subjectDetail.IsDelete;
            this.IsAverageCalculate = (bool)subjectDetail.IsAverageCalculate;
            this.Recurrent = (double)subjectDetail.RefreshCycle;
            this.Scores = subjectDetail.Subject_Score.Select(a => new Score(a));
            this.Id = subjectDetail.Id;
            this.CourseTypeId = subjectDetail.CourseTypeId.HasValue ? subjectDetail.CourseTypeId : 0;


        }
    }
}
