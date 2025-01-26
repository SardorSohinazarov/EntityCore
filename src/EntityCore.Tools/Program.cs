using System.ComponentModel;
using System.Diagnostics;

namespace EntityCore.Tools;
public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
                throw new InvalidOperationException("Usage: dotnet crud <command> [options]");

            var entityName = args[0];
            Console.WriteLine("entityName:" + entityName);

            var currentDirectory = Directory.GetCurrentDirectory();

            EnsureBuild(currentDirectory);

            var dllName = Path.GetFileName(currentDirectory.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("dllName:" + dllName);

            var dllPath = Path.Combine(currentDirectory, "bin", "Debug", "net9.0", $"{dllName}.dll");
            Console.WriteLine("dllPath:" + dllPath);

            CodeGenerator codeGenerator = new CodeGenerator();
            codeGenerator.GenerateService(dllPath, currentDirectory, entityName);
        }
        catch(InvalidOperationException ex)
        {
            HandleException($"Invalida operation exception 400 😁: \n{ex.Message}");
        }
        catch (Exception ex)
        {
            HandleException($"Unhandled exception 500 😁:{ex.Message}");
        }
    }

    private static void HandleException(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void EnsureBuild(string projectPath)
    {
        Console.WriteLine("Building the project...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidAsynchronousStateException("Build failed. Please fix the errors and try again.");

        Console.WriteLine("Build successful.");
    }
}