using System;

namespace PersistentRetryTemplate
{
    public class RetryTemplate
    {
        private IRetryPolicy _retryPolicy;

        public RetryTemplate(string databasePath) {

        }

        public IRetryPolicy RetryPolicy 
        {
            set
            {
                this._retryPolicy = value;
            }
        }
    }
}
