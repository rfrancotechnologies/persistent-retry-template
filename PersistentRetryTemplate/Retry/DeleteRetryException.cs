using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentRetryTemplate.Retry
{
    public class DeleteRetryException : Exception
    {
        public DeleteRetryException(string message) : base(message) { }

        public DeleteRetryException(string message, Exception cause) : base(message, cause) { }
    }
}
