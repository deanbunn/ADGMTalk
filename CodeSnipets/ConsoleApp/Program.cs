using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Threading;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;
using COEADGroupManagerSQL;

namespace COEADGroupManagerConApp
{
    class Program
    {

        //Initialize Database Access
        static COEADGroupManagerDBRepository adgm = new COEADGroupManagerDBRepository();

        static void Main(string[] args)
        {

            //Pull Current Date Time
            DateTime dtCrntly = DateTime.Now;

            //Provision Pending Requests
            Provision_Pending_Requests();

            //Process Manager Disaffiliation Requests
            Process_Manager_Disaffiliate_Requests(dtCrntly);

            //Run Snapshot Managers and AD Info Update and Reporting Every Six Hours
            if (dtCrntly.Hour % 6 == 0 && dtCrntly.Minute < 5)
            {
                Update_AGMGroups_AD_Info();
                Snapshot_Managers();
            }

            //Check for Send All Groups Reports
            Send_All_Groups_Reports_For_Managers();

            //Run Manager Account Check 
            if (dtCrntly.Hour == 2 && dtCrntly.Minute < 5)
            {
                Check_Managers_UCD_Account_Status();
            }

            
        }

        //########################################
        // End of Dev Run
        //########################################
        public static void End_of_Dev_Run()
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine("All Done!");
            Console.ReadLine();
        }

        //########################################
        // Update Group Descriptions
        //########################################
        public static void Update_AGMGroups_Descriptions()
        {
            //Create List for All AGM Groups
            List<AGMGroup> allAGMGroups = new List<AGMGroup>();

            //Load the List from SQL DB
            allAGMGroups = adgm.Get_All_AGMGroups();

            //Null\Empty Check on All Groups List
            if(allAGMGroups != null && allAGMGroups.Count() > 0)
            {

                foreach (AGMGroup agmGroup in allAGMGroups)
                {

                    //Var for Group's LDAP Path Based Upon AD GUID
                    string grpLDAPPath = "LDAP://xxxxxxx.xxxxxxxx.edu/<GUID=" + agmGroup.AGMGID.ToString() + ">";

                    //Check to See Group Exists in AD
                    if(DirectoryEntry.Exists(grpLDAPPath))
                    {
                        //Pull Directory Entry of AD Group Sync
                        DirectoryEntry deADGroup = new DirectoryEntry(grpLDAPPath);

                        //Var for Group's DN Value
                        string uADGrpDN = deADGroup.Properties["distinguishedname"][0].ToString();

                        //Var for Group's CN Value
                        string uADGrpCN = deADGroup.Properties["cn"][0].ToString();

                        //Var for Group's Description
                        string uADGrpDesc = string.Empty;


                        if (deADGroup.Properties["description"].Count > 0)
                        {
                            uADGrpDesc = deADGroup.Properties["description"][0].ToString();

                            Console.WriteLine(uADGrpDesc);
                        }

                        //Close out Directory Entry for AD Group
                        deADGroup.Close();
                    }
                    
                        
                }

            }//End of allAGMGroups Null\Empty Check

        }

        //########################################
        // Process Manager Disaffiliate Requests
        //########################################
        public static void Process_Manager_Disaffiliate_Requests(DateTime dtRunStart)
        {
            //Pull Pending Disaffiliate Requests from DB
            IQueryable<AGMManagerDisaffiliateRqst> iqPendingDisaffs = adgm.Get_Pending_AGMManagerDisaffiliateRqsts(dtRunStart);
            List<AGMManagerDisaffiliateRqst> lPendingDisaffs = new List<AGMManagerDisaffiliateRqst>();
            lPendingDisaffs = iqPendingDisaffs.ToList();

            //Null\Empty Check on Pending Disaffilations 
            if(lPendingDisaffs != null && lPendingDisaffs.Count > 0)
            {

                foreach(AGMManagerDisaffiliateRqst pamdr in lPendingDisaffs)
                {
                    //Check to See If Manager Exists for Kerb ID
                    AGMManager pagmm = adgm.Get_AGMManager_By_KerbID(pamdr.KerbID);

                    //Var for Manager Update Status
                    string mgrUpdtStatus = string.Empty;

                    //Null Check on Manager and Dept OU
                    if(pagmm != null && string.IsNullOrEmpty(pagmm.KerbID) == false && pagmm.AGMGroups != null && pagmm.AGMGroups.Count > 0 && string.IsNullOrEmpty(pamdr.uDept) == false)
                    {
                        //List of Dept Groups to Remove
                        List<AGMGroup> lGrpsRmv = new List<AGMGroup>();

                        //Var for Dept OU to Check
                        string dptOU = "ou=" + pamdr.uDept.ToLower() + ",ou=xxxxxxx,dc=xxxxx,dc=xxxxxxx,dc=xxxxxxx,dc=edu";

                        //Check Groups for Dept OU Partial
                        foreach(AGMGroup adGrp in pagmm.AGMGroups)
                        {

                            if(string.IsNullOrEmpty(adGrp.ADGrpDN) == false && adGrp.ADGrpDN.ToLower().Contains(dptOU) == true)
                            {
                                lGrpsRmv.Add(adGrp);
                            }

                        }//End of Groups Check for Dept OU Partial

                        //Check for Groups to Remove
                        if(lGrpsRmv != null && lGrpsRmv.Count > 0)
                        {
                            foreach(AGMGroup grpMgrRmv in lGrpsRmv)
                            {
                                pagmm.AGMGroups.Remove(grpMgrRmv);
                            }

                            //Update Manager in DB
                            mgrUpdtStatus = adgm.Update_AGM_Manager(pagmm);
                        }

                     
                    }//End of Manager Null\Empty Checks


                    //Complete Request
                    adgm.Complete_AGMManagerDisaffiliateRqst(pamdr.AMDRID);

                }//End of lPendingDisaffs Foreach

            }//End of lPendingDisaffs Null\Empty Check

        }

