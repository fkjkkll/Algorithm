namespace SparseSet;

/// <summary>
/// 删除的时候没法compact dense的内容
/// 除非牺牲O(n)遍历
/// 需要引入额外信息帮助完成O(1)删除！
/// </summary>
public class BadSparseDict<T>
{
    private int[] _sparse;
    private T?[] _dense;
    private int _count;

    private const int None = -1;
    private const int MinimumIncrement = 4;
	
    public BadSparseDict(int sparseCap, int denseCap)
    {
        _sparse = new int[sparseCap];
        _dense = new T[denseCap];
        Array.Fill(_sparse, None);
        _count = 0;
    }

    public bool Add(int key, T value)
    {
        EnsureCapacity(key);
        if (_sparse[key] != None) return false;
        var denseIndex = _count++;
        _sparse[key] = denseIndex;
        _dense[denseIndex] = value;
        return true;
    }

    public bool Remove(int key)
    {
        if (key >= _sparse.Length || _sparse[key] == None) return false;
        _dense[_sparse[key]] = default;
        _sparse[key] = None;
        return true;
    }

    public bool TryGetValue(int key, out T? value)
    {
        if (key < _sparse.Length && _sparse[key] != None)
        {
            value = _dense[_sparse[key]];
            return true;
        }
        value = default;
        return false;
    }

    private void EnsureCapacity(int key)
    {
        if (_sparse.Length <= key)
        {
            var oldSparseCap = _sparse.Length;
            var newSparseCap = oldSparseCap;
            while (newSparseCap <= key)
                newSparseCap <<= 1;
            Array.Resize(ref _sparse, newSparseCap);
            Array.Fill(_sparse, None, oldSparseCap, newSparseCap - oldSparseCap);
        }
        if (_dense.Length <= _count)
        {
            var newDenseCap = Math.Max(_dense.Length + MinimumIncrement, _dense.Length << 1);
            Array.Resize(ref _dense, newDenseCap);
        }
    }
}
