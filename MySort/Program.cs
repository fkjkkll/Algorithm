using System.Diagnostics;

var random = new Random();
int range = 10000;
int distribute = 10000;
var arr = Enumerable.Range(0, range).Select(_ => random.Next(0, distribute)).ToArray();

//arr = new int[] { 5, 4, 3, 2, 1 };

Stopwatch stopwatch = new();
stopwatch.Start();

MySort.ShellSort(arr);

stopwatch.Stop();
Console.WriteLine($"执行时间: {stopwatch.ElapsedMilliseconds} ms");

if (range <= 10000)
    MySort.ShowLog(arr);



public static partial class MySort
{
    
}