public class MenuService
{
    public List<string> Directories { get; }

    public MenuService()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Components", "Pages");

        if (Directory.Exists(basePath))
        {
            Directories = Directory.GetDirectories(basePath)
                .Where(d => !string.IsNullOrEmpty(d))
                .Select(d => d.Split(Path.DirectorySeparatorChar).LastOrDefault()!)
                .ToList();
        }
        else
        {
            Directories = new List<string>();
        }
    }
}

public static class MenuExtensions
{
    public static IServiceCollection AddDinamicMenu(this IServiceCollection services)
    {
        services.AddSingleton<MenuService>();
        return services;
    }
}