using System;
using System.Collections.Generic;
using System.Threading;
using PersistentRetryTemplate.Retry.BackOffPolicies;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry
{
    public interface IRetryTemplate
    {
        IBackOffPolicy BackOffPolicy { get; set; }
        IRetryPolicy RetryPolicy { get; set; }

        PendingRetry<T> SaveForRetry<T>(string operationId, T argument);

        IEnumerable<PendingRetry<T>> GetPendingRetries<T>(string operationId);

        PendingRetry<T> TakePendingRetry<T>(string operationId);

        R DoExecute<T, R>(PendingRetry<T> pendingRetry, Func<T, R> retryCallback, Func<T, R> recoveryCallback, 
                CancellationToken cancellationToken);
    }
}