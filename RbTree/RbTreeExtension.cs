using System.Text;
namespace RbTree;

public static class RbTreeExtension
{
    public static void ShowLog(this RbTree tree)
    {
        StringBuilder sb = new("层序遍历：\n");
        DoShowLog(tree.Root, sb);
        Console.WriteLine(sb);
    }
    
    private static void DoShowLog(Node root, StringBuilder sb)
    {
        Queue<Node?> queue = new();
        queue.Enqueue(root);
        int curLayer = 0;
        int totalLayer = GetHeight(root);
        while (queue.Count > 0)
        {
            int count = queue.Count;
            string layerOutput = "";
            ++curLayer;
            bool isFirstSiblingInCurrentLayer = true;
            for (int i = 0; i < count; ++i)
            {
                var indent = (int)(Math.Pow(2, (totalLayer - curLayer)));
                if (curLayer > 1 && !isFirstSiblingInCurrentLayer)
                    indent <<= 1;

                var curNode = queue.Dequeue();
                if (curNode != Node.Nil && curNode != null)
                {
                    var mark = (curNode.Color == Color.Red) ? 'R' : 'B';
                    layerOutput += Indent(indent - 1) + mark + curNode.Key.ToString().PadLeft(3, ' ');
                    queue.Enqueue(curNode.Left);
                    queue.Enqueue(curNode.Right);
                }
                else if (curLayer <= totalLayer)
                {
                    layerOutput += Indent(indent - 1) + "NULL";
                    if (curLayer < totalLayer)
                    {
                        queue.Enqueue(null);
                        queue.Enqueue(null);
                    }
                }
                isFirstSiblingInCurrentLayer = false;
            }
            sb.AppendLine(layerOutput);
            sb.AppendLine();
        }
    }

    private static string Indent(int n)
    {
        return string.Concat(Enumerable.Repeat("    ", n));
    }
    
    private static int GetHeight(Node node)
    {
        if (node == Node.Nil)
            return 0;
        var leftHeight = GetHeight(node.Left);
        var rightHeight = GetHeight(node.Right);
        return Math.Max(leftHeight, rightHeight) + 1;
    }
}