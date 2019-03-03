using System;
using Moq;
using StackExchange.Redis;
using Xunit;
using SeriLog.Sinks.RedisStream;
using Serilog.Events;
using Serilog.Parsing;
using System.Collections.Generic;

namespace Serilog.Sinks.RedisStream.Test
{
    public class SinkTests
    {

        [Fact]
        public void SinkConfig_MessageFieldDefaultedIfNotSet()
        {
            var config = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = "localhost:3267",
                RedisStreamName = "ExampleApp"
            };

            Assert.Equal("message", config.RedisStreamMessageField);
        }
        [Fact]
        public void SinkConfig_AutoPropertiesSetCorrectly()
        {
            var config = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = "a",
                RedisStreamName = "b",
                RedisStreamMessageField = "c"
            };

            Assert.Equal("a", config.RedisConnectionString);
            Assert.Equal("b", config.RedisStreamName);
            Assert.Equal("c", config.RedisStreamMessageField);
        }
        [Fact]
        public void RedisConnection_ConfigurationMissing()
        {
            var sink = new RedisStreamSink(null, null);
            Assert.Throws<NullReferenceException>(() => sink.ConnectToRedis());
        }
         [Fact]
        public void RedisConnection_ConfigurationConnectionMissing()
        {
            var config1 = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = null
            };
            var sink1 = new RedisStreamSink(config1, null);
            Assert.Throws<NullReferenceException>(() => sink1.ConnectToRedis());

            var config2 = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = ""
            };
            var sink2 = new RedisStreamSink(config2, null);
            Assert.Throws<NullReferenceException>(() => sink2.ConnectToRedis());
        }
        /*        [Fact]
                public void Logging_ConfigurationElementsMissing()
                {
                    var mockDatabase = BuildDatabase("SUCCESS_ID");
                    var mockMultiplexer = BuildSuccessConnectionMultiplexer(mockDatabase);
                    var mockAppender = BuildSuccessAppender(mockMultiplexer, errorHandler);
                    mockAppender.SetupProperty(_ => _.RedisConnectionString, null);
                    mockAppender.SetupProperty(_ => _.RedisStreamName, null);

                    var loggingEvent = new LoggingEvent(typeof(AppenderTests), null, "LoggerName", Level.Info, "Example of a Redis Stream logging entry", null);

                    mockAppender.Object.DoAppend(loggingEvent);

                    Assert.NotNull(errorHandler.LogException);
                    Assert.IsAssignableFrom<InvalidOperationException>(errorHandler.LogException);
                    Assert.Equal("Logging configuration elements are not correctly set.", errorHandler.Message);
                }
        */
        [Fact]
        public void SuccessfulLogger()
        {
            var config = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = "a",
                RedisStreamName = "b",
                RedisStreamMessageField = "c"
            };

            string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            IFormatProvider formatProvider = null;
            var formatter = new Serilog.Formatting.Display.MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var template = new MessageTemplateParser().Parse("{TestToken} is good");
            var properties = new List<LogEventProperty> { new LogEventProperty("TestToken", new ScalarValue("Ice Cream")) };

            var mockDatabase = BuildDatabase("SUCCESS_ID");
            var mockMultiplexer = BuildSuccessConnectionMultiplexer(mockDatabase);
            var mockSink = new Mock<RedisStreamSink>(config, formatter) { CallBase = true };
            mockSink.Setup(_ => _.ConnectToRedis())
                .Returns(mockMultiplexer.Object);

            var loggingEvent = new LogEvent(DateTimeOffset.Now,
                                            LogEventLevel.Information,
                                            null,
                                            template,
                                            properties);

            mockSink.Object.Emit(loggingEvent);

            Assert.True(true);
        }

        /*
                [Fact]
                public void FailedToLogToRedis_IncorrectReturnMessageId()
                {
                    var errorHandler = new Log4NetErrorHandler();
                    var mockDatabase = BuildDatabase(RedisValue.Null);
                    var mockMultiplexer = BuildSuccessConnectionMultiplexer(mockDatabase);
                    var mockAppender = BuildSuccessAppender(mockMultiplexer, errorHandler);

                    var loggingEvent = new LoggingEvent(typeof(AppenderTests), null, "LoggerName", Level.Info, "Example of a Redis Stream logging entry", null);

                    mockAppender.Object.DoAppend(loggingEvent);

                    Assert.NotNull(errorHandler.LogException);
                    Assert.IsAssignableFrom<RedisException>(errorHandler.LogException);
                }
        */
        /*  [Fact]
          public void FailedToLogToRedis_FailedConnectionToRedis()
          {
              var errorHandler = new Log4NetErrorHandler();
              var mockMultiplexer = BuildFailingConnectionMultiplexer();
              var mockAppender = BuildSuccessAppender(mockMultiplexer, errorHandler);

              var loggingEvent = new LoggingEvent(typeof(AppenderTests), null, "LoggerName", Level.Info, "Example of a Redis Stream logging entry", null);

              mockAppender.Object.DoAppend(loggingEvent);

              Assert.NotNull(errorHandler.LogException);
              Assert.IsAssignableFrom<RedisConnectionException>(errorHandler.LogException);
          }
  */

        /*
                private Mock<RedisStreamSink> BuildSuccessAppender(Mock<IConnectionMultiplexer> mockMultiplexer)
                {

                    var config = new RedisStreamSinkConfiguration
                    {
                        RedisConnectionString = "a",
                        RedisStreamName = "b",
                        RedisStreamMessageField = "c"
                    };

                    var mockSink = new Mock<RedisStreamSink>(config) { CallBase = true };
                    mockSink.Setup(_ => _.ConnectToRedis())
                        .Returns(mockMultiplexer.Object);

                    mockSink.Object.Threshold = Level.Info;
                    mockSink.Object.ErrorHandler = errorHandler;
                    mockSink.Object.Layout = new PatternLayout("%date [%thread] %-5level %logger [%property{NDC}] - %message%newline");

                    return mockSink;
                }
        */
        private Mock<IDatabase> BuildDatabase(RedisValue returnValue)
        {
            var mockDatabase = new Mock<IDatabase>();
            mockDatabase.Setup(_ => _.StreamAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CommandFlags>()))
                        .Returns(System.Threading.Tasks.Task.FromResult<RedisValue>(returnValue));

            return mockDatabase;
        }
        private Mock<IConnectionMultiplexer> BuildSuccessConnectionMultiplexer(Mock<IDatabase> mockDatabase)
        {
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);
            mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                            .Returns(mockDatabase.Object);

            return mockMultiplexer;
        }
        private Mock<IConnectionMultiplexer> BuildFailingConnectionMultiplexer()
        {
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);
            mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Failed"));


            return mockMultiplexer;
        }
    }
}
