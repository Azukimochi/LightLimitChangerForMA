using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public sealed class LightLimitChangerObjectCache
    {
        internal readonly Dictionary<Object, Object> Cache = new Dictionary<Object, Object>();
        internal readonly Dictionary<object, Texture2D> BakedTextureCache = new Dictionary<object, Texture2D>();

        internal Object Container;

        public Object this[Object key]
        {
            get => Cache[key];
            set => Register(key, value);
        }

        public T Register<T>(T key, T value) where T : Object
        {
            if (key == null || value == null)
                return null;
            if (!Cache.ContainsKey(key) && !Cache.ContainsKey(value))
            {
                Cache.Add(key, value);
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(value)))
                    AssetDatabase.AddObjectToAsset(value, Container);
            }
            return value;
        }

        public T Register<T>(T item) where T : Object => Register(item, item);

        public Texture2D RegisterBakedTexture(object key, Texture2D value)
        {
            if (key == null || value == null)
                return null;
            if (!BakedTextureCache.ContainsKey(key))
            {
                BakedTextureCache.Add(key, value);
            }
            return value;
        }

        public bool TryGetValue<T>(T key, out T value) where T : Object
        {
            bool result = Cache.TryGetValue(key, out var temp);
            value = temp as T;
            return result && temp is T;
        }

        public bool TryGetBakedTexture(object key, out Texture2D value) => BakedTextureCache.TryGetValue(key, out value);

        public bool ContainsKey<T>(T key) where T : Object => Cache.ContainsKey(key);

        public IEnumerable<Object> MappedObjects => Cache.Values;
    }
}
