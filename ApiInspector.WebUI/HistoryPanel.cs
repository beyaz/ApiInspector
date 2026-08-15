using Newtonsoft.Json;

namespace ApiInspector.WebUI;

class HistoryPanel : Component
{
    public string FilterText { get; set; }

    [CustomEvent]
    public Func<MethodReference, Task> SelectionChanged { get; set; }
    
    [CustomEvent]
    public Func<Task> Closed { get; set; }

    protected override Element render()
    {
        IEnumerable<(string storageKey, MethodReference SelectedMethod)> searchResult = SearchInStoreage(FilterText, 5).Select(x => (storageKey: x.StorageKey, JsonConvert.DeserializeObject<MainWindowModel>(x.StorageValue).SelectedMethod));

        
        
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

                        new img { Src(GetSvgUrl("Method")), Size(14), MarginTop(5) },

                        renderItem(x.SelectedMethod, FilterText)
                    },
                    new img
                    {
                        Id(x.storageKey),
                        Src(GetSvgUrl("trash")), MarginRight(4), Size(24), Hover(Size(26)), Title("Remove From History"), OnClick(OnDeleteClicked)
                    }
                })
            }
        };
        
        
        static Element renderItem(MethodReference methodReference, string filterText)
        {
            var htmlContent = methodReference.DeclaringType.FullName + "::" + methodReference.FullNameWithoutReturnType;
            
            htmlContent = System.Net.WebUtility.HtmlEncode(htmlContent);

            if (filterText.HasValue())
            {
                var encodedFilter = System.Net.WebUtility.HtmlEncode(filterText);

                var sb = new System.Text.StringBuilder();

                var index = 0;

                while (true)
                {
                    var found = htmlContent.IndexOf(encodedFilter, index, StringComparison.OrdinalIgnoreCase);
                    if (found < 0)
                    {
                        sb.Append(htmlContent, index, htmlContent.Length - index);
                        break;
                    }

                    sb.Append(htmlContent, index, found - index);
                    sb.Append("<b>").Append(htmlContent, found, encodedFilter.Length).Append("</b>");

                    index = found + encodedFilter.Length;
                }

                htmlContent = sb.ToString();
            }
            
            return new div
            {
                DangerouslySetInnerHTML(htmlContent),
                MarginLeft(5),
                FontSize13,
                WordBreakAll
            };
        }
    }

    Task OnClose(MouseEvent obj)
    {
        DispatchEvent(Closed);
        
        return Task.CompletedTask;
    }

    Task OnClickHandler(MouseEvent e)
    {
        var storageKey = e.currentTarget.id;

        var fileContent = ReadFromStorage(storageKey);

        var methodReference = JsonConvert.DeserializeObject<MainWindowModel>(fileContent).SelectedMethod;

        DispatchEvent(SelectionChanged,[methodReference]);
        
        return Task.CompletedTask;
    }

    Task OnDeleteClicked(MouseEvent e)
    {
        var storageKey = e.currentTarget.id;

        DeleteFromStorage(storageKey);
        
        return Task.CompletedTask;
    }

    Task OnFilterTextKeypressCompleted()
    {
        return Task.CompletedTask;
    }
}