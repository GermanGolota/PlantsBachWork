namespace Plants.Shared;

public static class Helpers
{
    private readonly static Lazy<TypeHelper> _typeHelper = new(() => new TypeHelper());
    public static TypeHelper Type => _typeHelper.Value;
}
