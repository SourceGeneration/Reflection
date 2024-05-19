namespace SourceGeneration.Reflection.Test;

[TestClass]
public class InitOnlyPropertyTest
{
    [TestMethod]
    public void GetInfo()
    {
        var type = SourceReflector.GetType(typeof(InitOnlyPropertyTestObject));
        Assert.IsNotNull(type);
        var p = type.GetProperty("Property");
        var r = type.GetProperty("RequiredProperty");
        Assert.IsNotNull(p);
        Assert.IsNotNull(r);

        Assert.IsTrue(p.IsInitOnly);
        Assert.IsTrue(r.IsInitOnly);
        Assert.IsTrue(r.IsRequired);
    }

    [TestMethod]
    public void Construct()
    {
        var type = SourceReflector.GetType(typeof(InitOnlyPropertyTestObject));
        Assert.IsNotNull(type);
        var instance = (InitOnlyPropertyTestObject)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);

        Assert.AreEqual(1, instance.RequiredProperty);
        Assert.AreEqual(1, instance.Property);
    }
}

[SourceReflection]
public class InitOnlyPropertyTestObject
{
    public int Property { get; init; } = 1;
    public required int RequiredProperty { get; init; } = 1;
}