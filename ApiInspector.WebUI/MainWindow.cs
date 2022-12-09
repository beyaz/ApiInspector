using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

class MainWindowModel
{
        
}
class MainWindow: ReactComponent<MainWindowModel>
{
    protected override Element render()
    {

        
        var borderColor = "#d5d5d8";
        
        return new FlexRow(Padding(10), PositionAbsolute, Top(0),Bottom(0),Left(0),Right(0), Background("#eff3f8"))
        {
            new FlexColumn(Border($"1px solid {borderColor}"), WidthHeightMaximized, Background("white"))
            {
                   new FlexRow(PaddingLeftRight(30), PaddingTopBottom(5), BorderBottom($"1px solid {borderColor}"))
                   {
                       new FlexRow(Gap(5))
                       {
                           AlignItemsCenter,
                           new h3{"Api Inspector"}, new h5{ " (.net method invoker) " }
                       }
                   } ,
                   
                   new FlexRow(HeightMaximized)
                   {
                       new FlexColumn(Width(300), HeightMaximized)
                       {
                           "LeftMenu",External.GetAssemblyModel(),
                           BorderRight($"1px solid {borderColor}")
                       },
                       
                       new FlexRowCentered
                       {
                           "Aloha",
                           FlexGrow(1)
                       }
                       
                   }
            }
        };
    }
}


static class External
{
    
    
    public static string GetAssemblyModel()
    {
        return Execute<string>(nameof(GetAssemblyModel), "abc").Unwrap();
    }

    public static TResponse Unwrap<TResponse>(this (TResponse response, Exception exception) tuple)
    {
        if (tuple.exception != null)
        {
            throw tuple.exception;
        }

        return tuple.response;
    }
    
    public static (TResponse response, Exception exception) Execute<TResponse>(string methodName, string parameter)
    {
        var exitCode = RunProcess(methodName, parameter);
        if (exitCode == 1) 
        {
            return (JsonConvert.DeserializeObject<TResponse>(ReadResponse()), null);
        }

        const string error = @"c:\ApiInspector.Response.Error.json";
        if (File.Exists(error))
        {
            var errorMessage = File.ReadAllText(error);

            File.Delete(error);

            return (default, new Exception(errorMessage));
        }

        return (default, new Exception($"Unexpected exitCode: {exitCode}"));
    }

    public static string ReadResponse()
    {
        const string filePath = @"c:\ApiInspector.Response.json";

        while (true)
        {
            if (File.Exists(filePath))
            {
                if (!IsFileLocked(filePath))
                {
                    var response = File.ReadAllText(filePath);
                    
                    File.Delete(filePath);

                    return response;
                }
            }
            
            Thread.Sleep(100);
        }

        static bool IsFileLocked(string path)
        {
            FileStream stream = null;
            try
            {
                FileInfo file = new FileInfo(path);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }
    }
    
    

     static int RunProcess(string methodName, string parameter)
    {
        Process process = new Process();
        process.StartInfo.FileName  = @"D:\work\git\ApiInspector\ApiInspector\bin\Debug\ApiInspector.exe";
        process.StartInfo.Arguments = $"{methodName}:{parameter}";
        process.Start();
        process.WaitForExit();
        
        return process.ExitCode;
        
        
    }
}