
// 创建简化版 SortedSet
using System.Text;

var rankedList = new SimpleSortedSet<string>();

// 添加一些测试数据
rankedList.Add("A", 3);
rankedList.Add("B", 5);
rankedList.Add("C", 11);
rankedList.Add("D", 14);
rankedList.Add("E", 19);
rankedList.Add("F", 23);
rankedList.Add("G", 25);
rankedList.Add("H", 31);
rankedList.Add("I", 39);
rankedList.Add("J", 43);

rankedList.Remove("A");
rankedList.Remove("D");
rankedList.Remove("G");
rankedList.Remove("j");

rankedList.Add("A", 3);
rankedList.Add("D", 14);
rankedList.Add("G", 25);
rankedList.Add("J", 43);

// 打印结构
rankedList.PrintStructure();
Console.WriteLine();

// 测试各种功能
Console.WriteLine("=== 功能测试 ===");

// 获取排名
Console.WriteLine($"A 的排名: {rankedList.GetReverseRank("A")}");
Console.WriteLine($"A 的逆序排名: {rankedList.GetRank("A")}");

// 获取前3名
var top3 = rankedList.GetTopN(10);
Console.WriteLine($"前10名: {string.Join(", ", top3)}");

// 按排名范围查询
var range = rankedList.GetRangeByRank(2, 4);
Console.WriteLine($"GetRangeByRank: {string.Join(", ", range)}");

// 按分数范围查询
var scoreRange = rankedList.GetRangeByScore(10, 20);
Console.WriteLine($"区间分数的人: {string.Join(", ", scoreRange)}");

// 简化的 SortedSet 实现，基于跳跃表
public class SimpleSortedSet<T> where T : IComparable<T>
{
    private class SkipListNode
    {
        public T Value { get; set; }
        public double Score { get; set; }
        public SkipListNode Backward { get; set; }  // 后退指针
        public SkipListNode[] Forward { get; set; } // 前进指针数组
        public int[] Span { get; set; }             // 跨度数组

        public SkipListNode(T value, double score, int level)
        {
            Value = value;
            Score = score;
            Forward = new SkipListNode[level];
            Span = new int[level];
            Backward = null;
        }
    }

    private const int MAX_LEVEL = 32;         // 最大层数
    private const double PROBABILITY = 0.5;   // 层数增长概率

    private SkipListNode _header;             // 头节点（纯虚拟
    private SkipListNode _tail;               // 尾节点（指向真实存在的
    private int _level;                       // 当前最大层数
    private int _count;                       // 元素数量
    private Random _random;                   // 随机数生成器
    private Dictionary<T, double> _dict;      // 字典：Value -> Score

    public SimpleSortedSet()
    {
        _random = new Random();
        _dict = new Dictionary<T, double>();
        _level = 1;
        _count = 0;

        // 初始化头节点（分数为最小值）
        _header = new SkipListNode(default, double.MinValue, MAX_LEVEL);
        _tail = null;

        // 初始化头节点的跨度
        for (int i = 0; i < MAX_LEVEL; i++)
        {
            _header.Forward[i] = null;
            _header.Span[i] = 0;
        }
    }

    public bool Add(T value, double score)
    {
        // 如果已存在，返回false（简化实现，不支持更新）
        if (_dict.ContainsKey(value))
            return false;

        SkipListNode[] update = new SkipListNode[MAX_LEVEL];
        int[] rank = new int[MAX_LEVEL];
        SkipListNode current = _header;

        // 从最高层开始查找插入位置
        for (int i = _level - 1; i >= 0; i--)
        {
            rank[i] = (i == _level - 1) ? 0 : rank[i + 1];

            while (current.Forward[i] != null &&
                  (current.Forward[i].Score < score ||
                  (current.Forward[i].Score == score &&
                   current.Forward[i].Value.CompareTo(value) < 0)))
            {
                rank[i] += current.Span[i];
                current = current.Forward[i];
            }
            update[i] = current;
        }

        // 随机生成层数
        int newLevel = RandomLevel();
        if (newLevel > _level)
        {
            for (int i = _level; i < newLevel; i++)
            {
                rank[i] = 0; // 新节点是通过虚拟head直接跳过来的，所以前面的累加span是0
                update[i] = _header;
                update[i].Span[i] = _count; // 后面会处理，新节点会进行中间截断
            }
            _level = newLevel;
        }

        // 创建新节点
        SkipListNode newNode = new SkipListNode(value, score, newLevel);

        // 更新指针和跨度
        for (int i = 0; i < newLevel; i++)
        {
            // 普通的链表插入
            newNode.Forward[i] = update[i].Forward[i];
            update[i].Forward[i] = newNode;
            // 更新跨度
            newNode.Span[i] = update[i].Span[i] - (rank[0] - rank[i]);
            update[i].Span[i] = (rank[0] - rank[i]) + 1;
        }

        // 更新更高层的跨度
        for (int i = newLevel; i < _level; i++)
            update[i].Span[i]++;

        // 更新后退指针
        newNode.Backward = (update[0] == _header) ? null : update[0];
        if (newNode.Forward[0] != null)
            newNode.Forward[0].Backward = newNode;
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

        SkipListNode[] update = new SkipListNode[MAX_LEVEL];
        SkipListNode current = _header;

        for (int i = _level - 1; i >= 0; --i)
        {
            while (current.Forward[i] != null && 
                  (current.Forward[i].Score < score ||
                  (current.Forward[i].Score == score &&
                   current.Forward[i].Value.CompareTo(value) < 0)))
            {
                current = current.Forward[i];
            }
            update[i] = current;
        }
        
        var deleteNode = update[0].Forward[0];
        if (deleteNode == null || !deleteNode.Value.Equals(value) || Math.Abs(deleteNode.Score - score) > 1e-10)
            return false;
        
        // 更新各层的指针和跨度
        for (int i = 0; i < _level; ++i)
        {
            // 普通链表删除操作
            if (update[i].Forward[i] == deleteNode)
            {
                update[i].Span[i] += (deleteNode.Span[i] - 1); // +=
                update[i].Forward[i] = deleteNode.Forward[i];
            }
            // 该层链表不包含，仅需要改变span
            else if (update[i].Span[i] > 0)
            {
                --update[i].Span[i];
            }
        }

        // 更新后退指针
        if (deleteNode.Forward[0] != null)
            deleteNode.Forward[0].Backward = deleteNode.Backward;
        else
            _tail = deleteNode.Backward;

        // 更新最高层数（循环）
        while (_level > 1 && _header.Forward[_level - 1] == null)
            --_level;

        --_count;
        _dict.Remove(value);
        return true;
    }

