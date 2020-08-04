using System;
using System.Reflection;

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

            {
                var fieldInfo = type.GetField("wcfDomain", allBindings);
                if (fieldInfo == null)
                {
                    throw new ArgumentNullException(nameof(fieldInfo));
                }

                fieldInfo.SetValue(null, null);
            }

            {
                var fieldInfo = type.GetField("appDomainExecuter", allBindings);
                if (fieldInfo == null)
                {
                    throw new ArgumentNullException(nameof(fieldInfo));
                }

                fieldInfo.SetValue(null, null);
            }
        }
        #endregion
    }
}