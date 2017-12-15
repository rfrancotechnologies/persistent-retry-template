using System;

namespace PersistentRetryTemplate.Retry.ExceptionClassifiers
{
    public interface IExceptionClassifier
    {
        bool Classify(Exception classifiable);
    }
}