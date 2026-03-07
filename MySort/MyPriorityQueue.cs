public class MyPriorityQueue<T> where T : IComparable<T>
{
    private int _count { get; set; } = 0;
    private IList<T> _arr;

    public bool IsEmpty => _count == 0;

    public MyPriorityQueue(IList<T> arr)
    {
        _arr = arr;
        _count = arr.Count;
        for (int i = (_count - 1) >> 1; i >= 0; i--)
            Down(i);
    }

    public void Push(T item)
    {
        InnerAdd(item);
        Up(_count - 1, item);
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

    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException("IsEmpty!");
        var ret = _arr[0];
        _arr[0] = _arr[_count - 1];
        InnerRemove();
        Down(0);
        return ret;
    }

    private void Down(int parent)
    {
        var temp = _arr[parent];
        for (int child = (parent << 1) + 1; child < _count; child = (parent << 1) + 1)
        {
            if (child + 1 < _count && _arr[child + 1].CompareTo(_arr[child]) > 0)
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
        ++_count;
    }

    private void InnerRemove()
    {
        --_count;
    }
}