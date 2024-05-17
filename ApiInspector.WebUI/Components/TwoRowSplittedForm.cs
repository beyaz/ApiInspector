using ReactWithDotNet.ThirdPartyLibraries._react_split_;

namespace ApiInspector.WebUI.Components;

sealed class TwoRowSplittedForm : PureComponent
{
    public int[] sizes { get; init; } = [50, 50];

    public static Element Create(Element[] children, int[] sizes)
    {
        return new FlexRow(SizeFull)
        {
            new style
            {
                new CssClass("gutter",
                [
                    PaddingLeftRight(8),
                    BackgroundRepeatNoRepeat,
                    BackgroundPosition("50%")
                ]),
                new CssClass("gutter.gutter-horizontal",
                [
                    BackgroundImage("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAeCAYAAADkftS9AAAAIklEQVQoU2M4c+bMfxAGAgYYmwGrIIiDjrELjpo5aiZeMwF+yNnOs5KSvgAAAABJRU5ErkJggg==')"),
                    Cursor("col-resize")
                ])
            },

            new Split
            {
                sizes      = sizes,
                gutterSize = 12,
                style      = { SizeFull, DisplayFlexRow },

                children =
                {
                    children
                }
            }
        };
    }

    protected override Element render()
    {
        return new FlexRow(SizeFull)
        {
            new style
            {
                new CssClass("gutter",
                [
                    PaddingLeftRight(8),
                    BackgroundRepeatNoRepeat,
                    BackgroundPosition("50%")
                ]),
                new CssClass("gutter.gutter-horizontal",
                [
                    BackgroundImage("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAeCAYAAADkftS9AAAAIklEQVQoU2M4c+bMfxAGAgYYmwGrIIiDjrELjpo5aiZeMwF+yNnOs5KSvgAAAABJRU5ErkJggg==')"),
                    Cursor("col-resize")
                ])
            },

            new Split
            {
                sizes      = sizes,
                gutterSize = 12,
                style      = { SizeFull, DisplayFlexRow },

                children =
                {
                    children
                }
            }
        };
    }
}