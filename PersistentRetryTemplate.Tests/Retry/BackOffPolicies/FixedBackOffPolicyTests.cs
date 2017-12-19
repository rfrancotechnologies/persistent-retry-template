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
    public class FixedBackOffPolicyTests
    {
        [Fact]
        public void ShouldBackOffAFixedAmountOfTimeEveryCall() 
        {
            TimeSpan testDelay = TimeSpan.FromMilliseconds(100);
            FixedBackOffPolicy policy = new FixedBackOffPolicy(testDelay);
            policy.StartContext();

            DateTime startTime = DateTime.Now;
            policy.BackOff();
            TimeSpan difference = DateTime.Now - startTime;
            Assert.True(difference - testDelay < TimeSpan.FromMilliseconds(20));

            startTime = DateTime.Now;
            policy.BackOff();
            difference = DateTime.Now - startTime;
            Assert.True(difference - testDelay < TimeSpan.FromMilliseconds(20));
        }
    }
}