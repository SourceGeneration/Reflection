namespace SourceGeneration.Reflection.Test;

[TestClass]
public class ConstantFieldTest
{
    [TestMethod]
    public void Test()
    {
        var type = SourceReflector.GetType<ConstantFieldTestObject>();
        var field = type.GetField(nameof(ConstantFieldTestObject.NullConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual(null, field.GetValue(null));

        field = type.GetField(nameof(ConstantFieldTestObject.StringConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual("a", field.GetValue(null));

        field = type.GetField(nameof(ConstantFieldTestObject.IntegerConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual(1, field.GetValue(null));

        field = type.GetField(nameof(ConstantFieldTestObject.BoolConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual(true, field.GetValue(null));

        field = type.GetField(nameof(ConstantFieldTestObject.FloatConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual(-double.E, field.GetValue(null));

        field = type.GetField(nameof(ConstantFieldTestObject.EnumConst));
        Assert.IsTrue(field.IsConst);
        Assert.AreEqual(1, field.GetValue(null));
    }
}

[SourceReflection]
public class ConstantFieldTestObject
{
    public const string StringConst = "a";
    public const int IntegerConst = 1;
    public const bool BoolConst = true;
    public const double FloatConst = -double.E;
    public const string? NullConst = null;
    public const DayOfWeek EnumConst = DayOfWeek.Monday;
}
