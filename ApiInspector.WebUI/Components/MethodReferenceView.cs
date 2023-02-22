namespace ApiInspector.WebUI.Components;

class MethodReferenceView : ReactPureComponent
{
    public MethodReference MethodReference { get; set; }
    
    protected override Element render()
    {
        return new FlexRowCentered(BorderRadius(5), PaddingLeftRight(15)) { MethodReference?.ToString() };
    }
}