    // 获取元素的排名（从0开始，分数从低到高）
    public int? GetRank(T value)
    {
        if (!_dict.TryGetValue(value, out double score))
            return null;

        SkipListNode current = _header;
        int rank = 0;

        for (int i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null &&
                  (current.Forward[i].Score < score ||
                  (current.Forward[i].Score == score &&
                   current.Forward[i].Value.CompareTo(value) <= 0)))
            {
                rank += current.Span[i];
                current = current.Forward[i];
            }

            if (current.Value != null && current.Value.Equals(value))
                return rank;
        }
        return null;
    }

    // 获取逆序排名（从0开始，分数从高到低）
    public int? GetReverseRank(T value)
    {
        int? rank = GetRank(value);
        return rank.HasValue ? _count - rank.Value + 1 : null;
    }

    // 获取前N名（分数从高到低）
    public List<T> GetTopN(int n)
    {
        if (n <= 0) return new List<T>();
        if (n > _count) n = _count;

        List<T> result = new List<T>();
        SkipListNode current = _tail;

        // 从尾部向前遍历
        for (int i = 0; i < n && current != null; i++)
        {
            result.Add(current.Value);
            current = current.Backward;
        }

        return result;
    }

    // 获取分数范围内的元素（利用跳跃表特性，O(log N + M)）
    public List<T> GetRangeByScore(double minScore, double maxScore)
    {
        if (minScore > maxScore) return new List<T>();

        List<T> result = new();
        SkipListNode current = _header;

        // 步骤1：使用高层索引快速定位到第一个 >= minScore 的节点 (O(log N))
        for (int i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null && current.Forward[i].Score < minScore)
            {
                current = current.Forward[i];
            }
        }

        // 现在 current 是最后一个 < minScore 的节点，下一个就是第一个 >= minScore 的节点
        current = current.Forward[0];

        // 步骤2：从定位点开始顺序遍历，直到超过 maxScore (O(M))
        while (current != null && current.Score <= maxScore)
        {
            result.Add(current.Value);
            current = current.Forward[0];
        }

        return result;
    }

    public List<T> GetRangeByRank(int start, int end)
    {
        var ret = new List<T>();
        if (start > end)
            return ret;
        if (start <= 0) start = 1;
        if (end > _count) end = _count;
        
        var rank = _count - start + 1;
        var current = _header;
        for (int i = _level - 1; i >= 0; i--)
        {
            while (current.Forward[i] != null && current.Span[i] <= rank)
            {
                rank -= current.Span[i];
                current = current.Forward[i];
            }

            if (rank == 0)
                break;
        }

        var cnt = end - start + 1;
        while (cnt-- > 0 && current != null)
        {
            ret.Add(current.Value);
            current = current.Backward;
        }
        return ret;
    }

    // 随机生成节点层数（抛硬币算法）
    private int RandomLevel()
    {
        int level = 1;
        while (_random.NextDouble() < PROBABILITY && level < MAX_LEVEL)
            ++level;
        return level;
    }

    // 打印跳跃表结构（用于调试）
    public void PrintStructure()
    {
        Console.WriteLine($"跳跃表结构 (元素数量: {_count}, 最大层数: {_level}):");

        for (int i = _level - 1; i >= 0; i--)
        {
            StringBuilder sb = new();
            sb.Append($"Layer{i}\t");
            SkipListNode current = _header;
            while (current != null)
            {
                sb.Append($"{current.Span[i]}\t");
                if (current.Forward[i] != null)
                    sb.Append(new string('\t', current.Span[i] - 1));
                else
                    sb.Append(new string('\t', current.Span[i]));
                current = current.Forward[i];
            }
            Console.WriteLine(sb);

        }
        var cur = _header;
        string o = "Data\t";
        string s = "Score\t";
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
                s += cur.Score.ToString() + '\t';
            }
            cur = cur.Forward[0];
        }
        Console.WriteLine();
        Console.WriteLine(o);
        Console.WriteLine(s);
    }
}