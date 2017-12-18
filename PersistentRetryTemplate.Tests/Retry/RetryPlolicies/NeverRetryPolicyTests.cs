using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class NeverRetryPolicyTests
    {
        [Fact]
        public void ShouldNeverAllowRetries() 
        {
            NeverRetryPolicy policy = new NeverRetryPolicy();
            policy.StartContext();

            Exception testException = new Exception();
            for (int i = 1; i <= 1000; i++)
            {
                Assert.False(policy.CanRetry(testException));
            }
        }
    }
}