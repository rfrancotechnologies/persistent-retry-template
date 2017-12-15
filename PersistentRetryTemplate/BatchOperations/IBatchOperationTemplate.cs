using System;
using System.Collections.Generic;
using PersistentRetryTemplate.Retry;

namespace PersistentRetryTemplate.BatchOperations
{
    public interface IBatchOperationTemplate
    {
        BatchOperation<T> StartBatchOperation<T>(string operationId);

        IEnumerable<BatchOperation<T>> GetPendingBatchOperations<T>(string operationId);

        void CompleteBatch<T>(BatchOperation<T> batchOperation);

        PendingRetry<List<T>> SaveRecoveryCallbackForRetries<T>(RetryTemplate retryTemplate, BatchOperation<T> batchOperation);

        void AddBatchOperationData<T>(BatchOperation<T> batchOperation, T data);
    }
}