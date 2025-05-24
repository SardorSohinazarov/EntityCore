using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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

            Manager manager = new Manager(currentDirectory, arguments);

            if (arguments.TryGetValue("command", out string command))
            {
                if (command == "view")
                {
                    if (arguments.TryGetValue("entityName", out string viewEntityName))
                    {
                        manager.GenerateBlazorViews(viewEntityName);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: EntityName not provided for --view command.");
                        Console.ResetColor();
                        DrawLogo(); // Show help
                    }
                }
                else
                {
                    // Handle other commands like dto, service, controller which are driven by Generate()
                    manager.Generate();
                }
            }
            else
            {
                // This case should ideally not be reached if ParseArguments ensures a command is present or DrawLogo is called.
                 manager.Generate(); // Default behavior or could show help
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("An operation error occurred:");
            Console.ResetColor();
            HandleException(ex);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An unexpected error occurred:");
            Console.ResetColor();
            HandleException(ex);
        }
    }

    private static void DrawLogo()
    {
        Console.WriteLine(@"
         ███████ ███   ██ ████████ ██ ████████ ██    ██  ██████   ██████  ██████  ███████
         ██      ████  ██    ██    ██    ██    ██    ██ ██       ██    ██ ██   ██ ██
         █████   ██ ██ ██    ██    ██    ██     ██████  ██       ██    ██ █████   █████
         ██      ██  ████    ██    ██    ██       ██    ██       ██    ██ ██   ██ ██
         ███████ ██   ███    ██    ██    ██       ██     ██████   ██████  ██   ██ ███████
        ");

        Console.WriteLine("EntityCore.Tools - A tool to generate CRUD operations for Entity Framework Core.");

        Console.WriteLine("Usage: dotnet crud <entityName> [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --context <context>         The DbContext class name. Default is the first DbContext found in the project.");
        Console.WriteLine("  --controller <true/false>   Generate a controller for the entity. Default is false.");
        Console.WriteLine("  --view <true/false>         Generate views for the entity. Default is false.");
        Console.WriteLine("Use \"dotnet crud\" for more information about a command.");
    }

    private static void HandleException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();

        // string botToken = "REMOVED_FOR_SECURITY";
        // string chatId = "REMOVED_FOR_SECURITY";

        var message = $"❗️ An error occurred\n\n{ex.Message} \n\n{ex.StackTrace}";
        Console.WriteLine("Details: " + message); // Output details to console instead of Telegram

        // if (message.Length > 4096)
        // {
        //     for (int i = 0; i < message.Length; i += 4096)
        //     {
        //         string part = message.Substring(i, Math.Min(4096, message.Length - i));
        //         // SendToTelegram(botToken, chatId, part); // Call removed
        //     }
        // }
        // else
        // {
        //     // SendToTelegram(botToken, chatId, message); // Call removed
        // }
    }

    // private static void SendToTelegram(string botToken, string chatId, string message)
    // {
    //     // Method content removed as it's no longer called and contains sensitive URL structure
    // }

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

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Build successful.");
        Console.ResetColor();
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var arguments = new Dictionary<string, string>();
        if (args.Length == 0)
        {
            // No command, will result in DrawLogo() in Main
            return arguments;
        }

        // First argument is the command (e.g., "dto", "view") or an option if only options are provided
        // For this tool, we expect a command first.
        string command = args[0];
        if (command.StartsWith("--")) // Old style, only options
        {
             // Repurpose the first option as a "command" for legacy calls if needed, or treat as error
             // For now, let's assume the first arg is a command like "dto" or "view"
             // and options follow. This simplifies the current refactoring.
             // The old ParseArguments was already flawed for commands like "dto <EntityName>".
             // We'll assume the old way of calling (e.g. `dotnet crud --dto MyEntity`)
             // is now `dotnet crud dto MyEntity`.
            throw new InvalidOperationException("Commands (like 'dto', 'view') should precede entity names and options. Usage: dotnet crud <command> <EntityName> [options]");
        }
        
        arguments["command"] = command;

        if (args.Length < 2 || args[1].StartsWith("--"))
        {
            // EntityName is expected after command, unless it's a command that doesn't need one (not the case for dto/view)
            // This will be caught by specific command handlers if EntityName is mandatory
        }
        else
        {
            arguments["entityName"] = args[1];
        }

        for (int i = 2; i < args.Length; i++) // Start parsing options from the 3rd argument
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[i + 1] : null;
                arguments[key] = value; 
                if (value != null) i++; // Increment i if a value was consumed
            }
            else
            {
                throw new InvalidOperationException($"Unknown option format: {args[i]}. Options should be like --key value or --key.");
            }
        }
        // For compatibility with existing Generate() method in Manager, if "dto", "service", "controller" command is used,
        // set the relevant key for them.
        if (arguments.TryGetValue("entityName", out string entityNameValue))
        {
            if (command == "dto") arguments["dto"] = entityNameValue;
            if (command == "service") arguments["service"] = entityNameValue;
            if (command == "controller") arguments["controller"] = entityNameValue;
            // "view" command is handled separately in Main
        }

        // Console.WriteLine("Arguments:" + JsonSerializer.Serialize(arguments)); // Debug statement removed
        return arguments;
    }
}