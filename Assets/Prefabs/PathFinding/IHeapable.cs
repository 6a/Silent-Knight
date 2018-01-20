namespace PathFinding
{
    /// <summary>
    /// Generic interface for a heap object.
    /// </summary>
    public interface IHeapable<T> : System.IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}
