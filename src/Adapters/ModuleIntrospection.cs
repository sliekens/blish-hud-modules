using Blish_HUD.Modules;

using SL.Common;

namespace SL.Adapters;

public class ModuleIntrospection(ModuleParameters parameters) : IIntrospection
{
    public Stream? GetFileStream(string path)
    {
        return parameters.ContentsManager.GetFileStream(path);
    }
}
