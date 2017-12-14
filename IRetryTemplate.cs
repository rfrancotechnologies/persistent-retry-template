using System;
using System.Collections.Generic;
using System.Threading;
using PersistentRetryTemplate.BackOffPolicies;
using PersistentRetryTemplate.RetryPolicies;

namespace PersistentRetryTemplate
{
    public interface IRetryTemplate
    {
        IBackOffPolicy BackOffPolicy { get; set; }
        IRetryPolicy RetryPolicy { get; set; }

        PendingRetry<T> SaveForRetry<T>(string operationId, T argument);

        R DoExecute<T, R>(PendingRetry<T> pendingRetry, Func<T, R> retryCallback, Func<T, R> recoveryCallback, 
                CancellationToken cancellationToken);

        BatchOperation<T> StartBatchOperation<T>(string operationId);

        IEnumerable<BatchOperation<T>> GetPendingBatchOperations<T>(string operationId);

        void CompleteBatch<T>(BatchOperation<T> batchOperation);

        void ExecuteRecoveryCallback();
    }
}