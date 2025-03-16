using NuGet.Common;
using System.Reflection;
using System.Runtime.Loader;

namespace EntityCore.Tools
{
    /// <summary>
    /// Assembly loader from current project
    /// </summary>
    public class AssemblyLoader
    {
        private readonly string _nugetPath;

        public AssemblyLoader()
        {
            _nugetPath = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome), "packages");
        }

        public void Load(string projectRoot)
        {
            string binPath = Path.Combine(projectRoot, "bin", "Debug");
            if (Directory.Exists(binPath))
            {
                var dllFiles = Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories);
                foreach (var dll in dllFiles)
                {
                    string dllName = Path.GetFileName(dll);

                    if (dllName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                        || dllName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipping DLL: {dllName}");
                        continue;
                    }

                    try
                    {
                        Assembly.LoadFrom(dll);
                        Console.WriteLine($"Loaded DLL from bin: {Path.GetFileName(dll)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load DLL from bin: {Path.GetFileName(dll)} - exception message: {ex.Message}");
                    }
                }
            }

            string dllPath = FindDllPath(projectRoot);
            Assembly mainAssembly = Assembly.LoadFrom(dllPath);
            AssemblyName[] references = mainAssembly.GetReferencedAssemblies();

            if (references.Length != 0)
                Console.WriteLine("Referenced assemblies");

            LoadReferencedAssemblies(references);
        }

        private void LoadReferencedAssemblies(AssemblyName[] references)
        {

            foreach (var reference in references)
            {
                try
                {
                    Assembly.Load(reference);
                }
                catch
                {
                    try
                    {
                        LoadFromNuGet(reference);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to load: {reference.Name}");
                    }
                }
                finally
                {
                    Console.WriteLine($"Assembly: {reference.Name}");
                }
            }
        }

        private void LoadFromNuGet(AssemblyName reference)
        {
            string assemblyPath = Path.Combine(_nugetPath, reference.Name, reference.Version.ToString(), $"{reference.Name}.dll");
            if (File.Exists(assemblyPath))
                AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            else
                Console.WriteLine($"Assembly not found in NuGet packages: {reference.Name}");
        }

        private string FindDllPath(string projectRootPath)
        {
            var dllName = Path.GetFileName(projectRootPath.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("dllName:" + dllName);

            var debugPath = Path.Combine(projectRootPath, "bin", "Debug");
            if (!Directory.Exists(debugPath))
                throw new InvalidOperationException("Debug folder not found.");

            var versions = Directory.GetDirectories(debugPath)
                           .Select(Path.GetFileName)
                           .Where(x => x.StartsWith("net"))
                           .OrderDescending();

            foreach (var version in versions)
            {
                string path = Path.Combine(debugPath, version, $"{dllName}.dll");

                if (File.Exists(path))
                {
                    Console.WriteLine($"Dll path: {path}");
                    return path;
                }
            }

            throw new InvalidOperationException("Dll file not found.");
        }
    }
}