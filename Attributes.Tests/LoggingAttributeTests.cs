using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NLog;
using Xunit;

namespace AOP.Attributes.Tests
{
    public class LoggingAttributeTests
    {
        #region Tests ---------------------------------------------------------

        [Fact]
        public void LoggingAttribute_Constructor_Should_Create_Instance()
        {
            // Arrange

            // Act
            var instance = new LoggingAttribute();

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<LoggingAttribute>();
        }

        #endregion

        #region Helper --------------------------------------------------------

        [Logging]
        private class ClassUnderTest
        {
            public void MethodToBeTested() { }

            public void MethodWith2ArgumentsToBeTested(int firstArgument, string secondArgument) { }

            public int MethodWithReturnValueToBeTested()
            {
                return 42;
            }

            public void NestedMethodToBeTested1()
            {
                NestedMethodToBeTested2();
            }
            public void NestedMethodToBeTested2()
            {
                NestedMethodToBeTested3();
            }

            public void NestedMethodToBeTested3()
            {
                NestedMethodToBeTested4();
            }

            public void NestedMethodToBeTested4()
            {

            }

            public void MethodThatThrowsExceptionToBeTested()
            {
                throw new Exception("UnitTestException");
            }

            public Task<int> AsyncMethodToBeTested()
            {
                return Task.FromResult(42);
            }

            public Task AsyncMethodWithoutReturnValueToBeTested()
            {
                return Task.Delay(100);
            }

            public Task AsyncMethodThatThrowsExceptionToBeTested()
            {
                throw new Exception("UnitTestException");
            }
        }

        private void SetLoggingInterfaceInAttributeOfClassUnderTest(ILogger logger)
        {
            typeof(LoggingAttribute)
                .GetField("Logger", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, logger);
        }

        private Mock<ILogger> GetMockedLogger()
        {
            var mockedLogger = new Mock<ILogger>();

            mockedLogger.Setup(m => m.Fatal(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Error(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Warn(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Info(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Debug(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Trace(It.IsAny<string>())).Callback<string>(Console.WriteLine);

            return mockedLogger;
        }

        #endregion
    }
}
