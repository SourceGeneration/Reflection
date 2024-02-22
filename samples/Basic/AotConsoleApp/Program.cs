using SourceGeneration.Reflection;

var type = SourceReflector.GetType(typeof(Goods))!;

// Get default ConstructorInfo and create a instance
var goods = (Goods)type.GetConstructor([])!.Invoke([]);

// Get PropertyInfo and set value
type.GetProperty("Id")!.SetValue(goods, 1); // private property
type.GetProperty("Name")!.SetValue(goods, "book"); // private property setter
type.GetProperty("Price")!.SetValue(goods, 3.14); // public property

// Output 1
Console.WriteLine(type.GetProperty("Id")!.GetValue(goods));
// Output book
Console.WriteLine(goods.Name);
// Output 3.14
Console.WriteLine(goods.Price);

// Get MethodInfo and invoke
type.GetMethod("Discount")!.Invoke(goods, [0.5]);
// Output 1.57
Console.WriteLine(goods.Price);

[SourceReflection]
public class Goods
{
    private int Id { get; set; }
    public string? Name { get; private set; }
    public double Price { get; set; }

    internal void Discount(double discount)
    {
        Price = Price * discount;
    }
}