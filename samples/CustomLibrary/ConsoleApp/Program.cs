using SourceGeneration.Reflection;

var types = SourceReflector.GetTypes();

foreach (var type in types)
{
    Console.WriteLine("Type: " + type.Name);
}

Console.ReadLine();
