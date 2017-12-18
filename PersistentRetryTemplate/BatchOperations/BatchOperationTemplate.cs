using System;
using System.Collections.Generic;
using LiteDB;
using PersistentRetryTemplate.Retry;

namespace PersistentRetryTemplate.BatchOperations
{
    public class BatchOperationTemplate : IBatchOperationTemplate
    {
        private const string BATCH_OPERATIONS_COLLECTION_NAME = "batch-operations";
        private string databasePath;

        public BatchOperationTemplate(string databasePath) {
            this.databasePath = databasePath;
        }

        public void AddBatchOperationData<T>(BatchOperation<T> batchOperation, T data)
        {
            using(var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);

                batchOperation.BatchData.Add(data);
                collection.Update(batchOperation);
            }
        }

        public void CompleteBatch<T>(BatchOperation<T> batchOperation)
        {
            using(var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);
                collection.Delete(batchOperation.Id);
            }
        }

        public PendingRetry<List<T>> SaveRecoveryCallbackForRetries<T>(IRetryTemplate retryTemplate, 
                BatchOperation<T> batchOperation)
        {
            var pendingRetry = retryTemplate.SaveForRetry(batchOperation.OperationId, batchOperation.BatchData);
            CompleteBatch(batchOperation);
            return pendingRetry;
        }

        public IEnumerable<BatchOperation<T>> GetPendingBatchOperations<T>(string operationId)
        {
            using(var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);
                return collection.Find(Query.EQ("OperationId", operationId));
            }
        }

        public BatchOperation<T> StartBatchOperation<T>(string operationId)
        {
            using(var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<BatchOperation<T>>(BATCH_OPERATIONS_COLLECTION_NAME);

                var batchOperation = new BatchOperation<T> {
                    OperationId = operationId
                };

                collection.EnsureIndex(x => x.OperationId);
                collection.Insert(batchOperation);

                return batchOperation;
            }
        }
    }
}