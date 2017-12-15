using System;

namespace PersistentRetryTemplate.Retry
{
    public class RetryInterruptedException: RetryException
    {
        public RetryInterruptedException(string message): base(message) {}

        public RetryInterruptedException(string message, Exception cause): base(message, cause) {}
    }
}