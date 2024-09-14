using SourceGeneration.AutoMapper;
using SourceGeneration.Reflection;

ViewModel model = new()
{
    Name = "Root",
    Id = 1,
    Count = 1,
    List = ["a", "b"],
    DateTime = DateTime.Now,
    Nested = new ViewModel
    {
        Id = 2,
        Name = "Nested",
        Count = 2,
    }
};

var dic = SourceAutoMapper.ToDictionary(model, typeof(ViewModel));

Console.WriteLine(dic["List"]);

var dto = SourceAutoMapper.Map<ViewModel, ModelDto>(model);

Console.WriteLine(dto.Id);
Console.WriteLine(dto.name);
Console.WriteLine(dto.Nested!.Id);
Console.WriteLine(dto.Nested!.name);

Console.ReadLine();

[SourceReflection]
public class ViewModel
{
    public int Id { get; init; }
    public int Count { get; set; }
    public string? Name { get; set; }
    public ViewModel? Nested { get; set; }
    public string[]? List { get; set; }
    public DateTime? DateTime { get; set; }
}

[SourceReflection]
public class ModelDto
{
    public float Id { get; init; }
    public string? name { get; set; }
    public IEnumerable<string>? List { get; set; }
    public DateTime Count { get; set; }
    public ModelDto? Nested { get; set; }
}