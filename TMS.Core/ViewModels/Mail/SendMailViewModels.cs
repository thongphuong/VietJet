using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Mail
{
    public class SendMailViewModels
    {
        public int? Id { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        [AllowHtml]
        public string TemplateMail { get; set; }
        public int? FlagSend { get; set; }

    }
}
