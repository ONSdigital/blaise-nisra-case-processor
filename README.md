# Blaise_NISRA_Case_Processor

The Blaise NISRA Case Processor application is a Windows service that runs on a virtual machine hosting a Blaise 5 server.
The application works on a timer and preiodically checks for differences between imported NISRA Blaise data and the data stored on the Blaise server. Once any new data is found (data differing from that which is stored on the server), the application will apply a number of conflict resolution rules specified for dealing with differing data.

# Setup Development Environment

Clone the git respository to your IDE of choice. Visual Studio 2019 is recomended.

Populate the key values in the App.config file accordingly. **Never committ App.config with key values.**

Build the solution to obtain the necessary references.

# Installing the Service

  - Build the Solution
    - In Visual Studio select "Release" as the Solution Configuration
    - Select the "Build" menu
    - Select "Build Solution" from the "Build" menu
  - Copy the release files (/bin/release/) to the install location on the server
  - Uninstall any previous installs
    - Stop the service from running
    - Open a command prompt as administrator
    - Navigate to the windows service installer location
      - cd c:\Windows\Microsoft.NET\Framework\v4.0.30319\
    - Run installUtil.exe /U from this location and pass it the location of the service executable
      - InstallUtil.exe /U {install location}\BlaiseNISRACaseProcessor.exe
  - Run the installer against the release build
    - Open a command prompt as administrator
    - Navigate to the windows service installer location
      - cd c:\Windows\Microsoft.NET\Framework\v4.0.30319\
    - Run installUtil.exe from this location and pass it the location of the service executable
      - InstallUtil.exe {install location}\BlaiseNISRACaseProcessor.exe
    - Set the service to delayed start
    - Start the service
