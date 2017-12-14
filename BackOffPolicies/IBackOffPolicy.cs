using System;
using PersistentRetryTemplate.RetryPolicies;

namespace PersistentRetryTemplate.BackOffPolicies
{
    public interface IBackOffPolicy
    {
        void StartContext();

        void BackOff();
    }
}