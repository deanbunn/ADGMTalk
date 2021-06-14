using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class RequestLogEntryJsn
    {
        public string pending { get; set; }
        public string user_id { get; set; }
        public string action { get; set; }
        public string group { get; set; }
        public string requestor { get; set; }
        public string submitted_on { get; set; }


        public RequestLogEntryJsn()
        {
            pending = string.Empty;
            user_id = string.Empty;
            action = string.Empty;
            group = string.Empty;
            requestor = string.Empty;
            submitted_on = string.Empty;
        }
    }
}