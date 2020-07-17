using System;
using System.Reflection;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector
{
    static class Utility
    { 
        public static BindingFlags AllBindings
        {
            get
            {
                return BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            }
        }
        #region Public Methods
        public static bool IsSuccess<T>(Func<T> action, ref T target)
        {
            try
            {
                target = action();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static string SerializeToJson(object value, bool ignoreDefaultValues = true)
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

        public static Exception TryRun(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
        #endregion
    }
}