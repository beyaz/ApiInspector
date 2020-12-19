using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.InvokingParameterAdapters;
using ApiInspector.Models;
using ApiInspector.Tracing;

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
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvocationParameterPreparer" /> class.
        /// </summary>
        public InvocationParameterPreparer(BOAContext boaContext, ITracer tracer)
        {
            this.boaContext = boaContext ?? throw new ArgumentNullException(nameof(boaContext));
            this.tracer     = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Prepares the specified parameters.
        /// </summary>
        public List<object> Prepare(IReadOnlyList<InvocationMethodParameterInfo> parameters, MethodInfo methodInfo)
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
                    tracer.Trace($"Parameter: {parameterAdapterInput.ParameterInfo.Name} calculated in {stopwatch.Elapsed.Milliseconds} milliseconds.");
                    continue;
                }

                throw new Exception($"Parameter not adapted. Value: {parameterAdapterInput.InvocationValue}, target parameter type: {parameterAdapterInput.ParameterInfo.ParameterType}");
            }

            return invocationParameters;
        }
        #endregion
    }
}