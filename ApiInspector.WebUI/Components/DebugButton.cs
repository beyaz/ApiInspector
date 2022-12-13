namespace ApiInspector.WebUI.Components;

class DebugButton : ReactComponent
{
    public Action Click { get; set; }

    public bool IsProcessing { get; set; }

    public bool IsMouseEnter { get; set; }
    
    protected override Element render()
    {
        var button = new ActionButton { Label = "Debug", SvgFileName = "bug", OnClick = Click, IsProcessing = IsProcessing } ;
        
        if (IsMouseEnter)
        {
            return new FlexRow(PositionRelative)
            {
                button,

                new div
                {
                    Right(0),
                    Padding(3),
                    BorderRadius(3),
                    Width(250),
                    Background(BluePrimary),
                    PositionAbsolute, Text("Attach to 'ApiInspector' process by visual studio or any other ide.")
                }
            };
        }

        return new div
        {
            onMouseEnter = e=>IsMouseEnter = true,
            children = { button }
        };
    }
}