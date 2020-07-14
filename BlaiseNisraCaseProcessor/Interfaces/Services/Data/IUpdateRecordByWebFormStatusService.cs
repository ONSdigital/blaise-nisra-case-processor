﻿using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Interfaces.Services.Data
{
    public interface IUpdateRecordByWebFormStatusService
    {
        void UpdateDataRecordViaWebFormStatus(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark,
            string surveyName, string serialNumber);
    }
}