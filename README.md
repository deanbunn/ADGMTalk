## Active Directory Group Management Talk

A discussion about the development of a tool that allows users to manage the membership of assigned AD groups.

### Background

Our unit started heavily using AD groups for access to cloud services, workstations, facilities, labs, licensed software, and email list membership. Historically, users would submit support tickets for AD group membership changes.   

### Requirements

- Allow users to change direct membership of a group
- Not allows users to remove nested AD groups or child domain users
- Bulk adds and removes
- Reporting for users that sends reminders of groups managed and their membership
- Users can receive individual report or summary of all groups managed
- Provisioning completion notices to users and all managers of AD group
- Limit group membership count. Including nested members
- Delegated admin access for department admins based up AD group DN

### Configuration

- \.NET MVC website for submitting requests and configuring automation options
- \.NET console application for processing requests and other automated tasks
- MSSQL database for storing application data
- Frontend application pool runs under a regular AD account\. No access to modify AD group membership

### Features

#### Roles

The application is based upon three levels of access:

- Admin
  - Add, remove, and configure all groups
  - Add, remove, and configure all managers
  - View membership request log
- Department Admin
  - Add, remove, and configure all groups under their associated OU assignments\. Based upon match with DN partials of the requested group\'s DN 
  - Add, remove, and configure all managers
  - View membership request log
- Manager
  - Add and remove members from directly assigned AD groups

#### Admin View

Both admin and department admins will be presented a list of all the AD groups in the application. This view allows admins to change a group's membership, manager assignments, configure the group's app settings, and if necessary remove the group. 

![Admin Group Listing](Images/adgm_01.JPG)

#### Manager View

Managers only see a list of directly assigned groups they can manage. 

![Manager Group Listing](Images/adgm_02.JPG)

#### Changing Membership 

When viewing a group's membership, managers can see current members and any pending requests. Requests are processed by a backend process that runs every few minutes. 

![Changing Membership](Images/adgm_03.JPG)

#### Adding Members

Add members feature should allow managers to submit a list of new members. We chose to lookup users by user ID or email address \(or a mixture of both\)   

![Adding Members](Images/adgm_04.JPG)

#### Directory Search

Managers might not know the user ID or email address of the member, so we added a directory search feature.

![Directory Search](Images/adgm_12.JPG)

#### Bulk Removes

Besides having a way to remove an individual user, managers quickly requested the ability to remove all members, or numerous users, in one submission.

![Bulk Removes](Images/adgm_05.JPG)

#### Membership Reports and Change Notifications











