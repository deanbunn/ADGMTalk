## Active Directory Group Management Talk

### MVC Website

The CodeSnipets/MVCWebsite folder contains the code snipets for the ADGM C\# MVC website.

- App\_Start 
  - RouteConfig\.cs has the custom routing for the site URLs
- Controllers
  - HomeController\.cs the main page that displays the group lists
  - ManagersController\.cs manager listing and changes
  - MembersController\.cs membership listings and changes
- Models
  - ADGMGrpLimitWrkr\.cs utility to check group membership count
  - ADGMGrpVerifier\.cs searches AD for group and verifies correct group settings
  - COERoleToOUGroupAccess\.cs determines if user has access to ADGM group
- RoleProvider
  - COEADGroupManagerRoleProvider\.cs custom role provider to determine user primary role
- Views



