# SourceReflection

## Why

随着.NET的发展，越来越多的应用有AOT Native的需要，但反射和动态代码阻碍了AOT的发布，`Source generator`可以很好的解决该问题，例如`System.Json.Text`通过`Source generator`处理对象的序列化，但都针对具体的业务进行实现无法通用化，类似的情况还有很多。

SourceReflection 希望提供一种更为通用的方式，为更多的开发人员提供 AOTable Reflection 支持，而无需重复编写 Source generator。

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

