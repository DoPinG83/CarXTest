using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Core.Utils
{
    public class ExtendedStack<T> : IEnumerable<T> where T : class
    {
        public ExtendedStack() : this(int.MaxValue) { }

        public ExtendedStack(int maxSize)
        {
            m_InternalCollection = new LinkedList<T>();
            m_MaxSize = maxSize;
        }

        readonly int m_MaxSize;
        LinkedList<T> m_InternalCollection;

        public int Count { get { return m_InternalCollection.Count; } }

        public void Push(T item)
        {
            m_InternalCollection.AddFirst(item);
            if (m_InternalCollection.Count > m_MaxSize)
                m_InternalCollection.RemoveLast();
        }

        public void Add(T item)
        {
            m_InternalCollection.AddLast(item);
            if (m_InternalCollection.Count > m_MaxSize)
                m_InternalCollection.RemoveFirst();
        }

        public void Clear()
        {
            m_InternalCollection.Clear();
        }

        public void RemoweAll(Func<T, bool> predicate = null)
        {
            if(predicate == null)
            {
                Clear();
                return;
            }
            var collectionToIterate = m_InternalCollection.ToArray();
            foreach (var e in collectionToIterate)
                if (predicate(e))
                    m_InternalCollection.Remove(e);
        }

        public void Remove(T item)
        {
            m_InternalCollection.Remove(item);
        }

        public bool Contains(T item)
        {
            return m_InternalCollection.Contains(item);
        }

        public T Peek()
        {
            return m_InternalCollection.FirstOrDefault();
        }

        public T Pop()
        {
            var item = Peek();
            m_InternalCollection.RemoveFirst();
            return item;
        }

        public T[] ToArray()
        {
            return m_InternalCollection.ToArray();
        }

        public void PushOverride(T item)
        {
            m_InternalCollection.RemoveFirst();
            m_InternalCollection.AddFirst(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)m_InternalCollection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)m_InternalCollection).GetEnumerator();
        }
    }
}