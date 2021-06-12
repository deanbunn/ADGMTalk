using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace COEADGroupManager.Models
{
    public class ADGMUsrSearchADResultCollection
    {
        public string rcStatus { get; set; }

        public List<ADGMUsrSearchADResult> lResults { get; set; }

        public ADGMUsrSearchADResultCollection(string uSearchTerm)
        {
            //Initiate Result List
            lResults = new List<ADGMUsrSearchADResult>();

            //Var for First and Last Name Together Array
            string[] arrName;

            //Var for Search Filter
            String strFilter = "(&(objectclass=user)(!objectClass=organizationalUnit)(!objectClass=computer)(!objectClass=contact)";

            //Chech for Email Address or UPN Format. If Not then Search for Other User Properties
            if (uSearchTerm.Contains("@"))
            {
                strFilter = strFilter + "(|";
                strFilter = strFilter + "(mail=" + uSearchTerm + "*)";
                strFilter = strFilter + "(userPrincipalName=" + uSearchTerm + "*)";
                strFilter = strFilter + "(proxyAddresses=smtp:" + uSearchTerm + "*)";
            }
            else
            {
                //Check the Search Term for a Space
                if (String.IsNullOrEmpty(uSearchTerm) == false && uSearchTerm.Trim().Contains(" ") && uSearchTerm.Trim().Split(' ').Count() == 2)
                {
                    //Put Search Term Into Array
                    arrName = uSearchTerm.Trim().Split(' ');

                    strFilter = strFilter + "(|";
                    strFilter = strFilter + "(&(givenName=" + arrName[0] + "*)(sn=" + arrName[1] + "*))";
                    strFilter = strFilter + "(sn=" + uSearchTerm.Trim() + "*)";
                    strFilter = strFilter + "(displayName=" + uSearchTerm + "*)";

                }
                else
                {

                    strFilter = strFilter + "(|";
                    strFilter = strFilter + "(givenName=" + uSearchTerm + "*)";
                    strFilter = strFilter + "(sn=" + uSearchTerm + "*)";
                    strFilter = strFilter + "(mail=" + uSearchTerm + "*)";
                    strFilter = strFilter + "(displayName=" + uSearchTerm + "*)";
                }

            }//End of Email Address or UPN Format

            strFilter = strFilter + "))";

            //Directory Entry for uConnect Domain
            DirectoryEntry deDomain = new DirectoryEntry("LDAP://DC=XXXXX,DC=XXXXXXXX,DC=EDU");

            //Directory Searcher to Conduct Search
            DirectorySearcher dsSearch = new DirectorySearcher();
            dsSearch.SearchRoot = new DirectoryEntry(deDomain.Path);
            dsSearch.Filter = strFilter;
            dsSearch.PageSize = 100;
            dsSearch.SizeLimit = 100;
            dsSearch.SearchScope = System.DirectoryServices.SearchScope.Subtree;
            dsSearch.PropertiesToLoad.Add("sAMAccountName");
            dsSearch.PropertiesToLoad.Add("displayName");
            dsSearch.PropertiesToLoad.Add("mail");
            dsSearch.PropertiesToLoad.Add("givenName");
            dsSearch.PropertiesToLoad.Add("sn");
            dsSearch.PropertiesToLoad.Add("proxyAddresses");
            dsSearch.PropertiesToLoad.Add("distinguishedName");
            dsSearch.PropertiesToLoad.Add("userAccountControl");
            dsSearch.PropertiesToLoad.Add("objectGuid");
            dsSearch.PropertiesToLoad.Add("extensionAttribute6");
            dsSearch.PropertiesToLoad.Add("department");

            //Pull Search Results
            SearchResultCollection srcResults = dsSearch.FindAll();

            if (srcResults != null)
            {

                foreach (SearchResult srcResult in srcResults)
                {

                    if (lResults.Count < 100)
                    {

                        //Initiate Search Result Reporting Object
                        ADGMUsrSearchADResult asar = new ADGMUsrSearchADResult();

                        //Set Object Guid
                        asar.ad_object_guid = srcResult.Properties["objectGuid"][0].ToString();

                        //Set SAM
                        asar.user_id = srcResult.Properties["sAMAccountName"][0].ToString().ToLower();

                        //Set DN
                        asar.ad_dn = srcResult.Properties["distinguishedName"][0].ToString();

                        //Check Domain
                        if (asar.ad_dn.ToLower().Contains(",dc=ou,"))
                        {
                            asar.ad_domain = "ou";
                        }
                        else
                        {
                            asar.ad_domain = "ad3";
                        }


                        //Set Display Name
                        int intDNCount = srcResult.Properties["displayName"].Count;

                        if (intDNCount > 0)
                        {
                            asar.display_name = srcResult.Properties["displayName"][0].ToString();
                        }


                        //Set First Name
                        int intGNCount = srcResult.Properties["givenName"].Count;
                        if (intGNCount > 0)
                        {
                            asar.first_name = srcResult.Properties["givenName"][0].ToString();
                        }


                        //Set Last Name
                        int intSNCount = srcResult.Properties["sn"].Count;
                        if (intSNCount > 0)
                        {
                            asar.last_name = srcResult.Properties["sn"][0].ToString();
                        }

                        //Set Title
                        int intTitleCount = srcResult.Properties["extensionAttribute6"].Count;
                        if (intTitleCount > 0)
                        {
                            asar.title = srcResult.Properties["extensionAttribute6"][0].ToString();
                        }

                        //Set Department
                        int intDeptCount = srcResult.Properties["department"].Count;
                        if (intDeptCount > 0)
                        {
                            asar.department = srcResult.Properties["department"][0].ToString();
                        }

                        //userAccountControl
                        int uacCount = srcResult.Properties["userAccountControl"].Count;
                        if (uacCount > 0)
                        {

                            //Converting Numeric Account Status 
                            switch (srcResult.Properties["userAccountControl"][0].ToString())
                            {

                                case "512":
                                    asar.ad_enabled = "Yes";
                                    break;
                                case "514":
                                    asar.ad_enabled = "No";
                                    break;
                                case "544":
                                    asar.ad_enabled = "Yes";
                                    break;
                                case "546":
                                    asar.ad_enabled = "No";
                                    break;
                                case "66048":
                                    asar.ad_enabled = "Yes";
                                    break;
                                case "66050":
                                    asar.ad_enabled = "No";
                                    break;
                                case "8388608":
                                    asar.ad_enabled = "Yes";
                                    break;
                                default:
                                    asar.ad_enabled = "Yes";
                                    break;

                            }//End of UAC Switch

                        }//End of UAC

                        //Check Primary Email Address
                        int iPA = srcResult.Properties["proxyAddresses"].Count;
                        if (iPA > 0)
                        {

                            for (int x = 0; x <= iPA - 1; x++)
                            {

                                string strSMTP = srcResult.Properties["proxyAddresses"][x].ToString();

                                if (strSMTP.StartsWith("SMTP:"))
                                {
                                    asar.email_address = strSMTP.ToLower().Replace("smtp:", "");
                                }

                            }

                        }


                        //Check Mail 
                        int iMail = srcResult.Properties["mail"].Count;
                        if (string.IsNullOrEmpty(asar.email_address) == true && iMail > 0)
                        {
                            asar.email_address = srcResult.Properties["mail"][0].ToString().ToLower();
                        }

                        //Add Search Result to Collection
                        lResults.Add(asar);

                    }//End of Results 100 Check

                }//End of srcResults Foreach

                //Sort View Results Before Returning
                lResults = lResults.OrderBy(r => r.display_name).ToList();

            }//End of Null Check on srcResults

        }

    }
}