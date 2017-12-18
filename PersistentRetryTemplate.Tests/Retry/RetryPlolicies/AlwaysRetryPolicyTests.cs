using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class AlwaysRetryPolicyTests
    {
        [Fact]
        public void ShouldAlwaysAllowRetries() 
        {
            AlwaysRetryPolicy policy = new AlwaysRetryPolicy();
            policy.StartContext();

            Exception testException = new Exception();
            for (int i = 1; i <= 1000; i++)
            {
                Assert.True(policy.CanRetry(testException));
            }
        }
    }
}