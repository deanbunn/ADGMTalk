using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class MembersBulkRemoveViewModel
    {

        public string brStatus { get; set; }

        public List<ADGMuCntGrpMemberSelectItem> lUGMSI { get; set; }

        public MembersBulkRemoveViewModel()
        {
            lUGMSI = new List<ADGMuCntGrpMemberSelectItem>();
        }
    }
}