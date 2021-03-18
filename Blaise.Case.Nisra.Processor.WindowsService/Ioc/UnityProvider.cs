using System;
using System.Configuration;
using System.IO.Abstractions;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.CloudStorage.Providers;
using Blaise.Case.Nisra.Processor.CloudStorage.Services;
using Blaise.Case.Nisra.Processor.Core.Configuration;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Services;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Services;
using Blaise.Case.Nisra.Processor.MessageBroker.Handler;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Mappers;
using Blaise.Case.Nisra.Processor.MessageBroker.Services;
using Blaise.Case.Nisra.Processor.WindowsService.Interfaces;
using Blaise.Nuget.Api.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using Unity;

namespace Blaise.Case.Nisra.Processor.WindowsService.Ioc
{
    public class UnityProvider
    {
        private readonly IUnityContainer _unityContainer;

        public UnityProvider()
        {
            _unityContainer = new UnityContainer();

            //blaise services
            _unityContainer.RegisterType<IBlaiseCaseApi, BlaiseCaseApi>();

            //logging
            _unityContainer.RegisterType<ILoggingService, EventLogging>();

            //system abstractions
            _unityContainer.RegisterType<IFileSystem, FileSystem>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            // When running in Release, the service will be running as compute account which will have access to all buckets. In test we need to get credentials
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);

#endif
            //configuration
            _unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();

            //pub sub message broker
            _unityContainer.RegisterSingleton<IFluentQueueApi, FluentQueueApi>();
            _unityContainer.RegisterType<IMessageBrokerService, MessageBrokerService>();
            _unityContainer.RegisterType<IMessageModelMapper, MessageModelMapper>();
            _unityContainer.RegisterType<IMessageHandler, MessageHandler>();

            //cloud storage
            _unityContainer.RegisterType<ICloudStorageClientProvider, CloudStorageClientProvider>();
            _unityContainer.RegisterType<IStorageService, StorageService>();

            //core services   
            _unityContainer.RegisterType<IImportNisraDataFileService, ImportNisraDataFileService>();
            _unityContainer.RegisterType<ICatiDataService, CatiDataService>();
            _unityContainer.RegisterType<IImportNisraCaseService, ImportNisraCaseService>();

            //main windows service
            _unityContainer.RegisterType<IInitialiseWindowsService, InitialiseWindowsService>();
        }

        public T Resolve<T>()
        {
            return _unityContainer.Resolve<T>();
        }
    }
}
