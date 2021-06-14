using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class ReportConfigureViewModel
    {

        public string rcStatus { get; set; }

        public AGMGroup agmGroup { get; set; }

        [Required(ErrorMessage = "Limit Members: Required")]
        public bool bLimitMbr { get; set; }

        [Required(ErrorMessage = "Send Report: Required")]
        public bool bSendReport { get; set; }

        [Required(ErrorMessage = "Send to Managers: Required")]
        public bool bSendToManagers {get; set;}

        [Required(ErrorMessage = "Send HTML Report: Required")]
        public bool bSendHTMLReport { get; set; }

        [Required(ErrorMessage = "Send Provisioning Report: Required")]
        public bool bSendProvReport { get; set; }

        [Required(ErrorMessage = "Days Option: Required")]
        public int nDaysBetween { get; set; }

        [Required(ErrorMessage = "Limit Member Count: Required")]
        public int nLimitMbr { get; set; }

        [Required(ErrorMessage = "AD3 Admin Only: Required")]
        public bool bAD3AdminOnly { get; set; }

        [Required(ErrorMessage = "Group Description: Required")]
        public bool bGrpDescptAD { get; set; }

        [RegularExpression("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*", ErrorMessage = "Send To: Email Address Invalid Format")]
        public string addrRptSentTo { get; set; }

        //[RegularExpression("[a-zA-Z0-9\\-_\\@\\.\\'\\s]*", ErrorMessage = "Invalid Description")]
        public string cfgGrpDescpt { get; set; }

        public ReportConfigureViewModel()
        {

        }

    }
}