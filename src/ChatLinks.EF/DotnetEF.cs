using System.Diagnostics;

namespace SL.ChatLinks.EF;

public static class DotnetEF
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
            Console.WriteLine("Error: ");
            Console.WriteLine(error);
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
