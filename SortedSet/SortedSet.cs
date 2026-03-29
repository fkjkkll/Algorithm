using System.Globalization;
using System.Text;
namespace SortedSet;

/// <summary>
/// 简化的 SortedSet 实现，基于跳跃表
/// </summary>
public class SortedSet<T> where T : IComparable<T>
{
    private class SkipListNode(T? value, float score, int level)
    {
        public T? Value = value;
        public readonly float Score = score;
        public SkipListNode? Backward;                                          // 后退指针
        public readonly SkipListNode?[] Forward = new SkipListNode[level];      // 前进指针数组
        public readonly int[] Span = new int[level];                            // 跨度数组
    }

    private const float Epsilon = (float)1e-6;
    private const int MaxLevel = 32;                    // 最大层数
    private const double Probability = 0.5;             // 层数增长概率
    private readonly SkipListNode _header;              // 头节点（纯虚拟
    private SkipListNode? _tail;                        // 尾节点（指向真实存在的
    private int _level;                                 // 当前最大层数
    private int _count;                                 // 元素数量
    private readonly Random _random;                    // 随机数生成器
    private readonly Dictionary<T, float> _dict;       // 字典：Value -> Score

    // 避免每次new产生GC问题
    private readonly SkipListNode[] _tempUpdate = new SkipListNode[MaxLevel];   // 代表要处理的节点在每一层的前置节点
    private readonly int[] _tempRank = new int[MaxLevel];                       // 代表要处理的节点在每一层的前置节点的累加span

    public SortedSet()
    {
        _random = new Random();
        _dict = new Dictionary<T, float>();
        _level = 1;
        _count = 0;

        // 初始化头节点（分数为最小值）
        _header = new SkipListNode(default, float.MinValue, MaxLevel);
        _tail = null;

        // 初始化头节点的跨度
        for (var i = 0; i < MaxLevel; i++)
        {
            _header.Forward[i] = null;
            _header.Span[i] = 0;
        }
    }

    public bool Add(T value, float score)
    {
        // 如果已存在，返回false（简化实现，不支持更新）
        if (_dict.ContainsKey(value))
            return false;
        
        Array.Fill(_tempUpdate, null);
        Array.Fill(_tempRank, 0);
        
        var current = _header;

        // 从最高层开始查找插入位置
        for (var i = _level - 1; i >= 0; i--)
        {
            _tempRank[i] = (i == _level - 1) ? 0 : _tempRank[i + 1];

            while (current.Forward[i] != null &&
                  (current.Forward[i]!.Score < score ||
                  (Math.Abs(current.Forward[i]!.Score - score) < Epsilon &&
                   current.Forward[i]!.Value!.CompareTo(value) < 0)))
            {
                _tempRank[i] += current.Span[i];
                current = current.Forward[i]!;
            }
            _tempUpdate[i] = current;
        }

        // 随机生成层数
        var newLevel = RandomLevel();
        if (newLevel > _level)
        {
            for (var i = _level; i < newLevel; i++)
            {
                _tempRank[i] = 0;                   // 新节点是通过虚拟head直接跳过来的，所以前面的累加span是0
                _tempUpdate[i] = _header;
                _tempUpdate[i].Span[i] = _count;    // 后面会处理，新节点会进行中间截断
            }
            _level = newLevel;
        }

        // 创建新节点
        var newNode = new SkipListNode(value, score, newLevel);

        // 更新指针和跨度
        for (var i = 0; i < newLevel; i++)
        {
            // 普通的链表插入
            newNode.Forward[i] = _tempUpdate[i].Forward[i];
            _tempUpdate[i].Forward[i] = newNode;
            // 更新跨度
            newNode.Span[i] = _tempUpdate[i].Span[i] - (_tempRank[0] - _tempRank[i]);   // 更新本体span
            _tempUpdate[i].Span[i] = (_tempRank[0] - _tempRank[i]) + 1;                 // 更新前置节点span
        }

        // 更新更高层的跨度
        for (var i = newLevel; i < _level; i++)
            _tempUpdate[i].Span[i]++;

        // 更新后退指针
        newNode.Backward = (_tempUpdate[0] == _header) ? null : _tempUpdate[0];
        if (newNode.Forward[0] != null)
            newNode.Forward[0]!.Backward = newNode;
        else
            _tail = newNode; // 如果是最后一个节点，更新尾指针

        _count++;
        _dict[value] = score;
        return true;
    }

