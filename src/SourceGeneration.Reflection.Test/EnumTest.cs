namespace SourceGeneration.Reflection.Test;

[TestClass]
public class EnumTest
{
    [TestMethod]
    public void Enum()
    {
        var type = SourceReflector.GetType<EnumTestObject>(true);

        Assert.IsNotNull(type);

        var fields = type.GetFields();
        Assert.AreEqual(2, fields.Length);
        Assert.AreEqual("A", fields[0].Name);
        Assert.AreEqual("B", fields[1].Name);

        Assert.AreEqual(0, fields[0].GetValue(null));
        Assert.AreEqual(1, fields[1].GetValue(null));
    }
}

[SourceReflection]
public enum EnumTestObject
{
    A,B,
}
