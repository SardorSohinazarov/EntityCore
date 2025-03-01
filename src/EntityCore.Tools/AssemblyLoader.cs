using System.Reflection;

namespace EntityCore.Tools
{
    public class AssemblyLoader
    {
        public Assembly Load(string projectRoot)
        {
            string dllPath = FindDllPath(projectRoot);
            return Assembly.LoadFrom(dllPath);
        }

        private string? FindDllPath(string projectRootPath)
        {
            string[] versions = { "net7.0", "net8.0", "net9.0" };

            var dllName = Path.GetFileName(projectRootPath.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("dllName:" + dllName);

            foreach (var version in versions)
            {
                string path = Path.Combine(projectRootPath, "bin", "Debug", version, $"{dllName}.dll");

                if (File.Exists(path))
                {
                    Console.WriteLine($"dll-path: {path}");
                    return path;
                }
            }

            throw new InvalidOperationException("Dll file not found.");
        }
    }
}