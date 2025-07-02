using System.CommandLine;
using AntDesign.Cli.Services;

var componentService = new ComponentService();

var rootCommand = new RootCommand("Ant Design Blazor Component CLI");

var searchCommand = new Command("search", "Search for a component by name");
var nameOption = new Option<string>(
    aliases: new[] { "--name", "-n" },
    description: "The name of the component to search for")
{
    IsRequired = true
};
searchCommand.AddOption(nameOption);

var listCommand = new Command("list", "List all available components");

searchCommand.SetHandler(async (string name) =>
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

listCommand.SetHandler(async () =>
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

rootCommand.AddCommand(searchCommand);
rootCommand.AddCommand(listCommand);

return await rootCommand.InvokeAsync(args);
