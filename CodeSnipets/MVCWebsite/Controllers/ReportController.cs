using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net.Mail;
using System.Text.RegularExpressions;
using COEADGroupManager.Models;
using COEADGroupManagerSQL;
using UCDMemberADGM;
using COEADGroupManagerSecurityAccess;

namespace COEADGroupManager.Controllers
{

    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner")]
    public class ReportController : Controller
    {

        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        //Initiate Role to OU Group Access
        COERoleToOUGroupAccess roga = new COERoleToOUGroupAccess();

        //Var for Regex of Kerb ID
        private const string regUCDIdnty = "^[a-zA-Z0-9\\-_\\.\\@]*$";

        //##########################
        // Index View
        //##########################
        public ActionResult Index()
        {
            ReportIndexViewModel vmodel = new ReportIndexViewModel();

            return View(vmodel);
        }

        //#########################
        // Send Group Reports
        //#########################
        public ActionResult SendGrpReports(string grpguid)
        {

            //Var for Return Message
            string rtnStatus = string.Empty;

            //Check User's Access to AD Group
            rtnStatus = CheckAccessToAGMGroup(grpguid);

            //Check If Access Error Were Returned
            if(String.IsNullOrEmpty(rtnStatus))
            {

                //Check Campus LDAP
                CampusLDAP campusLDAP = new CampusLDAP();

                //Search for User's Email Address
                campusLDAP.SearchForUser(true, User.Identity.Name.ToLower());

                //Null\Empty Check on User's Mail Listing in Campus Directory
                if(!String.IsNullOrEmpty(campusLDAP.Mail))
                {

                    UCDCampusServiceCred uCmpSrvCredO365 = agmd.Get_UCDCampusServiceCred_By_ServiceName("OFFICE365");

                    //Configure SMTP Client
                    SmtpClient ucdMail = new SmtpClient();
                    ucdMail.Host = uCmpSrvCredO365.ServiceURL1;
                    ucdMail.Credentials = new System.Net.NetworkCredential(uCmpSrvCredO365.ServiceAcnt, uCmpSrvCredO365.ServicePwd);
                    ucdMail.Port = 587;
                    ucdMail.EnableSsl = true;

                    //Mail Address for Automation Account
                    MailAddress maADGMAuto = new MailAddress(uCmpSrvCredO365.ServiceEmail);

                    //Mail Address for Current User
                    MailAddress maTo = new MailAddress(campusLDAP.Mail);

                    //Use Member View Model 
                    MembersGetMembersViewModel vmodel = new MembersGetMembersViewModel();

                    //Pull the AGM Group
                    vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                    //Load Group Members
                    vmodel.LoadGroupMembers();

                    //Var for Body Format HTML
                    StringBuilder sbMailMessageHTML = new StringBuilder();

                    //Start Format of HTML Report Table
                    sbMailMessageHTML.AppendLine("<html><body>");
                    sbMailMessageHTML.AppendLine("<br />");
                    sbMailMessageHTML.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\" width=\"500\" style=\"font-size:8pt;font-family:Arial,sans-serif\">");
                    sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
                    sbMailMessageHTML.AppendLine("<td colspan=\"3\" align=\"center\">");
                    sbMailMessageHTML.AppendLine("<strong><font color=\"#002855\" style=\"font-size:10pt\">");
                    sbMailMessageHTML.AppendLine(vmodel.agmGroup.ADGrpName + " on " + DateTime.Now.ToShortDateString());
                    sbMailMessageHTML.AppendLine("</font></strong>");
                    sbMailMessageHTML.AppendLine("</td>");
                    sbMailMessageHTML.AppendLine("</tr>");

                    //Managers Row
                    sbMailMessageHTML.AppendLine("<tr bgcolor=\"#4D688C\">");
                    sbMailMessageHTML.AppendLine("<td colspan=\"3\" align=\"left\">");
                    sbMailMessageHTML.AppendLine("<strong><font color=\"#FFFFFF\">");
                    sbMailMessageHTML.AppendLine("Group Managers");
                    sbMailMessageHTML.AppendLine("</font></strong>");
                    sbMailMessageHTML.AppendLine("</td>");
                    sbMailMessageHTML.AppendLine("</tr>");

                    //Managers Headers
                    sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">#</font></strong></td>");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">Display Name</font></strong></td>");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">Email Address</font></strong></td>");
                    sbMailMessageHTML.AppendLine("</tr>");

