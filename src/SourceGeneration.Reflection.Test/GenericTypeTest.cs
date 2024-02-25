using SourceGeneration.Reflection;

[assembly: SourceReflectionType(typeof(List<string>))]

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class GenericTypeTest
{
    [TestMethod]
    public void SourceReflection()
    {
        var type = SourceReflector.GetRequiredType<List<string>>();
        var method = type.GetMethod("Add");
        var property = type.GetProperty("Count");

        List<string> list = ["a", "b"];

        method.Invoke(list, ["c"]);

        Assert.AreEqual(3, property.GetValue(list));
    }

    //[TestMethod]
    //public void RuntimeReflection()
    //{
    //    var type = SourceReflector.GetRequiredType<List<string>>(true);
    //    var method = type.GetMethod("Add");
    //    var property = type.GetProperty("Count");

    //    List<string> list = ["a", "b"];

    //    method.Invoke(list, ["c"]);

    //    Assert.AreEqual(3, property.GetValue(list));
    //}
}
