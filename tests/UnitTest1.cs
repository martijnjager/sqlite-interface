using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Database.Contracts;
using Database.Tests.Models;

namespace Database.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var mockConnectionManager = new Mock<ITransactionManager>();
            var mockConnection = new Mock<IConnection>();
            var expectedValue = "expected value";
            var columnName = "TestColumn";

            // Setup mock behavior
            mockConnection.Setup(conn => conn.GetDatabasePath()).Returns("MockDatabasePath");
            mockConnectionManager.Setup(manager => manager.GetConnection()).Returns(mockConnection.Object);

            // Assuming InstanceContainer can be set up to use our mock
            InstanceContainer.Instance.SetSingleton(mockConnectionManager.Object, mockConnectionManager.Object.GetType().Name);

            var model = new TestModel();

            // Act
            var value = model.GetValue(columnName);

            // Assert
            Assert.Equals(expectedValue, value);
        }
    }
}
