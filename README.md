# Blaise_NISRA_Case_Processor

Blaise NISRA Case Processor is a Windows service for processing the NISRA Blaise case data. The service runs preiodically on a timer and compares the NISRA data with the data on our servers, rules are applied to use the best data available.

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

# Create folders on the VM where the service is installed:

    - for NisraProcessFolder : so as per the config value - e.g. C:\Blaise_NISRA_Proc

# Storage Settings

When running locally edit the code shown under `if DEBUG` in `BlaiseNISRACaseProcessor.cs`. When running locally, reference bucket credentials file. When running in VM the service will run as comupte account and will not a refernce to the credentials file.
  
    
    
