namespace SourceGeneration.Reflection.Test;

[TestClass]
public class EnumTest
{
    [TestMethod]
    public void Enum()
    {
        var type = SourceReflector.GetType<EnumTestObject>(true);

        Assert.IsNotNull(type);

        Assert.AreEqual(2, type.DeclaredFields.Length);
        Assert.AreEqual("A", type.DeclaredFields[0].Name);
        Assert.AreEqual("B", type.DeclaredFields[1].Name);

        Assert.AreEqual(0, type.DeclaredFields[0].GetValue(null));
        Assert.AreEqual(1, type.DeclaredFields[1].GetValue(null));
    }
}

[SourceReflection]
public enum EnumTestObject
{
    A,B,
}
