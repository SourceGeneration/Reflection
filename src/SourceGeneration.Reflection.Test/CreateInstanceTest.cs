namespace SourceGeneration.Reflection.Test;

[TestClass]
public class CreateInstanceTest
{
    [TestMethod]
    public void Parameterless()
    {
        var instance = (CreateInstanceTestObject)SourceReflector.CreateInstance(typeof(CreateInstanceTestObject));
        Assert.AreEqual((byte)0, instance.Byte);
        Assert.AreEqual(0, instance.Int);
        Assert.AreEqual(0L, instance.Long);
        Assert.AreEqual(null, instance.String);
    }

    [TestMethod]
    public void Parameter1()
    {
        var instance = (CreateInstanceTestObject)SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), 1);
        Assert.AreEqual((byte)0, instance.Byte);
        Assert.AreEqual(1, instance.Int);
        Assert.AreEqual(0L, instance.Long);
        Assert.AreEqual("b", instance.String);
    }

    [TestMethod]
    public void Parameter2()
    {
        var instance = (CreateInstanceTestObject)SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), (byte)1);
        Assert.AreEqual((byte)1, instance.Byte);
        Assert.AreEqual(0, instance.Int);
        Assert.AreEqual(0L, instance.Long);
        Assert.AreEqual("a", instance.String);
    }

    [TestMethod]
    public void Parameter3()
    {
        var instance = (CreateInstanceTestObject)SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), (long)1);
        Assert.AreEqual((byte)0, instance.Byte);
        Assert.AreEqual(0, instance.Int);
        Assert.AreEqual(1L, instance.Long);
        Assert.AreEqual("c", instance.String);
    }

    [TestMethod]
    public void Parameter_Full()
    {
        var instance = (CreateInstanceTestObject)SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), (long)1, "abc");
        Assert.AreEqual((byte)0, instance.Byte);
        Assert.AreEqual(0, instance.Int);
        Assert.AreEqual(1L, instance.Long);
        Assert.AreEqual("abc", instance.String);
    }

}

[SourceReflection]
public class CreateInstanceTestObject
{
    public CreateInstanceTestObject() { }

    public CreateInstanceTestObject(byte a, string? b = "a")
    {
        Byte = a;
        String = b;
    }

    public CreateInstanceTestObject(int a, string? b = "b")
    {
        Int = a;
        String = b;
    }

    public CreateInstanceTestObject(long a, string? b = "c")
    {
        Long = a;
        String = b;
    }

    public int Int { get; }
    public byte Byte { get; }
    public long Long { get; }
    public string? String { get; }
}
