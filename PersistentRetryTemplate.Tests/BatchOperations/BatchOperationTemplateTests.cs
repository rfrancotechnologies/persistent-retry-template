using System;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.BatchOperations;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.RetryPolicies;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace PersistentRetryTemplate.Retry
{
    public class BatchOperationTemplateTests
    {
        [Fact]
        public void ShouldListSavedBatchOperationsInThePendingListing() 
        {
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(Path.GetTempFileName());

            string testOperationId = "test.operation";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);
            var batchOperations = batchOperationTemplate.GetPendingBatchOperations<string>(testOperationId);
            Assert.Contains<BatchOperation<string>>(batchOperations, (x) => x.OperationId == testOperationId);
        }

        [Fact]
        public void ShouldPersistTheBatchOperationsAfterCreatingNewInstancesOfTheBatchOperationTemplate() 
        {
            string tempFileName = Path.GetTempFileName();
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(tempFileName);

            string testOperationId = "test.operation";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);

            batchOperationTemplate = new BatchOperationTemplate(tempFileName);
            var batchOperations = batchOperationTemplate.GetPendingBatchOperations<string>(testOperationId);
            Assert.Contains<BatchOperation<string>>(batchOperations, (x) => x.OperationId == testOperationId);
        }

        [Fact]
        public void ShouldNotListCompletedBatchOperationsInThePendingListing() 
        {
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(Path.GetTempFileName());

            string testOperationId = "test.operation";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);
            batchOperationTemplate.CompleteBatch(batchOperation);

            var batchOperations = batchOperationTemplate.GetPendingBatchOperations<string>(testOperationId);
            Assert.DoesNotContain<BatchOperation<string>>(batchOperations, (x) => x.OperationId == testOperationId);
        }

        [Fact]
        public void ShouldPersistTheBatchOperationDataEvenAfterCreatingNewInstancesOfTheBatchOperationTemplate() 
        {
            string tempFileName = Path.GetTempFileName();
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(tempFileName);

            string testOperationId = "test.operation";
            string testBatchOperationData = "test data";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);
            batchOperationTemplate.AddBatchOperationData(batchOperation, testBatchOperationData);

            batchOperationTemplate = new BatchOperationTemplate(tempFileName);
            var batchOperations = batchOperationTemplate.GetPendingBatchOperations<string>(testOperationId);
            var obtainedBatchOperation = batchOperations.FirstOrDefault((x) => x.OperationId == testOperationId);
            Assert.NotNull(obtainedBatchOperation);
            Assert.Contains<string>(testBatchOperationData, obtainedBatchOperation.BatchData);
        }

        [Fact]
        public void ShouldSaveANewPendingRetryWhenSavingTheBatchRecoveryCallForRetries()
        {
            string tempFileName = Path.GetTempFileName();
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(tempFileName);

            string testOperationId = "test.operation";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);

            Mock<IRetryTemplate> mockRetryTemplate = new Mock<IRetryTemplate>();
            batchOperationTemplate.SaveRecoveryCallbackForRetries(mockRetryTemplate.Object, batchOperation);            
            mockRetryTemplate.Verify(x => x.SaveForRetry(testOperationId, It.IsAny<List<string>>()));
        }

        [Fact]
        public void ShouldNotListBatchOperationsWhoseRecoveryHasBeenSavedForRetriesInThePendingListing() 
        {
            BatchOperationTemplate batchOperationTemplate = new BatchOperationTemplate(Path.GetTempFileName());

            string testOperationId = "test.operation";
            var batchOperation = batchOperationTemplate.StartBatchOperation<string>(testOperationId);
            Mock<IRetryTemplate> mockRetryTemplate = new Mock<IRetryTemplate>();
            batchOperationTemplate.SaveRecoveryCallbackForRetries(mockRetryTemplate.Object, batchOperation);            

            var batchOperations = batchOperationTemplate.GetPendingBatchOperations<string>(testOperationId);
            Assert.DoesNotContain<BatchOperation<string>>(batchOperations, (x) => x.OperationId == testOperationId);
        }
    }
}
