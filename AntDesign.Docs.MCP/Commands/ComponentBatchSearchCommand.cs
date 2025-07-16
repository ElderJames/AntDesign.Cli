using System.CommandLine;
using AntDesign.Docs.MCP.Services;

namespace AntDesign.Docs.MCP.Commands;

public class ComponentBatchSearchCommand : Command
{
    public ComponentBatchSearchCommand() : base("search", "Search for multiple components by names")
    {
        var namesOption = new Option<string>(
            aliases: new[] { "--names", "-n" },
            description: "Comma-separated list of component names to search for, e.g. Button,Table")
        {
            IsRequired = true
        };
        AddOption(namesOption);
        this.SetHandler(async (string names) =>
        {
            try
            {
                var tools = new AntDesignTools();
                var result = await tools.SearchComponents(names);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, namesOption);
    }
} 