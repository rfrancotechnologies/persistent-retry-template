using System;

namespace PersistentRetryTemplate.ExceptionClassifiers
{
    public interface IExceptionClassifier
    {
        bool Classify(Exception classifiable);
    }
}