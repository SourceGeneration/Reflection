using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class GenericDictionaryTypeMemberTest
{
    [TestMethod]
    public void GenericDictionary()
    {
        var typeInfo = SourceReflector.GetRequiredType<GenericDictionaryMemberType>();

        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.Int))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.Object))!.IsGenericDictionaryType);
        Assert.IsFalse(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.NonGenericDictionaryInterface))!.IsGenericDictionaryType);

        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.Dictionary))!.IsGenericDictionaryType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.SortedDictionary))!.IsGenericDictionaryType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.DictionaryInterface))!.IsGenericDictionaryType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.CustomDictionary))!.IsGenericDictionaryType);

        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.Dictionary))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.SortedDictionary))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.DictionaryInterface))!.IsGenericEnumerableType);
        Assert.IsTrue(typeInfo.GetFieldOrProperty(nameof(GenericDictionaryMemberType.CustomDictionary))!.IsGenericEnumerableType);
    }
}

[SourceReflection]
public class GenericDictionaryMemberType
{
    public object? Object { get; set; }
    public int Int { get; set; }
    public SortedDictionary<string, string>? SortedDictionary { get; set; }
    public Dictionary<string, string>? Dictionary { get; set; }
    public IDictionary<string, string>? DictionaryInterface { get; set; }
    public IDictionary? NonGenericDictionaryInterface { get; set; }

    public CustomDictionary? CustomDictionary;
}

public class CustomDictionary : IDictionary<int, object>
{
    public object this[int key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ICollection<int> Keys => throw new NotImplementedException();

    public ICollection<object> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(int key, object value)
    {
        throw new NotImplementedException();
    }

    public void Add(KeyValuePair<int, object> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<int, object> item)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(int key)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<int, object>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(int key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<int, object> item)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out object value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}