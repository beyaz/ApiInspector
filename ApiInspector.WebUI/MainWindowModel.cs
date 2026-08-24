using System.Collections.Immutable;

namespace ApiInspector.WebUI;

sealed record MainWindowModel
{
    public string AssemblyDirectory { get; init; }

    public string AssemblyFileName { get; init; }

    public string ClassFilter { get; init; }

    public string MethodFilter { get; set; }

    public ImmutableList<ScenarioModel> ScenarioList { get; set; } = [new()];

    public int ScenarioListSelectedIndex { get; set; }

    public MethodReference SelectedMethod { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    public string ScenarioFilterText { get; set; }
    
    public string EnvironmentText { get; set; }
    
    public string InvokerExeFilePath { get; set; }
}

static class RuntimeNames
{
    public static string NetCore => nameof(NetCore);
    
    public static string NetFramework => nameof(NetFramework);
}

sealed class ScenarioModel
{
    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }

    public string ResponseAsJson { get; set; }
}