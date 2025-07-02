# 将 .NET CLI 工具转换为 GitHub Copilot MCP 服务器：实战案例

在这篇文章中，我将分享如何将一个普通的 .NET CLI 工具转换为支持 GitHub Copilot 的 MCP (Model Context Protocol) 服务器。我们将以一个实际项目 AntDesign.Cli 为例，这是一个用于查询 Ant Design Blazor 组件信息的工具。

## 背景

随着 AI 辅助编程工具的发展，GitHub Copilot 推出了 Agent 模式，允许它与本地工具进行交互。这为我们提供了一个机会，可以将现有的命令行工具转换为 AI 可以直接调用的服务。

## 项目概述

AntDesign.Cli 是一个用于查询 Ant Design Blazor 组件信息的命令行工具。它的主要功能包括：

1. 搜索特定组件的详细信息
2. 列出所有可用组件
3. 按类别筛选组件

## 实现步骤

### 1. 准备工作

首先，我们需要添加必要的 NuGet 包：

```xml
<ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.1.0-preview.12" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>
```

### 2. 创建数据模型

我们使用 System.Text.Json 来处理组件数据：

```csharp
public class ComponentModel
{
    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    // ... 其他属性
}
```

### 3. 实现核心服务

创建一个服务类来处理组件数据：

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

    // ... 其他方法
}
```

### 4. 创建 MCP 工具类

这是转换为 MCP 服务器的关键部分：

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

### 5. 配置 MCP 服务器

在 Program.cs 中添加 MCP 服务器支持：

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

## 使用方法

### 安装工具

```bash
dotnet tool install --global --add-source ./bin/Debug AntDesign.Cli
```

### 配置 VS Code

在 VS Code 的 `mcp.json` 中添加：

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

### 通过 GitHub Copilot 使用

现在，你可以在 VS Code 中通过 GitHub Copilot 直接查询组件信息：

- "What properties does the Button component have?"
- "Show me all available components"
- "What components are in the Navigation category?"

## 技术要点

1. **双模式支持**：工具同时支持传统的命令行模式和 MCP 服务器模式。

2. **描述性 API**：使用 Description 特性提供清晰的工具和参数描述，帮助 AI 更好地理解和使用工具。

3. **错误处理**：提供友好的错误消息和适当的错误处理机制。

4. **代码复用**：核心业务逻辑在 CLI 和 MCP 模式之间共享。

## 优势

1. **增强开发体验**：开发者可以通过自然语言查询组件信息。

2. **AI 集成**：与 GitHub Copilot 无缝集成，提供智能辅助。

3. **文档可访问性**：使文档查询更加便捷和直观。

4. **可扩展性**：易于添加新功能和支持更多查询类型。

## 未来展望

1. 支持更复杂的查询模式
2. 添加组件示例代码生成
3. 实现组件配置建议
4. 支持多语言切换

## 结论

将 .NET CLI 工具转换为 MCP 服务器是一个相对简单的过程，但能带来显著的价值。通过这种转换，我们不仅保留了传统的命令行功能，还为工具添加了 AI 辅助能力，使其更加强大和易用。

随着 AI 辅助开发工具的不断发展，这种集成将变得越来越重要。通过本文的实践案例，希望能为更多开发者提供参考，帮助他们将自己的工具转换为支持 AI 的智能助手。

## 参考资源

1. [ModelContextProtocol 文档](https://github.com/microsoft/mcp)
2. [GitHub Copilot 文档](https://docs.github.com/en/copilot)
3. [Ant Design Blazor 文档](https://antblazor.com)
4. [System.CommandLine](https://github.com/dotnet/command-line-api)

## 关于作者

[作者信息]

## 许可证

MIT 