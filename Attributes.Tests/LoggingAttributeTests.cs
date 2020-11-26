using System;
using System.Reflection;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NLog;
using Xunit;
using Xunit.Abstractions;

namespace AOP.Attributes.Tests
{
    public class LoggingAttributeTestsSetupCode : IDisposable
    {
        public ITestOutputHelper Output { get; set; }

        public Mock<ILogger> MockedLogger { get; set; }

        public LoggingAttributeTestsSetupCode()
        {
            // setup code
            MockedLogger = GetMockedLogger();

            typeof(LoggingAttribute)
                .GetField("Logger", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, MockedLogger.Object);
        }

        public void Dispose()
        {
            // clean-up code
        }

        private Mock<ILogger> GetMockedLogger()
        {
            var mockedLogger = new Mock<ILogger>();

            mockedLogger.Setup(m => m.Fatal(It.IsAny<string>())).Callback<string>(WriteLine);
            mockedLogger.Setup(m => m.Error(It.IsAny<string>())).Callback<string>(WriteLine);
            mockedLogger.Setup(m => m.Warn(It.IsAny<string>())).Callback<string>(WriteLine);
            mockedLogger.Setup(m => m.Info(It.IsAny<string>())).Callback<string>(WriteLine);
            mockedLogger.Setup(m => m.Debug(It.IsAny<string>())).Callback<string>(WriteLine);
            mockedLogger.Setup(m => m.Trace(It.IsAny<string>())).Callback<string>(WriteLine);

            mockedLogger.SetupGet(p => p.IsFatalEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsErrorEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsWarnEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsInfoEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsDebugEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsTraceEnabled).Returns(true);

            return mockedLogger;
        }

        private void WriteLine(string s)
        {
            Output?.WriteLine(s);
        }
    }

    public class LoggingAttributeTests : IClassFixture<LoggingAttributeTestsSetupCode>
    {
        private readonly Mock<ILogger> _mockedLogger;
        private readonly ClassUnderTest _classUnderTest;
        private readonly ITestOutputHelper _output;

        private Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public LoggingAttributeTests(LoggingAttributeTestsSetupCode fixture, ITestOutputHelper output)
        {
            fixture.Output = output;
            _mockedLogger = fixture.MockedLogger;
            _output = output;
            _mockedLogger.Invocations.Clear();
            _classUnderTest = new ClassUnderTest();
        }

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

        [Fact]
        public void Method_Entry_And_Exit_Should_Be_Logged()
        {
            // Arrange 
            // Act
            _classUnderTest.MethodToBeTested();

            _output.WriteLine("Harald");
            // Assert
            _mockedLogger.Verify(m => m.Fatal(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Error(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Warn(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Info(It.Is<string>(s =>
                s.Equals("Init: AOP.Attributes.Tests.LoggingAttributeTests+ClassUnderTest.MethodToBeTested [0] params"))), Times.Once);
            _mockedLogger.Verify(m => m.Info(It.Is<string>(s => s.Equals("Exit: []"))), Times.Once);
            _mockedLogger.Verify(m => m.Debug(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Trace(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Method_Arguments_Should_Be_Logged()
        {
            // Arrange 
            // Act
            _classUnderTest.MethodWith2ArgumentsToBeTested(42, "UnitTest");

            // Assert
            _mockedLogger.Verify(m => m.Fatal(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Error(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Warn(It.IsAny<string>()), Times.Never);
            _mockedLogger.Verify(m => m.Info(It.Is<string>(s =>
                s.Equals("Init: AOP.Attributes.Tests.LoggingAttributeTests+ClassUnderTest.MethodWith2ArgumentsToBeTested [2] params"))), Times.Once);
            _mockedLogger.Verify(m => m.Debug(It.IsAny<string>()), Times.Exactly(2));
            _mockedLogger.Verify(m => m.Debug(It.Is<string>(s => s.Equals("firstArgument: 42"))), Times.Once);
            _mockedLogger.Verify(m => m.Debug(It.Is<string>(s => s.Equals("secondArgument: UnitTest"))), Times.Once);
            _mockedLogger.Verify(m => m.Info(It.Is<string>(s => s.Equals("Exit: []"))), Times.Once);
            _mockedLogger.Verify(m => m.Trace(It.IsAny<string>()), Times.Never);
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

        private Mock<ILogger> GetMockedLogger()
        {
            var mockedLogger = new Mock<ILogger>();

            mockedLogger.Setup(m => m.Fatal(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Error(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Warn(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Info(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Debug(It.IsAny<string>())).Callback<string>(Console.WriteLine);
            mockedLogger.Setup(m => m.Trace(It.IsAny<string>())).Callback<string>(Console.WriteLine);

            mockedLogger.SetupGet(p => p.IsFatalEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsErrorEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsWarnEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsInfoEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsDebugEnabled).Returns(true);
            mockedLogger.SetupGet(p => p.IsTraceEnabled).Returns(true);

            return mockedLogger;
        }

        #endregion
    }
}
