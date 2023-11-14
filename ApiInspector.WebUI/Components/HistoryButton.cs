namespace ApiInspector.WebUI.Components;

class HistoryButton : ReactComponent
{
    public MouseEventHandler Click { get; set; }

    protected override Element render()
    {
        return new FlexRowCentered
        {
            "History",
            Color(BluePrimary),
            Border($"1px solid {BluePrimary}"),
            Background("transparent"),
            BorderRadius(5),
            CursorPointer,
            OnClick(Click),
            Height(25),
            Width(60)
        };
    }
}