using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIPostCategoryModel
    {
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int Id { get; set; }
        public string Background { get; set; }
        public string Icon { get; set; }
        public APIPostCategoryModel(Postnews_Category postnew)
        {
            this.Id = postnew.Id;
            this.IsActive = postnew.IsActive;
            this.IsDeleted = postnew.IsDeleted;
            this.Name = postnew.Name;
            this.Background = postnew.Background;
            this.Icon = postnew.Icon;
        }
    }
}
