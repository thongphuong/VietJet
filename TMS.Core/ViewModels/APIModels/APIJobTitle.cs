using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIJobTitle
    {
        public string JobTitleCode { get; set; }
        public string JobTitleName { get; set; }
        public string JobTitleDescription { get; set; }
        public string JobTitleHeaderName { get; set; }
        public string JobTitlePositionName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }
        public class Subject
        {
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public int? SubjectId { get; set; }

            public Subject(Title_Standard titleStandard)
            {
                this.SubjectCode = titleStandard.SubjectDetail.Code ?? string.Empty;
                this.SubjectName = titleStandard.SubjectDetail.Name ?? string.Empty;
                this.SubjectId = titleStandard?.SubjectDetail?.Id;
            }
        }

        public APIJobTitle(JobTitle job)
        {
            this.JobTitleCode = job.Code ?? string.Empty;
            this.JobTitleName = job.Name ?? string.Empty;
            this.JobTitleDescription = job.Description ?? string.Empty;
            //this.JobTitleHeaderName = job.JobtitleHeader.Name ?? string.Empty;
            //this.JobTitlePositionName = job.JobtitlePosition.Name ?? string.Empty;
            this.IsActive = (bool)job.IsActive;
            this.IsDeleted = (bool)job.IsDelete;
            this.Subjects = job.Title_Standard.Select(a => new Subject(a));

        }
    }
}
