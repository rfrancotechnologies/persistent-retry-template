using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class SimpleRetryPolicyTests
    {
        [Fact]
        public void SholdOnlyAllowTheSpecifiedNumberOfAttempts() 
        {
            SimpleRetryPolicy policy = new SimpleRetryPolicy(5);
            policy.StartContext();

            Exception testException = new Exception();
            for (int i = 1; i <=5; i++)
            {
                Assert.True(policy.CanRetry(testException));
                policy.RegisterRetry(testException);
            }
            Assert.False(policy.CanRetry(testException));
        }
    }
}