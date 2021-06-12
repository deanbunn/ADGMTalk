using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Mail;
using COEADGroupManagerSQL;


namespace COEADGroupManagerSecurityAccess
{
    class COEADGroupManagerRoleProvider : RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {

            //Initiate Entities 
            COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

            //List to Hold Roles User is In
            List<string> lRoles = new List<string>();

            //List of AD Group Roles
            List<UCDADGroupAppRole> lADGrpAppRoles = new List<UCDADGroupAppRole>();

            //Create Directory Entry for Root Domain in the Forest
            DirectoryEntry deRoot = Forest.GetCurrentForest().RootDomain.GetDirectoryEntry();

            //Create Directory Searcher Using Root
            DirectorySearcher dsSearcher = new DirectorySearcher(deRoot);

            //Set Search Scope
            dsSearcher.SearchScope = SearchScope.Subtree;

            //Set Search Page Size
            dsSearcher.PageSize = 900;

            //Load Properties to Return
            dsSearcher.PropertiesToLoad.Add("distinguishedName");
            dsSearcher.PropertiesToLoad.Add("memberof");

            //Search for Specific User
            dsSearcher.Filter = "(&(objectclass=user)(sAMAccountName=" + username + "))";
            SearchResult usrSrchResult = dsSearcher.FindOne();

            //Check to See If Required Account Information is Returned
            if (usrSrchResult != null && usrSrchResult.Properties["distinguishedName"].Count > 0 && usrSrchResult.Properties["memberof"].Count > 0)
            {

                //Create Directory Entry for Departments Roles OU
                DirectoryEntry deDeptRolesOU = new DirectoryEntry("LDAP://OU=COE-OU-IT-AppRoles,OU=XXXXXXXX,OU=XXXXXXX,OU=XXXXXXXX,DC=XXXXXX,DC=XXXXXX,DC=XXXXXXX,DC=edu");

                //Set Search Root to Departments Roles OU
                dsSearcher.SearchRoot = deDeptRolesOU;

                //Set Search Filter to Look for All Groups User is a Member of
                dsSearcher.Filter = "(&(objectclass=group)(member:1.2.840.113556.1.4.1941:=" + usrSrchResult.Properties["distinguishedName"][0].ToString() + "))";

                //Run Search
                SearchResultCollection srResults = dsSearcher.FindAll();

                //Check to See Search Results Returned
                if (srResults != null && srResults.Count > 0)
                {
                    //Pull App Role from DB
                    lADGrpAppRoles = agmd.Get_All_UCDADGroupAppRoles();

                    //Loop Through Results and Assign Roles
                    foreach (SearchResult srResult in srResults)
                    {
                        //Var for Current Group Membership
                        string crntMbrshpDN = srResult.Properties["distinguishedName"][0].ToString().ToLower();

                        //Loop Through App Roles
                        foreach(UCDADGroupAppRole uaga in lADGrpAppRoles)
                        {
                            //Check DNs of Membership and Role
                            if(String.IsNullOrEmpty(uaga.RoleGroupDN) == false && uaga.RoleGroupDN == crntMbrshpDN)
                            {
                                //Add Role Name
                                lRoles.Add(uaga.RoleName);

                                //Check for ADGM Partner and Dept Role
                                if((bool)uaga.IsDeptRole == true && lRoles.Contains("ADGMPartner") == false)
                                {
                                    lRoles.Add("ADGMPartner");
                                }

                            }//End of DN Checks

                        }//End of lADGrpAppRoles Foreach

                    }//End of srResults Foreach

                }//End of srResults Null Check

            }//End of User Search Result and DN Value Check


            //Add Default UCD Role for Auditing
            lRoles.Add("UCDUser");

            return lRoles.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return this.GetRolesForUser(username).Contains(roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
