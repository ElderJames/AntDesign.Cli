# 开发 .NET 本地工具作为 GitHub Copilot MCP 服务器

我一直在用 AntDesign Blazor 配合AI辅助编程，能够非常高效地进行全栈开发。但时常会出现有些属性使用方式落后的问题。不料昨日突然灵感乍现：我可以用.NET的本地工具写一个CLI查询我们文档在构建时生成的json产物，然后再加一个MCP服务让AI编辑器调用。这个项目命名为 AntDesign.Cli 为例，这是一个用于查询 Ant Design Blazor 组件最新API信息的工具，能够避免模型训练数据集版本落后的问题。

## 背景

随着 AI 辅助编程工具的发展，GitHub Copilot 推出了 Agent 模式，允许它与MCP进行交互。.NET早已官方提供MCP SDK，而本地工具能提供很便捷的分发渠道，只要安装过.NET SDK，则可通过一个命令行安装使用。虽然没有npx的免安装特性方便，但这是我能想到的最便捷的.NET MCP分发方式了。

## 项目概述

AntDesign.Cli 是一个用于查询 Ant Design Blazor 组件信息的命令行工具。它的主要功能包括：

1. 搜索特定组件的详细信息
2. 列出所有可用组件
3. 按类别筛选组件

## 实现步骤

### 1. 准备工作

首先，我们需要添加必要的 NuGet 包并配置项目文件：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>antblazor</ToolCommandName>
    <PackageOutputPath>./nupkg</PackageOutputPath>
    
    <!-- NuGet 包信息 -->
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <Description>Ant Design Blazor CLI and MCP Server Tool</Description>
    <PackageTags>antdesign;blazor;cli;mcp</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.1.0-preview.12" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
  </ItemGroup>
</Project>
```

关键配置说明：
- `PackAsTool`: 设置为 true 表示这是一个 .NET Tool
- `ToolCommandName`: 定义工具的命令名称，用户将使用这个命令调用工具
- `PackageOutputPath`: 指定生成的 NuGet 包的输出目录

发布工具的命令：
```bash
# 打包
dotnet pack

# 本地安装（开发测试用）
dotnet tool install --global --add-source ./bind/Release AntDesign.Cli

# 发布到 NuGet（需要 API Key）
dotnet nuget push ./nupkg/AntDesign.Cli.1.0.0.nupkg --api-key your-api-key --source https://api.nuget.org/v3/index.json
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
dotnet tool install AntDesign.Cli
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