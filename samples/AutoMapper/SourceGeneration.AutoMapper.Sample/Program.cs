using SourceGeneration.AutoMapper;
using SourceGeneration.Reflection;

Console.WriteLine("Hello, World!");

ViewModel model = new ViewModel
{
    Name = "Hello",
    Id = 1,
    Count = 3,
};

var dto = SourceAutoMapper.Map<ViewModel, ModelDto>(model);

Console.ReadLine();

[SourceReflection]
public class ViewModel
{
    public int Id { get; set; }
    public int Count { get; set; }
    public string Name { get; set; }
}

[SourceReflection]
public class ModelDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}