using LiteDB;

namespace PersistentRetryTemplate
{
    public class PendingRetry<T>
    {
        public ObjectId Id { get; set; }
        public string CallbackKey { get; set; }
        public T Argument { get; set; }
    }
}