namespace RbTree;

public class RbTree
{
    public Node Root { get; private set; } = Node.Nil;

    public RbTree() { }

    public RbTree(int[] keys)
    {
        foreach (var key in keys)
            Insert(key);
    }

    public void Insert(int key)
    {
        var explorer = Root;
        var behind = explorer;
        while (explorer != Node.Nil)
        {
            behind = explorer;
            if (key > explorer.Key)
                explorer = explorer.Right;
            else if (key < explorer.Key)
                explorer = explorer.Left;
            else
                return;
        }
        var newNode = new Node(key) { Parent = behind, Color = Color.Red };
        if (behind != Node.Nil)
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
        while (checkNode != Node.Nil);
    }

    private Node FixupAfterInsert(Node node)
    {
        // 根叶黑
        if (node == Root)
        {
            node.Color = Color.Black;
            return Node.Nil;
        }
        // !不红红
        if (node.Parent.Color == Color.Black)
            return Node.Nil;

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
                    Lr(node.Grandpa);
                }
            }
            else
            {
                if (node.Parent.Left == node)
                {
                    node.Color = Color.Black;
                    Rl(node.Grandpa);
                }
                else
                {
                    node.Parent.Color = Color.Black;
                    LeftRotation(node.Grandpa);
                }
            }
            return Node.Nil;
        }
    }

    public void Remove(int key)
    {
        var explorer = Root;
        while (explorer != Node.Nil)
        {
            if (key > explorer.Key)
                explorer = explorer.Right;
            else if (key < explorer.Key)
                explorer = explorer.Left;
            else
            {
                // 没有孩子
                if (explorer.Left == Node.Nil && explorer.Right == Node.Nil)
                {
                    // 删除单个黑节点：可能需要多次循环处理
                    if (explorer.Color == Color.Black)
                    {
                        var dealNode = explorer;
                        do { dealNode = FixupBeforeRemove(dealNode); }
                        while (dealNode != Node.Nil);
                    }
                    // 删除普通节点
                    if (explorer != Root)
                    {
                        if (explorer.Parent.Left == explorer)
                            explorer.Parent.Left = Node.Nil;
                        else
                            explorer.Parent.Right = Node.Nil;
                    }
                    // 删除根节点 -> 变成空节点
                    else
                        Root = Node.Nil;
                    return;
                }
                // 只有一个孩子 -> 替代后变黑
                else if (explorer.Left == Node.Nil || explorer.Right == Node.Nil)
                {
                    var successor = explorer.Left == Node.Nil ? explorer.Right : explorer.Left;
                    successor.Parent = explorer.Parent;
                    if (explorer.Parent != Node.Nil)
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
                    while (successor.Left != Node.Nil)
                        successor = successor.Left;
                    explorer.Key = successor.Key;
                    key = successor.Key;
                    explorer = explorer.Right;
                }
            }
        }
    }

    private Node FixupBeforeRemove(Node node)
    {
        if (node == Root)
            return Node.Nil;

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
                        Lr(node.Parent);
                    }
                }
                else
                {
                    if (node.Sibling.Left.Color == Color.Red)
                    {
                        node.Sibling.Left.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        Rl(node.Parent);
                    }
                    else
                    {
                        node.Sibling.Right.Color = node.Sibling.Color;
                        node.Sibling.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        LeftRotation(node.Parent);
                    }
                }
                return Node.Nil;
            }
            // 全是黑侄子
            else
            {
                var parent = node.Parent;
                node.Sibling.Color = Color.Red;

                // 如果父亲是根节点，直接返回
                if (parent == Root)
                    return Node.Nil;

                // 如果父节点是红色，变黑即可
                if (parent.Color == Color.Red)
                {
                    parent.Color = Color.Black;
                    return Node.Nil;
                }
                return parent;
            }
        }
    }

    private void LeftRotation(Node node)
    {
        var outParent = node.Parent;
        var rightNode = node.Right;
        var rightLeftNode = rightNode.Left;

        rightNode.Left = node;
        node.Parent = rightNode;

        node.Right = rightLeftNode;
        if (rightLeftNode != Node.Nil)
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

    private void RightRotation(Node node)
    {
        var outParent = node.Parent;
        var leftNode = node.Left;
        var leftRightNode = leftNode.Right;

        leftNode.Right = node;
        node.Parent = leftNode;

        node.Left = leftRightNode;
        if (leftRightNode != Node.Nil)
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

    private void Lr(Node node)
    {
        var leftNode = node.Left;
        LeftRotation(leftNode);
        RightRotation(node);
    }

    private void Rl(Node node)
    {
        var rightNode = node.Right;
        RightRotation(rightNode);
        LeftRotation(node);
    }
}