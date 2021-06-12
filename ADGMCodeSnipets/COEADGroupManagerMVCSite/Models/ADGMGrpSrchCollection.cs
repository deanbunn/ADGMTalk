using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using COEADGroupManagerSQL;


namespace COEADGroupManager.Models
{
    public class ADGMGrpSrchCollection
    {

        public List<ADGMGrpSrchResult> lADGMGrpSrchResults { get; set; }

        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        public ADGMGrpSrchCollection(string srchDomain, string srchGroupName)
        {
            //Initiate the Search Results List
            lADGMGrpSrchResults = new List<ADGMGrpSrchResult>();

            //Create List of SQL AGMGroups and Fill It
            List<AGMGroup> lDB_AGMGroups = new List<AGMGroup>();
            lDB_AGMGroups = agmd.Get_All_AGMGroups();

            //Var for ADSI Path
            string adsiPath = string.Empty;

            //Switch Statement Based Upon Search Domain to Set ADSI Path
            switch (srchDomain.ToLower())
            {
                case "xxxx":
                    adsiPath = "LDAP://OU=XXXXXX,DC=XXXX,DC=XXXX,DC=XXXXXXXX,DC=EDU";
                    break;

                case "xxx":
                    adsiPath = "LDAP://DC=XXXX,DC=XXXXXXX,DC=EDU";
                    break;

                default:
                    adsiPath = "LDAP://DC=XXXX,DC=XXXXX,DC=EDU";
                    break;
            }


            //Configure AD Search Filter
            string adGroupFilter = "(&(objectclass=group)(extensionAttribute2=coeadgm)(|(groupType=8)(groupType=-2147483640))(|(displayName=" + srchGroupName + "*)" + "(cn=" + srchGroupName + "*)))";

            //Search AD for Group
            DirectoryEntry deDomain = new DirectoryEntry(adsiPath);
            DirectorySearcher dsSearch = new DirectorySearcher(deDomain);
            dsSearch.Filter = adGroupFilter;
            dsSearch.SearchScope = SearchScope.Subtree;
            dsSearch.PageSize = 600;
            dsSearch.PropertiesToLoad.Add("cn");
            dsSearch.PropertiesToLoad.Add("distinguishedname");
            dsSearch.PropertiesToLoad.Add("displayname");
            dsSearch.PropertiesToLoad.Add("objectGuid");
            dsSearch.PropertiesToLoad.Add("groupType");

            //Pull Search Collection
            SearchResultCollection srcResults = dsSearch.FindAll();


            //Check for Search Results
            if (srcResults != null)
            {
                //Loop Through Results
                foreach (SearchResult srResult in srcResults)
                {
                    //Only Add the First 30 Results
                    if (lADGMGrpSrchResults.Count <= 30)
                    {
                        //Initiate Admin AD Group
                        ADGMGrpSrchResult amgsr = new ADGMGrpSrchResult();

                        //Use Common Name for Group Name
                        int nCN = srResult.Properties["cn"].Count;
                        if (nCN > 0)
                        {
                            amgsr.GrpCN = srResult.Properties["cn"][0].ToString();
                        }

                        //Add AD Object Guid and Check Existing Status
                        int nOG = srResult.Properties["objectGuid"].Count;
                        if (nOG > 0)
                        {

                            amgsr.GrpGuid = new Guid((Byte[])srResult.Properties["objectGuid"][0]);

                            int nGDB = lDB_AGMGroups.Where(r => r.AGMGID == amgsr.GrpGuid).Count();
                            if (nGDB > 0)
                            {
                                amgsr.GrpExistingStatus = true;
                            }

                        }

                        //Add Display Name
                        int nDSPN = srResult.Properties["displayname"].Count;
                        if (nDSPN > 0)
                        {
                            amgsr.GrpDisplayName = srResult.Properties["displayname"][0].ToString();
                        }

                        //Add DN 
                        int nDN = srResult.Properties["distinguishedname"].Count;
                        if (nDN > 0)
                        {
                            amgsr.GrpDN = srResult.Properties["distinguishedname"][0].ToString();
                        }

                        //Add Group to Listing
                        lADGMGrpSrchResults.Add(amgsr);

                    }//End of lADGrps Count Check

                }//End of srcResults Foreach

                lADGMGrpSrchResults = lADGMGrpSrchResults.OrderBy(r => r.GrpCN).ToList();

            }//End of srcResults Null Check

            //Close out DirectoryEntry for Domain
            deDomain.Close();

        }//End of Initiator


    }
}