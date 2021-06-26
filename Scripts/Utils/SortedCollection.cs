using System.Collections;
using System.Collections.Generic;

namespace UnityEventBus.Utils
{
    internal class SortedCollection<T> : ICollection<T>
    {
        public  int          Count      => m_Collection.Count;
        public  bool         IsReadOnly => false;
        private List<T>      m_Collection;
        private IComparer<T> m_OrderComparer;

        //////////////////////////////////////////////////////////////////////////
        public SortedCollection(IComparer<T> orderComparer)
        {
            m_Collection = new List<T>();
            m_OrderComparer   = orderComparer;
        }
        public SortedCollection(IComparer<T> orderComparer, int size)
        {
            m_Collection = new List<T>(size);
            m_OrderComparer   = orderComparer;
        }

        public IEnumerator<T> GetEnumerator() => m_Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_Collection.GetEnumerator();

        public void Add(T item)
        {
            var index = m_Collection.FindIndex(n => m_OrderComparer.Compare(n, item) > 0);

            if (index != -1)
                m_Collection.Insert(index, item);
            else
                m_Collection.Add(item);
        }

        public void Clear() => m_Collection.Clear();

        public bool Contains(T item) => m_Collection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => m_Collection.CopyTo(array, arrayIndex);

        public bool Remove(T item) => m_Collection.Remove(item);
    }
}