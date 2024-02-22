// See https://aka.ms/new-console-template for more information
using SourceGeneration.Reflection;

var goods = SourceReflector.CreateInstance<Goods>(1);
Console.WriteLine(goods.Name);

goods = SourceReflector.CreateInstance<Goods>(1L);
Console.WriteLine(goods.Name);

goods = SourceReflector.CreateInstance<Goods>((short)1);
Console.WriteLine(goods.Name);

goods = SourceReflector.CreateInstance<Goods>(1, "custom name");
Console.WriteLine(goods.Name);

Console.ReadLine();

[SourceReflection]
public class Goods
{
    public Goods() { }

    public Goods(short id)
    {
        Id = id;
        Name = "short";
    }

    protected Goods(long id)
    {
        Id = (int)id;
        Name = "long";
    }

    protected Goods(int id, string name = "default value")
    {
        Id = id;
        Name = name;
    }


    private int Id { get; set; }
    public string? Name { get; private set; }
}