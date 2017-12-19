using System;
using System.Collections.Generic;
using System.Threading;
using PersistentRetryTemplate.Retry.BackOffPolicies;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry
{
    public interface IRetryTemplate
    {
        /// <summary>Back-off policy to use for waiting before each retry.</summary>  
        IBackOffPolicy BackOffPolicy { get; set; }
        /// <summary>Retry policy to use for deciding whether the operations should be retried after some exception.</summary>  
        IRetryPolicy RetryPolicy { get; set; }

        /// <summary>Saves a new operation as pending for retries.</summary>  
        /// <param name="operationId">The operation identifier, that can be subsequently used to look for peding retry operations.</param>
        /// <param name="argument">The argument that must be provided to the operation when retried. It may be null.</param>
        /// <returns>A new instance of <see cref="PendingRetry"/> referring to the new created operation pending for retries.</returns>  
        PendingRetry<T> SaveForRetry<T>(string operationId, T argument);

        /// <summary>Retrieves the list of operations pending for retries that match the given operation identifier.</summary>  
        /// <param name="operationId">The operation identifier for which all the matching pending retries will be retrieved.</param>
        /// <returns>An enumerable of operations pending for retries that match the given operation identifer.</returns>  
        IEnumerable<PendingRetry<T>> GetPendingRetries<T>(string operationId);

        /// <summary>
        /// Retrieves one operation pending for retries in a blocking fashion. This operation will block the caller
        /// until an operation for that operationId is available. If a pending retry operation is available at the
        /// moment of invoking TakePendingRetry, the function will return immediately. 
        /// </summary>  
        /// <param name="operationId">The operation identifier for which one matching pending operation will be retrieved.</param>
        /// <returns>One operation pending for retries, that match the given operation identifer.</returns>  
        PendingRetry<T> TakePendingRetry<T>(string operationId);

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
        R DoExecute<T, R>(PendingRetry<T> pendingRetry, Func<T, R> retryCallback, Func<T, R> recoveryCallback, 
                CancellationToken cancellationToken);
    }
}