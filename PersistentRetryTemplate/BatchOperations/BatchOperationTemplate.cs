using System;
using System.Collections.Generic;
using LiteDB;
using PersistentRetryTemplate.Retry;

namespace PersistentRetryTemplate.BatchOperations
{
    /// <summary>Manager of batch operations that manage several event-like pieces of data.</summary>  
    public class BatchOperationTemplate : IBatchOperationTemplate
    {
        private const string BATCH_OPERATIONS_COLLECTION_NAME = "batch-operations";
        private LiteDatabase database;

        /// <summary>Creates a new instance of <see cref="BatchOperationTemplate"/> given an instance of <see cref="LiteDatabase"/>.</summary>  
        public BatchOperationTemplate(LiteDatabase database) {
            this.database = database;
        }

        /// <summary>Adds a new piece of data to the specified batch operation.</summary>  
        /// <param name="batchOperation">The batch operation that will be added some data.</param>
        /// <param name="data">The piece of data that will be added to the batch operation.</param>
        public void AddBatchOperationData<T>(BatchOperation<T> batchOperation, T data)
        {
            var collection = database.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);

            batchOperation.BatchData.Add(data);
            collection.Update(batchOperation);
        }

        /// <summary>Marks the given batch operation as completed.</summary>  
        /// <param name="batchOperation">The batch operation that will be marked as completed.</param>
        public void Complete<T>(BatchOperation<T> batchOperation)
        {
            var collection = database.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);
            collection.Delete(batchOperation.Id);
        }

        /// <summary>Marks the given batch operation as completed, also returning a new pending retry operation for executing a completion callback.</summary>  
        /// <param name="retryTemplate">The retry template that will be used.</param>
        /// <param name="batchOperation">The batch operation that will be marked as completed.</param>
        /// <returns>Pending retry handle for retrying a completion callback.</returns>  
        public PendingRetry<List<T>> CompleteWithFinishingCallback<T>(IRetryTemplate retryTemplate, 
                BatchOperation<T> batchOperation)
        {
            var pendingRetry = retryTemplate.SaveForRetry(batchOperation.OperationId, batchOperation.BatchData);
            Complete(batchOperation);
            return pendingRetry;
        }

        /// <summary>Retrieves an enumeration of all the non-completed operations matching the given operation identifier.</summary>  
        /// <param name="operationId">The operation identifier for which all the matching batch operations will be retrieved.</param>
        /// <returns>Enumeration of batch operations matching the given operation identifier.</returns>  
        public IEnumerable<BatchOperation<T>> GetPendingBatchOperations<T>(string operationId)
        {
            var collection = database.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);
            return collection.Find(Query.EQ("OperationId", operationId));
        }

        /// <summary>Starts a new batch operation.</summary>  
        /// <param name="operationId">The operation identifier for the new batch operation.</param>
        /// <returns>New instance of BatchOperation that refers to the newly created batch operation.</returns>  
        public BatchOperation<T> StartBatchOperation<T>(string operationId)
        {
            var collection = database.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);

            var batchOperation = new BatchOperation<T> {
                OperationId = operationId
            };

            collection.EnsureIndex(x => x.OperationId);
            collection.Insert(batchOperation);

            return batchOperation;
        }
    }
}