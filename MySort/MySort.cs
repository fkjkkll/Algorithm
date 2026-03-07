public static partial class MySort
{
    // 冒泡排序
    public static void BubbleSort<T>(IList<T> arr) where T : IComparable<T>
    {
        for (int last = arr.Count - 1; last > 0; --last)
        {
            var end = true;
            for (int i = 0; i < last; ++i)
            {
                if (arr[i].CompareTo(arr[i + 1]) > 0)
                {
                    Swap(arr, i, i + 1);
                    end = false;
                }
            }
            if (end)
                break;
        }
    }

    // 插入排序
    public static void InsertSort<T>(IList<T> arr) where T : IComparable<T>
    {
        for (int i = 1; i < arr.Count; ++i)
        {
            var pre = i - 1;
            var origin = arr[i];
            for (; pre >= 0; --pre)
            {
                if (arr[pre].CompareTo(origin) > 0)
                    arr[pre + 1] = arr[pre];
                else
                    break;
            }
            arr[pre + 1] = origin;
        }
    }

    // 插入排序with区间
    public static void InsertSort<T>(IList<T> arr, int l, int r) where T : IComparable<T>
    {
        for (int i = l + 1; i <= r; ++i)
        {
            var pre = i - 1;
            var origin = arr[i];
            for (; pre >= l; --pre)
            {
                if (arr[pre].CompareTo(origin) > 0)
                    arr[pre + 1] = arr[pre];
                else
                    break;
            }
            arr[pre + 1] = origin;
        }
    }

    // 二分插入排序
    public static void BinaryInsertSort<T>(IList<T> arr, int l, int r) where T : IComparable<T>
    {
        for (int i = l + 1; i <= r; ++i)
        {
            var origin = arr[i];
            int bl = l, br = i - 1;
            while (bl <= br)
            {
                int bm = (bl + br) >> 1;
                if (arr[bm].CompareTo(origin) > 0)
                    br = bm - 1;
                else
                    bl = bm + 1;
            }
            for (int t = i; t > bl; --t)
                arr[t] = arr[t - 1];
            arr[bl] = origin;
        }
    }

    // 希尔排序
    public static void ShellSort<T>(IList<T> arr) where T : IComparable<T>
    {
        int power = PowerOfTwoFloor(arr.Count);
        for (int delta = power - 1; delta >= 1; delta = power - 1)
        {
            for (int cur = delta; cur < arr.Count; ++cur)
            {
                var temp = arr[cur];
                var pre = cur - delta;
                for (; pre >= 0; pre -= delta)
                {
                    if (arr[pre].CompareTo(temp) > 0)
                        arr[pre + delta] = arr[pre];
                    else
                        break;
                }
                arr[pre + delta] = temp;
            }
            power >>= 1;
        }
    }

    // 选择排序
    public static void SelectSort<T>(IList<T> arr) where T : IComparable<T>
    {
        for (int i = 0; i < arr.Count - 1; ++i)
        {
            var minPosition = i;
            for (int j = i; j < arr.Count; ++j)
            {
                if (arr[j].CompareTo(arr[minPosition]) < 0)
                    minPosition = j;
            }
            if (i != minPosition)
                Swap(arr, i, minPosition);
        }
    }

    // 堆排序
    public static void HeapSort<T>(IList<T> arr) where T : IComparable<T>
    {
        int count = arr.Count;
        for (int subRoot = (arr.Count - 2) >> 1; subRoot >= 0; --subRoot)
            PriorityQueueDown(arr, subRoot, count);
        for (int left = arr.Count - 1; left > 0; --left)
        {
            Swap(arr, 0, left);
            PriorityQueueDown(arr, 0, left);
        }
    }

    // 归并排序
    public static void MergeSort<T>(IList<T> arr) where T : IComparable<T>
    {
        List<T> tempList = new(arr);
        //DoMergeSortRecursive(arr, tempList, 0, arr.Count - 1);
        DoMergeSortIterative(arr, tempList);
    }

    private static void DoMergeSortRecursive<T>(IList<T> arr, IList<T> tempList, int l, int r) where T : IComparable<T>
    {
        if (l >= r)
            return;
        int m = (l + r) >> 1;
        DoMergeSortRecursive(arr, tempList, l, m);
        DoMergeSortRecursive(arr, tempList, m + 1, r);
        Merge(arr, tempList, l, m + 1, r);
        for (int i = l; i <= r; i++)
            arr[i] = tempList[i];
    }

    // 公用merge
    private static void Merge<T>(IList<T> arr, IList<T> tempList, int lStart, int rStart, int rEnd) where T : IComparable<T>
    {
        var lEnd = rStart - 1;
        var indexT = lStart;
        while (lStart <= lEnd && rStart <= rEnd)
        {
            if (arr[lStart].CompareTo(arr[rStart]) <= 0) tempList[indexT++] = arr[lStart++];
            else tempList[indexT++] = arr[rStart++];
        }
        while (lStart <= lEnd) tempList[indexT++] = arr[lStart++];
        while (rStart <= rEnd) tempList[indexT++] = arr[rStart++];
    }

    // 这一步最是精髓
    private static void DoMergeSortIterative<T>(IList<T> arr, IList<T> tempList) where T : IComparable<T>
    {
        int mergeSize = 1;
        while (mergeSize < arr.Count)
        {
            DoMergeOnce(arr, tempList, mergeSize);
            mergeSize <<= 1;
            DoMergeOnce(tempList, arr, mergeSize);
            mergeSize <<= 1;
        }
    }

    private static void DoMergeOnce<T>(IList<T> arr, IList<T> tempList, int mergeSize) where T : IComparable<T>
    {
        int lStart = 0;
        for (; lStart + (mergeSize << 1) - 1 < arr.Count; lStart += mergeSize << 1)
        {
            int rStart = lStart + mergeSize;
            int rEnd = rStart + mergeSize - 1;
            Merge(arr, tempList, lStart, rStart, rEnd);
        }
        if (lStart + mergeSize < arr.Count)
            Merge(arr, tempList, lStart, lStart + mergeSize, arr.Count - 1);
        else if (lStart < arr.Count)
            Merge(arr, tempList, lStart, arr.Count, arr.Count - 1);
    }

    // 快速排序
    public static void QuickSort<T>(IList<T> arr) where T : IComparable<T>
    {
        //DoQuickSort_1(arr, 0, arr.Count - 1);
        //DoQuickSort_Scratch(arr, 0, arr.Count - 1);
        DoQuickSort_Three(arr, 0, arr.Count - 1);
    }

    // 快速排序（自己随便实现的）
    private static void DoQuickSort_1<T>(IList<T> arr, int l, int r) where T : IComparable<T>
    {
        if (r - l < 16)
        {
            InsertSort(arr, l, r);
            return;
        }
        var pivot = arr[l];
        int indexL = l + 1, indexR = r;
        while (indexL < indexR)
        {
            bool moved = false;
            while (indexL < indexR && arr[indexL].CompareTo(pivot) < 0)
            {
                ++indexL;
                moved = true;
            }
            while (indexL < indexR && arr[indexR].CompareTo(pivot) > 0)
            {
                --indexR;
                moved = true;
            }
            Swap(arr, indexL, indexR);
            if (!moved)
            {
                ++indexL;
                --indexR;
            }
        }
        if (arr[indexL].CompareTo(pivot) >= 0)
            --indexL;
        Swap(arr, l, indexL);
        DoQuickSort_1(arr, l, indexL - 1);
        DoQuickSort_1(arr, indexL + 1, r);
    }

    // 快速排序（基础版）
    private static void DoQuickSort_Scratch<T>(IList<T> arr, int l, int r) where T : IComparable<T>
    {
        if (r - l < 16)
        {
            InsertSort(arr, l, r);
            return;
        }
        var pivot = arr[l];
        int indexL = l, indexR = r + 1;
        while (true)
        {
            // 因为左侧有哨兵，必须右侧先移动
            while (arr[--indexR].CompareTo(pivot) > 0) ; // 左侧枢纽作为哨兵，所以这里不需要判断指针大小
            while (indexL < indexR && arr[++indexL].CompareTo(pivot) < 0) ; // 右侧由于没有哨兵，所以需要判断指针大小
            if (indexL < indexR)
                Swap(arr, indexL, indexR);
            else
                break;
        }
        Swap(arr, l, indexL);
        DoQuickSort_Scratch(arr, l, indexL - 1);
        DoQuickSort_Scratch(arr, indexL + 1, r);
    }

    // 枢纽三选一中位数
    private static void DoQuickSort_Three<T>(IList<T> arr, int l, int r) where T : IComparable<T>
    {
        // 必须进行截断，否则只有两个元素，下面的代码indexR必会越界
        if (r - l < 16)
        {
            InsertSort(arr, l, r);
            return;
        }
        int m = (l + r) >> 1;
        if (arr[l].CompareTo(arr[m]) > 0)
            Swap(arr, l, m);
        if (arr[m].CompareTo(arr[r]) > 0)
            Swap(arr, m, r);
        if (arr[l].CompareTo(arr[m]) > 0)
            Swap(arr, l, m);
        Swap(arr, m, r - 1); // 锚点暂存右边第二位，这样即使只有两个元素，也是完全正确的
        var pivot = arr[r - 1];
        int indexL = l, indexR = r - 1;
        while (true)
        {
            // 下面的顺序无所谓
            while (arr[++indexL].CompareTo(pivot) < 0) ; // 左右侧都有哨兵，可以消去判断指针大小的语句
            while (arr[--indexR].CompareTo(pivot) > 0) ; // 左右侧都有哨兵，可以消去判断指针大小的语句
            if (indexL < indexR)
                Swap(arr, indexL, indexR);
            else
                break;
        }
        // 需要将数换回来，由于锚点在右侧，所以需要一个大于等于锚点的值，indexL满足；indexR是小于等于锚点的值，不满足
        Swap(arr, indexL, r - 1);
        DoQuickSort_Three(arr, l, indexL - 1);
        DoQuickSort_Three(arr, indexL + 1, r);
    }

    // 基数排序
    public static void RadixSort(IList<int> arr)
    {
        List<List<int>> buckets = Enumerable.Range(0, 16).Select(_ => new List<int>()).ToList();
        uint mask = 0x0f;
        int shiftCount = 0;
        while (mask > 0)
        {
            foreach (var item in arr)
            {
                var index = (int)((item & mask) >> shiftCount);
                buckets[index].Add(item);
            }

            if (buckets[0].Count == arr.Count)
                break;

            var iterator = buckets.SelectMany(e => e);
            var i = 0;
            foreach (var item in iterator)
                arr[i++] = item;
            foreach (var bucket in buckets)
                bucket.Clear();
            mask <<= 4;
            shiftCount += 4;
        }
    }

    // 基数排序（计数优化）
    public static void RadixCountSort(IList<int> arr)
    {
        List<int> tempList = new(arr);
        uint mask = 0xf;
        int shiftCount = 0;
        while (mask > 0)
        {
            List<int> counting = Enumerable.Repeat(0, 16).ToList();
            foreach (var item in arr)
            {
                var index = (int)((item & mask) >> shiftCount);
                ++counting[index];
            }
            if (counting[0] == arr.Count)
                break;
            for (int i = 1; i < counting.Count; ++i)
                counting[i] += counting[i - 1];

            // 错误！因为arr继承自上一次低基排序的顺序
            // counting的计数又是默认从后往前
            // 因此，对本次基数的排序必须也是从后往前，这样才能保持低基的顺序不会被打乱
            // 它不是稳定不稳定的问题，而是排序会完全错误
            //foreach (var item in arr)
            //{
            //    var index = (int)((item & mask) >> shiftCount);
            //    tempList[--counting[index]] = item;
            //}

            // 必须是从后往前
            for (int i = arr.Count - 1; i >= 0; --i)
            {
                var index = (int)((arr[i] & mask) >> shiftCount);
                tempList[--counting[index]] = arr[i];
            }


            for (int i = 0; i < arr.Count; ++i)
                arr[i] = tempList[i];
            shiftCount += 4;
            mask <<= 4;
        }
    }

    // 基数排序（最高位优先）
    public static void MSD(IList<int> arr)
    {
        List<int> trr = new(arr);
        var maxItem = int.MinValue;
        foreach (var item in trr)
            maxItem = Math.Max(maxItem, item);
        int shiftCount = 0;
        uint mask = 0x0000000f;
        while (maxItem > mask)
        {
            shiftCount += 4;
            maxItem >>= 4;
        }
        mask <<= shiftCount; // 得到最大的遮罩
        DoMSD(arr, trr, 0, arr.Count - 1, mask);
    }

    // 获得遮罩的额外位移次数
    private static int ShiftCount(uint mask)
    {
        var single = mask & -mask; // 可以快速获取二进制最低位的1
        int shiftCount = 0;
        while (single > 0)
        {
            ++shiftCount;
            single >>= 1;
        }
        return shiftCount - 1;
    }

    private static void DoMSD(IList<int> arr, IList<int> trr, int l, int r, uint mask)
    {
        if (l >= r || mask == 0)
            return;

        var counting = Enumerable.Repeat(0, 16).ToList();
        var shiftCount = ShiftCount(mask);
        // l~r计数
        for (int i = l; i <= r; ++i)
        {
            var index = (int)((arr[i] & mask) >> shiftCount);
            ++counting[index];

        }
        for (int i = 1; i < counting.Count; ++i)
            counting[i] += counting[i - 1];

        /*
         * 从前往后，虽然不至于像LSD那样直接排序错误，但是会导致排序不稳定。之所以不会排序错误，
         * 是因为MSD是先序遍历的递归，每一层处理时只负责分组，不会包含次级排序的信息
         */
        //for (int i = l; i <= r; ++i)
        //{
        //    var index = (int)((arr[i] & mask) >> shiftCount);
        //    trr[--counting[index] + l] = arr[i]; // + l的偏移别忘了
        //}

        // 还是得从后往前进行
        for (int i = r; i >= l; --i)
        {
            var index = (int)((arr[i] & mask) >> shiftCount);
            trr[--counting[index] + l] = arr[i]; // + l的偏移别忘了
        }

        for (int i = l; i <= r; ++i)
            arr[i] = trr[i];
        for (int i = 0; i < 16; ++i)
        {
            if (i < 15)
                DoMSD(arr, trr, l + counting[i], l + counting[i + 1] - 1, mask >> 4);
            else
                DoMSD(arr, trr, l + counting[i], r, mask >> 4);
        }
    }
}