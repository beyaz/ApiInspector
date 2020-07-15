using System.Collections.Generic;
using System.IO;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector.History
{
    static class HistoryManager
    {
        #region Static Fields
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryDataKey = new DataKey<IReadOnlyList<InvocationInfo>>(nameof(HistoryDataKey));
        #endregion

        static string DirectoryPath => Path.GetDirectoryName(typeof(HistoryManager).Assembly.Location) + Path.DirectorySeparatorChar + nameof(ApiInspector)+"History"+Path.DirectorySeparatorChar;

        public static void SaveToHistory(InvocationInfo info)
        {
            var filePath = Path.Combine(DirectoryPath, info.ToString().Replace(":", "____") + ".json");

            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            File.WriteAllText(filePath,SerializeToJson(info));
        }

         static string SerializeToJson(object value)
        {
            if (value == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore ,
                Formatting           = Formatting.Indented,
                DateFormatString     = "yyyy.MM.dd hh:mm:ss",
               TypeNameHandling = TypeNameHandling.Objects
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        #region Public Methods
        public static IReadOnlyList<InvocationInfo> GetHistory(DataContext context)
        {
            return new List<InvocationInfo>
            {
                new InvocationInfo
                {
                    Environment             = "dev",
                    AssemblyName            = "BOA.Business.Card.Accounting.dll",
                    ClassName               = "BOA.Business.Card.Accounting.LegalFollowup",
                    MethodName              = "GetLegalFollowList",
                    AssemblySearchDirectory = @"d:\boa\server\bin\"
                },

                new InvocationInfo
                {
                    Environment             = "dev",
                    AssemblyName            = "BOA.Process.Kernel.Card.dll",
                    ClassName               = "BOA.Process.Kernel.Card.InternetBanking.CardGeneral.GetCardListHandler",
                    MethodName              = "ExecuteInOldCardSystem",
                    AssemblySearchDirectory = @"d:\boa\server\bin\",
                    Parameters = new List<InvocationMethodParameterInfo>
                    {
                        new InvocationMethodParameterInfo
                        {
                            Value = "{ customerNumber: 1000 }"
                        }
                    }
                },

                new InvocationInfo
                {
                    Environment             = "dev",
                    AssemblyName            = "ApiInspector.Test.dll",
                    ClassName               = "ApiInspector.TestData.AnyClass",
                    MethodName              = "AnyMethod_2",
                    AssemblySearchDirectory = @"D:\git\ApiInspector\ApiInspector.Test\bin\Debug\",
                    Parameters = new List<InvocationMethodParameterInfo>
                    {
                        new InvocationMethodParameterInfo
                        {
                            Value = "a",
                        },
                        new InvocationMethodParameterInfo
                        {
                            Value = 2,
                        },
                        new InvocationMethodParameterInfo
                        {
                            Value = "c",
                        },
                        new InvocationMethodParameterInfo
                        {
                            Value = 5,
                        }
                    }
                }
            };
        }
        #endregion
    }
}