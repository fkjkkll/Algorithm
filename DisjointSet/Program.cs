using DisjointSet;

DisjointSet<string> ds = new(["合肥","厦门","重庆","北京","上海"]);
ds.Union("北京", "上海");
Console.WriteLine(ds.IsUnion("北京","厦门"));
ds.Union("上海", "合肥");
Console.WriteLine(ds.IsUnion("北京", "厦门"));
ds.Union("厦门", "合肥");
Console.WriteLine(ds.IsUnion("北京", "厦门"));

Console.WriteLine("");