        //########################################
        // SnapShot Managers
        //########################################
        public static void Snapshot_Managers()
        {
   
            //Pull List of All ADGM Managers
            List<AGMManager> lManagers = adgm.Get_All_AGMManagers();

            //Null\Empty Check on ADGM Managers
            if(lManagers != null && lManagers.Count > 0)
            {

                foreach(AGMManager agmManager in lManagers)
                {
                    //Initiate Snapshot
                    AGMManagerSnapShot nwMgrSS = new AGMManagerSnapShot();

                    //Var for Report Time
                    DateTime dtRptTime = DateTime.Now;

                    //Set Report Time
                    nwMgrSS.SSRptDate = dtRptTime;

                    //Set Kerb ID
                    nwMgrSS.KerbID = agmManager.KerbID;

                    //Set Display Name
                    if(!string.IsNullOrEmpty(agmManager.DisplayName))
                    {
                        nwMgrSS.DisplayName = agmManager.DisplayName;
                    }
                    else
                    {
                        nwMgrSS.DisplayName = string.Empty;
                    }

                    //Set Email Address
                    if(!string.IsNullOrEmpty(agmManager.EmailAddress))
                    {
                        nwMgrSS.EmailAddress = agmManager.EmailAddress;
                    }
                    else
                    {
                        nwMgrSS.EmailAddress = string.Empty;
                    }



                    //Check for Assignments
                    if(agmManager.AGMGroups != null && agmManager.AGMGroups.Count > 0)
                    {
                        foreach(AGMGroup adGrp in agmManager.AGMGroups)
                        {
                            //Initiate Management Assignment
                            AGMManagerSnapShot_MgmtAssignment nwAMSMA = new AGMManagerSnapShot_MgmtAssignment();
                            nwAMSMA.SSRptDate = dtRptTime;
                            nwAMSMA.ADGrpGUID = adGrp.AGMGID.ToString();
                            nwAMSMA.ADGrpName = adGrp.ADGrpName;

                            //Add Management Assignment to Snapshot
                            nwMgrSS.AGMManagerSnapShot_MgmtAssignment.Add(nwAMSMA);

                        }//End of agmManager.AGMGroups Foreach
                    }

                    //Check for Existing SnapShot to Compare Before Sending to Database
                    AGMManagerSnapShot exstMgrSS = adgm.Get_Indv_AGMManagerSnapShot_by_KerbID(agmManager.KerbID);

                    //Null Checks for Existing Manager Snapshot
                    if(exstMgrSS != null && string.IsNullOrEmpty(exstMgrSS.KerbID) == false)
                    {
                        //Var for Snapshot Not Changed
                        bool bChanged = true;

                        //Var for Old Group Assignment GUIDS
                        string oldGrpAssignmentsGuids = string.Empty;

                        //Var for Current Group Assignments GUIDs
                        string crntGrpAssignmentsGuids = string.Empty;

                        //Check for Old Group Assignments
                        if(exstMgrSS.AGMManagerSnapShot_MgmtAssignment != null && exstMgrSS.AGMManagerSnapShot_MgmtAssignment.Count > 0)
                        {
                            //Configure Existing SnapShot Assignment GUID List
                            foreach(AGMManagerSnapShot_MgmtAssignment exMgtAssmt in exstMgrSS.AGMManagerSnapShot_MgmtAssignment.OrderBy(r => r.ADGrpGUID))
                            {
                                oldGrpAssignmentsGuids += exMgtAssmt.ADGrpGUID;
                            }

                        }//End of Check Old Snapshot Assignments

                        //Check for Current Assignments
                        if(nwMgrSS.AGMManagerSnapShot_MgmtAssignment != null && nwMgrSS.AGMManagerSnapShot_MgmtAssignment.Count > 0)
                        {
                            //Configure Current Snapshot Assignment GUID List
                            foreach(AGMManagerSnapShot_MgmtAssignment crntMgtAssmt in nwMgrSS.AGMManagerSnapShot_MgmtAssignment.OrderBy(r => r.ADGrpGUID))
                            {
                                crntGrpAssignmentsGuids += crntMgtAssmt.ADGrpGUID;
                            }

                        }//End of Check for Current Assignments

                        //Compare Existing Snapshot to Current SnapShot Values
                        if(oldGrpAssignmentsGuids == crntGrpAssignmentsGuids)
                        {
                            bChanged = false;
                        }

                        //Check Changed Status and Either Insert New Snapshot or Update Existing
                        if(bChanged == true)
                        {
                            adgm.Add_AGMManagerSnapShot(nwMgrSS);
                        }
                        else
                        {
                            adgm.Update_AGMManagerSnapShot(exstMgrSS);
                        }

                    }
                    else
                    {
                        //Add Snap Shot to Database
                        adgm.Add_AGMManagerSnapShot(nwMgrSS);
                    }


                }//End of lManagers Foreach

            }//End of lManagers Null\Empty Checks

        }

        

        //########################################
        // Check Manager's UCD Account Status
        //########################################
        public static void Check_Managers_UCD_Account_Status()
        {
            //List of ADGM Managers
            List<AGMManager> lCrntManagers = new List<AGMManager>();

            //Var for Send Report
            bool bSendReport = false;

            //Var for Gone Count
            int nGoneCnt = 0;

            //Var for Email Notice Subject
            string msgSubject = "ADGM Gone Away Manager(s) Report for " + DateTime.Now.ToShortDateString();

            //Var for Body Format HTML
            StringBuilder sbMailMessageHTML = new StringBuilder();

            //Start Format of HTML Report Table
            sbMailMessageHTML.AppendLine("<html><body><br /><br />");
            sbMailMessageHTML.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\" width=\"600\" style=\"font-size:8pt;font-family:Arial,sans-serif\">");
            sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
            sbMailMessageHTML.AppendLine("<td colspan=\"4\" align=\"center\">");
            sbMailMessageHTML.AppendLine("<strong><font color=\"#002855\" style=\"font-size:10pt\">");
            sbMailMessageHTML.AppendLine("ADGM Gone Away Manager(s) Report for " + DateTime.Now.ToShortDateString());
            sbMailMessageHTML.AppendLine("</font></strong>");
            sbMailMessageHTML.AppendLine("</td>");
            sbMailMessageHTML.AppendLine("</tr>");
            sbMailMessageHTML.AppendLine("<tr bgcolor=\"#4D688C\">");
            sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#FFFFFF\">#</font></strong></td>");
            sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#FFFFFF\">User ID</font></strong></td>");
            sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#FFFFFF\">Display Name</font></strong></td>");
            sbMailMessageHTML.AppendLine("<td align=\"center\"><strong><font color=\"#FFFFFF\">AD Groups</font></strong></td>");
            sbMailMessageHTML.AppendLine("</tr>");

            //Load Manager List
            lCrntManagers = adgm.Get_All_AGMManagers();

