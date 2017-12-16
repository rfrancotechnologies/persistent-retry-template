using System;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class TimeoutRetryPolicy : IRetryPolicy
    {
        private DateTime timeoutDateTime;
        private TimeSpan timeout;

        public TimeoutRetryPolicy(TimeSpan timeout)
        {
            this.timeout = timeout;
            StartContext();
        }

        public bool CanRetry(Exception exception)
        {
            return DateTime.Now < timeoutDateTime;
        }

        public void RegisterRetry(Exception exception)
        {
        }

        public void StartContext()
        {
            timeoutDateTime = DateTime.Now.Add(timeout);
        }
    }
}