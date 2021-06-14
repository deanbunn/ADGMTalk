using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using COEADGroupManagerSQL;


namespace COEADGroupManager.Models
{
    public class MembersGetMembersViewModel
    {
        public string gmStatus { get; set; }

        public AGMGroup agmGroup { get; set; }

        public List<ADGMuCntGrpMember> lUCGM { get; set; }

        public bool bShowBulk { get; set; }

        public MembersGetMembersViewModel()
        {
            lUCGM = new List<ADGMuCntGrpMember>();
            bShowBulk = false;
        }


        public void LoadGroupMembers()
        {

            PrincipalContext prctxOU = new PrincipalContext(ContextType.Domain, "XXXX", "DC=XXX,DC=XXXXX,DC=XXXXXX,DC=EDU");

            //Var for Group's LDAP Path Based Upon AD GUID
            string grpLDAPPath = "LDAP://xxxx.xxxxx.edu/<GUID=" + agmGroup.AGMGID.ToString() + ">";
           
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
                    //Loop Through Group Membership (Not Nested) and Load HashTable with DNs
                    foreach (var crntMbr in grpPrincipal.GetMembers(false))
                    {
                        //Pull Directory Entry for Group Member
                        DirectoryEntry deMember = crntMbr.GetUnderlyingObject() as DirectoryEntry;

                        //Load Up HashTable with Current Users DN
                        if (deMember != null && deMember.Properties["distinguishedName"].Count > 0)
                        {
                            //Initiate Group Member Reporting Object
                            ADGMuCntGrpMember uCntGrpMbr = new ADGMuCntGrpMember();

                            //Load Only AD3 Users
                            if (deMember.Properties["objectClass"].Contains("user") == true && deMember.Properties["objectClass"].Contains("computer") == false && deMember.Properties["distinguishedName"][0].ToString().ToLower().Contains(",dc=xxx,dc=xxxxx,dc=xxxxxx,dc=edu") == false)
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


                                lUCGM.Add(uCntGrpMbr);

                            }//End of AD3 User Checks

                        }//End of deMember and DN Null Checks

                        //Close out DirectoryEntry for Group Member
                        deMember.Close();

                    }//End of Group Membership Foreach

                    //Check for lUCGM Count Check
                    if(lUCGM != null && lUCGM.Count() > 0)
                    {
                        bShowBulk = true;
                    }

                }//End of Current Group Membership Check

                //Close out DirectoryEntry for Group
                deADGroup.Close();

            }//End of Directory Entry Exists Check

        }//End of LoadGroupMembers


    }
}