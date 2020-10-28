using System.Collections.Generic;
using Blaise.Nisra.Case.Processor.Enums;
using Blaise.Nisra.Case.Processor.Mappers;
using Blaise.Nisra.Case.Processor.Models;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Nisra.Case.Processor.Tests.Unit.Mappers
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
        public void Given_I_Call_MapFieldDictionaryFromRecordFields_Then_I_Get_The_Expected_Dictionary_Back()
        {
            //arrange
            var field1Name = "QID.Field1Name";
            var field1Value = "1";
            var iField1Mock = new Mock<IField>();
            iField1Mock.Setup(f => f.FullName).Returns(field1Name);
            iField1Mock.Setup(f => f.DataValue.ValueAsText).Returns(field1Value);

            var field2Name = "QID.Field2Name";
            var field2Value = "2";
            var iField2Mock = new Mock<IField>();
            iField2Mock.Setup(f => f.FullName).Returns(field2Name);
            iField2Mock.Setup(f => f.DataValue.ValueAsText).Returns(field2Value);

            var dataRecordMock = new Mock<IDataRecord2>();
            dataRecordMock.Setup(d => d.GetDataFields())
                .Returns(new List<IField> {iField1Mock.Object, iField2Mock.Object});

            //act
            var result = _sut.MapFieldDictionaryFromRecordFields(dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Dictionary<string, string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(field1Name));
            Assert.AreEqual(field1Value, result[field1Name]);
            Assert.IsTrue(result.ContainsKey(field2Name));
            Assert.AreEqual(field2Value, result[field2Name]);
        }

        [Test]
        public void Given_A_Message_With_A_Process_Action_When_I_Call_MapToNisraCaseActionModel_Then_I_Get_An_Expected_Model_Back()
        {
            //arrange
            const string message =
                @"{ ""ACTION"":""process""}";

            //act
            var result = _sut.MapToNisraCaseActionModel(message);

            //assert`
            Assert.NotNull(result);
            Assert.IsInstanceOf<NisraCaseActionModel>(result);
            Assert.AreEqual(ActionType.Process, result.Action);
        }

        [Test]
        public void Given_An_Invalid_Message_With_A_Process_Action_When_I_Call_MapToNisraCaseActionModel_Then_I_Get_A_Default_Model_Back()
        {
            //arrange
            const string message =
                @"{ ""ACTION"":""none""}";

            //act
            var result = _sut.MapToNisraCaseActionModel(message);

            //assert`
            Assert.NotNull(result);
            Assert.IsInstanceOf<NisraCaseActionModel>(result);
            Assert.AreEqual(ActionType.NotSupported, result.Action);
        }
    }
}
