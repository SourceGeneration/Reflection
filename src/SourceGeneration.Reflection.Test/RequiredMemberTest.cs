namespace SourceGeneration.Reflection.Test;

[TestClass]
public class RequiredMemberTest
{
    [TestMethod]
    public void GetInfo()
    {
        var type = SourceReflector.GetType(typeof(RequiredMemberTestObject));
        Assert.IsNotNull(type);
        var p = type.GetProperty("Property");
        var f = type.GetField("Field");
        Assert.IsNotNull(p);
        Assert.IsNotNull(f);

        Assert.IsTrue(p.IsRequired);
        Assert.IsTrue(f.IsRequired);
    }

    [TestMethod]
    public void Construct()
    {
        var type = SourceReflector.GetType(typeof(RequiredMemberTestObject));
        Assert.IsNotNull(type);
        var instance = (RequiredMemberTestObject)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);

        Assert.AreEqual(1, instance.Field);
        Assert.AreEqual(1, instance.Property);
    }
}

[SourceReflection]
public class RequiredMemberTestObject
{
    public required int Property { get; set; } = 1;
    public required int Field = 1;
}