using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Core.ViewModels.PostNews
{
    public class PostNewsCategoryModel
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Sort { get; set; }
        public string Ancestor { get; set; }
        public int? ParentId { get; set; }
        public string Icon { get; set; }
        public string BackgroundColor { get; set; }
        public Dictionary<int, string> Parent { get; set; }
    }
}
