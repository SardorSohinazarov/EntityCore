using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace EntityCore.Tools;
public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
                throw new InvalidOperationException("Usage: dotnet crud <command> [options]");

            var arguments = ParseArguments(args);

            var entityName = arguments["entity"];
            Console.WriteLine("entityName:" + entityName);

            var currentDirectory = Directory.GetCurrentDirectory();

            EnsureBuild(currentDirectory);

            var dllName = Path.GetFileName(currentDirectory.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("dllName:" + dllName);

            var dllPath = Path.Combine(currentDirectory, "bin", "Debug", "net8.0", $"{dllName}.dll");
            Console.WriteLine("dllPath:" + dllPath);

            string dbContextName = arguments.ContainsKey("context") ? arguments["context"] : null;
            Console.WriteLine("dbContextName:" + dbContextName);
            bool withcontroller = arguments.ContainsKey("controller") ? bool.TryParse(arguments["controller"], out withcontroller) : false;
            Console.WriteLine("withcontroller:" + withcontroller);
            bool withView = arguments.ContainsKey("view") ? bool.TryParse(arguments["view"], out withView) : false;
            Console.WriteLine("withView:" + withView);

            CodeGenerator codeGenerator = new CodeGenerator();
            codeGenerator.GenerateService(dllPath, currentDirectory, entityName, dbContextName, withcontroller);
        }
        catch(InvalidOperationException ex)
        {
            Console.WriteLine("Stack Trace:" + ex.StackTrace);
            HandleException($"Invalida operation exception 400 😁: \n{ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Stack Trace:" + ex.StackTrace);
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

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var arguments = new Dictionary<string, string>();

        if (args.Length < 1)
            throw new InvalidOperationException("Usage: dotnet crud <command> [options]");

        arguments["entity"] = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            if(args[i].StartsWith("--") && i + 1 < args.Length)
            {
                arguments[args[i].Substring(2)] = args[i + 1];
                i++;
            }
            else
            {
                throw new InvalidOperationException($"Unknown argument or missing value: {args[i]}");
            }
        }

        Console.WriteLine("Arguments:" +JsonSerializer.Serialize(arguments));

        return arguments;
    }
}