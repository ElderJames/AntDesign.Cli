using System.ComponentModel;
using ModelContextProtocol.Server;
using SmartComponents.LocalEmbeddings;

namespace AntDesign.Cli.Services;

[McpServerToolType]
public sealed class AntDesignTools
{
    private readonly ComponentService _componentService;
    private readonly DemoService _demoService;

    public AntDesignTools()
    {
        _componentService = new ComponentService();
        _demoService = new DemoService();
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
    [Description("Search for multiple Ant Design Blazor components by names")]
    public async Task<string> SearchComponents(
        [Description("Comma-separated list of component names to search for, were splited by ','")] string names)
    {
        await _componentService.LoadComponentsAsync();
        var componentNames = names.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n));
        var results = new List<string>();

        foreach (var name in componentNames)
        {
            var component = _componentService.FindComponent(name);
            if (component != null)
            {
                results.Add($"### {name}\n{component}");
            }
            else
            {
                results.Add($"### {name}\nComponent not found.");
            }
        }

        return string.Join("\n\n", results);
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

    [McpServerTool]
    [Description("Search for Ant Design Blazor component demos by component name and scenario, supports fuzzy search and multiple queries. Format: 'Component:Scenario,Component2:Scenario2'")]
    public async Task<string> SearchComponentDemos(
        [Description("Comma-separated list of 'Component:Scenario' pairs, in English")] string queries)
    {

        var queryPairs = queries.Split(',')
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrEmpty(q))
            .Select(q =>
            {
                var parts = q.Split(':');
                return (Component: parts[0].Trim(), Scenario: parts.Length > 1 ? parts[1].Trim() : "");
            });
        var results = new List<string>();
        using var embedder = new LocalEmbedder();
        var allDemos = await _demoService.LoadDemosAsync();
        foreach (var (component, scenario) in queryPairs)
        {
            var candidates = allDemos.Where(d => d.Component.Equals(component, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!candidates.Any())
            {
                results.Add($"### {component} - {scenario}\nDemo not found.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(scenario))
            {
                var demo = candidates.First();
                results.Add($"### {component} - {demo.Scenario}\n```csharp\n{demo.Source}\n```");
                continue;
            }
            var queryVec = embedder.Embed(scenario);
            var best = candidates
                .Select(d => (Demo: d, Score: queryVec.Similarity(embedder.Embed(d.Scenario + " " + d.Description))))
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (best.Demo != null && best.Score > 0)
            {
                results.Add($"### {component} - {best.Demo.Scenario}\n```csharp\n{best.Demo.Source}\n```");
            }
            else
            {
                results.Add($"### {component} - {scenario}\nDemo not found.");
            }
        }
        return string.Join("\n\n", results);
    }
} 