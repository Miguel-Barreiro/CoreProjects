using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    public class GraphCallStack
    {
        public class Entry
        {
            public Node Caller;
        }

        protected Stack<Entry> entries;

        public GraphCallStack()
        {
            entries = new Stack<Entry>();
        }

        public void Push(Node Caller)
        {
            Entry entry = new Entry();
            entry.Caller = Caller;
            entries.Push(entry);
        }

        public Node? Pop()
        {
            Entry entry = entries.Pop();
            if (entry != null)
            {
                return entry.Caller;
            }
            return null;
        }
    }

    
}
