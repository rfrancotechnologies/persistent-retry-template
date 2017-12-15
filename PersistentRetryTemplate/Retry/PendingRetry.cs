using System;
using System.Threading;
using LiteDB;

namespace PersistentRetryTemplate.Retry
{
    public class PendingRetry<T>
    {
        public ObjectId Id { get; set; }
        public string OperationId { get; set; }
        public T Argument { get; set; }
    }
}