                    if (vmodel.agmGroup.AGMManagers != null && vmodel.agmGroup.AGMManagers.Count() > 0)
                    {
                        //Var for Manager Count
                        int mgrCnt = 0;

                        foreach (AGMManager agmManager in vmodel.agmGroup.AGMManagers)
                        {

                            //Increment Manager Count
                            mgrCnt++;

                            //Check for Row Count for Row Coloration 
                            if (mgrCnt % 2 == 0)
                            {
                                sbMailMessageHTML.AppendLine("<tr bgcolor=\"#E0E0E0\">");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<tr>");
                            }

                            //Add Row Count to Report
                            sbMailMessageHTML.AppendLine("<td>" + mgrCnt.ToString() + "</td>");

                           
                            //Check for Empty Display Name
                            if (!String.IsNullOrEmpty(agmManager.DisplayName))
                            {
                                
                                sbMailMessageHTML.AppendLine("<td>" + agmManager.DisplayName + "</td>");
                            }
                            else if (!String.IsNullOrEmpty(agmManager.KerbID))
                            {
                                
                                sbMailMessageHTML.AppendLine("<td>" + agmManager.KerbID + "</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td> </td>");
                            }


                            //Check for Empty Email Address
                            if (!String.IsNullOrEmpty(agmManager.EmailAddress))
                            {
                                //Add Email Address
                                sbMailMessageHTML.AppendLine("<td>" + agmManager.EmailAddress + "</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td> </td>");
                            }//End of Manager Email Address Check

                            //End HTML Row
                            sbMailMessageHTML.AppendLine("</tr>");

                        }//End of Managers Foreach

                    }
                    else
                    {
                        sbMailMessageHTML.AppendLine("<tr><td colspan=\"3\" align=\"center\">No Managers Configured</td></tr>");
                    }


                    //Group Members Row
                    sbMailMessageHTML.AppendLine("<tr bgcolor=\"#4D688C\">");
                    sbMailMessageHTML.AppendLine("<td colspan=\"3\" align=\"left\">");
                    sbMailMessageHTML.AppendLine("<strong><font color=\"#FFFFFF\">");
                    sbMailMessageHTML.AppendLine("Group Members");
                    sbMailMessageHTML.AppendLine("</font></strong>");
                    sbMailMessageHTML.AppendLine("</td>");
                    sbMailMessageHTML.AppendLine("</tr>");

