
using System;
using System.Threading;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry.BackOffPolicies 
{
    public class FixedBackOffPolicy: IBackOffPolicy 
    {
        private TimeSpan backOffPeriod;

        public FixedBackOffPolicy(): this(TimeSpan.FromSeconds(1)) 
        {
        }

        public FixedBackOffPolicy(TimeSpan backOffPeriod) 
        {
            this.backOffPeriod = backOffPeriod;
        }

        public void BackOff() 
        {
            Thread.Sleep(backOffPeriod);
        }

        public override string ToString() 
        {
            return "FixedBackOffPolicy[backOffPeriod=" + backOffPeriod + "]";
        }

        public void StartContext()
        {
        }
    }
}