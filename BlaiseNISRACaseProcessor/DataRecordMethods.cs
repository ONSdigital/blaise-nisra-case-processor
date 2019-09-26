using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNISRACaseProcessor
{
    public static class DataRecordMethods
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Assign value to field if it exists in the datarecord
        /// </summary>
        public static IDataRecord AssignValueIfFieldExists(IDataRecord dataRecord, string field, string assignValue)
        {
            if (dataRecord.Keys.Contains(field))
            {
                log.Debug($"Assiging value: {assignValue} to field: {field} in dataRecord: {dataRecord}");
                var recordField = dataRecord.GetField(field);
                recordField.DataValue.Assign(assignValue);
            }

            log.Debug($"Cannot assign Assiging value: {assignValue} to field: {field} in dataRecord: {dataRecord}, as Field does not exists");
            return dataRecord;
        }

        /// <summary>
        /// Checks passed in Blaise record for the passed in field name.
        /// </summary>
        public static bool CheckForField(IDataRecord dataRecord, string fieldName)
        {
            if (dataRecord != null)
            {
                IDataRecord2 dataRecord2 = (IDataRecord2)dataRecord;
                foreach (IField3 field in dataRecord2.GetDataFields())
                {
                    if (field.FullName == fieldName)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Get Field from data record if it exists otherwise return null 
        /// </summary>
        public static IField GetFieldIfExists(IDataRecord dataRecord, string fieldName)
        {
            if (dataRecord.Keys.Contains(fieldName))
            {
                log.Debug($"Getting Value for field: {fieldName} from dataRecord: {dataRecord}");
                return dataRecord.GetField(fieldName);
            }

            log.Warn($"Field: {fieldName} does not exist in dataRecord: {dataRecord}");
            return null;
        }

    }
}
