using System;
using System.Configuration;
using System.IO.Abstractions;
using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Mappers;
using BlaiseNisraCaseProcessor.MessageHandler;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Unity;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class UnityProvider
    {
        private readonly UnityContainer _unityContainer;

        public UnityProvider()
        {
            _unityContainer = new UnityContainer();

            //blaise services
            _unityContainer.RegisterType<IFluentBlaiseApi, FluentBlaiseApi>();

            //system abstractions
            _unityContainer.RegisterType<IFileSystem, FileSystem>();

            //providers
            _unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            _unityContainer.RegisterType<IStorageClientProvider, LocalStorageClientProvider>();
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);
#else
            // When running in Release, the service will be running as compute account which will have access to all buckets.
            unityContainer.RegisterType<IStorageClientProvider, StorageClientProvider>();
#endif

            _unityContainer.RegisterSingleton<IFluentQueueApi, FluentQueueApi>();
            _unityContainer.RegisterFactory<ILog>(f => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));

            //mappers
            _unityContainer.RegisterType<ICaseMapper, CaseMapper>();

            //handlers
            _unityContainer.RegisterType<IMessageHandler, NisraMessageHandler>();

            //services   
            _unityContainer.RegisterType<IPublishCaseStatusService, PublishCaseStatusService>();
            _unityContainer.RegisterType<IBlaiseApiService, BlaiseApiService>();
            _unityContainer.RegisterType<IUpdateCaseByHoutService, UpdateCaseByHoutService>();
            _unityContainer.RegisterType<IUpdateCaseServiceService, UpdateCaseService>();
            _unityContainer.RegisterType<ICloudStorageService, CloudStorageService>();
            _unityContainer.RegisterType<IImportCasesService, ImportCasesService>();
            _unityContainer.RegisterType<IProcessFilesService, ProcessFilesService>();

            //queue service
            _unityContainer.RegisterType<IQueueService, QueueService>();

            //main service
            _unityContainer.RegisterType<IInitialiseService, InitialiseService>();
        }

        public T Resolve<T>()
        {
            return _unityContainer.Resolve<T>();
        }
    }
}
