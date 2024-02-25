namespace SourceGeneration.Reflection.Test;

[TestClass]
public class IndexerTest
{
    [TestMethod]
    public void Test1()
    {
        var indexer = SourceReflector.GetRequiredType<IndexerTestObject>().DeclaredProperties[0];
        IndexerTestObject obj = new();

        indexer.SetIndexerValue(obj, "abc", [1]);
        Assert.AreEqual("abc", indexer.GetIndexerValue(obj, [1]));
    }

    [TestMethod]
    public void Test2()
    {
        var indexer = SourceReflector.GetRequiredType<IndexerTestObject>().DeclaredProperties[1];
        IndexerTestObject obj = new();

        indexer.SetIndexerValue(obj, "abc", [1, 2]);
        Assert.AreEqual("abc", indexer.GetIndexerValue(obj, [1, 2]));
    }

}

[SourceReflection]
public class IndexerTestObject
{
    private Dictionary<int, string> _dic = [];

    public string this[int a]
    {
        get => _dic[a];
        set => _dic[a] = value;
    }

    public string this[int a, int b]
    {
        get => _dic[a];
        set => _dic[a] = value;
    }

}
