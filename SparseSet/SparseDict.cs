using System.Runtime.CompilerServices;

namespace SparseSet;

public sealed class SparseDict<T>
{
	private int[] _sparse;
	private int[] _denseKeys;
	private T[] _denseValues;

	private int _count;
	private const int None = -1;
	private const int MinimumIncrement = 4;

	public SparseDict(int sparseCap, int denseCap)
	{
		_sparse = new int[sparseCap];
		_denseValues = new T[denseCap];
		_denseKeys = new int[denseCap];
		Array.Fill(_sparse, None);
		_count = 0;
	}

	public bool Add(int key, T value)
	{
		if (Contains(key)) return false;
		EnsureCapacity(key);
		_sparse[key] = _count;
		_denseKeys[_count] = key;
		_denseValues[_count] = value;
		++_count;
		return true;
	}

	public void Set(int key, T value)
	{
		if (Contains(key))
			_denseValues[_sparse[key]] = value;
		else
			Add(key, value);
	}
	
	public bool Remove(int key)
	{
		if (!Contains(key)) return false;

		var index = _sparse[key];
		var lastIndex = _count - 1;
		var lastKey = _denseKeys[lastIndex];

		if (index != lastIndex)
		{
			// swap + 覆盖
			_denseValues[index] = _denseValues[lastIndex];
			_denseKeys[index] = lastKey;
			_sparse[lastKey] = index;
		}
		
		// 清理
		_sparse[key] = None; // reset
		_denseValues[lastIndex] = default!;
		
		--_count;
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
	public bool AddIn(int key, in T value)
	{
		if (Contains(key)) return false;
		EnsureCapacity(key);
		_sparse[key] = _count;
		_denseKeys[_count] = key;
		_denseValues[_count] = value;
		++_count;
		return true;
	}

	public void SetIn(int key, in T value)
	{
		if (Contains(key))
			_denseValues[_sparse[key]] = value;
		else
			AddIn(key, value);
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