            //Null\Empty Check on lCrntManagers 
            if(lCrntManagers != null && lCrntManagers.Count > 0)
            {

                foreach(AGMManager agMgr in lCrntManagers.OrderBy(r => r.DisplayName))
                {
                    //Null\Empty Check on KerbID
                    if(!string.IsNullOrEmpty(agMgr.KerbID))
                    {
                        //Pull Mothra Account
                        AGMSDB_Mothra_UserAccnt amUsrAccnt = adgm.Get_Manager_Mothra_Accnt_by_UserID(agMgr.KerbID);

                        //Null Checks on Mothra Account
                        if (amUsrAccnt != null && string.IsNullOrEmpty(amUsrAccnt.CLIENTSTATUS) == false && amUsrAccnt.CLIENTSTATUS == "G")
                        {
                            //Send Report
                            bSendReport = true;

                            //Increment Gone Count
                            nGoneCnt++;

                            if (nGoneCnt % 2 == 0)
                            {
                                sbMailMessageHTML.AppendLine("<tr bgcolor=\"#E0E0E0\">");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<tr>");
                            }

                            //Add Row Count to Report
                            sbMailMessageHTML.AppendLine("<td>" + nGoneCnt.ToString() + "</td>");

                            //Add User ID to Report
                            sbMailMessageHTML.AppendLine("<td>" + agMgr.KerbID + "</td>");

                            //Add Display Name to Report
                            if (!string.IsNullOrEmpty(agMgr.DisplayName))
                            {
                                sbMailMessageHTML.AppendLine("<td>" + agMgr.DisplayName + "</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td> </td>");
                            }

                            //Add AD Groups 
                            if(agMgr.AGMGroups != null && agMgr.AGMGroups.Count > 0)
                            {
                                sbMailMessageHTML.AppendLine("<td>");

                                foreach(AGMGroup agmGrp in agMgr.AGMGroups.OrderBy(r => r.ADGrpName))
                                {
                                    sbMailMessageHTML.AppendLine(agmGrp.ADGrpName + "<br />");
                                }

                                sbMailMessageHTML.AppendLine("</td>");
                            }
                            else
                            {
                                sbMailMessageHTML.AppendLine("<td>&nbsp;</td>");
                            }

                            //End HTML Row
                            sbMailMessageHTML.AppendLine("</tr>");

                        }//End of amUsrAccnt Null\Empty Checks  

                    }//End of Null\Empty Check on KerbID 

                }//End of lCrntManagers Foreach

            }//End of Null\Empty Check on lCrntManagers


            //Close Report Table and End Message Body
            sbMailMessageHTML.AppendLine("</table><br /><br />");
            sbMailMessageHTML.AppendLine("<br />");
            sbMailMessageHTML.AppendLine("<br />");
            sbMailMessageHTML.AppendLine("<br />");
            sbMailMessageHTML.AppendLine("</body></html>");


