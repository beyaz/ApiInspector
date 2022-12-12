namespace ApiInspector.WebUI.Components;

class ExecuteButton : ReactComponent
{
    public Action Click { get; set; }

    public bool IsProcessing { get; set; }

    protected override Element render()
    {
        return new ActionButton { Label = "Execute", SvgFileName = "play", OnClick = Click, IsProcessing = IsProcessing };
    }
}