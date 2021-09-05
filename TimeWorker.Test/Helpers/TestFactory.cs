using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TimeWorker.Commom.Models;
using TimeWorker.Functions.Entities;

namespace TimeWorker.Test.Helpers
{
    public class TestFactory
    {
        public static TimeWorkerEntity GetEntity()
        {
            return new TimeWorkerEntity
            {
                ETag = "*",
                PartitionKey = "TimeWorker",
                RowKey = DateTime.UtcNow.ToString().Replace("/", "-"),
                Id = int.Parse(Guid.NewGuid().ToString()),
                CreatedTime = DateTime.UtcNow,
                Type = "0",
                Consolidated = false
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(int Id, Timeworker Request)
        {
            string request = JsonConvert.SerializeObject(Request);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{Id}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(int Id)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{Id}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Timeworker Request)
        {
            string request = JsonConvert.SerializeObject(Request);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
            
        }

        public static Timeworker GetRequest()
        {
            return new Timeworker
            {
                Id = int.Parse(Guid.NewGuid().ToString()),
                CreatedTime = DateTime.UtcNow,
                Type = "0",
                Consolidated = false
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }


    }
}
