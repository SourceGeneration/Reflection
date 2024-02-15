// See https://aka.ms/new-console-template for more information
using SourceGeneration.Reflection;

var type = SourceReflector.GetType(typeof(Model))!;

Model model = new()
{
    Name = "a"
};

// ouput "a"
Console.WriteLine(type.GetField("Constant")!.GetValue(null));

// ouput "a"
Console.WriteLine(type.GetProperty("Name")!.GetValue(model));

// output "b"
type.GetProperty("Name")!.SetValue(model, "b");
Console.WriteLine(type.GetProperty("Name")!.GetValue(model));

// output 1
Console.WriteLine(type.GetField("_field")!.GetValue(model));

// output 2
type.GetField("_field")!.SetValue(model, 2);
Console.WriteLine(type.GetField("_field")!.GetValue(model));

// output 3
Console.WriteLine(type.GetMethod("Add")!.Invoke(model, [1, 2]));

// output 3
Console.WriteLine(type.GetMethod("StaticAdd")!.Invoke(null, [1, 2]));

var newModel =(Model)type.GetConstructor([typeof(int), typeof(string)])!.Invoke([1, "abc"]);
// output abc
Console.WriteLine(newModel.Name);
// output 1
Console.WriteLine(newModel.No);

Console.ReadLine();


[SourceReflection]
public class Model
{
    const string Constant = "Hello SourceReflection";

    public int No { get; set; }
    public string? Name { get; set; }

    private int _field = 1;

    private Model(int no, string name)
    {
        No = no;
        Name = name;
    }

    public Model() { }

    public int Add(int a, int b) => a + b;

    private static int StaticAdd(int a, int b) => a + b;

}
