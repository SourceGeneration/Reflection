namespace SourceGeneration.Reflection.Test;

[TestClass]
public class InheritedTest
{
    [TestMethod]
    public void Inherited()
    {
        var type1 = SourceReflector.GetType<InheritedObject1>();
        var type2 = SourceReflector.GetType<InheritedObject2>();
        var baseType = SourceReflector.GetType<BaseObject>();

        Assert.IsNotNull(type1);
        Assert.IsNotNull(type2);
        Assert.IsNotNull(baseType);
    }
}

public class BaseObject { }
[SourceReflection] public class InheritedObject1 : BaseObject { }
[SourceReflection] public class InheritedObject2 : BaseObject { }