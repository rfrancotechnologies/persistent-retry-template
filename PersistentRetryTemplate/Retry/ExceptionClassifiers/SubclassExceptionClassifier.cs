using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace PersistentRetryTemplate.Retry.ExceptionClassifiers {
    public class SubclassExceptionClassifier: IExceptionClassifier {

        public ConcurrentDictionary<Type, bool> Classified { get; set; }

        public bool DefaultValue { get; set; }

        public SubclassExceptionClassifier(): this(false)
        {
        }

        public SubclassExceptionClassifier(bool defaultValue):this(new Dictionary<Type, bool>(), defaultValue)
        {
        }

        public SubclassExceptionClassifier(Dictionary<Type, bool> typeMap, bool defaultValue) {
            this.Classified = new ConcurrentDictionary<Type, bool>(typeMap);
            this.DefaultValue = defaultValue;
        }

        public void setTypeMap(Dictionary<Type, bool> typeMap) {
            this.Classified = new ConcurrentDictionary<Type, bool>(typeMap);
        }

        public bool Classify(Exception classifiable) {
            if (classifiable == null) {
                return DefaultValue;
            }

            Type exceptionType = classifiable.GetType();
            if (Classified.ContainsKey(exceptionType)) {
                return Classified[exceptionType];
            }

            foreach (var type in Classified.Keys) {
                if (exceptionType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())) {
                    bool value = Classified[type];
                    this.Classified.TryAdd(exceptionType, value);
                    return value;
                }
            }

            return DefaultValue;
        }
    }    
}
