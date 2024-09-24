using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace io.github.azukimochi;

internal ref struct ValueDictionary<TKey, TValue>
{
    private Entry[] entries;
    private int[] buckets;
    private int count;

    public readonly ReadOnlySpan<Entry> Entries => entries.AsSpan(0, count);

    public bool TryAdd(TKey key, TValue value)
    {
        ref var entry = ref FindEntry(key);
        if (Unsafe.IsNullRef(ref entry)) 
            return false;
        AddInternal(key) = value;
        return true;
    }

    public ref TValue GetOrAdd(TKey key)
    {
        ref var entry = ref FindEntry(key);
        if (!Unsafe.IsNullRef(ref entry))
            return ref entry.Value;
        return ref AddInternal(key);
    }

    public readonly ref TValue TryGetValue(TKey key)
    {
        ref var entry = ref FindEntry(key);
        if (Unsafe.IsNullRef(ref entry))
            return ref Unsafe.NullRef<TValue>();
        return ref entry.Value;
    }

    private ref TValue AddInternal(TKey key)
    {
        if (buckets is null)
            Initialize();

        if (count >= buckets.Length)
        {
            ResizeAndRehash();
        }

        int hash = typeof(TKey).IsValueType ? key.GetHashCode() : EqualityComparer<TKey>.Default.GetHashCode(key);
        ref Entry entry = ref Unsafe.NullRef<Entry>();

        int index = GetBucketIndex(hash);

        if (buckets[index] == 0)
        {
            buckets[index] = count + 1;
        }
        else
        {
            entry = ref entries[buckets[index] - 1];
            while(entry.Next != 0)
            {
                entry = ref entries[entry.Next - 1];
            }
            entry.Next = count + 1;
        }
        entry = ref entries[count];
        entry.Key = key;
        entry.Hash = hash;
        entry.Next = 0;
        count++;

        return ref entry.Value;
    }

    private void Initialize()
    {
        entries = ArrayPool<Entry>.Shared.Rent(32);
        buckets = ArrayPool<int>.Shared.Rent(32);
        buckets.AsSpan().Clear();
    }

    private void ResizeAndRehash()
    {
        var size = entries.Length + 1;
        Resize(ref entries, size);
        Resize(ref buckets, size, false);
        buckets.AsSpan().Clear();

        for (int i = 0; i < entries.Length; i++)
        {
            ref var entry = ref entries[i];
            entry.Next = 0;
            int idx = GetBucketIndex(entry.Hash);
            ref var bucket = ref buckets[idx];
            if (bucket == 0)
            {
                bucket = i + 1;
            }
            else
            {
                entry = ref entries[bucket - 1];
                while(entry.Next != 0)
                {
                    entry = ref entries[entry.Next];
                }
                entry.Next = i + 1;
            }
        }
    }

    private readonly int GetBucketIndex(int hash) => hash & (buckets.Length - 1);

    private readonly ref Entry FindEntry(TKey key)
    {
        if (buckets == null)
            goto NotFound;

        int hash = typeof(TKey).IsValueType ? key.GetHashCode() : EqualityComparer<TKey>.Default.GetHashCode(key);
        int index = buckets[GetBucketIndex(hash)] - 1;
        ref var e = ref MemoryMarshal.GetReference(entries.AsSpan());

        while ((uint)index < entries.Length)
        {
            ref var entry = ref Unsafe.Add(ref e, index);
            if (!Unsafe.IsNullRef(ref entry) && entry.Hash == hash)
            {
                return ref entry;
            }
            index = entry.Next - 1;
        }

        NotFound:
        return ref Unsafe.NullRef<Entry>();
    }

    public void Dispose()
    {
        if (entries is not null)
        {
            ArrayPool<Entry>.Shared.Return(entries);
        }
        if (buckets is not null)
        {
            ArrayPool<int>.Shared.Return(buckets);
        }
    }

    private static void Resize<T>(ref T[] array, int size, bool copy = true)
    {
        var newArray = ArrayPool<T>.Shared.Rent(size);
        if (array is not null && copy)
        {
            array.AsSpan().CopyTo(newArray);
            ArrayPool<T>.Shared.Return(array);
        }
        array = newArray;
    }

    public struct Entry
    {
        public TKey Key;
        public TValue Value;
        public int Hash;
        public int Next;
    }
}