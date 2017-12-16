using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class TimeoutRetryPolicyTests
    {
        [Fact]
        public void SholdOnlyAllowTheRetriesUntilTheTimeoutPasses() 
        {
            TimeoutRetryPolicy policy = new TimeoutRetryPolicy(TimeSpan.FromMilliseconds(400));
            policy.StartContext();

            Exception testException = new Exception();
            Thread.Sleep(300);
            Assert.True(policy.CanRetry(testException));
            Thread.Sleep(150);
            Assert.False(policy.CanRetry(testException));
        }
    }
}