using System.Text;
namespace AvlTree;

public class AvlTree
{
    private Node? Root { get; set; }

    public AvlTree() { }

    public AvlTree(int[] ids)
    {
        foreach (var id in ids)
            Insert(id);
    }

    private void Insert(int id)
    {
        Root = DoInsert(Root, id);
    }

    private Node DoInsert(Node? node, int id)
    {
        // 新节点
        if (node == null)
            return new Node() { Id = id, Height = 1, Left = null, Right = null };
        if (id > node.Id)
            node.Right = DoInsert(node.Right, id);
        else if (id < node.Id)
            node.Left = DoInsert(node.Left, id);
        else
            return node; // 重复key不插入
        // 刷新插入后的树高
        node.RefreshHeight();
        // 判断是否要旋转
        var factor = node.Factor;
        if (factor > 1)
        {
            var leftChild = node.Left!;
            var leftChildFactor = leftChild.Factor;
            if (leftChildFactor >= 0) // 这样写，但是不可能出现等于
                return Ll(node);
            else
                return Lr(node);
        }
        else if (factor < -1)
        {
            var rightChild = node.Right!;
            var rightChildFactor = rightChild.Factor;
            if (rightChildFactor <= 0) // 这样写，但是不可能出现等于
                return Rr(node);
            else
                return Rl(node);
        }
        return node;
    }

    public Node? Find(int id)
    {
        return DoFind(Root, id);
    }

    private static Node? DoFind(Node? node, int id)
    {
        if (node == null)
            return null;
        if (id > node.Id)
            return DoFind(node.Right, id);
        else if (id < node.Id)
            return DoFind(node.Left, id);
        return node;
    }

    public void Remove(int id)
    {
        Root = DoRemove(Root, id);
    }

    private static Node? DoRemove(Node? node, int id)
    {
        if (node == null)
            return null;
        if (id > node.Id)
            node.Right = DoRemove(node.Right, id);
        else if (id < node.Id)
            node.Left = DoRemove(node.Left, id);
        else
        {
            if (node.Left == null && node.Right == null)
                node = null;
            else if (node.Left == null || node.Right == null)
                node = node.Left ?? node.Right;
            else
            {
                var rightMinNode = node.Right;
                while (rightMinNode.Left != null)
                    rightMinNode = rightMinNode.Left;
                node.Id = rightMinNode.Id;
                node.Right = DoRemove(node.Right, rightMinNode.Id);
            }
        }
        if (node == null)
            return node;
        // 刷新插入后的树高
        node.RefreshHeight();
        // 判断是否要旋转
        var factor = node.Factor;
        if (factor > 1)
        {
            var leftChild = node.Left!;
            var leftChildFactor = leftChild.Factor;
            node = leftChildFactor >= 0 ? Ll(node) : Lr(node); // 删除时，需要判断等于0
        }
        else if (factor < -1)
        {
            var rightChild = node.Right!;
            var rightChildFactor = rightChild.Factor;
            node = rightChildFactor <= 0 ? Rr(node) : Rl(node); // 删除时，需要判断等于0
        }
        return node;
    }

    private static Node Ll(Node maxNode)
    {
        var midNode = maxNode.Left!;
        maxNode.Left = midNode.Right;
        midNode.Right = maxNode;

        maxNode.RefreshHeight();
        midNode.RefreshHeight();
        return midNode;
    }

    private static Node Rr(Node minNode)
    {
        var midNode = minNode.Right!;
        minNode.Right = midNode.Left;
        midNode.Left = minNode;

        minNode.RefreshHeight();
        midNode.RefreshHeight();
        return midNode;
    }

    // LR旋转可以替换为对maxNode左孩子minNode执行左旋(LL)，然后再对maxNode执行右旋(RR)
    // 本方法不想分两步，直接一步将MidNode"提上来"
    private static Node Lr(Node maxNode)
    {
        var minNode = maxNode.Left!;
        var midNode = minNode.Right!;
        // 可能失去孩子的两个节点分给他们可能存在的新孩子
        minNode.Right = midNode.Left;
        maxNode.Left = midNode.Right;
        // 中间节点成为新的父亲
        midNode.Left = minNode;
        midNode.Right = maxNode;
        // 先刷新两侧收养孩子的节点，最后再刷新父亲节点
        minNode.RefreshHeight();
        maxNode.RefreshHeight();
        midNode.RefreshHeight();
        return midNode;
    }

    // RL旋转可以替换为对minNode的右孩子maxNode执行右旋(RR)，然后再对minNode执行左旋(LL)
    // 本方法不想分两步，直接一步将MidNode"提上来"
    private static Node Rl(Node minNode)
    {
        var maxNode = minNode.Right!;
        var midNode = maxNode.Left!;
        // 可能失去孩子的两个节点分给他们可能存在的新孩子
        minNode.Right = midNode.Left;
        maxNode.Left = midNode.Right;
        // 中间节点成为新的父亲
        midNode.Left = minNode;
        midNode.Right = maxNode;
        // 先刷新两侧收养孩子的节点，最后再刷新父亲节点
        minNode.RefreshHeight();
        maxNode.RefreshHeight();
        midNode.RefreshHeight();
        return midNode;
    }

    public void ShowLog()
    {
        StringBuilder sb = new("层序遍历：\n");
        DoShowLog(Root, sb);
        Console.WriteLine(sb);
    }

    private static void DoShowLog(Node? node, StringBuilder sb)
    {
        Queue<Node?> queue = new();
        queue.Enqueue(node);
        int curLayer = 0;
        int totalLayer = node?.Height ?? 0;
        while(queue.Count > 0)
        {
            int count = queue.Count;
            string layerOutput = "";
            ++curLayer;
            bool isFirstSiblingInCurrentLayer = true;
            for (int i=0; i< count; ++i)
            {
                var indent = (int)(Math.Pow(2, (totalLayer - curLayer)));
                if (curLayer > 1 && !isFirstSiblingInCurrentLayer)
                    indent <<= 1;

                var curNode = queue.Dequeue();
                if (curNode != null)
                {
                    layerOutput += Indent(indent - 1) + curNode.Id.ToString().PadLeft(4, ' ');
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
}