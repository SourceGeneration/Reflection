namespace System.Text;

internal static class CSharpCodeBuilderExtensions
{
    public static void AppendArrayInitializer(this CSharpCodeBuilder builder, string itemType, params string[] items)
    {
        if (items == null || items.Length == 0)
        {
            builder.Append($"global::System.Array.Empty<{itemType}>()");
            return;
        }

        builder.Append($"new {itemType}[] {{ ");
        for (int i = 0; i < items.Length; i++)
        {
            builder.Append(items[i]);
            if (i < items.Length - 1)
            {
                builder.Append(", ");
            }
        }
        builder.Append(" }");
    }
}
