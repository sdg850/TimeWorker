using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorker.Commom.Responses
{
    public class RegisterResponse
    {
        public bool isSuccess { get; set; }
        public string Mesages { get; set; }
        public object Result { get; set; }

    }
}
