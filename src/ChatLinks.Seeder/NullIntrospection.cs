using SL.Common;

public sealed class NullIntrospection : IIntrospection
{
    public Stream? GetFileStream(string path)
    {
        return Stream.Null;
    }
}
