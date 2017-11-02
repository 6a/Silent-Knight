namespace PathFinding
{
    public class Heap<T> where T : IHeapable<T>
    {
        T[] m_items;
        public int Count { get; private set; }
        
        public Heap(int maxHeapSize)
        {
            m_items = new T[maxHeapSize];
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            return Equals(m_items[item.HeapIndex], item);
        }

        public T RemoveFirst()
        {
            var firstItem = m_items[0];
            Count--;

            m_items[0] = m_items[Count];
            m_items[0].HeapIndex = 0;
            SortDown(m_items[0]);
            return firstItem;
        }

        public void Add(T item)
        {
            item.HeapIndex = Count;
            m_items[Count] = item;
            SortUp(item);
            Count++;
        }

        void SortDown(T item)
        {
            while (true)
            {
                var leftChildIndex = item.HeapIndex * 2 + 1;
                var rightChildIndex = item.HeapIndex * 2 + 2;
                var swapIndex = 0;

                if (leftChildIndex < Count)
                {
                    swapIndex = leftChildIndex;

                    if(rightChildIndex < Count)
                    {
                        if (m_items[leftChildIndex].CompareTo(m_items[rightChildIndex]) < 0)
                        {
                            swapIndex = rightChildIndex;
                        }
                    }

                    if (item.CompareTo(m_items[swapIndex]) < 0)
                    {
                        Swap(item, m_items[swapIndex]); 
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        void SortUp(T item)
        {
            var parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                var parentItem = m_items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }
            }
        }

        void Swap(T itemA, T itemB)
        {
            m_items[itemA.HeapIndex] = itemB;
            m_items[itemB.HeapIndex] = itemA;

            var itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }

    }


}
