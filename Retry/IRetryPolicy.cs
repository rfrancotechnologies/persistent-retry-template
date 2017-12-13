using System;

namespace PersistentRetryTemplate.Retry
{
    public interface IRetryPolicy
    {
        bool CanRetry(RetryContext retryContext);
    }
}