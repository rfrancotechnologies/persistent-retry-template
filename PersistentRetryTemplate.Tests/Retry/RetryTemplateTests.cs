using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;

namespace PersistentRetryTemplate.Retry
{
    public class RetryTemplateTests
    {
        [Fact]
        public void ShouldListSavedRetriesInThePendingRetries() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
            Assert.Contains<PendingRetry<string>>(pendingRetries, (x) => 
                    x.OperationId == testOperationId && x.Argument == testArgument);
        }

        [Fact]
        public void ShouldNotListSuccessfullyCompletedRetriesInThePendingRetries() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            
            retryTemplate.DoExecute(pendingRetry, (argument) => (object) null, null, CancellationToken.None);

            var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
            Assert.DoesNotContain<PendingRetry<string>>(pendingRetries, (x) => 
                    x.OperationId == testOperationId && x.Argument == testArgument);
        }

        [Fact]
        public void ShouldThrowExhaustedRetriesExceptionWhenRetriesExhausted() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());
            retryTemplate.RetryPolicy = new NeverRetryPolicy();

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);

            Assert.Throws<RetryExhaustedException>(() => {
                retryTemplate.DoExecute<string, object>(pendingRetry, (argument) => throw new Exception(), null, CancellationToken.None);
            }); 
        }

        [Fact]
        public void ShouldNotListRetriesWithExhaustedRetriesInThePendingRetries() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());
            retryTemplate.RetryPolicy = new NeverRetryPolicy();

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            
            try
            {
                retryTemplate.DoExecute<string, object>(pendingRetry, (argument) => throw new Exception(), null, CancellationToken.None);
            }
            catch(RetryExhaustedException) {}

            var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
            Assert.DoesNotContain<PendingRetry<string>>(pendingRetries, (x) => 
                    x.OperationId == testOperationId && x.Argument == testArgument);
        }

        [Fact]
        public void ShouldThrowRetryInterruptedExceptionWhenTheExcutionIsCancelled() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());
            retryTemplate.RetryPolicy = new NeverRetryPolicy();

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);

            Assert.Throws<RetryInterruptedException>(() => {
                retryTemplate.DoExecute<string, object>(pendingRetry, (argument) => (object) null, 
                        null, new CancellationToken(true));
            }); 
        }

        [Fact]
        public void ShouldListRetriesWithCancelledExcutionInThePendingRetries() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());
            retryTemplate.RetryPolicy = new NeverRetryPolicy();

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            
            try
            {
                retryTemplate.DoExecute<string, object>(pendingRetry, (argument) => (object) null, 
                        null, new CancellationToken(true));
            }
            catch(RetryInterruptedException) {}

            var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
            Assert.Contains<PendingRetry<string>>(pendingRetries, (x) => 
                    x.OperationId == testOperationId && x.Argument == testArgument);
        }

        [Fact]
        public void ShouldRetryOnExceptionsWhileTheRetryPolicySaysSoAndFinishWithARetryExhaustedException()
        {
            RetryTemplate retryTemplate = new RetryTemplate(Path.GetTempFileName());
            retryTemplate.RetryPolicy = new SimpleRetryPolicy(3);

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            
            int executionCounter = 0;
            try
            {
                retryTemplate.DoExecute<string, object>(pendingRetry, (argument) => 
                        {
                            executionCounter++;
                            throw new Exception();
                        }, null, CancellationToken.None);
            }
            catch(RetryExhaustedException) {}
            Assert.Equal(3, executionCounter);
        }
    }
}
