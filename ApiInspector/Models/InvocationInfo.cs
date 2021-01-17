using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using ApiInspector.Invoking;
using Newtonsoft.Json;

namespace ApiInspector.Models
{

    static class Fix
    {
        public static InvocationInfo FixAsScenarioModel(InvocationInfo invocationInfo)
        {
            if (invocationInfo.Parameters?.Count > 0)
            {
                invocationInfo.Scenarios = new List<Scenario>
                {
                    new Scenario
                    {
                        MethodParameters = invocationInfo.Parameters,
                        ResponseOutputFilePath = invocationInfo.ResponseOutputFilePath,
                        Assertions = new List<Assertion>()
                    }
                };
            }

            invocationInfo.Parameters             = null;
            invocationInfo.ResponseOutputFilePath = null;

            

            return invocationInfo;
        }
    }

    /// <summary>
    ///     The invocation information
    /// </summary>
    [Serializable]
    public class InvocationInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the assembly.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        ///     Gets or sets the assembly search directory.
        /// </summary>
        public string AssemblySearchDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the name of the class.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///     Gets or sets the environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        ///     Gets or sets the name of the method.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        public List<InvocationMethodParameterInfo> Parameters { get; set; } = new List<InvocationMethodParameterInfo>();

        /// <summary>
        ///     Gets or sets the response output file path.
        /// </summary>
        public string ResponseOutputFilePath { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{ClassName}:{MethodName}";
        }
        
        public List<Scenario> Scenarios { get; set; } = new List<Scenario>();

        #endregion
    }

    [Serializable]
    public class Scenario
    {
        public List<InvocationMethodParameterInfo> MethodParameters { get; set; } = new List<InvocationMethodParameterInfo>();

        public string ResponseOutputFilePath { get; set; }

        public List<Assertion> Assertions { get; set; }

        public Scenario()
        {
            
        }

    }

    [Serializable]
    public class Assertion
    {
        public string Description { get; set; }

        public string OperatorName { get; set; }

        public ValueAccessInfo Actual { get; set; } = new ValueAccessInfo();

        public ValueAccessInfo Expected { get; set; } = new ValueAccessInfo();
    }

    class AssertionOperatorNames
    {
        public static IReadOnlyList<string> GetDescriptions()
        {
            return new[]
            {
                "=",
                "!=",
                ">",
                ">=",
                "<",
                "<=",
                "Contains",
                "StartsWith",
                "EndsWith"

            };
        }
    }



    [Serializable]
    public class ValueAccessInfo
    {

        public bool FetchFromDatabase { get; set; }
        public string DatabaseName { get; set; }

        public string Text { get; set; }


    }
}