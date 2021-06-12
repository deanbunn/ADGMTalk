using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using COEADGroupManager.Models;
using COEADGroupManagerSQL;
using UCDMemberADGM;
using COEADGroupManagerSecurityAccess;

namespace COEADGroupManager.Controllers
{
    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner,UCDUser")]
    public class MembersController : Controller
    {
        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        //Initiate Role to OU Group Access
        COERoleToOUGroupAccess roga = new COERoleToOUGroupAccess();

        //Var for Regex of Kerb ID
        private const string regUCDIdnty = "^[a-zA-Z0-9\\-_\\.\\@]*$";
        private const string regEmailAddr = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";      // "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";


        //#########################################
        // Index View 
        //#########################################
        public ActionResult Index(string grpguid)
        {
            
            //Initiate View Model
            MembersIndexViewModel vmodel = new MembersIndexViewModel();

            vmodel.miStatus = "hmmmm...forget something?";
            
            return View(vmodel);
        }

        //##########################################
        // List View
        //##########################################
        public ActionResult List(string grpguid)
        {
            //Initiate View Model
            MembersListViewModel vmodel = new MembersListViewModel();

            //Check Access to Group 
            vmodel.liStatus = CheckAccessToAGMGroup(grpguid);

            //If Go then Pull Group
            if(String.IsNullOrEmpty(vmodel.liStatus))
            {
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));
            }

