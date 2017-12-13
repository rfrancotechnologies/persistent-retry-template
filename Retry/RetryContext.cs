using System;

namespace PersistentRetryTemplate.Retry
{
    public class RetryContext
    {
        private int _retryCount;
        private Exception _lastException;

        /// <summary>  
        /// Counts the number of retry attempts. Before the first attempt this counter is zero,
        /// and before the first and subsequent attempts it should increment accordingly.
        /// </summary>  
        /// <returns>The number of retries.</returns>
        public int RetryCount 
        {
            get 
            {
                return _retryCount;
            }
        }

        /// <summary>  
        /// The exception object that caused the current retry.
        /// </summary>  
        /// <returns>
        /// The last exception that caused a retry, or null if this is the first attempt.
        /// </returns>
	    public Exception LastException
        {
            get
            {
                return _lastException;
            }
        }
    }
}