using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using COEADGroupManagerSQL;


namespace COEADGroupManager.Models
{

    public class ADGMGrpLimitWrkr
    {

        public int nCrntMbrCount { get; set; }

        public ADGMGrpLimitWrkr()
        {
            nCrntMbrCount = 0;
        }


        public void Get_Current_Membership_Count(string uGrpGuid)
        {

            //Set Current Member Count to Zero
            nCrntMbrCount = 0;

            PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "XXXXX", "DC=XXXXX,DC=XXXX,DC=XXXXXX,DC=EDU");

            //Var for Group's LDAP Path Based Upon AD GUID
            string grpLDAPPath = "LDAP://xxxxx.xxxxxxx.edu/<GUID=" + uGrpGuid + ">";

            //Check to See Group Exists in AD
            if (DirectoryEntry.Exists(grpLDAPPath))
            {

                //Pull Directory Entry of AD Group Sync
                DirectoryEntry deADGroup = new DirectoryEntry(grpLDAPPath);

                //Var for Group's DN Value
                string uADGrpDN = deADGroup.Properties["distinguishedname"][0].ToString();

                //AD Group Principal
                GroupPrincipal grpPrincipal = GroupPrincipal.FindByIdentity(prctxOU, IdentityType.DistinguishedName, uADGrpDN);

                //Check Current Group Membership
                if (grpPrincipal.Members.Count > 0)
                {
                    nCrntMbrCount = grpPrincipal.GetMembers(true).Count();

                }//End of Members Count Check

            }//End of Group Exists Check

        }//End of Get_Current_Membership_Count

    }
}