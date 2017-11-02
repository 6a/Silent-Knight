namespace PathFinding
{
    public interface IHeapable<T> : System.IComparable<T>
    {
        int HeapIndex { get; set; }

    }
}
