using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using COEADGroupManagerSQL;


namespace COEADGroupManager.Models
{
    public class ManagersListViewModel
    {
        public string mlStatus { get; set; }

        public List<AGMManager> lManagers { get; set; }
   
        public ManagersListViewModel()
        {
            lManagers = new List<AGMManager>();
        }

    }
}