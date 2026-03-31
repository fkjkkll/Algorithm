using SparseSet;

IndependentSparseDict<string> isd = new(10,5);

var a = isd.Add("aaa");
var b = isd.Add("bbb");
var c = isd.Add("ccc");
var d = isd.Add("ddd");
var e = isd.Add("eee");

isd.Remove(c);

var f = isd.Add("fff");

if (isd.TryGet(f, out var r))
    Console.WriteLine(r);
