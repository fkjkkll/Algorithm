using System.Collections;
namespace SparseSet;

public class SparseSet
{
    private int[] _sparse;
    private int[] _dense;
    private int _count;
    
    private const int MinimumIncrement = 4;
    
    public SparseSet(int sparseCap, int denseCap)
    {
        _sparse = new int[sparseCap];
        _dense = new int[denseCap];
        _count = 0;
    }

    public bool Contains(int key)
    {
        return key < _sparse.Length && _sparse[key] < _count &&  _dense[_sparse[key]] == key;
    }

    public bool Add(int key)
    {
        if (Contains(key)) return false;
        EnsureCapacity(key);
        var index = _count++;
        _sparse[key] = index;
        _dense[index] = key;
        return true;
    }

    public bool Remove(int key)
    {
        if (!Contains(key)) return false;
        var index = _sparse[key];
        var lastKey = _dense[--_count];
        _sparse[lastKey] = index;
        _dense[index] = lastKey;
        return true;
    }

    public void Clear() => _count = 0;

    private void EnsureCapacity(int key)
    {
        if (_sparse.Length <= key)
        {
            var newSparseCap = _sparse.Length;
            while (newSparseCap <= key)
                newSparseCap <<= 1;
            Array.Resize(ref _sparse, newSparseCap);
        }
        if (_dense.Length <= _count)
        {
            var newDenseCap = Math.Max(_dense.Length + MinimumIncrement, _dense.Length << 1);
            Array.Resize(ref _dense, newDenseCap);
        }
    }

    /// <summary>
    /// foreach要求类型实现接口<see cref="IEnumerator{T}"/>或者<see cref="IEnumerator"/>
    /// 或者有一个公开的<see cref="GetEnumerator"/>方法，返回的类型满足鸭子类型要求
    /// 要求该鸭子类型内部包含公开的Current属性和MoveNext方法
    /// （也就是说，下面的<see cref="Enumerator"/>不需要实现接口，同时可以删掉其他函数）
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(this);
    
    // 结构体，无GC
    public struct Enumerator : IEnumerator<int>
    {
        private SparseSet _self;
        private int _index;

        public Enumerator(SparseSet self)
        {
            _self = self;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_index + 1 < _self._count)
            {
                Current = _self._dense[++_index];
                return true;
            }
            Current = 0;
            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public int Current { get; private set; }

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            _self = null!;
        }
    }
}