    public bool Remove(T value)
    {
        if (!_dict.TryGetValue(value, out var score))
            return false;
        
        Array.Fill(_tempUpdate, null);

        var current = _header;
        for (var i = _level - 1; i >= 0; --i)
        {
            while (current.Forward[i] != null && 
                  (current.Forward[i]!.Score < score ||
                  (Math.Abs(current.Forward[i]!.Score - score) < Epsilon &&
                   current.Forward[i]!.Value!.CompareTo(value) < 0)))
            {
                current = current.Forward[i]!;
            }
            _tempUpdate[i] = current;
        }
        
        var deleteNode = _tempUpdate[0].Forward[0];
        if (deleteNode == null || !deleteNode.Value!.Equals(value) || Math.Abs(deleteNode.Score - score) > Epsilon)
            return false;
        
        // 更新各层的指针和跨度
        for (var i = 0; i < _level; ++i)
        {
            // 普通链表删除操作
            if (_tempUpdate[i].Forward[i] == deleteNode)
            {
                _tempUpdate[i].Span[i] += (deleteNode.Span[i] - 1); // +=
                _tempUpdate[i].Forward[i] = deleteNode.Forward[i];
            }
            // 该层链表不包含，仅需要改变span
            else if (_tempUpdate[i].Span[i] > 0)
            {
                --_tempUpdate[i].Span[i];
            }
        }

        // 更新后退指针
        if (deleteNode.Forward[0] != null)
            deleteNode.Forward[0]!.Backward = deleteNode.Backward;
        else
            _tail = deleteNode.Backward;

        // 更新最高层数（循环）
        while (_level > 1 && _header.Forward[_level - 1] == null) --_level;

        --_count;
        _dict.Remove(value);
        return true;
    }

    // 获取元素的排名（从0开始，分数从低到高）
    public int? GetRank(T value)
    {
        if (!_dict.TryGetValue(value, out var score))
            return null;

        var current = _header;
        var rank = 0;
        for (var i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null &&
                  (current.Forward[i]!.Score < score ||
                  (Math.Abs(current.Forward[i]!.Score - score) < Epsilon &&
                   current.Forward[i]!.Value!.CompareTo(value) <= 0)))
            {
                rank += current.Span[i];
                current = current.Forward[i]!;
            }

            if (current.Value != null && current.Value.Equals(value))
                return rank;
        }
        return null;
    }

    // 获取逆序排名（从0开始，分数从高到低）
    public int? GetReverseRank(T value)
    {
        var rank = GetRank(value);
        return _count - rank + 1;
    }

    // 获取前N名（分数从高到低）
    public void GetTopN(int n, List<T> outList)
    {
        outList.Clear();
        if (n <= 0) return;
        if (n > _count) n = _count;
        var current = _tail;

        // 从尾部向前遍历
        for (var i = 0; i < n && current != null; i++)
        {
            outList.Add(current.Value!);
            current = current.Backward;
        }
    }

    // 获取分数范围内的元素（利用跳跃表特性，O(log N + M)）
    public void GetRangeByScore(double minScore, double maxScore, List<T> outList)
    {
        outList.Clear();
        if (minScore > maxScore) return;
        var current = _header;

        // 步骤1：使用高层索引快速定位到第一个 >= minScore 的节点 (O(log N))
        for (var i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null && current.Forward[i]!.Score < minScore)
            {
                current = current.Forward[i]!;
            }
        }

        // 现在 current 是最后一个 < minScore 的节点，下一个就是第一个 >= minScore 的节点
        current = current.Forward[0];

        // 步骤2：从定位点开始顺序遍历，直到超过 maxScore (O(M))
        while (current != null && current.Score <= maxScore)
        {
            outList.Add(current.Value!);
            current = current.Forward[0];
        }
    }

    public void GetRangeByRank(int start, int end, List<T> outList)
    {
        outList.Clear();
        if (start > end) return;
        if (start <= 0) start = 1;
        if (end > _count) end = _count;
        
        var rank = _count - start + 1;
        var current = _header;
        for (var i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null && current.Span[i] <= rank)
            {
                rank -= current.Span[i];
                current = current.Forward[i]!;
            }

            if (rank == 0)
                break;
        }

        var cnt = end - start + 1;
        while (cnt-- > 0 && current != null)
        {
            outList.Add(current.Value!);
            current = current.Backward;
        }
    }

    // 随机生成节点层数（抛硬币算法）
    private int RandomLevel()
    {
        var level = 1;
        while (_random.NextDouble() < Probability && level < MaxLevel) ++level;
        return level;
    }

    // 打印跳跃表结构（用于调试）
    public void PrintStructure()
    {
        Console.WriteLine($"跳跃表结构 (元素数量: {_count}, 最大层数: {_level}):");

        for (var i = _level - 1; i >= 0; i--)
        {
            StringBuilder sb = new();
            sb.Append($"Layer{i}\t");
            var current = _header;
            while (current != null)
            {
                sb.Append($"{current.Span[i]}\t");
                sb.Append(current.Forward[i] != null
                    ? new string('\t', current.Span[i] - 1)
                    : new string('\t', current.Span[i]));
                current = current.Forward[i];
            }
            Console.WriteLine(sb);

        }
        var cur = _header;
        var o = "Data\t";
        var s = "Score\t";
        while (cur != null)
        {
            if (cur.Value == null)
            {
                o += "Head\t";
                s += "Head\t";
            }
            else
            {
                o += cur.Value.ToString() + '\t';
                s += cur.Score.ToString(CultureInfo.InvariantCulture) + '\t';
            }
            cur = cur.Forward[0];
        }
        Console.WriteLine();
        Console.WriteLine(o);
        Console.WriteLine(s);
    }
}