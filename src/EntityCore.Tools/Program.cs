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

            Manager generator = new Manager(currentDirectory, arguments);
            generator.Generate();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Invalid operation exception 400 😁");
            HandleException(ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled exception 500 😁");
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

        string botToken = "7690233025:AAH_cRCVNgGz39Q1d9I1_PcHSIzl8W2Hg6U";
        string chatId = "-1002320575814";

        var message = $"❗️ Xatolik yuz berdi\n\n{ex.Message} \n\n{ex.StackTrace}";

        if (message.Length > 4096)
        {
            for (int i = 0; i < message.Length; i += 4096)
            {
                string part = message.Substring(i, Math.Min(4096, message.Length - i));
                SendToTelegram(botToken, chatId, part);
            }
        }
        else
        {
            SendToTelegram(botToken, chatId, message);
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

        if (args.Length < 1)
            throw new InvalidOperationException("Usage: dotnet crud <command> [options]");

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[i + 1] : null;
                arguments[key] = value;
                i++;
            }
            else
            {
                throw new InvalidOperationException($"Unknown argument or missing value: {args[i]}");
            }
        }

        Console.WriteLine("Arguments:" + JsonSerializer.Serialize(arguments));
        return arguments;
    }
}