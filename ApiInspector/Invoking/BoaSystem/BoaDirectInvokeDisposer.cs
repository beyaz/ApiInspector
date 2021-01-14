using System;
using System.Reflection;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The boa direct invoke disposer
    /// </summary>
    class BoaDirectInvokeDisposer
    {
        #region Public Methods
        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            const BindingFlags allBindings = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var type = Type.GetType("BOA.Common.Helpers.DirectExecuteHelper+BOADirectExecuteGate+BoaWcfAppDomainHelper,BOA.Common", true);

            var getField = fun((string fieldName) =>
            {
                var fieldInfo = type.GetField(fieldName, allBindings);
                if (fieldInfo == null)
                {
                    throw new ArgumentNullException(nameof(fieldInfo));
                }

                return fieldInfo;
            });

            var clearField = fun((string fieldName) => { getField(fieldName).SetValue(null, null); });

            var unloadWcfDomain = fun(() =>
            {
                var wcfDomain = (AppDomain) getField("wcfDomain").GetValue(null);

                if (wcfDomain != null)
                {
                    AppDomain.Unload(wcfDomain);
                }
            });

            // unloadWcfDomain();
            clearField("appDomainExecuter");
            clearField("wcfDomain");
        }
        #endregion
    }
}