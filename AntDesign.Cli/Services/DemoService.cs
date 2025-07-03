using System.Text.Json;
using AntDesign.Cli.Models;
using System.Net.Http;

namespace AntDesign.Cli.Services;

public class DemoService
{
    private List<DemoModel>? _demos;
    private const string RemoteUrl = "https://antblazor.com/_content/AntDesign.Docs/meta/components.en-US.json";
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<List<DemoModel>> LoadDemosAsync()
    {
        if (_demos != null) return [];
        var json = await _httpClient.GetStringAsync(RemoteUrl);
        using var doc = JsonDocument.Parse(json);
        _demos = new List<DemoModel>();
        foreach (var comp in doc.RootElement.EnumerateArray())
        {
            var component = comp.GetProperty("Title").GetString() ?? string.Empty;
            if (!comp.TryGetProperty("DemoList", out var demoList)) continue;
            foreach (var demo in demoList.EnumerateArray())
            {
                var title = demo.GetProperty("Title").GetString() ?? string.Empty;
                var desc = demo.TryGetProperty("Description", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                var code = demo.TryGetProperty("Code", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                _demos.Add(new DemoModel
                {
                    Component = component,
                    Scenario = title,
                    Description = desc,
                    Source = code
                });
            }
        }

        return _demos;
    }
}