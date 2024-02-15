# SourceReflection

## Why

With the development of .NET, there is an increasing need for AOT Native in many applications. However, reflection and dynamic code pose obstacles to AOT deployment. `Source generators` can effectively this issue. For example, `System.Json.Text` uses a `Source generator` to handle object serialization. However, these implementations are specific to individual businesses and cannot be easily generalized.

SourceReflection aims to provide a more universal solution, offering `AOTable` Reflection support to more developers without the need for repetitive source generator implementation.

## Supports

- Field
- Property
- Method
- Constructor

## Start

Define your class
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

Use SourceReflector
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
Console.WriteLine(goods.Id);
// Output 3.14
Console.WriteLine(goods.Price);

// Get MethodInfo and invoke
type.GetMethod("Discount").Invoke(goods, 0.5);
// Output 1.57
Console.WriteLine(goods.Price);
```

## I don't want use SourceReflectionAttribute

Uou can reflection without `SourceReflectionAttribute`

Define your class whitout attribute

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
Use SourceReflector
```c#
using SourceGeneration.Reflection;

// Get TypeInfo and allow Runtime Reflection
var type = SourceReflector.GetType(typeof(Goods), true);

var goods = (Goods)type.GetConstructor([]).Invoke([]);
type.GetProperty("Id").SetValue(goods, 1); // private property
type.GetProperty("Name").SetValue(goods, "book"); // private property setter
type.GetProperty("Price").SetValue(goods, 3.14); // public property
type.GetMethod("Discount").Invoke(goods, 0.5);
```

It can work properly after AOT compilation. `DynamicallyAccessedMembers` allows tools to understand which members are being accessed during the execution of a program. 

## Use Custom Attribute

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
[System.ComponentModel.DataAnnotations.Display]
public class Goods
{
    private int Id { get; set; }
    public string Name { get; private set; }
    public double Price { get; set; }
}
```

## Samples

### HelloWorld

[HelloWord](https://github.com/SourceGeneration/Reflection/tree/main/samples/HelloWorld) example demonstrates some basic uses of SourceReflection.

### CsvWriter

[CsvWriter](https://github.com/SourceGeneration/Reflection/tree/main/samples/CsvWriter) is a aotable sample library, it provider only one method to export `.csv` file 
```C#
CsvUtility.Write("test.csv", models);
```

### CustomLibrary

[CustomLibrary](https://github.com/SourceGeneration/Reflection/tree/main/samples/CustomLibrary) example demonstrates how to use SourceReflection to publish your NuGet package and propagate your attributes.