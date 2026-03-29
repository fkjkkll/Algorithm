using System.Diagnostics;


Console.Write("输入排序数目：");
var str = Console.ReadLine();
if (!int.TryParse(str, out var range))
    range = 100000;

var random = new Random();
int distribute = 10000;
var arr = Enumerable.Range(0, range).Select(_ => random.Next(0, distribute)).ToArray();

//arr = new int[] { 5, 4, 3, 2, 1 };

Stopwatch stopwatch = new();
stopwatch.Start();

Sort.Sort.QuickSort(arr);

stopwatch.Stop();
Console.WriteLine($"对{range}个分布在0到{distribute}的数字进行排序，执行时间: {stopwatch.ElapsedMilliseconds} ms");

if (range <= 10000)
    Sort.SortUtils.ShowLog(arr);