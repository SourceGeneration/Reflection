namespace SourceGeneration.AutoMapper;

public class SourceAutoMapOptions
{
    public readonly static SourceAutoMapOptions Default = new();
    public bool PropertyNameCaseInsensitive { get; set; } = true;
    public bool IncludeFields { get; set; } = true;

    public MapNamingPolicy? NamingPolicy { get; set; }
}

public abstract class MapNamingPolicy
{

}

public enum MapIgnoreCondition
{
    Nerver = 0,
    Always = 0,
}
