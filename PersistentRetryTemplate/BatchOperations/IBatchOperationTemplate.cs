using System;
using System.Collections.Generic;
using PersistentRetryTemplate.Retry;

namespace PersistentRetryTemplate.BatchOperations
{
    /// <summary>Manager of batch operations that manage several event-like pieces of data.</summary>  
    public interface IBatchOperationTemplate
    {
        /// <summary>Starts a new batch operation.</summary>  
        /// <param name="operationId">The operation identifier for the new batch operation.</param>
        /// <returns>New instance of BatchOperation that refers to the newly created batch operation.</returns>  
        BatchOperation<T> StartBatchOperation<T>(string operationId);

        /// <summary>Retrieves an enumeration of all the non-completed operations matching the given operation identifier.</summary>  
        /// <param name="operationId">The operation identifier for which all the matching batch operations will be retrieved.</param>
        /// <returns>Enumeration of batch operations matching the given operation identifier.</returns>  
        IEnumerable<BatchOperation<T>> GetPendingBatchOperations<T>(string operationId);

        /// <summary>Marks the given batch operation as completed.</summary>  
        /// <param name="batchOperation">The batch operation that will be marked as completed.</param>
        void Complete<T>(BatchOperation<T> batchOperation);

        /// <summary>Marks the given batch operation as completed, also returning a new pending retry operation for executing a completion callback.</summary>  
        /// <param name="retryTemplate">The retry template that will be used.</param>
        /// <param name="batchOperation">The batch operation that will be marked as completed.</param>
        /// <returns>Pending retry handle for retrying a completion callback.</returns>  
        PendingRetry<List<T>> CompleteWithFinishingCallback<T>(IRetryTemplate retryTemplate, BatchOperation<T> batchOperation);

        /// <summary>Adds a new piece of data to the specified batch operation.</summary>  
        /// <param name="batchOperation">The batch operation that will be added some data.</param>
        /// <param name="data">The piece of data that will be added to the batch operation.</param>
        void AddBatchOperationData<T>(BatchOperation<T> batchOperation, T data);
    }
}