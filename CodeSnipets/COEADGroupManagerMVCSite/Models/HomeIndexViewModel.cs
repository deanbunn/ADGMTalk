using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class HomeIndexViewModel
    {
        
        public string hiStatus { get; set; }
        public List<AGMGroup> lAGMGroups { get; set; }


        public HomeIndexViewModel()
        {
            lAGMGroups = new List<AGMGroup>();
        }


    }

}