using System.CommandLine;
using AntDesign.Docs.MCP.Services;

namespace AntDesign.Docs.MCP.Commands;

public class ComponentDemoCommand : Command
{
    public ComponentDemoCommand() : base("demo", "Search for component demos by component name and scenario")
    {
        var queryOption = new Option<string>(
            aliases: new[] { "--query", "-q" },
            description: "Comma-separated list of 'Component:Scenario' pairs, e.g. Button:download,Table:pagination")
        {
            IsRequired = true
        };
        AddOption(queryOption);
        this.SetHandler(async (string query) =>
        {
            try
            {
                var tools = new AntDesignTools();
                var result = await tools.SearchComponentDemos(query);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, queryOption);
    }
} 