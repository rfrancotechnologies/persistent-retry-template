using System;
using System.Threading;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry.BackOffPolicies 
{
    public class ExponentialBackOffPolicy: IBackOffPolicy
    {
        private TimeSpan currentInterval;
        public TimeSpan InitialInterval { get; set; }
        public TimeSpan MaxInterval { get; set; }
        public int Multiplier { get; set; }

        public ExponentialBackOffPolicy() 
        {
            InitialInterval = TimeSpan.FromMilliseconds(100);            
            MaxInterval = TimeSpan.FromSeconds(30);
            Multiplier = 2;
            currentInterval = InitialInterval;
        }

        public void StartContext() 
        {
            currentInterval = InitialInterval;
        }

        public void BackOff() 
        {
            TimeSpan sleepTime = GetSleepAndIncrement();
            Thread.Sleep(sleepTime);
        }

        public override string ToString() 
        {
            return "ExponentialBackOffPolicy[initialInterval=" + InitialInterval + ", multiplier="
                    + Multiplier + ", maxInterval=" + MaxInterval + "]";
        }

        private TimeSpan GetSleepAndIncrement() 
        {
            lock(this) {
                TimeSpan sleep = currentInterval;
                if (sleep > MaxInterval) {
                    sleep = MaxInterval;
                }
                else {
                    currentInterval = GetNextInterval();
                }
                return sleep;
            }
        }

        private TimeSpan GetNextInterval() 
        {
            TimeSpan result = TimeSpan.Zero;
            for (int i = 0; i < Multiplier; i++)
                result += currentInterval;
            return result;
        }
    }
}