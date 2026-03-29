namespace AvlTree;

public class Node
{
    public int Id;
    public int Height;
    public Node? Left;
    public Node? Right;

    public int Factor => (Left?.Height ?? 0) - (Right?.Height ?? 0);

    public void RefreshHeight()
    {
        Height = Math.Max((Left?.Height ?? 0), (Right?.Height ?? 0)) + 1;
    }
}