namespace ApiInspector.WebUI.Components;

class DebugButton : ReactComponent
{
    public Func<Task> Click { get; set; }

    public bool IsProcessing { get; set; }

    public bool ShowStatusAsFail { get; set; }

    public bool ShowStatusAsSuccess { get; set; }

    protected override Element render()
    {
        var svgFileName = "bug";

        if (ShowStatusAsSuccess)
        {
            svgFileName = "success";
        }

        if (ShowStatusAsFail)
        {
            svgFileName = "fail";
        }

        return new ActionButton
        {
            Label        = "Debug",
            SvgFileName  = svgFileName,
            OnClick      = Click,
            IsProcessing = IsProcessing,
            TooltipText        = "Press Debug button and attach to 'ApiInspector.exe' process by visual studio or any other ide."
        };
    }
}