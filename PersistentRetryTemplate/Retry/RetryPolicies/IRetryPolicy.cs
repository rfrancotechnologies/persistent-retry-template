using System;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public interface IRetryPolicy
    {
        bool CanRetry(Exception exception);

        void RegisterRetry(Exception exception);

    	void StartContext();
    }
}