PriorityQueue pq = new(new int[] { 34, 53, 21, 12, 5, 76, 95, 4, 32, 45 });
while (!pq.IsEmpty)
    Console.WriteLine(pq.Pop());

public class PriorityQueue
{
    private List<int> _list;

    public bool IsEmpty => _list.Count == 0;

    public PriorityQueue() { _list = new(); }

    public PriorityQueue(int[] arr)
    {
        // O(N)
        //_list = arr.ToList();
        //for (int i = (_list.Count - 1) >> 1; i >= 0; --i)
        //    Down(i);

        // O(Nlog(N))
        _list = new();
        foreach (var n in arr)
            Push(n);
    }

    public void Push(int value)
    {
        int child = _list.Count;
        _list.Add(value);
        while (child > 0)
        {
            var parent = (child - 1) >> 1;
            // 当前父亲大于插入值，找到了
            if (_list[parent] >= value)
                break;
            _list[child] = _list[parent];
            child = parent;
        }
        _list[child] = value;
    }

    public int Pop()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("IsEmpty!");
        var ret = _list[0];
        _list[0] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        if (!IsEmpty)
            Down(0);
        return ret;
    }

    private void Down(int parent)
    {
        var origin = _list[parent];
        for (int child = (parent << 1) + 1; child < _list.Count; child = (parent << 1) + 1)
        {
            if (child + 1 < _list.Count && _list[child + 1] > _list[child])
                ++child;
            if (_list[child] > origin)
                _list[parent] = _list[child];
            else
                break;
            parent = child;
        } 
        _list[parent] = origin;
    }
}