using System.CommandLine;
using AntDesign.Docs.MCP.Services;

namespace AntDesign.Docs.MCP.Commands;

public class ComponentCategoryCommand : Command
{
    public ComponentCategoryCommand() : base("category", "Get component information by category")
    {
        var categoryOption = new Option<string>(
            aliases: new[] { "--category", "-c" },
            description: "The category to filter components by, e.g. Components, Feedback, Navigation")
        {
            IsRequired = true
        };
        AddOption(categoryOption);
        this.SetHandler(async (string category) =>
        {
            try
            {
                var tools = new AntDesignTools();
                var result = await tools.GetComponentsByCategory(category);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, categoryOption);
    }
} 