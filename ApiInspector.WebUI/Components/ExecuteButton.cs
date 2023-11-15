namespace ApiInspector.WebUI.Components;

class ExecuteButton : ReactComponent
{
    public Func<Task> Click { get; set; }

    public bool IsProcessing { get; set; }

    public bool ShowStatusAsFail { get; set; }

    public bool ShowStatusAsSuccess { get; set; }

    protected override Element render()
    {
        var svgFileName = "play";

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
            Label        = "Execute",
            SvgFileName  = svgFileName,
            OnClick      = Click,
            IsProcessing = IsProcessing,
            TooltipText        = "Executes selected method by given parameters above then show results in below."
        };
    }
}