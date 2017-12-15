using System;

namespace PersistentRetryTemplate.Retry 
{
    public class RetryException: Exception
    {
        public RetryException(string message): base(message) {}

        public RetryException(string message, Exception cause): base(message, cause) {}
    }
}