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
        readonly Func<ParameterAdapterInput,ParameterAdapterInput>[] parameterAdapters =
        {
             ParameterAdapterForObjectType.TryAdapt,
             ParameterAdapterForStringType.TryAdapt,
             ParameterAdapterForObjectHelperType.TryAdapt,
             ParameterAdapterForSerializableTypes.TryAdapt,
             ParameterAdapterForConvertibleTypes.TryAdapt
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

            foreach (var parameterAdapterInput in methodInfo.GetParameters().ToList((x,i)=>new ParameterAdapterInput(x,boaContext,parameters[i].Value)))
            {
                var stopwatch = Stopwatch.StartNew();

                var isAdapted = false;
                foreach (var tryAdapt in parameterAdapters)
                {
                    var input = tryAdapt(parameterAdapterInput);
                    if (input != null)
                    {
                        isAdapted = true;

                        invocationParameters.Add(input.InvocationValue);
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