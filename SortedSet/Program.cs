using SortedSet;
var rankedList = new SortedSet.SortedSet<string>();

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