namespace SourceGeneration.Reflection.Test;

[TestClass]
public class NestedClassTest
{
    [TestMethod]
    public void Public_Nested()
    {
        var type = SourceReflector.GetType(typeof(PublicNestedClass));
        Assert.IsNotNull(type);
        var instance = type.GetConstructor([])!.Invoke([]);
    }

    [TestMethod]
    public void Private_Nested()
    {
        var type = SourceReflector.GetType(typeof(PrivateNestedClass));
        Assert.IsNull(type);
    }

    [SourceReflection]
    public class PublicNestedClass
    {
        public int Property { get; set; }
    }

    [SourceReflection]
    private class PrivateNestedClass
    {
        public int Property { get; set; }
    }
}