            return View(vmodel);
        }

        //#########################################
        // Export to CSV
        //#########################################
        public FileResult CurrentMembersExport(string grpguid)
        {
            //Create MemoryStream and StreamWriter
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output);

            DateTime rptDate = DateTime.Now;
            string rptFilename = "uConnect_Group_Report_" + rptDate.ToString("yyyyMMddHHmmss") + ".csv";

            //Initiate View Model
            MembersGetMembersViewModel vmodel = new MembersGetMembersViewModel();

            vmodel.gmStatus = CheckAccessToAGMGroup(grpguid);

            if (String.IsNullOrEmpty(vmodel.gmStatus))
            {
                //Pull the AGM Group
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                //Load Group Members
                vmodel.LoadGroupMembers();

                //Write Out Header Information
                writer.Write("User ID,");
                writer.Write("Display Name,");
                writer.Write("Email Address,");
                writer.WriteLine();

                //Check for Members to Report On
                if(vmodel.lUCGM != null && vmodel.lUCGM.Count() > 0)
                {

                    foreach(ADGMuCntGrpMember ucgm in vmodel.lUCGM.OrderBy(r => r.DisplayName))
                    {
                        writer.Write(CSV_String_Check(ucgm.LoginID));
                        writer.Write(CSV_String_Check(ucgm.DisplayName));
                        writer.Write(CSV_String_Check(ucgm.EmailAddress));
                        writer.WriteLine();
                    }


                }//End of 

                rptFilename = vmodel.agmGroup.ADGrpName.Trim().Replace(" ", "_") + "_" + rptDate.ToString("yyyyMMddHHmmss") + ".csv";

            }//End of gmStatus Check


            //Flush StreamWriter and Move MemoryStream to Top Position
            writer.Flush();
            output.Position = 0;

            //Return the File Output
            return File(output, "text/csv", rptFilename);

        }

        //#########################################
        // Get Members View
        //#########################################
        public ActionResult GetMembers(string grpguid)
        {
            //Initiate View Model
            MembersGetMembersViewModel vmodel = new MembersGetMembersViewModel();

            vmodel.gmStatus = CheckAccessToAGMGroup(grpguid);

            if(String.IsNullOrEmpty(vmodel.gmStatus))
            {
                //Pull the AGM Group
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));
                
                //Load Group Members
                vmodel.LoadGroupMembers();

                //Check for Pending Requests
                if (vmodel.agmGroup.AGMMemberRequests != null 
                    && vmodel.agmGroup.AGMMemberRequests.Count() > 0 
                    && vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true).Count() > 0)
                {

                    //Check for Pending Removals
                    if (vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Remove").Count() > 0)
                    {
                        //Pull List of Pending Removal Requests
                        List<AGMMemberRequest> lRemoveRqsts = vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Remove").ToList();

                        List<string> lRemoveUserIDs = lRemoveRqsts.Select(r => r.KerbID.ToLower()).ToList();

                        foreach (ADGMuCntGrpMember grpMbr in vmodel.lUCGM.Where(r => lRemoveUserIDs.Contains(r.LoginID.ToLower())))
                        {

                            grpMbr.RequestStatus = "Remove";

                            AGMMemberRequest rmvRequest = lRemoveRqsts.Where(r => r.KerbID.ToLower() == grpMbr.LoginID.ToLower()).FirstOrDefault();

                            if (rmvRequest != null)
                            {
                                grpMbr.SubmittedOn = rmvRequest.SubmittedOn.Value.ToString("MM/dd/yyyy hh:mm tt");
                            }

                        }

                    }//End of Pending Removals Requests

                    //Check for Pending Additions
                    if (vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Add").Count() > 0)
                    {
                        
                        List<AGMMemberRequest> lAddUsers = vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Add").ToList();

                        foreach(AGMMemberRequest agmMbrRqst in lAddUsers)
                        {
                            ADGMuCntGrpMember addGMbr = new ADGMuCntGrpMember();
                            addGMbr.RequestStatus = "Add";
                            addGMbr.LoginID = agmMbrRqst.KerbID;
                            addGMbr.SubmittedOn = agmMbrRqst.SubmittedOn.Value.ToString("MM/dd/yyyy hh:mm tt");
                            vmodel.lUCGM.Add(addGMbr);
                        }

                    }//End of Pending Additions

                }//Check for Pending Requests

            }

            return PartialView(vmodel);
        }

        //############################################
        // Add Members View
        //############################################
        [HttpGet]
        public ActionResult AddMembers(string grpguid)
        {
            //Initiate the View Model
            MembersAddMemberViewModel vmodel = new MembersAddMemberViewModel();

            //Check Access to Group
            vmodel.amStatus = CheckAccessToAGMGroup(grpguid);

            //If No Errors Load Model
            if(String.IsNullOrEmpty(vmodel.amStatus))
            {
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));
            }

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMembers(MembersAddMemberViewModel vmodel, string btn1, string grpguid)
        {
            //Check Access to Group
            vmodel.amStatus = CheckAccessToAGMGroup(grpguid);

            //List for User IDs to Save as Provision Requests
            List<string> lProvUserIDs = new List<string>();

            //Check Access Results 
            if(String.IsNullOrEmpty(vmodel.amStatus))
            {
                
                //Load Group
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                //Check Button's Action
                if(String.IsNullOrEmpty(btn1) == false && btn1 == "Add")
                {

                    //Check ModelState
                    if(ModelState.IsValid)
                    {
                        //Var for Process Requests Check
                        bool bProcessRqsts = false;

                        //Var for Process Request Status
                        string sProcessRqstStatus = string.Empty;

                        //String Array for Submitted User IDs
                        string[] smUserIdnts = vmodel.nUsers.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                        //Limits Checks 
                        if((bool)vmodel.agmGroup.MbrshpLimited == false)
                        {
                            bProcessRqsts = true;
                        }
                        else
                        {

                            //Initiate Limit Worker
                            ADGMGrpLimitWrkr adLimitWrkr = new ADGMGrpLimitWrkr();

                            //Pull Current Member Count
                            adLimitWrkr.Get_Current_Membership_Count(vmodel.agmGroup.AGMGID.ToString());

                            //Var for Limit Counts
                            int nPendingAdds = 0;
                            int nPendingRmvs = 0;
                            int MbrshpLimit = (int)vmodel.agmGroup.MbrshpLimit;

                            //Count Pending Actions
                            if(vmodel.agmGroup.AGMMemberRequests != null && vmodel.agmGroup.AGMMemberRequests.Count() > 0)
                            {
                                nPendingAdds = vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Add").Count();
                                nPendingRmvs = vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Remove").Count();
                            }

                            //sProcessRqstStatus = adLimitWrkr.nCrntMbrCount.ToString();

                            //Check to See If Group has Passed Limits
                            if (MbrshpLimit >= ((adLimitWrkr.nCrntMbrCount + smUserIdnts.Count() + nPendingAdds) - nPendingRmvs))
                            {
                                bProcessRqsts = true;
                            }
                            else
                            {
                                sProcessRqstStatus = "Request exceeds set nested membership limit";
                            }

                        }//End of Limits Checks


                        //Process Requests Status Check
                        if(bProcessRqsts == true)
                        {

                            //Verify Each Submitted User ID
                            foreach (string smUserIdnt in smUserIdnts)
                            {

                                //Check for Correct UCD Identity Format
                                if (Regex.IsMatch(smUserIdnt.Trim(), regUCDIdnty))
                                {
                                    //Var for What to Search AD By
                                    string usrSrchBy = string.Empty;

                                    //Check for Submitted Email Address
                                    if (smUserIdnt.Trim().ToLower().Contains("@"))
                                    {
                                        usrSrchBy = "mail";
                                    }
                                    else
                                    {
                                        usrSrchBy = "sAMAccountName";
                                    }

                                    //Search uConnect for Submitted Account
                                    UCDADUser ucdADUser = new UCDADUser("XXXXX", usrSrchBy, smUserIdnt.Trim());

                                    //If AD Search Turns Up Nothing via Email Switch to Looking in Campus LDAP for User ID
                                    if (String.IsNullOrEmpty(ucdADUser.SAM) == true && usrSrchBy == "mail")
                                    {

                                        //Still Empty Check by UPN  userPrincipalName
                                        ucdADUser = new UCDADUser("XXXX", "userPrincipalName", smUserIdnt.Trim());

                                        if (String.IsNullOrEmpty(ucdADUser.SAM) == true)
                                        {
                                            //Check Campus LDAP
                                            CampusLDAP campusLDAP = new CampusLDAP();
                                            campusLDAP.SearchForUser(false, smUserIdnt);

                                            //If KerbID Returned Search AD3 for that to Pull Account
                                            if (!String.IsNullOrEmpty(campusLDAP.KerbID))
                                            {
                                                ucdADUser = new UCDADUser("XXXX", "sAMAccountName", campusLDAP.KerbID);
                                            }
                                        }

                                    }//End of Failed AD3 Mail LDAP ReLookup

                                    //Check to See If AD User ID was Returned
                                    if (!String.IsNullOrEmpty(ucdADUser.SAM))
                                    {
                                        //Check for Pending Provision Request and Then Existing Group Membership
                                        if (vmodel.agmGroup.AGMMemberRequests != null
                                            && vmodel.agmGroup.AGMMemberRequests.Count() > 0
                                            && vmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.KerbID.ToLower() == ucdADUser.SAM.ToLower()).Count() > 0)
                                        {
                                            vmodel.lErrorMsgs.Add(smUserIdnt + " has a pending membership request");
                                        }
                                        else if (ucdADUser.CheckGroupMember(vmodel.agmGroup.ADGrpDN))
                                        {
                                            vmodel.lErrorMsgs.Add(smUserIdnt + " is already a group member");
                                        }
                                        else if (ucdADUser.SAM.ToLower().StartsWith("admin-") == false && (bool)vmodel.agmGroup.AD3AdminAcntOnly == true)
                                        {
                                            vmodel.lErrorMsgs.Add(smUserIdnt + " is not an admin account");
                                        }
                                        else
                                        {
                                            //Add SAM to Provision List
                                            lProvUserIDs.Add(ucdADUser.SAM.ToLower());

                                        }//End of Pending and Membership Checks

                                    }
                                    else
                                    {
                                        vmodel.lErrorMsgs.Add(smUserIdnt + " couldn't be found in requested domain");
                                    }//End of AD Check

                                }
                                else
                                {
                                    vmodel.lErrorMsgs.Add(smUserIdnt + " has an invalid character");
                                }//End of Regex Check



                            }//End of smUserIdnts Foreach


                            //Check for Error Messages
                            if (vmodel.lErrorMsgs.Count == 0)
                            {

                                //Check Account to Provision Count
                                if (lProvUserIDs.Count > 0)
                                {
                                    //Loop Through User IDs to Provision and Submit Request
                                    foreach (string pvUsrID in lProvUserIDs)
                                    {
                                        //If Error Occurred on Previous DB Submission Then Don't Add More
                                        if (String.IsNullOrEmpty(vmodel.amStatus))
                                        {
                                            //Create Member Request
                                            AGMMemberRequest nAGMRequest = new AGMMemberRequest();
                                            nAGMRequest.KerbID = pvUsrID;
                                            nAGMRequest.MRAction = "Add";
                                            nAGMRequest.Pending = true;
                                            nAGMRequest.SubmittedBy = User.Identity.Name.ToString().ToLower();
                                            nAGMRequest.SubmittedOn = DateTime.Now;
                                            nAGMRequest.ADGroupName = vmodel.agmGroup.ADGrpName;
                                            nAGMRequest.AGMGroups.Add(vmodel.agmGroup);

                                            //Add Request to Database
                                            vmodel.amStatus = agmd.Add_AGMMemberRequest(nAGMRequest);

                                        }//End of Last model.amStatus Check Before Submitting to DB

                                    }//End of lProvUserIDs Foreach

                                    //Check Add Status
                                    if (String.IsNullOrEmpty(vmodel.amStatus))
                                    {
                                        //Update Group's Modify Stamp
                                        agmd.Update_AGMGroup_ModifyTime(vmodel.agmGroup, User.Identity.Name.ToString().ToLower());

                                        return RedirectToAction("List", "Members", new { grpguid = vmodel.agmGroup.AGMGID.ToString() });
                                    }
                                    else
                                    {
                                        vmodel.lErrorMsgs.Add("An error occurred attempting to save membership request to the database");
                                    }

                                }
                                else
                                {
                                    vmodel.lErrorMsgs.Add("No accounts were able to be submitted to the database");
                                }

                            }//End of lErrorMsgs Count Check

                        }
                        else
                        {
                            vmodel.lErrorMsgs.Add(sProcessRqstStatus);

                        }//End of Process Requests Status
                        

                    }//End of ModelState Check

                }
                else
                {
                    vmodel.amStatus = "Hmmmm...no action submitted.";
                }

            }//End of Access Result Check
            
            return View(vmodel);
        }

        //############################################
        // Bulk Remove
        //############################################
        [HttpGet]
        public ActionResult BulkRemove(string grpguid)
        {

            //Initiate View Model  
            MembersBulkRemoveViewModel vmodel = new MembersBulkRemoveViewModel();

            //Check Access to Group
            vmodel.brStatus = CheckAccessToAGMGroup(grpguid);

            //If No Errors Load Model
            if (String.IsNullOrEmpty(vmodel.brStatus))
            {

                //Initiate GetMembersModel 
                MembersGetMembersViewModel gmodel = new MembersGetMembersViewModel();

                List<string> lRemoveUserIDs = new List<string>();

                //Pull AGM Group from DB
                gmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                //Pull AD Group Membership
                gmodel.LoadGroupMembers();

                //Check for Pending Requests
                if (gmodel.agmGroup.AGMMemberRequests != null
                    && gmodel.agmGroup.AGMMemberRequests.Count() > 0
                    && gmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Remove").Count() > 0)
                {
                    lRemoveUserIDs = gmodel.agmGroup.AGMMemberRequests.Where(r => r.Pending == true && r.MRAction == "Remove").Select(c => c.KerbID.ToLower()).ToList();
                }

                //Load Select List
                if (gmodel.lUCGM != null && gmodel.lUCGM.Count() > 0)
                {

                    foreach(ADGMuCntGrpMember ucgm in gmodel.lUCGM.OrderBy(r => r.DisplayName))
                    {

                        if(!lRemoveUserIDs.Contains(ucgm.LoginID.ToLower()))
                        {
                            ADGMuCntGrpMemberSelectItem ucgmSel = new ADGMuCntGrpMemberSelectItem();
                            ucgmSel.Selected = false;
                            ucgmSel.LoginID = ucgm.LoginID;
                            ucgmSel.DisplayName = ucgm.DisplayName;
                            vmodel.lUGMSI.Add(ucgmSel);
                        }//End of LoginID Check in RemoveUsers List Check
                        
                    }//End of gmodel.lUCGM Foreach

                }//End of lUCGM Null\Empty Check
               
            }//End of brStatus Null\Empty Check

            return PartialView(vmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkRemove(MembersBulkRemoveViewModel vmodel, string grpguid)
        {

            //Check Access to Group
            vmodel.brStatus = CheckAccessToAGMGroup(grpguid);

            //If No Errors Load Model
            if (String.IsNullOrEmpty(vmodel.brStatus))
            {

                //Pull Group
                AGMGroup agmGrp = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));


                if(vmodel.lUGMSI != null && vmodel.lUGMSI.Count() > 0 && vmodel.lUGMSI.Where(r => r.Selected == true).Count() > 0)
                {

                    foreach (ADGMuCntGrpMemberSelectItem ucgmsi in vmodel.lUGMSI.Where(r => r.Selected == true))
                    {
                       
                        if(String.IsNullOrEmpty(vmodel.brStatus))
                        {
                            //Create Member Request
                            AGMMemberRequest nAGMRequest = new AGMMemberRequest();
                            nAGMRequest.KerbID = ucgmsi.LoginID;
                            nAGMRequest.MRAction = "Remove";
                            nAGMRequest.Pending = true;
                            nAGMRequest.SubmittedBy = User.Identity.Name.ToString().ToLower();
                            nAGMRequest.SubmittedOn = DateTime.Now;
                            nAGMRequest.ADGroupName = agmGrp.ADGrpName;
                            nAGMRequest.AGMGroups.Add(agmGrp);

                            //Add Request to Database
                            vmodel.brStatus = agmd.Add_AGMMemberRequest(nAGMRequest);
                        }
                        

                    }//End of lUGMSI Selected Foreach


                    //Update Group's Modify Stamp
                    agmd.Update_AGMGroup_ModifyTime(agmGrp, User.Identity.Name.ToString().ToLower());

                }//End of lUGMSI Checked Check

            
                //If No Errors Then Send Close and Reload 
                if (String.IsNullOrEmpty(vmodel.brStatus))
                {

                    vmodel.brStatus = "<script type=\"text/javascript\">location.reload();</script>";
                }

            }//End of Access Check to AGM Group Check

            return Content(vmodel.brStatus, "text/html");
        }

        //############################################
        // Remove Member View
        //############################################
        [HttpGet]
        public ActionResult RemoveMember()
        {
            //Initiate View Model
            MembersRemoveMemberViewModel vmodel = new MembersRemoveMemberViewModel();
            
            return PartialView(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMember(MembersRemoveMemberViewModel vmodel, string btn1)
        {
            //Check Access to Group
            vmodel.rmStatus = CheckAccessToAGMGroup(vmodel.rmvGrpID);

            //Check for Access Error Messages
            if(String.IsNullOrEmpty(vmodel.rmStatus))
            {
                if(ModelState.IsValid)
                {
                    //Pull Group
                    AGMGroup agmGrp = agmd.Get_AGMGroup_ByID(Guid.Parse(vmodel.rmvGrpID));

                    //Create Member Request
                    AGMMemberRequest nAGMRequest = new AGMMemberRequest();
                    nAGMRequest.KerbID = vmodel.rmvUserID;
                    nAGMRequest.MRAction = "Remove";
                    nAGMRequest.Pending = true;
                    nAGMRequest.SubmittedBy = User.Identity.Name.ToString().ToLower();
                    nAGMRequest.SubmittedOn = DateTime.Now;
                    nAGMRequest.ADGroupName = agmGrp.ADGrpName;
                    nAGMRequest.AGMGroups.Add(agmGrp);
                    
                    //Add Request to Database
                    vmodel.rmStatus = agmd.Add_AGMMemberRequest(nAGMRequest);

                    //If No Errors Returned Then Send Reload the Page
                    if (String.IsNullOrEmpty(vmodel.rmStatus))
                    {
                        //Update Group's Modify Stamp
                        agmd.Update_AGMGroup_ModifyTime(agmGrp, User.Identity.Name.ToString().ToLower());

                        vmodel.rmStatus = "<script type=\"text/javascript\">location.reload();</script>";
                    }
                }
                else
                {
                    //Notify User of Input Errors
                    vmodel.rmStatus = "<p>";

                    foreach (var ms in ModelState.Where(r => r.Value.Errors.Count > 0).ToArray())
                    {
                        foreach (var em in ms.Value.Errors)
                        {
                            vmodel.rmStatus += "<strong>" + em.ErrorMessage.ToString() + "</strong><br />";
                        }
                    }

                    vmodel.rmStatus += "</p>";

                }//End of ModelState Check

            }//End of Access Null\Empty Check

            return Content(vmodel.rmStatus, "text/html");
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

                if(Guid.TryParse(sGrpGuid, out adGrpGuid))
                {
                    //Pull Requested Group
                    AGMGroup agmGrp = agmd.Get_AGMGroup_ByID(adGrpGuid);

                    if(agmGrp != null && String.IsNullOrEmpty(agmGrp.ADGrpDN) == false)
                    {

                        if (User.IsInRole("ADGMAdmin") || 
                            agmGrp.AGMManagers.Any(r => r.KerbID.ToLower().Contains(User.Identity.Name.ToLower())) ||
                            roga.CheckAccess(User.Identity.Name.ToString(), agmGrp.ADGrpDN))
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


        //CSV String Check
        public string CSV_String_Check(string sInput)
        {
            string sReturn = string.Empty;

            if (!string.IsNullOrEmpty(sInput))
            {
                sReturn = sInput;

                if (sReturn.StartsWith("0") || sReturn.StartsWith("-"))
                {
                    sReturn = "\'" + sReturn;
                }

                if (sReturn.Contains(",") || sReturn.Contains("\""))
                {
                    sReturn = "\"" + sReturn + "\"";
                }

                sReturn = sReturn + ",";
            }
            else
            {
                sReturn = ",";
            }

            return sReturn;
        }


    }
}