namespace ApiInspector.WebUI.Components;

public class ActionButton : ReactComponent
{
    public bool IsProcessing { get; set; }

    public string Label { get; set; }

    [ReactCustomEvent]
    public Action OnClick { get; set; }

    public string SvgFileName { get; set; }
    
    public string Title { get; set; }

    protected override Element render()
    {
        return new FlexRowCentered(When(Title.HasValue(), Title(Title)))
        {
            children =
            {
                When(IsProcessing, new LoadingIcon { wh(20), mr(10) }),
                When(!IsProcessing && SvgFileName.HasValue(), new img { Src(GetSvgUrl(SvgFileName)), wh(20), MarginRight(5) }),
                new div(Label)
            },
            onClick = ActionButtonOnClick,
            style =
            {
                Color(BluePrimary),
                Border($"1px solid {BluePrimary}"),
                Background("transparent"),
                BorderRadius(5),
                Padding(10, 20),
                CursorPointer
            }
        };
    }

    void ActionButtonOnClick(MouseEvent _)
    {
        if (!IsProcessing)
        {
            IsProcessing = true;
        }

        DispatchEvent(() => OnClick);
    }
}