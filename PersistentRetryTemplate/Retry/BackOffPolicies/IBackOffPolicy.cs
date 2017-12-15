using System;

namespace PersistentRetryTemplate.Retry.BackOffPolicies
{
    public interface IBackOffPolicy
    {
        void StartContext();

        void BackOff();
    }
}