using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));

            string testOperationId = "test.operation";
            string testArgument = "test argument";
            var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
            Assert.Contains<PendingRetry<string>>(pendingRetries, (x) => 
                    x.OperationId == testOperationId && x.Argument == testArgument);
        }

        [Fact]
        public void ShouldPersistThePendingRetriesAfterCreateNewInstancesOfTheRetryTemplate() 
        {
            string testOperationId = "test.operation";
            string testArgument = "test argument";
            string tempFileName = Path.GetTempFileName();

            using (var database = new LiteDatabase(tempFileName))
            {
                RetryTemplate retryTemplate = new RetryTemplate(database);
                var pendingRetry = retryTemplate.SaveForRetry<string>(testOperationId, testArgument);
            }

            using (var database = new LiteDatabase(tempFileName))
            {
                RetryTemplate retryTemplate = new RetryTemplate(database);
                var pendingRetries = retryTemplate.GetPendingRetries<string>(testOperationId);
                Assert.Contains<PendingRetry<string>>(pendingRetries, (x) => 
                        x.OperationId == testOperationId && x.Argument == testArgument);
            }
        }

        [Fact]
        public void ShouldNotListSuccessfullyCompletedRetriesInThePendingRetries() 
        {
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));

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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
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
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
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

        [Fact]
        public void ShouldUnblockWaitingCallersWhenNewPendingRetriesAreAvailable()
        {
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
            string testOperationId = "test.operation";

            AutoResetEvent autoResetEvent = new AutoResetEvent(false); 

            Task.Run(() => 
            {
                retryTemplate.TakePendingRetry<string>(testOperationId);
                autoResetEvent.Set();
            });

            retryTemplate.SaveForRetry<string>(testOperationId, "");
            Assert.True(autoResetEvent.WaitOne(20));
        }

        [Fact]
        public void ShouldUnblockWaitingCallersIfPendingRetriesAlreadyExisted()
        {
            RetryTemplate retryTemplate = new RetryTemplate(new LiteDatabase(Path.GetTempFileName()));
            string testOperationId = "test.operation";

            AutoResetEvent autoResetEvent = new AutoResetEvent(false); 
            retryTemplate.SaveForRetry<string>(testOperationId, "");

            Task.Run(() => 
            {
                retryTemplate.TakePendingRetry<string>(testOperationId);
                autoResetEvent.Set();
            });

            Assert.True(autoResetEvent.WaitOne(20));
        }
    }
}
