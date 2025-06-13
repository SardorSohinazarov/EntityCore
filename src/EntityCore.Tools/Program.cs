using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace EntityCore.Tools;
public class Program
{
    private static bool _enableVerboseLogging = false;
    private static void Main(string[] args)
    {
        try
        {
            // ParseArguments will throw an exception if args are invalid (e.g. args.Length == 0, missing entityName).
            var arguments = ParseArguments(args);

            // For debugging: This line will only be reached if ParseArguments succeeds.
            Console.WriteLine("Arguments:" + JsonSerializer.Serialize(arguments));

            // entityName retrieval and check is implicitly handled by ParseArguments throwing an error if it's not valid.
            // No need for: var entityName = arguments.GetValueOrDefault("_entityName");

            var currentDirectory = Directory.GetCurrentDirectory();
            EnsureBuild(currentDirectory);

            Manager generator = new Manager(currentDirectory, arguments); // Manager will use arguments["_entityName"]
            generator.Generate();
        }
        catch (InvalidOperationException ex)
        {
            HandleException(ex); // Shows the detailed error message from ParseArguments.
            // Show usage instructions if the error is about missing/invalid arguments based on ParseArguments's messages.
            if (ex.Message.StartsWith("Error: Missing <entityName>") ||
                ex.Message.StartsWith("Error: Missing value for option") ||
                ex.Message.StartsWith("Error: Unexpected argument"))
            {
                DrawLogo(); // Provides general usage information.
            }
        }
        catch (Exception ex)
        {
            // The "Unhandled exception 500 😁" can be removed as HandleException is called.
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
        // Construct the message for Telegram first, ensuring it has full details.
        var telegramMessage = $"❗️ Xatolik yuz berdi\n\n{ex.Message} \n\n{ex.StackTrace}";

        // User-friendly error message to console
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();

        // Conditional stack trace for console
        if (_enableVerboseLogging)
        {
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        // Telegram notification logic (uses the full detail message)
        string botToken = "7690233025:AAH_cRCVNgGz39Q1d9I1_PcHSIzl8W2Hg6U"; // Consider moving to config
        string chatId = "-1002670987415"; // Consider moving to config

        if (telegramMessage.Length > 4096)
        {
            for (int i = 0; i < telegramMessage.Length; i += 4096)
            {
                string part = telegramMessage.Substring(i, Math.Min(4096, telegramMessage.Length - i));
                SendToTelegram(botToken, chatId, part);
            }
        }
        else
        {
            SendToTelegram(botToken, chatId, telegramMessage);
        }
    }

    private static void SendToTelegram(string botToken, string chatId, string message)
    {
        using (var client = new HttpClient())
        {
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                chat_id = chatId,
                text = message,
            }), Encoding.UTF8, "application/json");

            try
            {
                var response = client.PostAsync(url, content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("An error occurred while sending a message to the Telegram group about this exception.\nIf you want to report this issue to the contributors, you can do so here: [https://t.me/entitycore].");
                    Console.ResetColor();
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("An error occurred while sending a message to the Telegram group about this exception.\nIf you want to report this issue to the contributors, you can do so here: [https://t.me/entitycore].");
                Console.ResetColor();
            }
        }
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

        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("Build successfully.");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var arguments = new Dictionary<string, string>();
        int startIndex = 0;

        if (args.Length == 0)
        {
            // This specific message will be shown by Main calling DrawLogo and exiting.
            // Throwing here means DrawLogo in Main's initial check might not be reached if ParseArguments is called first.
            // Let's adjust Main to call ParseArguments and then DrawLogo only if an error specific to no args occurs.
            // For now, this throw is correct based on requirements.
            throw new InvalidOperationException("Error: Missing <entityName>. Usage: dotnet crud <entityName> [options]");
        }

        if (!args[0].StartsWith("--"))
        {
            arguments["_entityName"] = args[0];
            startIndex = 1;
        }
        else
        {
            throw new InvalidOperationException("Error: Missing <entityName>. <entityName> must be the first argument. Usage: dotnet crud <entityName> [options]");
        }

        for (int i = startIndex; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i]; // Keep the full "--key" for error reporting if needed
                if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                {
                    throw new InvalidOperationException($"Error: Missing value for option '{key}'.");
                }
                arguments[key[2..]] = args[i + 1];
                i++; // Increment to skip the value part of the option
            }
            else
            {
                // This case means an argument that is not the entityName and does not start with --
                throw new InvalidOperationException($"Error: Unexpected argument '{args[i]}'. Options must start with '--'.");
            }
        }
        return arguments;
    }
}