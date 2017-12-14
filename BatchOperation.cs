using System.Collections.Generic;
using LiteDB;

namespace PersistentRetryTemplate
{
    public class BatchOperation<T>
    {
        public ObjectId Id { get; set; }

        public string OperationId { get; set; }

        public List<T> BatchData { get; set; }
    }
}