using System.Collections.Generic;
using System.IO;
using BOA.Common.Helpers;
using BOA.Common.Types;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector.CardSystemOldAndNewApiCall
{
    /// <summary>
    ///     The result serializer
    /// </summary>
    class ResultSerializer 
    {
        public static DataKey<string> NewCardSystemResultOutputFilePath = new DataKey<string>(nameof(NewCardSystemResultOutputFilePath));
        public static DataKey<List<Result>> NewSystemExecutionErrors = new DataKey<List<Result>>(nameof(NewSystemExecutionErrors));
        public static DataKey<object> NewCardSystemResult = new DataKey<object>(nameof(NewCardSystemResult));
        public static DataKey<string> OldCardSystemResultOutputFilePath = new DataKey<string>(nameof(OldCardSystemResultOutputFilePath));
        public static DataKey<List<Result>> OldSystemExecutionErrors = new DataKey<List<Result>>(nameof(OldSystemExecutionErrors));
        public static DataKey<object> OldCardSystemResult = new DataKey<object>(nameof(OldCardSystemResult));

        #region Public Methods
        /// <summary>
        ///     Serializes this instance.
        /// </summary>
        public static void Serialize(DataContext context)
        {
            ExportOldCardSystemResult(context);

            ExportNewCardSystemResult(context);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Serializes to json.
        /// </summary>
        static string SerializeToJson(object value, bool ignoreDefaultValues = true)
        {
            if (value == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                Formatting           = Formatting.Indented,
                DateFormatString     = "yyyy.MM.dd hh:mm:ss"
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        ///     Exports the new card system result.
        /// </summary>
        static void ExportNewCardSystemResult(DataContext context)
        {
            var path = context.Get(NewCardSystemResultOutputFilePath);

            var errors = context.TryGet(NewSystemExecutionErrors);
            if (errors?.Count > 0)
            {
                File.WriteAllText(path, StringHelper.ResultToDetailedString(errors));
                return;
            }

            var newCardSystemResult = SerializeToJson(context.Get(NewCardSystemResult), false);

            File.WriteAllText(path, newCardSystemResult);
        }

        /// <summary>
        ///     Exports the old card system result.
        /// </summary>
        static void ExportOldCardSystemResult(DataContext context)
        {
            var path = context.Get(OldCardSystemResultOutputFilePath);

            var errors = context.TryGet(OldSystemExecutionErrors);
            if (errors?.Count > 0)
            {
                File.WriteAllText(path, StringHelper.ResultToDetailedString(errors));
                return;
            }

            var oldCardSystemResult = SerializeToJson(context.Get(OldCardSystemResult), false);

            File.WriteAllText(path, oldCardSystemResult);
        }
        #endregion
    }
}