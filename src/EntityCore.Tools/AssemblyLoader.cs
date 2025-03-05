using NuGet.Common;
using System.Reflection;
using System.Runtime.Loader;

namespace EntityCore.Tools
{
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
                    try
                    {
                        Assembly.LoadFrom(dll);
                        Console.WriteLine($"Loaded DLL from bin: {Path.GetFileName(dll)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load DLL from bin: {Path.GetFileName(dll)} - {ex.Message}");
                    }
                }
            }

            string dllPath = FindDllPath(projectRoot);
            Assembly mainAssembly = Assembly.LoadFrom(dllPath);
            AssemblyName[] references = mainAssembly.GetReferencedAssemblies();

            if(references.Length != 0)
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