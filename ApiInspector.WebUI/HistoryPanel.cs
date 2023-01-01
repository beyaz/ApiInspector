using System.IO;
using Newtonsoft.Json;
using ReactWithDotNet.Libraries.PrimeReact;

namespace ApiInspector.WebUI;

class HistoryPanel : ReactComponent
{
    [ReactCustomEvent]
    public Action<MethodReference> SelectionChanged { get; set; }

    public string FilterText { get; set; }

    protected override Element render()
    {
        var searchResult = Search(FilterText).Take(5).ToList();
        
        return new FlexColumn(AlignItemsCenter, PaddingLeftRight(20), Gap(10), Height("50vh"))
        {
            new InputText
            {
                placeholder              = "Search",
                style                    = { width = "50%" },
                valueBind                = () => FilterText,
                valueBindDebounceTimeout = 700,
                valueBindDebounceHandler = OnFilterTextKeypressCompleted
            },
            new FlexColumn(AlignItemsStretch, Gap(10))
            {
                Width("60vw"),
                searchResult.Select(x => new FlexRow(AlignItemsCenter, CursorPointer, Padding(10))
                {
                    Id(x.file),
                    OnClick(OnClickHandler),
                    Border("1px solid #d9d9d9"), BorderRadius(3),
                    Hover(Background("rgba(68, 66, 178, 0.1)"),BoxShadow("rgb(68 66 178 / 20%) 0px 0px 0px 0.5px inset")),
                    
                    new img { Src(GetSvgUrl("Method")), wh(14), mt(5) },

                    new div { Text(x.SelectedMethod.DeclaringType.FullName + "::"+ x.SelectedMethod.FullNameWithoutReturnType), MarginLeft(5), FontSize13 }
                })
            }
        };
    }

    void OnClickHandler(MouseEvent e)
    {
        var filePath    = e.FirstNotEmptyId;
        
        var fileContent = File.ReadAllText(filePath);
        var methodReference = JsonConvert.DeserializeObject<MainWindowModel>(fileContent).SelectedMethod;
        
        DispatchEvent(()=>SelectionChanged,methodReference);
    }

    static IEnumerable<(string file, MethodReference SelectedMethod)> Search(string filter)
    {
        foreach (var directory in Directory.GetDirectories(CacheDirectory.CacheDirectoryPath).OrderByDescending(x => new DirectoryInfo(x).LastWriteTime))
        {
            foreach (var file in Directory.GetFiles(directory).OrderByDescending(x=>new FileInfo(x).LastWriteTime))
            {
                var fileContent = File.ReadAllText(file);
                if (fileContent?.IndexOf(filter+string.Empty,StringComparison.OrdinalIgnoreCase) >=0)
                {
                    var mainWindowModel = JsonConvert.DeserializeObject<MainWindowModel>(fileContent);

                    yield return (file, mainWindowModel.SelectedMethod);
                }
            }
        }
        
       
    }

    void OnFilterTextKeypressCompleted()
    {
        
    }
}