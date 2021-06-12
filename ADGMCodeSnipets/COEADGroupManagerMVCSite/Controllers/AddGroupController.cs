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
    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner")]
    public class AddGroupController : Controller
    {

        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        //##################################
        // Add Group Index Views
        //##################################
        [HttpGet]
        public ActionResult Index()
        {
            //Initiate View Model
            AddGroupIndexViewModel vmodel = new AddGroupIndexViewModel();

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AddGroupIndexViewModel vmodel)
        {
            //Check ModelState and Then Search AD
            if(ModelState.IsValid)
            {
                vmodel.adGrpCol = new ADGMGrpSrchCollection(vmodel.srchDomain, vmodel.srchName);
            }

            return View(vmodel);
        }

        //##################################
        // New ADGM Group View
        //##################################
        public ActionResult NewADGMGroup(string grpguid)
        {
            //Initiate View Model
            AddGroupNewADGMGroupViewModel vmodel = new AddGroupNewADGMGroupViewModel();

            //Check to Group Guid String is Null or Empty
            if (string.IsNullOrEmpty(grpguid) == false)
            {

                //Initiate Guid for Check
                Guid adGrpGuid;

                //Try to Parse Guid String. If Successful Look Up Group in AD and Addmin to DB
                if (Guid.TryParse(grpguid, out adGrpGuid))
                {
                    //Create Instance of ADGM Group Verifier for Lookup
                    ADGMGrpVerifier adgmVerf = new ADGMGrpVerifier();
                    adgmVerf.Check_uConnect_Group_By_Guid(grpguid);

                    if(adgmVerf.uADGrpStatus == true && string.IsNullOrEmpty(adgmVerf.uADGrpName) == false)
                    {
                        //Create New AGM Group and Assign Values
                        AGMGroup nAGMGroup = new AGMGroup();
                        nAGMGroup.AGMGID = adGrpGuid;
                        nAGMGroup.ADGrpDN = adgmVerf.uADGrpDN;
                        nAGMGroup.ADGrpName = adgmVerf.uADGrpName;
                        nAGMGroup.ModifiedBy = User.Identity.Name.ToLower();
                        nAGMGroup.ModifiedOn = DateTime.Now;
                        nAGMGroup.DaysBtwnReport = 30;
                        nAGMGroup.SendGrpReport = false;
                        nAGMGroup.SendMgrReport = true;
                        nAGMGroup.SendHTMLRpt = true;
                        nAGMGroup.SendProvisionRpts = false;
                        nAGMGroup.MbrshpLimit = 500;
                        nAGMGroup.MbrshpLimited = false;
                        nAGMGroup.AD3AdminAcntOnly = false;
                        nAGMGroup.GrpDescriptionAD = false;

                        //Add New AGM Group
                        if(agmd.Check_For_AdminGroupGuid(nAGMGroup.AGMGID) == false)
                        {
                            vmodel.nStatus = agmd.Add_AGMGroup(nAGMGroup);
                        }
                        else
                        {
                            vmodel.nStatus = "COE Admin Groups Cannot Be Added";
                        }

                        //Check Add Status
                        if(String.IsNullOrEmpty(vmodel.nStatus))
                        {
                            return RedirectToAction("Index", "Home");
                        }

                    }//End of Worker Chek and Grp Name Null\Emtpy Check


                }
                else
                {
                    vmodel.nStatus = "No go";
                }

            }//End of String Guid Null\Empty Check

            return View();
        }



    }
}