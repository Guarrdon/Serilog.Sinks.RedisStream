using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;

namespace SeriLog.Sinks.RedisStream
{
    public static class RedisStreamSinkExtensions
    {
       private const string DefaultOutputTemplate ="[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public static LoggerConfiguration RedisStreamSink(
            this LoggerSinkConfiguration loggerConfiguration,
            RedisStreamSinkConfiguration redisStreamSinkConfiguration,
            string outputTemplate = DefaultOutputTemplate, 
            IFormatProvider formatProvider = null)
        {
            var formatter = new Serilog.Formatting.Display.MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return loggerConfiguration.Sink(new RedisStreamSink(redisStreamSinkConfiguration, formatter));
        }
    }
}
