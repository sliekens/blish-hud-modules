using SQLitePCL;

namespace SL.ChatLinks;

public static class Sqlite3Setup
{
    public static void Run()
    {
        SQLite3Provider_dynamic_cdecl.Setup("e_sqlite3", new ModuleGetFunctionPointer("sliekens.e_sqlite3"));
        raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
    }
}
