using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class COERoleToOUGroupAccess
    {
        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        public COERoleToOUGroupAccess()
        {

        }

        public bool CheckAccess(string usrName, string grpDN)
        {
            //Var for Return Status
            bool rtStatus = false;

            //List of AD Group Roles
            List<UCDADGroupAppRole> lADGrpAppRoles = new List<UCDADGroupAppRole>();

            //Load List of AD Group App Roles from DB
            lADGrpAppRoles = agmd.Get_All_UCDADGroupAppRoles();

            //Arry for User Assigned Roles
            string[] usrRoles = Roles.GetRolesForUser(usrName);

            //If More Than Basic UCDUser Role then Check Access
            if(usrRoles != null && usrRoles.Count() > 1)
            {
                //Loop Through Each Role Name
                foreach(string usrRole in usrRoles)
                {

                    //Loop Through the Roles
                    foreach(UCDADGroupAppRole uAGAR in lADGrpAppRoles)
                    {
                        //Check for Role Name Match
                        if(String.IsNullOrEmpty(uAGAR.RoleName) == false && uAGAR.RoleName == usrRole)
                        {
                            //Check for Associated AD Object DN Partials
                            if(uAGAR.UCDADObjctDNPartials != null && uAGAR.UCDADObjctDNPartials.Count > 0)
                            {
                                foreach(UCDADObjctDNPartial uaodPart in uAGAR.UCDADObjctDNPartials)
                                {
                                    if(grpDN.ToLower().Contains(uaodPart.DNPartial.ToLower()))
                                    {
                                        rtStatus = true;
                                    }
                                }
                            }

                        }//End of Role Name Checks

                    }//End of lADGrpAppRoles

                    
                }//End of usrRoles Foreach

            }//End of usrRoles Null|Empty Check

            return rtStatus;
        }


    }
}