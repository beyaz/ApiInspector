using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InvokingParameterAdapters;
using ApiInspector.Models;
using static FunctionalPrograming.FPExtensions;

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
            ParameterAdapter.TryAdaptForObjectType,
            ParameterAdapter.TryAdaptForStringType,
            ParameterAdapter.TryAdaptForDateTime,
            ParameterAdapter.TryAdaptForObjectHelperType,
            ParameterAdapter.TryAdaptForCustomTypes,
            ParameterAdapter.TryAdaptForSerializableTypes,
            ParameterAdapter.TryAdaptForIConvertibleTypes
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
            var adaptInput = fun(() =>
            {
                foreach (var tryAdapt in parameterAdapters)
                {
                    var input = tryAdapt(parameterAdapterInput);
                    if (input != null)
                    {
                        return input.AdaptedInvocationValue;
                    }
                }

                throw new Exception($"Parameter not adapted. Value: {parameterAdapterInput.InvocationValue}, target parameter type: {parameterAdapterInput.ParameterInfo.ParameterType}");
            });

            var run = fun((Func<object> action) =>
            {
                var stopwatch = Stopwatch.StartNew();

                var result = action();

                stopwatch.Stop();

                trace($"Parameter: {parameterAdapterInput.ParameterInfo.Name} calculated in {stopwatch.Elapsed.Milliseconds} milliseconds.");

                return result;
            });

            return run(adaptInput);
        }
        #endregion
    }
}