using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ApiInspector.Invoking.InvokingParameterAdapters;
using ApiInspector.Models;

namespace ApiInspector.Invoking.Invokers
{
    /// <summary>
    ///     The invocation parameter preparer
    /// </summary>
    class InvocationParameterPreparer
    {
        #region Fields
        /// <summary>
        ///     The boa context
        /// </summary>
        readonly BOAContext boaContext;

        /// <summary>
        ///     The method information
        /// </summary>
        readonly MethodInfo methodInfo;

        /// <summary>
        ///     The parameter adapters
        /// </summary>
        readonly IParameterAdapter[] parameterAdapters =
        {
            new ParameterAdapterForObjectType(),
            new ParameterAdapterForStringType(),
            new ParameterAdapterForObjectHelperType(),
            new ParameterAdapterForSerializableTypes(),
            new ParameterAdapterForConvertibleTypes()
        };

        /// <summary>
        ///     The trace
        /// </summary>
        readonly Action<string> trace;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvocationParameterPreparer" /> class.
        /// </summary>
        public InvocationParameterPreparer(BOAContext boaContext, MethodInfo methodInfo, Action<string> trace)
        {
            this.boaContext = boaContext;
            this.methodInfo = methodInfo;
            this.trace      = trace;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Prepares the specified parameters.
        /// </summary>
        public List<object> Prepare(List<InvocationMethodParameterInfo> parameters)
        {
            var invocationParameters = new List<object>();

            var methodParametersInDotNet = methodInfo.GetParameters();

            var parameterAdapterInputs = new List<ParameterAdapterInput>();

            for (var i = 0; i < methodParametersInDotNet.Length; i++)
            {
                var parameterAdapterInput = new ParameterAdapterInput
                {
                    InvocationValue = parameters[i].Value,
                    ParameterInfo   = methodParametersInDotNet[i],
                    BoaContext      = boaContext
                };
                parameterAdapterInputs.Add(parameterAdapterInput);
            }

            foreach (var parameterAdapterInput in parameterAdapterInputs)
            {
                var stopwatch = Stopwatch.StartNew();

                var isAdapted = false;
                foreach (var parameterAdapter in parameterAdapters)
                {
                    isAdapted = parameterAdapter.TryAdapt(parameterAdapterInput);
                    if (isAdapted)
                    {
                        invocationParameters.Add(parameterAdapterInput.InvocationValue);
                        break;
                    }
                }

                if (isAdapted)
                {
                    stopwatch.Stop();
                    trace($"Parameter: {parameterAdapterInput.ParameterInfo.Name} calculated in {stopwatch.Elapsed.Milliseconds} milliseconds.");
                    continue;
                }

                throw new Exception($"Parameter not adapted. Value: {parameterAdapterInput.InvocationValue}, target parameter type: {parameterAdapterInput.ParameterInfo.ParameterType}");
            }

            return invocationParameters;
        }
        #endregion
    }
}