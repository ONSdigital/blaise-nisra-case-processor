# Blaise_NISRA_Case_Processor

Blaise NISRA Case Processor is a Windows service for processing the NISRA Blaise case data. The service is triggered by listening for messages on 
the 'process-nisra-case-topic' topic in PubSub in GCP., and compares the NISRA data with the data on our servers, rules are applied to use the best data available.

# Setup Development Environment

Clone the git repository to your IDE of choice. Visual Studio 2019 is recommended.

Populate the key values in the App.config file accordingly. **Never commit App.config with key values.**

Build the solution to obtain the necessary references.

#Topics & Subscriptions
This service needs to listen to messages put on the 'process-nisra-case-topic'. It does this by subscribing to a subscription called 'process-nisra-case-subscription'.

    
# Example message
```

{"action": "process"}

```

#debugging
Due to the nature of the GCP pubsub implementation, it will be listening on a worker thread. If you wish to debug the service locally you will
need to add a Thread.Sleep(n seconds) just after the subscription is setup to push the service to use the worker thread in the 'InitialiseSservice' and set a breakpoint. If a breakpoint is not set,
the service will just drop out as pubsub works off a streaming pull mechanism on background worker threads and not events.


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

    - for LocalProcessFolder : so as per the config value - e.g. C:\Blaise_NISRA_Proc

# Storage Settings

When running locally edit the code shown under `if DEBUG` in `BlaiseNISRACaseProcessor.cs`. When running locally, reference bucket credentials file. When running in VM the service will run as comupte account and will not a refernce to the credentials file.
  
    
    
