using System.Threading;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry.BackOffPolicies 
{
    public class ExponentialBackOffPolicy: IBackOffPolicy
    {
        public const int DEFAULT_INITIAL_INTERVAL = 100;
        public const int DEFAULT_MAX_INTERVAL = 30000;
        public const double DEFAULT_MULTIPLIER = 2;

        private int initialInterval;
        private int maxInterval;
        private double multiplier;
        private int currentInterval;

        public ExponentialBackOffPolicy() 
        {
            initialInterval = DEFAULT_INITIAL_INTERVAL;            
            maxInterval = DEFAULT_MAX_INTERVAL;
            multiplier = DEFAULT_MULTIPLIER;
            currentInterval = initialInterval;
        }

        public int InitialInterval 
        { 
            get 
            {
                return initialInterval;
            }
            
            set
            {
                initialInterval = (value > 1 ? value : 1);
            }
        }

        public int MaxInterval
        { 
            get 
            {
                return maxInterval;
            }
            
            set
            {
                maxInterval = (value > 0 ? value : 1);
            }
        }
        
        private double Multiplier
        { 
            get 
            {
                return multiplier;
            }
            
            set
            {
                multiplier = (value > 1.0 ? value : 1.0);
            }
        }

        public void StartContext() 
        {
            currentInterval = initialInterval;
        }

        public void BackOff() 
        {
            int sleepTime = GetSleepAndIncrement();
            Thread.Sleep(sleepTime);
        }

        public override string ToString() 
        {
            return "ExponentialBackOffPolicy[initialInterval=" + initialInterval + ", multiplier="
                    + multiplier + ", maxInterval=" + maxInterval + "]";
        }

        private int GetSleepAndIncrement() 
        {
            lock(this) {
                int sleep = currentInterval;
                if (sleep > MaxInterval) {
                    sleep = MaxInterval;
                }
                else {
                    currentInterval = GetNextInterval();
                }
                return sleep;
            }
        }

        private int GetNextInterval() 
        {
            return (int) (currentInterval * this.Multiplier);
        }
    }
}