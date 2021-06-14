using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class ExportGroupManagersRptJson
    {
        public string group_name { get; set; }
        public string group_id { get; set; }
        public List<ExportGroupManagerIndvJson> managers { get; set; }

        public ExportGroupManagersRptJson()
        {
            group_name = string.Empty;
            group_id = string.Empty;
            managers = new List<ExportGroupManagerIndvJson>();
        }
    }
}