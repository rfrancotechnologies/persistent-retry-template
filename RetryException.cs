using System;

namespace PersistentRetryTemplate 
{
    public class RetryException: Exception
    {
        public RetryException(string message): base(message) {}

        public RetryException(string message, Exception cause): base(message, cause) {}
    }
}