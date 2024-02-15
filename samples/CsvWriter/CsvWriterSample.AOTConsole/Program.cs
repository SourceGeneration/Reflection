using CsvWriterSample;
using SourceGeneration.Reflection;

Model[] models = 
[
    new Model
    {
        Num = 1,
        Title = "A",
        Count = 12,
        Price = 8.23,
        Date = new DateTime(2023,7,3),
    },
    new Model
    {
        Num = 2,
        Title = "B",
        Count = 5,
        Price = 14.82,
        Date = new DateTime(2024,1,25),
    }
];

CsvUtility.Write("test.csv", models);

[SourceReflection]
public class Model
{
    public int Num { get; set; }
    public required string Title { get; set; }
    public int Count { get; set; }
    public double Price { get; set; }
    public DateTime Date { get; set; }
}