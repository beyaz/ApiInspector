using System.Collections.Generic;
using ApiInspector.Application;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using ApiInspector.Tracing;
using BOA.Card.Contracts.CreditCard.Limit;
using BOA.Common.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The card service invocation tests
    /// </summary>
    [TestClass]
    public class CardServiceInvocationTests
    {
        #region Public Methods
        /// <summary>
        ///     Shoulds the invoke any service method.
        /// </summary>
        [TestMethod]
        public void Should_invoke_any_service_method()
        {
            BoaAssemblyResolver.AttachToCurrentDomain();

            var invocationInfo = new InvocationInfo
            {
                AssemblyName            = "BOA.Card.Services.CreditCard.Limit.dll",
                AssemblySearchDirectory = "d:\\boa\\server\\bin\\",
                ClassName               = "BOA.Card.Services.CreditCard.Limit.CRDLimitService",

                Environment = "dev",
                MethodName  = "GetCardAvailableLimit",
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "{  CardRefNumber:'1000'}"
                    }
                }
            };

            var targetType = Invoker.GetTargetType(invocationInfo);

            var invocationParameters = new List<object>
            {
                new GetCardAvailableLimitRequest
                {
                    CardRefNumber = "1"
                }
            };
            var input = new CardServiceMethodInvokerInput(targetType, invocationInfo.MethodName, invocationParameters);

            GenericResponse<GetCardAvailableLimitResponse> response = null;

            using (var injector = new Injector(new TraceQueue(),EnvironmentInfo.Dev))
            {
                var cardServiceMethodInvoker = injector.Get<CardServiceMethodInvoker>();
                response = (GenericResponse<GetCardAvailableLimitResponse>) cardServiceMethodInvoker.Invoke(input);
            }


            

            response.Success.Should().BeTrue();

            response.Value.Should().NotBeNull();
        }
        #endregion
    }
}