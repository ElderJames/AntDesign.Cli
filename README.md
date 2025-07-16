# AntDesign.Docs.MCP

A command-line tool and MCP (Model Context Protocol) server for querying Ant Design Blazor component information. It helps developers quickly access component documentation and API details directly from the command line or through GitHub Copilot.

For detailed implementation guide and technical discussion, check out our blog posts:
- [Developing a .NET Local Tool as a GitHub Copilot MCP Server](docs/en/blog-post.md)

## Features

- Search for specific component information
- List all available components
- Filter components by category
- Supports both CLI and MCP Server modes
- GitHub Copilot integration

## Supported Tools

### CLI Commands

- `list`: List all components
- `search -n <ComponentName1,ComponentName2,...>`: Batch search components by name
- `category -n <CategoryName>`: Filter components by category
- `demo-list`: List all component demos
- `demo -n <ComponentName>`: View demos for a specific component

### MCP Server Tools

- `SearchComponents`: Batch search components by name
- `ListComponents`: List all components
- `GetComponentsByCategory`: Get components by category
- `ListAllDemos`: List all component demos
- `SearchComponentDemos`: Get demos by component name and scenario


## Installation

```bash
dotnet tool install --global AntDesign.Docs.MCP
```

## Usage

### CLI Mode

```bash
# List all components
antblazor list

# Search for a specific component
antblazor search -n Button

# Get components by category
antblazor category -n Navigation
```

### MCP Server Mode (for GitHub Copilot)

1. Install the tool globally
2. Add the following configuration to your VS Code's `mcp.json`:

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

Or use the installation link:
```
vscode:mcp/install?{"name":"antblazor","command":"antblazor","args":["-mcp"]}
```

## Implementation Details

This project demonstrates how to:
1. Create a .NET Global Tool
2. Implement MCP Server functionality
3. Load and parse component documentation
4. Provide a clean CLI interface
5. Enable GitHub Copilot integration

Key technologies used:
- .NET 9.0
- System.CommandLine
- ModelContextProtocol
- System.Text.Json

## Technical Details

### Converting a .NET CLI Tool to an MCP Server

The process of converting a .NET CLI tool to an MCP server involves several steps:

1. **Add Required Packages**
```xml
<ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.1.0-preview.12" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>
```

2. **Create MCP Tool Classes**
```csharp
[McpServerToolType]
public sealed class AntDesignTools
{
    [McpServerTool]
    [Description("Search for an Ant Design Blazor component by name")]
    public async Task<string> SearchComponent(
        [Description("The name of the component to search for")] string name)
    {
        // Implementation
    }
}
```

3. **Configure MCP Server**
```csharp
if (args.Length == 1 && args[0] == "-mcp")
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<AntDesignTools>();
    await builder.Build().RunAsync();
    return 0;
}
```

4. **Handle Both CLI and MCP Modes**
- Maintain existing CLI functionality
- Add MCP server support
- Share core business logic

5. **Benefits**
- Enhanced developer experience
- AI-powered assistance
- Seamless integration with GitHub Copilot
- Improved documentation accessibility

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT