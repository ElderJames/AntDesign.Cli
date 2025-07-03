using System.CommandLine;
using AntDesign.Cli.Services;
using AntDesign.Cli.Commands;
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

rootCommand.AddCommand(new ComponentListCommand(componentService));
rootCommand.AddCommand(new ComponentDemoCommand());
rootCommand.AddCommand(new ComponentBatchSearchCommand());
rootCommand.AddCommand(new ComponentCategoryCommand());
rootCommand.AddCommand(new DemoListCommand());

return await rootCommand.InvokeAsync(args);
