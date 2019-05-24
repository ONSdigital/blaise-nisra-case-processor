# Blaise_NISRA_Case_Processor

The Blaise NISRA Case Processor application is a Windows service that runs on a virtual machine hosting a Blaise 5 server.
The application works on a timer and preiodically checks for differences between imported NISRA Blaise data and the data stored on the Blaise server. Once any new data is found (data differing from that which is stored on the server), the application will apply a number of conflict resolution rules specified for dealing with differing data.

# Populate the key values in the app.config file.

The following keys need to be added to the App.config file for the program to work correctly.
Fill in the associated values as required.

    <add key="BlaiseServerHostName" value=""/>
    <add key="BlaiseServerUserName" value=""/>
    <add key="BlaiseServerPassword" value=""/>
    

# Install the Log4Net & SQLite packages via NuGet.

  ```
  Install-Package log4net
  Install-Package System.Data.SQLite
  ```

# Blaise API
Ensure you have the latest version of Blaise 5 installed from the Statistics Netherlands FTP.

To use the API's:
  - Right Click the "References" object under the project in the VS Solution Explorer
  - Select "Add Reference"
  - Use the "Browse" tab and navigate to "C:\Program Files (x86)\StatNeth\Blaise5\Bin"
  - In this folder all the required API's for Blaise interaction are available.
  - Install the following Blaise APIs:
    - StatNeth.Blaise.API.DataInterface
    - StatNeth.Blaise.API.ServerManager
    - StatNeth.Blaise.API.DataLink
    - StatNeth.Blaise.API.DataRecord

# Installing the Service

  - Build the Solution 
    - In Visual Studio select "Release" as the Solution Confiiguration.
    - Select "Build" from the toolbar.
    - Select "Build Solution" from the "Build" dropdown
  - Copy the release files to the program install location of the target machine     
  - Run the installer against the release build
    - Open command prompt as administrator
    - Navigate to the windows service installer (cd c:\Windows\Microsoft.NET\Framework\v4.0.30319)
    - Run the installUtil.exe from the Windows folder accessed above using the service's exe from the release directory
      - InstallUtil.exe #Program install location on target machine#\BlaiseNISRACaseProcessor.exe
