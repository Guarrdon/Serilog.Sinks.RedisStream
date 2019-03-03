using System;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using SeriLog.Sinks.RedisStream;

namespace SeriLog.Sinks.RedisStream.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            var config = new RedisStreamSinkConfiguration
            {
                RedisConnectionString = "10.10.1.99:32769",
                RedisStreamName = "ExampleApp"
            };
            Console.WriteLine("Logging configuration set.");
            Console.WriteLine(JsonConvert.SerializeObject(config));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty("Version", "0.9.1")
                .WriteTo.RedisStreamSink(config)
                .CreateLogger();
            Console.WriteLine("Logger created.");

            Log.Information("Completed log test to Redis Streams.");
            Console.WriteLine("Log entry written.");
        }
    }
}
