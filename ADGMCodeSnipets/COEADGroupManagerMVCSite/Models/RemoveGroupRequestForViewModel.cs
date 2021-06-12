using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class RemoveGroupRequestForViewModel
    {

        public string rgrStatus { get; set; }

        public string rmvGrpID { get; set; }

        public AGMGroup rmvGroup { get; set; }

        public RemoveGroupRequestForViewModel()
        {

        }

    }
}