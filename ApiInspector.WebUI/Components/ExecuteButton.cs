namespace ApiInspector.WebUI.Components;

sealed class ExecuteButton : PureComponent
{
    public MouseEventHandler Click { get; init; }

    public bool IsProcessing { get; init; }

    public bool ShowStatusAsFail { get; init; }

    public bool ShowStatusAsSuccess { get; init; }

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
            OnClicked      = Click,
            IsProcessing = IsProcessing,
            TooltipText        = "Executes selected method by given parameters above then show results in below."
        };
    }
}