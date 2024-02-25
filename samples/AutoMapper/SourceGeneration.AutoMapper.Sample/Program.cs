using SourceGeneration.AutoMapper;
using SourceGeneration.Reflection;

ViewModel model = new()
{
    Name = "Root",
    Id = 1,
    Count = 1,
    Nested = new ViewModel
    {
        Id = 2,
        Name = "Nested",
        Count = 2,
    }
};

var dto = SourceAutoMapper.Map<ViewModel, ModelDto>(model);

Console.WriteLine(dto.Id);
Console.WriteLine(dto.Name);
Console.WriteLine(dto.Nested!.Id);
Console.WriteLine(dto.Nested!.Name);

Console.ReadLine();

[SourceReflection]
public class ViewModel
{
    public int Id { get; init; }
    public int Count { get; set; }
    public string? Name { get; set; }
    public ViewModel? Nested { get; set; }
}

[SourceReflection]
public class ModelDto
{
    public int Id { get; init; }
    public string? Name { get; set; }

    public DateTime Count { get; set; }
    public ModelDto? Nested { get; set; }
}