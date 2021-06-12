using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class ReportConfigureManagerViewModel
    {

        public string rcmStatus { get; set; }

        [Required(ErrorMessage = "Send All Groups Report: Required")]
        public bool bSendReport { get; set; }

        [Required(ErrorMessage = "Reset Report Time: Required")]
        public bool bRprtTime { get; set; }

        [Required(ErrorMessage = "Days Option: Required")]
        public int nDaysBetween { get; set; }

        public AGMManager agmManager { get; set; }

        public ReportConfigureManagerViewModel()
        {

        }


    }
}