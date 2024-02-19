using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPService
{
    public class AppSettings
    {
        public string username { get; set; }
        public string password { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string zoho_auth_client { get; set; }
        public string code { get; set; }
        public string zoho_base_client { get; set; }
        public string mip_base_client { get; set; }
        public int timer { get; set; }
        public string OID { get; set; }
        public string AVSCode { get; set; }
        public string DOMandate { get; set; }
        public string PastYearAdmission { get; set; }
        public string ProcessDTTM { get; set; }
        public string mip { get; set; }
        public string Main { get; set; }
        public string Adult { get; set; }
        public string Spouse { get; set; }
        public string Child { get; set; }
        public string MainType { get; set; }
        public string SpouseType { get; set; }
        public string AdultType { get; set; }
        public string ChildType { get; set; }
        public string MainPrem { get; set; }
        public string Zoho { get; set; }
    }
}
