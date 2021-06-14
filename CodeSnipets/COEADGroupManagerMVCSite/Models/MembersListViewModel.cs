using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class MembersListViewModel
    {
        
        public string liStatus { get; set; }

        public string uGrpID { get; set; }

        public AGMGroup agmGroup { get; set; }

        public List<ADGMuCntGrpMember> lUCGM { get; set; }

        public MembersListViewModel()
        {
            lUCGM = new List<ADGMuCntGrpMember>();
        }


    }
}