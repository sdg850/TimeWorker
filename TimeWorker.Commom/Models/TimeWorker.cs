using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorker.Commom.Models
{
    public class Timeworker
    {
        public Int32 Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
