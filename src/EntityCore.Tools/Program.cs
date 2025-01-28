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
            {
                DrawLogo();
                return;
            }

            var arguments = ParseArguments(args);

            var currentDirectory = Directory.GetCurrentDirectory();

            EnsureBuild(currentDirectory);

            Generator generator = new Generator(currentDirectory, arguments);
            generator.Generate();
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

    private static void DrawLogo()
    {
        Console.WriteLine(@"
         ███████ ███    ██ ████████ ██ ████████ ██    ██  ██████   ██████  ██████  ████████
         ██      ████   ██    ██    ██    ██    ██    ██ ██       ██    ██ ██   ██ ██
         █████   ██ ██  ██    ██    ██    ██     ██████  ██       ██    ██ █████   ████████
         ██      ██  ██ ██    ██    ██    ██       ██    ██       ██    ██ ██   ██ ██
         ███████ ██   ████    ██    ██    ██       ██     ██████   ██████  ██   ██ ████████
        ");

        Console.WriteLine("EntityCore.Tools - A tool to generate CRUD operations for Entity Framework Core.");

        Console.WriteLine("Usage: dotnet crud <entity> [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --context <context>  The DbContext class name. Default is the first DbContext found in the project.");
        Console.WriteLine("  --controller         Generate a controller for the entity. Default is false.");
        Console.WriteLine("  --view               Generate views for the entity. Default is false.");
        Console.WriteLine("Use \"dotnet crud [command] --help\" for more information about a command.");
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