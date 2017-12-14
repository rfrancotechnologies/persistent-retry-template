using System;
using System.Collections.Generic;
using PersistentRetryTemplate.ExceptionClassifiers;

namespace PersistentRetryTemplate.RetryPolicies
{
    public class SimpleRetryPolicy: IRetryPolicy {
        public const int DEFAULT_MAX_ATTEMPTS = 3;

        public int maxAttempts { get; set; }

        public int count;

        private SubclassExceptionClassifier retryableClassifier = new SubclassExceptionClassifier(false);

        public SimpleRetryPolicy(): this(DEFAULT_MAX_ATTEMPTS)
        {
            
        }

        public SimpleRetryPolicy(int maxAttempts): this(maxAttempts, SimpleRetryPolicy.GetDefaultRetryableExceptions())
        {
        }

        public SimpleRetryPolicy(int maxAttempts, Dictionary<Type, bool> retryableExceptions): 
            this(maxAttempts, retryableExceptions, false)
        {
        }

        public SimpleRetryPolicy(int maxAttempts, Dictionary<Type, bool> retryableExceptions, bool defaultValue) 
        {
            this.maxAttempts = maxAttempts;
            retryableClassifier = new SubclassExceptionClassifier(retryableExceptions, defaultValue);
            count = 0;
        }

        public bool CanRetry(Exception exception) {
            return (exception == null || RetryForException(exception)) && count < maxAttempts;
        }


        public void StartContext() {
            count = 0;
        }

        private bool RetryForException(Exception ex) {
            return retryableClassifier.Classify(ex);
        }

        public String toString() {
            return "SimpleRetryPolicy " + "[maxAttempts=" + maxAttempts + "]";
        }

        private static Dictionary<Type, bool> GetDefaultRetryableExceptions() {
            var retryableExceptions = new Dictionary<Type, Boolean>();
            retryableExceptions.Add(typeof(Exception), true);
            return retryableExceptions;
        }
    }
}
