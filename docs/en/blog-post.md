# Developing a .NET Local Tool as a GitHub Copilot MCP Server

I've been using AntDesign Blazor with AI-assisted programming for efficient full-stack development. However, I occasionally encountered issues with outdated property usage. Yesterday, an inspiration struck: I could create a CLI tool using .NET to query the JSON artifacts generated during our documentation build, and then add an MCP service for AI editors to call. Taking AntDesign.Cli as an example, this tool queries the latest API information for Ant Design Blazor components, effectively solving the problem of outdated model training datasets.

## Background

With the evolution of AI-assisted programming tools, GitHub Copilot introduced the Agent mode, enabling interaction with MCP. .NET has already officially provided the MCP SDK, and local tools offer a convenient distribution channel - as long as the .NET SDK is installed, the tool can be installed and used with a single command. While it may not be as convenient as npx's installation-free feature, this is the most straightforward .NET MCP distribution method I could think of.

## Project Overview

AntDesign.Cli is a command-line tool for querying Ant Design Blazor component information. Its main features include:

1. Searching for detailed information about specific components
2. Listing all available components
3. Filtering components by category

## Implementation Steps

### 1. Preparation

First, we need to add the necessary NuGet packages:

```xml
<ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.1.0-preview.12" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>
```

### 2. Creating Data Models

We use System.Text.Json to handle component data:

```csharp
public class ComponentModel
{
    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    // ... other properties
}
```

### 3. Implementing Core Services

Create a service class to handle component data:

```csharp
public class ComponentService
{
    private readonly HttpClient _httpClient;
    private List<ComponentModel>? _components;

    public async Task LoadComponentsAsync()
    {
        var json = await _httpClient.GetStringAsync(ComponentsUrl);
        _components = JsonSerializer.Deserialize<List<ComponentModel>>(json);
    }

    public ComponentModel? FindComponent(string name)
    {
        return _components?.FirstOrDefault(c => 
            c.Title.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // ... other methods
}
```

### 4. Creating MCP Tool Class

This is the key part of converting to an MCP server:

```csharp
[McpServerToolType]
public sealed class AntDesignTools
{
    private readonly ComponentService _componentService;

    [McpServerTool]
    [Description("Search for an Ant Design Blazor component by name")]
    public async Task<string> SearchComponent(
        [Description("The name of the component to search for")] string name)
    {
        await _componentService.LoadComponentsAsync();
        var component = _componentService.FindComponent(name);
        return component?.ToString() ?? $"Component '{name}' not found.";
    }

    [McpServerTool]
    [Description("List all available Ant Design Blazor components")]
    public async Task<string> ListComponents()
    {
        await _componentService.LoadComponentsAsync();
        var components = _componentService.ListComponents();
        return $"Available components:\n{string.Join("\n", components)}";
    }
}
```

### 5. Configuring MCP Server

Add MCP server support in Program.cs:

```csharp
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
```

## Usage

### Installing the Tool

```bash
dotnet tool install AntDesign.Cli
```

### Configuring VS Code

Add to VS Code's `mcp.json`:

```json
{
    "servers": {
        "antblazor": {
            "type": "stdio",
            "command": "antblazor",
            "args": ["-mcp"]
        }
    }
}
```

### Using with GitHub Copilot

Now you can query component information directly through GitHub Copilot in VS Code:

- "What properties does the Button component have?"
- "Show me all available components"
- "What components are in the Navigation category?"

## Technical Highlights

1. **Dual Mode Support**: The tool supports both traditional command-line mode and MCP server mode.

2. **Descriptive API**: Uses Description attributes to provide clear tool and parameter descriptions, helping AI better understand and use the tools.

3. **Error Handling**: Provides friendly error messages and appropriate error handling mechanisms.

4. **Code Reuse**: Core business logic is shared between CLI and MCP modes.

## Advantages

1. **Enhanced Development Experience**: Developers can query component information using natural language.

2. **AI Integration**: Seamless integration with GitHub Copilot, providing intelligent assistance.

3. **Documentation Accessibility**: Makes documentation queries more convenient and intuitive.

4. **Extensibility**: Easy to add new features and support more query types.

## Future Prospects

1. Support for more complex query patterns
2. Addition of component example code generation
3. Implementation of component configuration suggestions
4. Support for multiple language switching

## Conclusion

Converting a .NET CLI tool to an MCP server is a relatively simple process that can bring significant value. Through this conversion, we not only retain traditional command-line functionality but also add AI assistance capabilities, making the tool more powerful and user-friendly.

As AI-assisted development tools continue to evolve, this type of integration will become increasingly important. Through this practical case study, we hope to provide a reference for more developers, helping them transform their tools into AI-supported intelligent assistants.

## References

1. [ModelContextProtocol Documentation](https://github.com/microsoft/mcp)
2. [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
3. [Ant Design Blazor Documentation](https://antblazor.com)
4. [System.CommandLine](https://github.com/dotnet/command-line-api)

## About the Author

[Author Information]

## License

MIT 