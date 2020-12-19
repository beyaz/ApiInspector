using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InvokingParameterAdapters;
using ApiInspector.Models;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The invocation parameter preparer
    /// </summary>
    class InvocationParameterPreparer
    {
        #region Static Fields
        /// <summary>
        ///     The parameter adapters
        /// </summary>
        static readonly Func<ParameterAdapterInput, ParameterAdapterInput>[] parameterAdapters =
        {
            ParameterAdapterForObjectType.TryAdapt,
            ParameterAdapterForStringType.TryAdapt,
            ParameterAdapterForObjectHelperType.TryAdapt,
            ParameterAdapterForSerializableTypes.TryAdapt,
            ParameterAdapterForConvertibleTypes.TryAdapt
        };
        #endregion

        #region Public Methods
        public static IReadOnlyList<object> Prepare(IReadOnlyList<InvocationMethodParameterInfo> parameters, MethodInfo methodInfo, BOAContext boaContext, Action<string> trace)
        {
            return methodInfo.GetParameters().ToList((x, i) => CreateInputValue(new ParameterAdapterInput(x, boaContext, parameters[i].Value), trace));
        }
        #endregion

        #region Methods
        static object CreateInputValue(ParameterAdapterInput parameterAdapterInput, Action<string> trace)
        {
            var stopwatch = Stopwatch.StartNew();

            foreach (var tryAdapt in parameterAdapters)
            {
                var input = tryAdapt(parameterAdapterInput);
                if (input != null)
                {
                    stopwatch.Stop();
                    trace($"Parameter: {parameterAdapterInput.ParameterInfo.Name} calculated in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                    return input.InvocationValue;
                }
            }

            throw new Exception($"Parameter not adapted. Value: {parameterAdapterInput.InvocationValue}, target parameter type: {parameterAdapterInput.ParameterInfo.ParameterType}");
        }
        #endregion
    }
}