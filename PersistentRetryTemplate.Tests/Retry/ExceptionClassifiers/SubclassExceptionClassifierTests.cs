using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Moq;
using PersistentRetryTemplate.Retry;
using PersistentRetryTemplate.Retry.ExceptionClassifiers;
using Xunit;

namespace PersistentRetryTemplate.Retry.RetryPolicies
{
    public class SubclassExceptionClassifierTests
    {
        public class TestParentException: Exception { }
        public class TestChildException: TestParentException { }
        public class TestGrandChildException: TestChildException { }
        public class TestOtherException: Exception { }

        [Fact]
        public void ShouldByDefaultClassifyEveryUnspecifiedExceptionAsFalse() 
        {
            SubclassExceptionClassifier classifier = new SubclassExceptionClassifier();
            Assert.False(classifier.Classify(new TestOtherException()));
        }

        [Fact]
        public void ShouldClassifyEveryUnspecifiedExceptionWithTheGivenDefaultValue() 
        {
            SubclassExceptionClassifier classifier = new SubclassExceptionClassifier(true);
            Assert.True(classifier.Classify(new TestOtherException()));
        }

        [Fact]
        public void ShouldClassifyEverySpecifiedExceptionAndTheirChildrenAsTheGivenValue() 
        {
            var specifiedExceptions = new Dictionary<Type, bool>();
            specifiedExceptions.Add(typeof(TestParentException), true);
            specifiedExceptions.Add(typeof(TestOtherException), false);
            SubclassExceptionClassifier classifier = new SubclassExceptionClassifier(specifiedExceptions, false);

            Assert.True(classifier.Classify(new TestParentException()));
            Assert.True(classifier.Classify(new TestChildException()));
            Assert.True(classifier.Classify(new TestGrandChildException()));
            Assert.False(classifier.Classify(new TestOtherException()));
        }

        [Fact]
        public void ShouldClassifyASpecifiedExceptionWithTheGivenValueEvenIfParentValueIsDifferent() 
        {
            var specifiedExceptions = new Dictionary<Type, bool>();
            specifiedExceptions.Add(typeof(TestParentException), true);
            specifiedExceptions.Add(typeof(TestGrandChildException), false);
            SubclassExceptionClassifier classifier = new SubclassExceptionClassifier(specifiedExceptions, false);

            Assert.True(classifier.Classify(new TestParentException()));
            Assert.False(classifier.Classify(new TestGrandChildException()));
        }
    }
}