using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using COEADGroupManager.Models;
using COEADGroupManagerSQL;
using COEADGroupManagerSecurityAccess;

namespace COEADGroupManager.Controllers
{

    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner,UCDUser")]
    public class HomeController : Controller
    {
       
        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        public ActionResult Index()
        {
            
            HomeIndexViewModel vmodel = new HomeIndexViewModel();

            if (User.IsInRole("ADGMAdmin") || User.IsInRole("ADGMPartner"))
            {
                vmodel.lAGMGroups = agmd.Get_All_AGMGroups();
            }
            else if (agmd.Check_For_Existing_AGMManager_By_KerbID(User.Identity.Name.ToString().ToLower()))
            {
                vmodel.lAGMGroups = agmd.Get_AGMGroups_For_Manager_By_KerbID(User.Identity.Name.ToString().ToLower());
            }

            return View(vmodel);
        }
    }
}