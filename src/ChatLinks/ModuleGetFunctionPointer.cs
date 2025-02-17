using System.Diagnostics;

using Microsoft.Win32.SafeHandles;

using SQLitePCL;

using Windows.Win32;

namespace SL.ChatLinks;

public class ModuleGetFunctionPointer(ProcessModule module) : IGetFunctionPointer
{
    private readonly ProcessModule _module = module ?? throw new ArgumentNullException(nameof(module));

    public ModuleGetFunctionPointer(string moduleName)
        : this(GetModule(moduleName))
    {
    }

    public static ProcessModule GetModule(string moduleName)
    {
        List<ProcessModule> modules = [.. Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(e => Path.GetFileNameWithoutExtension(e.ModuleName) == moduleName)];
        return modules switch
        {
            [var module] => module,
            [] => throw new ArgumentException($"Found no modules named '{moduleName}' in the current process.", nameof(moduleName)),
            _ => throw new ArgumentException($"Found several modules named '{moduleName}' in the current process."),
        };
    }

    public IntPtr GetFunctionPointer(string name)
    {
        using SafeProcessHandle handle = new(_module.BaseAddress, ownsHandle: false);
        return PInvoke.GetProcAddress(handle, name);
    }
}
