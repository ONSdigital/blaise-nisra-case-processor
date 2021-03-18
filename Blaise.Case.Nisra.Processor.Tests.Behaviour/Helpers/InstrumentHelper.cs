using Blaise.Nuget.Api.Api;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class InstrumentHelper
    {
        private readonly IBlaiseSurveyApi _blaiseSurveyApi;

        private static InstrumentHelper _currentInstance;

        public InstrumentHelper()
        {
            _blaiseSurveyApi = new BlaiseSurveyApi();
        }

        public static InstrumentHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new InstrumentHelper());
        }

        public void InstallSurvey()
        {
            _blaiseSurveyApi.InstallSurvey(
                BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName,
                BlaiseConfigurationHelper.InstrumentPackage,
                SurveyInterviewType.Cati);
        }

        public void UninstallSurvey()
        {
            _blaiseSurveyApi.UninstallSurvey(
                BlaiseConfigurationHelper.InstrumentName,
                BlaiseConfigurationHelper.ServerParkName);
        }
    }
}
