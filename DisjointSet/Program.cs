DisjointSet<string> ds = new(["合肥","厦门","重庆","北京","上海"]);
ds.Union("北京", "上海");
Console.WriteLine(ds.IsUnion("北京","厦门"));
ds.Union("上海", "合肥");
Console.WriteLine(ds.IsUnion("北京", "厦门"));
ds.Union("厦门", "合肥");
Console.WriteLine(ds.IsUnion("北京", "厦门"));

Console.WriteLine("");

public class DisjointSet<T> where T : notnull
{
    private Dictionary<T, int> _elementToIndex = new();
    private List<int> _parents = new();
    public int SetCount => _parents.Count(e => e < 0);

    public DisjointSet(IEnumerable<T> datas)
    {
        foreach (var data in datas)
        {
            var uid = _parents.Count;
            _parents.Add(-1);
            _elementToIndex[data] = uid;
        }
    }

    private int GetIndex(T element)
    {
        if (!_elementToIndex.TryGetValue(element, out var index))
            throw new ArgumentException($"Element {element} not found in disjoint set");
        return index;
    }

    private int Find(int index)
    {
        if (_parents[index] < 0)
            return index;
        return _parents[index] = Find(_parents[index]);
    }

    public bool IsUnion(T left, T right)
    {
        return Find(GetIndex(left)) == Find(GetIndex(right));
    }

    public void Union(T left, T right)
    {
        UnionByTreeHeight(left, right);
        //UnionByTreeScale(left, right);
    }

    // 按秩归并方法一：比较树高
    private void UnionByTreeHeight(T left, T right)
    {
        var leftRoot = Find(GetIndex(left));
        var rightRoot = Find(GetIndex(right));
        if (leftRoot == rightRoot)
            return;
        // 因为是负数，所以反过来
        if (_parents[leftRoot] < _parents[rightRoot])
        {
            _parents[rightRoot] = leftRoot;
        }
        else
        {
            // 因为是负数，所以--
            if (_parents[leftRoot] == _parents[rightRoot])
                --_parents[rightRoot];
            _parents[leftRoot] = rightRoot;
        }
    }

    // 按秩归并方法二：比较树规模（个数）
    private void UnionByTreeScale(T left, T right)
    {
        var leftRoot = Find(GetIndex(left));
        var rightRoot = Find(GetIndex(right));
        if (leftRoot == rightRoot)
            return;
        // 因为是负数，所以反过来
        if (_parents[leftRoot] < _parents[rightRoot])
        {
            _parents[leftRoot] += _parents[rightRoot];
            _parents[rightRoot] = leftRoot;
        }
        else
        {
            _parents[rightRoot] += _parents[leftRoot];
            _parents[leftRoot] = rightRoot;
        }
    }
}