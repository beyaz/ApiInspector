using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;

namespace ApiInspector.WebUI.Components;

sealed class ActionButton : Component<ActionButton.State>
{
    public bool IsProcessing { get; init; }

    public string Label { get; init; }

    [CustomEvent]
    public Func<Task> OnClicked { get; init; }

    public string SvgFileName { get; init; }

    public string TooltipText { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            IsProcessingInitialValue = IsProcessing,
            IsProcessing             = IsProcessing
        };

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        var isProcessing = state.IsProcessing;

        var loadingIcon = isProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) };

        var icon = !isProcessing && SvgFileName.HasValue() ? new img { Src(GetSvgUrl(SvgFileName)), Size(20), MarginRight(5) } : null;

        var buttonStyle = new Style
        {
            Color(BluePrimary),
            Border(1, solid, BluePrimary),
            Background(transparent),
            BorderRadius(5),
            Padding(10, 20),
            CursorPointer
        };

        var onClick = isProcessing ? null : OnClick(ActionButtonOnClick);

        var content = new FlexRowCentered(buttonStyle, onClick)
        {
            loadingIcon,
            icon,
            new div(Label)
        };

        return ArrangeTooltip(content);
    }

    Task ActionButtonOnClick(MouseEvent _)
    {
        state = state with { IsProcessing = true };

        DispatchEvent(OnClicked);

        return Task.CompletedTask;
    }

    Element ArrangeTooltip(Element content)
    {
        if (TooltipText.HasValue())
        {
            return new Tooltip
            {
                arrow = true,
                title = TooltipText,
                children =
                {
                    content
                }
            };
        }

        return content;
    }

    internal record State
    {
        public bool IsProcessing { get; init; }

        public bool IsProcessingInitialValue { get; init; }
    }
}