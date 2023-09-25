using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEditor;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public sealed class LightLimitChangerObjectCache
    {
        internal readonly Dictionary<Object, Object> Cache = new Dictionary<Object, Object>();
        internal BuildContext Context;

        public T Register<T>(T key, T value) where T : Object
        {
            if (key == null || value == null)
                return null;
            if (!Cache.ContainsKey(key) && !Cache.ContainsKey(value))
            {
                Cache.Add(key, value);
                AssetDatabase.AddObjectToAsset(value, Context.AssetContainer);
            }
            return value;
        }

        public T Register<T>(T item) where T : Object => Register(item, item);

        public bool TryGetValue<T>(T key, out T value) where T : Object
        {
            bool result = Cache.TryGetValue(key, out var temp);
            value = temp as T;
            return result && temp is T;
        }

        public bool ContainsKey<T>(T key) where T : Object => Cache.ContainsKey(key);

        public IEnumerable<Object> MappedObjects => Cache.Values;
    }
}
