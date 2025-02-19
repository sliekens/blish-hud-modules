using System.Diagnostics;

namespace SL.ChatLinks.EF;

internal static class DotnetEF
{
    public static void Run(string arguments)
    {
        ProcessStartInfo processStartInfo = new()
        {
            FileName = "dotnet",
            Arguments = $"ef --no-build --project {GetModuleDirectory()} {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = GetDesignDirectory()
        };

        Console.WriteLine($"dotnet {processStartInfo.Arguments}");

        using Process process = new();
        process.StartInfo = processStartInfo;
        _ = process.Start();

        // Read the output (or the error)
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        // Print the output (or the error)
        Console.WriteLine(output);
        if (!string.IsNullOrEmpty(error))
        {

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Error: ");
            Console.WriteLine(error);
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }

        static string GetModuleDirectory()
        {
            return File.ReadLines("ModuleDirectory.txt").FirstOrDefault();
        }

        static string GetDesignDirectory()
        {
            return File.ReadLines("DesignDirectory.txt").FirstOrDefault();
        }
    }
}
