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
    public class DirectorySearchController : Controller
    {
        
        // GET: DirectorySearch
        public ActionResult Index()
        {
            //Initiate View Model 
            DirectorySearchIndexViewModel vmodel = new DirectorySearchIndexViewModel();

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DirectorySearchIndexViewModel vmodel)
        {

            if(ModelState.IsValid)
            {
                vmodel.rsltCollection = new ADGMUsrSearchADResultCollection(vmodel.uSearchTerm);
            }

            return View(vmodel);
        }
    }
}