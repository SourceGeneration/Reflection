namespace SourceGeneration.Reflection.Test;

[TestClass]
public class PropertyAccessibilityTest
{
    [TestMethod]
    public void Accessibility()
    {
        var type = SourceReflector.GetType(typeof(TestPropertyAccessibilityClass));
        Assert.IsNotNull(type);

        Assert.AreEqual(6, type.DeclaredFields.Length);

        AssertPropertyInfo(type.DeclaredProperties[0], "Public", typeof(int), SourceAccessibility.Public);
        AssertPropertyInfo(type.DeclaredProperties[1], "Private", typeof(int), SourceAccessibility.Private);
        AssertPropertyInfo(type.DeclaredProperties[2], "Protected", typeof(int), SourceAccessibility.Protected);
        AssertPropertyInfo(type.DeclaredProperties[3], "Internal", typeof(int), SourceAccessibility.Internal);
        AssertPropertyInfo(type.DeclaredProperties[4], "InternalProtected", typeof(int), SourceAccessibility.ProtectedOrInternal);
        AssertPropertyInfo(type.DeclaredProperties[5], "ProtectedInternal", typeof(int), SourceAccessibility.ProtectedOrInternal);
    }

    [TestMethod]
    public void ReadOnly()
    {
        var type = SourceReflector.GetType(typeof(TestPropertyReadOnlyClass));
        Assert.IsNotNull(type);

        var property = type.GetProperty("ReadOnlyField");
        Assert.IsNotNull(property);
        Assert.IsTrue(property.CanRead);
        Assert.IsFalse(property.CanWrite);

        property = type.GetProperty("NotReadOnlyField");
        Assert.IsNotNull(property);
        Assert.IsTrue(property.CanRead);
        Assert.IsTrue(property.CanWrite);

    }

    static void AssertPropertyInfo(SourcePropertyInfo property, string name, Type type, SourceAccessibility accessibility)
    {
        Assert.AreEqual(name, property.Name);
        Assert.AreEqual(type, property.PropertyType);
        Assert.AreEqual(accessibility, property.Accessibility);
    }
}

[SourceReflection]
public class TestPropertyAccessibilityClass
{
    public int Public { get; set; } = 1;
    private int Private { get; set; } = 1;
    protected int Protected { get; set; } = 1;
    internal int Internal { get; set; } = 1;
    internal protected int InternalProtected { get; set; } = 1;
    protected internal int ProtectedInternal { get; set; } = 1;

}

[SourceReflection]
public class TestPropertyReadOnlyClass
{
    public string ReadOnlyField { get; } = "abc";
    public string NotReadOnlyField { get; set; } = "abc";
}