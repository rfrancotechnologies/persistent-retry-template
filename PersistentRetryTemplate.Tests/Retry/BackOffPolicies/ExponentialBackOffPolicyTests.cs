using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.BackOffPolicies;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class ExponentialBackOffPolicyTests
    {
        [Fact]
        public void ShouldBackOffAnIncrementingIntervalEveryCallUpToTheMaximumSpecifiedInterval() 
        {
            ExponentialBackOffPolicy policy = new ExponentialBackOffPolicy();
            policy.InitialInterval = TimeSpan.FromMilliseconds(100);
            policy.MaxInterval = TimeSpan.FromMilliseconds(300);
            policy.Multiplier = 2;
            policy.StartContext();

            AssertBackOffIsApproximately(policy, TimeSpan.FromMilliseconds(100));
            AssertBackOffIsApproximately(policy, TimeSpan.FromMilliseconds(200));
            AssertBackOffIsApproximately(policy, TimeSpan.FromMilliseconds(300));
            AssertBackOffIsApproximately(policy, TimeSpan.FromMilliseconds(300));
        }

        private void AssertBackOffIsApproximately(ExponentialBackOffPolicy policy, TimeSpan reference) 
        {
            DateTime startTime = DateTime.Now;
            policy.BackOff();
            TimeSpan difference = DateTime.Now - startTime;
            Assert.True(difference - reference < TimeSpan.FromMilliseconds(10));
        }
    }
}