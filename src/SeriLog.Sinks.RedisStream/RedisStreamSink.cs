using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using StackExchange.Redis;
using Serilog.Formatting;
using System.IO;

namespace SeriLog.Sinks.RedisStream
{
    public class RedisStreamSink : ILogEventSink
    {

        private readonly ITextFormatter _formatter;
        private readonly RedisStreamSinkConfiguration _configuration;

        protected ConnectionMultiplexer RedisConnection { get; set; }

        public RedisStreamSink(RedisStreamSinkConfiguration config, ITextFormatter textFormatter)
        {
            _formatter = textFormatter;
            _configuration = config;
        }

        public virtual IConnectionMultiplexer ConnectToRedis()
        {
            if (_configuration==null)
                throw new NullReferenceException("Serilog configuration for logger must be configured.  Currently null.");

            if (string.IsNullOrEmpty(_configuration.RedisConnectionString))
                throw new NullReferenceException("Connection string required to connect to Redis");

            if (RedisConnection == null)
                RedisConnection = ConnectionMultiplexer.Connect(_configuration.RedisConnectionString);

            return RedisConnection;
        }


        public async void Emit(LogEvent logEvent)
        {

            //check configurations
            if (string.IsNullOrEmpty(_configuration.RedisConnectionString) || string.IsNullOrEmpty(_configuration.RedisStreamName))
                throw new InvalidOperationException("RedisConnectionString and RedisStreamName configuration elements are required.");

            //connect to Redis
            using (var connection = ConnectToRedis())
            {
                //get Redis database
                var db = connection.GetDatabase();

                string message = default(string);
                //format message per formatProvider
                using (var writer = new StringWriter())
                {
                    _formatter.Format(logEvent, writer);
                    message = writer.ToString();
                }

                //add log message to stream
                var messageId = await db.StreamAddAsync(_configuration.RedisStreamName, _configuration.RedisStreamMessageField, message, null, null, false, CommandFlags.None);

                //check for message failure
                if (messageId == RedisValue.Null || ((string)messageId).Length == 0)
                    throw new RedisException("The message failed to log to a Redis Stream.  Return message was either null or empty.");
            }

        }
    }
}
