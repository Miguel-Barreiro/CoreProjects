using System;
using System.Collections.Generic;

namespace Core.Utils.CachedDataStructures
{
	public sealed class CachedHashset<T> : HashSet<T>, IDisposable
    {
        public bool InCache { get; set; }

        public void Dispose()
        {
            HashsetCache<T>.Free(this);
        }

        ~CachedHashset()
        {
            // NOTE: Is this called if Dispose() is first called? I imagine so.
            // Should we add an error message if this finalizer is called before Dispose()
            // to catch incorrect usages?
            Dispose();
        }
    }

    public static class HashsetCache<T>
    {
        private readonly static List<CachedHashset<T>> freeHashsets = new();
        public static int FreeCount => freeHashsets.Count;

        private readonly static List<CachedHashset<T>> inUseHashsets = new();
        public static int InUseCount => inUseHashsets.Count;

        public static CachedHashset<T> Get()
        {
            if (freeHashsets.Count > 0)
            {
                CachedHashset<T> cachedHashset = freeHashsets[0];
                freeHashsets.RemoveAt(0);
                inUseHashsets.Add(cachedHashset);
                cachedHashset.InCache = false;
                cachedHashset.Clear();
                return cachedHashset;
            }

            CachedHashset<T> newHashset = new ();
            inUseHashsets.Add(newHashset);
            newHashset.InCache = false;
            return newHashset;
        }

        internal static void Free(CachedHashset<T> freeHashset)
        {
            if (!freeHashset.InCache)
            {
                freeHashsets.Add(freeHashset);
                freeHashset.InCache = true;
                inUseHashsets.Remove(freeHashset);
            }
        }
    }
	
}