using System;
using Serilog;
using Serilog.Events;
using SeriLog.Sinks.RedisStream;

namespace SeriLog.Sinks.RedisStream.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new RedisStreamSinkConfiguration{
                RedisConnectionString="10.10.1.99:32769",
                RedisStreamName = "ExampleApp"
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty("Version", "0.9.1")
                .WriteTo.RedisStreamSink(config, "{Timestamp:HH:mm} [{Level}] ({ThreadId}) {Message}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Completed log test to Redis Streams.");
        }
    }
}
