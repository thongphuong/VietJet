using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
namespace TMS.Core.ViewModels.Mail
{
    public class MailViewModels
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Subject_Mail { get; set; }
        [AllowHtml]
        public string TemplateMail { get; set; }
        [Required]
        public string Code { get; set; }
        public string KeyTagMail { get; set; }
        

    }
}
