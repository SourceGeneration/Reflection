using System.Diagnostics;

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class FieldAccessibilityTest
{
    [TestMethod]
    public void Accessibility()
    {
        var type = SourceReflector.GetType(typeof(TestFieldAccessibilityClass));
        Assert.IsNotNull(type);

        Assert.AreEqual(6, type.DeclaredFields.Length);

        AssertFieldInfo(type.DeclaredFields[0], "PublicField", typeof(int), SourceAccessibility.Public);
        AssertFieldInfo(type.DeclaredFields[1], "PrivateField", typeof(int), SourceAccessibility.Private);
        AssertFieldInfo(type.DeclaredFields[2], "ProtectedField", typeof(int), SourceAccessibility.Protected);
        AssertFieldInfo(type.DeclaredFields[3], "InternalField", typeof(int), SourceAccessibility.Internal);
        AssertFieldInfo(type.DeclaredFields[4], "InternalProtectedField", typeof(int), SourceAccessibility.ProtectedOrInternal);
        AssertFieldInfo(type.DeclaredFields[5], "ProtectedInternalField", typeof(int), SourceAccessibility.ProtectedOrInternal);
    }

    [TestMethod]
    public void ReadOnly()
    {
        var type = SourceReflector.GetType(typeof(TestFieldReadOnlyClass));
        Assert.IsNotNull(type);

        var field = type.GetField("ReadOnlyField");
        Assert.IsNotNull(field);
        Assert.IsTrue(field.IsReadOnly);

        field = type.GetField("NotReadOnlyField");
        Assert.IsNotNull(field);
        Assert.IsFalse(field.IsReadOnly);

    }

    [DebuggerHidden]
    static void AssertFieldInfo(SourceFieldInfo field, string name, Type type, SourceAccessibility accessibility)
    {
        Assert.AreEqual(name, field.Name);
        Assert.AreEqual(type, field.FieldType);
        Assert.AreEqual(accessibility, field.Accessibility);
    }
}

[SourceReflection]
public class TestFieldAccessibilityClass
{
    public int PublicField = 1;
#pragma warning disable IDE0044
    private int PrivateField = 1;
#pragma warning restore IDE0044
    protected int ProtectedField = 1;
    internal int InternalField = 1;
    internal protected int InternalProtectedField = 1;
    protected internal int ProtectedInternalField = 1;

}

[SourceReflection]
public class TestFieldReadOnlyClass
{
    public readonly string ReadOnlyField = "abc";
    public string NotReadOnlyField = "abc";
}