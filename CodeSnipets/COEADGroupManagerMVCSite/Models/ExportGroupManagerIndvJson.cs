using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class ExportGroupManagerIndvJson
    {
        public string kerb_id { get; set; }
        public string display_name { get; set; }
        public string email_address { get; set; }

        public ExportGroupManagerIndvJson()
        {
            kerb_id = string.Empty;
            display_name = string.Empty;
            email_address = string.Empty;
        }
    }
}