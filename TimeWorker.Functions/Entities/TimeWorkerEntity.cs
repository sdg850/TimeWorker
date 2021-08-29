using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorker.Functions.Entities
{
    public class TimeWorkerEntity
    {
        public int Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
