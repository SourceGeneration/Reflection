using SourceGeneration.Reflection;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

var options = new JsonSerializerOptions
{
    TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithSourceReflection(),
};

Model model = new()
{
    Id = Guid.NewGuid(),
    No = 1,
    Name = "hello",
    Enable = true,
    DateTime = DateTime.Now,
    //Extension1 = DateTime.Now.ToString(),
};

var json = JsonSerializer.Serialize(model, options);
model = JsonSerializer.Deserialize<Model>(json, options);

Console.WriteLine(json);
Console.WriteLine(model.Id);

Console.ReadLine();


[SourceReflection]
public class Model
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int No { get; set; }
    public bool Enable { get; set; }

    public DateTime DateTime { get; set; }

    [JsonPropertyName("ext1")]
    public string? Extension1 { get; set; }

}