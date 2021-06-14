using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace COEADGroupManager
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("ADGMReportConfigureManager", "Report/ConfigureManager/{mgrid}", new { controller = "Report", action = "ConfigureManager", mgrid = UrlParameter.Optional });
            routes.MapRoute("ADGMReportSendGrpReports", "Report/SendGrpReports/{grpguid}", new { controller = "Report", action = "SendGrpReports", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMReportConfigure", "Report/Configure/{grpguid}", new { controller = "Report", action = "Configure", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMManagersDeleteManager", "Managers/DeleteManager/{mgrid}", new { controller = "Managers", action = "DeleteManager", mgrid = UrlParameter.Optional });
            routes.MapRoute("ADGMManagersManagedBy", "Managers/GroupsManagedBy/{mgrid}", new { controller = "Managers", action = "GroupsManagedBy", mgrid = UrlParameter.Optional });
            routes.MapRoute("ADGMManagersGroupList", "Managers/GroupList/{grpguid}", new { controller = "Managers", action = "GroupList", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMAddMembers", "Members/AddMembers/{grpguid}", new { controller = "Members", action = "AddMembers", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMGetMembers", "Members/GetMembers/{grpguid}", new { controller = "Members", action = "GetMembers", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMExportMembers", "Members/CurrentMembersExport/{grpguid}", new { controller = "Members", action = "CurrentMembersExport", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMBulkRemoveMembers", "Members/BulkRemove/{grpguid}", new { controller = "Members", action = "BulkRemove", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMMembers", "Members/List/{grpguid}", new { controller = "Members", action = "List", grpguid = UrlParameter.Optional });
            routes.MapRoute("ADGMNewGroup", "AddGroup/NewADGMGroup/{grpguid}", new { controller = "AddGroup", action = "NewADGMGroup", grpguid = UrlParameter.Optional });
            routes.MapRoute("RemoveGroupRequestFor", "RemoveGroup/RequestFor/{grpguid}", new { controller = "RemoveGroup", action = "RequestFor", grpguid = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
