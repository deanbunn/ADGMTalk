using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.Protocols;
using COEADGroupManagerSQL;

namespace UCDMemberADGM
{
    public class CampusLDAP
    {

        public string KerbID { get; set; }
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentCodeAssigned { get; set; }
        public string CommonName { get; set; }
        public string TelephoneNumber { get; set; }
        public string DepartmentName { get; set; }
        public string Title { get; set; }
        public string TitleCode { get; set; }
        public string StudentMajor { get; set; }
        public string StudentLevel { get; set; }
        public string PostalAddress { get; set; }
        public string StudentPIDM { get; set; }
        public string UCDAffiliation { get; set; }
        public string EmployeeNumber { get; set; }
        public string StudentIDNumber { get; set; }

        // Private Members
        private LdapConnection _connection = null;
        private LdapDirectoryIdentifier _identifier = null;
        private static string[] allAttributes = new string[] { "ucdPersonUUID",
                                                               "ucdPersonPIDM",
                                                               "ucdPersonAffiliation",
                                                                "uid",
                                                                "cn",
                                                                "mail", 
                                                                "displayName", 
                                                                "ou", 
                                                                "departmentNumber", 
                                                                "title", 
                                                                "ucdStudentMajor", 
                                                                "ucdStudentLevel", 
                                                                "employeeNumber", 
                                                                "ucdAppointmentDepartmentCode", 
                                                                "ucdAppointmentTitleCode", 
                                                                "sn", 
                                                                "givenName", 
                                                                "ucdStudentSID"};

        public CampusLDAP()
        {
            //Initiate Entities 
            COEADGroupManagerDBRepository agmd2 = new COEADGroupManagerDBRepository();

            //Pull LDAP Service Creds from SQL DB
            UCDCampusServiceCred uCmpSrvCred = agmd2.Get_UCDCampusServiceCred_By_ServiceName("LDAPCOE");

            _identifier = new LdapDirectoryIdentifier("xxxxx.xxxxxxxx.edu", 636, true, false);
            _connection = new LdapConnection(_identifier);
            _connection.Credential = new System.Net.NetworkCredential("uid=xxxxxxx,ou=xxxxxxxx,dc=xxxxxxxxxx,dc=edu", uCmpSrvCred.ServicePwd);
            _connection.AuthType = AuthType.Basic;
            _connection.SessionOptions.ProtocolVersion = 3;
            _connection.SessionOptions.SecureSocketLayer = true;
            _connection.Bind();

          
        }

        public void SearchForUser(bool srchByUserID, string ucdID)
        {

            KerbID = string.Empty;
            Mail = string.Empty;
            DisplayName = string.Empty;
            LastName = string.Empty;
            FirstName = string.Empty;
            DepartmentCode = string.Empty;
            DepartmentCodeAssigned = string.Empty;
            CommonName = string.Empty;
            TelephoneNumber = string.Empty;
            DepartmentName = string.Empty;
            Title = string.Empty;
            StudentMajor = string.Empty;
            StudentLevel = string.Empty;
            PostalAddress = string.Empty;
            StudentPIDM = string.Empty;
            UCDAffiliation = string.Empty;
            EmployeeNumber = string.Empty;
            TitleCode = string.Empty;
            StudentIDNumber = string.Empty;


            string strSearchBase = "ou=xxxxxxxx,dc=xxxxxxx,dc=edu";

            string strFilter = string.Empty;

            if (srchByUserID == true)
            {
                strFilter = "(&(uid=" + ucdID.Trim().ToLower() + "))";
            }
            else
            {
                strFilter = "(&(mail=" + ucdID.Trim().ToLower() + "))";
            }


            //Only Run if Filter has Been Set
            if (!string.IsNullOrEmpty(strFilter))
            {

                SearchRequest sRequest = new SearchRequest(strSearchBase, strFilter, SearchScope.Subtree, allAttributes);
                //Send the Request and Load the Response
                SearchResponse sResponse = (SearchResponse)_connection.SendRequest(sRequest);

                SearchResultEntryCollection sreCol = sResponse.Entries;

                foreach (SearchResultEntry entry in sreCol)
                {
                    SearchResultAttributeCollection attributes = entry.Attributes;

                    foreach (DirectoryAttribute attr in attributes.Values)
                    {

                        switch (attr.Name.ToString())
                        {

                            case "uid":
                                KerbID = attr[0].ToString().ToUpper();
                                break;

                            case "mail":
                                Mail = attr[0].ToString();
                                break;

                            case "displayName":
                                DisplayName = attr[0].ToString();
                                break;

                            case "givenName":
                                FirstName = attr[0].ToString();
                                break;

                            case "sn":
                                LastName = attr[0].ToString();
                                break;

                            case "departmentNumber":
                                DepartmentCode = attr[0].ToString();
                                break;

                            case "ucdAppointmentDepartmentCode":
                                DepartmentCodeAssigned = attr[0].ToString();
                                break;

                            case "ucdPersonPIDM":
                                StudentPIDM = attr[0].ToString();
                                break;

                            case "ucdPersonAffiliation":
                                UCDAffiliation = attr[0].ToString();
                                break;

                            case "ou":
                                DepartmentName = attr[0].ToString();
                                break;

                            case "title":
                                Title = attr[0].ToString();
                                break;

                            case "ucdStudentMajor":
                                StudentMajor = attr[0].ToString();
                                break;

                            case "ucdStudentLevel":
                                StudentLevel = attr[0].ToString();
                                break;

                            case "employeeNumber":
                                EmployeeNumber = attr[0].ToString();
                                break;

                            case "ucdAppointmentTitleCode":
                                TitleCode = attr[0].ToString();
                                break;

                            case "ucdStudentSID":
                                StudentIDNumber = attr[0].ToString();
                                break;

                            case "cn":
                                CommonName = attr[0].ToString();
                                break;

                        }//End of Attr Name Switch 


                    }//End of Foreach Attr in Attribute Collection


                }//End of Foreach Search Collection

            }//End of strFilter Null\Empty Check

        }//End of SearchForUser Method

    }//End of CampusLDAP Class
}
