using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using COEADGroupManager.Models;
using COEADGroupManagerSQL;
using UCDMemberADGM;
using COEADGroupManagerSecurityAccess;

namespace COEADGroupManager.Controllers
{

    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner")]
    public class ManagersController : Controller
    {

        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        //Initiate Role to OU Group Access
        COERoleToOUGroupAccess roga = new COERoleToOUGroupAccess();

        //Var for Regex of Kerb ID
        private const string regUCDIdnty = "^[a-zA-Z0-9\\-_\\.\\@]*$";
        private const string regEmailAddr = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";      // "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";


        //###################################
        // Index View 
        //###################################
        public ActionResult Index()
        {
            //Initiate View Model
            ManagersIndexViewModel vmodel = new ManagersIndexViewModel();

            //vmodel.miStatus = roga.CheckAccess(User.Identity.Name.ToString(), "wutang").ToString();

            return View(vmodel);
        }


        //##################################
        // List View
        //##################################
        public ActionResult List()
        {
            //Initiate View Model
            ManagersListViewModel vmodel = new ManagersListViewModel();

            //Load List of Managers
            vmodel.lManagers = agmd.Get_All_AGMManagers();

            return View(vmodel);
        }

        //##################################
        // Groups Managed By View
        //##################################
        public ActionResult GroupsManagedBy(string mgrid)
        {

            ManagersGroupsManagedByViewModel vmodel = new ManagersGroupsManagedByViewModel();

            if(!String.IsNullOrEmpty(mgrid))
            {
                if (Regex.IsMatch(mgrid, regUCDIdnty) == true)
                {
                    vmodel.agmManager = agmd.Get_AGMManager_By_KerbID(mgrid);

                    if(vmodel.agmManager == null || String.IsNullOrEmpty(vmodel.agmManager.KerbID))
                    {
                        vmodel.mbStatus = "The requested manager ID couldn't be found in the system";
                    }
                }
                else
                {
                    vmodel.mbStatus = "hmmm...no bueno data";
                }
            }
            else
            {
                vmodel.mbStatus = "hmmm...did you forget something";
            }

            return View(vmodel);
        }

        //##################################
        // Delete Manager
        //##################################
        [HttpGet]
        public ActionResult DeleteManager(string mgrid)
        {
            //Initiate View Model
            ManagersDeleteManagerViewModel vmodel = new ManagersDeleteManagerViewModel();

            if (!String.IsNullOrEmpty(mgrid))
            {
                if (Regex.IsMatch(mgrid, regUCDIdnty) == true)
                {
                    vmodel.agmManager = agmd.Get_AGMManager_By_KerbID(mgrid);

                    if (vmodel.agmManager == null || String.IsNullOrEmpty(vmodel.agmManager.KerbID))
                    {
                        vmodel.dmStatus = "The requested manager ID couldn't be found in the system";
                    }
                    else
                    {
                        //Set Required Manager User ID
                        vmodel.delMgrID = vmodel.agmManager.KerbID;
                    }
                }
                else
                {
                    vmodel.dmStatus = "hmmm...no bueno data";
                }
            }
            else
            {
                vmodel.dmStatus = "hmmm...did you forget something";
            }

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteManager(ManagersDeleteManagerViewModel vmodel, string btn1, string mgrid)
        {

            if (String.IsNullOrEmpty(btn1) == false && btn1 == "Remove")
            {

                if (!String.IsNullOrEmpty(mgrid))
                {

                    if (Regex.IsMatch(mgrid, regUCDIdnty) == true)
                    {
                        
                        vmodel.agmManager = agmd.Get_AGMManager_By_KerbID(mgrid);

                        if (vmodel.agmManager != null && String.IsNullOrEmpty(vmodel.agmManager.KerbID) == false)
                        {
                            
                            if(ModelState.IsValid && vmodel.delMgrID.ToLower() == mgrid.ToLower())
                            {
                                
                                //Remove Manager 
                                vmodel.dmStatus = agmd.Remove_AGMManager(vmodel.agmManager);

                                if(String.IsNullOrEmpty(vmodel.dmStatus))
                                {
                                    return RedirectToAction("List", "Managers");
                                }
                            }
                            else
                            {
                                vmodel.dmStatus = "hmmm...please talk to Dean before you do that again";
                            }
                        }
                        else
                        {
                            vmodel.dmStatus = "The requested manager ID couldn't be found in the system";
                        }
                    }
                    else
                    {
                        vmodel.dmStatus = "hmmm...no bueno data";
                    }
                }
                else
                {
                    vmodel.dmStatus = "hmmm...did you forget something";
                }

            }
            else
            {
                vmodel.dmStatus = "hmmm...did you forget something important";
            }

            return View(vmodel);
        }



        //###################################
        // Group List View
        //###################################
        public ActionResult GroupList(string grpguid)
        {
            
            //Initiate View Model
            ManagersGroupListViewModel vmodel = new ManagersGroupListViewModel();

            //Check Access to Group
            vmodel.glStatus = CheckAccessToAGMGroup(grpguid);

            if(String.IsNullOrEmpty(vmodel.glStatus))
            {
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));
            }

