using SourceGeneration.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CsvWriterSample;

public static class CsvUtility
{
    private static readonly Encoding CsvEncoding = new UTF8Encoding(true);

    
    public static void Write<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(string filename, IEnumerable<T> items)
    {
        var type = SourceReflector.GetType(typeof(T), true)!;
        StringBuilder builder = new();

        var properties = type.DeclaredProperties.Where(x => x.CanRead).ToArray();

        foreach (var item in items)
        {
            if (item == null)
                continue;

            for (int c = 0; c < properties.Length; c++)
            {
                SourcePropertyInfo? property = properties[c];
                var value = property.GetValue(item)?.ToString();
                if (value != null)
                {
                    if (value.Contains('\r') || value.Contains('\n'))
                    {
                        builder.Append('\"');
                        builder.Append(EscapeCsvString(value));
                        builder.Append('\"');
                    }
                    else
                    {
                        builder.Append(EscapeCsvString(value));
                    }
                }

                if (c < properties.Length - 1)
                {
                    builder.Append(',');
                }
            }

            builder.AppendLine();
        }

        File.WriteAllText(filename, builder.ToString(), CsvEncoding);

        static string? EscapeCsvString(string? s) => s?.Replace("\"", "\"\"");
    }
}
