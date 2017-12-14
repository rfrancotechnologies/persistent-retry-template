using System.Threading;
using PersistentRetryTemplate.RetryPolicies;

namespace PersistentRetryTemplate.BackOffPolicies 
{
    public class ExponentialBackOffPolicy: IBackOffPolicy
    {
        public const int DEFAULT_INITIAL_INTERVAL = 100;
        public const int DEFAULT_MAX_INTERVAL = 30000;
        public const double DEFAULT_MULTIPLIER = 2;

        private int initialInterval;
        private int maxInterval;
        private double multiplier;

        private ExponentialBackOffContext context;

        public ExponentialBackOffPolicy() 
        {
            initialInterval = DEFAULT_INITIAL_INTERVAL;            
            maxInterval = DEFAULT_MAX_INTERVAL;
            multiplier = DEFAULT_MULTIPLIER;
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

        public void StartContext() {
             context = new ExponentialBackOffContext(this.initialInterval, this.multiplier, this.maxInterval);
        }

        public void BackOff() {
            int sleepTime = context.GetSleepAndIncrement();
            Thread.Sleep(sleepTime);
        }

        private class ExponentialBackOffContext {

            public double Multiplier { get; private set; }
            public int Interval { get; private set; }
            public int MaxInterval { get; private set; }

            public ExponentialBackOffContext(int expSeed, double multiplier, int maxInterval) {
                this.Interval = expSeed;
                this.Multiplier = multiplier;
                this.MaxInterval = maxInterval;
            }

            public int GetSleepAndIncrement() {
                lock(this) {
                    int sleep = this.Interval;
                    if (sleep > MaxInterval) {
                        sleep = MaxInterval;
                    }
                    else {
                        this.Interval = GetNextInterval();
                    }
                    return sleep;
                }
            }

            protected int GetNextInterval() {
                return (int) (this.Interval * this.Multiplier);
            }
        }

        public override string ToString() {
            return "ExponentialBackOffPolicy[initialInterval=" + initialInterval + ", multiplier="
                    + multiplier + ", maxInterval=" + maxInterval + "]";
        }
    }
}