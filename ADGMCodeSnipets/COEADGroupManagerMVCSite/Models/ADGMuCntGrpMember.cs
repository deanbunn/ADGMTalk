using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COEADGroupManager.Models
{
    public class ADGMuCntGrpMember
    {
        
        public string LoginID { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string RequestStatus { get; set; }
        public string SubmittedOn { get; set; }
       
        public ADGMuCntGrpMember()
        {

        }

    }
}