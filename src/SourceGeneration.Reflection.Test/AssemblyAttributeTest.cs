using SourceGeneration.Reflection;

[assembly: SourceReflectionType<object>]
[assembly: SourceReflectionType<Array>]
[assembly: SourceReflectionType(typeof(Enumerable))]
[assembly: SourceReflectionType(typeof(List<>))]

namespace SourceGeneration.Reflection.Test;


[TestClass]
public class AssemblyAttributeTest
{
    [TestMethod]
    public void Object()
    {
        var type = SourceReflector.GetRequiredType<object>();

    }
}