using System.Collections.Immutable;

namespace ApiInspector.WebUI;

sealed record MainWindowModel
{
    public string AssemblyDirectory { get; init; }

    public string AssemblyFileName { get; init; }

    public string ClassFilter { get; init; }

    public string MethodFilter { get; init; }

    public ImmutableList<ScenarioModel> ScenarioList { get; init; } = [new()];

    public int ScenarioListSelectedIndex { get; init; }

    public MethodReference SelectedMethod { get; init; }

    public string SelectedMethodTreeNodeKey { get; init; }

    public string ScenarioFilterText { get; init; }
    
    public string InvokerExeFilePath { get; init; }
}

sealed class ScenarioModel
{
    public string JsonTextForDotNetInstanceProperties { get; set; }

    public string JsonTextForDotNetMethodParameters { get; set; }

    public string ResponseAsJson { get; set; }
}