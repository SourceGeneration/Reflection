

# Reflection

[![NuGet](https://img.shields.io/nuget/vpre/SourceGeneration.Reflection.svg)](https://www.nuget.org/packages/SourceGeneration.Reflection)

## Why

With the development of .NET, there is an increasing need for AOT Native in many applications. However, reflection and dynamic code pose obstacles to AOT deployment. `Source generators` can effectively this issue. For example, `System.Json.Text` use `SourceGenerator` to handle object serialization. However, these implementations are specific to individual businesses and cannot be easily generalized.

SourceReflection aims to provide a more universal solution, offering `AOTable` Reflection support to more developers without the need for repetitive source generator implementation.

## Supports

**Supports the following types**
- Class
- Record
- Struct (ref struct is not supports)
- Enum

**Supports the the following members**
- Field
- Property
- Indexer
- Method
- Constructor

**Unsupports**
- GenericType
- GenericMethod*

> Currently, support for generic types is not yet available. The main issue lies in the handling of `MarkGenericType`. Although the source generation can handle some known types, the results are not particularly satisfactory. I am currently experimenting with more effective approaches, but there is no specific plan at the moment.

> Similar to generic types, there are also generic methods. The issue lies in `MarkGenericMethod`. However, it can currently handle some specific cases, such as situations where the generic type can be inferred.


## Installing Reflection

```powershell
Install-Package SourceGeneration.Reflection -Version 1.0.0-beta2.240225.1
```
```powershell
dotnet add package SourceGeneration.Reflection --version 1.0.0-beta2.240225.1
```

## Source Reflection

SourceReflection requires using an attribute to mark the type
- Adding `[SourceReflectionAttribute]` to type.
- Adding `SourceReflectionTypeAttribute]` to assembly, it is possible to specify types from other assemblies

```c#
[assembly: SourceReflectionType<object>]
[assembly: SourceReflectionType(typeof(Enumerable))]

[SourceReflection]
public class Goods { }
```

## Basic

Add `[SourceReflectionAttribute]` to your class
```c#
using SourceGeneration.Reflection;

[SourceReflection]
public class Goods
{
    private int Id { get; set; }
    public string Name { get; private set; }
    public double Price { get; set; }

    internal void Discount(double discount)
    {
        Price = Price * discount;
    }
}
```

```c#
using SourceGeneration.Reflection;

// Get TypeInfo
var type = SourceReflector.GetType(typeof(Goods));

// Get default ConstructorInfo and create a instance
var goods = (Goods)type.GetConstructor([]).Invoke([]);

// Get PropertyInfo and set value
type.GetProperty("Id").SetValue(goods, 1); // private property
type.GetProperty("Name").SetValue(goods, "book"); // private property setter
type.GetProperty("Price").SetValue(goods, 3.14); // public property

// Output book
Console.WriteLine(goods.Name);
// Output 1
Console.WriteLine(type.GetProperty("Id").GetValue(goods));
// Output 3.14
Console.WriteLine(goods.Price);

// Get MethodInfo and invoke
type.GetMethod("Discount").Invoke(goods, [0.5]);
// Output 1.57
Console.WriteLine(goods.Price);
```

## Enum
```c#
[SourceReflection]
public enum TestEnum { A, B }
```

```c#
var type = SourceReflector.GetType(typeof(TestEnum));
Assert.IsTrue(type.IsEnum);
Assert.AreEqual("A", type.DeclaredFields[0].Name);
Assert.AreEqual("B", type.DeclaredFields[1].Name);
Assert.AreEqual(0, type.DeclaredFields[0].GetValue(null));
Assert.AreEqual(1, type.DeclaredFields[1].GetValue(null));
```

## Nullable Annotation

SourceReflection supports `nullable` annotations,
`nullable` annotations can be obtained for fields, properties, method return values, and parameters. It includes:
- SourceNullableAnnotation.Annotated
- SourceNullableAnnotation.NotAnnotated
- SourceNullableAnnotation.None, Indicates that nullable is disabled in the current context.

```c#
[SourceReflection]
public class NullableAnnotationTestObject
{
    public string? Nullable { get; set; }
    public string NotNullable { get; set; } = null!;

    #nullable disable

    public string DisableNullable { get; set; }
}
```

```c#
var type = SourceReflector.GetType<NullableAnnotationTestObject>()!;

Assert.AreEqual(SourceNullableAnnotation.Annotated, type.GetProperty("Nullable").NullableAnnotation);
Assert.AreEqual(SourceNullableAnnotation.NotAnnotated, type.GetProperty("NotNullable").NullableAnnotation);
Assert.AreEqual(SourceNullableAnnotation.None, type.GetProperty("DisableNullable").NullableAnnotation);
```

## Required Member

```c#
[SourceReflection]
public class RequiredMemberTestObject
{
    public required int Property { get; set; } = 1;
    public required int Field = 1;
}
```
```c#
var type = SourceReflector.GetType(typeof(RequiredMemberTestObject));
Assert.IsTrue(type.GetProperty("Property").IsRequired);
Assert.IsTrue(type.GetProperty("Field").IsRequired);
```

## InitOnly Property
```c#
[SourceReflection]
public class InitOnlyPropertyTestObject
{
    public required int Property { get; init; }
}
```
```c#
var type = SourceReflector.GetType(typeof(InitOnlyPropertyTestObject));
Assert.IsTrue(type.GetProperty("Property").IsInitOnly);
```

## Create Instance

The `SourceReflector.CreateInstance` method has almost the same functionality and features as the `System.Activator.CreateInstance` method.
It supports parameter matching and parameter default values.

```c#
[SourceReflection]
public class CreateInstanceTestObject
{
    public CreateInstanceTestObject() { }
    public CreateInstanceTestObject(byte a, string? c = "abc") { }
    internal CreateInstanceTestObject(int a, int b) { }
    protected CreateInstanceTestObject(long a, int c = 1, string? c = "abc") { }
}
```

```c#
var o1 = SourceReflector.CreateInstance<CreateInstanceTestObject>(); // Call the first constructor.
var o2 = SourceReflector.CreateInstance<CreateInstanceTestObject>((byte)1); // Call the second constructor.
var o3 = SourceReflector.CreateInstance<CreateInstanceTestObject>(1, 2); // Call the third constructor.
var o4 = SourceReflector.CreateInstance<CreateInstanceTestObject>(1L); // Call the fourth constructor.
//or use non-generic method
var o5 = SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), 1); // Call the fourth constructor.
var o6 = SourceReflector.CreateInstance(typeof(CreateInstanceTestObject), 1, 2, "abc"); // Call the fourth constructor.

```

## Without `SourceReflectionAttribute`

You can also without using `SourceReflectionAttribute` for reflection

```c#
public class Goods
{
    private int Id { get; set; }
    public string Name { get; private set; }
    public double Price { get; set; }

    internal void Discount(double discount)
    {
        Price = Price * discount;
    }
}
```

```c#
// Get TypeInfo and allow Runtime Reflection
var type = SourceReflector.GetType(typeof(Goods), true);

var goods = (Goods)type.GetConstructor([]).Invoke([]);
type.GetProperty("Id").SetValue(goods, 1); // private property
type.GetProperty("Name").SetValue(goods, "book"); // private property setter
type.GetProperty("Price").SetValue(goods, 3.14); // public property
type.GetMethod("Discount").Invoke(goods, [0.5]);
```

It can work properly after AOT compilation. `DynamicallyAccessedMembers` allows tools to understand which members are being accessed during the execution of a program. 

## Use other attribute to mark SourceReflection

You can create a custom attribute to indicate to the source generator which types need to be reflected. 

Edit your project `.csproj`
```xml
<!-- define your Attribute -->
<PropertyGroup>
  <DisplaySourceReflectionAttribute>System.ComponentModel.DataAnnotations.DisplayAttribute</DisplaySourceReflectionAttribute>
</PropertyGroup>

<!-- set property visible  -->
<!-- property name must be endswith 'SourceReflectionAttribute'  -->
<ItemGroup>
  <CompilerVisibleProperty Include="DisplaySourceReflectionAttribute" />
</ItemGroup>
```

Now you can use the `DisplayAttribute` to inform the source generator that you need to reflect it.

```c#
[System.ComponentModel.DataAnnotations.DisplayAttribute]
public class Goods
{
    private int Id { get; set; }
    public string Name { get; private set; }
    public double Price { get; set; }
}
```

## Performance & Optimization

`SourceReflection` generates alternative reflection-based method invocations through source generator, for example:

```c#
// code generated
new SourcePropertyInfo()
{
    Name = "Name",
    FieldType = typeof(string),
    GetValue = instance => ((Goods)instance).Name,
    SetValue = (instance, value) => ((Goods)instance).Name = (string)value,
    //Other properties init
}
```

For public and internal members, this approach is used,
while for protected and private members, runtime reflection is still used to accomplish it.

You can use `SourceReflection` get the runtime `MemberInfo`, the code like this:

```c#
public class SourcePropertyInfo
{
    private PropertyInfo? _propertyInfo;
    private Func<object, object?>? _getMethod;

    public PropertyInfo PropertyInfo
    {
        get => _propertyInfo ??= typeof(Goods).GetProperty(BindingFlags.NonPublic | BindingFlags.Instance, "Id");
    }

    public Func<object, object?> GetValue
    {
        get => _getMethod ??= PropertyInfo.GetValue;
        init => _getMethod = value;
    }
}
```
`SourceReflection` uses lazy evaluation, which means that reflection is only performed and the result is cached when you first retrieve it.

You don't need to worry about whether the user has marked an object with the `SourceReflectionAttribute`. You can use the `SourceReflection` to retrieve metadata or invoke methods in a generic way regardless of whether the attribute is used.

`SourceReflection` globally caching all objects (Type, FieldInfo, PropertyInfo, MethodInfo, ConstructorInfo) in a static cache.

## Generic Method

For generic methods with inferable types, they can be called using source generation

```c#
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
```
```c#
SourceTypeInfo type = SourceReflector.GetRequiredType<GenericMethodTestObject>();
GenericMethodTestObject instance = new();

// Success
type.GetMethod("Invoke1").Invoke(instance, [1]);
type.GetMethod("Invoke2").Invoke(instance, [1]);
type.GetMethod("Invoke4").Invoke(instance, [1]);
type.GetMethod("Invoke5").Invoke(instance, [1]);
type.GetMethod("Invoke6").Invoke(instance, [1, 2]);

//Error
type.GetMethod("Invoke0").Invoke(instance, []);
type.GetMethod("Invoke3").Invoke(instance, [1]);
type.GetMethod("InvokeArray1").Invoke(instance, [new int[] { 1 }]);
```

When the generic type cannot be inferred, the only option is to use runtime reflection through `MarkGenericMethod`, which will not be supported in AOT compilation.

```c#
type.GetMethod("Invoke0").MethodInfo.MarkGenericMethod([]).Invoke(instance);
```

Even if the type can be inferred, this approach has its drawbacks. If the internal implementation of the method has type checks on the generic parameters, the result may not meet expectations.

```c#
public class GenericMethodTestObject
{
    public string Invoke1<T>(T value) => typeof(T).Name;
    public string Invoke2<T>(T value) where T : ICloneable => typeof(T).Name;
}
```
```c#
var type = SourceReflector.GetRequiredType<GenericMethodTestInferObject>();
GenericMethodTestInferObject instance = new();
Assert.AreEqual("Object", type.GetMethod("Invoke1")!.Invoke(instance, [1]));
Assert.AreEqual("ICloneable", type.GetMethod("Invoke2")!.Invoke(instance, ["a"]));
```

## Samples

- [Basic](https://github.com/SourceGeneration/Reflection/tree/main/samples/Basic) example demonstrates some basic uses of SourceReflection.

- [CsvExporter](https://github.com/SourceGeneration/Reflection/tree/main/samples/CsvWriter) is a csv file export sample library.

- [AutoMapper](https://github.com/SourceGeneration/Reflection/tree/main/samples/AutoMapper) is a object-object mapper sample library.

- [CustomLibrary](https://github.com/SourceGeneration/Reflection/tree/main/samples/CustomLibrary) example demonstrates how to use SourceReflection to publish your NuGet package and propagate your attributes.
