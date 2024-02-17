using SourceGeneration.AutoMapper;
using SourceGeneration.Reflection;

ViewModel model = new()
{
    Name = "Hello",
    Id = 1,
    Count = 3,
};

var dto = SourceAutoMapper.Map<ViewModel, ModelDto>(model);

Console.WriteLine(dto.Id);
Console.WriteLine(dto.Name);

Console.ReadLine();

[SourceReflection]
public class ViewModel
{
    public int Id { get; set; }
    public int Count { get; set; }
    public string? Name { get; set; }
}

[SourceReflection]
public class ModelDto
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public DateTime Count { get; set; }
}