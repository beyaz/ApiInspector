using Newtonsoft.Json;

namespace ApiInspector.WebUI;

class HistoryPanel : Component
{
    public string FilterText { get; set; }

    [ReactCustomEvent]
    public Func<MethodReference, Task> SelectionChanged { get; set; }
    
    [ReactCustomEvent]
    public Func<Task> Closed { get; set; }

    protected override Element render()
    {
        var searchResult = SearchInStoreage(FilterText, 5).Select(x => (storageKey: x.StorageKey, JsonConvert.DeserializeObject<MainWindowModel>(x.StorageValue).SelectedMethod));

        return new FlexColumn(AlignItemsCenter, PaddingLeftRight(20), Gap(15), Height("50vh"))
        {
            new FlexRowCentered(FontSize40, FontWeight700, Color("#ced4da"), Padding(5),  CursorDefault, Hover(Color("#adada6")))
            {
                "\u2190",
                Title("Return Back"),
                OnClick(OnClose)
            },
            new input
            {
                placeholder              = "Search in history",
                type                     ="text",
                valueBind                = () => FilterText,
                valueBindDebounceTimeout = 700,
                valueBindDebounceHandler = OnFilterTextKeypressCompleted,
                style                    = { Width("50%"), FontSize12, Padding(8), Border(Solid(1,"#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057") }
            },
            new FlexColumn(AlignItemsStretch, Gap(10))
            {
                Width("60vw"),
                searchResult.Select(x => new FlexRow(JustifyContentSpaceBetween)
                {
                    AlignItemsCenter,
                    Border("1px solid #d9d9d9"), BorderRadius(3),
                    Hover(Background("rgba(68, 66, 178, 0.1)"), BoxShadow("rgb(68 66 178 / 20%) 0px 0px 0px 0.5px inset")),
                    BoxShadow("inset 0px 0px 4px 0px rgb(69 42 124 / 15%)"),

                    new FlexRow(AlignItemsCenter, CursorPointer, Padding(10))
                    {
                        Id(x.storageKey),
                        OnClick(OnClickHandler),

                        new img { Src(GetSvgUrl("Method")), wh(14), mt(5) },

                        new div
                        {
                            Text(x.SelectedMethod.DeclaringType.FullName + "::" + x.SelectedMethod.FullNameWithoutReturnType),
                            MarginLeft(5),
                            FontSize13,
                            WordBreakAll
                        }
                    },
                    new img
                    {
                        Id(x.storageKey),
                        Src(GetSvgUrl("trash")), mr(4), wh(24), Hover(wh(26)), Title("Remove From History"), OnClick(OnDeleteClicked)
                    }
                })
            }
        };
    }

    Task OnClose(MouseEvent obj)
    {
        DispatchEvent(Closed);
        
        return Task.CompletedTask;
    }

    Task OnClickHandler(MouseEvent e)
    {
        var storageKey = e.FirstNotEmptyId;

        var fileContent = ReadFromStorage(storageKey);

        var methodReference = JsonConvert.DeserializeObject<MainWindowModel>(fileContent).SelectedMethod;

        DispatchEvent(SelectionChanged,[methodReference]);
        
        return Task.CompletedTask;
    }

    Task OnDeleteClicked(MouseEvent e)
    {
        var storageKey = e.FirstNotEmptyId;

        DeleteFromStorage(storageKey);
        
        return Task.CompletedTask;
    }

    Task OnFilterTextKeypressCompleted()
    {
        return Task.CompletedTask;
    }
}