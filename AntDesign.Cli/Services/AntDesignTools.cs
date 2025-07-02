using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AntDesign.Cli.Services;

[McpServerToolType]
public sealed class AntDesignTools
{
    private readonly ComponentService _componentService;

    public AntDesignTools()
    {
        _componentService = new ComponentService();
    }

    [McpServerTool]
    [Description("Search for an Ant Design Blazor component by name")]
    public async Task<string> SearchComponent(
        [Description("The name of the component to search for")] string name)
    {
        await _componentService.LoadComponentsAsync();
        var component = _componentService.FindComponent(name);
        
        if (component == null)
        {
            return $"Component '{name}' not found.";
        }

        return component.ToString();
    }

    [McpServerTool]
    [Description("List all available Ant Design Blazor components")]
    public async Task<string> ListComponents()
    {
        await _componentService.LoadComponentsAsync();
        var components = _componentService.ListComponents();
        
        return $"Available components:\n{string.Join("\n", components.Select(c => $"  {c}"))}";
    }

    [McpServerTool]
    [Description("Get component information by category")]
    public async Task<string> GetComponentsByCategory(
        [Description("The category to filter components by (e.g. 'Components', 'Feedback', 'Navigation')")] string category)
    {
        await _componentService.LoadComponentsAsync();
        var components = _componentService.ListComponents()
            .Where(c => _componentService.FindComponent(c)?.Category.Equals(category, StringComparison.OrdinalIgnoreCase) ?? false);

        if (!components.Any())
        {
            return $"No components found in category '{category}'.";
        }

        return $"Components in category '{category}':\n{string.Join("\n", components.Select(c => $"  {c}"))}";
    }
} 