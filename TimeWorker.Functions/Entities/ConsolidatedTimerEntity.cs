using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorker.Functions.Entities
{
    class ConsolidatedTimerEntity : TableEntity
    {
        public int id { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string TimeWorked { get; set; }
    }
}
