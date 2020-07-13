using System.Collections.Generic;
using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Helpers;
using BlaiseNisraCaseProcessor.Mappers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Mappers
{
    public class CaseMapperTests
    {
        private Mock<IDataRecord> _recordMock;

        private readonly CaseMapper _sut;

        public CaseMapperTests()
        {
            _sut = new CaseMapper();
        }

        [SetUp]
        public void SetUpTests()
        {
            _recordMock = new Mock<IDataRecord>();

            var qidField1Mock = new Mock<IField>();
            qidField1Mock.Setup(f => f.LocalName).Returns("Field1Name");
            qidField1Mock.Setup(f => f.DataValue.ValueAsText).Returns("Field1Value");

            var qidField2Mock = new Mock<IField>();
            qidField2Mock.Setup(f => f.LocalName).Returns("Field2Name");
            qidField2Mock.Setup(f => f.DataValue.ValueAsText).Returns("Field2Value");

            var qidFieldCollection = new List<IField>
            {
                qidField1Mock.Object,
                qidField2Mock.Object
            };

            var fieldCollectionMock = new Mock<IFieldCollection>();
            fieldCollectionMock.Setup(f => f.GetEnumerator()).Returns(qidFieldCollection.GetEnumerator());

            _recordMock.Setup(r => r.GetField("QID").Fields).Returns(fieldCollectionMock.Object);
        }

        [Test]
        public void Given_I_Call_MapToSerializedJson_Then_I_Get_A_String_Returned()
        {
            //arrange
            const string instrumentName = "instrument1";
            const string serverPark = "serverPark1";
            const string primaryKey = "primaryKey";
            const CaseStatusType caseStatusType = CaseStatusType.NisraCaseImported;

            //act
            var result = _sut.MapToSerializedJson(_recordMock.Object, instrumentName, serverPark, primaryKey, caseStatusType);

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<string>(result);
        }

        [Test]
        public void Given_I_Call_MapToSerializedJson_Then_I_Get_The_Expected_String_Returned()
        {
            //arrange
            const string instrumentName = "instrument1";
            const string serverPark = "serverPark1";
            const string primaryKey = "primaryKey";
            const CaseStatusType caseStatusType = CaseStatusType.NisraCaseImported;


            //act
            var result = _sut.MapToSerializedJson(_recordMock.Object, instrumentName, serverPark, primaryKey, caseStatusType);

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"{""Field1Name"":""field1value"",""Field2Name"":""field2value"",""primary_key"":""primaryKey"",""instrument_name"":""instrument1"",""server_park"":""serverPark1"",""status"":""Case Completed""}", 
                result);
        }

        [Test]
        public void Given_I_Call_MapToSerializedJson_Then_Only_Qid_Fields_In_The_Data_Record_Should_Be_Mapped()
        {
            //arrange
            const string instrumentName = "instrument1";
            const string serverPark = "serverPark1";
            const string primaryKey = "primaryKey";
            const CaseStatusType caseStatusType = CaseStatusType.NisraCaseImported;

            //should never be mapped as not QID
            var nonQidField3Mock = new Mock<IField>();
            nonQidField3Mock.Setup(f => f.LocalName).Returns("ShouldNotBeMappedName");
            nonQidField3Mock.Setup(f => f.DataValue.ValueAsText).Returns("ShouldNotBeMappedValue");

            var nonQidFieldCollection = new List<IField>
            {
                nonQidField3Mock.Object
            };

            var nonQidFieldCollectionMock = new Mock<IFieldCollection>();
            nonQidFieldCollectionMock.Setup(f => f.GetEnumerator()).Returns(nonQidFieldCollection.GetEnumerator());

            _recordMock.Setup(r => r.GetField("NON-QID").Fields).Returns(nonQidFieldCollectionMock.Object);


            //act
            var result = _sut.MapToSerializedJson(_recordMock.Object, instrumentName, serverPark, primaryKey, caseStatusType);

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"{""Field1Name"":""field1value"",""Field2Name"":""field2value"",""primary_key"":""primaryKey"",""instrument_name"":""instrument1"",""server_park"":""serverPark1"",""status"":""Case Completed""}",
                result);
        }

        [Test]
        public void Given_I_Call_MapToSerializedJson_Then_I_Get_A_Json_String_Returned()
        {
            //arrange
            const string instrumentName = "instrument1";
            const string serverPark = "serverPark1";
            const string primaryKey = "primaryKey";
            const CaseStatusType caseStatusType = CaseStatusType.NisraCaseImported;


            //act
            var jsonDataString = _sut.MapToSerializedJson(
                null, instrumentName, serverPark, primaryKey, caseStatusType);

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonDataString);

            //assert
            Assert.AreEqual(EnumHelper.GetEnumDescription(caseStatusType), result["status"]);
            Assert.AreEqual(instrumentName, result["instrument_name"]);
            Assert.AreEqual(serverPark, result["server_park"]);
            Assert.AreEqual(primaryKey, result["primary_key"]);
        }
    }
}
