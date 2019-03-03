using Serilog.Configuration;

namespace SeriLog.Sinks.RedisStream
{
    public class RedisStreamSinkConfiguration
    {


        public string RedisConnectionString { get; set; }
        public string RedisStreamName { get; set; }
        public string RedisStreamMessageField { get; set; } = "message";
    }
}