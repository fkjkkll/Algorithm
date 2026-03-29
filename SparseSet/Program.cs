


using SparseSet;

SparseDict<long> sd = new(32,8);
sd.Add(24, 7545);
sd.Add(22, 84);
sd.Add(12, 9999);

foreach (var e in sd)
{
    Console.WriteLine(e);
}