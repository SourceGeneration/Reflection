
namespace SourceGeneration.Reflection.Test;

[TestClass]
public class GenericMethodTest
{
    [TestMethod]
    public void NoConstraint()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke1")!;

        GenericMethodTestObject instance = new();

        Assert.AreEqual("abc", method.Invoke(instance, ["abc"]));
        Assert.AreEqual(1, method.Invoke(instance, [1]));
        Assert.AreEqual(null, method.Invoke(instance, [null]));
    }

    [TestMethod]
    public void InterfaceConstraint()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke2")!;

        GenericMethodTestObject instance = new();

        Assert.AreEqual("abc", method.Invoke(instance, ["abc"]));
        Assert.AreEqual(null, method.Invoke(instance, [null]));

        Assert.ThrowsException<InvalidCastException>(() =>
        {
            Assert.AreEqual(1, method.Invoke(instance, [1]));
        });
    }

    [TestMethod]
    public void UnmanagedConstraint()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke3")!;

        GenericMethodTestObject instance = new();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Assert.AreEqual(1, method.Invoke(instance, [1]));
        });
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Assert.AreEqual("abc", method.Invoke(instance, ["abc"]));
        });
    }

    [TestMethod]
    public void NotNullConstraint()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke4")!;

        GenericMethodTestObject instance = new();
        Assert.AreEqual(1, method.Invoke(instance, [1]));
        Assert.AreEqual("abc", method.Invoke(instance, ["abc"]));
        Assert.AreEqual(null, method.Invoke(instance, [null]));
    }

    [TestMethod]
    public void DoubleInternfaceConstraint()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke5")!;

        GenericMethodTestObject instance = new();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Assert.AreEqual(1, method.Invoke(instance, [1]));
        });
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            Assert.AreEqual("abc", method.Invoke(instance, ["abc"]));
        });
    }


    [TestMethod]
    public void DoubleTypeParameter()
    {
        SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();

        var method = type.GetMethod("Invoke6")!;

        GenericMethodTestObject instance = new();

        Assert.AreEqual("a", method.Invoke(instance, ["a", "b"]));
    }
}

[SourceReflection]
public class GenericMethodTestObject
{
    public T Invoke0<T>() => default!;

    public T Invoke1<T>(T t) => t;

    public T Invoke2<T>(T t) where T : ICloneable => t;

    public T Invoke3<T>(T t) where T : unmanaged => t;

    public T Invoke4<T>(T t) where T : notnull => t;

    public T Invoke5<T>(T t) where T : ICloneable, IComparable => t;

    public T Invoke6<T, K>(T t, K k) where T : ICloneable where K : IComparable => t;


    public T[] InvokeArray1<T>(T[] t) => t;

}