            return View(vmodel);
        }


        //##################################
        // Add Manager
        //##################################
        [HttpGet]
        public ActionResult AddManager()
        {
            //Initiate View Model
            ManagersGroupAddViewModel vmodel = new ManagersGroupAddViewModel();

            return PartialView(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddManager(ManagersGroupAddViewModel vmodel, string btn1)
        {

            //Group Access Check
            vmodel.gaStatus = CheckAccessToAGMGroup(vmodel.addGrpID);

            if (String.IsNullOrEmpty(btn1) == false && btn1 == "Add")
            {

                //If No Errors Proceed
                if (String.IsNullOrEmpty(vmodel.gaStatus))
                {

                    //Check Model State
                    if (ModelState.IsValid)
                    {

                        //Check Campus LDAP
                        CampusLDAP campusLDAP = new CampusLDAP();

                        //Check for Submitted Email Address
                        if (vmodel.addMgrID.Trim().ToLower().Contains("@"))
                        {
                            campusLDAP.SearchForUser(false, vmodel.addMgrID.Trim().ToLower());
                        }
                        else
                        {
                            campusLDAP.SearchForUser(true, vmodel.addMgrID.Trim().ToLower());
                        }

                        if (String.IsNullOrEmpty(campusLDAP.KerbID) == false)
                        {

                            //Pull AGM Group to Associate with Manager
                            AGMGroup agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(vmodel.addGrpID));
                            agmGroup.ModifiedBy = User.Identity.Name.ToString().ToLower();
                            agmGroup.ModifiedOn = DateTime.Now;

                            //Var for Existing Manager Status and Associated Manager
                            bool exstManager = agmd.Check_For_Existing_AGMManager_By_KerbID(campusLDAP.KerbID.ToLower());
                            bool asctManager = false;

                            //Check to See If Account is Already a Manager in Database
                            if (exstManager == true)
                            {

                                //Check for Existing Manager Association
                                if (agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0 && agmGroup.AGMManagers.Where(r => r.KerbID.ToLower() == campusLDAP.KerbID.ToLower()).Count() > 0)
                                {
                                    vmodel.gaStatus = "User is already a manager of that group";
                                    asctManager = true;
                                }

                                if (asctManager == false)
                                {
                                    //Var for Manager
                                    AGMManager agmManager = agmd.Get_AGMManager_By_KerbID(campusLDAP.KerbID.ToLower());
                                    
                                    //Make Association Between Manager and Group
                                    vmodel.gaStatus = agmd.Associate_Manager_to_AGMGroup(agmGroup, agmManager);
                                }

                            }
                            else
                            {
                                //For Non Existing Manager Create and Save to DB
                                AGMManager nAGMMgr = new AGMManager();
                                nAGMMgr.KerbID = campusLDAP.KerbID.ToLower();

                                //Assign Mail
                                if (!String.IsNullOrEmpty(campusLDAP.Mail))
                                {
                                    nAGMMgr.EmailAddress = campusLDAP.Mail.ToLower();
                                }

                                //Assign Display Name
                                if (!String.IsNullOrEmpty(campusLDAP.DisplayName))
                                {
                                    nAGMMgr.DisplayName = campusLDAP.DisplayName;
                                }

                                //Set Reporting Settings
                                nAGMMgr.DaysBtwnReport = 30;
                                nAGMMgr.SendAllGrpsRpt = false;

                                //Save to DB
                                vmodel.gaStatus = agmd.Associate_NewManager_to_AGMGroup(agmGroup, nAGMMgr);

                            }//End of Existing Manager Check


                            //If No Errors Then Send Back to Group Managers
                            if (String.IsNullOrEmpty(vmodel.gaStatus))
                            {
                                vmodel.gaStatus = "<script type=\"text/javascript\">location.reload();</script>";

                            }//End of Existing Manager Check

                        }
                        else
                        {
                            vmodel.gaStatus = "Couldn't find that user account in the Campus Directory";
                        }//End of Campus LDAP Kerb ID Null\Empty Check


                    }
                    else
                    {

                        //Notify User of Input Errors
                        vmodel.gaStatus = "<p>";

                        foreach (var ms in ModelState.Where(r => r.Value.Errors.Count > 0).ToArray())
                        {
                            foreach (var em in ms.Value.Errors)
                            {
                                vmodel.gaStatus += "<strong>" + em.ErrorMessage.ToString() + "</strong><br />";
                            }
                        }

                        vmodel.gaStatus += "</p>";

                    }//End of ModelState Check

                }//End of Group Access Check

            }
            else
            {
                vmodel.gaStatus = "hmmmm...no bueno";
            }//End of Button Action Check

            return Content(vmodel.gaStatus, "text/html");
        }

        //###########################################
        // Group Remove Manager
        //###########################################
        [HttpGet]
        public ActionResult RemoveGroup()
        {
            //Initiate View Model
            ManagersGroupRemoveViewModel vmodel = new ManagersGroupRemoveViewModel();

            return PartialView(vmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveGroup(ManagersGroupRemoveViewModel vmodel, string btn1)
        {
            //Check Action Button
            if (String.IsNullOrEmpty(btn1) == false && btn1 == "Remove")
            {
                //Check Access to Group
                vmodel.grStatus = CheckAccessToAGMGroup(vmodel.rmvGrpID);

                //
                if(String.IsNullOrEmpty(vmodel.grStatus))
                {

                    if(ModelState.IsValid)
                    {
                        //Pull AGM Group
                        AGMGroup agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(vmodel.rmvGrpID));
                        //Pull AGM Manager
                        AGMManager agmManager = agmd.Get_AGMManager_By_KerbID(vmodel.rmvMgrID);

                        //Remove Manager from 
                        if(agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0)
                        {
                            agmGroup.AGMManagers.Remove(agmManager);
                            vmodel.grStatus = agmd.Update_AGMGroup(agmGroup, User.Identity.Name.ToString().ToLower());

                            if(String.IsNullOrEmpty(vmodel.grStatus))
                            {
                                vmodel.grStatus = "<script type=\"text/javascript\">location.reload();</script>";
                            }

                        }
                        else
                        {
                            vmodel.grStatus = "The requested manager has already been remove";
                        }

                    }
                    else
                    {
                        //Notify User of Input Errors
                        vmodel.grStatus = "<p>";

                        foreach (var ms in ModelState.Where(r => r.Value.Errors.Count > 0).ToArray())
                        {
                            foreach (var em in ms.Value.Errors)
                            {
                                vmodel.grStatus += "<strong>" + em.ErrorMessage.ToString() + "</strong><br />";
                            }
                        }

                        vmodel.grStatus += "</p>";

                    }//End of ModelState Check

                }//End of Access to Group Check

            } 
            else
            {
                vmodel.grStatus = "hmmmm...no bueno";
            }//End of Button Action Check

            return Content(vmodel.grStatus, "text/html");
        }


        //############################################
        // Worker Functions
        //############################################
        private string CheckAccessToAGMGroup(string sGrpGuid)
        {
            //Var for Return Value
            string accessStatus = "Your account doesn't have access to this resource.";

            //Check to Group Guid String is Null or Empty
            if (string.IsNullOrEmpty(sGrpGuid) == false)
            {
                //Initiate Guid for Check
                Guid adGrpGuid;

                if (Guid.TryParse(sGrpGuid, out adGrpGuid))
                {
                    //Pull Requested Group
                    AGMGroup agmGrp = agmd.Get_AGMGroup_ByID(adGrpGuid);

                    if (agmGrp != null && String.IsNullOrEmpty(agmGrp.ADGrpDN) == false)
                    {

                        if (User.IsInRole("ADGMAdmin") || roga.CheckAccess(User.Identity.Name.ToString(),agmGrp.ADGrpDN))
                        {
                            return String.Empty;
                        }
                        else
                        {
                            accessStatus = "Your account doesn't have access to this resource.";
                        }

                    }
                    else
                    {
                        accessStatus = "Hmmmm...that group doesn't seem to be in our system.";
                    }

                }
                else
                {
                    accessStatus = "Hmmmm...the submitted information is not in correct format.";
                }

            }
            else
            {
                accessStatus = "Hmmmm...something is missing.";
            }

            return accessStatus;
        }


    }
}