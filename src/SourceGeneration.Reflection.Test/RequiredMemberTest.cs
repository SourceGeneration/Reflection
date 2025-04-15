using SourceGeneration.Reflection.SourceGenerator.TestLib;

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class RequiredMemberTest
{
    [TestMethod]
    public void GetRequiredPropertyInfo()
    {
        var type = SourceReflector.GetType(typeof(RequiredPropertyTestObject));
        Assert.IsNotNull(type);

        var p = type.GetProperty("A");
        Assert.IsNotNull(p);
        Assert.IsTrue(p.IsRequired);
    }

    [TestMethod]
    public void GetRequiredFieldInfo()
    {
        var type = SourceReflector.GetType(typeof(RequiredFieldTestObject));
        Assert.IsNotNull(type);

        var f = type.GetField("A");
        Assert.IsNotNull(f);
        Assert.IsTrue(f.IsRequired);
    }

    [TestMethod]
    public void ConstructRequiredPropertyClass()
    {
        var type = SourceReflector.GetType(typeof(RequiredPropertyTestObject));
        Assert.IsNotNull(type);
        var instance = (RequiredPropertyTestObject)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);

        Assert.AreEqual(1, instance.A);
    }

    [TestMethod]
    public void ConstructRequiredFieldClass()
    {
        var type = SourceReflector.GetType(typeof(RequiredFieldTestObject));
        Assert.IsNotNull(type);
        var instance = (RequiredFieldTestObject)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);
        Assert.AreEqual(1, instance.A);
    }

    [TestMethod]
    public void ConstructDerivedType()
    {
        var type = SourceReflector.GetType(typeof(RequiredFieldTestObject));
        Assert.IsNotNull(type);
        var instance = (RequiredFieldTestObject)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);
        Assert.AreEqual(1, instance.A);
    }

    [TestMethod]
    public void ConstructRequiredMemberFromLibClass()
    {
        var type = SourceReflector.GetType(typeof(RequiredMemberDerivedLibType));
        Assert.IsNotNull(type);
        var instance = (RequiredMemberDerivedLibType)type.GetConstructor([])!.Invoke([]);
        Assert.IsNotNull(instance);
        Assert.AreEqual(1, instance.RequiredProperty);
    }
}

[SourceReflection]
public class RequiredPropertyTestObject
{
    public required int A { get; set; } = 1;
}

[SourceReflection]
public class RequiredFieldTestObject
{
    public required int A = 1;
}

[SourceReflection]
public class RequiredMemberBaseType
{
    public required int A { get; set; } = 1;
    public required int B = 1;
}

[SourceReflection]
public class RequiredMemberDerivedType1 : RequiredMemberBaseType
{
    public required int C = 1;
}

[SourceReflection]
public class RequiredMemberDerivedType2 : RequiredMemberDerivedType1
{
}

[SourceReflection]
public class RequiredMemberDerivedLibType : LibTestClass
{
}


[SourceReflection]
public class RequiredMemberDefaultValueType
{
    public required int A { get; set; } = 1;
    public required bool B = true;
    public required string C { get; set; } = "abced";
    public required DateTime D { get; set; } = new DateTime(1234567);
    public required TimeOnly E { get; set; } = TimeOnly.MaxValue;
    public required object F = new();
    public required List<string> H { get; set; } = [];
    //public required RequiredMemberDerivedType1 G { get; set; } = new RequiredMemberDerivedType1 { A = 1, B = 2, C = 3 };

}