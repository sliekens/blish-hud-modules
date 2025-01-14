using System.Diagnostics;

using Windows.Win32;

using Microsoft.Win32.SafeHandles;

using SQLitePCL;

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
        var modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
            .Where(e => Path.GetFileNameWithoutExtension(e.ModuleName) == moduleName).ToList();
        return modules switch
        {
        [var module] => module,
        [] => throw new ArgumentException($"Found no modules named '{moduleName}' in the current process.", nameof(moduleName)),
            _ => throw new ArgumentException($"Found several modules named '{moduleName}' in the current process."),
        };
    }

    public IntPtr GetFunctionPointer(string name)
    {
        using var handle = new SafeProcessHandle(_module.BaseAddress, ownsHandle: false);
        return PInvoke.GetProcAddress(handle, name);
    }
}