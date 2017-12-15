using System.Collections.Generic;
using LiteDB;

namespace PersistentRetryTemplate.BatchOperations
{
    public class BatchOperation<T>
    {
        internal BatchOperation() 
        {
            BatchData = new List<T>();
        }

        public ObjectId Id { get; set; }

        public string OperationId { get; set; }

        public List<T> BatchData { get; set; }
    }
}