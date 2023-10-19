using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.ViewModels.Certificate
{
    public class CertificateViewModels
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        [AllowHtml]
        public string Template { get; set; }
        public string Code { get; set; }
        public string KeyTagMail { get; set; }
        public int TypeCertificate { get; set; }
        public int TypeCertificateID { get; set; }

    }
}
