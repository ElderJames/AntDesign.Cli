using System.CommandLine;
using AntDesign.Docs.MCP.Services;

namespace AntDesign.Docs.MCP.Commands;

public class DemoListCommand : Command
{
    public DemoListCommand() : base("demo-list", "List all available demos with their component, scenario, and description")
    {
        this.SetHandler(async () =>
        {
            try
            {
                var tools = new AntDesignTools();
                var result = await tools.ListAllDemos();
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });
    }
} 