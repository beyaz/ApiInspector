using System.Collections.Generic;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.History
{
    static class HistoryManager
    {
        #region Static Fields
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryDataKey = new DataKey<IReadOnlyList<InvocationInfo>>(nameof(HistoryDataKey));
        #endregion

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
                            ValueAsJson = "{ customerNumber: 1000 }"
                        }
                    }
                },

                new InvocationInfo
                {
                    Environment             = "dev",
                    AssemblyName            = "ApiInspector.Test.dll",
                    ClassName               = "ApiInspector.Test.AnyClass",
                    MethodName              = "AnyMethod_2",
                    AssemblySearchDirectory = @"D:\git\ApiInspector\ApiInspector.Test\bin\Debug\",
                    Parameters = new List<InvocationMethodParameterInfo>
                    {
                        new InvocationMethodParameterInfo
                        {
                            ValueAsJson = "\"a\"",
                            Type = typeof(string)
                        },
                        new InvocationMethodParameterInfo
                        {
                            ValueAsJson = "2",
                            Type        = typeof(int)
                        },
                        new InvocationMethodParameterInfo
                        {
                            ValueAsJson = "\"c\"",
                            Type        = typeof(string)
                        },
                        new InvocationMethodParameterInfo
                        {
                            ValueAsJson = "5",
                            Type        = typeof(int)
                        }
                    }
                }
            };
        }
        #endregion
    }
}