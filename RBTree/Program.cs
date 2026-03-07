using System.Text;

RBTree tree = new([15,9,18,6,13,17,27,10,23,34,25,37]);

int[] keys = new int[] { 15,9,13,34,25,37,10 };
foreach (var key in keys)
{
    Console.WriteLine($"删除{key}后:");
    tree.Remove(key);
    tree.ShowLog();
}

public enum Color
{
    Red,
    Black,
}

public class TNode
{
    public readonly static TNode Nil = new();

    public int Key;
    public Color Color;
    public TNode Parent;
    public TNode Left;
    public TNode Right;

    public TNode Sibling => (Parent.Left == this) ? Parent.Right : Parent.Left;
    public TNode Grandpa => Parent.Parent;
    public TNode Uncle => (Grandpa.Left == Parent) ? Grandpa.Right : Grandpa.Left;

    // Nil节点（黑）
    private TNode()
    { 
        Color = Color.Black;
        Parent = Left = Right = this;
    }

    // 其他节点
    public TNode(int key)
    {
        Key = key;
        Parent = Left = Right = Nil;
    }

    public override string ToString()
    {
        if (Parent == this)
            return "NIL";
        string show = $"Color: {Color}, ";
        show += "Left: " + (Left == Nil ? "NIL" : Left.Key.ToString()) + ", ";
        show += $"Me: {Key}, ";
        show += "Right: " + (Right == Nil ? "Nil" : Right.Key.ToString()) + ", ";
        show += "Parent: " + (Parent == Nil ? "Nil" : Parent.Key.ToString());
        return show;
    }
}

public class RBTree
{
    private TNode Root = TNode.Nil;

    public RBTree() { }

    public RBTree(int[] keys)
    {
        foreach (var key in keys)
            Insert(key);
    }

    public void Insert(int key)
    {
        var explorer = Root;
        var behind = explorer;
        while (explorer != TNode.Nil)
        {
            behind = explorer;
            if (key > explorer.Key)
                explorer = explorer.Right;
            else if (key < explorer.Key)
                explorer = explorer.Left;
            else
                return;
        }
        var newNode = new TNode(key) { Parent = behind, Color = Color.Red };
        if (behind != TNode.Nil)
        {
            if (key > behind.Key)
                behind.Right = newNode;
            else
                behind.Left = newNode;
        }
        else
        {
            // 插入根
            Root = newNode;
        }
        // 尝试循环调整
        var checkNode = newNode;
        do { checkNode = FixupAfterInsert(checkNode); }
        while (checkNode != TNode.Nil);
    }

    private TNode FixupAfterInsert(TNode node)
    {
        // 根叶黑
        if (node == Root)
        {
            node.Color = Color.Black;
            return TNode.Nil;
        }
        // !不红红
        if (node.Parent.Color == Color.Black)
            return TNode.Nil;

        // 处理叔叔是红色节点的情况 -> 黑下沉，红冒泡，抛出问题，交由上层去处理
        // 叔、父、爷变色，进一步处理爷爷
        if (node.Uncle.Color == Color.Red)
        {
            node.Uncle.Color = Color.Black;
            node.Parent.Color = Color.Black;
            node.Grandpa.Color = Color.Red;
            return node.Grandpa;
        }
        // 处理叔叔是黑色节点的情况 -> 叔叔黑色，所以可以自由进行旋转，由偏链转为对称链，即可在子树范围内解决问题
        // 四种旋转，旧、新根变色
        else
        {
            node.Grandpa.Color = Color.Red;
            if (node.Grandpa.Left == node.Parent)
            {
                if (node.Parent.Left == node)
                {
                    node.Parent.Color = Color.Black;
                    RightRotation(node.Grandpa);
                }
                else
                {
                    node.Color = Color.Black;
                    LR(node.Grandpa);
                }
            }
            else
            {
                if (node.Parent.Left == node)
                {
                    node.Color = Color.Black;
                    RL(node.Grandpa);
                }
                else
                {
                    node.Parent.Color = Color.Black;
                    LeftRotation(node.Grandpa);
                }
            }
            return TNode.Nil;
        }
    }

