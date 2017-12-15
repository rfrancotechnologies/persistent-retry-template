using System;
using System.Collections.Generic;
using PersistentRetryTemplate.Retry.ExceptionClassifiers;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public abstract class AbstractSubclassRetryPolicy: IRetryPolicy 
    {
        private SubclassExceptionClassifier retryableClassifier;
        
        protected AbstractSubclassRetryPolicy(): this(GetDefaultExceptions())
        {
        }

        protected AbstractSubclassRetryPolicy(Dictionary<Type, bool> specificExceptions): this(specificExceptions, false)
        {
        }

        protected AbstractSubclassRetryPolicy(Dictionary<Type, bool> specificExceptions, bool defaultRetryability)
        {
            retryableClassifier = new SubclassExceptionClassifier(specificExceptions, defaultRetryability);
        }

        public bool CanRetry(Exception exception) {
            return (exception == null || retryableClassifier.Classify(exception)) && CanRetry();
        }

        protected abstract bool CanRetry();

        public abstract void StartContext();

        private static Dictionary<Type, bool> GetDefaultExceptions() {
            var retryableExceptions = new Dictionary<Type, Boolean>();
            retryableExceptions.Add(typeof(Exception), true);
            return retryableExceptions;
        }
    }
}