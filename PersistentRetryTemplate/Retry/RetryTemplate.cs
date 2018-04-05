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

        /// <summary>Back-off policy to use for waiting before each retry. 
        /// The default value is an exponential back-off policy with a 100ms interval interval and a multiplier by 2.</summary>  
        public IBackOffPolicy BackOffPolicy { get; set; }

        /// <summary>Retry policy to use for deciding whether the operations should be retried after some exception.
        /// The default value is a simple retry policy with 3 maximum attempts.</summary>  
        public IRetryPolicy RetryPolicy { get; set; }

        /// <summary>Creates a new instance of <see cref="RetryTemplate"/> given an instance of <see cref="LiteDatabase"/>.</summary>  
        public RetryTemplate(LiteDatabase database) {
            this.database = database;
            BackOffPolicy = new ExponentialBackOffPolicy();
            RetryPolicy = new SimpleRetryPolicy();
        }

        /// <summary>Saves a new operation as pending for retries.</summary>  
        /// <param name="operationId">The operation identifier, that can be subsequently used to look for peding retry operations.</param>
        /// <param name="argument">The argument that must be provided to the operation when retried. It may be null.</param>
        /// <returns>A new instance of <see cref="PendingRetry"/> referring to the new created operation pending for retries.</returns>  
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
            
            lock(this) 
            {
                collection.Insert(pendingRetry);

                BlockingCollection<PendingRetry<T>> blockingRetriesCollection = blockingOperationCollections.GetOrAdd(operationId, 
                        new BlockingCollection<PendingRetry<T>>()) as BlockingCollection<PendingRetry<T>>;
                blockingRetriesCollection.Add(pendingRetry);
            }

            return pendingRetry;
        }

        /// <summary>
        /// Executes the provided retry callback. In case the callback is successful, the returned value of type 
        /// R will be returned. In case of failure (the retry callback throws an exception), the operation will be
        /// retried while the specified retry policy allows it. 
        ///
        /// If some back-off policy is defined, the amount of time configured in the policy will be waited before
        /// retrying the callback execution.
        ///
        /// When the callback fails and the retry policy does not allow more retries for some exception, a 
        /// RetryExhaustedException will be thrown (containing a reference to the exception that caused the 
        /// failure in the retry callback).
        /// </summary>  
        /// <param name="pendingRetry">Handle to an operation pending for retries as returned by SaveForRetry.</param>
        /// <param name="retryCallback">The function that will be invoked in each retry</param>
        /// <param name="recoveryCallback">An optional function that will be invoked when the retries over the operation have been exhausted according the specified RetryPolicy . If null, it be ignored.</param>
        /// <param name="cancellationToken">Cancellation token that allows to manually indicate that the retries should stop.</returns>  
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

        /// <summary>Retrieves the list of operations pending for retries that match the given operation identifier.</summary>  
        /// <param name="operationId">The operation identifier for which all the matching pending retries will be retrieved.</param>
        /// <returns>An enumerable of operations pending for retries that match the given operation identifer.</returns>  
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

        /// <summary>
        /// Retrieves one operation pending for retries in a blocking fashion. This operation will block the caller
        /// until an operation for that operationId is available. If a pending retry operation is available at the
        /// moment of invoking TakePendingRetry, the function will return immediately. 
        /// </summary>  
        /// <param name="operationId">The operation identifier for which one matching pending operation will be retrieved.</param>
        /// <returns>One operation pending for retries, that match the given operation identifer.</returns>  
        public PendingRetry<T> TakePendingRetry<T>(string operationId)
        {
            BlockingCollection<PendingRetry<T>> blockingCollection = blockingOperationCollections.GetOrAdd(operationId, 
                    new BlockingCollection<PendingRetry<T>>()) as BlockingCollection<PendingRetry<T>>;
            lock(this)
            {
                if (blockingCollection.Count == 0)
                {
                    var pendingRetries = GetPendingRetries<T>(operationId);
                    foreach (var pendingRetry in pendingRetries)
                        blockingCollection.Add(pendingRetry);
                }
            }
            return blockingCollection.Take();
        }
    }
}