            //Check for Send Report
            if (bSendReport == true)
            {
                //Pull Service Creds to Office365 Mailbox
                UCDCampusServiceCred uCmpSrvCredO365 = adgm.Get_UCDCampusServiceCred_By_ServiceName("OFFICE365");

                //Configure SMTP Client
                SmtpClient ucdMail = new SmtpClient();
                ucdMail.Host = uCmpSrvCredO365.ServiceURL1;
                ucdMail.Credentials = new System.Net.NetworkCredential(uCmpSrvCredO365.ServiceAcnt, uCmpSrvCredO365.ServicePwd);
                ucdMail.Port = 587;
                ucdMail.EnableSsl = true;

                //Mail Address for Automation Account
                MailAddress maADGMAuto = new MailAddress(uCmpSrvCredO365.ServiceEmail);

                //Mail Address for Dev Team
                MailAddress maCOEITSSAppDev = new MailAddress("xxxxxxx@xxxxxxxxx.edu");
                

                MailMessage mailMSGGoneReport = new MailMessage();
                mailMSGGoneReport.From = maADGMAuto;
                mailMSGGoneReport.Subject = msgSubject;
                mailMSGGoneReport.To.Add(maCOEITSSAppDev);
                mailMSGGoneReport.IsBodyHtml = true;
                mailMSGGoneReport.Body = sbMailMessageHTML.ToString();
                ucdMail.Send(mailMSGGoneReport);

            }//End of bSendReport Check

        }

        //##############################
        // Provision Pending Requests
        //##############################
        public static void Provision_Pending_Requests()
        {
            //List for AGM Groups Pending Requests
            List<AGMGroup> lGroupsPendingRqsts = new List<AGMGroup>();

            //Pull List from Database
            lGroupsPendingRqsts = adgm.Get_AGMGroups_Pending_Membership_Requests();

            //Check for Pending Requests
            if(lGroupsPendingRqsts != null && lGroupsPendingRqsts.Count() > 0)
            {

                //Pull Service Creds to Office365 Mailbox
                UCDCampusServiceCred uCmpSrvCredO365 = adgm.Get_UCDCampusServiceCred_By_ServiceName("OFFICE365");

                //Configure SMTP Client
                SmtpClient ucdMail = new SmtpClient();
                ucdMail.Host = uCmpSrvCredO365.ServiceURL1;
                ucdMail.Credentials = new System.Net.NetworkCredential(uCmpSrvCredO365.ServiceAcnt, uCmpSrvCredO365.ServicePwd);
                ucdMail.Port = 587;
                ucdMail.EnableSsl = true;

                
                //Mail Address for Automation Account
                MailAddress maADGMAuto = new MailAddress(uCmpSrvCredO365.ServiceEmail);

                //Mail Address for COEITSS-Support 
                MailAddress maCOEITSSSupport = new MailAddress("xxxxxxx@xxxxxxxx.edu");

                //Mail Address for Dev Team
                MailAddress maCOEITSSAppDev = new MailAddress("xxxxxxx@xxxxxxxxxxx.edu");

                //AD Principal Context for XXXXXX and XXXXX Domains
                PrincipalContext prctxAD3 = new PrincipalContext(ContextType.Domain, "XXX", "DC=XXX,DC=XXXXXXXX,DC=EDU");
                PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "XXXXX", "DC=XXXXX,DC=XXX,DC=XXXXXXXX,DC=EDU");

                //Directory Entry for XXXX Domain
                DirectoryEntry deAD3Domain = new DirectoryEntry("LDAP://DC=XXXX,DC=XXXXXXX,DC=EDU");
                //Directory Searcher for XXXXX Domain
                DirectorySearcher dsAD3Search = new DirectorySearcher(deAD3Domain);
                dsAD3Search.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                dsAD3Search.PropertiesToLoad.Add("sAMAccountName");
                dsAD3Search.PropertiesToLoad.Add("distinguishedName");

                //Loop Through Each Group With Pending Requests
                foreach(AGMGroup agmGroup in lGroupsPendingRqsts)
                {

                    //Var for Group's LDAP Path Based Upon AD GUID
                    string grpLDAPPath = "LDAP://xxxxxx.xxxxxx.edu/<GUID=" + agmGroup.AGMGID.ToString() + ">";

                    //Pull Directory Entry of AGM Group
                    DirectoryEntry deAGMGroup = new DirectoryEntry(grpLDAPPath);

                    //AD Group Principal
                    GroupPrincipal grpPrincipal;

                    //Null Check on DirectoryEntry for Group
                    if(deAGMGroup != null && COECheckDirectoryEntry(deAGMGroup) == true)
                    {

                        //Var for Group's DN Value
                        string uADGrpDN = deAGMGroup.Properties["distinguishedname"][0].ToString();

                        //Var for Group's CN Value
                        string uADGrpCN = deAGMGroup.Properties["cn"][0].ToString();

                        //String Builder for Provision Completion Notice Body
                        StringBuilder sbProvCompNotice = new StringBuilder();

                        sbProvCompNotice.AppendLine("Provisioning Completion Notice for " + uADGrpCN);
                        sbProvCompNotice.AppendLine(" ");
                        sbProvCompNotice.AppendLine("Notice Timestamp: " + DateTime.Now.ToString());
                        sbProvCompNotice.AppendLine(" ");
                        sbProvCompNotice.AppendLine("Provisioning Request(s):");
                        sbProvCompNotice.AppendLine("----------------------------------");
                        sbProvCompNotice.AppendLine(" ");

                        //Pull Group Principal Information for OU Group
                        grpPrincipal = GroupPrincipal.FindByIdentity(prctxOU, IdentityType.DistinguishedName, uADGrpDN);

                        //Null Check on Group Principal
                        if (grpPrincipal != null)
                        {

                            //Create List for Pending Requests for Group
                            List<AGMMemberRequest> lPendingRequests = new List<AGMMemberRequest>();

                            //Pull Pending List for Group
                            lPendingRequests = agmGroup.AGMMemberRequests.Where(r => r.Pending == true).OrderBy(c => c.SubmittedOn).ToList();

                            //Loop Through Pending Requests
                            foreach(AGMMemberRequest pdgRqst in lPendingRequests)
                            {
                                
                                //Var for Request Completion Status
                                string cmpStatus = string.Empty;

                                try
                                {
                                    //Configure AD3 Search Filter
                                    dsAD3Search.Filter = "(&(objectClass=user)(sAMAccountName=" + pdgRqst.KerbID + "))";
                                    //Search AD3 for User ID
                                    SearchResult srAD3Result = dsAD3Search.FindOne();

                                    //Check Search Result
                                    if (srAD3Result != null)
                                    {
                                        //Var for Search Result User DN
                                        string srUsrDN = srAD3Result.Properties["distinguishedName"][0].ToString().ToLower();

                                        if (String.IsNullOrEmpty(srUsrDN) == false && String.IsNullOrEmpty(pdgRqst.MRAction) == false)
                                        {

                                            if (pdgRqst.MRAction == "Add")
                                            {
                                                //Add User to Group
                                                grpPrincipal.Members.Add(prctxAD3, IdentityType.DistinguishedName, srUsrDN);

                                                //Set Add Action
                                                sbProvCompNotice.AppendLine("Action: Add");
                                            }
                                            else if (pdgRqst.MRAction == "Remove")
                                            {
                                                //Remove User from Group
                                                grpPrincipal.Members.Remove(prctxAD3, IdentityType.DistinguishedName, srUsrDN);

                                                //Set Remove Action
                                                sbProvCompNotice.AppendLine("Action: Remove");
                                            }

                                            sbProvCompNotice.AppendLine("User ID: ad3\\" + pdgRqst.KerbID.ToLower());
                                            sbProvCompNotice.AppendLine("Requested By: " + pdgRqst.SubmittedBy.ToLower());
                                            sbProvCompNotice.AppendLine(" ");

                                        }//End of srUsrDN and MRAction Null\Empty Checks

                                    }
                                    else
                                    {
                                        cmpStatus = "Couldn't find " + pdgRqst.KerbID + " in AD3";
                                        sbProvCompNotice.AppendLine(cmpStatus);
                                    }//End of srAD3Result Null Check

                                    //Check Completion Status Before Updating DB
                                    if (String.IsNullOrEmpty(cmpStatus))
                                    {
                                        //Update Request Status in DB
                                        cmpStatus = adgm.Complete_AGMMemberRequest(pdgRqst.AGMMRID);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    cmpStatus = ex.ToString();

                                    if(cmpStatus.ToLower().Contains("the principal already exists in the store") == true)
                                    {
                                        //Update Request Status in DB
                                        cmpStatus = adgm.Complete_AGMMemberRequest(pdgRqst.AGMMRID);
                                    }
                                }

                                //Log Failed Membership Attempts
                                if(!String.IsNullOrEmpty(cmpStatus))
                                {
                                    COELogThatEvent(cmpStatus);
                                }

                            }//End of lPendingRequests Foreach

                            //Try Catch for Save On Group Incase We Don't have Access
                            try
                            {
                                //Save Group Membership
                                grpPrincipal.Save();

                                //Send Provisioning Completion Notice If Requested
                                if(agmGroup.SendGrpReport == true && agmGroup.SendProvisionRpts == true)
                                {

                                    sbProvCompNotice.AppendLine(" ");
                                    sbProvCompNotice.AppendLine(" ");
                                    sbProvCompNotice.AppendLine(" ");

                                    //Configure Mail Message for Completion Notice
                                    MailMessage mailMSGProvNotice = new MailMessage();
                                    mailMSGProvNotice.From = maADGMAuto;
                                    mailMSGProvNotice.IsBodyHtml = false;
                                    mailMSGProvNotice.Body = sbProvCompNotice.ToString();
                                    mailMSGProvNotice.Subject = "ADGM provisioning completion notice for " + uADGrpCN;

                                    //Check for Additional Address to Send To
                                    if (!String.IsNullOrEmpty(agmGroup.ReportAdtlAddr))
                                    {
                                        MailAddress maTo = new MailAddress(agmGroup.ReportAdtlAddr);
                                        mailMSGProvNotice.To.Add(maTo);
                                    }

                                    //Check If Managers Should Get Report
                                    if(agmGroup.SendMgrReport == true && agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0)
                                    {
                                        //Loop Through Managers and Add to To Field
                                        foreach(AGMManager agmManager in agmGroup.AGMManagers)
                                        {

                                            if(!String.IsNullOrEmpty(agmManager.EmailAddress))
                                            {
                                                MailAddress maTo = new MailAddress(agmManager.EmailAddress);
                                                mailMSGProvNotice.To.Add(maTo);
                                            }

                                        }//End of AGM Managers Foreach

                                    }//End of Send to Managers Check

                                    //Send Email Notice (Fire in the Hole)
                                    ucdMail.Send(mailMSGProvNotice);
                                  
                                }//End of Provisioning Completion Notice Checks

                            }
                            catch(Exception ex3)
                            {
                                COELogThatEvent(ex3.ToString());
                            }

                        }//End of Null Check on 

                    }
                    else
                    {
                        COELogThatEvent(agmGroup.ADGrpName + " couldn't be found in uConnect AD.");
                    }//End of deAGMGroup Null Check

                    //Close out Directory Entry for Group
                    deAGMGroup.Close();

                }//End of lGroupsPendingRqsts Foreach

            }//End of Pending Requests Check

        }

        //#######################################
        // Send All Groups Reports for Managers
        //#######################################
        public static void Send_All_Groups_Reports_For_Managers()
        {
            
            PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "XXXXX", "DC=XXXXX,DC=XXXX,DC=XXXXXX,DC=EDU");

            //Pull Service Creds to Office365 Mailbox
            UCDCampusServiceCred uCmpSrvCredO365 = adgm.Get_UCDCampusServiceCred_By_ServiceName("OFFICE365");

            //Configure SMTP Client
            SmtpClient ucdMail = new SmtpClient();
            ucdMail.Host = uCmpSrvCredO365.ServiceURL1;
            ucdMail.Credentials = new System.Net.NetworkCredential(uCmpSrvCredO365.ServiceAcnt, uCmpSrvCredO365.ServicePwd);
            ucdMail.Port = 587;
            ucdMail.EnableSsl = true;

           
            //Mail Address for Automation Account
            MailAddress maADGMAuto = new MailAddress(uCmpSrvCredO365.ServiceEmail);

            //List for Managers that Want All Group Reports
            List<AGMManager> lAllGrpRptManagers = adgm.Get_Managers_Wanting_AllGroups_Report();

            //Null\Empty Check on Managers List
            if(lAllGrpRptManagers != null && lAllGrpRptManagers.Count() > 0)
            {

                //Loop Through Managers List
                foreach(AGMManager agmManager in lAllGrpRptManagers)
                {

                    //Check Manager Account for Email Address and Groups to Report On and Report Last Sent On
                    if (String.IsNullOrEmpty(agmManager.EmailAddress) == false && agmManager.AGMGroups != null && agmManager.AGMGroups.Count() > 0 && 
                       (agmManager.RptLastSent == null || agmManager.RptLastSent.Value.AddDays((int)agmManager.DaysBtwnReport) < DateTime.Now))
                    {
                        //Configure Mail Message Settings
                        MailAddress maMgr = new MailAddress(agmManager.EmailAddress);
                        MailMessage mailMSGAllGrpReport = new MailMessage();
                        mailMSGAllGrpReport.From = maADGMAuto;
                        mailMSGAllGrpReport.Subject = "ADGM All Groups Report for " + DateTime.Now.ToShortDateString();
                        mailMSGAllGrpReport.To.Add(maMgr);
                        mailMSGAllGrpReport.IsBodyHtml = true;

                        //Var for Body Format HTML
                        StringBuilder sbMailMessageHTML = new StringBuilder();

                        //Start Format of HTML Report Table
                        sbMailMessageHTML.AppendLine("<html><body>");
                        sbMailMessageHTML.AppendLine("<p><font color=\"#002855\" style=\"font-size:14pt\">ADGM All Groups Report for " + DateTime.Now.ToShortDateString() + "</font></p>");
                        sbMailMessageHTML.AppendLine("<p><font color=\"#002855\" style=\"font-size:10pt\">Total Number of Managed Groups: " + agmManager.AGMGroups.Count.ToString() + "</font></p>");
                        sbMailMessageHTML.AppendLine("<p><font color=\"#002855\" style=\"font-size:10pt\">To Manage Your Groups, Visit <a href=\"https://adgm.xxxxxx.xxxxxxxxx.edu\" target=\"_blank\">https://adgm.xxxxxxx.xxxxxxxxx.edu</a></font></p>");

                        //Loop Through AGM Groups
                        foreach (AGMGroup agmGroup in agmManager.AGMGroups.OrderBy(r => r.ADGrpName))
                        {

                            //Var for Group's LDAP Path Based Upon AD GUID
                            string grpLDAPPath = "LDAP://xxxxxxx.xxxxxxxxx.edu/<GUID=" + agmGroup.AGMGID.ToString() + ">";

                            //Check to See Group Exists in AD
                            if (DirectoryEntry.Exists(grpLDAPPath))
                            {
                                //Pull Directory Entry of AD Group Sync
                                DirectoryEntry deADGroup = new DirectoryEntry(grpLDAPPath);

                                //Var for Group's DN Value
                                string uADGrpDN = deADGroup.Properties["distinguishedname"][0].ToString();

                                //Var for Group's CN Value
                                string uADGrpCN = deADGroup.Properties["cn"][0].ToString();

                                //List for Group Members
                                List<ADGMuCntGrpMemberRpt> lGMUGMR = new List<ADGMuCntGrpMemberRpt>();

                                //AD Group Principal
                                GroupPrincipal grpPrincipal = GroupPrincipal.FindByIdentity(prctxOU, IdentityType.DistinguishedName, uADGrpDN);

                                //Check Current Group Membership
                                if (grpPrincipal.Members.Count > 0)
                                {
                                    //Loop Through Group Membership (Not Nested) and Load HashTable with DNs
                                    foreach (var crntMbr in grpPrincipal.GetMembers(false))
                                    {
                                        //Pull Directory Entry for Group Member
                                        DirectoryEntry deMember = crntMbr.GetUnderlyingObject() as DirectoryEntry;

                                        //Load Up HashTable with Current Users DN
                                        if (deMember != null && COECheckDirectoryEntry(deMember) == true && deMember.Properties["distinguishedName"].Count > 0)
                                        {
                                            //Initiate Group Member Reporting Object
                                            ADGMuCntGrpMemberRpt uCntGrpMbr = new ADGMuCntGrpMemberRpt();

                                            //Load Only AD3 Users
                                            if (deMember.Properties["objectClass"].Contains("user") == true && deMember.Properties["objectClass"].Contains("computer") == false && deMember.Properties["distinguishedName"][0].ToString().ToLower().Contains(",dc=xxxx,dc=xxxx,dc=xxxxxx,dc=edu") == false)
                                            {

                                                //Load User ID
                                                uCntGrpMbr.LoginID = deMember.Properties["samAccountName"][0].ToString().ToLower();

                                                //Load Display Name
                                                int nGMDisplayNameCount = deMember.Properties["displayName"].Count;
                                                if (nGMDisplayNameCount > 0)
                                                {
                                                    uCntGrpMbr.DisplayName = deMember.Properties["displayName"][0].ToString();
                                                }

                                                //Mail Address Check
                                                int nGMMailCount = deMember.Properties["mail"].Count;
                                                if (nGMMailCount > 0)
                                                {
                                                    uCntGrpMbr.EmailAddress = deMember.Properties["mail"][0].ToString().ToLower();
                                                }

                                                //Proxy Address Check (for Primary SMTP Address)
                                                int nGMProxyAddrCount = deMember.Properties["proxyAddresses"].Count;
                                                if (nGMProxyAddrCount > 0)
                                                {
                                                    for (int x = 0; x <= nGMProxyAddrCount - 1; x++)
                                                    {

                                                        if (deMember.Properties["proxyAddresses"][x].ToString().StartsWith("SMTP:"))
                                                        {
                                                            uCntGrpMbr.EmailAddress = deMember.Properties["proxyAddresses"][x].ToString().ToLower().Replace("smtp:", "");
                                                        }

                                                    }

                                                }//End of Proxy Address Check

                                                //Add Group Member to Report List
                                                lGMUGMR.Add(uCntGrpMbr);

                                            }//End of AD3 User Checks

                                        }//End of deMember and DN Null Checks

                                        //Close DirectoryEntry on Group Member
                                        deMember.Close();

                                    }//End of Group Membership Foreach

                                }//End of Current Group Membership Check

                               
                                //sbMailMessageHTML.AppendLine("<br />");
                                sbMailMessageHTML.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\" width=\"500\" style=\"font-size:8pt;font-family:Arial,sans-serif\">");
                                sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
                                sbMailMessageHTML.AppendLine("<td colspan=\"3\" align=\"center\">");
                                sbMailMessageHTML.AppendLine("<strong><font color=\"#002855\" style=\"font-size:10pt\">");
                                sbMailMessageHTML.AppendLine(uADGrpCN);
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


                                if (agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0)
                                {
                                    //Var for Manager Count
                                    int mgrCnt = 0;

                                    foreach (AGMManager agmMngr in agmGroup.AGMManagers)
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
                                        if (!String.IsNullOrEmpty(agmMngr.DisplayName))
                                        {

                                            sbMailMessageHTML.AppendLine("<td>" + agmMngr.DisplayName + "</td>");
                                        }
                                        else if (!String.IsNullOrEmpty(agmMngr.KerbID))
                                        {
                                            sbMailMessageHTML.AppendLine("<td>" + agmMngr.KerbID + "</td>");
                                        }
                                        else
                                        {
                                            sbMailMessageHTML.AppendLine("<td> </td>");
                                        }


                                        //Check for Empty Email Address
                                        if (!String.IsNullOrEmpty(agmMngr.EmailAddress))
                                        {
                                            //Add Email Address
                                            sbMailMessageHTML.AppendLine("<td>" + agmMngr.EmailAddress + "</td>");

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

                                //Check for Group Members to Report
                                if (lGMUGMR != null && lGMUGMR.Count() > 0)
                                {

                                    int mbrCnt = 0;

                                    foreach (ADGMuCntGrpMemberRpt mbrRpt in lGMUGMR.OrderBy(r => r.DisplayName))
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
                                }//End of Membership Check

                                sbMailMessageHTML.AppendLine("</table><br /><br />");

                                //Close out Directory Entry
                                deADGroup.Close();

                            }//End of AD Group Exists Check

                         
                        }//End of AGMGroups Foreach

                        //End HTML Report
                        sbMailMessageHTML.AppendLine("<br />");
                        sbMailMessageHTML.AppendLine("<br />");
                        sbMailMessageHTML.AppendLine("<br />");
                        sbMailMessageHTML.AppendLine("</body></html>");

                        //Set Body of Mail Message
                        mailMSGAllGrpReport.Body = sbMailMessageHTML.ToString();

                        try
                        {
                            if(mailMSGAllGrpReport.To.Count() > 0)
                            {
                                ucdMail.Send(mailMSGAllGrpReport);
                            }
                            
                        }
                        catch(Exception ex3)
                        {
                            COELogThatEvent(ex3.ToString());
                        }

                        //Var for Manager Update Status
                        string mgrUpdtStatus = string.Empty;

                        //Update Report Time for Manager
                        agmManager.RptLastSent = DateTime.Now;

                        //Updat Manager in SQL DB
                        mgrUpdtStatus = adgm.Update_AGM_Manager(agmManager);

                        if(!String.IsNullOrEmpty(mgrUpdtStatus))
                        {
                            COELogThatEvent(mgrUpdtStatus);
                        }

                    }//End of Email Address and AGM Groups Collection and Last Sent Report Date Checks Null\Empty Check

                }//End of lAllGrpRptManagers Foreach

            }//End of Null\Empty Check on Managers List


        }

        //###############################
        // Update AGM Groups AD Info
        //###############################
        public static void Update_AGMGroups_AD_Info()
        {

            PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "XXX", "DC=XXXX,DC=XXX,DC=XXXXXXX,DC=EDU");

            //Pull Service Creds to Office365 Mailbox
            UCDCampusServiceCred uCmpSrvCredO365 = adgm.Get_UCDCampusServiceCred_By_ServiceName("OFFICE365");

            //Configure SMTP Client
            SmtpClient ucdMail = new SmtpClient();
            ucdMail.Host = uCmpSrvCredO365.ServiceURL1;
            ucdMail.Credentials = new System.Net.NetworkCredential(uCmpSrvCredO365.ServiceAcnt, uCmpSrvCredO365.ServicePwd);
            ucdMail.Port = 587;
            ucdMail.EnableSsl = true;

           
            //Mail Address for Automation Account
            MailAddress maADGMAuto = new MailAddress(uCmpSrvCredO365.ServiceEmail);

            //Mail Address for COEITSS-Support 
            MailAddress maCOEITSSSupport = new MailAddress("xxxxxxxxxx@xxxxxxxx.edu");

            //Mail Address for Dev Team
            MailAddress maCOEITSSAppDev = new MailAddress("xxxxxxxxx@xxxxxxxxxx.edu");
            

            //Create List for All AGM Groups
            List<AGMGroup> allAGMGroups = new List<AGMGroup>();
            
            //Load the List from SQL DB
            allAGMGroups = adgm.Get_All_AGMGroups();

            //Null\Empty Check on All Groups List
            if(allAGMGroups != null && allAGMGroups.Count() > 0)
            {

                foreach(AGMGroup agmGroup in allAGMGroups)
                {
                    //Var for Group's LDAP Path Based Upon AD GUID
                    string grpLDAPPath = "LDAP://xxxxx.xxxxxxxxx.edu/<GUID=" + agmGroup.AGMGID.ToString() + ">";

                    //Check to See Group Exists in AD
                    if (DirectoryEntry.Exists(grpLDAPPath))
                    {

                        //Pull Directory Entry of AD Group Sync
                        DirectoryEntry deADGroup = new DirectoryEntry(grpLDAPPath);

                        //Var for Group's DN Value
                        string uADGrpDN = deADGroup.Properties["distinguishedname"][0].ToString();

                        //Var for Group's CN Value
                        string uADGrpCN = deADGroup.Properties["cn"][0].ToString();

                        //Var for Group's Description
                        string uADGrpDesc = string.Empty;

                        //Var for Manager Notice Sent
                        bool bMngrNtcOut = false;

                        //List for Group Members
                        List<ADGMuCntGrpMemberRpt> lGMUGMR = new List<ADGMuCntGrpMemberRpt>();

                        //Configure Group's Description Value
                        if((bool)agmGroup.GrpDescriptionAD == true && deADGroup.Properties["description"].Count > 0)
                        {
                            uADGrpDesc = deADGroup.Properties["description"][0].ToString();
                        }
                        else if((bool)agmGroup.GrpDescriptionAD == false && string.IsNullOrEmpty(agmGroup.GrpDescription) == false)
                        {
                            uADGrpDesc = agmGroup.GrpDescription;
                        }//End of Group's Description Configuration

                        //Check to See If Group Membership Reports Should be Sent
                        if (agmGroup.SendGrpReport == true && (agmGroup.MngrNtcLastSent == null || agmGroup.MngrNtcLastSent.Value.AddDays((int)agmGroup.DaysBtwnReport) < DateTime.Now))
                        {

                            //Check for Managers or Additional Address to Send Report To
                            if ((agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0) || String.IsNullOrEmpty(agmGroup.ReportAdtlAddr) == false)
                            {
                                //AD Group Principal
                                GroupPrincipal grpPrincipal = GroupPrincipal.FindByIdentity(prctxOU, IdentityType.DistinguishedName, uADGrpDN);

                                //Check Current Group Membership
                                if (grpPrincipal.Members.Count > 0)
                                {
                                    //Loop Through Group Membership (Not Nested) and Load HashTable with DNs
                                    foreach (var crntMbr in grpPrincipal.GetMembers(false))
                                    {
                                        //Pull Directory Entry for Group Member
                                        DirectoryEntry deMember = crntMbr.GetUnderlyingObject() as DirectoryEntry;

                                        //Load Up HashTable with Current Users DN
                                        if (deMember != null && deMember.Properties["distinguishedName"].Count > 0)
                                        {
                                            //Initiate Group Member Reporting Object
                                            ADGMuCntGrpMemberRpt uCntGrpMbr = new ADGMuCntGrpMemberRpt();

                                            //Load Only AD3 Users
                                            if (deMember.Properties["objectClass"].Contains("user") == true && deMember.Properties["objectClass"].Contains("computer") == false && deMember.Properties["distinguishedName"][0].ToString().ToLower().Contains(",dc=xxxx,dc=xxxx,dc=xxxxxx,dc=edu") == false)
                                            {

                                                //Load User ID
                                                uCntGrpMbr.LoginID = deMember.Properties["samAccountName"][0].ToString().ToLower();

                                                //Load Display Name
                                                int nGMDisplayNameCount = deMember.Properties["displayName"].Count;
                                                if (nGMDisplayNameCount > 0)
                                                {
                                                    uCntGrpMbr.DisplayName = deMember.Properties["displayName"][0].ToString();
                                                }

                                                //Mail Address Check
                                                int nGMMailCount = deMember.Properties["mail"].Count;
                                                if (nGMMailCount > 0)
                                                {
                                                    uCntGrpMbr.EmailAddress = deMember.Properties["mail"][0].ToString().ToLower();
                                                }

                                                //Proxy Address Check (for Primary SMTP Address)
                                                int nGMProxyAddrCount = deMember.Properties["proxyAddresses"].Count;
                                                if (nGMProxyAddrCount > 0)
                                                {
                                                    for (int x = 0; x <= nGMProxyAddrCount - 1; x++)
                                                    {

                                                        if (deMember.Properties["proxyAddresses"][x].ToString().StartsWith("SMTP:"))
                                                        {
                                                            uCntGrpMbr.EmailAddress = deMember.Properties["proxyAddresses"][x].ToString().ToLower().Replace("smtp:", "");
                                                        }

                                                    }

                                                }//End of Proxy Address Check

                                                //Add Group Member to Report List
                                                lGMUGMR.Add(uCntGrpMbr);

                                            }//End of AD3 User Checks

                                        }//End of deMember and DN Null Checks

                                        //Close out Directory Entry for Group Member
                                        deMember.Close();

                                    }//End of Group Membership Foreach

                                }//End of Current Group Membership Check

                                //###############################
                                // Email Report
                                //###############################
                                MailMessage mailMSGGrpReport = new MailMessage();
                                mailMSGGrpReport.From = maADGMAuto;
                                mailMSGGrpReport.Subject = "ADGM Report for " + uADGrpCN + " on " + DateTime.Now.ToShortDateString();

                                //Check for Additional Address to Send To
                                if (!String.IsNullOrEmpty(agmGroup.ReportAdtlAddr))
                                {
                                    MailAddress maTo = new MailAddress(agmGroup.ReportAdtlAddr);
                                    mailMSGGrpReport.To.Add(maTo);
                                }

                                //Var for Body Format Plain Text
                                StringBuilder sbMailMessage = new StringBuilder();
                                
                                //Var for Body Format HTML
                                StringBuilder sbMailMessageHTML = new StringBuilder();

                                //Start Format of HTML Report Table
                                sbMailMessageHTML.AppendLine("<html><body>");
                                sbMailMessageHTML.AppendLine("<br />");
                                sbMailMessageHTML.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\" width=\"500\" style=\"font-size:8pt;font-family:Arial,sans-serif\">");
                                sbMailMessageHTML.AppendLine("<tr bgcolor=\"#ECD47F\">");
                                sbMailMessageHTML.AppendLine("<td colspan=\"3\" align=\"center\">");
                                sbMailMessageHTML.AppendLine("<strong><font color=\"#002855\" style=\"font-size:10pt\">");
                                sbMailMessageHTML.AppendLine(uADGrpCN + " on " + DateTime.Now.ToShortDateString());
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

                                sbMailMessage.AppendLine("ADGM Report for " + uADGrpCN);
                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine("Group Managers:");
                                sbMailMessage.AppendLine("------------------------");
                                
                                if(agmGroup.AGMManagers != null && agmGroup.AGMManagers.Count() > 0)
                                {
                                    //Var for Manager Count
                                    int mgrCnt = 0;

                                    foreach(AGMManager agmManager in agmGroup.AGMManagers)
                                    {
                                        
                                        //Increment Manager Count
                                        mgrCnt++;

                                        //Check for Row Count for Row Coloration 
                                        if(mgrCnt % 2 == 0)
                                        {
                                            sbMailMessageHTML.AppendLine("<tr bgcolor=\"#E0E0E0\">");
                                        }
                                        else
                                        {
                                            sbMailMessageHTML.AppendLine("<tr>");
                                        }

                                        //Add Row Count to Report
                                        sbMailMessageHTML.AppendLine("<td>" + mgrCnt.ToString() + "</td>");
                                        
                                        //Var for Manager Identity
                                        string mgrIdentity = string.Empty;

                                        //Check for Empty Display Name
                                        if(!String.IsNullOrEmpty(agmManager.DisplayName))
                                        {
                                            mgrIdentity += agmManager.DisplayName;
                                            sbMailMessageHTML.AppendLine("<td>" + agmManager.DisplayName + "</td>");
                                        }
                                        else if(!String.IsNullOrEmpty(agmManager.KerbID))
                                        {
                                            mgrIdentity += agmManager.KerbID;
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
                                            mgrIdentity += "(" + agmManager.EmailAddress + ")";
                                            sbMailMessageHTML.AppendLine("<td>" + agmManager.EmailAddress + "</td>");

                                            //Check If Report Needs to Go to Them
                                            if (agmGroup.SendMgrReport == true && agmManager.SendAllGrpsRpt == false)
                                            {
                                                MailAddress maTo = new MailAddress(agmManager.EmailAddress);
                                                mailMSGGrpReport.To.Add(maTo);
                                            }

                                        }
                                        else
                                        {
                                            sbMailMessageHTML.AppendLine("<td> </td>");
                                        }//End of Manager Email Address Check

                                        //Add Manager Identity to Report
                                        sbMailMessage.AppendLine(mgrIdentity);

                                        //End HTML Row
                                        sbMailMessageHTML.AppendLine("</tr>");

                                    }//End of Managers Foreach

                                }
                                else
                                {
                                    sbMailMessage.AppendLine("No Managers Configured");
                                    sbMailMessageHTML.AppendLine("<tr><td colspan=\"3\" align=\"center\">No Managers Configured</td></tr>");
                                }

                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine("Group Members:");
                                sbMailMessage.AppendLine("------------------------");

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
                                
                                //Check for Group Members to Report
                                if(lGMUGMR != null && lGMUGMR.Count() > 0)
                                {

                                    int mbrCnt = 0;

                                    foreach(ADGMuCntGrpMemberRpt mbrRpt in lGMUGMR.OrderBy(r => r.DisplayName))
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

                                        //Var for Member Identity
                                        string mbrIdentity = string.Empty;

                                        //Check Group Member Display Name
                                        if(!String.IsNullOrEmpty(mbrRpt.DisplayName))
                                        {
                                            mbrIdentity += mbrRpt.DisplayName;
                                            sbMailMessageHTML.AppendLine("<td>" + mbrRpt.DisplayName + "</td>");
                                        }
                                        else if(!String.IsNullOrEmpty(mbrRpt.LoginID))
                                        {
                                            mbrIdentity += mbrRpt.LoginID.ToLower();
                                            sbMailMessageHTML.AppendLine("<td>" + mbrRpt.LoginID.ToLower() + "</td>");
                                        }
                                        else
                                        {
                                            sbMailMessageHTML.AppendLine("<td> </td>");
                                        }

                                        //Check Group Member Email Address
                                        if(!String.IsNullOrEmpty(mbrRpt.EmailAddress))
                                        {
                                            mbrIdentity += "(" + mbrRpt.EmailAddress.ToLower() + ")";
                                            sbMailMessageHTML.AppendLine("<td>" + mbrRpt.EmailAddress.ToLower() + "</td>");
                                        }
                                        else
                                        {
                                            sbMailMessageHTML.AppendLine("<td> </td>");
                                        }

                                        //Add Member Identity to Report
                                        sbMailMessage.AppendLine(mbrIdentity);

                                        //End HTML Row
                                        sbMailMessageHTML.AppendLine("</tr>");

                                    }//End of Member Report Foreach

                                }
                                else
                                {
                                    sbMailMessage.AppendLine("No Direct Members to Report On");
                                    sbMailMessageHTML.AppendLine("<tr><td colspan=\"3\" align=\"center\">No Direct Members to Report On</td></tr>");
                                }//End of Membership Check


                                //Close Out String Builders
                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine("To Manage this Group, Visit https://adgm.xxxxxx.xxxxxxx.edu");
                                sbMailMessage.AppendLine(" ");
                                sbMailMessage.AppendLine(" ");
                                sbMailMessageHTML.AppendLine("</table><br /><br />");
                                sbMailMessageHTML.AppendLine("<p><font color=\"#002855\" style=\"font-size:8pt\">To Manage this Group, Visit <a href=\"https://adgm.xxxxxx.xxxxxx.edu\" target=\"_blank\">https://adgm.xxxx.xxxxxx.edu</a></font></p>");
                                sbMailMessageHTML.AppendLine("<br /><br /></body>");
                                sbMailMessageHTML.AppendLine("</html>");

                                //Check Which Version of Report the Admin Wants
                                if(agmGroup.SendHTMLRpt == true)
                                {
                                    mailMSGGrpReport.IsBodyHtml = true;
                                    mailMSGGrpReport.Body = sbMailMessageHTML.ToString();
                                }
                                else
                                {
                                    mailMSGGrpReport.IsBodyHtml = false;
                                    mailMSGGrpReport.Body = sbMailMessage.ToString();
                                }//End of Report Version Check


                                try
                                {

                                    if(mailMSGGrpReport.To.Count() > 0)
                                    {
                                        //Send Report (Fire in the Hole)
                                        ucdMail.Send(mailMSGGrpReport);
                                    }

                                    //Mark Notice as Sent If It Makes It Here
                                    bMngrNtcOut = true;
                                }
                                catch (Exception ex2)
                                {
                                    COELogThatEvent(ex2.ToString());
                                }

                            }
                            else
                            {
                                bMngrNtcOut = true;
                            }//End of AGM Managers or Additional Address Null\Empty Check

                        }//End of Send Report Check
                        
                        //Update Information in DB
                        adgm.Update_AGMGroup_AD_Info_MgrNotice(agmGroup.AGMGID, uADGrpDN, uADGrpCN, uADGrpDesc, bMngrNtcOut);

                        //Close out Directory Entry for AD Group
                        deADGroup.Close();

                    }//End of Exists in AD Check

                }//End of allAGMGroups Foreach

            }//End of allAGMGroups Null\Empty Check

        }


        //Log The Event in the Application Logs
        static void COELogThatEvent(string evtError)
        {

            EventLog eventLog = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists("COEADGroupManager"))
            {
                System.Diagnostics.EventLog.CreateEventSource("COEADGroupManager", "Application");
            }

            eventLog.Source = "COEADGroupManager";

            eventLog.WriteEntry(evtError, EventLogEntryType.Warning, 1);

            eventLog.Close();
        }

        //Check AD Directory Entry Status
        static bool COECheckDirectoryEntry(DirectoryEntry deEntry)
        {

            bool rtnDEStatus = false;

            try
            {
                if (deEntry.Guid != null)
                {
                    rtnDEStatus = true;
                }
            }
            catch
            {
                rtnDEStatus = false;
            }

            return rtnDEStatus;
        }

    }
}
