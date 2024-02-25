namespace SourceGeneration.Reflection.Test;

[TestClass]
public class StaticMemberTest
{
    [TestMethod]
    public void Field()
    {
        var type = SourceReflector.GetRequiredType<StaticFieldTestObject>();
        var field = type.GetRequriedField(nameof(StaticFieldTestObject.Null));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(null, field.GetValue(null));

        field = type.GetRequriedField(nameof(StaticFieldTestObject.String));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual("a", field.GetValue(null));

        field = type.GetRequriedField(nameof(StaticFieldTestObject.Integer));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(1, field.GetValue(null));

        field = type.GetRequriedField(nameof(StaticFieldTestObject.Bool));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(true, field.GetValue(null));

        field = type.GetRequriedField(nameof(StaticFieldTestObject.Float));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(-double.E, field.GetValue(null));

        field = type.GetRequriedField(nameof(StaticFieldTestObject.Enum));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(DayOfWeek.Monday, field.GetValue(null));
    }

    [TestMethod]
    public void Property()
    {
        var type = SourceReflector.GetRequiredType<StaticPropertyTestObject>();
        var field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.Null));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(null, field.GetValue(null));

        field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.String));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual("a", field.GetValue(null));

        field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.Integer));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(1, field.GetValue(null));

        field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.Bool));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(true, field.GetValue(null));

        field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.Float));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(-double.E, field.GetValue(null));

        field = type.GetRequriedProperty(nameof(StaticPropertyTestObject.Enum));
        Assert.IsTrue(field.IsStatic);
        Assert.AreEqual(DayOfWeek.Monday, field.GetValue(null));
    }

}

[SourceReflection]
public class StaticFieldTestObject
{
    public static string String = "a";
    public static int Integer = 1;
    public static bool Bool = true;
    public static double Float = -double.E;
    public static string? Null = null;
    public static DayOfWeek Enum = DayOfWeek.Monday;
}

[SourceReflection]
public class StaticPropertyTestObject
{
    public static string String { get; } = "a";
    public static int Integer { get; } = 1;
    public static bool Bool { get; } = true;
    public static double Float { get; } = -double.E;
    public static string? Null { get; } = null;
    public static DayOfWeek Enum { get; } = DayOfWeek.Monday;
}
