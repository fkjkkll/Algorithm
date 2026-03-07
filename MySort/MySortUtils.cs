public static partial class MySort
{
    public static void Swap<T>(IList<T> arr, int i1, int i2)
    {
        (arr[i2], arr[i1]) = (arr[i1], arr[i2]);
    }

    public static int PowerOfTwoFloor(int n)
    {
        n |= (n >> 1);
        n |= (n >> 2);
        n |= (n >> 4);
        n |= (n >> 8);
        n |= (n >> 16);
        return (n >> 1) + 1;
    }

    public static void ShowLog<T>(IList<T> arr)
    {
        string log = "";
        foreach (var e in arr)
            log += $"{e} ";
        Console.WriteLine(log);
    }

    public static void PriorityQueueDown<T>(IList<T> arr, int parent, int count) where T : IComparable<T>
    {
        var temp = arr[parent];
        for (int child = (parent << 1) + 1; child < count; child = (parent << 1) + 1)
        {
            if (child + 1 < count && arr[child + 1].CompareTo(arr[child]) > 0)
                ++child;
            if (arr[child].CompareTo(temp) > 0)
                arr[parent] = arr[child];
            else
                break;
            parent = child;
        }
        arr[parent] = temp;
    }
}