using System;
using System.Collections.Generic;
using PersistentRetryTemplate.Retry.ExceptionClassifiers;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class NeverRetryPolicy: IRetryPolicy {
        public bool CanRetry(Exception exception)
        {
            return false;
        }

        public void RegisterRetry(Exception exception)
        {
        }

        public void StartContext()
        {
        }
    }
}
