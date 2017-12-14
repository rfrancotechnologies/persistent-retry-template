using System;

namespace PersistentRetryTemplate 
{
    public class RetryExhaustedException: RetryException
    {
        public RetryExhaustedException(string message): base(message) {}

        public RetryExhaustedException(string message, Exception cause): base(message, cause) {}
    }
}