using System;
using PersistentRetryTemplate.Retry;

namespace PersistentRetryTemplate.BackOff
{
    public interface IBackOffPolicy
    {
        BackOffContext Start(RetryContext context);

        void BackOff(BackOffContext backOffContext);
    }
}