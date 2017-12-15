
using System.Threading;
using PersistentRetryTemplate.Retry.RetryPolicies;

namespace PersistentRetryTemplate.Retry.BackOffPolicies 
{
    public class NoBackOffPolicy: IBackOffPolicy 
    {
        public void BackOff() 
        {
        }

        public override string ToString() 
        {
            return "NoBackOffPolicy[No back-off is performed]";
        }

        public void StartContext()
        {
        }
    }
}