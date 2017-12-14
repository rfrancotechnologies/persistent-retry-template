using System;

namespace PersistentRetryTemplate.RetryPolicies
{
    public interface IRetryPolicy
    {
        bool CanRetry(Exception exception);

    	void StartContext();
    }
}