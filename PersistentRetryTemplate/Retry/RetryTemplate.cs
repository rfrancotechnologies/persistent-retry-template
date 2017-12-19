using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LiteDB;
using PersistentRetryTemplate.Retry.BackOffPolicies;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry
{
    public class RetryTemplate: IRetryTemplate
    {
        internal const string PENDING_RETRIES_COLLECTION_NAME = "pending-retries";

        private static ConcurrentDictionary<string, object> blockingOperationCollections = new  ConcurrentDictionary<string, object>(); 

        private LiteDatabase database;

        public IBackOffPolicy BackOffPolicy { get; set; }
        public IRetryPolicy RetryPolicy { get; set; }

        public RetryTemplate(LiteDatabase database) {
            this.database = database;
            BackOffPolicy = new ExponentialBackOffPolicy();
            RetryPolicy = new SimpleRetryPolicy();
        }

        public PendingRetry<T> SaveForRetry<T>(string operationId, T argument)
        {
            var collection = database.GetCollection<PendingRetry<T>>(PENDING_RETRIES_COLLECTION_NAME);

            var pendingRetry = new PendingRetry<T>
            { 
                Argument = argument,
                OperationId = operationId
            };
            
            // Create index for the OperationId field
            collection.EnsureIndex(x => x.OperationId);
            
            collection.Insert(pendingRetry);

            BlockingCollection<PendingRetry<T>> blockingRetriesCollection = blockingOperationCollections.GetOrAdd(operationId, 
                    new BlockingCollection<PendingRetry<T>>()) as BlockingCollection<PendingRetry<T>>;
            blockingRetriesCollection.Add(pendingRetry);

            return pendingRetry;
        }

        public R DoExecute<T, R>(PendingRetry<T> pendingRetry, Func<T, R> retryCallback, Func<T, R> recoveryCallback, CancellationToken cancellationToken)
        {
            var collection = database.GetCollection<PendingRetry<T>>(RetryTemplate.PENDING_RETRIES_COLLECTION_NAME);

            Exception lastException = null;
            var retryPolicy = RetryPolicy;
            var backOffPolicy = BackOffPolicy;

            retryPolicy.StartContext();
            backOffPolicy.StartContext();

            while (retryPolicy.CanRetry(lastException) && !cancellationToken.IsCancellationRequested) 
            {
                try {
                    lastException = null;
                    R result = retryCallback.Invoke(pendingRetry.Argument);
                    collection.Delete(pendingRetry.Id);
                    return result;
                }
                catch (Exception e) 
                {
                    lastException = e;

                    if (retryPolicy.CanRetry(lastException) && !cancellationToken.IsCancellationRequested) 
                    {
                        retryPolicy.RegisterRetry(lastException);
                        backOffPolicy.BackOff();
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw new RetryInterruptedException("The execution of retries has been explicitly cancelled.");
            }
            else
            {
                collection.Delete(pendingRetry.Id);
                return HandleRetryExhausted(recoveryCallback, pendingRetry.Argument, lastException);
            }
        }

        public IEnumerable<PendingRetry<T>> GetPendingRetries<T>(string operationId)
        {
            var collection = database.GetCollection<PendingRetry<T>>(PENDING_RETRIES_COLLECTION_NAME);
            return collection.Find(Query.EQ("OperationId", operationId));
        }
        
        private void MarkAsCompleted<T>(PendingRetry<T> pendingRetry)
        {
            var collection = database.GetCollection<PendingRetry<T>>(PENDING_RETRIES_COLLECTION_NAME);

            collection.Delete(pendingRetry.Id);
        }

        private R HandleRetryExhausted<T, R>(Func<T, R> recoveryCallback, T argument, Exception lastException)
        {
            if (recoveryCallback != null) {
                try
                {
                    R recovered = recoveryCallback.Invoke(argument);
                    return recovered;
                }
                catch(Exception ex)
                {
                    throw new RetryExhaustedException("The retries are exhausted according to the specified policy.", ex);
                }
            }
            throw new RetryExhaustedException("The retries are exhausted according to the specified policy.", lastException);
        }

        public PendingRetry<T> TakePendingRetry<T>(string operationId)
        {
            BlockingCollection<PendingRetry<T>> blockingCollection = blockingOperationCollections.GetOrAdd(operationId, 
                    new BlockingCollection<PendingRetry<T>>()) as BlockingCollection<PendingRetry<T>>;
            if (blockingCollection.Count == 0)
            {
                var pendingRetries = GetPendingRetries<T>(operationId);
                foreach (var pendingRetry in pendingRetries)
                    blockingCollection.Add(pendingRetry);
            }
            return blockingCollection.Take();
        }
    }
}
