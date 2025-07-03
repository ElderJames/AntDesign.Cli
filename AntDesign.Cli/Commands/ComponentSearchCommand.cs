using System.CommandLine;
using AntDesign.Cli.Services;

namespace AntDesign.Cli.Commands;

public class ComponentSearchCommand : Command
{
    public ComponentSearchCommand(ComponentService componentService) : base("search", "Search for a component by name")
    {
        var nameOption = new Option<string>(
            aliases: new[] { "--name", "-n" },
            description: "The name of the component to search for")
        {
            IsRequired = true
        };
        AddOption(nameOption);
        this.SetHandler(async (string name) =>
        {
            try
            {
                await componentService.LoadComponentsAsync();
                var component = componentService.FindComponent(name);
                if (component != null)
                {
                    Console.WriteLine(component.ToString());
                }
                else
                {
                    Console.WriteLine($"Component '{name}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, nameOption);
    }
} 