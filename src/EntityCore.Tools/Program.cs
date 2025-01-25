using EntityCore.Tools;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet crud <command> [options]");
    return;
}

var entityName = args[0];
Console.WriteLine("entityName:" + entityName);

var currentDirectory = Directory.GetCurrentDirectory();

var dllName = Path.GetFileName(currentDirectory.TrimEnd(Path.DirectorySeparatorChar));
Console.WriteLine("dllName:" + dllName);

var dllPath = Path.Combine(currentDirectory, "bin", "Debug", "net9.0", $"{dllName}.dll");
Console.WriteLine("dllPath:" + dllPath);

CodeGenerator codeGenerator = new CodeGenerator();
codeGenerator.GenerateService(dllPath, currentDirectory, entityName);
