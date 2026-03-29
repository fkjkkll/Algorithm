using RbTree;

RbTree.RbTree tree = new([15,9,18,6,13,17,27,10,23,34,25,37]);
tree.Insert(77);
int[] keys = [15,9,13,34,25,37,10];
foreach (var key in keys)
{
    Console.WriteLine($"删除{key}后:");
    tree.Remove(key);
    tree.ShowLog();
}