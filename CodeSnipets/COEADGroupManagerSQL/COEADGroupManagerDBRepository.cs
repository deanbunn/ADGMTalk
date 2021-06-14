using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COEADGroupManagerSQL
{
    public class COEADGroupManagerDBRepository
    {
        //Initiate Entities
        private COEADGroupManagerEntities adgme = new COEADGroupManagerEntities();


        //#####################################################################
        // ADGM Manager Disaffiliate Request Functions
        //#####################################################################

        //################################################
        // Pull All ADGM Manager Disaffiliations 
        //################################################
        public IQueryable<AGMManagerDisaffiliateRqst> Get_All_AGMManagerDisaffiliates()
        {
            return adgme.AGMManagerDisaffiliateRqsts;
        }

        //################################################
        // Get Pending ADGM Manager Disaffiliations
        //################################################
        public IQueryable<AGMManagerDisaffiliateRqst> Get_Pending_AGMManagerDisaffiliateRqsts(DateTime dtRunTime)
        {
            return adgme.AGMManagerDisaffiliateRqsts.Where(r => r.uPending == true && r.uCancelled == false && r.DisaffiliateOn.Value <= dtRunTime);
        }

        //################################################
        // Add ADGM Manager Disaffiliation Request
        //################################################
        public void Add_AGMManagerDisaffiliateRqst(AGMManagerDisaffiliateRqst nAMDR)
        {
            nAMDR.SubmittedOn = DateTime.Now;
            adgme.AGMManagerDisaffiliateRqsts.Add(nAMDR);
            adgme.SaveChanges();

        }

        //################################################
        // Cancel ADGM Manager Disaffiliation Request
        //################################################
        public string Cancel_AGMManagerDisaffiliationRqst(int cnlAMMDR)
        {
            string rStatus = string.Empty;

            try
            {
                //Pull Manager Disaffiliation
                AGMManagerDisaffiliateRqst amdr = Get_AGMManagerDisaffilationRqst_By_ID(cnlAMMDR);

                if(amdr.uPending == true)
                {
                    //Set Cancel Status
                    amdr.uCancelled = true;
                    amdr.uPending = true;
                }

                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //##################################################
        // Pull Manager Disaffiliation by ID
        //##################################################
        public AGMManagerDisaffiliateRqst Get_AGMManagerDisaffilationRqst_By_ID(int nAGMDR)
        {
            return adgme.AGMManagerDisaffiliateRqsts.Where(r => r.AMDRID == nAGMDR).FirstOrDefault();
        }

        //###################################################
        // Complete Manager Disaffiliation Request
        //###################################################
        public void Complete_AGMManagerDisaffiliateRqst(int cmpltAGMDR)
        {

            //Pull Manager Disaffiliation Request by ID
            AGMManagerDisaffiliateRqst cmpAGMMDrqt = Get_AGMManagerDisaffilationRqst_By_ID(cmpltAGMDR);

            //Set Complete Status
            cmpAGMMDrqt.CompletedOn = DateTime.Now;
            cmpAGMMDrqt.uPending = false;
            cmpAGMMDrqt.uCancelled = false;
            adgme.SaveChanges();

        }


        //#####################################################################
        // ADGM Manager Snapshot Functions
        //#####################################################################

        //#################################################
        // Add Manager Snapshot
        //#################################################
        public void Add_AGMManagerSnapShot(AGMManagerSnapShot nwAMSS)
        {
            adgme.AGMManagerSnapShots.Add(nwAMSS);
            adgme.SaveChanges();
        }

        //#################################################
        // Update Manager Snapshot
        //#################################################
        public void Update_AGMManagerSnapShot(AGMManagerSnapShot updtAMSS)
        {
            //Var for Update TimeStamp
            DateTime dtUpdateStmp = DateTime.Now;

            //Update Manager Snapshot
            updtAMSS.SSRptDate = dtUpdateStmp;

            //Update Manager Assignements As Well
            if (updtAMSS.AGMManagerSnapShot_MgmtAssignment != null && updtAMSS.AGMManagerSnapShot_MgmtAssignment.Count > 0)
            {
                foreach(AGMManagerSnapShot_MgmtAssignment amssma in updtAMSS.AGMManagerSnapShot_MgmtAssignment)
                {
                    amssma.SSRptDate = dtUpdateStmp;
                }
            }

            adgme.SaveChanges();

        }

        //#####################################################
        // Pull Individual Manager Snap Shot by Snap Shot ID
        //#####################################################
        public AGMManagerSnapShot Get_Indv_AGMManagerSnapShot_by_ID(int amssID)
        {
            return adgme.AGMManagerSnapShots.Where(r => r.AMSSID == amssID).FirstOrDefault();
        }

        //######################################################
        // Pull Individual Latest Manager Snap Shot by KerbID
        //######################################################
        public AGMManagerSnapShot Get_Indv_AGMManagerSnapShot_by_KerbID(string smbtKerbID)
        {
            return adgme.AGMManagerSnapShots.Where(r => r.KerbID.ToLower() == smbtKerbID.ToLower()).OrderByDescending(c => c.SSRptDate).FirstOrDefault();
        }


        //#####################################################################
        // ADGM API Auth Key Functions
        //#####################################################################

        //Pull API Auth Key by Key
        public ADGMAPIAuthKey Get_ADGMAPIAuthKey_By_Key(string apiKey)
        {
            return adgme.ADGMAPIAuthKeys.Where(r => r.APIAuthKey == apiKey).FirstOrDefault();
        }

        //Pull API Auth Key by Key
        public ADGMAPIAuthKey Get_ADGMAPIAuthKey_Enabled_By_Key(string apiKey)
        {
            return adgme.ADGMAPIAuthKeys.Where(r => r.APIAuthKey == apiKey && r.uEnabled == true).FirstOrDefault();
        }

        //Pull API Auth Key by KerbID
        public ADGMAPIAuthKey Get_ADGMAPIAuthKey_By_KerbID(string krbID)
        {
            return adgme.ADGMAPIAuthKeys.Where(r => r.KerbID.ToLower() == krbID.ToLower()).FirstOrDefault();
        }

        //Check for Existing API Key Assigned to Another Account
        public bool Check_ADGMAPIAuthKey_For_Existing_To_Another_User(string apiKey, string usrID)
        {
            return adgme.ADGMAPIAuthKeys.Where(r => r.KerbID.ToLower() != usrID.ToLower()).Select(c => c.APIAuthKey).Contains(apiKey);
        }

        //Update ADGM API Auth Key
        public string Update_ADGMAPIAuthKey(ADGMAPIAuthKey uCAAK)
        {
            //Var for Return Status
            string rStatus = string.Empty;

            try
            {
                uCAAK.uEnabled = true;
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }


            return rStatus;
        }

        //Add ADGM API Auth Key
        public string Add_ADGMAPIAuthKey(ADGMAPIAuthKey nCAAK)
        {
            //Var for Return Status
            string rStatus = string.Empty;

            try
            {

                if(adgme.ADGMAPIAuthKeys.Where(r => r.KerbID.ToLower() == nCAAK.KerbID.ToLower()).Count() == 0)
                {
                    nCAAK.uEnabled = true;
                    nCAAK.uPersonalAccnt = true;
                    adgme.ADGMAPIAuthKeys.Add(nCAAK);
                    adgme.SaveChanges();
                }
                else
                {
                    rStatus = "hmmm...something went wrong. Try again later.";
                }
                
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }





        //#######################################################################
        // UCD Mothra User Account Functions
        //#######################################################################

        //#####################################
        // Pull Mothra User Account by User ID
        //#####################################
        public AGMSDB_Mothra_UserAccnt Get_Manager_Mothra_Accnt_by_UserID(string ucdLogin)
        {
            return adgme.SP_Mothra_Pull_User_Account_Status_By_Login_ID(ucdLogin).ToList().FirstOrDefault();
        }

        //#######################################################################
        // UCD AD Group App Roles Functions
        //#######################################################################
        public List<UCDADGroupAppRole> Get_All_UCDADGroupAppRoles()
        {
            return adgme.UCDADGroupAppRoles.ToList();
        }

        //#######################################################################
        // AD Manager Group Functions
        //#######################################################################

        //#####################################
        // Pull All AGM Groups
        //#####################################
        public List<AGMGroup> Get_All_AGMGroups()
        {
            return adgme.AGMGroups.OrderBy(r => r.ADGrpName).ToList();
        }

        //#####################################
        // Get Individual AGMGroup by ID
        //#####################################
        public AGMGroup Get_AGMGroup_ByID(Guid agmgID)
        {
            return adgme.AGMGroups.Where(r => r.AGMGID == agmgID).FirstOrDefault();
        }

        //#####################################
        // Add AGMGroup 
        //#####################################
        public string Add_AGMGroup(AGMGroup nAGMGrp)
        {
            string rStatus = string.Empty;

            try
            {
                //Check for Existing Group with Same Object Guid
                var nExstChk = adgme.AGMGroups.Where(r => r.AGMGID == nAGMGrp.AGMGID).Count();

                if(nExstChk == 0)
                {
                    adgme.AGMGroups.Add(nAGMGrp);
                    adgme.SaveChanges();
                }
                else
                {
                    rStatus = "The requested group already exists in the database.";
                }

            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //#####################################
        // Remove AGMGroup
        //#####################################
        public string Remove_AGMGroup(AGMGroup rAGMGroup)
        {
            string rStatus = string.Empty;

            try
            {
                //List for Managers to Remove
                List<AGMManager> lRmvMangers = new List<AGMManager>();
                
                //List for Member Requests to Remove
                List<AGMMemberRequest> lRmvMemberRequests = new List<AGMMemberRequest>();

                //Check for Managers to Remove
                if(rAGMGroup.AGMManagers != null && rAGMGroup.AGMManagers.Count > 0)
                {
                    lRmvMangers = rAGMGroup.AGMManagers.ToList();

                    foreach(AGMManager rManger in lRmvMangers)
                    {
                        rAGMGroup.AGMManagers.Remove(rManger);
                    }

                }//End of Manager to Remove Check



                //Check for Member Requests to Remove
                if(rAGMGroup.AGMMemberRequests != null && rAGMGroup.AGMMemberRequests.Count > 0)
                {
                    lRmvMemberRequests = rAGMGroup.AGMMemberRequests.ToList();

                    foreach(AGMMemberRequest rMbrRequest in lRmvMemberRequests)
                    {
                        rAGMGroup.AGMMemberRequests.Remove(rMbrRequest);
                    }

                }//End of Member Requests to Remove Check

                

                adgme.AGMGroups.Remove(rAGMGroup);
                adgme.SaveChanges();

            }
            catch (Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //###############################################
        // Update AGM Group
        //###############################################
        public string Update_AGMGroup(AGMGroup uAGMGrp, string updatedBy)
        {

            //Var for Return Status
            string rStatus = string.Empty;

            try
            {
                uAGMGrp.ModifiedOn = DateTime.Now;
                uAGMGrp.ModifiedBy = updatedBy.ToLower();
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

       
        //#############################################
        // Pull Group Pending Member Actions
        //#############################################
        public List<AGMGroup> Get_AGMGroups_Pending_Membership_Requests()
        {
            return adgme.AGMGroups.Where(r => r.AGMMemberRequests.Any(c => c.Pending == true)).ToList();
        }

        //#############################################
        // Update AGM Group Modify Stamps
        //#############################################
        public void Update_AGMGroup_ModifyTime(AGMGroup uAGMGrp, string updatedBy)
        {
            try
            {
                uAGMGrp.ModifiedOn = DateTime.Now;
                uAGMGrp.ModifiedBy = updatedBy.ToLower();
                adgme.SaveChanges();
            }
            catch
            {
                //Nada
            }

        }

        //##############################################
        // Update AGM Group AD Info and MgrNtcLastSent
        //##############################################
        public void Update_AGMGroup_AD_Info_MgrNotice(Guid grpGuid, string grpDN, string grpCN, string grpDesc, bool bMgrNtcSent)
        {
            try
            {

                //Pull AGM Group by GUID
                var agmGroup = adgme.AGMGroups.Where(r => r.AGMGID == grpGuid).First();
                agmGroup.ADGrpDN = grpDN;
                agmGroup.ADGrpName = grpCN;
                agmGroup.GrpDescription = grpDesc;
                
                //Check to See If Notice was Sent to Managers
                if(bMgrNtcSent)
                {
                    agmGroup.MngrNtcLastSent = DateTime.Now;
                }

                //Save Changes to AGM Group
                adgme.SaveChanges();

            }
            catch
            {
                //Nada
            }
        }

        //##############################################
        // Associate Existing Manager to AGM Group
        //##############################################
        public string Associate_Manager_to_AGMGroup(AGMGroup agmGrp, AGMManager agmMgr)
        {
            //Var for Return Status
            string rStatus = string.Empty;

            try
            {
                agmGrp.AGMManagers.Add(agmMgr);
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //################################################
        // Create New Manager and Associate AGM Group
        //################################################
        public string Associate_NewManager_to_AGMGroup(AGMGroup agmGrp, AGMManager nAGMMgr)
        {
            //Var for Return Status
            string rStatus = string.Empty;

            try
            {
                if(!Check_For_Existing_AGMManager_By_KerbID(nAGMMgr.KerbID))
                {
                    adgme.AGMManagers.Add(nAGMMgr);
                    agmGrp.AGMManagers.Add(nAGMMgr);
                    adgme.SaveChanges();
                }
                else
                {
                    rStatus = "Existing manager account. Try again";
                }

            }
            catch (Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }



        //###############################################################
        // Get AGM Groups that User is Manager On Based Upon Kerb ID
        //###############################################################
        public List<AGMGroup> Get_AGMGroups_For_Manager_By_KerbID(string kerbID)
        {
            return adgme.AGMGroups.Where(r => r.AGMManagers.Any(c => c.KerbID.ToLower() == kerbID.ToLower())).ToList();
        }


        //#############################################################
        // AGM Manager Functions
        //#############################################################

        //###########################################
        // Pull All AGM Managers
        //###########################################
        public List<AGMManager> Get_All_AGMManagers()
        {
            return adgme.AGMManagers.ToList();
        }

        //#################################################
        // Pull All Managers Set for All Group Reports
        //#################################################
        public List<AGMManager> Get_Managers_Wanting_AllGroups_Report()
        {
            return adgme.AGMManagers.Where(r => r.SendAllGrpsRpt == true).ToList();
        }


        //###########################################
        // Add AGM Manager
        //###########################################
        public string Add_AGMManager(AGMManager nAGMManger)
        {
            string rStatus = string.Empty;

            try
            {
                adgme.AGMManagers.Add(nAGMManger);
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //########################################
        // Update AGM Manager 
        //########################################
        public string Update_AGM_Manager(AGMManager updtAGMManager)
        { 
            //Var for Return Status
            string rStatus = string.Empty;

            try
            {
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //##########################################
        // Remove AGM Manager
        //##########################################
        public string Remove_AGMManager(AGMManager rAGMMgr)
        {
            //Var for Return Status
            string rStatus = string.Empty;

            //List for Groups to Remove
            List<AGMGroup> lRmvGroups = new List<AGMGroup>();

            try
            {
                //Check for Group to Remove
                if(rAGMMgr.AGMGroups != null && rAGMMgr.AGMGroups.Count() > 0)
                {
                    //Load Remove List
                    lRmvGroups = rAGMMgr.AGMGroups.ToList();

                    foreach(AGMGroup rGrp in lRmvGroups)
                    {
                        rAGMMgr.AGMGroups.Remove(rGrp);
                    }

                }//End of Groups Check

                adgme.AGMManagers.Remove(rAGMMgr);
                adgme.SaveChanges();

            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //###########################################
        // Check for Existing AGM Manger by Kerb ID
        //###########################################
        public bool Check_For_Existing_AGMManager_By_KerbID(string kerbID)
        {
            return adgme.AGMManagers.Select(r => r.KerbID.ToLower()).Contains(kerbID.ToLower());
        }

        //###########################################
        // Get AGM Manager by Kerb ID
        //###########################################
        public AGMManager Get_AGMManager_By_KerbID(string kerbID)
        {
            return adgme.AGMManagers.Where(r => r.KerbID.ToLower() == kerbID.ToLower()).FirstOrDefault();
        }

        //###########################################
        // Get AGM Manager by DB ID
        //###########################################
        public AGMManager Get_AGMManger_By_ID(int mngrID)
        {
            return adgme.AGMManagers.Where(r => r.AGMMID == mngrID).FirstOrDefault();
        }

        
        //#################################################################
        // AGM Member Request Functions
        //#################################################################

        //#######################################################
        // Get AGM Member Requests Submitted on By Certain Date
        //#######################################################
        public List<AGMMemberRequest> Get_AGMMemberRequests_By_Date(DateTime frmDate)
        {
            return adgme.AGMMemberRequests.ToList().Where(r => r.SubmittedOn >= frmDate).ToList();
        }

        //#######################################################
        // Get All AGM Member Requests Submitted
        //#######################################################
        public IQueryable<AGMMemberRequest> Get_All_AGMMemberRequests()
        {
            return adgme.AGMMemberRequests;
        }

        //######################################################
        // Add AGM Member Request
        //######################################################
        public string Add_AGMMemberRequest(AGMMemberRequest nAGMRqst)
        {
            //Var for Return Value
            string rStatus = string.Empty;

            try
            {
                adgme.AGMMemberRequests.Add(nAGMRqst);
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //#################################################
        // Complete AGM Member Request
        //#################################################
        public string Complete_AGMMemberRequest(int agmmrID)
        {
            string rStatus = string.Empty;

            try
            {
                var mbrRequest = adgme.AGMMemberRequests.Where(r => r.AGMMRID == agmmrID).First();
                mbrRequest.Pending = false;
                mbrRequest.CompletedOn = DateTime.Now;
                adgme.SaveChanges();
            }
            catch(Exception ex)
            {
                rStatus = ex.ToString();
            }

            return rStatus;
        }

        //##########################################
        // Check for AdminGroupGuid
        //##########################################
        public bool Check_For_AdminGroupGuid(Guid adGrpGuid)
        {
            bool rStatus = true;

            if (adgme.AdminGrpGuids.Where(r => r.AGGID == adGrpGuid).Count() == 0)
            {
                rStatus = false;
            }

            return rStatus;
        }


        
       

        //######################################################################
        // End of Class
        //######################################################################
    }
}
