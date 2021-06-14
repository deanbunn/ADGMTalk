## Active Directory Group Management Talk

### Console App

The CodeSnipets/ConsoleApp folder contains the code snipets for the ADGM C\# console application. The app is designed to be run as a scheduled task that executes every five minutes. 

- Program\.cs
  - Provision\_Pending\_Requests function for processing membership change requests
  - Check\_Managers\_UCD\_Account\_Status function for checking the Campus account status of managers
  - Update\_AGMGroups\_AD\_Info updates common name and distinguished name values for groups in the local database
  - Send\_All\_Groups\_Reports\_For\_Managers function sends group membership reports


