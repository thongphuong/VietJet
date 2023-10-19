using System.ComponentModel.DataAnnotations;


namespace TMS.Core.ViewModels.ViewModel
{
    public class NationalityViewModel
    {
        [Required(ErrorMessageResourceName = "NATIONALITYCODE_REQUIRED", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string code { get; set; }
        [Required(ErrorMessageResourceName = "NATIONALITYNAME_REQUIRED", ErrorMessageResourceType = typeof(App_GlobalResources.Messege))]
        public string name { get; set; }
        public string description { get; set; }
        public int? id { get; set; }
    }
}