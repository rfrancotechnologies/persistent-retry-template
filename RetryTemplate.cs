using System;
using System.Threading;
using PersistentRetryTemplate.BackOffPolicies;
using PersistentRetryTemplate.RetryPolicies;

namespace PersistentRetryTemplate
{
    public class RetryTemplate<K>
    {
        public IBackOffPolicy BackOffPolicy { get; set; }
        public IRetryPolicy RetryPolicy { get; set; }

        public RetryTemplate(string databasePath) {

        }

        protected R DoExecute<T, R>(K key, T argument, Func<T, R> retryCallback, Func<T, R> recoveryCallback, 
                CancellationToken cancellationToken) {
            Exception lastException = null;
            var retryPolicy = RetryPolicy;
            var backOffPolicy = BackOffPolicy;

            retryPolicy.StartContext();
            backOffPolicy.StartContext();

            while (retryPolicy.CanRetry(lastException) && !cancellationToken.IsCancellationRequested) {
                try {
                    lastException = null;
                    return retryCallback.Invoke(argument);
                }
                catch (Exception e) {
                    lastException = e;

                    if (retryPolicy.CanRetry(lastException) && !cancellationToken.IsCancellationRequested) {
                        backOffPolicy.BackOff();
                    }
                }
            }

            return HandleRetryExhausted(recoveryCallback, argument, lastException);
        }

        protected R HandleRetryExhausted<T, R>(Func<T, R> recoveryCallback, T argument, Exception lastException)
        {
            if (recoveryCallback != null) {
                R recovered = recoveryCallback.Invoke(argument);
                return recovered;
            }
            throw lastException;
        }
    }
}
