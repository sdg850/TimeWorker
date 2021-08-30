using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorker.Functions.Entities
{
    public class TimeWorkerEntity : TableEntity
    {
        public Int32 Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
