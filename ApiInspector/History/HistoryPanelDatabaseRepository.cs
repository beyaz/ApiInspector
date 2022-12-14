using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApiInspector.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using static ApiInspector._;
using static ApiInspector.Keys;
using static ApiInspector.Models.Fix;
using static ApiInspector.Serialization.Serializer;
using static Newtonsoft.Json.JsonConvert;

namespace ApiInspector.History
{
    /// <summary>
    ///     The history panel database repository
    /// </summary>
    class HistoryPanelDatabaseRepository
    {
        static List<RecordModel> historyInLocalDirectory;

        static  string HistoryDirectoryPath => Path.Combine(_.InitialConfiguration.ConfigurationDirectoryPath, "History");

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public static IReadOnlyList<InvocationInfo> GetHistory(Scope scope)
        {
            var userName             = _.AuthenticationUserName;
            var dbConnection         = DbConnection;
            var searchText           = scope.TryGet(DataKeys.SearchTextKey);
            var searchTextIsNotReady = string.IsNullOrWhiteSpace(searchText) || searchText.Length <= 3;

            List<RecordModel> records = null;

            var useLocalDirectoryForHistory = _.InitialConfiguration.UseLocalDirectoryForHistory;
            if (useLocalDirectoryForHistory)
            {
                if (historyInLocalDirectory == null)
                {
                    if (Directory.Exists(HistoryDirectoryPath))
                    {
                        historyInLocalDirectory = Directory.GetFiles(HistoryDirectoryPath, "*.json").Select(x => JsonConvert.DeserializeObject<RecordModel>(File.ReadAllText(x))).ToList();    
                    }
                    else
                    {
                        historyInLocalDirectory = new List<RecordModel>();
                    }

                    if (historyInLocalDirectory.Count ==0)
                    {
                        historyInLocalDirectory = new List<RecordModel>
                        {
                            CreateFrom(new InvocationInfo
                            {
                                AssemblyName            = "ApiInspector.exe",
                                AssemblySearchDirectory = Path.GetDirectoryName(typeof(HistoryPanelDatabaseRepository).Assembly.Location),
                                ClassName               = "ApiInspector.Test.ClassB",
                                Environment             = "dev",
                                MethodName              = "Topla",
                                Scenarios               = new List<ScenarioInfo>
                                {
                                    new ScenarioInfo
                                    {
                                        Description = "Succesfully sum operations",
                                        
                                        MethodParameters = new List<InvocationMethodParameterInfo>
                                        {
                                            new InvocationMethodParameterInfo
                                            {
                                                Value = "5"
                                            },
                                            new InvocationMethodParameterInfo
                                            {
                                                Value = "6"
                                            }
                                        },
                                        Assertions = new List<AssertionInfo>
                                        {
                                            new AssertionInfo
                                            {
                                                Description = "5 + 6 should be 11",
                                                Actual = new ValueAccessInfo(){ Text = "@output"},
                                                Expected = new ValueAccessInfo{Text = "11"},
                                                OperatorName = AssertionOperatorNames.IsEqual
                                            }
                                        }
                                    }
                                }
                            })
                        };
                    }
                }

                if (searchTextIsNotReady)
                {
                    records = historyInLocalDirectory.Take(10).OrderBy(x => x.LastExecutionTime).ToList();
                }
                else
                {
                    records = historyInLocalDirectory.Where(x=>(x.Value+"").IndexOf(searchText,StringComparison.OrdinalIgnoreCase)>=0).OrderBy(x => x.LastExecutionTime).Take(10).ToList();
                }
            }
            else
            {
                if (searchTextIsNotReady)
                {
                    var sql = $"SELECT TOP 10 * FROM DBT.ApiInspectorWhiteStone WITH (NOLOCK) WHERE UserName = @{nameof(userName)} ORDER BY LastExecutionTime DESC";
                    records = dbConnection.Query<RecordModel>(sql, new
                    {
                        userName
                    }).ToList();

                    if (records.Count == 0)
                    {
                        sql     = "SELECT TOP 10 * FROM DBT.ApiInspectorWhiteStone WITH (NOLOCK) ORDER BY LastExecutionTime DESC";
                        records = dbConnection.Query<RecordModel>(sql).ToList();
                    }
                }
                else
                {
                    searchText = $"%{searchText}%";

                    var sql = $"SELECT TOP 10 * FROM DBT.ApiInspectorWhiteStone WITH (NOLOCK) WHERE [Key] LIKE @{nameof(searchText)} OR [Value] LIKE @{nameof(searchText)} ORDER BY LastExecutionTime DESC";
                    records = dbConnection.Query<RecordModel>(sql, new
                    {
                        searchText
                    }).ToList();
                }
            }

            return records.Select(x => DeserializeObject<InvocationInfo>(x.Value)).ToList().Select(FixAsScenarioModel).ToList();
        }

        /// <summary>
        ///     Removes the specified scope.
        /// </summary>
        public static void Remove(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            
            var useLocalDirectoryForHistory = _.InitialConfiguration.UseLocalDirectoryForHistory;

            var previousKey                 = invocationInfo.ToString();
            if (previousKey.Contains("("))
            {
                previousKey = previousKey.Substring(0,previousKey.IndexOf("(", StringComparison.Ordinal));
            }

            var possibleKeys = new List<string> {CreateFrom(invocationInfo).Key, previousKey};

            foreach (var key in possibleKeys)
            {
                if (useLocalDirectoryForHistory)
                {
                    if (File.Exists(key))
                    {
                        File.Delete(key);
                    }
                }
                else
                {
                    DbConnection.Execute($"DELETE FROM DBT.ApiInspectorWhiteStone WHERE [Key] = @{nameof(key)}", new {key});    
                }
                
            }
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public static void SaveToHistory(Scope scope)
        {
            var invocationInfo = scope.Get(SelectedInvocationInfo);
            var dbConnection   = DbConnection;

            var model = CreateFrom(invocationInfo);

            Remove(scope);

            var useLocalDirectoryForHistory = _.InitialConfiguration.UseLocalDirectoryForHistory;
            if (useLocalDirectoryForHistory)
            {
                Utility.WriteToFile(Path.Combine(HistoryDirectoryPath, model.Key.Replace("<","_").Replace(">","_").Replace(":","_")+".json" ) , SerializeObject(model,new JsonSerializerSettings{ Formatting =  Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore}));
            }
            else
            {
                dbConnection.Insert(model);    
            }
            
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates from.
        /// </summary>
        static RecordModel CreateFrom(InvocationInfo info)
        {
            var key = info.ToString();
            var userName  = _.AuthenticationUserName;

            return new RecordModel
            {
                Key               = key,
                UserName          = userName,
                Value             = SerializeToJson(info),
                LastExecutionTime = DateTime.Now
            };
        }
        #endregion

        /// <summary>
        ///     The record model
        /// </summary>
        [Table("DBT.ApiInspectorWhiteStone")]
        class RecordModel
        {
            #region Public Properties
            /// <summary>
            ///     Gets or sets the key.
            /// </summary>
            [ExplicitKey]
            public string Key { get; set; }

            /// <summary>
            ///     Gets or sets the last execution time.
            /// </summary>
            public DateTime? LastExecutionTime { get; set; }

            /// <summary>
            ///     Gets or sets the name of the user.
            /// </summary>
            [ExplicitKey]
            public string UserName { get; set; }

            /// <summary>
            ///     Gets or sets the value.
            /// </summary>
            public string Value { get; set; }
            #endregion
        }
    }
}