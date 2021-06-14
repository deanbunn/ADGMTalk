using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class ADGMUsrSearchADResult
    {
        public string user_id { get; set; }
        public string common_name { get; set; }
        public string display_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email_address { get; set; }
        public string ad_domain { get; set; }
        public string ad_object_guid { get; set; }
        public string ad_dn { get; set; }
        public string ad_enabled { get; set; }
        public string title { get; set; }
        public string department { get; set; }


        public ADGMUsrSearchADResult()
        {
            user_id = string.Empty;
            common_name = string.Empty;
            display_name = string.Empty;
            first_name = string.Empty;
            last_name = string.Empty;
            email_address = string.Empty;
            ad_domain = string.Empty;
            ad_object_guid = string.Empty;
            ad_dn = string.Empty;
            ad_enabled = string.Empty;
            title = string.Empty;
            department = string.Empty;
        }


    }
}