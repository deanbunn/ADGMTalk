using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class ADGMGrpSrchResult
    {

        public Guid GrpGuid { get; set; }
        public string GrpCN { get; set; }
        public string GrpDisplayName { get; set; }
        public string GrpDN { get; set; }
        public bool GrpExistingStatus { get; set; }

        public ADGMGrpSrchResult()
        {

        }

    }
}