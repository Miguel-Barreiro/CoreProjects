using System;
using System.Collections.Generic;

namespace Core.Utils.CachedDataStructures
{
    public class CachedList<T> : List<T>, IDisposable
    {
#if DEBUG_BUILD              
        public string UsageStackTrace;
#endif
        
        public bool InCache { get; set; }
        public void Dispose()
        {
            ListCache<T>.Free(this);
        }
        ~CachedList()
        {
            // NOTE: Is this called if Dispose() is first called? I imagine so.
            // Should we add an error message if this finalizer is called before Dispose()
            // to catch incorrect usages?
            Dispose();
        }
    }

    
#if DEBUG_BUILD
    public static class CachedListDebug
    {
        public static bool KeepStackTraces { get; set; } = false;
    }
#endif


    public static class ListCache<T>
    {
        
        private static List<CachedList<T>> freeLists = new List<CachedList<T>>();
        public static int FreeCount => freeLists.Count;

        private static List<CachedList<T>> inUseLists = new List<CachedList<T>>();
        public static int InUseCount => inUseLists.Count;
#if DEBUG_BUILD
        public static string StackTraces { get => GetStackTraces(); }
        private static string GetStackTraces()
        {
            string stackTraces = "";
            foreach (CachedList<T> list in inUseLists)
            {
                if (list.UsageStackTrace != null)
                {
                    stackTraces += list.UsageStackTrace + "\n----------------------------------------------\n\n";
                }

            }
            return stackTraces;
        }
#endif
        

        public static CachedList<T> Get(IEnumerable<T> values)
        {
            CachedList<T> newList = Get();
            newList.AddRange(values);
            return newList;
        }
        
        
        public static CachedList<T> Get()
        {
            while (freeLists.Count > 0)
            {
                CachedList<T> getList = freeLists[0];
                freeLists.RemoveAt(0);
                if(getList == null)
                {
                    continue;
                }
                inUseLists.Add(getList);
                getList.InCache = false;
                getList.Clear();
#if DEBUG_BUILD
                if (CachedListDebug.KeepStackTraces)
                {
                    getList.UsageStackTrace = Environment.StackTrace;
                }
#endif                
                return getList;
            }

            CachedList<T> newList = new CachedList<T>();
            inUseLists.Add(newList);
            newList.InCache = false;
#if DEBUG_BUILD
            if (CachedListDebug.KeepStackTraces)
            {
                newList.UsageStackTrace = Environment.StackTrace;
            }
#endif                
            return newList;
        }

        internal static void Free(CachedList<T> freeList)
        {
            if (freeList != null && !freeList.InCache)
            {
                freeLists.Add(freeList);
                freeList.InCache = true;
#if DEBUG_BUILD                
                freeList.UsageStackTrace = null;
#endif                
                // VERY IMPORTANT: this line was added to make sure to not have leaked content inside lists that are considered free in the pool, but when this happened
                // issues became to appear. The reason is that we had some code that was counting on the fact that a freed list still had its content so another reference to
                // the list was actually using it. This happened, for example, in StatusEffectUtils.GetFilteredStatusEffectsForApply where the getter list used for
                // 'db.GetAllSources(trigger, getterList);' was a cached list that was then assigned to statusEffectsBeforeFiltering and its content was used outside
                // of the scope of the original list. This kind of prectices should be absolutely avoided, since they relied on a bug to make the code work
                freeList.Clear();
                inUseLists.Remove(freeList);
            }
        }
    }
}
