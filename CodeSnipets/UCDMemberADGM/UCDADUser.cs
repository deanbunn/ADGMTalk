using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;

namespace UCDMemberADGM
{
    public class UCDADUser
    {
        public string ADSearchStatus { get; set; }
        public string Domain { get; set; }
        public string SAM { get; set; }
        public string CN { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string UPN { get; set; }
        public string DistinguishedName { get; set; }

        public UCDADUser()
        {

        }

        //Constructor with Three Vars
        public UCDADUser(string uDomain, string srchByProp, string srchPropValue)
        {
            //Set All Properties to Empty Strings
            SAM = string.Empty;
            CN = string.Empty;
            DisplayName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            EmailAddress = string.Empty;
            UPN = string.Empty;
            DistinguishedName = string.Empty;

            //Vars for Domain and ADS Path
            string adsPath = string.Empty;
            string adDomain = string.Empty;
            string adSrchFilter = string.Empty;

            //Determine Domains to Run Against
            switch (uDomain.Trim().ToUpper())
            {

                case "XXXXX":
                    adsPath = "LDAP://DC=xxxxxx,DC=xxxxxxxxxx,DC=EDU";
                    adDomain = "XXXXX.XXXXXXXXX.EDU";
                    Domain = "XXXXX";
                    break;

                case "XXXXX":
                    adsPath = "LDAP://DC=xxxxxx,DC=xxxxxx,DC=xxxxxxx,DC=EDU";
                    adDomain = "XXXXXX.XXXXXXX.XXXXXXXXX.EDU";
                    Domain = "XXXX";
                    break;

               

                default:
                    adsPath = "LDAP://DC=XXXXXX,DC=XXXXXXXX,DC=EDU";
                    adDomain = "XXXXXX.XXXXXXX.EDU";
                    Domain = "XXXX";
                    break;

            }

            //Verify Passed In Values Are Not Null or Empty
            if (string.IsNullOrEmpty(srchByProp) == false && string.IsNullOrEmpty(srchPropValue) == false)
            {

                //Configure AD Search Filter
                adSrchFilter = "(&(objectclass=user)(" + srchByProp + "=" + srchPropValue + "))";

                //Search Requested Domain for User Meeting that Criteria
                DirectoryEntry deSrchRoot = new DirectoryEntry(adsPath);
                DirectorySearcher dsSearch = new DirectorySearcher(deSrchRoot);
                dsSearch.Filter = adSrchFilter;
                dsSearch.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                dsSearch.PropertiesToLoad.Add("memberof");
                dsSearch.PropertiesToLoad.Add("sAMAccountName");
                dsSearch.PropertiesToLoad.Add("displayName");
                dsSearch.PropertiesToLoad.Add("distinguishedName");
                dsSearch.PropertiesToLoad.Add("mail");
                dsSearch.PropertiesToLoad.Add("proxyAddresses");
                dsSearch.PropertiesToLoad.Add("cn");
                dsSearch.PropertiesToLoad.Add("givenName");
                dsSearch.PropertiesToLoad.Add("sn");
                dsSearch.PropertiesToLoad.Add("userPrincipalName");

                //Search for One User Meeting Criteria
                SearchResult srResult = dsSearch.FindOne();

                if (srResult != null)
                {
                    //Common Name
                    int nCNCount = srResult.Properties["cn"].Count;
                    if (nCNCount > 0)
                    {
                        CN = srResult.Properties["cn"][0].ToString();
                    }

                    //sAMAccountName
                    SAM = srResult.Properties["sAMAccountName"][0].ToString().ToLower();

                    //Display Name
                    int dnCount = srResult.Properties["displayName"].Count;
                    if (dnCount > 0)
                    {
                        DisplayName = srResult.Properties["displayName"][0].ToString();
                    }

                    //First Name 
                    int gnCount = srResult.Properties["givenName"].Count;
                    if (gnCount > 0)
                    {
                        FirstName = srResult.Properties["givenName"][0].ToString();
                    }

                    //Last Name
                    int snCount = srResult.Properties["sn"].Count;
                    if (snCount > 0)
                    {
                        LastName = srResult.Properties["sn"][0].ToString();
                    }

                    //UPN
                    int upnCount = srResult.Properties["userPrincipalName"].Count;
                    if (upnCount > 0)
                    {
                        UPN = srResult.Properties["userPrincipalName"][0].ToString().ToLower();
                    }

                    //Pull Email Address
                    int amCount = srResult.Properties["proxyAddresses"].Count;
                    if (amCount > 0)
                    {

                        for (int i = 0; i <= amCount - 1; i++)
                        {

                            if (srResult.Properties["proxyAddresses"][i].ToString().StartsWith("SMTP:"))
                            {
                                EmailAddress = srResult.Properties["proxyAddresses"][i].ToString().ToLower().Replace("smtp:", "");
                            }

                        }

                    }

                    //If No Exchange Email Addresses Check Mail Property
                    if (String.IsNullOrEmpty(EmailAddress))
                    {
                        int mailCount = srResult.Properties["mail"].Count;
                        if (mailCount > 0)
                        {
                            EmailAddress = srResult.Properties["mail"][0].ToString().ToLower();
                        }
                    }

                    //Distinguished Name
                    if (srResult.Properties["distinguishedName"].Count > 0)
                    {
                        DistinguishedName = srResult.Properties["distinguishedName"][0].ToString();
                    }


                }//End of srResult Null Check

                //Close out Directory Entry for Domain
                deSrchRoot.Close();

            }//End of srchByProp and srchPropValue Null\Empty Checks

        }//End of Constructor with Three Vars

        public bool CheckGroupMember(string groupDN)
        {
            bool rMbrStatus = false;

            if (!String.IsNullOrEmpty(DistinguishedName))
            {
                string usrAdsPath = "LDAP://" + DistinguishedName;

                DirectoryEntry deADUser = new DirectoryEntry(usrAdsPath);

                DirectorySearcher dsSrchUser = new DirectorySearcher(deADUser);

                dsSrchUser.Filter = "(memberof:1.2.840.113556.1.4.1941:=" + groupDN + ")";

                SearchResult srSrchUser = dsSrchUser.FindOne();

                if (srSrchUser != null)
                {
                    rMbrStatus = true;
                }

                deADUser.Close();
            }

            return rMbrStatus;

        }//End of CheckGroupMember

    }
}
