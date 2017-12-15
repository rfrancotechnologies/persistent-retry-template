using System;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public interface IRetryPolicy
    {
        bool CanRetry(Exception exception);

    	void StartContext();
    }
}