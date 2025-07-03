using System.CommandLine;
using AntDesign.Cli.Services;

namespace AntDesign.Cli.Commands;

public class ComponentListCommand : Command
{
    public ComponentListCommand(ComponentService componentService) : base("list", "List all available components")
    {
        this.SetHandler(async () =>
        {
            try
            {
                await componentService.LoadComponentsAsync();
                var components = componentService.ListComponents();
                Console.WriteLine("Available components:");
                foreach (var component in components)
                {
                    Console.WriteLine($"  {component}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });
    }
} 