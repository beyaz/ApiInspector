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
        
        return new FlexColumn(AlignItemsCenter, PaddingLeftRight(20), Gap(10))
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
                    Hover(PrimaryBackground, Border("1px solid #d9d9d9"), BorderRadius(3)),
                    
                    new img { Src(GetSvgUrl("Method")), wh(14), mt(5) },

                    new div { Text(x.SelectedMethod.DeclaringType.FullName + "::"+ x.SelectedMethod.FullNameWithoutReturnType), MarginLeft(5), FontSize13 }
                })
            }
        };
    }

    
    static IEnumerable<(string file, MethodReference SelectedMethod)> Search(string filter)
    {
        foreach (var directory in Directory.GetDirectories(CacheDirectory.CacheDirectoryPath))
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var fileContent = File.ReadAllText(file);
                if (fileContent?.IndexOf(filter+string.Empty,StringComparison.OrdinalIgnoreCase) >0)
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