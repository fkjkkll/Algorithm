namespace RbTree;

public enum Color
{
    Red,
    Black,
}

public class Node
{
    public static readonly Node Nil = new();

    public int Key;
    public Color Color;
    public Node Parent;
    public Node Left;
    public Node Right;

    public Node Sibling => (Parent.Left == this) ? Parent.Right : Parent.Left;
    public Node Grandpa => Parent.Parent;
    public Node Uncle => (Grandpa.Left == Parent) ? Grandpa.Right : Grandpa.Left;

    // Nil节点（黑）
    private Node()
    { 
        Color = Color.Black;
        Parent = Left = Right = this;
    }

    // 其他节点
    public Node(int key)
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