using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.PostNews
{
    public class PostNewsModel
    {
        public int? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [AllowHtml]
        [Required]
        public string Content { get; set; }
        public Dictionary<int, string> Categories { get; set; }
        public Dictionary<int, string> GroupTrainee { get; set; }
        public int[] GroupTraineeListID { get; set; }
        public int? GroupTraineeID { get; set; }
        public int? Category { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Sort { get; set; }
        public string ImgName { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
    }
}
