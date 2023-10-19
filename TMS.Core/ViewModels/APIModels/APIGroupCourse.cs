using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    using DAL.Entities;
    public class APIGroupCourse
    {
     
        public string code_group { get; set; }
        public string name_group { get; set; }
        public string description { get; set; }
        public int is_delete { get; set; }
        public int is_active { get; set; }
        public IEnumerable<ApiSubjectDetailModel> subjects { get; set; }


        public class ApiSubjectDetailModel
        {
      

            public string code { get; set; }
            public int SubjectId { get; set; }
        
          
            public ApiSubjectDetailModel(CAT_GROUPSUBJECT_ITEM catGroupsubjectItem)
            {
               
               
                this.code = catGroupsubjectItem.SubjectDetail.Code;
                this.SubjectId = catGroupsubjectItem.SubjectDetail.Id;             
            }
        }

        public APIGroupCourse(CAT_GROUPSUBJECT catGroupsubject)
        {
            
            this.code_group = catGroupsubject.Code ?? "";
            this.name_group = catGroupsubject.Name ?? "";
            this.description = catGroupsubject.Description ?? "";
            this.is_delete = catGroupsubject.IsDeleted == true ? 1 : 0;
            this.is_active = catGroupsubject.IsActive == true ? 1 : 0;
            this.subjects = catGroupsubject.CAT_GROUPSUBJECT_ITEM?.Where(a => a.SubjectDetail.IsActive==true && a.SubjectDetail.IsDelete==false).Select(a => new ApiSubjectDetailModel(a));
        }
    }
}
