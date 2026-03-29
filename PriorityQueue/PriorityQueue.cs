namespace PriorityQueue;

public class PriorityQueue<T> where T : IComparable<T>
{
    private int Count { get; set; } = 0;
    private readonly IList<T> _arr;

    public bool IsEmpty => Count == 0;

    /// <summary>
    /// O(N)构造
    /// </summary>
    public PriorityQueue(IList<T> arr)
    {
        _arr = arr;
        Count = arr.Count;
        for (var i = (Count - 1) >> 1; i >= 0; i--)
            Down(i);
    }
    
    /// <summary>
    /// O(NlogN)构造
    /// </summary>
    // public PriorityQueue(IList<T> arr)
    // {
    //     _arr = new List<T>(arr.Count);
    //     foreach (var item in arr)
    //         Push(item);
    // }

    private void Push(T item)
    {
        InnerAdd(item);
        Up(Count - 1, item);
    }
    
    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException("IsEmpty!");
        var ret = _arr[0];
        _arr[0] = _arr[Count - 1];
        InnerRemove();
        Down(0);
        return ret;
    }

    private void Up(int child, T item)
    {
        for (int parent = (child - 1) >> 1; parent >= 0; parent = (child - 1) >> 1)
        {
            if (_arr[parent].CompareTo(item) < 0)
                _arr[child] = _arr[parent];
            else
                break;
            child = parent;
        }
        _arr[child] = item;
    }

    private void Down(int parent)
    {
        var temp = _arr[parent];
        for (int child = (parent << 1) + 1; child < Count; child = (parent << 1) + 1)
        {
            if (child + 1 < Count && _arr[child + 1].CompareTo(_arr[child]) > 0)
                ++child;
            if (_arr[child].CompareTo(temp) > 0)
                _arr[parent] = _arr[child];
            else
                break;
            parent = child;
        }
        _arr[parent] = temp;
    }

    private void InnerAdd(T item)
    {
        _arr.Add(item);
        ++Count;
    }

    private void InnerRemove()
    {
        --Count;
    }
}