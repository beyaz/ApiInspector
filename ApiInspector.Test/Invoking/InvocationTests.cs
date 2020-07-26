using System.Collections.Generic;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.TestData;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The invocation tests
    /// </summary>
    [TestClass]
    public class InvocationTests
    {
        #region Public Methods
        /// <summary>
        ///     Shoulds the invoke non static method.
        /// </summary>
        [TestMethod]
        public void Should_invoke_non_static__method()
        {
            var invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_0)
            };

            Invoke_method(invocationInfo, "0");

            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_1),
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "a"
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = 5
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = "c"
                    },
                }
            };

            Invoke_method(invocationInfo, "a-5-c");

            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_2),
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "a"
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = 5
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = "c"
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = 6
                    }
                }
            };
            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_3),
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "a"
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = 5
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = "{StringProperty:'A', IntegerProperty: 56}"
                    },
                    new InvocationMethodParameterInfo
                    {
                        Value = "c"
                    },
                }
            };
            Invoke_method(invocationInfo, "a-5-A-56-c");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Invokes the method.
        /// </summary>
        static void Invoke_method(InvocationInfo invocationInfo, string expectedResponse)
        {
            invocationInfo.AssemblySearchDirectory = string.Empty;

            var output = new Invoker(message => { }).Invoke(invocationInfo);

            output.ExecutionResponse.Should().Be(expectedResponse);
        }
        #endregion
    }
}