                    //Managers Headers
                    sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">#</font></strong></td>");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">Display Name</font></strong></td>");
                    sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#002855\">Email Address</font></strong></td>");
                    sbMailMessageHTML.AppendLine("</tr>");


                    if(vmodel.lUCGM != null && vmodel.lUCGM.Count() > 0)
                    {
                        int mbrCnt = 0;

                        foreach (ADGMuCntGrpMember mbrRpt in vmodel.lUCGM.OrderBy(r => r.DisplayName))
                        {
                            //Increment Member Count
                            mbrCnt++;

                            //Check for Row Count for Row Coloration 
                            if (mbrCnt % 2 == 0)
                            {
                                sbMailMessageHTML.AppendLine("<tr bgcolor=\"#E0E0E0\">");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<tr>");
                            }

                            //Add Row Count to Report
                            sbMailMessageHTML.AppendLine("<td>" + mbrCnt.ToString() + "</td>");

                            //Check Group Member Display Name
                            if (!String.IsNullOrEmpty(mbrRpt.DisplayName))
                            {
                                
                                sbMailMessageHTML.AppendLine("<td>" + mbrRpt.DisplayName + "</td>");
                            }
                            else if (!String.IsNullOrEmpty(mbrRpt.LoginID))
                            {
                                
                                sbMailMessageHTML.AppendLine("<td>" + mbrRpt.LoginID.ToLower() + "</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td> </td>");
                            }

                            //Check Group Member Email Address
                            if (!String.IsNullOrEmpty(mbrRpt.EmailAddress))
                            {
                                
                                sbMailMessageHTML.AppendLine("<td>" + mbrRpt.EmailAddress.ToLower() + "</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td> </td>");
                            }

                            //End HTML Row
                            sbMailMessageHTML.AppendLine("</tr>");

                        }//End of Member Report Foreach

                    }
                    else
                    {
                        sbMailMessageHTML.AppendLine("<tr><td colspan=\"3\" align=\"center\">No Direct Members to Report On</td></tr>");
                    }

                    sbMailMessageHTML.AppendLine("</table><br /><br />");
                    sbMailMessageHTML.AppendLine("</body>");
                    sbMailMessageHTML.AppendLine("</html>");

                    //Mail Message for Group Report
                    MailMessage mailMSGGrpReport = new MailMessage();
                    mailMSGGrpReport.From = maADGMAuto;
                    mailMSGGrpReport.Subject = "ADGM Report for " + vmodel.agmGroup.ADGrpName + " on " + DateTime.Now.ToShortDateString();
                    mailMSGGrpReport.To.Add(maTo);
                    mailMSGGrpReport.IsBodyHtml = true;
                    mailMSGGrpReport.Body = sbMailMessageHTML.ToString();

                    //String Builder for Provision Completion Notice Body
                    StringBuilder sbProvCompNotice = new StringBuilder();
                    sbProvCompNotice.AppendLine("Provisioning Completion Notice for " + vmodel.agmGroup.ADGrpName);
                    sbProvCompNotice.AppendLine(" ");
                    sbProvCompNotice.AppendLine("Notice Timestamp: " + DateTime.Now.ToString());
                    sbProvCompNotice.AppendLine(" ");
                    sbProvCompNotice.AppendLine("Provisioning Request(s):");
                    sbProvCompNotice.AppendLine("----------------------------------");
                    sbProvCompNotice.AppendLine(" ");
                    //Set Add Action
                    sbProvCompNotice.AppendLine("Action: Add");
                    sbProvCompNotice.AppendLine("User ID: xxxx\\dbunn");
                    sbProvCompNotice.AppendLine("Requested By: aabunn");
                    sbProvCompNotice.AppendLine(" ");
                    //Set Add Action
                    sbProvCompNotice.AppendLine("Action: Remove");
                    sbProvCompNotice.AppendLine("User ID: xxxx\\ibunn");
                    sbProvCompNotice.AppendLine("Requested By: aabunn");
                    sbProvCompNotice.AppendLine(" ");
                    //Set Add Action
                    sbProvCompNotice.AppendLine("Action: Remove");
                    sbProvCompNotice.AppendLine("User ID: xxxx\\tbunn");
                    sbProvCompNotice.AppendLine("Requested By: szbunn");
                    sbProvCompNotice.AppendLine(" ");
                    sbProvCompNotice.AppendLine(" ");
                    sbProvCompNotice.AppendLine(" ");
                    sbProvCompNotice.AppendLine(" ");

                    //Configure Mail Message for Demo Completion Notice
                    MailMessage mailMSGProvNotice = new MailMessage();
                    mailMSGProvNotice.From = maADGMAuto;
                    mailMSGProvNotice.IsBodyHtml = false;
                    mailMSGProvNotice.Body = sbProvCompNotice.ToString();
                    mailMSGProvNotice.Subject = "[Example] ADGM provisioning completion notice for " + vmodel.agmGroup.ADGrpName;
                    mailMSGProvNotice.To.Add(maTo);

                    //Send Email Report and Notice (Fire in the Hole)
                    ucdMail.Send(mailMSGGrpReport);
                    ucdMail.Send(mailMSGProvNotice);

                    rtnStatus = "Report emails sent. Please check your " + campusLDAP.Mail.ToLower() + " mailbox";

                }
                else
                {
                    rtnStatus = "Couldn't pull your email address in Directory";
                }
                
            }//End of Access Check to AGM Group

            return Content(rtnStatus, "text/html");
        }

        
        //###############################
        // Configure Manager
        //###############################
        [HttpGet] 
        public ActionResult ConfigureManager(string mgrid)
        {
            //Initiate View Model
            ReportConfigureManagerViewModel vmodel = new ReportConfigureManagerViewModel();

            if (!String.IsNullOrEmpty(mgrid))
            {

                if (Regex.IsMatch(mgrid, regUCDIdnty) == true)
                {
                    //Pull Manager
                    vmodel.agmManager = agmd.Get_AGMManager_By_KerbID(mgrid);

                    //Null\Empty Check on Manager
                    if (vmodel.agmManager != null && String.IsNullOrEmpty(vmodel.agmManager.KerbID) == false)
                    {
                        //Pull Send Report Value
                        vmodel.bSendReport = (bool)vmodel.agmManager.SendAllGrpsRpt;

                        //Pull Days Between Value
                        vmodel.nDaysBetween = (int)vmodel.agmManager.DaysBtwnReport;

                        //Initialize Reset Report Time
                        vmodel.bRprtTime = false;

                    }
                    else
                    {
                        vmodel.rcmStatus = "The requested manager ID couldn't be found in the system";
                    }//End of Null\Empty Check on Manager

                }
                else
                {
                    vmodel.rcmStatus = "hmmm...no bueno data";
                }
            }
            else
            {
                vmodel.rcmStatus = "hmmm...did you forget something";
            }

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfigureManager(ReportConfigureManagerViewModel vmodel, string btn1, string mgrid)
        {

            if (!String.IsNullOrEmpty(mgrid))
            {

                if (Regex.IsMatch(mgrid, regUCDIdnty) == true)
                {
                    //Pull Manager
                    vmodel.agmManager = agmd.Get_AGMManager_By_KerbID(mgrid);

                    //Null\Empty Check on Manager
                    if (vmodel.agmManager != null && String.IsNullOrEmpty(vmodel.agmManager.KerbID) == false)
                    {
                        
                        if(ModelState.IsValid)
                        {

                            //Set Send All Group Reports Option
                            vmodel.agmManager.SendAllGrpsRpt = vmodel.bSendReport;

                            //Set Days Between Report
                            vmodel.agmManager.DaysBtwnReport = vmodel.nDaysBetween;

                            //Check to See If Need to Reset Report Time
                            if(vmodel.bRprtTime == true)
                            {
                                vmodel.agmManager.RptLastSent = null;
                            }

                            //Update Manager Changes in DB
                            vmodel.rcmStatus = agmd.Update_AGM_Manager(vmodel.agmManager);

                            //If No Errors Return to Manager Listing
                            if(String.IsNullOrEmpty(vmodel.rcmStatus))
                            {
                                return RedirectToAction("List", "Managers");
                            }

                        }//End of Model State Check

                    }
                    else
                    {
                        vmodel.rcmStatus = "The requested manager ID couldn't be found in the system";
                    }//End of Null\Empty Check on Manager

                }
                else
                {
                    vmodel.rcmStatus = "hmmm...no bueno data";
                }
            }
            else
            {
                vmodel.rcmStatus = "hmmm...did you forget something";
            }

            return View(vmodel);
        }
        

        //#########################
        // Configure 
        //#########################
        [HttpGet]
        public ActionResult Configure(string grpguid)
        {
            //Initiate View Model
            ReportConfigureViewModel vmodel = new ReportConfigureViewModel();

            //Check User's Access to AD Group
            vmodel.rcStatus = CheckAccessToAGMGroup(grpguid);

            //If No Issues Then Pull Group Information and Set Form Data
            if(String.IsNullOrEmpty(vmodel.rcStatus))
            {
                //Pull AGM Group 
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                //Pull Send Report Setting
                vmodel.bSendReport = (bool)vmodel.agmGroup.SendGrpReport;

                //Pull Send Managers Setting
                vmodel.bSendToManagers = (bool)vmodel.agmGroup.SendMgrReport;

                //Pull Send HTML Report
                vmodel.bSendHTMLReport = (bool)vmodel.agmGroup.SendHTMLRpt;

                //Pull Send Provisioning Reports
                vmodel.bSendProvReport = (bool)vmodel.agmGroup.SendProvisionRpts;

                //Pull AD3 Admin Only Setting
                vmodel.bAD3AdminOnly = (bool)vmodel.agmGroup.AD3AdminAcntOnly;

                //Pull Group Description in AD Setting
                vmodel.bGrpDescptAD = (bool)vmodel.agmGroup.GrpDescriptionAD;

                //Pull Number of Days Between Report
                vmodel.nDaysBetween = (int)vmodel.agmGroup.DaysBtwnReport;

                //Pull Number of Limit Members Count
                vmodel.nLimitMbr = (int)vmodel.agmGroup.MbrshpLimit;

                //Pull Limit Membership
                vmodel.bLimitMbr = (bool)vmodel.agmGroup.MbrshpLimited;

                //Pull Additional Email Address 
                if(!string.IsNullOrEmpty(vmodel.agmGroup.ReportAdtlAddr))
                {
                    vmodel.addrRptSentTo = vmodel.agmGroup.ReportAdtlAddr;
                }

                //Pull Group Description
                if(!string.IsNullOrEmpty(vmodel.agmGroup.GrpDescription))
                {
                    vmodel.cfgGrpDescpt = vmodel.agmGroup.GrpDescription;
                }

            }//End of Access Check

            return View(vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ReportConfigureViewModel vmodel, string btn1, string grpguid)
        {

            //Check User's Access to AD Group
            vmodel.rcStatus = CheckAccessToAGMGroup(grpguid);

            //If No Issues Then Pull Group Information and Set Form Data
            if (String.IsNullOrEmpty(vmodel.rcStatus))
            {
                //Pull AGM Group
                vmodel.agmGroup = agmd.Get_AGMGroup_ByID(Guid.Parse(grpguid));

                if(vmodel.bSendReport == true && vmodel.bSendToManagers == false && String.IsNullOrEmpty(vmodel.addrRptSentTo) == true)
                {
                    ModelState.AddModelError("Receipient:", "Please select someone to receive the report");
                }

                if(ModelState.IsValid == true && String.IsNullOrEmpty(btn1) == false && btn1 == "UpdateReport")
                {
                    //Set Send Report Setting
                    vmodel.agmGroup.SendGrpReport = vmodel.bSendReport;

                    //Set Send Managers Setting
                    vmodel.agmGroup.SendMgrReport = vmodel.bSendToManagers;

                    //Set Send HTML Report Setting
                    vmodel.agmGroup.SendHTMLRpt = vmodel.bSendHTMLReport;

                    //Set Send Provisioning Reports
                    vmodel.agmGroup.SendProvisionRpts = vmodel.bSendProvReport;

                    //Set AD3 Admin Only Setting
                    vmodel.agmGroup.AD3AdminAcntOnly = vmodel.bAD3AdminOnly;

                    //Set Group Description in AD 
                    vmodel.agmGroup.GrpDescriptionAD = vmodel.bGrpDescptAD;

                    //Set Number of Days Between Reports
                    vmodel.agmGroup.DaysBtwnReport = vmodel.nDaysBetween;

                    //Set Limit Membership Count
                    vmodel.agmGroup.MbrshpLimit = vmodel.nLimitMbr;

                    //Set Limit Membership Setting
                    vmodel.agmGroup.MbrshpLimited = vmodel.bLimitMbr;

                    //Set Additional Address
                    if(!String.IsNullOrEmpty(vmodel.addrRptSentTo))
                    {
                        vmodel.agmGroup.ReportAdtlAddr = vmodel.addrRptSentTo.ToLower();
                    }

                    //Set Group Description
                    if(vmodel.bGrpDescptAD == false)
                    {

                        if(string.IsNullOrEmpty(vmodel.cfgGrpDescpt) == false)
                        {
                            vmodel.agmGroup.GrpDescription = vmodel.cfgGrpDescpt;
                        }
                        else
                        {
                            vmodel.agmGroup.GrpDescription = string.Empty;
                        }

                    }


                    //Update AGM Group
                    vmodel.rcStatus = agmd.Update_AGMGroup(vmodel.agmGroup, User.Identity.Name.ToLower());

                    //Check for Errors
                    if(String.IsNullOrEmpty(vmodel.rcStatus))
                    {
                        return RedirectToAction("index", "home");
                    }


                }//End of ModelState and Button Action Check

            }//End of Access Check

            return View(vmodel);
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

                        if (User.IsInRole("ADGMAdmin") || roga.CheckAccess(User.Identity.Name.ToString(), agmGrp.ADGrpDN))
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