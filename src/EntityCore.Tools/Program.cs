// Version: System.CommandLine Refactor v1.1
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks; // Required for Task

namespace EntityCore.Tools;

public class Program
{
    private static bool _enableVerboseLogging = false;
    private static readonly string BotToken = "7690233025:AAH_cRCVNgGz39Q1d9I1_PcHSIzl8W2Hg6U";
    private static readonly string ChatId = "-1002670987415";

    static async Task<int> Main(string[] args)
    {
        var entityArgument = new Argument<string>("entityName", "Name of the entity for CRUD operations.");
        var contextOption = new Option<string>("--context", "The DbContext class name.");
        var dtoOption = new Option<bool>("--dto", "Generate Data Transfer Objects.");
        var serviceOption = new Option<bool>("--service", "Generate Service and Interface.");
        var controllerOption = new Option<bool>("--controller", "Generate API Controller.");
        var viewOption = new Option<bool>("--view", "Generate Razor Views (for Blazor).");
        var resultOption = new Option<bool>("--result", "Generate common Result classes.");
        var exceptionMOption = new Option<bool>("--exceptionM", "Generate Exception Handling Middleware.");
        var serviceAttributeOption = new Option<bool>("--serviceAttribute", "Generate common Service Attributes for DI.");
        var verboseOption = new Option<bool>(["--verbose", "-v"], "Enable verbose output, including stack traces for errors.");

        var rootCommand = new RootCommand("EntityCore.Tools - A .NET CLI tool for generating CRUD operations.")
        {
            entityArgument,
            contextOption,
            dtoOption,
            serviceOption,
            controllerOption,
            viewOption,
            resultOption,
            exceptionMOption,
            serviceAttributeOption,
            verboseOption
        };

        rootCommand.Description = @"
         ███████ ███   ██ ████████ ██ ████████ ██    ██  ██████   ██████  ██████  ███████
         ██      ████  ██    ██    ██    ██    ██    ██ ██       ██    ██ ██   ██ ██
         █████   ██ ██ ██    ██    ██    ██     ██████  ██       ██    ██ █████   █████
         ██      ██  ████    ██    ██    ██       ██    ██       ██    ██ ██   ██ ██
         ███████ ██   ███    ██    ██    ██       ██     ██████   ██████  ██   ██ ███████

EntityCore.Tools - A tool to generate CRUD operations for Entity Framework Core.
Usage: dotnet crud <entityName> [options]";


        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            var entityNameValue = context.ParseResult.GetValueForArgument(entityArgument);
            var contextValue = context.ParseResult.GetValueForOption(contextOption);
            var dtoValue = context.ParseResult.GetValueForOption(dtoOption);
            var serviceValue = context.ParseResult.GetValueForOption(serviceOption);
            var controllerValue = context.ParseResult.GetValueForOption(controllerOption);
            var viewValue = context.ParseResult.GetValueForOption(viewOption);
            var resultValue = context.ParseResult.GetValueForOption(resultOption);
            var exceptionMValue = context.ParseResult.GetValueForOption(exceptionMOption);
            var serviceAttributeValue = context.ParseResult.GetValueForOption(serviceAttributeOption);
            var verboseValue = context.ParseResult.GetValueForOption(verboseOption);

            _enableVerboseLogging = verboseValue;

            var arguments = new Dictionary<string, string>();
            arguments["_entityName"] = entityNameValue;

            if (!string.IsNullOrEmpty(contextValue)) arguments["context"] = contextValue;
            if (dtoValue) arguments["dto"] = "true";
            if (serviceValue) arguments["service"] = "true";
            if (controllerValue) arguments["controller"] = "true";
            if (viewValue) arguments["view"] = "true";
            if (resultValue) arguments["result"] = "true";
            if (exceptionMValue) arguments["exceptionM"] = "true";
            if (serviceAttributeValue) arguments["serviceAttribute"] = "true";

            if (_enableVerboseLogging)
            {
                Console.WriteLine("Arguments constructed for Manager:" + JsonSerializer.Serialize(arguments));
            }

            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                EnsureBuild(currentDirectory);
                Manager generator = new Manager(currentDirectory, arguments);
                generator.Generate();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                context.ExitCode = 1;
            }
        });

        return await rootCommand.InvokeAsync(args);
    }

    private static void HandleException(Exception ex)
    {
        var telegramMessage = $"❗️ Xatolik yuz berdi\n\n{ex.Message} \n\n{ex.StackTrace}";

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();

        if (_enableVerboseLogging)
        {
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        if (telegramMessage.Length > 4096)
        {
            for (int i = 0; i < telegramMessage.Length; i += 4096)
            {
                string part = telegramMessage.Substring(i, Math.Min(4096, telegramMessage.Length - i));
                SendToTelegram(BotToken, ChatId, part);
            }
        }
        else
        {
            SendToTelegram(BotToken, ChatId, telegramMessage);
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
            catch (Exception telEx)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("An error occurred while sending a message to the Telegram group about this exception: " + telEx.Message + "\nIf you want to report this issue to the contributors, you can do so here: [https://t.me/entitycore].");
                Console.ResetColor();
            }
        }
    }

    private static void EnsureBuild(string projectPath)
    {
        if (_enableVerboseLogging)
        {
            Console.WriteLine("Building the project...");
        }
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
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var errorMessage = $"Build failed with Exit Code: {process.ExitCode}.\nOutput:\n{output}\nError:\n{error}\nPlease fix the errors and try again.";
            throw new InvalidOperationException(errorMessage);
        }

        if (_enableVerboseLogging)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write("Build successfully.");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
