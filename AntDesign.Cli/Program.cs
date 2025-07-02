using System.CommandLine;
using AntDesign.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Check if running in MCP mode
if (args.Length == 1 && args[0] == "-mcp")
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<AntDesignTools>();

    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    await builder.Build().RunAsync();
    return 0;
}

// Regular CLI mode
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
