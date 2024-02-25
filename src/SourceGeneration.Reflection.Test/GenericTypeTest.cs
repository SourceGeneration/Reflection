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

        List<string> list = ["a", "b"];
        
        type.GetMethod("Add")!.Invoke(list, ["c"]);
        Assert.AreEqual(3, type.GetProperty("Count")!.GetValue(list));
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
