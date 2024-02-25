namespace SourceGeneration.Reflection.Test;

[TestClass]
public class ArrayTest
{
    [TestMethod]
    public void Test()
    {
        var type = SourceReflector.GetRequiredType<int>();
        var arrayType = type.MarkArrayType();

        int[] array = [1, 2];
        Assert.AreEqual(2, arrayType.GetRequriedProperty("Length").GetValue(array));

        arrayType.GetMethod("Set")!.Invoke(array, [0, 2]);
        Assert.AreEqual(2, arrayType.GetMethod("Get")!.Invoke(array, [0]));
    }
}
