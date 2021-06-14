using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;

namespace COEADGroupManager.Models
{
    public class ADGMGrpVerifier
    {

        public bool uADGrpStatus { get; set; }
        public string uADGrpName { get; set; }
        public string uADGrpDN { get; set; }

        public ADGMGrpVerifier()
        {

        }

        public void Check_uConnect_Group_By_Guid(string uGrpGuid)
        {
            uADGrpStatus = false;
            uADGrpName = string.Empty;
            uADGrpDN = string.Empty;

            //Var for AD GUID Path
            string uPath = "LDAP://xxxx.xxxxxxx.edu/<GUID=" + uGrpGuid + ">";

            //Check to See Group Exists in AD
            if (DirectoryEntry.Exists(uPath))
            {
                //Pull Directory Entry of Group and Assign Status Values
                DirectoryEntry deGroup = new DirectoryEntry(uPath);
                uADGrpDN = deGroup.Properties["distinguishedname"][0].ToString();
                uADGrpName = deGroup.Properties["cn"][0].ToString();

                //Determine If COE Sync Group
                int nAtrCnt = deGroup.Properties["extensionAttribute2"].Count;
                if (nAtrCnt > 0 && deGroup.Properties["extensionAttribute2"][0].ToString().ToUpper().Trim() == "COEADGM")
                {
                    uADGrpStatus = true;
                }

            }

        }//End Get_uConnect_Group_By_Guid




    }//End of ADGMGrpVerifier
}