    public void Remove(int key)
    {
        var explorer = Root;
        while (explorer != TNode.Nil)
        {
            if (key > explorer.Key)
                explorer = explorer.Right;
            else if (key < explorer.Key)
                explorer = explorer.Left;
            else
            {
                // 没有孩子
                if (explorer.Left == TNode.Nil && explorer.Right == TNode.Nil)
                {
                    // 删除单个黑节点：可能需要多次循环处理
                    if (explorer.Color == Color.Black)
                    {
                        var dealNode = explorer;
                        do { dealNode = FixupBeforeRemove(dealNode); }
                        while (dealNode != TNode.Nil);
                    }
                    // 删除普通节点
                    if (explorer != Root)
                    {
                        if (explorer.Parent.Left == explorer)
                            explorer.Parent.Left = TNode.Nil;
                        else
                            explorer.Parent.Right = TNode.Nil;
                    }
                    // 删除根节点 -> 变成空节点
                    else
                        Root = TNode.Nil;
                    return;
                }
                // 只有一个孩子 -> 替代后变黑
                else if (explorer.Left == TNode.Nil || explorer.Right == TNode.Nil)
                {
                    var successor = explorer.Left == TNode.Nil ? explorer.Right : explorer.Left;
                    successor.Parent = explorer.Parent;
                    if (explorer.Parent != TNode.Nil)
                    {
                        if (explorer.Parent.Left == explorer)
                            explorer.Parent.Left = successor;
                        else
                            explorer.Parent.Right = successor;
                    }
                    successor.Color = Color.Black;
                    // 删除根节点 -> 新根继位
                    if (explorer == Root)
                        Root = successor;
                    return;
                }
                // 两个孩子，后继节点代替，随后去移除后继节点
                else
                {
                    var successor = explorer.Right;
                    while (successor.Left != TNode.Nil)
                        successor = successor.Left;
                    explorer.Key = successor.Key;
                    key = successor.Key;
                    explorer = explorer.Right;
                }
            }
        }
    }

    private TNode FixupBeforeRemove(TNode node)
    {
        if (node == Root)
            return TNode.Nil;

        // 兄弟是红色 -> 转换成兄弟是黑色
        if (node.Sibling.Color == Color.Red)
        {
            // 兄弟红色，需要转换（保留待删除的节点）
            node.Sibling.Color = node.Parent.Color;
            node.Parent.Color = Color.Red;
            if (node.Parent.Left == node)
                LeftRotation(node.Parent);
            else
                RightRotation(node.Parent);
            return node;
        }
        // 兄弟是黑色
        else
        {
            // 有红侄子：处理后结束
            if (node.Sibling.Left.Color == Color.Red || node.Sibling.Right.Color == Color.Red)
            {
                if (node.Parent.Left == node.Sibling)
                {
                    if (node.Sibling.Left.Color == Color.Red)
                    {
                        node.Sibling.Left.Color = node.Sibling.Color;
                        node.Sibling.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        RightRotation(node.Parent);
                    }
                    else
                    {
                        node.Sibling.Right.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        LR(node.Parent);
                    }
                }
                else
                {
                    if (node.Sibling.Left.Color == Color.Red)
                    {
                        node.Sibling.Left.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        RL(node.Parent);
                    }
                    else
                    {
                        node.Sibling.Right.Color = node.Sibling.Color;
                        node.Sibling.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        LeftRotation(node.Parent);
                    }
                }
                return TNode.Nil;
            }
            // 全是黑侄子
            else
            {
                var parent = node.Parent;
                node.Sibling.Color = Color.Red;

                // 如果父亲是根节点，直接返回
                if (parent == Root)
                    return TNode.Nil;

                // 如果父节点是红色，变黑即可
                if (parent.Color == Color.Red)
                {
                    parent.Color = Color.Black;
                    return TNode.Nil;
                }
                return parent;
            }
        }
    }

    private void LeftRotation(TNode node)
    {
        var outParent = node.Parent;
        var rightNode = node.Right;
        var rightLeftNode = rightNode.Left;

        rightNode.Left = node;
        node.Parent = rightNode;

        node.Right = rightLeftNode;
        if (rightLeftNode != TNode.Nil)
            rightLeftNode.Parent = node;

        rightNode.Parent = outParent;
        if (node != Root)
        {
            if (outParent.Left == node)
                outParent.Left = rightNode;
            else
                outParent.Right = rightNode;
        }
        else
        {
            Root = rightNode;
        }
    }

    private void RightRotation(TNode node)
    {
        var outParent = node.Parent;
        var leftNode = node.Left;
        var leftRightNode = leftNode.Right;

        leftNode.Right = node;
        node.Parent = leftNode;

        node.Left = leftRightNode;
        if (leftRightNode != TNode.Nil)
            leftRightNode.Parent = node;

        leftNode.Parent = outParent;
        if (node != Root)
        {
            if (outParent.Left == node)
                outParent.Left = leftNode;
            else
                outParent.Right = leftNode;
        }
        else
        {
            Root = leftNode;
        }
    }

    private void LR(TNode node)
    {
        var leftNode = node.Left;
        LeftRotation(leftNode);
        RightRotation(node);
    }

    private void RL(TNode node)
    {
        var rightNode = node.Right;
        RightRotation(rightNode);
        LeftRotation(node);
    }

    public void ShowLog()
    {
        StringBuilder sb = new("层序遍历：\n");
        DoShowLog(sb);
        Console.WriteLine(sb);
    }

    private int GetHeight(TNode node)
    {
        if (node == TNode.Nil)
            return 0;
        var leftHeight = GetHeight(node.Left);
        var rightHeight = GetHeight(node.Right);
        return Math.Max(leftHeight, rightHeight) + 1;
    }

    private void DoShowLog(StringBuilder sb)
    {
        Queue<TNode?> queue = new();
        queue.Enqueue(Root);
        int curLayer = 0;
        int totalLayer = GetHeight(Root);
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
                if (curNode != TNode.Nil && curNode != null)
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
}