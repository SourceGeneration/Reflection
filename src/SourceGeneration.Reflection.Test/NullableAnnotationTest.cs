namespace SourceGeneration.Reflection.Test;

[TestClass]
public class NullableAnnotationTest
{
    [TestMethod]
    public void Field()
    {
        var type = SourceReflector.GetType<NullableAnnotationTestObject>()!;

        Assert.AreEqual(SourceNullableAnnotation.Annotated, type.GetField("NullableField")!.NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.NotAnnotated, type.GetField("NotNullableField")!.NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.None, type.GetField("DisableNullableField")!.NullableAnnotation);
    }

    [TestMethod]
    public void Property()
    {
        var type = SourceReflector.GetType<NullableAnnotationTestObject>()!;

        Assert.AreEqual(SourceNullableAnnotation.Annotated, type.GetProperty("NullableProperty")!.NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.NotAnnotated, type.GetProperty("NotNullableProperty")!.NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.None, type.GetProperty("DisableNullableProperty")!.NullableAnnotation);
    }

    [TestMethod]
    public void MethodReturn()
    {
        var type = SourceReflector.GetType<NullableAnnotationTestObject>()!;

        Assert.AreEqual(SourceNullableAnnotation.Annotated, type.GetMethod("GetNullableMethod")!.ReturnNullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.NotAnnotated, type.GetMethod("GetNotNullableMethod")!.ReturnNullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.None, type.GetMethod("GetDisableNullableMethod")!.ReturnNullableAnnotation);
    }

    [TestMethod]
    public void MethodParameter()
    {
        var type = SourceReflector.GetType<NullableAnnotationTestObject>()!;

        Assert.AreEqual(SourceNullableAnnotation.Annotated, type.GetMethod("GetNullableParameterMethod")!.Parameters[0].NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.NotAnnotated, type.GetMethod("GetNotNullableParameterMethod")!.Parameters[0].NullableAnnotation);
        Assert.AreEqual(SourceNullableAnnotation.None, type.GetMethod("GetDisableNullableParameterMethod")!.Parameters[0].NullableAnnotation);
    }

}

[SourceReflection]
public class NullableAnnotationTestObject
{
    public string? NullableField;
    public string NotNullableField = null!;


    public string? NullableProperty { get; set; }
    public string NotNullableProperty { get; set; } = null!;

    public string? GetNullableMethod() => null;
    public string GetNotNullableMethod() => null!;

    public void GetNullableParameterMethod(string? args) { }
    public void GetNotNullableParameterMethod(string args) { }


#nullable disable

    public string DisableNullableField;
    public string DisableNullableProperty { get; set; }
    public string GetDisableNullableMethod() => null;
    public void GetDisableNullableParameterMethod(string args) { }
}


