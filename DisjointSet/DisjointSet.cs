namespace DisjointSet;

public class DisjointSet<T> where T : notnull
{
    private readonly Dictionary<T, int> _elementToIndex = new();
    private readonly List<int> _parents = [];
    public int SetCount => _parents.Count(e => e < 0);

    public DisjointSet(IEnumerable<T> data)
    {
        foreach (var item in data)
        {
            var uid = _parents.Count;
            _parents.Add(-1);
            _elementToIndex[item] = uid;
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