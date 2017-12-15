
using System.Threading;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry.BackOffPolicies 
{
    public class FixedBackOffPolicy: IBackOffPolicy 
    {
        private const int DEFAULT_BACK_OFF_PERIOD = 1000;

        private int backOffPeriod;

        public FixedBackOffPolicy(): this(DEFAULT_BACK_OFF_PERIOD) 
        {
        }

        public FixedBackOffPolicy(int backOffPeriod) 
        {
            this.backOffPeriod = backOffPeriod;
        }

        public int BackOffPeriod 
        { 
            get
            {
                return backOffPeriod;
            } 
            
            set
            {
                backOffPeriod = (value > 0 ? value : 1);
            } 
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