using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DapperExtensions
{
    public class GenericDictionary
    {
        private ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

        public void AddOrUpdate<T>(string key, T value) where T : class
        {
            _dict.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        public bool TryGetValue<T>(string key, out object value) where T : class
        {
            return _dict.TryGetValue(key, out value);
        }

        public T GetValue<T>(string key) where T : class
        {
            return _dict[key] as T;
        }
    }
}
