using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class ManagersGroupsManagedByViewModel
    {
        public string mbStatus { get; set; }

        public AGMManager agmManager { get; set; }

        public ManagersGroupsManagedByViewModel()
        {

        }

    }
}