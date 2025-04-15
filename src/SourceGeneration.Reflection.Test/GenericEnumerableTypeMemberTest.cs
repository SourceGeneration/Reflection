using System.Collections;
using System.Collections.ObjectModel;

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class GenericEnumerableTypeMemberTest
{
    [TestMethod]
    public void GenericEnumerable()
    {
        var typeInfo = SourceReflector.GetRequiredType<GenericEnumerableMemberType>();

        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.Int))!.IsGenericEnumerableType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.Object))!.IsGenericEnumerableType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.NonGenericEnumerableInterface))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.String))!.IsGenericEnumerableType);

        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.EnumerableInterface))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.ListInterface))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.List))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.ReadOnlyCollection))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomList))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomCollection))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomEnumerable))!.IsGenericEnumerableType);

        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.EnumerableInterface))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.ListInterface))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.List))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.ReadOnlyCollection))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomList))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomCollection))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericEnumerableMemberType.CustomEnumerable))!.IsGenericDictionaryType);
    }
}

[SourceReflection]
public class GenericEnumerableMemberType
{
    public object? Object { get; set; }
    public int Int { get; set; }
    public string? String { get; set; }
    public IEnumerable<string>? EnumerableInterface { get; set; }
    public IList<string>? ListInterface { get; set; }
    public List<string>? List { get; set; }
    public ReadOnlyCollection<string>? ReadOnlyCollection { get; set; }
    public IEnumerable? NonGenericEnumerableInterface { get; set; }

    public CustomList? CustomList;
    public CustomCollection? CustomCollection;
    public CustomEnumerable? CustomEnumerable;
}

public class CustomList : CustomCollection, IList<int>
{
    public int this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int IndexOf(int item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, int item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }
}

public class CustomCollection : ICollection<int>
{
    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(int item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(int item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(int[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<int> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(int item)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
public class CustomEnumerable : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}