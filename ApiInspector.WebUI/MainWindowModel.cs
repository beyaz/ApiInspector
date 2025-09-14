using System.Collections.Immutable;

namespace ApiInspector.WebUI;

class MainWindowModel
{
    public string AssemblyDirectory { get; set; }

    public string AssemblyFileName { get; set; }

    public string ClassFilter { get; set; }

    public string MethodFilter { get; set; }

    public ImmutableList<ScenarioModel> ScenarioList { get; set; } = ImmutableList<ScenarioModel>.Empty.Add(new ScenarioModel());

    public int ScenarioListSelectedIndex { get; set; }

    public MethodReference SelectedMethod { get; set; }

    public string SelectedMethodTreeNodeKey { get; set; }

    public string ScenarioFilterText { get; set; }
    
    public string RuntimeName { get; set; }
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