using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using COEADGroupManager.Models;
using COEADGroupManagerSQL;
using COEADGroupManagerSecurityAccess;

namespace COEADGroupManager.Controllers
{
    [COEADGroupManagerAccess(Roles = "ADGMAdmin,ADGMPartner")]
    public class RequestLogController : Controller
    {

        //Initiate Entities 
        COEADGroupManagerDBRepository agmd = new COEADGroupManagerDBRepository();

        public ActionResult Index()
        {
            //Initiate View Model
            RequestLogIndexViewModel vmodel = new RequestLogIndexViewModel();

            return View(vmodel);
        }

        public JsonResult LoadObjects()
        {
            //Initiate List of Json Friendly Reporting Objects
            List<RequestLogEntryJsn> lRequestsJsn = new List<RequestLogEntryJsn>();

            //Pull List of Request from Database
            IQueryable<AGMMemberRequest> lRequests = agmd.Get_All_AGMMemberRequests();

            //Null\Empty Check on Requests
            if(lRequests != null && lRequests.Count() > 0)
            {
                foreach(AGMMemberRequest mbrRqst in lRequests)
                {
                    //Initiate Reporting Object
                    RequestLogEntryJsn rlej = new RequestLogEntryJsn();

                    if(mbrRqst.Pending == true)
                    {
                        rlej.pending = "Yes";
                    }
                    else
                    {
                        rlej.pending = "No";
                    }

                    //Check Action
                    if(!string.IsNullOrEmpty(mbrRqst.MRAction))
                    {
                        rlej.action = mbrRqst.MRAction;
                    }
                    
                    //Check AD Group
                    if(!string.IsNullOrEmpty(mbrRqst.ADGroupName))
                    {
                        rlej.group = mbrRqst.ADGroupName;
                    }
                    
                    //Check User ID
                    if(!string.IsNullOrEmpty(mbrRqst.KerbID))
                    {
                        rlej.user_id = mbrRqst.KerbID;
                    }

                    //Check Requestor
                    if(!string.IsNullOrEmpty(mbrRqst.SubmittedBy))
                    {
                        rlej.requestor = mbrRqst.SubmittedBy;
                    }

                    //Check Submitted On
                    if(mbrRqst.SubmittedOn.HasValue)
                    {
                        rlej.submitted_on = mbrRqst.SubmittedOn.Value.ToString("MM/dd/yyyy hh:mm tt");
                    }

                    lRequestsJsn.Add(rlej);

                }//End of lRequest Foreach

            }//End of lRequests Null\Empty Checks

            //Create Json Result and Set Max Json Length
            var jsonResult = Json(lRequestsJsn, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return (jsonResult);
        }
    }
}