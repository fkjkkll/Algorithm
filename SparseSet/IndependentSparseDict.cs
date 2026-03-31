using System.Runtime.CompilerServices;

namespace SparseSet;

/// <summary>
/// 自增Id不由外部提供，内部自行维护
/// 相当于去商城外面的寄存柜，东西放进去，给你钥匙
/// </summary>
public class IndependentSparseDict<T>
{
    private int[] _sparse;		// key -> denseIndex
	private int[] _free;		// 收集回收key（new）
	private int[] _denseKeys;	// denseIndex -> key
	private T[] _denseValues;	// denseIndex -> value

	private int _count;				// 当前实际存储的个数
	private int _allocatedCount;	// 已经提供过的key（只增不减）（new）
	private int _freeCount;			// 当前复用池的元素个数（new）
	
	private const int None = -1;
	private const int MinimumIncrement = 4;

	public IndependentSparseDict(int sparseCap, int denseCap)
	{
		_sparse = new int[sparseCap];
		_free = new int[sparseCap];
		_denseValues = new T[denseCap];
		_denseKeys = new int[denseCap];
		Array.Fill(_sparse, None);
		_count = 0;
		_allocatedCount = 0;
		_freeCount = 0;
	}

	public int Add(T value)
	{
		var key = _freeCount > 0 ? _free[--_freeCount] : _allocatedCount++;
		EnsureCapacity(key);
		var index = _count++;
		_sparse[key] = index;
		_denseKeys[index] = key;
		_denseValues[index] = value;
		return key;
	}
	
	public bool Remove(int key)
	{
		var index = _sparse[key];
		if (index < 0) return false;

		var lastIndex = _count - 1;
		if (index != lastIndex)
		{
			var swappedKey = _denseKeys[lastIndex];
			_denseKeys[index] = swappedKey;
			_denseValues[index] = _denseValues[lastIndex];
			_sparse[swappedKey] = index;
		}

		--_count;
		_sparse[key] = None;
		_free[_freeCount++] = key;
		// 其实可以不重置
		_denseKeys[lastIndex] = None;
		_denseValues[lastIndex] = default!;
		return true;
	}
	
	public bool TryGet(int key, out T value)
	{
		if (Contains(key))
		{
			value = _denseValues[_sparse[key]];
			return true;
		}

		value = default!;
		return false;
	}

	public bool Contains(int key)
	{
		if (key < 0 || key >= _sparse.Length) return false;
		var index = _sparse[key];
		return index >= 0 && index < _count && _denseKeys[index] == key;
	}
	
	public void Clear()
	{
		for (var i = 0; i < _count; i++)
		{
			_sparse[_denseKeys[i]] = None;
			_denseValues[i] = default!;
		}
		_count = 0;
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
			Array.Resize(ref _free, newSparseCap);
			Array.Fill(_sparse, None, oldSparseCap, newSparseCap - oldSparseCap);
		}
		if (_denseKeys.Length <= _count)
		{
			var newDenseCap = Math.Max(_denseKeys.Length + MinimumIncrement, _denseKeys.Length << 1);
			Array.Resize(ref _denseKeys, newDenseCap);
			Array.Resize(ref _denseValues, newDenseCap);
		}
	}
	
	# region "遍历"
	public ReadOnlySpan<int> Keys => _denseKeys.AsSpan(0, _count);
	
	public Span<T> Values => _denseValues.AsSpan(0, _count);

	public Enumerator GetEnumerator() => new Enumerator(_denseKeys, _denseValues, _count);

	public struct Enumerator
	{
		private readonly int[] _keys;
		private readonly T[] _values;
		private readonly int _count;
		private int _index;

		public Enumerator(int[] keys, T[] values, int count)
		{
			_keys = keys;
			_values = values;
			_count = count;
			_index = -1;
		}

		public bool MoveNext()
		{
			return ++_index < _count;
		}

		// 想返回什么都可以：pair、key、value
		public (int key, T value) Current => (_keys[_index], _values[_index]);
	}
	# endregion
		
	# region "针对ECS里的大结构体"
	public int AddIn(in T value)
	{
		var key = _freeCount > 0 ? _free[--_freeCount] : _allocatedCount++;
		EnsureCapacity(key);
		var index = _count++;
		_sparse[key] = index;
		_denseKeys[index] = key;
		_denseValues[index] = value;
		return key;
	}
	
	public ref T TryGetRef(int key, out bool exists)
	{
		if (Contains(key))
		{
			exists = true;
			return ref _denseValues[_sparse[key]];
		}

		exists = false;
		return ref Unsafe.NullRef<T>();
	}
	# endregion
}