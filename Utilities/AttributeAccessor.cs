using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PersistentRetryTemplate.Utilities 
{
    public class AttributeAccessor
    {
        private ConcurrentDictionary<string, object> attributes;
        
        public AttributeAccessor() 
        {
            attributes = new ConcurrentDictionary<string, object>();
        }

        public object GetAttribute(string name)
        {
            try
            {
                return attributes[name];
            }
            catch(KeyNotFoundException)
            {
                return null;
            }
        }

        public void SetAttribute(string name, object value)
        {
            attributes.AddOrUpdate(name, value, (k, v) => value);
        }
    }
}