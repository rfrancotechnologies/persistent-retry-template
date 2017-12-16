using System;
using System.Collections.Generic;
using PersistentRetryTemplate.Retry.ExceptionClassifiers;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class SimpleRetryPolicy: AbstractSubclassRetryPolicy {
        public const int DEFAULT_MAX_ATTEMPTS = 3;

        public int maxAttempts { get; set; }

        public int count;

        public SimpleRetryPolicy(): this(DEFAULT_MAX_ATTEMPTS)
        {
        }

        public SimpleRetryPolicy(int maxAttempts): base()
        {
            this.maxAttempts = maxAttempts;
            count = 0;
        }

        public SimpleRetryPolicy(int maxAttempts, Dictionary<Type, bool> retriableExceptions): base(retriableExceptions)
        {
            this.maxAttempts = maxAttempts;
            count = 0;
        }

        public SimpleRetryPolicy(int maxAttempts, Dictionary<Type, bool> retriableExceptions, bool defaultRetryability)
            :base(retriableExceptions, defaultRetryability)

        {
            this.maxAttempts = maxAttempts;
            count = 0;
        }

        protected override bool CanRetry() {
            return count < maxAttempts;
        }

        public override void StartContext() {
            count = 0;
        }

        public override void RegisterRetry(Exception exception) 
        {
            count++;
        }

        public override string ToString() {
            return "SimpleRetryPolicy " + "[maxAttempts=" + maxAttempts + "]";
        }
    }
}
